# Achievement graph component

Компонентная часть для экрана графа достижений.

## Основные файлы

- `src/components/AchievementGraphPanel.vue` — готовая панель личного кабинета с действиями:
  - отрисовать граф;
  - обновить граф;
  - выйти из аккаунта.
- `src/components/GraphComponent.vue` — canvas-отрисовщик XML-графа.
- `src/components/Graph.ts` — фигуры, рёбра, выбор, hover, pan и zoom.
- `src/components/achievementGraphApi.ts` — клиент backend endpoints.
- `src/index.ts` — экспорт компонентов и API-клиента.

## Backend endpoints

- `GET /api/student/courses/{courseId}/{year}/achievements/graph`
- `POST /api/student/courses/{courseId}/{year}/achievements/graph/refresh`
- `POST /api/auth/logout`

## Установка

Команды нужно запускать из папки `Graph.Component`:

```bash
cd Graph.Component
npm install
```

## Подключение

```vue
<AchievementGraphPanel
  :course-id="courseId"
  :year="year"
  @logout="goToLoginPage" />
```

До нажатия "Отрисовать граф" XML не загружается и окно остаётся пустым.

## Локальный просмотр

```bash
npm run dev
```

Preview-страница открывается на `http://127.0.0.1:5173/` и показывает тестовый граф до и после обновления достижений.

## Тесты

```bash
npm test
```

Тесты проверяют панель действий, загрузку и обновление XML, построение узлов и рёбер, цвета статусов, порядок canvas-отрисовки, zoom, pan и очистку графа.

Для запуска тестов в watch-режиме:

```bash
npm run test:watch
```

## Сборка

```bash
npm run build
```

Команда собирает компонент как библиотеку `AchievementGraphComponent`.
