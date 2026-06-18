(function () {
    async function getCurrentStudent() {
        try {
            const response = await fetch('/api/auth/me', { credentials: 'include' });
            if (!response.ok) return null;
            return await response.json();
        } catch {
            return null;
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
                const err = await response.json().catch(() => ({}));
                return { success: false, message: err.message || 'Ошибка авторизации' };
            }
            const student = await response.json();
            return { success: true, student };
        } catch (e) {
            return { success: false, message: e.message || 'Сетевая ошибка' };
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
            if (!response.ok) return null;
            return await response.json();
        } catch {
            return null;
        }
    }

    window.platformApi = { getCurrentStudent, login, logout, getCourses };
})();
