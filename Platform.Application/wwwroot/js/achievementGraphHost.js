(function () {
    const mountedApps = new Map();

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
                ? await window.platformApi.protectedFetch(url, requestOptions)
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

            await window.Vue.nextTick();
            await window.Vue.nextTick();
            fitGraphViewport(container, effectiveWidth, effectiveHeight);

            return "";
        } catch (error) {
            unmount(elementId);
            return "Не удалось загрузить граф достижений. Повторите запрос.";
        }
    }

    function fitGraphViewport(container, canvasW, canvasH) {
        try {
            const graph =
                container.__vue_app__
                          ?._instance
                          ?.subTree
                          ?.component
                          ?.data
                          ?.graph;

            if (!graph || !Array.isArray(graph._nodes) || graph._nodes.length === 0)
                return;

            let minX = Infinity, minY = Infinity,
                maxX = -Infinity, maxY = -Infinity;

            for (const node of graph._nodes) {
                const x = node._x ?? 0;
                const y = node._y ?? 0;

                if (node._radius !== undefined) {
                    minX = Math.min(minX, x - node._radius);
                    maxX = Math.max(maxX, x + node._radius);
                    minY = Math.min(minY, y - node._radius);
                    maxY = Math.max(maxY, y + node._radius);
                } else {
                    const w = node._width  ?? 0;
                    const h = node._height ?? 0;
                    minX = Math.min(minX, x);
                    maxX = Math.max(maxX, x + w);
                    minY = Math.min(minY, y);
                    maxY = Math.max(maxY, y + h);
                }
            }

            if (minX === Infinity) return;

            const pad      = 60;
            const contentW = maxX - minX + pad * 2;
            const contentH = maxY - minY + pad * 2;

            // Scale to fit the whole graph inside the canvas
            const fitScale = Math.min(canvasW / contentW, canvasH / contentH);

            const centerX = (minX + maxX) / 2;
            const centerY = (minY + maxY) / 2;

            // Set viewport directly — TypeScript 'private' is compile-time only
            graph._scale   = fitScale;
            graph._offsetX = canvasW / 2 - centerX * fitScale;
            graph._offsetY = canvasH / 2 - centerY * fitScale;

        } catch (_) {
            // Viewport fitting failed — the graph will open with default viewport
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
