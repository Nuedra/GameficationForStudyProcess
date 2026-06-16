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

## Подключение

```vue
<AchievementGraphPanel
  :course-id="courseId"
  :year="year"
  @logout="goToLoginPage" />
```

До нажатия "Отрисовать граф" XML не загружается и окно остаётся пустым.
