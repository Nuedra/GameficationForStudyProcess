import axios from 'axios';

const apiClient = axios.create({
    baseURL: '',
    timeout: 10000,
    withCredentials: true
});

let csrfToken: string | null = null;

async function getCsrfToken(): Promise<string> {
    if (csrfToken)
        return csrfToken;

    const response = await apiClient.get<{ token: string }>('/api/auth/csrf');
    csrfToken = response.data.token;
    return csrfToken;
}

async function csrfHeaders(): Promise<Record<string, string>> {
    return {
        'X-CSRF-TOKEN': await getCsrfToken()
    };
}

function graphUrl(courseId: string, year: number): string {
    return `/api/student/courses/${courseId}/${year}/achievements/graph`;
}

export async function getAchievementGraphXml(
    courseId: string,
    year: number): Promise<string> {
    const response = await apiClient.get<string>(graphUrl(courseId, year), {
        responseType: 'text',
        headers: {
            Accept: 'application/xml'
        }
    });

    return response.data;
}

export async function refreshAchievementGraphXml(
    courseId: string,
    year: number): Promise<string> {
    const headers = await csrfHeaders();
    const response = await apiClient.post<string>(
        `${graphUrl(courseId, year)}/refresh`,
        undefined,
        {
            responseType: 'text',
            headers: {
                ...headers,
                Accept: 'application/xml'
            }
        });

    return response.data;
}

export async function logoutStudent(): Promise<void> {
    await apiClient.post('/api/auth/logout', undefined, {
        headers: await csrfHeaders()
    });
    csrfToken = null;
}
