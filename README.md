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

`local-setup.sh` ожидает готовности PostgreSQL, применяет временную локальную
SQL-схему и загружает
демонстрационные данные студенческого API. `local-run.sh` запускает приложение
с теми же параметрами подключения. После старта приложение доступно по адресу
`http://localhost:5284`, а Swagger — по адресу `http://localhost:5284/swagger`.

Скрипт запуска использует HTTP-профиль разработки и намеренно не перенаправляет
на HTTPS: это исключает предупреждение о не определённом HTTPS-порте. Для
проверки HTTPS локально выполните `dotnet dev-certs https --trust`, затем:

```bash
source scripts/local-env.sh
dotnet run --project "Platform.Application/Platform.Application.csproj" --launch-profile https
```

HTTPS-профиль доступен на `https://localhost:7075` и перенаправляет запросы с
`http://localhost:5284`. В production редирект остаётся включённым и использует
стандартный HTTPS-порт `443`.

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

### Автоматическая smoke-проверка

Одна команда подготавливает Docker PostgreSQL, применяет временную локальную
SQL-схему и seed,
запускает приложение, проверяет аутентификацию, курсы, граф достижений,
обновление графа, logout и основные отрицательные ответы API. В конце она
запускает Vitest-проверки Vue-компонента, которые подтверждают передачу XML в
компонент графа.

```bash
./scripts/smoke-test.sh
```

Сценарий использует только демонстрационные данные. Он запускает временный
экземпляр приложения и после завершения автоматически его останавливает.

### Состояние схемы и локальный bootstrap

EF Core migrations временно удалены: схема LMS ещё не существует, а новая
схема достижений пока не зафиксирована. Для disposable Docker-базы используется
идемпотентный SQL-bootstrap:

```bash
source scripts/local-env.sh
docker compose exec -T postgres \
  psql -v ON_ERROR_STOP=1 -U "$POSTGRES_USER" -d "$POSTGRES_DB" \
  < "scripts/sql/00_create_local_schema.sql"
```

Скрипт не является migration и не описывает будущую физическую схему LMS.

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

4. В Swagger выполните `GET /api/auth/csrf`, скопируйте поле `token`, затем
   выполните `POST /api/auth/login`, вставив значение в поле заголовка
   `X-CSRF-TOKEN`. Тело запроса:

```json
{
  "id": "b0000000-0000-0000-0000-000000000001"
}
```

После успешного ответа браузер получит непрозрачную session cookie `Platform.Auth`.
Роль определяется сервером и возвращается в ответе. Development-конфигурация
содержит следующие тестовые идентификаторы:

- студент: `b0000000-0000-0000-0000-000000000001`;
- преподаватель: `b1000000-0000-0000-0000-000000000001`;
- администратор: `b2000000-0000-0000-0000-000000000001`.

5. Откройте страницу графа:

```text
http://localhost:5284/achievement-graph-demo
```

или явный маршрут для первого тестового курса:

```text
http://localhost:5284/student/courses/a1000000-0000-0000-0000-000000000001/2026/achievement-graph
```

6. Нажмите кнопку `Отрисовать граф`.

Для тестового студента до обновления достижение «Полпути пройдено!» имеет
статус `available`. После нажатия «Обновить граф» оно получает статус `earned`:
демонстрационная ведомость содержит тег, соответствующий его критерию. Повторное
нажатие не создаёт дубликат достижения.

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

## Автоматическая проверка в GitHub Actions

Workflow `.github/workflows/ci.yml` запускается при каждом `push` и для каждого
pull request. Два независимых задания проверяют серверную и клиентскую части:

- `Backend (.NET)` восстанавливает NuGet-зависимости, собирает `Platform.sln` в
  конфигурации `Release` и запускает все тесты .NET;
- `Graph component (Node.js)` устанавливает зависимости строго по
  `package-lock.json`, собирает `Graph.Component` и запускает Vitest.

Перед merge оба задания должны завершиться успешно. Локально эквивалентные
проверки можно выполнить командами:

```bash
dotnet restore Platform.sln
dotnet build Platform.sln --configuration Release --no-restore
dotnet test Platform.sln --configuration Release --no-build

npm ci --prefix Graph.Component
npm --prefix Graph.Component run build
npm --prefix Graph.Component test
```

Полный сценарий с PostgreSQL остаётся отдельной локальной проверкой:

```bash
./scripts/smoke-test.sh
```

Базовый CI не публикует приложение и не использует секреты. После добавления
workflow в основную ветку рекомендуется включить в настройках GitHub защиту
ветки `main` и сделать проверки `Backend (.NET)` и
`Graph component (Node.js)` обязательными перед merge.
