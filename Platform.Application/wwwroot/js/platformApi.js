(function () {
    async function readApiError(response, fallback) {
        const error = await response.json().catch(() => null);
        return error?.message || fallback;
    }

    function networkError() {
        return 'Не удалось связаться с сервером. Проверьте подключение и повторите запрос.';
    }

    async function getCurrentStudent() {
        try {
            const response = await fetch('/api/auth/me', { credentials: 'include' });
            if (response.ok)
                return { success: true, student: await response.json() };

            return {
                success: false,
                status: response.status,
                message: await readApiError(response, 'Не удалось определить состояние сессии.')
            };
        } catch {
            return { success: false, status: 0, message: networkError() };
        }
    }

    async function login(studentId) {
        try {
            const response = await fetch('/api/auth/student/login', {
                method: 'POST',
                credentials: 'include',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ id: studentId })
            });
            if (!response.ok) {
                return {
                    success: false,
                    status: response.status,
                    message: await readApiError(response, 'Не удалось выполнить вход.')
                };
            }
            const student = await response.json();
            return { success: true, student };
        } catch {
            return { success: false, status: 0, message: networkError() };
        }
    }

    async function logout() {
        try {
            await fetch('/api/auth/logout', {
                method: 'POST',
                credentials: 'include'
            });
        } catch {}
    }

    async function getCourses() {
        try {
            const response = await fetch('/api/student/courses', { credentials: 'include' });
            if (response.ok)
                return { success: true, courses: await response.json() };

            return {
                success: false,
                status: response.status,
                message: await readApiError(response, 'Не удалось загрузить список курсов.')
            };
        } catch {
            return { success: false, status: 0, message: networkError() };
        }
    }

    window.platformApi = { getCurrentStudent, login, logout, getCourses };
})();
