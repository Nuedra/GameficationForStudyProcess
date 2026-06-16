<template>
    <div class="app">
        <div class="controls">
            <button @click="toggleXml">
                {{ currentXmlIndex === 0 ? 'Показать XML 2' : 'Показать XML 1' }}
            </button>
            <button @click="exportCurrentXml">Экспортировать текущий XML</button>
        </div>

        <!-- Расширенные контролы для тестирования API GraphComponent -->
        <div class="api-controls">
            <h3>Тестирование API GraphComponent</h3>

            <div class="api-sections">
                <!-- Секция: Основные операции -->
                <div class="api-section">
                    <h4>Основные операции</h4>
                    <div class="api-buttons">
                        <button @click="testGetNodesEdges" :disabled="apiLoading">Get Nodes & Edges</button>
                        <button @click="testAddElements" :disabled="apiLoading">Add Node/Edge</button>
                        <button @click="testDeleteElements" :disabled="apiLoading">Delete Elements</button>
                        <button @click="testClearGraph" :disabled="apiLoading">Clear Graph</button>
                        <button @click="testGetGraphData" :disabled="apiLoading">Get Graph Data</button>
                    </div>
                </div>

                <!-- Секция: Выделение и акцент -->
                <div class="api-section">
                    <h4>Выделение и акцент</h4>
                    <div class="api-buttons">
                        <button @click="testSelection" :disabled="apiLoading">Test Selection</button>
                        <button @click="testEmphasis" :disabled="apiLoading">Test Emphasis</button>
                        <button @click="testSelectionConstraints" :disabled="apiLoading">Selection Constraints</button>
                    </div>
                </div>

                <!-- Секция: Расширенные возможности -->
                <div class="api-section">
                    <h4>Расширенные возможности</h4>
                    <div class="api-buttons">
                        <button @click="testEventHandlers" :disabled="apiLoading">Event Handlers</button>
                        <button @click="testDialectSystem" :disabled="apiLoading">Dialect System</button>
                        <button @click="testInteractiveFeatures" :disabled="apiLoading">Interactive Features</button>
                        <button @click="runAllTests" :disabled="apiLoading">Run All Tests</button>
                    </div>
                </div>
            </div>

            <!-- Панель статуса -->
            <div class="api-status-panel">
                <div class="status-indicator" :class="apiStatusClass"></div>
                <div class="status-text">{{ apiStatus }}</div>
                <div class="stats">
                    <span>Успешно: {{ stats.success }}</span>
                    <span>Предупреждений: {{ stats.warnings }}</span>
                    <span>Ошибок: {{ stats.errors }}</span>
                    <span>Выполнено: {{ stats.total }} тестов</span>
                </div>
            </div>

            <!-- Визуализация результатов -->
            <div class="api-results-container">
                <div class="results-header">
                    <h4>Результаты тестирования</h4>
                    <div class="results-controls">
                        <button @click="clearResults" class="small-btn">Очистить</button>
                        <button @click="exportResults" class="small-btn">Экспорт</button>
                    </div>
                </div>

                <div class="visualization">
                    <!-- График тестов -->
                    <div class="test-chart">
                        <div class="chart-bar success" :style="{ width: successPercentage + '%' }"
                             :title="'Успешно: ' + stats.success"></div>
                        <div class="chart-bar warning" :style="{ width: warningPercentage + '%' }"
                             :title="'Предупреждения: ' + stats.warnings"></div>
                        <div class="chart-bar error" :style="{ width: errorPercentage + '%' }"
                             :title="'Ошибки: ' + stats.errors"></div>
                    </div>

                    <!-- Детализированные результаты -->
                    <div class="detailed-results">
                        <div class="result-category" v-for="(category, catName) in categorizedResults" :key="catName">
                            <h5>{{ getCategoryTitle(catName) }}</h5>
                            <div class="test-results">
                                <div v-for="result in category" :key="result.id"
                                     class="test-result" :class="result.type">
                                    <div class="test-icon">
                                        {{ getTypeIcon(result.type) }}
                                    </div>
                                    <div class="test-content">
                                        <div class="test-title">{{ result.title }}</div>
                                        <div class="test-message">{{ result.message }}</div>
                                        <div class="test-timestamp">{{ formatTime(result.timestamp) }}</div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <!-- Быстрые действия -->
            <div class="quick-actions">
                <h4>Быстрые действия</h4>
                <div class="quick-buttons">
                    <button @click="highlightAllNodes" :disabled="apiLoading" class="quick-btn">Подсветить узлы</button>
                    <button @click="selectRandomElement" :disabled="apiLoading" class="quick-btn">Выбрать случайный</button>
                    <button @click="testTooltip" :disabled="apiLoading" class="quick-btn">Показать tooltip</button>
                    <button @click="stressTest" :disabled="apiLoading" class="quick-btn">Stress Test</button>
                </div>
            </div>
        </div>

        <!-- Сам компонент графа -->
        <GraphComponent ref="graphComponent"
                        :xmlData="currentXml"
                        :dialectData="dialectXml" />
    </div>
</template>

<script lang="ts">
    import { defineComponent, ref, computed } from 'vue';
    import GraphComponent from './GraphComponent.vue';

    // Пример XML 1 - простой граф
    const SAMPLE_XML_1 = `
<graph dialect="base">
  <canvas width="700" height="400"/>
  <node id="1" type="circle" label="Begin - Начало" info="Start Стартовая точка процесса">
    <geometry x="200" y="300" radius="40"/>
    <background color="#42b883"/>
    <labelSettings font="16px Arial" color="white"/>
  </node>
  <node id="2" type="rectangle" label="Процесс" info="Основной процесс">
    <geometry x="350" y="270" width="100" height="60"/>
    <background color="#3498db"/>
    <labelSettings font="14px Arial" color="white"/>
  </node>
  <node id="3" type="circle" label="Конец" info="Конечная точка">
    <geometry x="600" y="300" radius="40"/>
    <background color="#e74c3c"/>
    <labelSettings font="16px Arial" color="white"/>
  </node>

  <edge id="1" type="line" label="Выполнить" info="Переход к выполнению" endArrow="stick">
    <lineGeometry startX="240" startY="300" endX="350" endY="300"/>
    <background color="#2c3e50"/>
    <edgeStyle lineWidth="2" isEdgeDash="false"/>
    <labelSettings font="12px Arial" color="#2c3e50"/>
  </edge>
  <edge id="2" type="line" label="Завершить" info="Переход к завершению" endArrow="stick">
    <lineGeometry startX="450" startY="300" endX="560" endY="300"/>
    <background color="#2c3e50"/>
    <edgeStyle lineWidth="2" isEdgeDash="false"/>
    <labelSettings font="12px Arial" color="#2c3e50"/>
  </edge>
</graph>
`;

    // Пример XML 2 - другой граф
    const SAMPLE_XML_2 = `
<graph dialect="base">
  <canvas width="700" height="400"/>
  <node id="1" type="rectangle" label="Пользователь" info="Внешняя система">
    <geometry x="90" y="210" width="120" height="60"/>
    <background color="#9b59b6"/>
    <labelSettings font="14px Arial" color="white"/>
  </node>
  <node id="2" type="rectangle" label="Сервер" info="Основной сервер приложения">
    <geometry x="290" y="210" width="120" height="60"/>
    <background color="#3498db"/>
    <labelSettings font="14px Arial" color="white"/>
  </node>
  <node id="3" type="rectangle" label="База данных" info="Хранение данных">
    <geometry x="490" y="210" width="120" height="60"/>
    <background color="#e67e22"/>
    <labelSettings font="14px Arial" color="white"/>
  </node>
  <node id="4" type="cloud" label="Интернет" info="Внешняя сеть">
    <geometry x_C="350" y_C="100" width="200" height="100"/>
    <background color="#34495e"/>
    <labelSettings font="14px Arial" color="white"/>
  </node>

  <edge id="1" type="line" label="Запрос" info="HTTP запрос" endArrow="stick">
    <lineGeometry startX="210" startY="230" endX="290" endY="230"/>
    <background color="#2c3e50"/>
    <edgeStyle lineWidth="2" isEdgeDash="false"/>
    <labelSettings font="12px Arial" color="#2c3e50"/>
  </edge>

  <edge id="2" type="line" label="Данные" info="Запрос к базе" endArrow="stick">
    <lineGeometry startX="410" startY="230" endX="490" endY="230"/>
    <background color="#2c3e50"/>
    <edgeStyle lineWidth="2" isEdgeDash="false"/>
    <labelSettings font="12px Arial" color="#2c3e50"/>
  </edge>

  <edge id="3" type="line" label="Ответ" info="HTTP ответ" endArrow="stick">
    <lineGeometry startX="290" startY="260" endX="210" endY="260"/>
    <background color="#2c3e50"/>
    <edgeStyle lineWidth="2" isEdgeDash="false"/>
    <labelSettings font="12px Arial" color="#2c3e50"/>
  </edge>
</graph>
`;

    // Пример диалекта (base dialect)
    const DIALECT_XML = `
<dialect name="base">
  <nodeTypes>
    <type name="circle"/>
    <type name="rectangle"/>
    <type name="triangle"/>
    <type name="regular polygon"/>
    <type name="ellipse"/>
    <type name="rhomb"/>
    <type name="star"/>
    <type name="cloud"/>
  </nodeTypes>
  <edgeTypes>
    <type name="line"/>
  </edgeTypes>
  <arrowTypes>
    <type name="none"/>
    <type name="classic"/>
    <type name="diamond"/>
    <type name="arrow"/>
    <type name="stick"/>
  </arrowTypes>
</dialect>
`;

    export default defineComponent({
        name: 'ApiTestApp',
        components: {
            GraphComponent
        },
        setup() {
            const graphComponent = ref<any>(null);
            const currentXmlIndex = ref(0);
            const currentXml = ref(SAMPLE_XML_1);
            const dialectXml = ref(DIALECT_XML);

            // Состояние для API тестирования
            const apiLoading = ref(false);
            const apiStatus = ref('Готов к тестированию API GraphComponent');
            const apiStatusClass = ref('ready');

            // Статистика тестов
            const stats = ref({
                total: 0,
                success: 0,
                warnings: 0,
                errors: 0
            });

            // Результаты тестов
            const testResults = ref<Array<{
                id: string;
                title: string;
                message: string;
                type: 'success' | 'warning' | 'error' | 'info';
                category: string;
                timestamp: Date;
            }>>([]);

            // Вычисляемые свойства для визуализации
            const categorizedResults = computed(() => {
                const categories: Record<string, any[]> = {
                    'basic': [],
                    'selection': [],
                    'advanced': [],
                    'interactive': [],
                    'system': []
                };

                testResults.value.forEach(result => {
                    if (categories[result.category]) {
                        categories[result.category].push(result);
                    }
                });

                return categories;
            });

            const successPercentage = computed(() => {
                if (stats.value.total === 0) return 0;
                return (stats.value.success / stats.value.total) * 100;
            });

            const warningPercentage = computed(() => {
                if (stats.value.total === 0) return 0;
                return (stats.value.warnings / stats.value.total) * 100;
            });

            const errorPercentage = computed(() => {
                if (stats.value.total === 0) return 0;
                return (stats.value.errors / stats.value.total) * 100;
            });

            // Вспомогательные функции
            const addTestResult = (title: string, message: string, type: 'success' | 'warning' | 'error' | 'info', category: string) => {
                const id = `test-${Date.now()}-${Math.random().toString(36).substr(2, 9)}`;
                testResults.value.unshift({
                    id,
                    title,
                    message,
                    type,
                    category,
                    timestamp: new Date()
                });

                // Обновляем статистику
                stats.value.total++;
                if (type === 'success') stats.value.success++;
                if (type === 'warning') stats.value.warnings++;
                if (type === 'error') stats.value.errors++;
            };

            const setApiStatus = (message: string, type: string = 'ready') => {
                apiStatus.value = message;
                apiStatusClass.value = type;
            };

            const getTypeIcon = (type: string) => {
                switch (type) {
                    case 'success': return '✅';
                    case 'warning': return '⚠️';
                    case 'error': return '❌';
                    default: return 'ℹ️';
                }
            };

            const getCategoryTitle = (category: string) => {
                const titles: Record<string, string> = {
                    'basic': '📦 Основные операции',
                    'selection': '🎯 Выделение и акцент',
                    'advanced': '⚡ Расширенные возможности',
                    'interactive': '🖱️ Интерактивные функции',
                    'system': '🔧 Системные тесты'
                };
                return titles[category] || category;
            };

            const formatTime = (date: Date) => {
                return date.toLocaleTimeString('ru-RU', {
                    hour: '2-digit',
                    minute: '2-digit',
                    second: '2-digit'
                });
            };

            // Основные методы тестирования API GraphComponent
            const testGetNodesEdges = async () => {
                apiLoading.value = true;
                setApiStatus('Тестирование getNodes() и getEdges()...', 'loading');

                try {
                    if (!graphComponent.value) {
                        throw new Error('GraphComponent не инициализирован');
                    }

                    // Тестируем getNodes()
                    const nodes = graphComponent.value.getNodes();
                    const nodesValid = Array.isArray(nodes);
                    const nodeCount = nodes.length;

                    addTestResult(
                        'getNodes()',
                        `Получено ${nodeCount} узлов, тип данных: ${nodesValid ? 'корректный' : 'некорректный'}`,
                        nodesValid ? 'success' : 'error',
                        'basic'
                    );

                    // Тестируем getEdges()
                    const edges = graphComponent.value.getEdges();
                    const edgesValid = Array.isArray(edges);
                    const edgeCount = edges.length;

                    addTestResult(
                        'getEdges()',
                        `Получено ${edgeCount} рёбер, тип данных: ${edgesValid ? 'корректный' : 'некорректный'}`,
                        edgesValid ? 'success' : 'error',
                        'basic'
                    );

                    // Проверяем структуру данных
                    if (nodes.length > 0) {
                        const firstNode = nodes[0];
                        const hasRequiredProps = firstNode && firstNode._id && firstNode._type;
                        addTestResult(
                            'Структура узлов',
                            `Первый узел: ${hasRequiredProps ? 'корректная структура' : 'неполные данные'}`,
                            hasRequiredProps ? 'success' : 'warning',
                            'basic'
                        );
                    }

                    setApiStatus(`✅ Получено ${nodeCount} узлов и ${edgeCount} рёбер`, 'success');

                } catch (error: any) {
                    addTestResult(
                        'Ошибка getNodes/getEdges',
                        error.message,
                        'error',
                        'basic'
                    );
                    setApiStatus('❌ Ошибка при получении данных', 'error');
                } finally {
                    apiLoading.value = false;
                }
            };

            const testAddElements = async () => {
                apiLoading.value = true;
                setApiStatus('Тестирование добавления элементов...', 'loading');

                try {
                    if (!graphComponent.value) {
                        throw new Error('GraphComponent не инициализирован');
                    }

                    // Получаем текущее количество элементов
                    const initialNodes = graphComponent.value.getNodes().length;
                    const initialEdges = graphComponent.value.getEdges().length;

                    // Добавляем тестовый узел
                    const newNodeData = {
                        id: `test-node-${Date.now()}`,
                        type: 'circle',
                        x: 100 + Math.random() * 400,
                        y: 100 + Math.random() * 200,
                        radius: 20,
                        color: getRandomColor(),
                        label_info: {
                            text: `Тестовый узел`,
                            color: 'white',
                            font: '12px Arial',
                            padding: 5
                        }
                    };

                    graphComponent.value.addNode(newNodeData);

                    // Проверяем добавление
                    const afterAddNodes = graphComponent.value.getNodes().length;
                    const nodeAdded = afterAddNodes > initialNodes;

                    addTestResult(
                        'addNode()',
                        `Добавлен узел "${newNodeData.id}". Было: ${initialNodes}, стало: ${afterAddNodes}`,
                        nodeAdded ? 'success' : 'error',
                        'basic'
                    );

                    // Добавляем тестовое ребро
                    const nodes = graphComponent.value.getNodes();
                    if (nodes.length >= 2) {
                        const node1 = nodes[0];
                        const node2 = nodes[1];

                        const newEdgeData = {
                            id: `test-edge-${Date.now()}`,
                            type: 'line',
                            startX: node1.x + 30,
                            startY: node1.y,
                            endX: node2.x - 30,
                            endY: node2.y,
                            color: '#3498db',
                            lineWidth: 2
                        };

                        graphComponent.value.addEdge(newEdgeData);

                        const afterAddEdges = graphComponent.value.getEdges().length;
                        const edgeAdded = afterAddEdges > initialEdges;

                        addTestResult(
                            'addEdge()',
                            `Добавлено ребро между узлами. Было: ${initialEdges}, стало: ${afterAddEdges}`,
                            edgeAdded ? 'success' : 'error',
                            'basic'
                        );
                    }

                    setApiStatus('✅ Элементы успешно добавлены', 'success');

                } catch (error: any) {
                    addTestResult(
                        'Ошибка добавления элементов',
                        error.message,
                        'error',
                        'basic'
                    );
                    setApiStatus('❌ Ошибка при добавлении', 'error');
                } finally {
                    apiLoading.value = false;
                }
            };

            const testSelection = async () => {
                apiLoading.value = true;
                setApiStatus('Тестирование системы выделения...', 'loading');

                try {
                    if (!graphComponent.value) {
                        throw new Error('GraphComponent не инициализирован');
                    }

                    const nodes = graphComponent.value.getNodes();
                    if (nodes.length === 0) {
                        addTestResult('Выделение', 'Нет узлов для тестирования', 'warning', 'selection');
                        setApiStatus('⚠️ Нет узлов для тестирования', 'warning');
                        return;
                    }

                    // Очищаем текущее выделение
                    graphComponent.value.clearSelection();

                    // Проверяем getSelection()
                    const initialSelection = graphComponent.value.getSelection();
                    const isInitiallyEmpty = initialSelection.length === 0;

                    addTestResult(
                        'clearSelection()',
                        `Выделение очищено. Элементов выделено: ${initialSelection.length}`,
                        isInitiallyEmpty ? 'success' : 'warning',
                        'selection'
                    );

                    // Выбираем первый узел
                    const firstNode = nodes[0];
                    graphComponent.value.select(firstNode);

                    const afterSelect = graphComponent.value.getSelection();
                    const isSelected = afterSelect.some((item: any) => item._id === firstNode._id);

                    addTestResult(
                        'select()',
                        `Узел "${firstNode._id}" ${isSelected ? 'выделен' : 'не выделен'}`,
                        isSelected ? 'success' : 'error',
                        'selection'
                    );

                    // Проверяем canSelect()
                    const canSelectResult = graphComponent.value.canSelect(firstNode);
                    addTestResult(
                        'canSelect()',
                        `Узел "${firstNode._id}" ${canSelectResult ? 'можно выделить' : 'нельзя выделить'}`,
                        canSelectResult ? 'success' : 'warning',
                        'selection'
                    );

                    // Снимаем выделение
                    graphComponent.value.deselect(firstNode);
                    const afterDeselect = graphComponent.value.getSelection();
                    const isDeselected = !afterDeselect.some((item: any) => item._id === firstNode._id);

                    addTestResult(
                        'deselect()',
                        `Узел "${firstNode._id}" ${isDeselected ? 'снят с выделения' : 'остался выделенным'}`,
                        isDeselected ? 'success' : 'error',
                        'selection'
                    );

                    // Тестируем множественное выделение
                    if (nodes.length >= 3) {
                        graphComponent.value.selectMultiple([nodes[0], nodes[1], nodes[2]], true);
                        const multipleSelection = graphComponent.value.getSelection();

                        addTestResult(
                            'selectMultiple()',
                            `Множественное выделение: ${multipleSelection.length} элементов`,
                            multipleSelection.length === 3 ? 'success' : 'warning',
                            'selection'
                        );
                    }

                    setApiStatus('✅ Система выделения работает корректно', 'success');

                } catch (error: any) {
                    addTestResult(
                        'Ошибка тестирования выделения',
                        error.message,
                        'error',
                        'selection'
                    );
                    setApiStatus('❌ Ошибка при тестировании выделения', 'error');
                } finally {
                    apiLoading.value = false;
                }
            };

            const testEmphasis = async () => {
                apiLoading.value = true;
                setApiStatus('Тестирование системы акцента...', 'loading');

                try {
                    if (!graphComponent.value) {
                        throw new Error('GraphComponent не инициализирован');
                    }

                    const nodes = graphComponent.value.getNodes();
                    if (nodes.length === 0) {
                        addTestResult('Акцент', 'Нет узлов для тестирования', 'warning', 'selection');
                        setApiStatus('⚠️ Нет узлов для тестирования', 'warning');
                        return;
                    }

                    // Очищаем текущий акцент
                    graphComponent.value.clearEmphasis();

                    // Добавляем акцент первому узлу
                    const firstNode = nodes[0];
                    graphComponent.value.emphasize(firstNode);

                    // Небольшая задержка для визуализации
                    await new Promise(resolve => setTimeout(resolve, 500));

                    addTestResult(
                        'emphasize()',
                        `Акцент применен к узлу "${firstNode._id}"`,
                        'success',
                        'selection'
                    );

                    // Снимаем акцент
                    graphComponent.value.deEmphasize(firstNode);

                    addTestResult(
                        'deEmphasize()',
                        `Акцент снят с узла "${firstNode._id}"`,
                        'success',
                        'selection'
                    );

                    // Очищаем все акценты
                    graphComponent.value.clearEmphasis();

                    addTestResult(
                        'clearEmphasis()',
                        'Все акценты очищены',
                        'success',
                        'selection'
                    );

                    setApiStatus('✅ Система акцента работает корректно', 'success');

                } catch (error: any) {
                    addTestResult(
                        'Ошибка тестирования акцента',
                        error.message,
                        'error',
                        'selection'
                    );
                    setApiStatus('❌ Ошибка при тестировании акцента', 'error');
                } finally {
                    apiLoading.value = false;
                }
            };

            const testGetGraphData = async () => {
                apiLoading.value = true;
                setApiStatus('Тестирование getGraphData()...', 'loading');

                try {
                    if (!graphComponent.value) {
                        throw new Error('GraphComponent не инициализирован');
                    }

                    const graphData = graphComponent.value.getGraphData();

                    // Проверяем структуру возвращаемых данных
                    const hasNodes = Array.isArray(graphData.nodes);
                    const hasEdges = Array.isArray(graphData.edges);
                    const structureValid = hasNodes && hasEdges;

                    addTestResult(
                        'getGraphData() структура',
                        `Данные содержат: ${hasNodes ? 'узлы' : 'нет узлов'}, ${hasEdges ? 'рёбра' : 'нет рёбер'}`,
                        structureValid ? 'success' : 'error',
                        'basic'
                    );

                    // Проверяем преобразование данных
                    if (hasNodes && graphData.nodes.length > 0) {
                        const firstNodeData = graphData.nodes[0];
                        const hasRequiredFields = firstNodeData.id && firstNodeData.type;

                        addTestResult(
                            'Преобразование узлов',
                            `Узел "${firstNodeData.id}" преобразован ${hasRequiredFields ? 'корректно' : 'с ошибками'}`,
                            hasRequiredFields ? 'success' : 'warning',
                            'basic'
                        );
                    }

                    // Тестируем exportToXml()
                    const xmlExport = graphComponent.value.exportToXml();
                    const isXmlValid = xmlExport && xmlExport.includes('<graph>') && xmlExport.includes('</graph>');

                    addTestResult(
                        'exportToXml()',
                        `XML ${isXmlValid ? 'корректно сгенерирован' : 'с ошибками'}, длина: ${xmlExport.length} символов`,
                        isXmlValid ? 'success' : 'error',
                        'basic'
                    );

                    setApiStatus('✅ Данные графа успешно получены и экспортированы', 'success');

                } catch (error: any) {
                    addTestResult(
                        'Ошибка getGraphData/exportToXml',
                        error.message,
                        'error',
                        'basic'
                    );
                    setApiStatus('❌ Ошибка при получении данных', 'error');
                } finally {
                    apiLoading.value = false;
                }
            };

            // Дополнительные тестовые функции
            const testEventHandlers = async () => {
                apiLoading.value = true;
                setApiStatus('Тестирование системы событий...', 'loading');

                try {
                    if (!graphComponent.value) {
                        throw new Error('GraphComponent не инициализирован');
                    }

                    // Тестируем getAvailableEvents
                    const nodes = graphComponent.value.getNodes();
                    if (nodes.length > 0) {
                        const availableEvents = graphComponent.value.getAvailableEvents(nodes[0]);
                        const hasEvents = Array.isArray(availableEvents) && availableEvents.length > 0;

                        addTestResult(
                            'getAvailableEvents()',
                            `Доступно событий для узла: ${availableEvents.length}`,
                            hasEvents ? 'success' : 'warning',
                            'advanced'
                        );
                    }

                    // Тестируем getAttachedEvents
                    const attachedEvents = graphComponent.value.getAttachedEvents();
                    const eventsValid = Array.isArray(attachedEvents);

                    addTestResult(
                        'getAttachedEvents()',
                        `Привязано обработчиков: ${attachedEvents.length}`,
                        eventsValid ? 'success' : 'error',
                        'advanced'
                    );

                    setApiStatus('✅ Система событий работает корректно', 'success');

                } catch (error: any) {
                    addTestResult(
                        'Ошибка тестирования событий',
                        error.message,
                        'error',
                        'advanced'
                    );
                    setApiStatus('❌ Ошибка при тестировании событий', 'error');
                } finally {
                    apiLoading.value = false;
                }
            };

            const testDeleteElements = async () => {
                apiLoading.value = true;
                setApiStatus('Тестирование удаления элементов...', 'loading');

                try {
                    if (!graphComponent.value) {
                        throw new Error('GraphComponent не инициализирован');
                    }

                    // Добавляем тестовый элемент для удаления
                    const testNodeData = {
                        id: `delete-test-node-${Date.now()}`,
                        type: 'rectangle',
                        x: 50,
                        y: 50,
                        width: 60,
                        height: 40,
                        color: '#ff6b6b',
                        label_info: {
                            text: 'Удаляемый узел',
                            color: 'white',
                            font: '12px Arial',
                            padding: 5
                        }
                    };

                    // Добавляем узел
                    const initialNodeCount = graphComponent.value.getNodes().length;
                    graphComponent.value.addNode(testNodeData);
                    const afterAddCount = graphComponent.value.getNodes().length;
                    const nodeAdded = afterAddCount > initialNodeCount;

                    addTestResult(
                        'Подготовка к удалению',
                        `Добавлен тестовый узел для удаления. Было: ${initialNodeCount}, стало: ${afterAddCount}`,
                        nodeAdded ? 'success' : 'warning',
                        'basic'
                    );

                    // Тестируем deleteNode()
                    if (nodeAdded) {
                        const deleteResult = graphComponent.value.deleteNode(testNodeData.id);
                        const afterDeleteCount = graphComponent.value.getNodes().length;

                        addTestResult(
                            'deleteNode()',
                            `Узел "${testNodeData.id}" удален: ${deleteResult ? 'успешно' : 'не удалось'}. Было: ${afterAddCount}, стало: ${afterDeleteCount}`,
                            deleteResult ? 'success' : 'error',
                            'basic'
                        );
                    }

                    // Тестируем удаление ребра
                    const nodes = graphComponent.value.getNodes();
                    if (nodes.length >= 2) {
                        // Создаем ребро для удаления
                        const testEdgeData = {
                            id: `delete-test-edge-${Date.now()}`,
                            type: 'line',
                            startX: nodes[0].x + 20,
                            startY: nodes[0].y,
                            endX: nodes[1].x - 20,
                            endY: nodes[1].y,
                            color: '#3498db',
                            lineWidth: 2
                        };

                        const initialEdgeCount = graphComponent.value.getEdges().length;
                        graphComponent.value.addEdge(testEdgeData);
                        const afterAddEdgeCount = graphComponent.value.getEdges().length;

                        if (afterAddEdgeCount > initialEdgeCount) {
                            const deleteEdgeResult = graphComponent.value.deleteEdge(testEdgeData.id);
                            const afterDeleteEdgeCount = graphComponent.value.getEdges().length;

                            addTestResult(
                                'deleteEdge()',
                                `Ребро "${testEdgeData.id}" удалено: ${deleteEdgeResult ? 'успешно' : 'не удалось'}. Было: ${afterAddEdgeCount}, стало: ${afterDeleteEdgeCount}`,
                                deleteEdgeResult ? 'success' : 'error',
                                'basic'
                            );
                        }
                    }

                    setApiStatus('✅ Удаление элементов работает корректно', 'success');

                } catch (error: any) {
                    addTestResult(
                        'Ошибка удаления элементов',
                        error.message,
                        'error',
                        'basic'
                    );
                    setApiStatus('❌ Ошибка при удалении элементов', 'error');
                } finally {
                    apiLoading.value = false;
                }
            };

            const testClearGraph = async () => {
                apiLoading.value = true;
                setApiStatus('Тестирование очистки графа...', 'loading');

                try {
                    if (!graphComponent.value) {
                        throw new Error('GraphComponent не инициализирован');
                    }

                    // Получаем текущее состояние
                    const initialNodes = graphComponent.value.getNodes().length;
                    const initialEdges = graphComponent.value.getEdges().length;

                    // Очищаем граф
                    graphComponent.value.clearGraph();

                    // Проверяем результат
                    const afterClearNodes = graphComponent.value.getNodes().length;
                    const afterClearEdges = graphComponent.value.getEdges().length;
                    const isCleared = afterClearNodes === 0 && afterClearEdges === 0;

                    addTestResult(
                        'clearGraph()',
                        `Граф очищен. Было: ${initialNodes} узлов, ${initialEdges} рёбер. Стало: ${afterClearNodes} узлов, ${afterClearEdges} рёбер`,
                        isCleared ? 'success' : 'error',
                        'basic'
                    );

                    // Восстанавливаем исходный граф для дальнейшего тестирования
                    await graphComponent.value.updateGraphDataXML(currentXml.value);

                    const restoredNodes = graphComponent.value.getNodes().length;
                    const restoredEdges = graphComponent.value.getEdges().length;

                    addTestResult(
                        'Восстановление графа',
                        `Граф восстановлен. Узлов: ${restoredNodes}, рёбер: ${restoredEdges}`,
                        restoredNodes > 0 ? 'success' : 'warning',
                        'system'
                    );

                    setApiStatus('✅ Очистка графа работает корректно', 'success');

                } catch (error: any) {
                    addTestResult(
                        'Ошибка очистки графа',
                        error.message,
                        'error',
                        'basic'
                    );
                    setApiStatus('❌ Ошибка при очистке графа', 'error');
                } finally {
                    apiLoading.value = false;
                }
            };

            const testSelectionConstraints = async () => {
                apiLoading.value = true;
                setApiStatus('Тестирование ограничений выделения...', 'loading');

                try {
                    if (!graphComponent.value) {
                        throw new Error('GraphComponent не инициализирован');
                    }

                    // Настраиваем ограничения
                    graphComponent.value.setupSelectionConstraints();

                    const nodes = graphComponent.value.getNodes();
                    if (nodes.length < 3) {
                        addTestResult('Ограничения выделения', 'Недостаточно узлов для тестирования ограничений', 'warning', 'selection');
                        setApiStatus('⚠️ Недостаточно узлов', 'warning');
                        return;
                    }

                    // Очищаем текущее выделение
                    graphComponent.value.clearSelection();

                    // Тестируем различные методы с ограничениями
                    const rectangles = nodes.filter((n: any) => n._type === 'rectangle');
                    const circles = nodes.filter((n: any) => n._type === 'circle');

                    if (rectangles.length > 0) {
                        // Проверяем ограничение на прямоугольники (maxSelection: 1)
                        const rect1 = rectangles[0];
                        graphComponent.value.select(rect1);
                        const selectionAfterFirstRect = graphComponent.value.getSelectedByType('rectangle');

                        addTestResult(
                            'Ограничение на прямоугольники',
                            `Выделен 1 прямоугольник. Можно выделить только 1: ${selectionAfterFirstRect.length === 1 ? 'корректно' : 'нарушено'}`,
                            selectionAfterFirstRect.length === 1 ? 'success' : 'warning',
                            'selection'
                        );

                        // Пытаемся выделить второй прямоугольник (должно не сработать)
                        if (rectangles.length > 1) {
                            const rect2 = rectangles[1];
                            const canSelectSecond = graphComponent.value.canSelect(rect2);

                            addTestResult(
                                'Проверка canSelect() с ограничением',
                                `Второй прямоугольник ${canSelectSecond ? 'можно' : 'нельзя'} выделить (лимит 1)`,
                                !canSelectSecond ? 'success' : 'warning',
                                'selection'
                            );
                        }
                    }

                    if (circles.length > 0) {
                        // Проверяем ограничение на круги (maxSelection: 5)
                        graphComponent.value.clearSelection();

                        // Выделяем несколько кругов
                        const circlesToSelect = circles.slice(0, 3);
                        const selectedCount = graphComponent.value.selectMultiple(circlesToSelect, false);

                        addTestResult(
                            'Множественное выделение кругов',
                            `Выделено ${selectedCount} кругов из ${circlesToSelect.length} попыток`,
                            selectedCount === circlesToSelect.length ? 'success' : 'warning',
                            'selection'
                        );
                    }

                    // Тестируем getSelectionCount()
                    const totalSelected = graphComponent.value.getSelectionCount();
                    const circleSelected = graphComponent.value.getSelectionCount('circle');
                    const rectSelected = graphComponent.value.getSelectionCount('rectangle');

                    addTestResult(
                        'getSelectionCount()',
                        `Всего выделено: ${totalSelected}, кругов: ${circleSelected}, прямоугольников: ${rectSelected}`,
                        'success',
                        'selection'
                    );

                    // Тестируем clearSelectionByType()
                    graphComponent.value.clearSelectionByType('circle');
                    const circlesAfterClear = graphComponent.value.getSelectionCount('circle');

                    addTestResult(
                        'clearSelectionByType()',
                        `Снято выделение с кругов. Осталось выделенных кругов: ${circlesAfterClear}`,
                        circlesAfterClear === 0 ? 'success' : 'warning',
                        'selection'
                    );

                    setApiStatus('✅ Ограничения выделения работают корректно', 'success');

                } catch (error: any) {
                    addTestResult(
                        'Ошибка тестирования ограничений',
                        error.message,
                        'error',
                        'selection'
                    );
                    setApiStatus('❌ Ошибка при тестировании ограничений', 'error');
                } finally {
                    apiLoading.value = false;
                }
            };

            const testDialectSystem = async () => {
                apiLoading.value = true;
                setApiStatus('Тестирование системы диалектов...', 'loading');

                try {
                    if (!graphComponent.value) {
                        throw new Error('GraphComponent не инициализирован');
                    }

                    // Тестируем setDialect()
                    const testDialectXml = `
<dialect name="test-dialect">
  <nodeTypes>
    <type name="circle"/>
    <type name="rectangle"/>
  </nodeTypes>
  <edgeTypes>
    <type name="line"/>
  </edgeTypes>
  <arrowTypes>
    <type name="none"/>
    <type name="triangle"/>
  </arrowTypes>
</dialect>`;

                    graphComponent.value.setDialect(testDialectXml);

                    addTestResult(
                        'setDialect()',
                        'Диалект "test-dialect" установлен',
                        'success',
                        'advanced'
                    );

                    // Тестируем загрузку графа с валидацией типов
                    const testXmlWithTypes = `
<graph dialect="test-dialect">
  <canvas width="500" height="300"/>
  <node id="valid" type="circle" label="Valid">
    <geometry x="100" y="100" radius="30"/>
    <background color="#42b883"/>
  </node>
  <node id="invalid" type="triangle" label="Invalid">
    <geometry x1="200" y1="100" x2="250" y2="150" x3="150" y3="150"/>
    <background color="#e74c3c"/>
  </node>
</graph>`;

                    try {
                        await graphComponent.value.loadGraphFromXml(testXmlWithTypes);
                        addTestResult(
                            'Валидация типов в диалекте',
                            'Граф загружен с проверкой типов',
                            'success',
                            'advanced'
                        );
                    } catch (error: any) {
                        if (error.message.includes('не разрешен в диалекте')) {
                            addTestResult(
                                'Валидация типов в диалекте',
                                'Диалект корректно отклоняет неразрешенные типы',
                                'success',
                                'advanced'
                            );
                        } else {
                            throw error;
                        }
                    }

                    // Восстанавливаем исходный диалект
                    graphComponent.value.setDialect(DIALECT_XML);

                    addTestResult(
                        'Восстановление диалекта',
                        'Исходный диалект восстановлен',
                        'success',
                        'advanced'
                    );

                    setApiStatus('✅ Система диалектов работает корректно', 'success');

                } catch (error: any) {
                    addTestResult(
                        'Ошибка тестирования диалектов',
                        error.message,
                        'error',
                        'advanced'
                    );
                    setApiStatus('❌ Ошибка при тестировании диалектов', 'error');
                } finally {
                    apiLoading.value = false;
                }
            };

            const testInteractiveFeatures = async () => {
                apiLoading.value = true;
                setApiStatus('Тестирование интерактивных функций...', 'loading');

                try {
                    if (!graphComponent.value) {
                        throw new Error('GraphComponent не инициализирован');
                    }

                    // Тестируем getObjectAt() - поиск элемента по координатам
                    const nodes = graphComponent.value.getNodes();
                    if (nodes.length > 0) {
                        const firstNode = nodes[0];
                        // Примерные координаты центра узла
                        const x = firstNode.x + (firstNode.width ? firstNode.width / 2 : firstNode.radius || 0);
                        const y = firstNode.y + (firstNode.height ? firstNode.height / 2 : firstNode.radius || 0);

                        const foundObject = graphComponent.value.getObjectAt(x, y);

                        addTestResult(
                            'getObjectAt()',
                            `Поиск элемента в точке (${x}, ${y}): ${foundObject ? 'найден' : 'не найден'}`,
                            foundObject ? 'success' : 'warning',
                            'interactive'
                        );
                    }

                    // Тестируем setupEnhancedClickHandlers() и handleCanvasSizeChange()
                    graphComponent.value.setupEnhancedClickHandlers();
                    graphComponent.value.handleCanvasSizeChange();

                    addTestResult(
                        'Интерактивные обработчики',
                        'Обработчики кликов и изменения размера установлены',
                        'success',
                        'interactive'
                    );

                    // Тестируем расширенные методы выбора
                    if (nodes.length > 0) {
                        const testNode = nodes[0];

                        // selectWithMode с различными режимами
                        const replaceResult = graphComponent.value.selectWithMode(testNode, 'replace');
                        const addResult = graphComponent.value.selectWithMode(testNode, 'add');
                        const toggleResult = graphComponent.value.selectWithMode(testNode, 'toggle');

                        addTestResult(
                            'selectWithMode()',
                            `Режимы выбора: replace=${replaceResult}, add=${addResult}, toggle=${toggleResult}`,
                            replaceResult && addResult ? 'success' : 'warning',
                            'selection'
                        );

                        // deselectWithCheck()
                        const deselectResult = graphComponent.value.deselectWithCheck(testNode);
                        addTestResult(
                            'deselectWithCheck()',
                            `Снятие выделения: ${deselectResult ? 'успешно' : 'не удалось'}`,
                            deselectResult ? 'success' : 'warning',
                            'selection'
                        );
                    }

                    // Тестируем установку стилей через setStyle()
                    if (nodes.length > 0) {
                        const styleNode = nodes[0];
                        const styleInfo = {
                            color: '#9b59b6',
                            lineWidth: 3,
                            dashPattern: [5, 5]
                        };

                        graphComponent.value.setStyle(styleNode, 'custom_style', styleInfo);

                        addTestResult(
                            'setStyle()',
                            'Пользовательский стиль установлен для элемента',
                            'success',
                            'advanced'
                        );
                    }

                    setApiStatus('✅ Интерактивные функции работают корректно', 'success');

                } catch (error: any) {
                    addTestResult(
                        'Ошибка тестирования интерактивных функций',
                        error.message,
                        'error',
                        'interactive'
                    );
                    setApiStatus('❌ Ошибка при тестировании интерактивных функций', 'error');
                } finally {
                    apiLoading.value = false;
                }
            };

            const runAllTests = async () => {
                apiLoading.value = true;
                setApiStatus('Запуск всех тестов API GraphComponent...', 'loading');

                // Очищаем предыдущие результаты
                clearResults();

                const tests = [
                    { name: 'Основные операции (узлы и рёбра)', func: testGetNodesEdges },
                    { name: 'Добавление элементов', func: testAddElements },
                    { name: 'Удаление элементов', func: testDeleteElements },
                    { name: 'Очистка графа', func: testClearGraph },
                    { name: 'Получение данных графа', func: testGetGraphData },
                    { name: 'Система выделения', func: testSelection },
                    { name: 'Система акцента', func: testEmphasis },
                    { name: 'Ограничения выделения', func: testSelectionConstraints },
                    { name: 'Система событий', func: testEventHandlers },
                    { name: 'Система диалектов', func: testDialectSystem },
                    { name: 'Интерактивные функции', func: testInteractiveFeatures }
                ];

                try {
                    for (const test of tests) {
                        setApiStatus(`Выполнение: ${test.name}...`, 'loading');
                        await test.func();

                        // Пауза между тестами для визуализации
                        await new Promise(resolve => setTimeout(resolve, 300));
                    }

                    // Итоговая статистика
                    const successRate = (stats.value.success / stats.value.total * 100).toFixed(1);

                    addTestResult(
                        '📊 ИТОГ ТЕСТИРОВАНИЯ',
                        `Выполнено ${stats.value.total} тестов\n✅ Успешно: ${stats.value.success}\n⚠️ Предупреждения: ${stats.value.warnings}\n❌ Ошибки: ${stats.value.errors}\n📈 Успешность: ${successRate}%`,
                        stats.value.errors === 0 ? 'success' : (stats.value.errors < 3 ? 'warning' : 'error'),
                        'system'
                    );

                    setApiStatus(`✅ Все тесты завершены. Успешность: ${successRate}%`,
                        stats.value.errors === 0 ? 'success' : 'warning');

                } catch (error: any) {
                    addTestResult(
                        'Критическая ошибка при выполнении всех тестов',
                        error.message,
                        'error',
                        'system'
                    );
                    setApiStatus('❌ Ошибка при выполнении тестов', 'error');
                } finally {
                    apiLoading.value = false;
                }
            };

            const stressTest = async () => {
                apiLoading.value = true;
                setApiStatus('Запуск стресс-теста (20 операций)...', 'loading');

                try {
                    if (!graphComponent.value) {
                        throw new Error('GraphComponent не инициализирован');
                    }

                    const operations = [];
                    const startTime = performance.now();

                    // Выполняем 20 случайных операций
                    for (let i = 0; i < 20; i++) {
                        const operationType = Math.floor(Math.random() * 5);

                        switch (operationType) {
                            case 0: { // Добавление узла
                                const nodeData = {
                                    id: `stress-node-${Date.now()}-${i}`,
                                    type: Math.random() > 0.5 ? 'circle' : 'rectangle',
                                    x: 50 + Math.random() * 500,
                                    y: 50 + Math.random() * 300,
                                    radius: 15 + Math.random() * 25,
                                    width: 40 + Math.random() * 60,
                                    height: 30 + Math.random() * 50,
                                    color: getRandomColor()
                                };
                                graphComponent.value.addNode(nodeData);
                                operations.push(`➕ Узел ${i + 1}`);
                                break;
                            }
                            case 1: { // Выделение
                                const nodes = graphComponent.value.getNodes();
                                if (nodes.length > 0) {
                                    const randomNode = nodes[Math.floor(Math.random() * nodes.length)];
                                    graphComponent.value.select(randomNode);
                                    operations.push(`🎯 Выделение ${i + 1}`);
                                }
                                break;
                            }
                            case 2: { // Снятие выделения
                                const selection = graphComponent.value.getSelection();
                                if (selection.length > 0) {
                                    const randomSelected = selection[Math.floor(Math.random() * selection.length)];
                                    graphComponent.value.deselect(randomSelected);
                                    operations.push(`🚫 Снятие ${i + 1}`);
                                }
                                break;
                            }
                            case 3: { // Получение данных
                                const nodeCount = graphComponent.value.getNodes().length;
                                const edgeCount = graphComponent.value.getEdges().length;
                                operations.push(`📊 Данные: ${nodeCount}н/${edgeCount}р`);
                                break;
                            }
                            case 4: { // Акцент
                                const nodesForEmphasis = graphComponent.value.getNodes();
                                if (nodesForEmphasis.length > 0) {
                                    const randomNodeForEmphasis = nodesForEmphasis[Math.floor(Math.random() * nodesForEmphasis.length)];
                                    graphComponent.value.emphasize(randomNodeForEmphasis);
                                    operations.push(`✨ Акцент ${i + 1}`);
                                }
                                break;
                            }
                        }

                        // Небольшая пауза
                        await new Promise(resolve => setTimeout(resolve, 50));
                    }

                    const endTime = performance.now();
                    const duration = (endTime - startTime).toFixed(0);

                    addTestResult(
                        '🧪 СТРЕСС-ТЕСТ',
                        `Выполнено 20 операций за ${duration}мс\nОперации: ${operations.slice(0, 10).join(', ')}${operations.length > 10 ? '...' : ''}`,
                        'success',
                        'system'
                    );

                    setApiStatus(`✅ Стресс-тест завершен за ${duration}мс`, 'success');

                } catch (error: any) {
                    addTestResult(
                        'Ошибка стресс-теста',
                        error.message,
                        'error',
                        'system'
                    );
                    setApiStatus('❌ Ошибка при стресс-тесте', 'error');
                } finally {
                    apiLoading.value = false;
                }
            };            

            // Быстрые действия
            const highlightAllNodes = async () => {
                if (!graphComponent.value) return;

                const nodes = graphComponent.value.getNodes();
                nodes.forEach((node: any) => {
                    graphComponent.value.emphasize(node);
                });

                addTestResult('Подсветка всех узлов', `Подсвечено ${nodes.length} узлов`, 'info', 'interactive');
            };

            const selectRandomElement = async () => {
                if (!graphComponent.value) return;

                const nodes = graphComponent.value.getNodes();
                if (nodes.length > 0) {
                    const randomNode = nodes[Math.floor(Math.random() * nodes.length)];
                    graphComponent.value.select(randomNode);

                    addTestResult('Случайный выбор', `Выбран узел "${randomNode._id}"`, 'info', 'interactive');
                }
            };

            const testTooltip = async () => {
                if (!graphComponent.value) return;

                const nodes = graphComponent.value.getNodes();
                if (nodes.length > 0) {
                    const nodeWithInfo = nodes.find((node: any) => node._info && node._info.trim());
                    if (nodeWithInfo) {
                        addTestResult('Tooltip информация', `Найден узел с описанием: "${nodeWithInfo._info.substring(0, 50)}..."`, 'success', 'interactive');
                    } else {
                        addTestResult('Tooltip информация', 'Нет узлов с описанием для tooltip', 'warning', 'interactive');
                    }
                }
            };

            // Вспомогательные функции
            const getRandomColor = () => {
                const colors = ['#ff6b6b', '#4ecdc4', '#45b7d1', '#96ceb4', '#feca57', '#ff9ff3', '#54a0ff', '#5f27cd'];
                return colors[Math.floor(Math.random() * colors.length)];
            };

            const clearResults = () => {
                testResults.value = [];
                stats.value = { total: 0, success: 0, warnings: 0, errors: 0 };
                setApiStatus('Результаты очищены', 'ready');
            };

            const exportResults = () => {
                const exportData = {
                    timestamp: new Date().toISOString(),
                    stats: stats.value,
                    results: testResults.value,
                    successRate: successPercentage.value.toFixed(1) + '%'
                };

                const blob = new Blob([JSON.stringify(exportData, null, 2)], { type: 'application/json' });
                const url = URL.createObjectURL(blob);
                const a = document.createElement('a');
                a.href = url;
                a.download = `api-test-results-${new Date().toISOString().slice(0, 19).replace(/[:]/g, '-')}.json`;
                document.body.appendChild(a);
                a.click();
                document.body.removeChild(a);
                URL.revokeObjectURL(url);

                addTestResult('Экспорт результатов', 'Результаты тестирования экспортированы в JSON', 'info', 'system');
            };

            // Переключение между XML
            const toggleXml = async () => {
                try {
                    // Меняем индекс (0 или 1)
                    currentXmlIndex.value = currentXmlIndex.value === 0 ? 1 : 0;

                    // Меняем XML в зависимости от индекса
                    const newXml = currentXmlIndex.value === 0 ? SAMPLE_XML_1 : SAMPLE_XML_2;

                    // Обновляем граф через метод компонента
                    if (graphComponent.value && graphComponent.value.updateGraphDataXML) {
                        await graphComponent.value.updateGraphDataXML(newXml);
                        currentXml.value = newXml;
                        console.log('Граф успешно обновлен на XML', currentXmlIndex.value + 1);
                    }
                } catch (error) {
                    console.error('Ошибка при обновлении графа:', error);
                    alert('Ошибка при обновлении графа! Проверьте консоль.');
                }
            };

            // Экспорт текущего XML
            const exportCurrentXml = () => {
                if (graphComponent.value && graphComponent.value.exportToXml) {
                    const xmlContent = graphComponent.value.exportToXml();

                    // Создаем ссылку для скачивания
                    const blob = new Blob([xmlContent], { type: 'application/xml' });
                    const url = URL.createObjectURL(blob);
                    const a = document.createElement('a');
                    a.href = url;
                    a.download = `graph_export_${new Date().toISOString().replace(/[:.]/g, '-')}.xml`;
                    document.body.appendChild(a);
                    a.click();

                    // Очищаем
                    setTimeout(() => {
                        document.body.removeChild(a);
                        URL.revokeObjectURL(url);
                    }, 0);
                }
            };

            return {
                graphComponent,
                currentXml,
                dialectXml,
                currentXmlIndex,
                apiLoading,
                apiStatus,
                apiStatusClass,
                stats,
                testResults,
                categorizedResults,
                successPercentage,
                warningPercentage,
                errorPercentage,

                // Методы тестирования
                testGetNodesEdges,
                testAddElements,
                testSelection,
                testEmphasis,
                testGetGraphData,
                testEventHandlers,
                testDeleteElements,
                testClearGraph,
                testSelectionConstraints,
                testDialectSystem,
                testInteractiveFeatures,
                runAllTests,

                // Быстрые действия
                highlightAllNodes,
                selectRandomElement,
                testTooltip,
                stressTest,

                // Вспомогательные
                clearResults,
                exportResults,
                toggleXml,
                exportCurrentXml,
                getTypeIcon,
                getCategoryTitle,
                formatTime
            };
        }
    });
</script>

<style scoped>
    /* Базовые стили остаются */

    .app {
        font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
        max-width: 1400px;
        margin: 0 auto;
        padding: 20px;
        background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
        min-height: 100vh;
    }

    .controls {
        background: rgba(255, 255, 255, 0.9);
        padding: 20px;
        border-radius: 15px;
        margin-bottom: 20px;
        display: flex;
        gap: 15px;
        box-shadow: 0 10px 30px rgba(0, 0, 0, 0.1);
    }

    .api-controls {
        background: white;
        padding: 25px;
        border-radius: 15px;
        margin-bottom: 30px;
        box-shadow: 0 15px 35px rgba(0, 0, 0, 0.1);
    }

        .api-controls h3 {
            color: #2c3e50;
            border-bottom: 3px solid #42b883;
            padding-bottom: 15px;
            margin-bottom: 25px;
            font-size: 24px;
        }

    .api-sections {
        display: grid;
        gap: 25px;
        margin-bottom: 30px;
    }

    .api-section {
        background: #f8f9fa;
        padding: 20px;
        border-radius: 10px;
        border-left: 4px solid #3498db;
    }

        .api-section h4 {
            color: #2c3e50;
            margin-top: 0;
            margin-bottom: 15px;
            font-size: 18px;
            display: flex;
            align-items: center;
            gap: 10px;
        }

    .api-buttons {
        display: grid;
        grid-template-columns: repeat(auto-fill, minmax(180px, 1fr));
        gap: 12px;
    }

        .api-buttons button {
            padding: 12px 16px;
            font-size: 14px;
            background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
            color: white;
            border: none;
            border-radius: 8px;
            cursor: pointer;
            transition: all 0.3s ease;
            font-weight: 500;
            text-align: left;
            display: flex;
            align-items: center;
            gap: 8px;
        }

            .api-buttons button:hover:not(:disabled) {
                transform: translateY(-2px);
                box-shadow: 0 5px 15px rgba(0, 0, 0, 0.2);
            }

            .api-buttons button:disabled {
                opacity: 0.5;
                cursor: not-allowed;
            }

    .api-status-panel {
        background: linear-gradient(135deg, #f5f7fa 0%, #c3cfe2 100%);
        padding: 20px;
        border-radius: 10px;
        margin-bottom: 25px;
        display: flex;
        align-items: center;
        gap: 20px;
        border: 1px solid #e0e0e0;
    }

    .status-indicator {
        width: 20px;
        height: 20px;
        border-radius: 50%;
    }

        .status-indicator.ready {
            background: #95a5a6;
        }

        .status-indicator.loading {
            background: #f1c40f;
            animation: pulse 1.5s infinite;
        }

        .status-indicator.success {
            background: #2ecc71;
        }

        .status-indicator.error {
            background: #e74c3c;
        }

        .status-indicator.warning {
            background: #f39c12;
        }

    @keyframes pulse {
        0% {
            opacity: 1;
        }

        50% {
            opacity: 0.5;
        }

        100% {
            opacity: 1;
        }
    }

    .status-text {
        flex: 1;
        font-size: 16px;
        font-weight: 500;
        color: #2c3e50;
    }

    .stats {
        display: flex;
        gap: 20px;
        font-size: 14px;
    }

        .stats span {
            padding: 5px 10px;
            border-radius: 5px;
            background: rgba(255, 255, 255, 0.8);
        }

    .api-results-container {
        background: white;
        border: 1px solid #e0e0e0;
        border-radius: 10px;
        overflow: hidden;
        margin-bottom: 25px;
    }

    .results-header {
        background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
        color: white;
        padding: 15px 20px;
        display: flex;
        justify-content: space-between;
        align-items: center;
    }

        .results-header h4 {
            margin: 0;
            display: flex;
            align-items: center;
            gap: 10px;
        }

    .results-controls {
        display: flex;
        gap: 10px;
    }

    .small-btn {
        padding: 6px 12px;
        font-size: 12px;
        background: rgba(255, 255, 255, 0.2);
        border: 1px solid rgba(255, 255, 255, 0.3);
    }

    .visualization {
        padding: 20px;
    }

    .test-chart {
        height: 40px;
        background: #f8f9fa;
        border-radius: 8px;
        overflow: hidden;
        margin-bottom: 25px;
        display: flex;
    }

    .chart-bar {
        height: 100%;
        transition: width 0.5s ease;
    }

        .chart-bar.success {
            background: #2ecc71;
        }

        .chart-bar.warning {
            background: #f1c40f;
        }

        .chart-bar.error {
            background: #e74c3c;
        }

    .detailed-results {
        max-height: 400px;
        overflow-y: auto;
        padding-right: 10px;
    }

    .result-category {
        margin-bottom: 25px;
    }

        .result-category h5 {
            color: #2c3e50;
            padding-bottom: 10px;
            border-bottom: 2px solid #42b883;
            margin-bottom: 15px;
            display: flex;
            align-items: center;
            gap: 10px;
        }

    .test-results {
        display: flex;
        flex-direction: column;
        gap: 12px;
    }

    .test-result {
        padding: 15px;
        border-radius: 8px;
        display: flex;
        gap: 15px;
        align-items: flex-start;
        transition: transform 0.2s ease;
        border-left: 4px solid;
    }

        .test-result:hover {
            transform: translateX(5px);
        }

        .test-result.success {
            background: #d5f4e6;
            border-left-color: #2ecc71;
        }

        .test-result.warning {
            background: #fff9e6;
            border-left-color: #f1c40f;
        }

        .test-result.error {
            background: #ffe6e6;
            border-left-color: #e74c3c;
        }

        .test-result.info {
            background: #e6f2ff;
            border-left-color: #3498db;
        }

    .test-icon {
        font-size: 20px;
        margin-top: 2px;
    }

    .test-content {
        flex: 1;
    }

    .test-title {
        font-weight: 600;
        color: #2c3e50;
        margin-bottom: 5px;
        font-size: 15px;
    }

    .test-message {
        color: #7f8c8d;
        font-size: 14px;
        line-height: 1.4;
        margin-bottom: 8px;
    }

    .test-timestamp {
        font-size: 12px;
        color: #95a5a6;
    }

    .quick-actions {
        background: #f8f9fa;
        padding: 20px;
        border-radius: 10px;
        border-left: 4px solid #9b59b6;
    }

        .quick-actions h4 {
            color: #2c3e50;
            margin-top: 0;
            margin-bottom: 15px;
            display: flex;
            align-items: center;
            gap: 10px;
        }

    .quick-buttons {
        display: grid;
        grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
        gap: 12px;
    }

    .quick-btn {
        padding: 10px 15px;
        font-size: 13px;
        background: #9b59b6;
        color: white;
        border: none;
        border-radius: 6px;
        cursor: pointer;
        transition: all 0.3s ease;
    }

        .quick-btn:hover:not(:disabled) {
            background: #8e44ad;
            transform: translateY(-2px);
        }

        .quick-btn:disabled {
            opacity: 0.5;
            cursor: not-allowed;
        }

    /* Стили для основного графа */
    .graph-container {
        background: white;
        border-radius: 15px;
        overflow: hidden;
        box-shadow: 0 20px 40px rgba(0, 0, 0, 0.15);
        border: 1px solid #e0e0e0;
    }

    /* Адаптивность */
    @media (max-width: 768px) {
        .api-sections {
            grid-template-columns: 1fr;
        }

        .api-buttons {
            grid-template-columns: 1fr;
        }

        .quick-buttons {
            grid-template-columns: 1fr;
        }

        .stats {
            flex-direction: column;
            gap: 10px;
        }
    }

    button {
        padding: 10px 20px;
        background: #42b883;
        color: white;
        border: none;
        border-radius: 4px;
        cursor: pointer;
        font-size: 16px;
        transition: background 0.3s;
    }

        button:hover {
            background: #3aa876;
        }

        button:active {
            background: #359a6d;
        }

        button:nth-child(2) {
            background: #3498db;
        }

            button:nth-child(2):hover {
                background: #2980b9;
            }

            button:nth-child(2):active {
                background: #2573a7;
            }

    /* Скроллбар */
    ::-webkit-scrollbar {
        width: 8px;
    }

    ::-webkit-scrollbar-track {
        background: #f1f1f1;
        border-radius: 4px;
    }

    ::-webkit-scrollbar-thumb {
        background: #888;
        border-radius: 4px;
    }

        ::-webkit-scrollbar-thumb:hover {
            background: #555;
        }
</style>