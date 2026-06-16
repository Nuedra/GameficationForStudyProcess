<template>
    <div class="main-view">
        <div class="container">
            <!-- Hero Section -->
            <section class="hero">
                <div class="container">
                    <h2>Визуализация графов</h2>
                    <p>Простой компонент для отображения графовых структур.</p>
                </div>
            </section>

            <!-- Main Graph Section -->
            <section class="main-graph-section">
                <div class="container">
                    <h3>Основной граф с диалектом</h3>
                    <p>Пример графа с поддержкой диалектов:</p>

                    <div class="graph-controls">
                        <button @click="toggleDialect"
                                class="dialect-toggle"
                                :class="{ 'active': showDialect }">
                            {{ showDialect ? 'Диалект включен' : 'Диалект выключен' }}
                        </button>
                    </div>

                    <!-- Основной большой граф -->
                    <div class="main-graph-wrapper">
                        <GraphComponent filePath="./text.txt"
                                        :xmlData="xmlStrDialectTest"
                                        :dialectData="showDialect ? dialectXML : undefined"
                                        :width="900"
                                        :height="500" />
                    </div>
                </div>
            </section>

            <!-- Additional Graphs Section -->
            <section class="additional-graphs-section">
                <div class="container">
                    <h3>Дополнительные примеры графов</h3>
                    <p>Различные типы графов для демонстрации:</p>

                    <!-- Сетка с широкими ячейками -->
                    <div class="wide-grid">
                        <!-- Первый ряд: 2 широких графа -->
                        <div class="graph-row">
                            <div class="wide-graph-cell">
                                <div class="graph-header">
                                    <h4>Комплексная структура (Egor)</h4>
                                    <span class="graph-badge">Связи + Формы</span>
                                </div>
                                <div class="graph-content">
                                    <GraphComponent 
                                                    :xmlData="xmlEgor"
                                                    :width="1600"
                                                    :height="750" />
                                </div>
                                <div class="graph-description">
                                    <p>Сложная графовая структура с множеством узлов и связей</p>
                                </div>
                            </div>
                        </div>

                        <div class="graph-row">
                            <div class="wide-graph-cell">
                                <div class="graph-header">
                                    <h4>Простой граф с кругами</h4>
                                    <span class="graph-badge">Базовый</span>
                                </div>
                                <div class="graph-content">
                                    <GraphComponent filePath="./text.txt"
                                                    :xmlData="xmlStr3"
                                                    :width="800"
                                                    :height="350" />
                                </div>
                                <div class="graph-description">
                                    <p>Простой граф с цветными узлами типа "circle"</p>
                                </div>
                            </div>
                        </div>

                        <!-- Второй ряд: 2 широких графа -->
                        <div class="graph-row">
                            <div class="wide-graph-cell">
                                <div class="graph-header">
                                    <h4>Кастомные фигуры</h4>
                                    <span class="graph-badge">Разнообразие</span>
                                </div>
                                <div class="graph-content">
                                    <GraphComponent filePath="./text.txt"
                                                    :xmlData="xmlString2"
                                                    :width="500"
                                                    :height="450" />
                                </div>
                                <div class="graph-description">
                                    <p>Различные геометрические фигуры: треугольники, эллипсы, звезды</p>
                                </div>
                            </div>
                        </div>

                        <div class="graph-row">
                            <div class="wide-graph-cell">
                                <div class="graph-header">
                                    <h4>Пустой канвас</h4>
                                    <span class="graph-badge">Минимальный</span>
                                </div>
                                <div class="graph-content">
                                    <GraphComponent filePath="./text.txt"
                                                    :xmlData="xmlStrClear"
                                                    :width="500"
                                                    :height="450" />
                                </div>
                                <div class="graph-description">
                                    <p>Базовый пустой граф для начала работы</p>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </section>

            <!-- Quick Links -->
            <section class="quick-links-section">
                <div class="container">
                    <h3>Быстрый переход</h3>
                    <div class="links-row">
                        <router-link to="/api-test" class="link-card wide">
                            <div class="link-icon">🔧</div>
                            <h4>Тестирование API</h4>
                            <p>Проверка API endpoints и интеграций с внешними сервисами</p>
                        </router-link>

                        <router-link to="/tests" class="link-card wide">
                            <div class="link-icon">🧪</div>
                            <h4>Тесты</h4>
                            <p>Unit и интеграционные тесты системы визуализации графов</p>
                        </router-link>

                        <router-link to="/generator" class="link-card wide">
                            <div class="link-icon">⚡</div>
                            <h4>Генератор</h4>
                            <p>Создание графовых структур из шаблонов и пользовательских настроек</p>
                        </router-link>
                    </div>
                </div>
            </section>
        </div>
    </div>
</template>

<script lang="ts">
    import { defineComponent, ref } from 'vue'
    import GraphComponent from '@/components/GraphComponent.vue'
    import { useGraphData } from '@/composables/useGraphData'

    export default defineComponent({
        name: 'MainView',
        components: {
            GraphComponent
        },
        setup() {
            const showDialect = ref(true)

            // Используем композабл для получения данных
            const {
                xmlStrDialectTest,
                dialectXML,
                xmlStr3,
                xmlEgor,
                xmlString2,
                xmlStrClear
            } = useGraphData()

            const toggleDialect = () => {
                showDialect.value = !showDialect.value
            }

            return {
                showDialect,
                toggleDialect,
                xmlStrDialectTest,
                dialectXML,
                xmlStr3,
                xmlEgor,
                xmlString2,
                xmlStrClear
            }
        }
    })
</script>

<style scoped>
    .main-view {
        padding: 20px 0;
        background-color: #f8f9fa;
    }

    .container {
        width: 100%;
        max-width: 1200px;
        margin: 0 auto;
        padding: 0 20px;
    }

    /* Hero Section */
    .hero {
        background: linear-gradient(135deg, #1976d2 0%, #1565c0 100%);
        padding: 40px 30px;
        text-align: center;
        border-radius: 12px;
        margin-bottom: 40px;
        color: white;
        box-shadow: 0 4px 15px rgba(25, 118, 210, 0.2);
    }

        .hero h2 {
            font-size: 2.8rem;
            margin-bottom: 15px;
            font-weight: 600;
        }

        .hero p {
            font-size: 1.2rem;
            opacity: 0.9;
            max-width: 600px;
            margin: 0 auto;
            line-height: 1.6;
        }

    /* Main Graph Section */
    .main-graph-section {
        background-color: white;
        padding: 40px 30px;
        border-radius: 12px;
        box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08);
        margin-bottom: 40px;
    }

        .main-graph-section h3 {
            font-size: 2.2rem;
            margin-bottom: 15px;
            color: #333;
            font-weight: 600;
        }

        .main-graph-section p {
            font-size: 1.1rem;
            color: #666;
            margin-bottom: 25px;
            line-height: 1.6;
        }

    .graph-controls {
        margin-bottom: 25px;
    }

    .dialect-toggle {
        padding: 12px 24px;
        background-color: #1976d2;
        color: white;
        border: none;
        border-radius: 6px;
        cursor: pointer;
        font-size: 16px;
        font-weight: 500;
        transition: all 0.3s ease;
        box-shadow: 0 2px 8px rgba(25, 118, 210, 0.3);
    }

        .dialect-toggle:hover {
            background-color: #1565c0;
            transform: translateY(-2px);
            box-shadow: 0 4px 12px rgba(25, 118, 210, 0.4);
        }

        .dialect-toggle.active {
            background-color: #4caf50;
            box-shadow: 0 2px 8px rgba(76, 175, 80, 0.3);
        }

            .dialect-toggle.active:hover {
                background-color: #388e3c;
                box-shadow: 0 4px 12px rgba(76, 175, 80, 0.4);
            }

    .main-graph-wrapper {
        background: #f8f9fa;
        padding: 25px;
        border-radius: 10px;
        border: 1px solid #e0e0e0;
        display: flex;
        justify-content: center;
        align-items: center;
        min-height: 520px;
    }

    /* Additional Graphs Section */
    .additional-graphs-section {
        background-color: white;
        padding: 40px 30px;
        border-radius: 12px;
        box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08);
        margin-bottom: 40px;
    }

        .additional-graphs-section h3 {
            font-size: 2.2rem;
            margin-bottom: 15px;
            color: #333;
            font-weight: 600;
        }

        .additional-graphs-section p {
            font-size: 1.1rem;
            color: #666;
            margin-bottom: 30px;
            line-height: 1.6;
        }

    /* Широкая сетка */
    .wide-grid {
        display: flex;
        flex-direction: column;
        gap: 30px;
    }

    .graph-row {
        display: flex;
        justify-content: center;
    }

    .wide-graph-cell {
        width: 100%;
        max-width: 850px;
        background: #f8f9fa;
        border-radius: 10px;
        overflow: hidden;
        border: 1px solid #e0e0e0;
        transition: all 0.3s ease;
    }

        .wide-graph-cell:hover {
            transform: translateY(-5px);
            box-shadow: 0 8px 25px rgba(0, 0, 0, 0.1);
            border-color: #1976d2;
        }

    .graph-header {
        background: white;
        padding: 20px;
        border-bottom: 1px solid #e0e0e0;
        display: flex;
        justify-content: space-between;
        align-items: center;
    }

        .graph-header h4 {
            margin: 0;
            color: #333;
            font-size: 1.4rem;
            font-weight: 600;
        }

    .graph-badge {
        background: #1976d2;
        color: white;
        padding: 6px 12px;
        border-radius: 20px;
        font-size: 0.9rem;
        font-weight: 500;
    }

    .graph-content {
        padding: 25px;
        display: flex;
        justify-content: center;
        align-items: center;
        min-height: 380px;
    }

    .graph-description {
        background: white;
        padding: 20px;
        border-top: 1px solid #e0e0e0;
    }

        .graph-description p {
            margin: 0;
            color: #666;
            font-size: 1rem;
            line-height: 1.5;
            text-align: center;
        }

    /* Quick Links Section */
    .quick-links-section {
        background-color: white;
        padding: 40px 30px;
        border-radius: 12px;
        box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08);
    }

        .quick-links-section h3 {
            font-size: 2.2rem;
            margin-bottom: 30px;
            color: #333;
            font-weight: 600;
            text-align: center;
        }

    .links-row {
        display: flex;
        flex-direction: column;
        gap: 25px;
        align-items: center;
    }

    .link-card.wide {
        width: 100%;
        max-width: 850px;
        background: linear-gradient(135deg, #f8f9fa 0%, #e9ecef 100%);
        padding: 35px 40px;
        border-radius: 10px;
        text-decoration: none;
        color: inherit;
        border: 2px solid transparent;
        transition: all 0.3s ease;
        display: flex;
        flex-direction: column;
        align-items: center;
        text-align: center;
    }

        .link-card.wide:hover {
            transform: translateY(-5px);
            border-color: #1976d2;
            box-shadow: 0 8px 25px rgba(25, 118, 210, 0.15);
            background: linear-gradient(135deg, #e3f2fd 0%, #bbdefb 100%);
        }

    .link-icon {
        font-size: 3.5rem;
        margin-bottom: 20px;
    }

    .link-card.wide h4 {
        margin: 0 0 15px 0;
        color: #1976d2;
        font-size: 1.6rem;
        font-weight: 600;
    }

    .link-card.wide p {
        margin: 0;
        color: #666;
        font-size: 1.1rem;
        line-height: 1.6;
        max-width: 700px;
    }

    /* Responsive */
    @media (max-width: 1200px) {
        .container {
            max-width: 100%;
        }

        .wide-graph-cell {
            max-width: 90%;
        }

        .link-card.wide {
            max-width: 90%;
        }
    }

    @media (max-width: 768px) {
        .hero {
            padding: 30px 20px;
        }

            .hero h2 {
                font-size: 2.2rem;
            }

        .main-graph-section,
        .additional-graphs-section,
        .quick-links-section {
            padding: 30px 20px;
        }

            .main-graph-section h3,
            .additional-graphs-section h3,
            .quick-links-section h3 {
                font-size: 1.8rem;
            }

        .graph-header {
            flex-direction: column;
            gap: 10px;
            text-align: center;
        }

        .graph-content {
            padding: 15px;
        }

        .link-card.wide {
            padding: 25px 20px;
        }

        .link-icon {
            font-size: 2.8rem;
        }

        .link-card.wide h4 {
            font-size: 1.4rem;
        }
    }

    @media (max-width: 480px) {
        .hero h2 {
            font-size: 1.8rem;
        }

        .hero p {
            font-size: 1rem;
        }

        .main-graph-section h3,
        .additional-graphs-section h3,
        .quick-links-section h3 {
            font-size: 1.6rem;
        }

        .dialect-toggle {
            width: 100%;
            padding: 15px;
        }

        .wide-graph-cell {
            width: 100%;
        }
    }
</style>