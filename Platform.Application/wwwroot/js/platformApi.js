(function () {
    let csrfToken = null;

    async function readApiError(response, fallback) {
        const error = await response.json().catch(() => null);
        return error?.message || fallback;
    }

    function networkError() {
        return 'Не удалось связаться с сервером. Проверьте подключение и повторите запрос.';
    }

    async function getCsrfToken(forceRefresh) {
        if (csrfToken && !forceRefresh)
            return csrfToken;

        const response = await fetch('/api/auth/csrf', { credentials: 'include' });
        if (!response.ok)
            throw new Error('Не удалось получить CSRF-токен.');

        const payload = await response.json();
        csrfToken = payload.token;
        return csrfToken;
    }

    async function protectedFetch(url, options) {
        const token = await getCsrfToken(false);
        return await fetch(url, {
            ...options,
            credentials: 'include',
            headers: {
                ...(options?.headers || {}),
                'X-CSRF-TOKEN': token
            }
        });
    }

    async function getCurrentUser() {
        try {
            const response = await fetch('/api/auth/me', { credentials: 'include' });
            if (response.ok)
                return { success: true, user: await response.json() };

            return {
                success: false,
                status: response.status,
                message: await readApiError(response, 'Не удалось определить состояние сессии.')
            };
        } catch {
            return { success: false, status: 0, message: networkError() };
        }
    }

    async function login(userId) {
        try {
            const response = await protectedFetch('/api/auth/login', {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify({ id: userId })
            });
            if (!response.ok) {
                return {
                    success: false,
                    status: response.status,
                    message: await readApiError(response, 'Не удалось выполнить вход.')
                };
            }
            const user = await response.json();
            csrfToken = null;
            return { success: true, user };
        } catch {
            return { success: false, status: 0, message: networkError() };
        }
    }

    async function logout() {
        try {
            await protectedFetch('/api/auth/logout', {
                method: 'POST'
            });
            csrfToken = null;
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

    async function getStaffCourses() {
        try {
            const response = await fetch('/api/staff/courses', { credentials: 'include' });
            if (response.ok)
                return { success: true, courses: await response.json() };

            return {
                success: false,
                status: response.status,
                message: await readApiError(response, 'Не удалось загрузить курсы кабинета.')
            };
        } catch {
            return { success: false, status: 0, message: networkError() };
        }
    }

    async function getStaffCourse(courseId, year) {
        try {
            const response = await fetch(
                `/api/staff/courses/${encodeURIComponent(courseId)}/${encodeURIComponent(year)}`,
                { credentials: 'include' });
            if (response.ok)
                return { success: true, course: await response.json() };

            return {
                success: false,
                status: response.status,
                message: await readApiError(response, 'Не удалось загрузить экземпляр курса.')
            };
        } catch {
            return { success: false, status: 0, message: networkError() };
        }
    }

    async function getTeachingAssignments(courseId, year) {
        try {
            const response = await fetch(
                `/api/admin/courses/${encodeURIComponent(courseId)}/${encodeURIComponent(year)}/teachers`,
                { credentials: 'include' });
            if (response.ok)
                return { success: true, management: await response.json() };

            return {
                success: false,
                status: response.status,
                message: await readApiError(response, 'Не удалось загрузить назначения преподавателей.')
            };
        } catch {
            return { success: false, status: 0, message: networkError() };
        }
    }

    async function saveTeachingAssignment(courseId, year, teacherId, startDate, endDate, isLead) {
        try {
            const response = await protectedFetch(
                `/api/admin/courses/${encodeURIComponent(courseId)}/${encodeURIComponent(year)}/teachers/${encodeURIComponent(teacherId)}`,
                {
                    method: 'PUT',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify({
                        startDate: `${startDate}T00:00:00Z`,
                        endDate: endDate ? `${endDate}T00:00:00Z` : null,
                        isLead
                    })
                });
            if (response.ok)
                return { success: true, management: await response.json() };

            return {
                success: false,
                status: response.status,
                message: await readApiError(response, 'Не удалось сохранить назначение преподавателя.')
            };
        } catch {
            return { success: false, status: 0, message: networkError() };
        }
    }

    async function endTeachingAssignment(courseId, year, teacherId) {
        try {
            const response = await protectedFetch(
                `/api/admin/courses/${encodeURIComponent(courseId)}/${encodeURIComponent(year)}/teachers/${encodeURIComponent(teacherId)}/end`,
                { method: 'POST' });
            if (response.ok)
                return { success: true, management: await response.json() };

            return {
                success: false,
                status: response.status,
                message: await readApiError(response, 'Не удалось завершить назначение преподавателя.')
            };
        } catch {
            return { success: false, status: 0, message: networkError() };
        }
    }

    async function getManagedAchievements(courseId, year) {
        try {
            const response = await fetch(
                `/api/staff/courses/${encodeURIComponent(courseId)}/${encodeURIComponent(year)}/achievements`,
                { credentials: 'include' });
            if (response.ok)
                return { success: true, achievements: await response.json() };

            return {
                success: false,
                status: response.status,
                message: await readApiError(response, 'Не удалось загрузить достижения курса.')
            };
        } catch {
            return { success: false, status: 0, message: networkError() };
        }
    }

    async function saveManagedAchievement(courseId, year, achievementId, achievement) {
        try {
            const isCreate = !achievementId;
            const url = isCreate
                ? `/api/staff/courses/${encodeURIComponent(courseId)}/${encodeURIComponent(year)}/achievements`
                : `/api/staff/courses/${encodeURIComponent(courseId)}/${encodeURIComponent(year)}/achievements/${encodeURIComponent(achievementId)}`;
            const response = await protectedFetch(url, {
                method: isCreate ? 'POST' : 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(achievement)
            });
            if (response.ok)
                return { success: true, achievement: await response.json() };

            return {
                success: false,
                status: response.status,
                message: await readApiError(response, 'Не удалось сохранить достижение.')
            };
        } catch {
            return { success: false, status: 0, message: networkError() };
        }
    }

    async function deleteManagedAchievement(courseId, year, achievementId, revokeAwards) {
        try {
            const query = revokeAwards ? '?revokeAwards=true' : '';
            const response = await protectedFetch(
                `/api/staff/courses/${encodeURIComponent(courseId)}/${encodeURIComponent(year)}/achievements/${encodeURIComponent(achievementId)}${query}`,
                { method: 'DELETE' });
            if (response.ok)
                return { success: true };

            return {
                success: false,
                status: response.status,
                message: await readApiError(response, 'Не удалось удалить достижение.')
            };
        } catch {
            return { success: false, status: 0, message: networkError() };
        }
    }

    async function getManagedAchievementAwards(courseId, year, achievementId) {
        try {
            const response = await fetch(
                `/api/staff/courses/${encodeURIComponent(courseId)}/${encodeURIComponent(year)}/achievements/${encodeURIComponent(achievementId)}/awards`,
                { credentials: 'include' });
            if (response.ok)
                return { success: true, awards: await response.json() };

            return {
                success: false,
                status: response.status,
                message: await readApiError(response, 'Не удалось загрузить список студентов.')
            };
        } catch {
            return { success: false, status: 0, message: networkError() };
        }
    }

    async function revokeManagedAchievementAward(courseId, year, achievementId, studentId) {
        try {
            const response = await protectedFetch(
                `/api/staff/courses/${encodeURIComponent(courseId)}/${encodeURIComponent(year)}/achievements/${encodeURIComponent(achievementId)}/awards/${encodeURIComponent(studentId)}`,
                { method: 'DELETE' });
            if (response.ok)
                return { success: true, achievement: await response.json() };

            return {
                success: false,
                status: response.status,
                message: await readApiError(response, 'Не удалось отозвать ачивку у студента.')
            };
        } catch {
            return { success: false, status: 0, message: networkError() };
        }
    }

    async function saveManagedAchievementCriteria(courseId, year, achievementId, criteria) {
        try {
            const response = await protectedFetch(
                `/api/staff/courses/${encodeURIComponent(courseId)}/${encodeURIComponent(year)}/achievements/${encodeURIComponent(achievementId)}/criteria`,
                {
                    method: 'PUT',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(criteria)
                });
            if (response.ok)
                return { success: true, achievement: await response.json() };

            return {
                success: false,
                status: response.status,
                message: await readApiError(response, 'Не удалось сохранить критерий.')
            };
        } catch {
            return { success: false, status: 0, message: networkError() };
        }
    }

    async function deleteManagedAchievementCriteria(courseId, year, achievementId) {
        try {
            const response = await protectedFetch(
                `/api/staff/courses/${encodeURIComponent(courseId)}/${encodeURIComponent(year)}/achievements/${encodeURIComponent(achievementId)}/criteria`,
                { method: 'DELETE' });
            if (response.ok)
                return { success: true, achievement: await response.json() };

            return {
                success: false,
                status: response.status,
                message: await readApiError(response, 'Не удалось удалить критерий.')
            };
        } catch {
            return { success: false, status: 0, message: networkError() };
        }
    }

    window.platformApi = {
        getCsrfToken,
        protectedFetch,
        getCurrentUser,
        login,
        logout,
        getCourses,
        getStaffCourses,
        getStaffCourse,
        getTeachingAssignments,
        saveTeachingAssignment,
        endTeachingAssignment,
        getManagedAchievements,
        saveManagedAchievement,
        deleteManagedAchievement,
        getManagedAchievementAwards,
        revokeManagedAchievementAward,
        saveManagedAchievementCriteria,
        deleteManagedAchievementCriteria
    };
})();
