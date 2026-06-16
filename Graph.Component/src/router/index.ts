import { createRouter, createWebHistory, RouteRecordRaw } from 'vue-router'

// Ленивая загрузка для оптимизации
const MainView = () => import('@/views/MainView.vue')
const ApiTestView = () => import('@/views/ApiTestView.vue')
const TestsView = () => import('@/views/TestsView.vue')
const GeneratorView = () => import('@/views/GeneratorView.vue')
const FrameGeneratorView = () => import('@/views/FrameGeneratorView.vue')

const routes: Array<RouteRecordRaw> = [
    {
        path: '/',
        name: 'Main',
        component: MainView,
        meta: { title: 'Главная' }
    },
    {
        path: '/api-test',
        name: 'ApiTest',
        component: ApiTestView,
        meta: { title: 'Тестирование API' }
    },
    {
        path: '/tests',
        name: 'Tests',
        component: TestsView,
        meta: { title: 'Тесты' }
    },
    {
        path: '/generator',
        name: 'Generator',
        component: GeneratorView,
        meta: { title: 'Генератор графов' }
    },
    {
        path: '/frame-generator',
        name: 'FrameGenerator',
        component: FrameGeneratorView,
        meta: { title: 'Генератор фреймов' }
    },
    {
        path: '/:pathMatch(.*)*',
        redirect: '/'
    }
]

const router = createRouter({
    history: createWebHistory(),
    routes
})

// Динамический заголовок страницы
router.beforeEach((to) => {
    document.title = to.meta.title ? `${to.meta.title} | Graph Viewer` : 'Graph Viewer'
})

export default router