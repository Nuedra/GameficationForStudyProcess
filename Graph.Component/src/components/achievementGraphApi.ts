import axios from 'axios';

const apiClient = axios.create({
    baseURL: '',
    timeout: 10000,
    withCredentials: true
});

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
    const response = await apiClient.post<string>(
        `${graphUrl(courseId, year)}/refresh`,
        undefined,
        {
            responseType: 'text',
            headers: {
                Accept: 'application/xml'
            }
        });

    return response.data;
}

export async function logoutStudent(): Promise<void> {
    await apiClient.post('/api/auth/logout');
}
