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

## Запуск проекта

Запуск из корня репозитория:

```bash
dotnet run --project "Platform.Application/Platform.Application.csproj"
```

После старта приложение доступно по адресу:

- `http://localhost:5284`, если порт 5284 свободен

## Локальная PostgreSQL для миграций

Для воспроизводимого локального окружения используется `docker-compose`.

1. Создайте локальный env-файл:

```bash
cp ".env.example" ".env"
```

2. Поднимите PostgreSQL:

```bash
docker compose up -d
```

3. Проверьте, что контейнер запущен:

```bash
docker compose ps
```

4. Подготовьте переменную подключения для `dotnet ef`:

```bash
export PLATFORM_DB_CONNECTION="Host=localhost;Port=5432;Database=platform;Username=postgres;Password=pass"
```

5. Примените миграции:

```bash
dotnet ef database update --project "Platform.DataAccess.Postgress/Platform.DataAccess.Postgress.csproj"
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
docker exec -i nir-platform-postgres psql -U postgres -d platform < "scripts/sql/03_seed_student_api.sql"
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
