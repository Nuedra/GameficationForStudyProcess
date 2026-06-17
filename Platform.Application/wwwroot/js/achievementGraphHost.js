(function () {
    const mountedApps = new Map();

    async function render(elementId, courseId, year, width, height) {
        const container = document.getElementById(elementId);
        if (!container)
            return "Контейнер графа не найден.";

        const graphLibrary = window.AchievementGraphComponent;
        if (!window.Vue || !graphLibrary?.GraphComponent)
            return "Компонент графа не загружен.";

        unmount(elementId);
        container.textContent = "Загрузка графа...";

        try {
            const response = await fetch(
                `/api/student/courses/${courseId}/${year}/achievements/graph`,
                {
                    credentials: "include",
                    headers: {
                        Accept: "application/xml"
                    }
                });

            if (!response.ok) {
                container.textContent = "";
                return await formatError(response);
            }

            const xml = await response.text();
            container.textContent = "";

            const app = window.Vue.createApp({
                render() {
                    return window.Vue.h(graphLibrary.GraphComponent, {
                        xmlData: xml,
                        width,
                        height
                    });
                }
            });

            app.mount(container);
            mountedApps.set(elementId, app);
            return "";
        } catch (error) {
            unmount(elementId);
            return error instanceof Error
                ? error.message
                : "Не удалось загрузить граф достижений.";
        }
    }

    function unmount(elementId) {
        const existing = mountedApps.get(elementId);
        if (existing) {
            existing.unmount();
            mountedApps.delete(elementId);
        }

        const container = document.getElementById(elementId);
        if (container)
            container.textContent = "";
    }

    async function formatError(response) {
        if (response.status === 401)
            return "Сначала выполните вход студентом.";
        if (response.status === 403)
            return "У студента нет доступа к этому курсу.";
        if (response.status === 404)
            return "Курс за указанный год не найден.";

        const body = await response.text();
        return body || `API вернул ошибку ${response.status}.`;
    }

    window.achievementGraphHost = {
        render,
        unmount
    };
})();
