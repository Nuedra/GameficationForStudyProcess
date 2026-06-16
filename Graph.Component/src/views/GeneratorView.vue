<template>
    <div class="generator-view">
        <div class="container">
            <section class="hero">
                <h2>Генератор графов</h2>
                <p>Создание и редактирование XML с интерактивным предпросмотром</p>
            </section>

            <div class="generator-layout">
                <!-- Левая панель - редактор XML -->
                <div class="editor-panel">
                    <div class="tabs">
                        <button @click="activeTab = 'graph'"
                                class="tab-btn"
                                :class="{ active: activeTab === 'graph' }">
                            Граф XML
                        </button>
                        <button @click="activeTab = 'dialect'"
                                class="tab-btn"
                                :class="{ active: activeTab === 'dialect' }">
                            Диалект XML
                        </button>
                    </div>

                    <div v-if="activeTab === 'graph'" class="editor-content">
                        <div class="panel-header">
                            <h3>Редактор XML графа</h3>
                            <div class="editor-controls">
                                <button @click="validateXML" class="control-btn" :disabled="!xmlContent">
                                    ✅ Валидация
                                </button>
                                <button @click="formatXML" class="control-btn" :disabled="!xmlContent">
                                    ✨ Форматировать
                                </button>
                                <button @click="loadExample" class="control-btn">
                                    📋 Пример
                                </button>
                                <button @click="clearEditor" class="control-btn">
                                    🗑️ Очистить
                                </button>
                            </div>
                        </div>

                        <div class="editor-container">
                            <textarea ref="xmlEditor"
                                      v-model="xmlContent"
                                      placeholder="Введите XML графа здесь..."
                                      @input="handleXMLChange"
                                      class="xml-editor"
                                      spellcheck="false"></textarea>

                            <div class="editor-info">
                                <span class="char-count">Символов: {{ charCount }}</span>
                                <span class="line-count">Строк: {{ lineCount }}</span>
                            </div>
                        </div>

                        <div class="editor-status" :class="validationStatus">
                            {{ validationMessage }}
                        </div>

                        <!-- Настройки канваса -->
                        <div class="canvas-settings">
                            <h4>Настройки канваса:</h4>
                            <div class="canvas-controls">
                                <div class="canvas-control-group">
                                    <label for="canvas-width">Ширина:</label>
                                    <div class="input-with-buttons">
                                        <input type="number"
                                               id="canvas-width"
                                               v-model.number="canvasWidth"
                                               min="100"
                                               max="2000"
                                               step="10">
                                        <div class="value-buttons">
                                            <button @click="adjustCanvasWidth(-10)" class="value-btn">-</button>
                                            <button @click="adjustCanvasWidth(10)" class="value-btn">+</button>
                                        </div>
                                    </div>
                                    <span class="unit">px</span>
                                </div>

                                <div class="canvas-control-group">
                                    <label for="canvas-height">Высота:</label>
                                    <div class="input-with-buttons">
                                        <input type="number"
                                               id="canvas-height"
                                               v-model.number="canvasHeight"
                                               min="100"
                                               max="2000"
                                               step="10">
                                        <div class="value-buttons">
                                            <button @click="adjustCanvasHeight(-10)" class="value-btn">-</button>
                                            <button @click="adjustCanvasHeight(10)" class="value-btn">+</button>
                                        </div>
                                    </div>
                                    <span class="unit">px</span>
                                </div>

                                <div class="canvas-control-group">
                                    <label for="canvas-scale">Масштаб:</label>
                                    <div class="scale-control">
                                        <input type="range"
                                               id="canvas-scale"
                                               v-model.number="canvasScale"
                                               min="10"
                                               max="200"
                                               step="10">
                                        <span class="scale-value">{{ canvasScale }}%</span>
                                    </div>
                                </div>

                                <div class="canvas-presets">
                                    <button @click="applyCanvasPreset('small')" class="preset-btn">640×480</button>
                                    <button @click="applyCanvasPreset('medium')" class="preset-btn active">800×600</button>
                                    <button @click="applyCanvasPreset('large')" class="preset-btn">1024×768</button>
                                    <button @click="applyCanvasPreset('wide')" class="preset-btn">1200×800</button>
                                    <button @click="updateCanvasInXML" class="preset-btn apply-btn">Применить</button>
                                </div>
                            </div>
                        </div>

                        <div class="template-buttons">
                            <h4>Быстрые шаблоны графов:</h4>
                            <div class="template-grid">
                                <button v-for="template in templates"
                                        :key="template.name"
                                        @click="loadTemplate(template)"
                                        class="template-btn">
                                    <span class="template-icon">{{ template.icon }}</span>
                                    <span class="template-name">{{ template.name }}</span>
                                </button>
                            </div>
                        </div>
                    </div>

                    <div v-else class="editor-content">
                        <div class="panel-header">
                            <h3>Редактор диалектов</h3>
                            <div class="editor-controls">
                                <button @click="validateDialectXML" class="control-btn" :disabled="!dialectXMLContent">
                                    ✅ Проверить
                                </button>
                                <button @click="formatDialectXML" class="control-btn" :disabled="!dialectXMLContent">
                                    ✨ Форматировать
                                </button>
                                <button @click="loadDefaultDialect" class="control-btn">
                                    📋 По умолчанию
                                </button>
                                <button @click="clearDialectEditor" class="control-btn">
                                    🗑️ Очистить
                                </button>
                            </div>
                        </div>

                        <div class="editor-container">
                            <textarea ref="dialectEditor"
                                      v-model="dialectXMLContent"
                                      placeholder="Введите XML диалекта здесь..."
                                      @input="handleDialectInput"
                                      class="xml-editor"
                                      spellcheck="false"></textarea>

                            <div class="editor-info">
                                <span class="char-count">Символов: {{ dialectCharCount }}</span>
                                <span class="line-count">Строк: {{ dialectLineCount }}</span>
                            </div>
                        </div>

                        <div class="editor-status" :class="dialectValidationStatus">
                            {{ dialectValidationMessage }}
                        </div>

                        <div class="template-buttons">
                            <h4>Готовые диалекты:</h4>
                            <div class="template-grid">
                                <button v-for="dialect in dialectTemplates"
                                        :key="dialect.name"
                                        @click="loadDialectTemplate(dialect)"
                                        class="template-btn">
                                    <span class="template-icon">{{ dialect.icon }}</span>
                                    <span class="template-name">{{ dialect.name }}</span>
                                </button>
                            </div>
                        </div>
                    </div>
                </div>

                <!-- Правая панель - предпросмотр графа -->
                <div class="preview-panel">
                    <div class="panel-header">
                        <h3>Предпросмотр графа</h3>
                        <div class="preview-controls">
                            <button @click="refreshPreview" class="control-btn" :disabled="!xmlContent">
                                🔄 Обновить
                            </button>
                            <button @click="toggleDialectPreview" class="control-btn" :class="{ active: showDialectPreview }">
                                {{ showDialectPreview ? 'Диалект вкл' : 'Диалект выкл' }}
                            </button>
                            <button @click="downloadXML" class="control-btn" :disabled="!xmlContent">
                                ⬇️ Скачать XML
                            </button>
                            <button @click="downloadDialectXML" class="control-btn" :disabled="!dialectXMLContent">
                                ⬇️ Диалект
                            </button>
                            <button @click="toggleFitToScreen" class="control-btn" :class="{ active: fitToScreen }">
                                {{ fitToScreen ? 'Вписать' : 'Оригинал' }}
                            </button>
                        </div>
                    </div>

                    <div class="preview-container">
                        <!-- Управление масштабом предпросмотра -->
                        <div class="preview-controls-overlay" v-if="showGraph && validXML">
                            <div class="zoom-controls">
                                <button @click="zoomOut" class="zoom-btn" title="Уменьшить">
                                    <span class="zoom-icon">🔍➖</span>
                                </button>
                                <span class="zoom-level">{{ previewScale }}%</span>
                                <button @click="zoomIn" class="zoom-btn" title="Увеличить">
                                    <span class="zoom-icon">🔍➕</span>
                                </button>
                                <button @click="resetZoom" class="zoom-btn reset" title="Сбросить масштаб">
                                    <span class="zoom-icon">🔍↺</span>
                                </button>
                                <button @click="centerGraph" class="zoom-btn" title="Центрировать">
                                    <span class="zoom-icon">🎯</span>
                                </button>
                            </div>
                        </div>

                        <!-- Граф компонент -->
                        <div v-if="showGraph && validXML" class="graph-wrapper" :style="previewStyle">
                            <GraphComponent :xmlData="xmlContent"
                                            :dialectData="showDialectPreview && validDialectXML ? dialectXMLContent : undefined"
                                            :width="actualCanvasWidth"
                                            :height="actualCanvasHeight"
                                            ref="graphComponent" />
                        </div>

                        <!-- Сообщение об ошибке -->
                        <div v-else-if="!validXML && xmlContent" class="error-message">
                            <div class="error-icon">❌</div>
                            <h4>Ошибка в XML графа</h4>
                            <p>{{ validationMessage }}</p>
                            <button @click="showSampleXML" class="btn-fix">
                                Показать пример правильного XML
                            </button>
                        </div>

                        <!-- Приветственное сообщение -->
                        <div v-else class="welcome-message">
                            <div class="welcome-icon">📊</div>
                            <h4>Добро пожаловать в генератор графов!</h4>
                            <p>Введите XML в редакторе слева, чтобы увидеть граф здесь</p>
                            <p>Или выберите один из готовых шаблонов</p>
                        </div>
                    </div>

                    <!-- Информация о графе -->
                    <div v-if="showGraph && validXML" class="graph-info">
                        <div class="info-grid">
                            <div class="info-item">
                                <span class="info-label">Узлов:</span>
                                <span class="info-value">{{ nodeCount }}</span>
                            </div>
                            <div class="info-item">
                                <span class="info-label">Связей:</span>
                                <span class="info-value">{{ edgeCount }}</span>
                            </div>
                            <div class="info-item">
                                <span class="info-label">Типы узлов:</span>
                                <span class="info-value">{{ nodeTypes.length }}</span>
                            </div>
                            <div class="info-item">
                                <span class="info-label">Канвас:</span>
                                <span class="info-value">{{ actualCanvasWidth }}×{{ actualCanvasHeight }}</span>
                            </div>
                            <div class="info-item">
                                <span class="info-label">Масштаб:</span>
                                <span class="info-value">{{ previewScale }}%</span>
                            </div>
                            <div class="info-item">
                                <span class="info-label">Просмотр:</span>
                                <span class="info-value">{{ fitToScreen ? 'Вписано' : 'Оригинал' }}</span>
                            </div>
                        </div>
                    </div>

                    <!-- Информация о диалекте -->
                    <div v-if="showDialectPreview && validDialectXML" class="dialect-info">
                        <h4>Активный диалект:</h4>
                        <div class="info-grid">
                            <div class="info-item">
                                <span class="info-label">Имя:</span>
                                <span class="info-value">{{ activeDialectName }}</span>
                            </div>
                            <div class="info-item">
                                <span class="info-label">Узлы:</span>
                                <span class="info-value">{{ dialectNodeTypes }}</span>
                            </div>
                            <div class="info-item">
                                <span class="info-label">Связи:</span>
                                <span class="info-value">{{ dialectEdgeTypes }}</span>
                            </div>
                            <div class="info-item">
                                <span class="info-label">Состояние:</span>
                                <span class="info-value status-active">Активен</span>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <!-- Быстрые действия -->
            <div class="quick-actions">
                <h3>Быстрые действия</h3>
                <div class="actions-grid">
                    <button @click="generateRandomGraph" class="action-btn">
                        <span class="action-icon">🎲</span>
                        <span class="action-text">Случайный граф</span>
                    </button>
                    <button @click="exportAsSVG" class="action-btn">
                        <span class="action-icon">🖼️</span>
                        <span class="action-text">Экспорт SVG</span>
                    </button>
                    <button @click="copyToClipboard" class="action-btn">
                        <span class="action-icon">📋</span>
                        <span class="action-text">Копировать XML</span>
                    </button>
                    <button @click="shareGraph" class="action-btn">
                        <span class="action-icon">📤</span>
                        <span class="action-text">Поделиться</span>
                    </button>
                </div>
            </div>
        </div>
    </div>
</template>

<script lang="ts">
    import { defineComponent, ref, computed, nextTick, watch } from 'vue'
    import GraphComponent from '@/components/GraphComponent.vue'
    import { useGraphData } from '@/composables/useGraphData'

    interface Template {
        name: string
        icon: string
        xml: string
        description: string
    }

    interface DialectTemplate {
        name: string
        icon: string
        xml: string
        description: string
    }

    export default defineComponent({
        name: 'GeneratorView',
        components: {
            GraphComponent
        },
        setup() {
            // Используем композабл для получения данных
            const { dialectXML: baseDialectXML } = useGraphData()

            // Рефы
            const xmlEditor = ref<HTMLTextAreaElement | null>(null)
            const dialectEditor = ref<HTMLTextAreaElement | null>(null)
            const graphComponent = ref<InstanceType<typeof GraphComponent> | null>(null)

            // Активная вкладка
            const activeTab = ref<'graph' | 'dialect'>('graph')

            // Состояние редактора графа
            const xmlContent = ref('')
            const validationStatus = ref<'idle' | 'valid' | 'error'>('idle')
            const validationMessage = ref('Введите XML для начала работы')
            const validXML = ref(false)
            const showGraph = ref(false)

            // Состояние редактора диалекта
            const dialectXMLContent = ref('')
            const dialectValidationStatus = ref<'idle' | 'valid' | 'error'>('idle')
            const dialectValidationMessage = ref('Введите XML диалекта или выберите готовый')
            const validDialectXML = ref(false)

            // Состояние предпросмотра
            const showDialectPreview = ref(false)
            const previewWidth = ref(800)
            const previewHeight = ref(500)
            const previewScale = ref(100)
            const fitToScreen = ref(true)

            // Настройки канваса
            const canvasWidth = ref(800)
            const canvasHeight = ref(600)
            const canvasScale = ref(100) // Масштаб для изменения всех узлов

            // Шаблоны канваса
            const canvasPresets = {
                small: { width: 640, height: 480 },
                medium: { width: 800, height: 600 },
                large: { width: 1024, height: 768 },
                wide: { width: 1200, height: 800 }
            }

            // Статистика графа
            const charCount = computed(() => xmlContent.value.length)
            const lineCount = computed(() => xmlContent.value.split('\n').length)

            // Статистика диалекта
            const dialectCharCount = computed(() => dialectXMLContent.value.length)
            const dialectLineCount = computed(() => dialectXMLContent.value.split('\n').length)

            // Вычисляемые свойства для канваса
            const actualCanvasWidth = computed(() => Math.floor(canvasWidth.value * (canvasScale.value / 100)))
            const actualCanvasHeight = computed(() => Math.floor(canvasHeight.value * (canvasScale.value / 100)))

            // Стиль предпросмотра
            const previewStyle = computed(() => {
                if (fitToScreen.value) {
                    return {
                        transform: `scale(${previewScale.value / 100})`,
                        transformOrigin: 'center'
                    }
                }
                return {}
            })

            // Шаблоны XML графов
            const templates = ref<Template[]>([
                {
                    name: 'Пустой граф',
                    icon: '⚪',
                    xml: `<graph>
  <canvas width="800" height="600"/>
</graph>`,
                    description: 'Начните с пустого графа'
                },
                {
                    name: 'Простой граф',
                    icon: '⭕',
                    xml: `<graph>
  <canvas width="800" height="600"/>

  <node id="node1" type="circle" label="Node 1">
    <geometry x="200" y="200" radius="30"/>
    <background color="#1976d2"/>
  </node>

  <node id="node2" type="circle" label="Node 2">
    <geometry x="400" y="200" radius="30"/>
    <background color="#4caf50"/>
  </node>

  <node id="node3" type="circle" label="Node 3">
    <geometry x="300" y="350" radius="30"/>
    <background color="#ff9800"/>
  </node>

  <edge id="edge1" type="line">
    <geometry startX="200" startY="200" endX="400" endY="200"/>
    <background color="#666"/>
  </edge>

  <edge id="edge2" type="line">
    <geometry startX="400" startY="200" endX="300" endY="350"/>
    <background color="#666"/>
  </edge>

  <edge id="edge3" type="line">
    <geometry startX="300" startY="350" endX="200" endY="200"/>
    <background color="#666"/>
  </edge>
</graph>`,
                    description: 'Треугольник из кругов'
                },
                {
                    name: 'Разнообразие форм',
                    icon: '🔶',
                    xml: `<graph>
  <canvas width="800" height="600"/>

  <node id="circle" type="circle" label="Circle">
    <geometry x="150" y="150" radius="40"/>
    <background color="#1976d2"/>
  </node>

  <node id="rect" type="rectangle" label="Rectangle">
    <geometry x="350" y="150" width="80" height="60"/>
    <background color="#4caf50"/>
  </node>

  <node id="triangle" type="triangle" label="Triangle">
    <geometry x1="550" y1="100" x2="600" y2="200" x3="500" y3="200"/>
    <background color="#ff9800"/>
  </node>

  <node id="ellipse" type="ellipse" label="Ellipse">
    <geometry x="150" y="350" radius_x="50" radius_y="30"/>
    <background color="#9c27b0"/>
  </node>

  <node id="polygon" type="regular polygon" label="Pentagon">
    <geometry x="350" y="350" radius="40" number_of_edges="5"/>
    <background color="#00bcd4"/>
  </node>

  <node id="star" type="star" label="Star">
    <geometry x="550" y="350" radius="35"/>
    <background color="#ff5722"/>
  </node>
</graph>`,
                    description: 'Различные геометрические фигуры'
                },
                {
                    name: 'С диалектом',
                    icon: '🎭',
                    xml: `<graph dialect="oriented">
  <canvas width="800" height="600"/>

  <node id="start" type="rectangle" label="Начало">
    <geometry x="200" y="200" width="120" height="60"/>
    <background color="#4caf50"/>
  </node>

  <node id="process" type="rectangle" label="Процесс">
    <geometry x="400" y="200" width="120" height="60"/>
    <background color="#2196f3"/>
  </node>

  <node id="end" type="rectangle" label="Конец">
    <geometry x="600" y="200" width="120" height="60"/>
    <background color="#f44336"/>
  </node>

  <edge id="edge1" type="line" startArrow="stick" endArrow="triangle">
    <geometry startX="320" startY="230" endX="400" endY="230"/>
    <background color="#333"/>
  </edge>

  <edge id="edge2" type="line" startArrow="stick" endArrow="triangle">
    <geometry startX="520" startY="230" endX="600" endY="230"/>
    <background color="#333"/>
  </edge>
</graph>`,
                    description: 'Ориентированный граф с диалектом'
                }
            ])

            // Шаблоны диалектов
            const dialectTemplates = ref<DialectTemplate[]>([
                {
                    name: 'Ориентированный',
                    icon: '➡️',
                    xml: baseDialectXML.value,
                    description: 'Ориентированные графы со стрелками'
                },
                {
                    name: 'Простой',
                    icon: '⭕',
                    xml: `<dialect name="simple">
  <Nodes>
    <allowedNode type="circle" />
    <allowedNode type="rectangle" />
    <allowedNode type="triangle" />
  </Nodes>
  <Edges>
    <allowedEdge type="line" />
  </Edges>
  <Arrowheads>
    <allowedArrowhead type="none" />
  </Arrowheads>
</dialect>`,
                    description: 'Простые неориентированные графы'
                },
                {
                    name: 'Цветной',
                    icon: '🎨',
                    xml: `<dialect name="colored">
  <Nodes>
    <allowedNode type="circle" />
    <allowedNode type="rectangle" />
    <allowedNode type="ellipse" />
  </Nodes>
  <Edges>
    <allowedEdge type="line" />
  </Edges>
  <graphSettings>
    <nodeSettings requireColor="true" />
  </graphSettings>
</dialect>`,
                    description: 'Графы с обязательными цветами'
                },
                {
                    name: 'Бизнес',
                    icon: '💼',
                    xml: `<dialect name="business">
  <Nodes>
    <allowedNode type="rectangle" />
    <allowedNode type="circle" />
    <allowedNode type="ellipse" />
  </Nodes>
  <Edges>
    <allowedEdge type="line" />
    <allowedEdge type="dashed" />
  </Edges>
  <Arrowheads>
    <allowedArrowhead type="triangle" />
    <allowedArrowhead type="diamond" />
  </Arrowheads>
  <graphSettings>
    <labelSettings required="true" />
  </graphSettings>
</dialect>`,
                    description: 'Диаграммы для бизнес-процессов'
                }
            ])

            // Вычисляемые свойства графа
            const nodeCount = computed(() => {
                if (!validXML.value) return 0
                const matches = xmlContent.value.match(/<node/g)
                return matches ? matches.length : 0
            })

            const edgeCount = computed(() => {
                if (!validXML.value) return 0
                const matches = xmlContent.value.match(/<edge/g)
                return matches ? matches.length : 0
            })

            const nodeTypes = computed(() => {
                if (!validXML.value) return []
                const typeMatches = xmlContent.value.match(/type="([^"]+)"/g) || []
                const types = new Set(
                    typeMatches.map(match => match.replace('type="', '').replace('"', ''))
                )
                return Array.from(types)
            })

            // Вычисляемые свойства диалекта
            const activeDialectName = computed(() => {
                if (!validDialectXML.value) return 'Нет'
                const match = dialectXMLContent.value.match(/name="([^"]+)"/)
                return match ? match[1] : 'Без имени'
            })

            const dialectNodeTypes = computed(() => {
                if (!validDialectXML.value) return 0
                const matches = dialectXMLContent.value.match(/<allowedNode/g)
                return matches ? matches.length : 0
            })

            const dialectEdgeTypes = computed(() => {
                if (!validDialectXML.value) return 0
                const matches = dialectXMLContent.value.match(/<allowedEdge/g)
                return matches ? matches.length : 0
            })

            // Методы для графа
            const handleXMLChange = () => {
                clearTimeout(window.validationTimeout)
                window.validationTimeout = setTimeout(validateXML, 500)
            }

            const validateXML = () => {
                if (!xmlContent.value.trim()) {
                    validationStatus.value = 'idle'
                    validationMessage.value = 'Введите XML для начала работы'
                    validXML.value = false
                    showGraph.value = false
                    return
                }

                try {
                    const parser = new DOMParser()
                    const xmlDoc = parser.parseFromString(xmlContent.value, 'text/xml')

                    const parserError = xmlDoc.getElementsByTagName('parsererror')[0]
                    if (parserError) {
                        throw new Error(parserError.textContent || 'Ошибка парсинга XML')
                    }

                    const graphElements = xmlDoc.getElementsByTagName('graph')
                    if (graphElements.length === 0) {
                        throw new Error('Отсутствует корневой элемент <graph>')
                    }

                    // Извлекаем размеры канваса из XML
                    const canvasElements = xmlDoc.getElementsByTagName('canvas')
                    if (canvasElements.length > 0) {
                        const canvas = canvasElements[0]
                        const width = canvas.getAttribute('width')
                        const height = canvas.getAttribute('height')

                        if (width) canvasWidth.value = parseInt(width)
                        if (height) canvasHeight.value = parseInt(height)
                    }

                    validationStatus.value = 'valid'
                    validationMessage.value = '✓ XML графа валиден'
                    validXML.value = true
                    showGraph.value = true

                    nextTick(() => {
                        if (graphComponent.value) {
                            graphComponent.value.updateGraphDataXML(xmlContent.value)
                        }
                    })

                } catch (error) {
                    validationStatus.value = 'error'
                    validationMessage.value = error instanceof Error ? error.message : 'Неизвестная ошибка'
                    validXML.value = false
                    showGraph.value = false
                }
            }

            const formatXML = () => {
                try {
                    const parser = new DOMParser()
                    const xmlDoc = parser.parseFromString(xmlContent.value, 'text/xml')
                    const serializer = new XMLSerializer()
                    const formatted = serializer.serializeToString(xmlDoc)

                    const formattedWithIndent = formatted
                        .replace(/>\s+</g, '>\n<')
                        .replace(/<([^>]+)>/g, match => {
                            if (match.startsWith('</')) return `  ${match}`
                            return match
                        })

                    xmlContent.value = formattedWithIndent
                    validationStatus.value = 'valid'
                    validationMessage.value = '✓ XML графа отформатирован'

                } catch (error) {
                    validationMessage.value = 'Ошибка форматирования XML графа'
                }
            }

            // Методы для диалекта
            const handleDialectInput = () => {
                clearTimeout(window.dialectValidationTimeout)
                window.dialectValidationTimeout = setTimeout(validateDialectXML, 500)
            }

            const validateDialectXML = () => {
                if (!dialectXMLContent.value.trim()) {
                    dialectValidationStatus.value = 'idle'
                    dialectValidationMessage.value = 'Введите XML диалекта или выберите готовый'
                    validDialectXML.value = false
                    return
                }

                try {
                    const parser = new DOMParser()
                    const xmlDoc = parser.parseFromString(dialectXMLContent.value, 'text/xml')

                    const parserError = xmlDoc.getElementsByTagName('parsererror')[0]
                    if (parserError) {
                        throw new Error(parserError.textContent || 'Ошибка парсинга XML диалекта')
                    }

                    const dialectElements = xmlDoc.getElementsByTagName('dialect')
                    if (dialectElements.length === 0) {
                        throw new Error('Отсутствует корневой элемент <dialect>')
                    }

                    dialectValidationStatus.value = 'valid'
                    dialectValidationMessage.value = '✓ XML диалекта валиден'
                    validDialectXML.value = true

                } catch (error) {
                    dialectValidationStatus.value = 'error'
                    dialectValidationMessage.value = error instanceof Error ? error.message : 'Неизвестная ошибка'
                    validDialectXML.value = false
                }
            }

            const formatDialectXML = () => {
                try {
                    const parser = new DOMParser()
                    const xmlDoc = parser.parseFromString(dialectXMLContent.value, 'text/xml')
                    const serializer = new XMLSerializer()
                    const formatted = serializer.serializeToString(xmlDoc)

                    const formattedWithIndent = formatted
                        .replace(/>\s+</g, '>\n<')
                        .replace(/<([^>]+)>/g, match => {
                            if (match.startsWith('</')) return `  ${match}`
                            return match
                        })

                    dialectXMLContent.value = formattedWithIndent
                    dialectValidationStatus.value = 'valid'
                    dialectValidationMessage.value = '✓ XML диалекта отформатирован'

                } catch (error) {
                    dialectValidationMessage.value = 'Ошибка форматирования XML диалекта'
                }
            }

            // Методы для работы с канвасом
            const adjustCanvasWidth = (delta: number) => {
                canvasWidth.value = Math.max(100, Math.min(2000, canvasWidth.value + delta))
            }

            const adjustCanvasHeight = (delta: number) => {
                canvasHeight.value = Math.max(100, Math.min(2000, canvasHeight.value + delta))
            }

            const applyCanvasPreset = (preset: keyof typeof canvasPresets) => {
                const { width, height } = canvasPresets[preset]
                canvasWidth.value = width
                canvasHeight.value = height
            }

            const updateCanvasInXML = () => {
                if (!xmlContent.value) return

                try {
                    const parser = new DOMParser()
                    const xmlDoc = parser.parseFromString(xmlContent.value, 'text/xml')

                    // Обновляем или добавляем canvas элемент
                    let canvasElements = xmlDoc.getElementsByTagName('canvas')
                    let canvas: Element

                    if (canvasElements.length > 0) {
                        canvas = canvasElements[0]
                    } else {
                        // Создаем новый canvas элемент
                        canvas = xmlDoc.createElement('canvas')
                        const graphElements = xmlDoc.getElementsByTagName('graph')
                        if (graphElements.length > 0) {
                            const graph = graphElements[0]
                            graph.insertBefore(canvas, graph.firstChild)
                        }
                    }

                    // Устанавливаем новые размеры
                    canvas.setAttribute('width', canvasWidth.value.toString())
                    canvas.setAttribute('height', canvasHeight.value.toString())

                    // Применяем масштаб ко всем узлам
                    if (canvasScale.value !== 100) {
                        const scaleFactor = canvasScale.value / 100

                        // Масштабируем все узлы
                        const nodes = xmlDoc.getElementsByTagName('node')
                        for (let i = 0; i < nodes.length; i++) {
                            const node = nodes[i]
                            const geometry = node.getElementsByTagName('geometry')[0]

                            if (geometry) {
                                // Масштабируем координаты
                                const x = geometry.getAttribute('x')
                                const y = geometry.getAttribute('y')
                                const radius = geometry.getAttribute('radius')
                                const width = geometry.getAttribute('width')
                                const height = geometry.getAttribute('height')
                                const radius_x = geometry.getAttribute('radius_x')
                                const radius_y = geometry.getAttribute('radius_y')

                                if (x) geometry.setAttribute('x', (parseFloat(x) * scaleFactor).toString())
                                if (y) geometry.setAttribute('y', (parseFloat(y) * scaleFactor).toString())
                                if (radius) geometry.setAttribute('radius', (parseFloat(radius) * scaleFactor).toString())
                                if (width) geometry.setAttribute('width', (parseFloat(width) * scaleFactor).toString())
                                if (height) geometry.setAttribute('height', (parseFloat(height) * scaleFactor).toString())
                                if (radius_x) geometry.setAttribute('radius_x', (parseFloat(radius_x) * scaleFactor).toString())
                                if (radius_y) geometry.setAttribute('radius_y', (parseFloat(radius_y) * scaleFactor).toString())

                                // Масштабируем координаты для треугольников
                                const x1 = geometry.getAttribute('x1')
                                const y1 = geometry.getAttribute('y1')
                                const x2 = geometry.getAttribute('x2')
                                const y2 = geometry.getAttribute('y2')
                                const x3 = geometry.getAttribute('x3')
                                const y3 = geometry.getAttribute('y3')

                                if (x1) geometry.setAttribute('x1', (parseFloat(x1) * scaleFactor).toString())
                                if (y1) geometry.setAttribute('y1', (parseFloat(y1) * scaleFactor).toString())
                                if (x2) geometry.setAttribute('x2', (parseFloat(x2) * scaleFactor).toString())
                                if (y2) geometry.setAttribute('y2', (parseFloat(y2) * scaleFactor).toString())
                                if (x3) geometry.setAttribute('x3', (parseFloat(x3) * scaleFactor).toString())
                                if (y3) geometry.setAttribute('y3', (parseFloat(y3) * scaleFactor).toString())
                            }
                        }

                        // Масштабируем все связи
                        const edges = xmlDoc.getElementsByTagName('edge')
                        for (let i = 0; i < edges.length; i++) {
                            const edge = edges[i]
                            const geometry = edge.getElementsByTagName('geometry')[0]

                            if (geometry) {
                                const startX = geometry.getAttribute('startX')
                                const startY = geometry.getAttribute('startY')
                                const endX = geometry.getAttribute('endX')
                                const endY = geometry.getAttribute('endY')

                                if (startX) geometry.setAttribute('startX', (parseFloat(startX) * scaleFactor).toString())
                                if (startY) geometry.setAttribute('startY', (parseFloat(startY) * scaleFactor).toString())
                                if (endX) geometry.setAttribute('endX', (parseFloat(endX) * scaleFactor).toString())
                                if (endY) geometry.setAttribute('endY', (parseFloat(endY) * scaleFactor).toString())
                            }
                        }

                        // Сбрасываем масштаб после применения
                        canvasScale.value = 100
                    }

                    const serializer = new XMLSerializer()
                    const newXML = serializer.serializeToString(xmlDoc)

                    xmlContent.value = newXML
                    validationMessage.value = '✓ Канвас обновлен'

                    // Перевалидируем XML
                    nextTick(() => validateXML())

                } catch (error) {
                    validationMessage.value = 'Ошибка обновления канваса'
                }
            }

            // Методы для масштабирования предпросмотра
            const zoomIn = () => {
                previewScale.value = Math.min(500, previewScale.value + 25)
            }

            const zoomOut = () => {
                previewScale.value = Math.max(25, previewScale.value - 25)
            }

            const resetZoom = () => {
                previewScale.value = 100
            }

            const centerGraph = () => {
                if (graphComponent.value) {
                    // Здесь можно добавить логику центрирования
                    validationMessage.value = 'Центрирование графа'
                }
            }

            const toggleFitToScreen = () => {
                fitToScreen.value = !fitToScreen.value
                if (!fitToScreen.value) {
                    previewScale.value = 100
                }
            }

            // Следим за изменением размеров канваса для автоматического подбора масштаба
            watch([canvasWidth, canvasHeight, fitToScreen], () => {
                if (fitToScreen.value && validXML.value) {
                    // Автоматически подбираем масштаб для вписывания в окно предпросмотра
                    const containerWidth = previewWidth.value
                    const containerHeight = previewHeight.value

                    const widthRatio = containerWidth / actualCanvasWidth.value
                    const heightRatio = containerHeight / actualCanvasHeight.value

                    previewScale.value = Math.floor(Math.min(widthRatio, heightRatio) * 100)
                    previewScale.value = Math.max(25, Math.min(200, previewScale.value))
                }
            })

            const loadTemplate = (template: Template) => {
                xmlContent.value = template.xml
                validationMessage.value = `Загружен шаблон: ${template.name}`
                nextTick(() => {
                    validateXML()
                    if (xmlEditor.value) {
                        xmlEditor.value.focus()
                    }
                })
            }

            const loadDialectTemplate = (dialect: DialectTemplate) => {
                dialectXMLContent.value = dialect.xml
                dialectValidationMessage.value = `Загружен диалект: ${dialect.name}`
                nextTick(() => {
                    validateDialectXML()
                    if (dialectEditor.value) {
                        dialectEditor.value.focus()
                    }
                })
            }

            const loadExample = () => {
                xmlContent.value = templates.value[1].xml // Простой граф
                validateXML()
            }

            const loadDefaultDialect = () => {
                dialectXMLContent.value = baseDialectXML.value
                validateDialectXML()
            }

            const clearEditor = () => {
                xmlContent.value = ''
                validationStatus.value = 'idle'
                validationMessage.value = 'Введите XML для начала работы'
                validXML.value = false
                showGraph.value = false
            }

            const clearDialectEditor = () => {
                dialectXMLContent.value = ''
                dialectValidationStatus.value = 'idle'
                dialectValidationMessage.value = 'Введите XML диалекта или выберите готовый'
                validDialectXML.value = false
            }

            const refreshPreview = () => {
                validateXML()
            }

            const toggleDialectPreview = () => {
                showDialectPreview.value = !showDialectPreview.value
            }

            const downloadXML = () => {
                if (!xmlContent.value) return

                const blob = new Blob([xmlContent.value], { type: 'application/xml' })
                const url = URL.createObjectURL(blob)
                const a = document.createElement('a')
                a.href = url
                a.download = `graph-${new Date().getTime()}.xml`
                document.body.appendChild(a)
                a.click()
                document.body.removeChild(a)
                URL.revokeObjectURL(url)
            }

            const downloadDialectXML = () => {
                if (!dialectXMLContent.value) return

                const blob = new Blob([dialectXMLContent.value], { type: 'application/xml' })
                const url = URL.createObjectURL(blob)
                const a = document.createElement('a')
                a.href = url
                a.download = `dialect-${new Date().getTime()}.xml`
                document.body.appendChild(a)
                a.click()
                document.body.removeChild(a)
                URL.revokeObjectURL(url)
            }

            const generateRandomGraph = () => {
                const nodes = []
                const edges = []
                const nodeCount = Math.floor(Math.random() * 8) + 3

                for (let i = 0; i < nodeCount; i++) {
                    const x = 100 + Math.random() * 600
                    const y = 100 + Math.random() * 400
                    const type = ['circle', 'rectangle', 'triangle'][Math.floor(Math.random() * 3)]
                    const color = `#${Math.floor(Math.random() * 16777215).toString(16)}`

                    nodes.push(`  <node id="node${i}" type="${type}" label="Node ${i}">
    <geometry x="${x.toFixed(0)}" y="${y.toFixed(0)}" ${type === 'circle' ? 'radius="30"' : type === 'rectangle' ? 'width="60" height="40"' : 'x1="0" y1="0" x2="30" y2="0" x3="15" y3="30"'}/>
    <background color="${color}"/>
  </node>`)
                }

                for (let i = 0; i < nodeCount - 1; i++) {
                    if (Math.random() > 0.5) {
                        edges.push(`  <edge id="edge${i}" type="line">
    <geometry startX="0" startY="0" endX="0" endY="0"/>
    <background color="#666"/>
  </edge>`)
                    }
                }

                xmlContent.value = `<graph>
  <canvas width="${canvasWidth.value}" height="${canvasHeight.value}"/>
${nodes.join('\n')}
${edges.join('\n')}
</graph>`

                validateXML()
            }

            const copyToClipboard = async () => {
                if (!xmlContent.value) return

                try {
                    await navigator.clipboard.writeText(xmlContent.value)
                    validationMessage.value = '✓ XML графа скопирован в буфер'
                } catch (error) {
                    validationMessage.value = 'Ошибка копирования'
                }
            }

            const showSampleXML = () => {
                xmlContent.value = `<graph>
  <!-- Пример корректного XML -->
  <canvas width="${canvasWidth.value}" height="${canvasHeight.value}"/>

  <node id="example1" type="circle" label="Пример">
    <geometry x="400" y="300" radius="40"/>
    <background color="#1976d2"/>
    <labelSettings font="16px Arial" color="white"/>
  </node>

  <edge id="exampleEdge" type="line" label="Связь">
    <geometry startX="360" startY="300" endX="300" endY="300"/>
    <background color="#333"/>
    <edgeStyle lineWidth="2"/>
  </edge>
</graph>`
                validateXML()
            }

            const exportAsSVG = () => {
                validationMessage.value = 'Экспорт SVG в разработке'
            }

            const shareGraph = () => {
                validationMessage.value = 'Шаринг в разработке'
            }

            // Инициализация
            loadExample()
            loadDefaultDialect()

            return {
                // Рефы
                xmlEditor,
                dialectEditor,
                graphComponent,

                // Состояние
                activeTab,
                xmlContent,
                validationStatus,
                validationMessage,
                validXML,
                showGraph,
                dialectXMLContent,
                dialectValidationStatus,
                dialectValidationMessage,
                validDialectXML,
                showDialectPreview,
                previewWidth,
                previewHeight,
                previewScale,
                fitToScreen,

                // Настройки канваса
                canvasWidth,
                canvasHeight,
                canvasScale,
                actualCanvasWidth,
                actualCanvasHeight,
                previewStyle,

                // Шаблоны
                templates,
                dialectTemplates,

                // Вычисляемые свойства
                charCount,
                lineCount,
                dialectCharCount,
                dialectLineCount,
                nodeCount,
                edgeCount,
                nodeTypes,
                activeDialectName,
                dialectNodeTypes,
                dialectEdgeTypes,

                // Методы
                handleXMLChange,
                validateXML,
                formatXML,
                adjustCanvasWidth,
                adjustCanvasHeight,
                applyCanvasPreset,
                updateCanvasInXML,
                zoomIn,
                zoomOut,
                resetZoom,
                centerGraph,
                toggleFitToScreen,
                loadTemplate,
                loadDialectTemplate,
                loadExample,
                loadDefaultDialect,
                clearEditor,
                clearDialectEditor,
                refreshPreview,
                toggleDialectPreview,
                downloadXML,
                downloadDialectXML,
                generateRandomGraph,
                copyToClipboard,
                showSampleXML,
                exportAsSVG,
                shareGraph,
                validateDialectXML,
                formatDialectXML,
                handleDialectInput
            }
        }
    })
</script>

<style scoped>
    .generator-view {
        padding: 20px 0;
        background-color: #f8f9fa;
        min-height: 100vh;
    }

    .container {
        width: 100%;
        max-width: 1400px;
        margin: 0 auto;
        padding: 0 20px;
    }

    /* Hero Section */
    .hero {
        background: linear-gradient(135deg, #6a11cb 0%, #2575fc 100%);
        padding: 40px 30px;
        text-align: center;
        border-radius: 12px;
        margin-bottom: 40px;
        color: white;
        box-shadow: 0 4px 20px rgba(106, 17, 203, 0.3);
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

    /* Layout */
    .generator-layout {
        display: grid;
        grid-template-columns: 1fr 1fr;
        gap: 30px;
        margin-bottom: 40px;
    }

    @media (max-width: 1200px) {
        .generator-layout {
            grid-template-columns: 1fr;
        }
    }

    /* Панели */
    .editor-panel,
    .preview-panel {
        background: white;
        border-radius: 12px;
        box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08);
        overflow: hidden;
    }

    /* Вкладки */
    .tabs {
        display: flex;
        background: #f1f3f4;
        border-bottom: 1px solid #e0e0e0;
    }

    .tab-btn {
        flex: 1;
        padding: 15px 20px;
        background: none;
        border: none;
        border-bottom: 3px solid transparent;
        font-size: 1rem;
        font-weight: 500;
        color: #666;
        cursor: pointer;
        transition: all 0.3s ease;
    }

        .tab-btn:hover {
            background: rgba(0, 0, 0, 0.05);
            color: #333;
        }

        .tab-btn.active {
            background: white;
            border-bottom-color: #1976d2;
            color: #1976d2;
            font-weight: 600;
        }

    .editor-content {
        display: flex;
        flex-direction: column;
        height: 100%;
    }

    .panel-header {
        background: linear-gradient(135deg, #2c3e50 0%, #34495e 100%);
        padding: 20px;
        color: white;
        display: flex;
        justify-content: space-between;
        align-items: center;
        flex-wrap: wrap;
        gap: 15px;
    }

        .panel-header h3 {
            margin: 0;
            font-size: 1.5rem;
            font-weight: 600;
        }

    .editor-controls,
    .preview-controls {
        display: flex;
        gap: 10px;
        flex-wrap: wrap;
    }

    .control-btn {
        padding: 8px 16px;
        background: rgba(255, 255, 255, 0.15);
        color: white;
        border: 1px solid rgba(255, 255, 255, 0.3);
        border-radius: 6px;
        cursor: pointer;
        font-size: 0.9rem;
        font-weight: 500;
        transition: all 0.3s ease;
        display: flex;
        align-items: center;
        gap: 6px;
    }

        .control-btn:hover:not(:disabled) {
            background: rgba(255, 255, 255, 0.25);
            transform: translateY(-2px);
        }

        .control-btn:disabled {
            opacity: 0.5;
            cursor: not-allowed;
        }

        .control-btn.active {
            background: #4caf50;
            border-color: #4caf50;
        }

    /* Редактор XML */
    .editor-container {
        padding: 0;
        position: relative;
        flex: 1;
    }

    .xml-editor {
        width: 100%;
        height: 100%;
        min-height: 300px;
        padding: 20px;
        border: none;
        font-family: 'Courier New', monospace;
        font-size: 14px;
        line-height: 1.5;
        background: #1e1e1e;
        color: #d4d4d4;
        resize: none;
        outline: none;
        tab-size: 2;
    }

        .xml-editor::placeholder {
            color: #888;
        }

        .xml-editor:focus {
            outline: none;
        }

    .editor-info {
        position: absolute;
        bottom: 10px;
        right: 20px;
        display: flex;
        gap: 15px;
        font-size: 0.8rem;
        color: #888;
        background: rgba(0, 0, 0, 0.7);
        padding: 4px 8px;
        border-radius: 4px;
    }

    /* Статус валидации */
    .editor-status {
        padding: 15px 20px;
        font-weight: 500;
        border-top: 1px solid #eee;
    }

        .editor-status.idle {
            background: #f8f9fa;
            color: #666;
        }

        .editor-status.valid {
            background: #e8f5e9;
            color: #2e7d32;
        }

        .editor-status.error {
            background: #ffebee;
            color: #c62828;
        }

    /* Настройки канваса */
    .canvas-settings {
        padding: 20px;
        border-top: 1px solid #eee;
        border-bottom: 1px solid #eee;
        background: #f8f9fa;
    }

        .canvas-settings h4 {
            margin: 0 0 15px 0;
            color: #333;
        }

    .canvas-controls {
        display: grid;
        grid-template-columns: repeat(2, 1fr);
        gap: 15px;
    }

    .canvas-control-group {
        display: flex;
        flex-direction: column;
        gap: 5px;
    }

        .canvas-control-group label {
            font-size: 0.9rem;
            font-weight: 500;
            color: #555;
        }

    .input-with-buttons {
        display: flex;
        gap: 5px;
        align-items: center;
    }

        .input-with-buttons input {
            flex: 1;
            padding: 8px 12px;
            border: 1px solid #ddd;
            border-radius: 4px;
            font-size: 0.9rem;
        }

    .value-buttons {
        display: flex;
        flex-direction: column;
        gap: 2px;
    }

    .value-btn {
        width: 30px;
        height: 15px;
        padding: 0;
        border: 1px solid #ddd;
        background: white;
        cursor: pointer;
        display: flex;
        align-items: center;
        justify-content: center;
        font-size: 0.8rem;
    }

        .value-btn:hover {
            background: #f0f0f0;
        }

    .unit {
        font-size: 0.9rem;
        color: #666;
        margin-left: 5px;
    }

    .scale-control {
        display: flex;
        align-items: center;
        gap: 10px;
    }

        .scale-control input[type="range"] {
            flex: 1;
        }

    .scale-value {
        font-size: 0.9rem;
        font-weight: 500;
        color: #1976d2;
        min-width: 40px;
    }

    .canvas-presets {
        grid-column: span 2;
        display: flex;
        gap: 10px;
        flex-wrap: wrap;
        margin-top: 10px;
    }

    .preset-btn {
        padding: 8px 16px;
        background: white;
        border: 1px solid #ddd;
        border-radius: 4px;
        font-size: 0.85rem;
        cursor: pointer;
        transition: all 0.3s ease;
    }

        .preset-btn:hover {
            border-color: #1976d2;
            color: #1976d2;
        }

        .preset-btn.active {
            background: #1976d2;
            border-color: #1976d2;
            color: white;
        }

    .apply-btn {
        background: #4caf50;
        border-color: #4caf50;
        color: white;
        margin-left: auto;
    }

        .apply-btn:hover {
            background: #45a049;
            border-color: #45a049;
        }

    /* Шаблоны */
    .template-buttons {
        padding: 20px;
        border-top: 1px solid #eee;
    }

        .template-buttons h4 {
            margin: 0 0 15px 0;
            color: #333;
        }

    .template-grid {
        display: grid;
        grid-template-columns: repeat(2, 1fr);
        gap: 10px;
    }

    .template-btn {
        padding: 12px;
        background: #f8f9fa;
        border: 1px solid #e0e0e0;
        border-radius: 8px;
        cursor: pointer;
        display: flex;
        align-items: center;
        gap: 10px;
        transition: all 0.3s ease;
    }

        .template-btn:hover {
            background: #e3f2fd;
            border-color: #1976d2;
            transform: translateY(-2px);
        }

    .template-icon {
        font-size: 1.2rem;
    }

    .template-name {
        font-weight: 500;
        color: #333;
    }

    /* Предпросмотр */
    .preview-container {
        padding: 25px;
        min-height: 500px;
        display: flex;
        align-items: center;
        justify-content: center;
        background: #f8f9fa;
        flex: 1;
        position: relative;
        overflow: auto;
    }

    .graph-wrapper {
        transition: transform 0.3s ease;
    }

    .preview-controls-overlay {
        position: absolute;
        top: 10px;
        right: 10px;
        z-index: 10;
        background: rgba(255, 255, 255, 0.9);
        padding: 8px 12px;
        border-radius: 6px;
        box-shadow: 0 2px 8px rgba(0, 0, 0, 0.15);
        border: 1px solid #e0e0e0;
    }

    .zoom-controls {
        display: flex;
        align-items: center;
        gap: 8px;
    }

    .zoom-btn {
        width: 32px;
        height: 32px;
        padding: 0;
        border: 1px solid #ddd;
        background: white;
        border-radius: 4px;
        cursor: pointer;
        display: flex;
        align-items: center;
        justify-content: center;
        font-size: 0.9rem;
    }

        .zoom-btn:hover {
            background: #f0f0f0;
        }

        .zoom-btn.reset {
            background: #f8f9fa;
            border-color: #ccc;
        }

    .zoom-icon {
        font-size: 0.9rem;
    }

    .zoom-level {
        font-size: 0.9rem;
        font-weight: 500;
        color: #333;
        min-width: 40px;
        text-align: center;
    }

    .error-message,
    .welcome-message {
        text-align: center;
        max-width: 400px;
        padding: 30px;
    }

    .error-icon,
    .welcome-icon {
        font-size: 4rem;
        margin-bottom: 20px;
    }

    .error-message h4,
    .welcome-message h4 {
        margin: 0 0 15px 0;
        color: #333;
    }

    .error-message p,
    .welcome-message p {
        margin: 0 0 10px 0;
        color: #666;
        line-height: 1.5;
    }

    .btn-fix {
        margin-top: 20px;
        padding: 10px 20px;
        background: #1976d2;
        color: white;
        border: none;
        border-radius: 6px;
        cursor: pointer;
        font-weight: 500;
        transition: background 0.3s ease;
    }

        .btn-fix:hover {
            background: #1565c0;
        }

    /* Информация о графе и диалекте */
    .graph-info,
    .dialect-info {
        padding: 20px;
        border-top: 1px solid #eee;
        background: #f8f9fa;
    }

    .dialect-info {
        background: #e8f5e9;
        border-top: 1px solid #c8e6c9;
    }

        .dialect-info h4 {
            margin: 0 0 15px 0;
            color: #2e7d32;
        }

    .info-grid {
        display: grid;
        grid-template-columns: repeat(6, 1fr);
        gap: 10px;
    }

    @media (max-width: 1200px) {
        .info-grid {
            grid-template-columns: repeat(3, 1fr);
        }
    }

    @media (max-width: 768px) {
        .info-grid {
            grid-template-columns: repeat(2, 1fr);
        }
    }

    @media (max-width: 480px) {
        .info-grid {
            grid-template-columns: 1fr;
        }
    }

    .info-item {
        text-align: center;
        padding: 10px;
        background: white;
        border-radius: 8px;
        border: 1px solid #e0e0e0;
    }

    .dialect-info .info-item {
        background: #f1f8e9;
        border-color: #c5e1a5;
    }

    .info-label {
        display: block;
        font-size: 0.8rem;
        color: #666;
        margin-bottom: 5px;
    }

    .info-value {
        display: block;
        font-size: 1rem;
        font-weight: 600;
        color: #1976d2;
    }

    .dialect-info .info-value {
        color: #2e7d32;
    }

    .status-active {
        color: #4caf50 !important;
    }

    /* Быстрые действия */
    .quick-actions {
        background: white;
        padding: 30px;
        border-radius: 12px;
        box-shadow: 0 4px 20px rgba(0, 0, 0, 0.08);
    }

        .quick-actions h3 {
            margin: 0 0 25px 0;
            text-align: center;
            color: #333;
            font-size: 1.8rem;
        }

    .actions-grid {
        display: grid;
        grid-template-columns: repeat(4, 1fr);
        gap: 20px;
    }

    @media (max-width: 768px) {
        .actions-grid {
            grid-template-columns: repeat(2, 1fr);
        }
    }

    @media (max-width: 480px) {
        .actions-grid {
            grid-template-columns: 1fr;
        }
    }

    .action-btn {
        padding: 20px;
        background: linear-gradient(135deg, #f8f9fa 0%, #e9ecef 100%);
        border: 2px solid transparent;
        border-radius: 10px;
        cursor: pointer;
        display: flex;
        flex-direction: column;
        align-items: center;
        gap: 10px;
        transition: all 0.3s ease;
    }

        .action-btn:hover {
            border-color: #1976d2;
            background: linear-gradient(135deg, #e3f2fd 0%, #bbdefb 100%);
            transform: translateY(-3px);
            box-shadow: 0 5px 15px rgba(25, 118, 210, 0.2);
        }

    .action-icon {
        font-size: 2rem;
    }

    .action-text {
        font-weight: 600;
        color: #333;
    }
</style>