import axios from 'axios';
import * as Graph from './Graph';

const API_BASE = 'http://localhost:5197/api/graph-component';

const apiClient = axios.create({
    baseURL: API_BASE,
    timeout: 10000,
});

// Обработчик ошибок
apiClient.interceptors.response.use(
    (response) => response,
    (error) => {
        console.error('API Error:', error.response?.data || error.message);
        return Promise.reject(error);
    }
);

export const graphApi = {
    // GET - Получить список вершин
    async getNodes(): Promise<Graph.IShape[]> {
        const response = await apiClient.get<Graph.IShape[]>('/get-nodes');
        return response.data;
    },

    // GET - Получить список рёбер
    async getEdges(): Promise<Graph.ILine[]> {
        const response = await apiClient.get<Graph.ILine[]>('/get-edges');
        return response.data;
    },

    // GET - Получить объект по координатам
    async getObjectAt(x: number, y: number): Promise<Graph.IShape | Graph.ILine | null> {
        const response = await apiClient.get<Graph.IShape | Graph.ILine | null>(`/get-object/${x}/${y}`);
        return response.data;
    },

    // PUT - Изменение структуры графа
    async editGraph(graphData: Graph.IShape | Graph.ILine): Promise<any> {
        const response = await apiClient.put('/edit-graph', graphData);
        return response.data;
    },

    // DELETE - Удаление ребра
    async deleteEdge(edgeId: string): Promise<void> {
        await apiClient.delete(`/delete-edge/${edgeId}`);
    },

    // DELETE - Удаление вершины
    async deleteNode(nodeId: string): Promise<void> {
        await apiClient.delete(`/delete-node/${nodeId}`);
    },

    // POST - Добавить вершину
    async addNode(nodeData: any): Promise<void> {
        await apiClient.post('/add-node', nodeData);
    },

    // POST - Добавить ребро
    async addEdge(edgeData: any): Promise<void> {
        await apiClient.post('/add-edge', edgeData);
    },

    // POST - Очистить граф
    async clearGraph(): Promise<void> {
        await apiClient.post('/clear-graph');
    },

    // GET - Получить данные графа в формате XML
    async getGraphXML(): Promise<string> {
        const response = await apiClient.get<string>('/get-graph-xml');
        return response.data;
    },

    // POST - Обновить граф из XML данных
    async updateGraphFromXML(xmlData: string): Promise<void> {
        await apiClient.post('/update-graph-from-xml', { xmlData });
    },

    // GET - Получить полные данные графа (узлы и рёбра)
    async getGraphData(): Promise<{ nodes: any[]; edges: any[] }> {
        const response = await apiClient.get<{ nodes: any[]; edges: any[] }>('/get-graph-data');
        return response.data;
    },

    // POST - Установить диалект
    async setDialect(dialectData: string): Promise<void> {
        await apiClient.post('/set-dialect', { dialectData });
    },

    // POST - Запустить тесты графа
    async runGraphTests(): Promise<void> {
        await apiClient.post('/run-tests');
    }
};