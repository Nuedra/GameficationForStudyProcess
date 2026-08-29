(function () {
    const mountedApps = new Map();
    let csrfToken = null;

    async function protectedGraphFetch(url, options) {
        if (!csrfToken) {
            const tokenResponse = await fetch('/api/auth/csrf', { credentials: 'include' });
            if (!tokenResponse.ok)
                throw new Error('Не удалось получить CSRF-токен графа.');
            csrfToken = (await tokenResponse.json()).token;
        }

        return await fetch(url, {
            ...options,
            credentials: 'include',
            headers: {
                ...(options?.headers || {}),
                'X-CSRF-TOKEN': csrfToken
            }
        });
    }

    async function render(elementId, courseId, year, width, height) {
        return await loadGraph(elementId, courseId, year, width, height, false);
    }

    async function renderRefresh(elementId, courseId, year, width, height) {
        return await loadGraph(elementId, courseId, year, width, height, true);
    }

    async function loadGraph(elementId, courseId, year, width, height, useRefresh) {
        const container = document.getElementById(elementId);
        if (!container)
            return "Контейнер графа не найден.";

        const graphLibrary = window.AchievementGraphComponent;
        if (!window.Vue || !graphLibrary?.GraphComponent)
            return "Компонент графа не загружен.";

        // Adaptive: use the container's actual pixel width
        const containerWidth = container.offsetWidth;
        const effectiveWidth = containerWidth > 100 ? containerWidth : (width || 900);
        const effectiveHeight = (width && height)
            ? Math.round(height * effectiveWidth / width)
            : Math.round(effectiveWidth * 0.62);

        unmount(elementId);
        container.textContent = "Загрузка графа...";

        try {
            const baseUrl = `/api/student/courses/${courseId}/${year}/achievements/graph`;
            const url = useRefresh ? `${baseUrl}/refresh` : baseUrl;

            const requestOptions = {
                method: useRefresh ? 'POST' : 'GET',
                credentials: "include",
                headers: { Accept: "application/xml" }
            };
            const response = useRefresh
                ? await protectedGraphFetch(url, requestOptions)
                : await fetch(url, requestOptions);

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
                        width: effectiveWidth,
                        height: effectiveHeight
                    });
                }
            });

            app.mount(container);
            mountedApps.set(elementId, app);

            return "";
        } catch (error) {
            unmount(elementId);
            return "Не удалось загрузить граф достижений. Повторите запрос.";
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
        const error = await response.json().catch(() => null);
        return error?.message || "Не удалось загрузить граф достижений. Повторите запрос.";
    }

    window.achievementGraphHost = {
        render,
        renderRefresh,
        unmount
    };
})();
