# NIR Platform

## Требования

- .NET SDK 8.0 (проверка: `dotnet --version`)
- macOS/Linux/Windows с установленным `dotnet`

## Структура решения

В решении `Platform.sln` есть несколько проектов, точка запуска веб-приложения:

- `Platform.Application` (Blazor Server, основной исполняемый проект)

## Сборка проекта

Из корня репозитория выполните:

```bash
dotnet restore "Platform.sln"
dotnet build "Platform.sln"
```

## Быстрый локальный запуск

Для работы нужны Docker с Compose и .NET SDK 8.0. Из корня репозитория выполните:

```bash
cp ".env.example" ".env"
./scripts/local-setup.sh
./scripts/local-run.sh
```

`local-setup.sh` ожидает готовности PostgreSQL, применяет миграции и загружает
демонстрационные данные студенческого API. `local-run.sh` запускает приложение
с теми же параметрами подключения. После старта приложение доступно по адресу
`http://localhost:5284`, а Swagger — по адресу `http://localhost:5284/swagger`.

По умолчанию Docker PostgreSQL публикуется на `localhost:5433`. Это исключает
неявное подключение к локальному PostgreSQL, который часто уже занимает `5432`.
При необходимости порт можно изменить только в `.env`: скрипты автоматически
сформируют для приложения и EF Core одинаковую переменную
`ConnectionStrings__Platform`.

Проверить готовность приложения и базы данных можно так:

```bash
curl http://localhost:5284/health/ready
```

Ожидаемый ответ:

```json
{
  "status": "ready"
}
```

### Ручная работа с миграциями

Если требуется запускать EF Core вручную, сначала загрузите настройки текущей
локальной базы. Нельзя использовать прежнюю переменную `PLATFORM_DB_CONNECTION`:
приложение и EF Core используют только `ConnectionStrings__Platform`.

```bash
source scripts/local-env.sh
dotnet ef database update \
  --project "Platform.DataAccess.Postgress/Platform.DataAccess.Postgress.csproj" \
  --startup-project "Platform.DataAccess.Postgress/Platform.DataAccess.Postgress.csproj"
```

## Проверка тестовых запросов к БД

После применения миграций можно прогнать демонстрационные SQL-скрипты.

1. Заполнить базу тестовыми данными:

```bash
docker exec -i nir-platform-postgres psql -U postgres -d platform < "scripts/sql/01_seed_demo.sql"
```

2. Выполнить запросы проверки:

```bash
docker exec -i nir-platform-postgres psql -U postgres -d platform < "scripts/sql/02_queries_demo.sql"
```

Ожидаемый результат:

- выводится 2 записи ачивок студента;
- выводится 2 ачивки курса вместе с критериями;
- `UPDATE 2` для обновления `IsNotificationSeen`;
- `NOTICE` о блокировке дубля по уникальному индексу `(StudentID, AchievementID)`.

Полезные команды:

```bash
docker compose ps
docker compose logs postgres
docker compose down
```

## Демонстрационные данные студенческого API

После применения миграций загрузите отдельный набор данных:

```bash
docker exec -i nir-platform-postgres psql -v ON_ERROR_STOP=1 -U postgres -d platform < "scripts/sql/03_seed_student_api.sql"
```

Он создаёт 20 студентов, две группы, два курса с прочтением за 2026 год и
несколько полученных ачивок.

Для проверки входа можно использовать ID первого студента:

```text
b0000000-0000-0000-0000-000000000001
```

Swagger доступен при запуске в Development:

```text
http://localhost:5284/swagger
```

Если seed падает с ошибкой вида
`column "ContentScopeID" of relation "courses" does not exist`, значит в этой
локальной базе ещё не применена миграция с экземплярами курсов и учебными
группами. Повторите применение миграций командой из раздела выше и затем
запустите seed ещё раз. Проверить наличие колонки можно так:

```bash
docker exec -i nir-platform-postgres psql -U postgres -d platform -c '\d "courses"'
```

В таблице `courses` должна быть колонка `ContentScopeID`.

Если это полностью локальная тестовая база и данные в ней не нужны, можно
пересоздать контейнер и volume, а затем повторить подготовку:

```bash
docker compose down -v
./scripts/local-setup.sh
```

## Проверка графа достижений в приложении

В `Platform.Application` добавлена минимальная страница без отдельного
оформления фронта: она берёт XML из API и передаёт его в canvas-компонент
отрисовки.

1. Поднимите PostgreSQL, примените миграции и загрузите seed:

```bash
./scripts/local-setup.sh
```

2. Запустите приложение:

```bash
./scripts/local-run.sh
```

3. Откройте Swagger в том же браузере, в котором будете смотреть граф:

```text
http://localhost:5284/swagger
```

4. Выполните `POST /api/auth/student/login` с телом:

```json
{
  "id": "b0000000-0000-0000-0000-000000000001"
}
```

После успешного ответа браузер получит cookie `Platform.Student`.

5. Откройте страницу графа:

```text
http://localhost:5284/achievement-graph-demo
```

или явный маршрут для первого тестового курса:

```text
http://localhost:5284/student/courses/a1000000-0000-0000-0000-000000000001/2026/achievement-graph
```

6. Нажмите кнопку `Отрисовать граф`.

Страница вызывает:

```text
GET /api/student/courses/a1000000-0000-0000-0000-000000000001/2026/achievements/graph
```

API возвращает XML из шаблона `Platform.Application/Templates/achievement-graph.xml`
с проставленными статусами нод и рёбер, а страница передаёт этот XML в
`GraphComponent`.

Если страница показывает сообщение `Сначала выполните вход студентом`, значит
cookie была создана не в этом браузере или срок сессии истёк. Повторите вход
через Swagger и обновите страницу графа.
