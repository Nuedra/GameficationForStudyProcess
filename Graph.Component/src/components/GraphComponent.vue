<template>
    <div class="graph-container">
        <canvas class="graph" 
                ref="graphCanvas">
        </canvas>
        <div ref="tooltipRef" class="tooltip"></div>
    </div>
</template>

<script lang="ts">
    import { defineComponent, ref, PropType, watch } from 'vue';
    import * as Graph from './Graph';
    import * as Dialect from './Dialect';
    import * as Tests from './tests';

    import * as graphApi from './graphApi';

    export default defineComponent({
        name: 'GraphComponent',

        props: {
            filePath: String as PropType<string | undefined>,
            xmlData: String as PropType<string | undefined>,
            dialectData: String as PropType<string | undefined>,
            // ОПЦИОНАЛЬНЫЕ параметры размера
            width: {
                type: Number,
                default: undefined
            },
            height: {
                type: Number,
                default: undefined
            }
        },

        setup(props) {
            const graphCanvas = ref<HTMLCanvasElement | null>(null);
            const tooltipRef = ref<HTMLDivElement | null>(null);

            // Отслеживаем изменение ширины
            watch(() => props.width, (newWidth, oldWidth) => {
                if (newWidth !== oldWidth && newWidth !== undefined) {
                    console.log('Width changed:', newWidth);
                    // Здесь можно вызвать метод обновления размера
                    // emit('size-change', { width: newWidth, height: props.height });
                }
            });

            // Отслеживаем изменение высоты
            watch(() => props.height, (newHeight, oldHeight) => {
                if (newHeight !== oldHeight && newHeight !== undefined) {
                    console.log('Height changed:', newHeight);
                    // Здесь можно вызвать метод обновления размера
                    // emit('size-change', { width: props.width, height: newHeight });
                }
            });

            //// Отслеживаем изменение XML данных
            //watch(() => props.xmlData, (newXmlData, oldXmlData) => {
            //    if (newXmlData !== oldXmlData && newXmlData) {
            //        console.log('XML data changed');
            //        // Здесь можно вызвать метод обновления графа
            //        // emit('xml-data-change', newXmlData);
            //    }
            //});

            //// Отслеживаем изменение диалекта
            //watch(() => props.dialectData, (newDialectData, oldDialectData) => {
            //    if (newDialectData !== oldDialectData && newDialectData) {
            //        console.log('Dialect data changed');
            //        // Здесь можно вызвать метод обновления диалекта
            //        // emit('dialect-change', newDialectData);
            //    }
            //});

            return {
                graphCanvas,
                tooltipRef
            }
        },

        data() {
            return {
                graph: null as Graph.Graph | null,
                graph_figures: [] as Graph.DataShapes[],
                dialect: null as Dialect.Dialect | null,
                runTests: true,
                hoveredFig: null as Graph.DataShapes | null,
                pre_hoveredFig: null as Graph.DataShapes | null,
                clickedFig: null as Graph.DataShapes | null,
                pre_clickedFig: null as Graph.DataShapes | null,
                xmlCanvasWidth: 800,  // Размер из XML
                xmlCanvasHeight: 600,  // Размер из XML
                containerRef: null as HTMLElement | null
            };
        },

        mounted() {
            // Получаем контейнер через ref
            this.containerRef = this.$el as HTMLElement;

            if (this.dialectData) {
                this.setDialect(this.dialectData);
            }

            if (this.xmlData) {
                this.$nextTick(() => {
                    this.loadGraphFromXml(this.xmlData as string);
                });
            }

            // Инициализация расширенных обработчиков
            this.$nextTick(() => {
                this.setupEnhancedClickHandlers();
                this.setupSelectionConstraints();
            });
        },

        methods: {
            // ==================== PUBLIC ====================

            async loadGraphFromXml(xmlData: string): Promise<void> {
                try {
                    await this.initializeGraphFromXml(xmlData);
                } catch (error: unknown) {
                    console.error('Ошибка загрузки графа:', error);
                    throw error;
                }
            },

            setDialect(dialectData: string): void {
                try {
                    this.dialect = Dialect.Dialect.fromXML(dialectData);
                } catch (error: unknown) {
                    console.error('Ошибка установки диалекта:', error);
                    throw error;
                }
            },

            runGraphTests(): void {
                if (!this.graph || !this.graph_figures.length) {
                    console.warn('Невозможно запустить тесты: граф не инициализирован');
                    return;
                }

                const canvas = this.graphCanvas;
                const ctx = canvas?.getContext("2d");
                if (!ctx) {
                    console.warn('Невозможно запустить тесты: контекст canvas не доступен');
                    return;
                }

                try {
                    Tests.test(ctx, this.graph, this.graph_figures);
                } catch (error: unknown) {
                    console.error('Ошибка выполнения тестов:', error);
                }
            },

            getNodes(): Graph.DataShapes[] {
                return this.graph ? this.graph._nodes : [];
            },

            getEdges(): Graph.DataShapes[] {
                return this.graph ? this.graph._edges : [];
            },

            getObjectAt(x: number, y: number): Graph.DataShapes | null {
                if (!this.graph_figures.length) return null;

                for (let fig_index = this.graph_figures.length - 1; fig_index >= 0; fig_index--) {
                    const fig = this.graph_figures[fig_index];
                    if (fig && fig.is_inside(x, y)) {
                        return fig;
                    }
                }
                return null;
            },

            addNode(nodeData: any): void {
                if (!this.graph) return;

                const node = this.createNodeFromData(nodeData);
                if (node) {
                    this.graph_figures.push(node);
                    this.graph.addNode(node);
                    this.graph.requestRedraw();
                }
            },

            addEdge(edgeData: any): void {
                if (!this.graph) return;

                const edge = new Graph.Line(edgeData);
                this.graph_figures.push(edge);
                this.graph.addEdge(edge);
                this.graph.requestRedraw();
            },

            deleteNode(nodeId: string): boolean {
                if (!this.graph) return false;

                const initialLength = this.graph._nodes.length;
                this.graph._nodes = this.graph._nodes.filter((n: Graph.DataShapes) => n?._id !== nodeId);
                this.graph_figures = this.graph_figures.filter((f: Graph.DataShapes) => f?._id !== nodeId);

                this.graph._edges = this.graph._edges.filter((edge: Graph.DataShapes) => {
                    const lineEdge = edge as Graph.Line;
                    return lineEdge._source?._id !== nodeId && lineEdge._target?._id !== nodeId;
                });

                this.graph.requestRedraw();
                return this.graph._nodes.length < initialLength;
            },

            deleteEdge(edgeId: string): boolean {
                if (!this.graph) return false;

                const initialLength = this.graph._edges.length;
                this.graph._edges = this.graph._edges.filter((e: Graph.DataShapes) => e?._id !== edgeId);
                this.graph_figures = this.graph_figures.filter((f: Graph.DataShapes) => f?._id !== edgeId);
                this.graph.requestRedraw();

                return this.graph._edges.length < initialLength;
            },

            clearGraph(): void {
                if (this.graph) {
                    this.graph._nodes = [];
                    this.graph._edges = [];
                    this.graph_figures = [];
                    this.graph.requestRedraw();
                }
            },

            getGraphData(): { nodes: any[]; edges: any[] } {
                const nodes = this.getNodes().map(node => this.nodeToData(node));
                const edges = this.getEdges().map(edge => this.edgeToData(edge as Graph.Line));

                return { nodes, edges };
            },

            exportToXml(): string {
                const data = this.getGraphData();
                return `<!-- Экспортированный граф -->
                        <graph>
                        <!-- ${data.nodes.length} вершин, ${data.edges.length} рёбер -->
                        <canvas width="800" height="600"/>
                        ${data.nodes.map(node => this.nodeToXml(node)).join('\n  ')}
                        ${data.edges.map(edge => this.edgeToXml(edge)).join('\n  ')}
                        </graph>`;
            },

            async updateGraphDataXML(newXmlData: string): Promise<void> {
                try {
                    // Получаем canvas и контекст
                    const canvas = this.graphCanvas;
                    if (!canvas) throw new Error('Canvas не найден');

                    const ctx = canvas.getContext("2d");
                    if (!ctx) throw new Error('Не удалось получить 2D контекст');

                    // Парсим XML
                    const xmlDoc = this.parseXML(newXmlData);

                    // Определяем размеры с учетом приоритета:
                    let newWidth = canvas.width;
                    let newHeight = canvas.height;

                    // Если пользовательские размеры не заданы, обновляем из XML
                    if (this.width === undefined || this.height === undefined) {
                        const canvasElement = xmlDoc.getElementsByTagName("canvas")[0];
                        if (canvasElement) {
                            const width = canvasElement.getAttribute("width");
                            const height = canvasElement.getAttribute("height");

                            if (width && height) {
                                newWidth = parseInt(width, 10);
                                newHeight = parseInt(height, 10);
                                this.xmlCanvasWidth = newWidth;
                                this.xmlCanvasHeight = newHeight;
                            }
                        }
                    }

                    canvas.width = newWidth;
                    canvas.height = newHeight;

                    // Проверяем и обновляем диалект если необходимо
                    const dialectName = xmlDoc.getElementsByTagName("graph")[0].getAttribute("dialect");
                    if (dialectName && dialectName !== "base" && (!this.dialect || this.dialect.name !== dialectName)) {
                        console.warn(`Диалект "${dialectName}" указан в новом XML, но не установлен в компоненте`);
                        // Здесь можно добавить логику для загрузки диалекта, если это предусмотрено
                    }

                    // Очищаем существующий граф
                    if (this.graph) {
                        this.graph._nodes = [];
                        this.graph._edges = [];
                        this.graph_figures = [];
                    }

                    // Загружаем кастомные описания
                    const customDescriptions = this.loadCustomDescriptions(xmlDoc);

                    // Создаем узлы и ребра из новой XML
                    this.createNodesFromXml(xmlDoc, customDescriptions);
                    this.createEdgesFromXml(xmlDoc);

                    // Перерисовываем граф
                    this.graph?.requestRedraw();

                } catch (error: unknown) {
                    console.error('Ошибка обновления графа:', error);
                    throw error;
                }
            },

            //// Метод для обновления размеров (например, при изменении props)
            //updateCanvasSize(): void {
            //    const canvas = this.graphCanvas;
            //    if (!canvas) return;

            //    // Определяем новые размеры с учетом приоритета
            //    let newWidth = 800;
            //    let newHeight = 600;

            //    if (this.width !== undefined && this.height !== undefined) {
            //        newWidth = this.width;
            //        newHeight = this.height;
            //    } else {
            //        newWidth = this.xmlCanvasWidth;
            //        newHeight = this.xmlCanvasHeight;
            //    }

            //    // Проверяем, изменились ли размеры
            //    if (canvas.width !== newWidth || canvas.height !== newHeight) {
            //        canvas.width = newWidth;
            //        canvas.height = newHeight;

            //        // Перерисовываем граф
            //        if (this.graph) {
            //            this.graph.requestRedraw();
            //        }
            //    }
            //},

            // Обработчик изменения размеров
            handleCanvasSizeChange(): void {
                const canvas = this.graphCanvas;
                if (!canvas || !this.graph) return;

                // Определяем новые размеры с учетом приоритета
                let newWidth = this.getCurrentCanvasWidth();
                let newHeight = this.getCurrentCanvasHeight();

                // Проверяем, изменились ли размеры
                if (canvas.width !== newWidth || canvas.height !== newHeight) {
                    canvas.width = newWidth;
                    canvas.height = newHeight;

                    // Перерисовываем граф
                    this.graph.requestRedraw();
                }
            },

            // Вспомогательный метод для определения текущих размеров
            getCurrentCanvasWidth(): number {
                if (this.width !== undefined) {
                    return this.width;
                }
                return this.xmlCanvasWidth || 800;
            },

            getCurrentCanvasHeight(): number {
                if (this.height !== undefined) {
                    return this.height;
                }
                return this.xmlCanvasHeight || 600;
            },

            async fetchGraphFromServer(): Promise<void> {
                try {
                    const xmlData = await graphApi.getGraphXML();
                    await this.updateGraphDataXML(xmlData);
                } catch (error: unknown) {
                    console.error('Ошибка загрузки графа с сервера:', error);
                    throw error;
                }
            },

            async saveGraphToServer(): Promise<void> {
                try {
                    //const graphData = this.getGraphData();
                    // Можно отправить либо XML, либо структурированные данные
                    const xmlData = this.exportToXml();
                    await graphApi.updateGraphFromXML(xmlData);
                } catch (error: unknown) {
                    console.error('Ошибка сохранения графа на сервер:', error);
                    throw error;
                }
            },

            async syncWithServer(): Promise<void> {
                try {
                    await this.fetchGraphFromServer();
                } catch (error) {
                    console.error('Ошибка синхронизации с сервером:', error);
                }
            },        

            getSelection(): Graph.DataShapes[] {
                return this.graph ? this.graph.getSelection() : [];
            },

            select(item: Graph.DataShapes): void {
                if (this.graph && this.graph.canSelect(item)) {
                    this.graph.select(item);
                }
            },

            deselect(item: Graph.DataShapes): void {
                if (this.graph) {
                    this.graph.deselect(item);
                }
            },

            canSelect(item: Graph.DataShapes): boolean {
                return this.graph ? this.graph.canSelect(item) : false;
            },

            emphasize(item: Graph.DataShapes): void {
                if (this.graph) {
                    this.graph.emphasize(item);
                }
            },

            deEmphasize(item: Graph.DataShapes): void {
                if (this.graph) {
                    this.graph.deEmphasize(item);
                }
            },

            getAvailableEvents(item: Graph.DataShapes): string[] {
                return this.graph ? this.graph.getAvailableEvents(item) : [];
            },

            getAttachedEvents(): any[] {
                return this.graph ? this.graph.getAttachedEvents() : [];
            },

            setStyle(item: Graph.DataShapes, eventType: string, styleInfo: any): void {
                if (this.graph) {
                    this.graph.setStyle(item, eventType, styleInfo);
                }
            },

            attachEvent(item: Graph.DataShapes, eventInfo: any): void {
                if (this.graph) {
                    this.graph.attachEvent(item, eventInfo);
                }
            },

            clearSelection(): void {
                if (this.graph) {
                    this.graph.clearSelection();
                }
            },

            clearEmphasis(): void {
                if (this.graph) {
                    this.graph.clearEmphasis();
                }
            },

            // Расширенные методы выбора
            selectWithMode(item: Graph.DataShapes, mode: 'toggle' | 'add' | 'replace' = 'toggle'): boolean {
                return this.graph?.select(item, mode) || false;
            },

            deselectWithCheck(item: Graph.DataShapes): boolean {
                return this.graph?.deselect(item) || false;
            },

            getSelectedByType(type: string): Graph.DataShapes[] {
                return this.graph?.getSelectedByType(type) || [];
            },

            getSelectionCount(type?: string): number {
                return this.graph?.getSelectionCount(type) || 0;
            },

            setSelectionConstraint(constraint: any): void {
                this.graph?.setSelectionConstraint(constraint);
            },

            getSelectionConstraint(type: string): any {
                return this.graph?.getSelectionConstraint(type);
            },

            selectMultiple(items: Graph.DataShapes[], clearExisting: boolean = true): number {
                return this.graph?.selectMultiple(items, clearExisting) || 0;
            },

            clearSelectionByType(type?: string): void {
                this.graph?.clearSelectionByType(type);
            },

            // Обработка кликов с учетом системы ограничений
            setupEnhancedClickHandlers(): void {
                const canvas = this.graphCanvas;
                if (!canvas || !this.graph) return;

                canvas.addEventListener("click", (event) => {
                    const rect = canvas.getBoundingClientRect();
                    const mouseX = event.clientX - rect.left;
                    const mouseY = event.clientY - rect.top;

                    const clickedItem = this.getObjectAt(mouseX, mouseY);
                    if (clickedItem) {
                        // Передаем событие в граф для обработки с учетом модификаторов
                        this.graph.handleClick(clickedItem, event);
                    } else {
                        // Клик мимо объектов - снимаем выделение
                        if (!event.ctrlKey && !event.metaKey && !event.shiftKey) {
                            this.graph.clearSelectionByType();
                        }
                    }
                });

                // Контекстное меню для снятия выделения
                canvas.addEventListener("contextmenu", (event) => {
                    const rect = canvas.getBoundingClientRect();
                    const mouseX = event.clientX - rect.left;
                    const mouseY = event.clientY - rect.top;

                    const clickedItem = this.getObjectAt(mouseX, mouseY);
                    if (clickedItem) {
                        event.preventDefault();

                        const constraint = this.graph.getSelectionConstraint(clickedItem._type);
                        if (constraint?.deselectMode === 'context_menu') {
                            this.graph.deselect(clickedItem);
                        }
                    }
                });
            },

            // Пример использования: настройка ограничений
            setupSelectionConstraints(): void {
                // Только 1 квадрат может быть выбран
                this.setSelectionConstraint({
                    type: 'rectangle',
                    maxSelection: 1,
                    selectionMode: 'single',
                    allowDeselect: true,
                    deselectMode: 'click'
                });

                // До 5 кругов
                this.setSelectionConstraint({
                    type: 'circle',
                    maxSelection: 5,
                    selectionMode: 'multiple',
                    allowDeselect: true,
                    deselectMode: 'ctrl+click'
                });

                // Треугольники нельзя снимать выделение
                this.setSelectionConstraint({
                    type: 'triangle',
                    maxSelection: 3,
                    selectionMode: 'multiple',
                    allowDeselect: false,
                    deselectMode: 'context_menu' // Но можно через контекстное меню
                });

                // Глобальные ограничения
                this.graph._selectionManager.updateConfig({
                    globalMaxSelection: 15,
                    allowMixedSelection: true,
                    autoDeselect: true
                });
            },

            // ==================== PRIVATE ====================

            async initializeGraphFromXml(xmlData: string): Promise<void> {
                const canvas = this.graphCanvas;
                if (!canvas) throw new Error('Canvas не найден');

                const ctx = canvas.getContext("2d");
                if (!ctx) throw new Error('Не удалось получить 2D контекст');

                const xmlDoc = this.parseXML(xmlData);

                // Определяем размеры канваса с учетом приоритета:
                let finalWidth = 800;
                let finalHeight = 600;

                // 1. Проверяем, есть ли пользовательские размеры в props
                if (this.width !== undefined && this.height !== undefined) {
                    finalWidth = this.width;
                    finalHeight = this.height;
                }
                // 2. Проверяем размеры в XML
                else {
                    const canvasElement = xmlDoc.getElementsByTagName("canvas")[0];
                    if (canvasElement) {
                        const width = canvasElement.getAttribute("width");
                        const height = canvasElement.getAttribute("height");

                        if (width && height) {
                            finalWidth = parseInt(width, 10);
                            finalHeight = parseInt(height, 10);
                        }
                    }
                }

                // Сохраняем размеры из XML для возможного использования
                const xmlCanvasElement = xmlDoc.getElementsByTagName("canvas")[0];
                if (xmlCanvasElement) {
                    const width = xmlCanvasElement.getAttribute("width");
                    const height = xmlCanvasElement.getAttribute("height");

                    if (width && height) {
                        this.xmlCanvasWidth = parseInt(width, 10);
                        this.xmlCanvasHeight = parseInt(height, 10);
                    }
                }

                // Устанавливаем размеры канваса
                canvas.width = finalWidth;
                canvas.height = finalHeight;

                this.graph = new Graph.Graph();
                this.graph_figures = [];
                this.graph.bindCanvas(canvas, ctx);

                this.initializeDialect(xmlDoc);

                const customDescriptions = this.loadCustomDescriptions(xmlDoc);

                this.createNodesFromXml(xmlDoc, customDescriptions);
                this.createEdgesFromXml(xmlDoc);

                if (this.runTests) {
                    this.runGraphTests();
                }

                this.graph.requestRedraw();
                this.setupEventListeners();
            },

            initializeDialect(xmlDoc: Document): void {
                const dialectName = xmlDoc.getElementsByTagName("graph")[0].getAttribute("dialect");

                if (dialectName && !this.dialect && dialectName !== "base") {
                    console.warn(`Диалект "${dialectName}" указан в XML, но не установлен в компоненте`);
                }

                if (this.dialect && dialectName && this.dialect.name !== dialectName && dialectName !== "base") {
                    throw new Error(`Диалект "${dialectName}" в XML не соответствует установленному диалекту "${this.dialect.name}"`);
                }
            },

            validateNodeType(type: string, dialectName: string | null): string {
                if (dialectName && this.dialect && dialectName !== "base") {
                    if (this.dialect.validateNodeType(type)) {
                        const validatedType = this.dialect.nodeTypes.get(type) || type;
                        return validatedType;
                    } else {
                        throw new Error(`Тип узла "${type}" не разрешен в диалекте "${dialectName}"`);
                    }
                }
                return type;
            },

            validateEdgeType(type: string, dialectName: string | null): string {
                if (dialectName && this.dialect && dialectName !== "base") {
                    if (this.dialect.validateEdgeType(type)) {
                        const validatedType = this.dialect.edgeTypes.get(type) || type;
                        return validatedType;
                    } else {
                        throw new Error(`Тип ребра "${type}" не разрешен в диалекте "${dialectName}"`);
                    }
                }
                return type;
            },

            validateArrowType(arrowType: string, dialectName: string | null): string {
                if (arrowType !== "none" && dialectName && this.dialect && dialectName !== "base") {
                    if (this.dialect.validateArrowheadType(arrowType)) {
                        const validatedType = this.dialect.arrowheadTypes.get(arrowType) || "none";
                        return validatedType;
                    } else {
                        throw new Error(`Тип стрелки "${arrowType}" не разрешен в диалекте "${dialectName}"`);
                    }
                }
                return arrowType;
            },

            parseXML(xml_text: string): Document {
                const parser = new DOMParser();
                return parser.parseFromString(xml_text, "application/xml");
            },

            loadCustomDescriptions(xmlDoc: Document): Map<string, any> {
                const customDescriptions = new Map<string, any>();
                const descriptions = xmlDoc.getElementsByTagName("customDescription");

                for (const description of Array.from(descriptions)) {
                    const descType = description.getAttribute("type") || "";
                    const points: Array<{ x: number; y: number }> = [];
                    const curve: Array<{ isCurved: boolean; cp1x: number; cp1y: number; cp2x: number; cp2y: number; }> = [];

                    const pointNodes = description.getElementsByTagName("point");
                    for (const pointNode of Array.from(pointNodes)) {
                        const x = parseFloat(pointNode.getAttribute("x") || "0");
                        const y = parseFloat(pointNode.getAttribute("y") || "0");
                        points.push({ x, y });
                    }

                    const curveNodes = description.getElementsByTagName("curvePoint");
                    for (const curveNode of Array.from(curveNodes)) {
                        const cp1x = parseFloat(curveNode.getAttribute("cp1x") || "0");
                        const cp1y = parseFloat(curveNode.getAttribute("cp1y") || "0");
                        const cp2x = parseFloat(curveNode.getAttribute("cp2x") || "0");
                        const cp2y = parseFloat(curveNode.getAttribute("cp2y") || "0");
                        const isCurved = curveNode.getAttribute("isCurved") === "true";
                        curve.push({ isCurved, cp1x, cp1y, cp2x, cp2y });
                    }

                    const customDescription = {
                        typeName: descType,
                        points,
                        curve
                    };

                    customDescriptions.set(descType, customDescription);
                }

                return customDescriptions;
            },

            createNodesFromXml(xmlDoc: Document, customDescriptions: Map<string, any>): void {
                const nodes = xmlDoc.getElementsByTagName("node");
                const dialectName = xmlDoc.getElementsByTagName("graph")[0].getAttribute("dialect");

                for (const node of Array.from(nodes)) {
                    const id = node.getAttribute("id") || "";
                    let type = node.getAttribute("type") || "";
                    const label = node.getAttribute("label") || "";
                    const info = node.getAttribute("info") || "";
                    const rotation = parseInt(node.getAttribute("rotation") || "0", 10);
                    const geometry = node.getElementsByTagName("geometry")[0];
                    const background = node.getElementsByTagName("background")[0];
                    const edgeStyle = node.getElementsByTagName("edgeStyle")[0];
                    const labelSettings = node.getElementsByTagName("labelSettings")[0];

                    const connectors = this.loadConnectorsForNode(xmlDoc, id);

                    const labelInfo = {
                        text: label,
                        color: (labelSettings && labelSettings.getAttribute("color")) || 'black',
                        font: (labelSettings && labelSettings.getAttribute("font")) || '16px Arial',
                        padding: 10,
                    };

                    let isEdgeDash = false;
                    if (edgeStyle) {
                        isEdgeDash = edgeStyle.getAttribute("isEdgeDash") === 'true';
                    }

                    const image_src = node.getAttribute("image_src") || undefined;
                    const image_scale = node.getAttribute("image_scale") ?
                        parseFloat(node.getAttribute("image_scale")!) : undefined;
                    const image_rotation = node.getAttribute("image_rotation") ?
                        parseFloat(node.getAttribute("image_rotation")!) : undefined;

                    type = this.validateNodeType(type, dialectName);

                    if (geometry) {
                        const baseParams = {
                            id, type, labelInfo, rotation, isEdgeDash, connectors, info,
                            image_src, image_scale, image_rotation
                        };

                        const nodeObj = this.createNodeObject(type, geometry, background, baseParams, customDescriptions);

                        if (nodeObj && this.graph) {
                            this.graph_figures.push(nodeObj);
                            this.graph.addNode(nodeObj);
                        }
                    }
                }
            },

            loadConnectorsForNode(xmlDoc: Document, nodeId: string): any[] {
                const connectors: any[] = [];
                const connectorElements = xmlDoc.getElementsByTagName("connector");

                for (let i = 0; i < connectorElements.length; i++) {
                    const connectorElement = connectorElements[i];
                    const parent_id = connectorElement.getAttribute("parent_id");

                    if (parent_id === nodeId) {
                        const connector = {
                            id: connectorElement.getAttribute("id") || undefined,
                            x: parseFloat(connectorElement.getAttribute("x") || "0"),
                            y: parseFloat(connectorElement.getAttribute("y") || "0"),
                            type: connectorElement.getAttribute("type") || undefined,
                            parent_id: parent_id,
                            info: connectorElement.getAttribute("info") || undefined
                        };
                        connectors.push(connector);
                    }
                }

                return connectors;
            },

            createNodeObject(type: string, geometry: Element, background: Element, baseParams: any, customDescriptions: Map<string, any>): Graph.DataShapes | null {
                const color = background.getAttribute("color") || "black";
                const params = {
                    ...baseParams,
                    label_info: baseParams.labelInfo,
                    color
                };

                switch (type) {
                    case 'circle': {
                        const x = parseFloat(geometry.getAttribute("x") || "0");
                        const y = parseFloat(geometry.getAttribute("y") || "0");
                        const radius = parseFloat(geometry.getAttribute("radius") || "0");

                        return new Graph.Circle({
                            ...params,
                            x, y, radius
                        });
                    }
                    case 'rectangle': {
                        const x = parseFloat(geometry.getAttribute("x") || "0");
                        const y = parseFloat(geometry.getAttribute("y") || "0");
                        const width = parseFloat(geometry.getAttribute("width") || "0");
                        const height = parseFloat(geometry.getAttribute("height") || "0");

                        return new Graph.Rectangle({
                            ...params,
                            x, y, width, height
                        });
                    }
                    case 'triangle': {
                        const x1 = parseFloat(geometry.getAttribute("x1") || "0");
                        const y1 = parseFloat(geometry.getAttribute("y1") || "0");
                        const x2 = parseFloat(geometry.getAttribute("x2") || "0");
                        const y2 = parseFloat(geometry.getAttribute("y2") || "0");
                        const x3 = parseFloat(geometry.getAttribute("x3") || "0");
                        const y3 = parseFloat(geometry.getAttribute("y3") || "0");

                        return new Graph.Triangle({
                            ...params,
                            x_1: x1, y_1: y1, x_2: x2, y_2: y2, x_3: x3, y_3: y3
                        });
                    }
                    case 'regular polygon': {
                        const x = parseFloat(geometry.getAttribute("x") || "0");
                        const y = parseFloat(geometry.getAttribute("y") || "0");
                        const radius = parseFloat(geometry.getAttribute("radius") || "0");
                        const number_of_edges = parseInt(geometry.getAttribute("number_of_edges") || "0", 10);

                        return new Graph.RegularPolygon({
                            ...params,
                            x, y, radius, number_of_edges
                        });
                    }
                    case 'ellipse': {
                        const x = parseFloat(geometry.getAttribute("x") || "0");
                        const y = parseFloat(geometry.getAttribute("y") || "0");
                        const radius_x = parseFloat(geometry.getAttribute("radius_x") || "0");
                        const radius_y = parseFloat(geometry.getAttribute("radius_y") || "0");

                        return new Graph.Ellipse({
                            ...params,
                            x, y, radius_x, radius_y
                        });
                    }
                    case 'rhomb': {
                        const x = parseFloat(geometry.getAttribute("x") || "0");
                        const y = parseFloat(geometry.getAttribute("y") || "0");
                        const width = parseFloat(geometry.getAttribute("width") || "0");
                        const height = parseFloat(geometry.getAttribute("height") || "0");

                        return new Graph.Rhombus({
                            ...params,
                            x, y, width, height
                        });
                    }
                    case 'star': {
                        const x_C = parseFloat(geometry.getAttribute("x_C") || "0");
                        const y_C = parseFloat(geometry.getAttribute("y_C") || "0");
                        const rad = parseFloat(geometry.getAttribute("rad") || "0");
                        const amount_points = parseInt(geometry.getAttribute("amount_points") || "0", 10);
                        const m = parseFloat(geometry.getAttribute("m") || "0");

                        return new Graph.Star({
                            ...params,
                            x_C, y_C, rad, amount_points, m
                        });
                    }
                    case 'cloud': {
                        const x_C = parseFloat(geometry.getAttribute("x_C") || "0");
                        const y_C = parseFloat(geometry.getAttribute("y_C") || "0");
                        const width = parseFloat(geometry.getAttribute("width") || "0");
                        const height = parseFloat(geometry.getAttribute("height") || "0");

                        return new Graph.Cloud({
                            ...params,
                            x_C, y_C, width, height
                        });
                    }
                    default: {
                        const description = customDescriptions.get(type);
                        if (description) {
                            const xCenter = parseFloat(geometry.getAttribute("x_center") || "0");
                            const yCenter = parseFloat(geometry.getAttribute("y_center") || "0");

                            const custom_info = {
                                ...params,
                                x_center: xCenter,
                                y_center: yCenter
                            };

                            return new Graph.CustomShape(custom_info, description);
                        }
                        return null;
                    }
                }
            },

            createEdgesFromXml(xmlDoc: Document): void {
                const edges = xmlDoc.getElementsByTagName("edge");
                const dialectName = xmlDoc.getElementsByTagName("graph")[0].getAttribute("dialect");

                for (const edge of Array.from(edges)) {
                    const id = edge.getAttribute("id") || "";
                    let type = edge.getAttribute("type") || "";
                    const label = edge.getAttribute("label") || "";
                    const info = edge.getAttribute("info") || "";
                    const rotation = parseFloat(edge.getAttribute("rotation") || "0");
                    const geometry = edge.getElementsByTagName("geometry")[0] || edge.getElementsByTagName("lineGeometry")[0];
                    const background = edge.getElementsByTagName("background")[0];
                    const edgeStyle = edge.getElementsByTagName("edgeStyle")[0];
                    let startArrow = edge.getAttribute("startArrow") || "none";
                    let endArrow = edge.getAttribute("endArrow") || "none";

                    let isEdgeDash = false;
                    let is_corners_rounded = false;
                    const internalPoints: Array<{ x: number; y: number }> = [];

                    const internalPointElements = edge.getElementsByTagName("internalPoint");
                    for (let i = 0; i < internalPointElements.length; i++) {
                        const x = parseFloat(internalPointElements[i].getAttribute('x') || "0");
                        const y = parseFloat(internalPointElements[i].getAttribute('y') || "0");
                        internalPoints.push({ x, y });
                    }

                    const labelSettings = edge.getElementsByTagName("labelSettings")[0];
                    const labelInfo = {
                        text: label,
                        color: (labelSettings && labelSettings.getAttribute("color")) || 'black',
                        font: (labelSettings && labelSettings.getAttribute("font")) || '12px Arial',
                        padding: 10,
                    };

                    const sourceNodeId = edge.getAttribute("sourceNodeId");
                    const targetNodeId = edge.getAttribute("targetNodeId");

                    type = this.validateEdgeType(type, dialectName);
                    startArrow = this.validateArrowType(startArrow, dialectName);
                    endArrow = this.validateArrowType(endArrow, dialectName);

                    if (geometry && type === 'line') {
                        const startX = parseFloat(geometry.getAttribute("startX") || "0");
                        const startY = parseFloat(geometry.getAttribute("startY") || "0");
                        const endX = parseFloat(geometry.getAttribute("endX") || "0");
                        const endY = parseFloat(geometry.getAttribute("endY") || "0");
                        const color = background.getAttribute("color") || "black";
                        const lineWidth = parseFloat(edgeStyle?.getAttribute("lineWidth") || "1");

                        if (edgeStyle) {
                            isEdgeDash = edgeStyle.getAttribute("isEdgeDash") === 'true';
                            is_corners_rounded = edgeStyle.getAttribute("isRounded") === 'true';
                        }

                        const max_radius_of_corners = parseFloat(edgeStyle?.getAttribute("maxRadiusOfCorners") || "7");

                        let sourceNode: Graph.DataShapes | null = null;
                        let targetNode: Graph.DataShapes | null = null;

                        if (sourceNodeId && this.graph) {
                            sourceNode = this.graph.getNode(sourceNodeId);
                        }
                        if (targetNodeId && this.graph) {
                            targetNode = this.graph.getNode(targetNodeId);
                        }

                        const line = new Graph.Line({
                            id, type, startX, startY, endX, endY, color, label_info: labelInfo,
                            rotation, lineWidth, isEdgeDash, points: internalPoints, info,
                            is_corners_rounded, max_radius_of_corners
                        }, endArrow, startArrow, targetNode, sourceNode);

                        if (this.graph) {
                            this.graph_figures.push(line);
                            this.graph.addEdge(line);
                        }
                    }
                }
            },

            createNodeFromData(nodeData: any): Graph.DataShapes | null {
                console.log('Creating node from ', nodeData.id);

                const baseParams = {
                    id: nodeData.id,
                    type: nodeData.type,
                    color: nodeData.color,
                    label_info: nodeData.label_info || {
                        text: '',
                        color: 'black',
                        font: '16px Arial',
                        padding: 10
                    },
                    rotation: nodeData.rotation || 0,
                    isEdgeDash: nodeData.isEdgeDash || false,
                    info: nodeData.info
                };

                switch (nodeData.type) {
                    case 'circle': {
                        const circleData = nodeData;
                        return new Graph.Circle({
                            ...baseParams,
                            x: circleData.x,
                            y: circleData.y,
                            radius: circleData.radius
                        });
                    }
                    case 'rectangle': {
                        const rectData = nodeData;
                        return new Graph.Rectangle({
                            ...baseParams,
                            x: rectData.x,
                            y: rectData.y,
                            width: rectData.width,
                            height: rectData.height
                        });
                    }
                    default:
                        return null;
                }
            },

            nodeToData(node: Graph.DataShapes): any {
                return {
                    id: node?._id,
                    type: node?._type,
                    color: node?._color,
                    label_info: node?._label_info,
                    rotation: node?._rotation,
                    isEdgeDash: node?._isEdgeDash,
                    info: node?._info
                };
            },

            edgeToData(edge: Graph.Line): any {
                return {
                    id: edge._id,
                    type: edge._type,
                    startX: edge._startX,
                    startY: edge._startY,
                    endX: edge._endX,
                    endY: edge._endY,
                    color: edge._color,
                    label_info: edge._label_info,
                    lineWidth: edge._lineWidth,
                    info: edge._info
                };
            },

            nodeToXml(node: any): string {
                return `<node id="${node.id}" type="${node.type}" label="${node.label_info?.text || ''}" />`;
            },

            edgeToXml(edge: any): string {
                return `<edge id="${edge.id}" type="${edge.type}" label="${edge.label_info?.text || ''}" />`;
            },

            setupEventListeners(): void {
                const canvas = this.graphCanvas;
                if (!canvas || !this.graph) return;

                // Используем ref вместо getElementById
                const tooltip = this.tooltipRef;
                const container = this.containerRef; // Используем сохраненный ref контейнера

                //const container = canvas.parentElement; // Получаем родительский контейнер

                if (!tooltip || !container) return;

                canvas.addEventListener("mousemove", (event) => {
                    const rect = canvas!.getBoundingClientRect();
                    const containerRect = container.getBoundingClientRect();
                    const mouseX = event.clientX - rect.left;
                    const mouseY = event.clientY - rect.top;

                    this.pre_hoveredFig = this.hoveredFig;
                    this.hoveredFig = null;

                    for (let fig_index = this.graph_figures.length - 1; fig_index >= 0; fig_index--) {
                        const fig = this.graph_figures[fig_index];
                        if (fig && fig.is_inside(mouseX, mouseY)) {
                            this.hoveredFig = fig;
                            if (tooltip && this.hoveredFig?._info && this.hoveredFig?._info !== "") {
                                // Позиционируем tooltip относительно контейнера
                                const tooltipX = event.clientX - containerRect.left + 10;
                                const tooltipY = event.clientY - containerRect.top + 10;

                                tooltip.textContent = this.hoveredFig._info;
                                tooltip.style.left = `${tooltipX}px`;
                                tooltip.style.top = `${tooltipY}px`;
                                tooltip.style.display = 'block';
                            }
                            break;
                        }
                    }

                    if (!this.hoveredFig && tooltip) {
                        tooltip.style.display = 'none';
                    }

                    if (this.hoveredFig !== this.pre_hoveredFig) {
                        const ctx = canvas.getContext("2d");
                        if (ctx) {
                            ctx.clearRect(0, 0, canvas.width, canvas.height);
                            for (const sh of this.graph_figures) {
                                if (sh) {
                                    if (this.clickedFig === sh) {
                                        sh.draw_clicked(ctx);
                                    } else if (this.hoveredFig === sh) {
                                        sh.draw_hovered(ctx);
                                    } else {
                                        sh.draw_canvas(ctx);
                                    }
                                }
                            }
                        }
                    }
                });

                canvas.addEventListener("mousedown", (event) => {
                    const rect = canvas!.getBoundingClientRect();
                    const mouseX = event.clientX - rect.left;
                    const mouseY = event.clientY - rect.top;

                    this.pre_clickedFig = this.clickedFig;
                    this.clickedFig = null;

                    for (let fig_index = this.graph_figures.length - 1; fig_index >= 0; fig_index--) {
                        const fig = this.graph_figures[fig_index];
                        if (fig && fig._type == 'circle') {
                            if ((fig as Graph.Circle).is_clicked(mouseX, mouseY)) {
                                this.clickedFig = fig;
                                break;
                            }
                        } else if (fig && fig.is_inside(mouseX, mouseY)) {
                            this.clickedFig = fig;
                            break;
                        }
                    }

                    if (this.pre_clickedFig !== this.clickedFig) {
                        const ctx = canvas.getContext("2d");
                        if (ctx) {
                            ctx.clearRect(0, 0, canvas.width, canvas.height);
                            for (const sh of this.graph_figures) {
                                if (sh) {
                                    if (this.clickedFig === sh) {
                                        sh.draw_clicked(ctx);
                                    } else {
                                        sh.draw_canvas(ctx);
                                    }
                                }
                            }
                        }
                    }
                });
            }
        },
        watch: {
            // Отслеживаем изменение ширины
            width(newWidth, oldWidth) {
                if (newWidth !== oldWidth && newWidth !== undefined) {
                    this.handleCanvasSizeChange();
                }
            },

            // Отслеживаем изменение высоты
            height(newHeight, oldHeight) {
                if (newHeight !== oldHeight && newHeight !== undefined) {
                    this.handleCanvasSizeChange();
                }
            },

            // Автоматически перерисовываемся при получении новых данных
            xmlData: {
                handler(newXml) {
                    if (newXml) {
                        this.loadGraphFromXml(newXml);
                    }
                },
                immediate: true
            }
        },

        expose: [
            'loadGraphFromXml',
            'setDialect',
            'runGraphTests',
            'getNodes',
            'getEdges',
            'getObjectAt',
            'addNode',
            'addEdge',
            'deleteNode',
            'deleteEdge',
            'clearGraph',
            'getGraphData',
            'exportToXml',
            'updateGraphDataXML',
            'fetchGraphFromServer',
            'saveGraphToServer',
            'syncWithServer',
            'getSelection',
            'select',
            'deselect',
            'canSelect',
            'emphasize',
            'deEmphasize',
            'getAvailableEvents',
            'getAttachedEvents',
            'setStyle',
            'attachEvent',
            'clearSelection',
            'clearEmphasis',
            'selectWithMode',
            'deselectWithCheck',
            'getSelectedByType',
            'getSelectionCount',
            'setSelectionConstraint',
            'getSelectionConstraint',
            'selectMultiple',
            'clearSelectionByType',
            'setupSelectionConstraints'
        ]
    });
</script>

<style scoped>
    .graph {
        background: #f5f5f5;
    }

    .graph-container {
        position: relative;
        display: flex;
        justify-content: center;
        align-items: center;
        padding: 20px;
    }

    .tooltip {
        position: absolute;
        background-color: rgba(0,0,0,0.85);
        color: white;
        padding: 8px 12px;
        border-radius: 4px;
        font-family: Arial, sans-serif;
        font-size: 14px;
        line-height: 1.4;
        pointer-events: none;
        z-index: 1000;
        max-width: 300px;
        box-shadow: 0 2px 5px rgba(0,0,0,0.2);
        display: none;
        transition: opacity 0.2s ease;
    }
</style>
