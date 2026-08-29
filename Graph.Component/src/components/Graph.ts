export interface IShape {
    id: string
    type: string
    dialect?: string
    rotation?: number
    color?: string //background color

    info?: string
    wasInside?: boolean
    //linewidth
    label_info?: ILabel

    //is edge puctir
    isEdgeDash?: boolean

    connectors?: IConnector[]

    image_src?: string;
    image_scale?: number; 
    image_rotation?: number;
}

export interface IRectangle extends IShape {
    x: number
    y: number
    width: number
    height: number

    //connectors?: Array<{ id: string; x: number; y: number }>;
}

export interface ICircle extends IShape {
    x: number
    y: number
    radius: number

    //connectors?: Array<{ id: string; x: number; y: number }>;
}

export interface IRhombus extends IShape {
    x: number
    y: number
    width: number
    height: number

    //connectors?: Array<{ id: string; x: number; y: number }>;
}

export interface IEllipse extends IShape {
    x: number
    y: number
    radius_x: number
    radius_y: number

    //connectors?: Array<{ id: string; x: number; y: number }>;
}

export interface ITriangle extends IShape {
    x_1: number
    y_1: number
    x_2: number
    y_2: number
    x_3: number
    y_3: number

    //connectors?: Array<{ id: string; x: number; y: number }>;
}

export interface IRegularPolygon extends IShape {
    radius: number
    number_of_edges: number
    x: number // center?
    y: number

    //connectors?: Array<{ id: string; x: number; y: number }>;
}

//rename variables to be readable
export interface IStar extends IShape {
    rad: number;
    amount_points: number; 
    m: number;
    x_C: number;
    y_C: number;

    //connectors?: Array<{ id: string; x: number; y: number }>;
}

export interface ICloud extends IShape {
    width: number;
    height: number;
    x_C: number;
    y_C: number;

    //connectors?: Array<{ id: string; x: number; y: number }>;
}

export interface ICustomDescription {
    typeName: string;
    points: Array<{ x: number; y: number }>;
    curve: Array<{ isCurved: boolean; cp1x: number; cp1y: number; cp2x: number; cp2y: number; }>;
    //is edge puctir

    //connectors?: Array<{ id: string; x: number; y: number }>;
}

export interface ICustomShape extends IShape {
    x_center: number;
    y_center: number;

    //connectors?: Array<{ id: string; x: number; y: number }>;
}


export interface ILine extends IShape {
    startX: number
    startY: number
    endX: number
    endY: number
    color?: string
    //label_info?: ILabel
    lineWidth?: number

    points?: Array<{ x: number; y: number }>; //intermediate 
    is_corners_rounded?: boolean
    max_radius_of_corners?: number
    // is puctir
}

// length ???
export interface ILabel {
    //id: string
    text: string
    icon?: 'lock'
    startX?: number
    startY?: number
    endX?: number
    endY?: number
    color?: string
    font?: string

    padding?: number
    alignment?: string
    position?: string // above, under
}

export interface IConnector {
    id?: string
    x: number
    y: number
    parent_id?: string
    info?: string
    type?: string
}


// обновить
export type DataFigure = ILine | ICircle | IRectangle | ITriangle | IRegularPolygon | IShape | ICustomShape
export type DataShapes = Line | Circle | Rectangle | Triangle | RegularPolygon | CustomShape | Node | null

export type GraphItem = Node | Edge;

// ==================== ИНТЕРФЕЙСЫ ====================
export interface IStyleInfo {
    color?: string;
    fillColor?: string;
    lineWidth?: number;
    dashPattern?: number[];
    glow?: {
        color: string;
        radius: number;
        speed: number;
    };
    opacity?: number;
    fontSize?: number;
    fontFamily?: string;
}

export interface IEventInfo {
    type: 'mouse_hover' | 'click' | 'double_click' | 'mouse_down' | 'mouse_up' | 'selected' | 'emphasized';
    handler?: (item: GraphItem, event?: MouseEvent) => void;
}

export interface IAttachedEvent {
    item: GraphItem;
    eventInfo: IEventInfo;
    method: Function;
}

export interface ISelectionConstraint {
    type: string;
    maxSelection: number;
    selectionMode: 'single' | 'multiple' | 'range';
    allowDeselect: boolean;
    deselectMode: 'click' | 'ctrl+click' | 'context_menu' | 'never';
}

export interface ISelectionConfig {
    constraints: ISelectionConstraint[];
    globalMaxSelection: number;
    allowMixedSelection: boolean;
    autoDeselect: boolean;
}


// ==================== КЛАСС МЕНЕДЖЕРА ВЫБОРА ====================
export class SelectionManager {
    private _selectedItems: Map<string, GraphItem> = new Map();
    private _config: ISelectionConfig;
    private _typeCounts: Map<string, number> = new Map();

    constructor(config?: Partial<ISelectionConfig>) {
        this._config = {
            constraints: [],
            globalMaxSelection: 100,
            allowMixedSelection: true,
            autoDeselect: true,
            ...config
        };
    }

    getConstraintForType(type: string): ISelectionConstraint | undefined {
        return this._config.constraints.find(c => c.type === type);
    }

    canSelect(item: GraphItem): { can: boolean; reason?: string } {
        const constraint = this.getConstraintForType(item._type);
        const currentCount = this._typeCounts.get(item._type) || 0;

        if (this._selectedItems.size >= this._config.globalMaxSelection) {
            return {
                can: false,
                reason: `Global selection limit (${this._config.globalMaxSelection}) reached`
            };
        }

        if (constraint) {
            if (constraint.maxSelection <= 0) {
                return {
                    can: false,
                    reason: `Selection of type "${item._type}" is disabled`
                };
            }

            if (currentCount >= constraint.maxSelection) {
                if (constraint.selectionMode === 'single' && currentCount > 0) {
                    return {
                        can: false,
                        reason: `Only one "${item._type}" can be selected at a time`
                    };
                }

                if (!this._config.autoDeselect) {
                    return {
                        can: false,
                        reason: `Maximum ${constraint.maxSelection} "${item._type}" items allowed`
                    };
                }
            }
        }

        if (!this._config.allowMixedSelection && this._selectedItems.size > 0) {
            const firstItem = Array.from(this._selectedItems.values())[0];
            if (firstItem._type !== item._type) {
                return {
                    can: false,
                    reason: 'Mixed selection of different types is not allowed'
                };
            }
        }

        return { can: true };
    }

    select(item: GraphItem, force: boolean = false): boolean {
        if (this._selectedItems.has(item._id)) return true;

        const check = this.canSelect(item);

        if (!check.can && !force) {
            console.warn(`Cannot select item ${item._id}: ${check.reason}`);
            return false;
        }

        const constraint = this.getConstraintForType(item._type);
        if (constraint && this._config.autoDeselect) {
            const currentCount = this._typeCounts.get(item._type) || 0;
            if (currentCount >= constraint.maxSelection) {
                const itemsToRemove = Array.from(this._selectedItems.values())
                    .filter(i => i._type === item._type);

                if (itemsToRemove.length > 0) {
                    this.deselect(itemsToRemove[0]);
                }
            }
        }

        this._selectedItems.set(item._id, item);

        const newCount = (this._typeCounts.get(item._type) || 0) + 1;
        this._typeCounts.set(item._type, newCount);

        return true;
    }

    deselect(item: GraphItem): boolean {
        const constraint = this.getConstraintForType(item._type);

        if (constraint && !constraint.allowDeselect) {
            console.warn(`Cannot deselect item ${item._id}: deselection is disabled for type "${item._type}"`);
            return false;
        }

        if (this._selectedItems.delete(item._id)) {
            const newCount = (this._typeCounts.get(item._type) || 0) - 1;
            if (newCount > 0) {
                this._typeCounts.set(item._type, newCount);
            } else {
                this._typeCounts.delete(item._type);
            }
            return true;
        }
        return false;
    }

    clear(): void {
        this._selectedItems.clear();
        this._typeCounts.clear();
    }

    getSelectedItems(): GraphItem[] {
        return Array.from(this._selectedItems.values());
    }

    getSelectedByType(type: string): GraphItem[] {
        return Array.from(this._selectedItems.values())
            .filter(item => item._type === type);
    }

    getSelectionCount(type?: string): number {
        if (type) {
            return this._typeCounts.get(type) || 0;
        }
        return this._selectedItems.size;
    }

    isSelected(item: GraphItem): boolean {
        return this._selectedItems.has(item._id);
    }

    updateConfig(config: Partial<ISelectionConfig>): void {
        this._config = { ...this._config, ...config };
        this.applyConstraints();
    }

    private applyConstraints(): void {
        Array.from(this._selectedItems.values()).forEach(item => {
            const constraint = this.getConstraintForType(item._type);
            if (constraint) {
                const currentCount = this._typeCounts.get(item._type) || 0;
                if (currentCount > constraint.maxSelection) {
                    const itemsOfType = this.getSelectedByType(item._type);
                    const excess = itemsOfType.slice(constraint.maxSelection);
                    excess.forEach(excessItem => this.deselect(excessItem));
                }
            }
        });
    }
}

// TODO _selectionManager initial constrains
export class Graph {
    // Основные поля
    _dialect: string = "base"
    _nodes: Node[] = [];
    _edges: Edge[] = [];

    //labels: Label[] = []; // несвязанные с объектами
    //type: string//enum //для проверки и диалектов???
    //is_directed ???

    // Система перерисовки
    private _redrawScheduled = false;
    private _canvas: HTMLCanvasElement | null = null;
    private _ctx: CanvasRenderingContext2D | null = null;
    private _scale = 1;
    private _offsetX = 0;
    private _offsetY = 0;

    // Система выбора
    private _selectionManager: SelectionManager;

    // Система событий и стилей
    private _attachedEvents: Map<string, IEventInfo[]> = new Map();
    private _eventStyles: Map<string, Map<string, IStyleInfo>> = new Map();

    // Элементы с акцентом
    private _emphasizedItems: GraphItem[] = [];

    constructor(nodes?: Node[], edges?: Edge[], dialect?: string) {
        if (nodes) {
            this._nodes = nodes
        }
        if (edges) {
            this._edges = edges
        }
        if (dialect) {
            this._dialect = dialect
        }

        // Инициализация менеджера выбора
        this._selectionManager = new SelectionManager({
            constraints: [
                {
                    type: 'rectangle',
                    maxSelection: 1,
                    selectionMode: 'single',
                    allowDeselect: true,
                    deselectMode: 'click'
                },
                {
                    type: 'circle',
                    maxSelection: 2,
                    selectionMode: 'multiple',
                    allowDeselect: true,
                    deselectMode: 'ctrl+click'
                },
                {
                    type: 'triangle',
                    maxSelection: 3,
                    selectionMode: 'multiple',
                    allowDeselect: false,
                    deselectMode: 'never'
                }
                // ... другие типы по умолчанию

            ],
            globalMaxSelection: 20,
            allowMixedSelection: true,
            autoDeselect: true
        });
    }

    // ==================== ОСНОВНЫЕ МЕТОДЫ ====================
    bindCanvas(canvas: HTMLCanvasElement, ctx: CanvasRenderingContext2D) {
        this._canvas = canvas;
        this._ctx = ctx;
    }

    panBy(deltaX: number, deltaY: number): void {
        this._offsetX += deltaX;
        this._offsetY += deltaY;
        this.requestRedraw();
    }

    zoomAt(screenX: number, screenY: number, factor: number): void {
        const nextScale = Math.min(4, Math.max(0.2, this._scale * factor));
        const graphX = (screenX - this._offsetX) / this._scale;
        const graphY = (screenY - this._offsetY) / this._scale;

        this._scale = nextScale;
        this._offsetX = screenX - graphX * this._scale;
        this._offsetY = screenY - graphY * this._scale;
        this.requestRedraw();
    }

    resetViewport(): void {
        this._scale = 1;
        this._offsetX = 0;
        this._offsetY = 0;
        this.requestRedraw();
    }

    fitViewportToNodes(viewportWidth?: number, viewportHeight?: number, padding: number = 60): void {
        if (!this._canvas || this._nodes.length === 0) return;

        const bounds = this.getNodesBounds();
        if (!bounds) return;

        const width = viewportWidth || this._canvas.width;
        const height = viewportHeight || this._canvas.height;
        const contentWidth = Math.max(bounds.maxX - bounds.minX + padding * 2, 1);
        const contentHeight = Math.max(bounds.maxY - bounds.minY + padding * 2, 1);
        const fitScale = Math.min(1, width / contentWidth, height / contentHeight);
        const centerX = (bounds.minX + bounds.maxX) / 2;
        const centerY = (bounds.minY + bounds.maxY) / 2;

        this._scale = Math.min(4, Math.max(0.2, fitScale));
        this._offsetX = width / 2 - centerX * this._scale;
        this._offsetY = height / 2 - centerY * this._scale;
        this.requestRedraw();
    }

    private getNodesBounds(): { minX: number; minY: number; maxX: number; maxY: number } | null {
        let minX = Infinity;
        let minY = Infinity;
        let maxX = -Infinity;
        let maxY = -Infinity;

        for (const node of this._nodes) {
            const bounds = this.getNodeBounds(node);
            if (!bounds) continue;

            minX = Math.min(minX, bounds.minX);
            minY = Math.min(minY, bounds.minY);
            maxX = Math.max(maxX, bounds.maxX);
            maxY = Math.max(maxY, bounds.maxY);
        }

        return minX === Infinity
            ? null
            : { minX, minY, maxX, maxY };
    }

    private getNodeBounds(node: Node): { minX: number; minY: number; maxX: number; maxY: number } | null {
        const item = node as any;

        if (typeof item._radius === 'number' &&
            typeof item._x === 'number' &&
            typeof item._y === 'number') {
            return {
                minX: item._x - item._radius,
                minY: item._y - item._radius,
                maxX: item._x + item._radius,
                maxY: item._y + item._radius
            };
        }

        if (typeof item._radius_x === 'number' &&
            typeof item._radius_y === 'number' &&
            typeof item._x === 'number' &&
            typeof item._y === 'number') {
            return {
                minX: item._x - item._radius_x,
                minY: item._y - item._radius_y,
                maxX: item._x + item._radius_x,
                maxY: item._y + item._radius_y
            };
        }

        if (typeof item._width === 'number' &&
            typeof item._height === 'number' &&
            typeof item._x === 'number' &&
            typeof item._y === 'number') {
            if (item._type === 'rhomb') {
                return {
                    minX: item._x - item._width / 2,
                    minY: item._y - item._height / 2,
                    maxX: item._x + item._width / 2,
                    maxY: item._y + item._height / 2
                };
            }

            return {
                minX: item._x,
                minY: item._y,
                maxX: item._x + item._width,
                maxY: item._y + item._height
            };
        }

        if (typeof item._x_1 === 'number' &&
            typeof item._y_1 === 'number' &&
            typeof item._x_2 === 'number' &&
            typeof item._y_2 === 'number' &&
            typeof item._x_3 === 'number' &&
            typeof item._y_3 === 'number') {
            return {
                minX: Math.min(item._x_1, item._x_2, item._x_3),
                minY: Math.min(item._y_1, item._y_2, item._y_3),
                maxX: Math.max(item._x_1, item._x_2, item._x_3),
                maxY: Math.max(item._y_1, item._y_2, item._y_3)
            };
        }

        if (Array.isArray(item.points) && item.points.length > 0) {
            const offsetX = typeof item._x_center === 'number' ? item._x_center : 0;
            const offsetY = typeof item._y_center === 'number' ? item._y_center : 0;
            const points = item.points.map((point: { x: number; y: number }) => ({
                x: point.x + offsetX,
                y: point.y + offsetY
            }));

            return {
                minX: Math.min(...points.map((point: { x: number; y: number }) => point.x)),
                minY: Math.min(...points.map((point: { x: number; y: number }) => point.y)),
                maxX: Math.max(...points.map((point: { x: number; y: number }) => point.x)),
                maxY: Math.max(...points.map((point: { x: number; y: number }) => point.y))
            };
        }

        if (typeof item._rad === 'number' &&
            typeof item._x_C === 'number' &&
            typeof item._y_C === 'number') {
            return {
                minX: item._x_C - item._rad,
                minY: item._y_C - item._rad,
                maxX: item._x_C + item._rad,
                maxY: item._y_C + item._rad
            };
        }

        if (typeof item._width === 'number' &&
            typeof item._height === 'number' &&
            typeof item._x_C === 'number' &&
            typeof item._y_C === 'number') {
            return {
                minX: item._x_C - item._width / 2,
                minY: item._y_C - item._height / 2,
                maxX: item._x_C + item._width / 2,
                maxY: item._y_C + item._height / 2
            };
        }

        return null;
    }

    screenToGraph(x: number, y: number): { x: number; y: number } {
        return {
            x: (x - this._offsetX) / this._scale,
            y: (y - this._offsetY) / this._scale
        };
    }

    applyViewport(ctx: CanvasRenderingContext2D): void {
        ctx.setTransform(this._scale, 0, 0, this._scale, this._offsetX, this._offsetY);
    }

    requestRedraw(): void {
        if (!this._redrawScheduled && this._ctx) {
            this._redrawScheduled = true;
            requestAnimationFrame(() => {
                if (this._ctx) {
                    this.draw_all_canvas(this._ctx);
                }
                this._redrawScheduled = false;
            });
        }
    }

    addNode(node: Node) {
        for (let i = 0; i < this._nodes.length; i++) {
            if (this._nodes[i]._id === node._id) {
                return
            }
        }
        node.attachToGraph(this);
        this._nodes.push(node)
    }

    addEdge(edge: Edge) {
        for (let i = 0; i < this._edges.length; i++) {
            if (this._edges[i]._id === edge._id) {
                return
            }
        }
        edge.attachToGraph(this);
        this._edges.push(edge)
    }

    getNode(node_id: string): Node | undefined {
        for (let i = 0; i < this._nodes.length; i++) {
            if (this._nodes[i]._id === node_id) {
                return this._nodes[i];
            }
        }
        return undefined;
    }

    getEdge(edge_id: string): Edge | undefined {
        for (let i = 0; i < this._edges.length; i++) {
            if (this._edges[i]._id === edge_id) {
                return this._edges[i];
            }
        }
        return undefined;
    }

    draw_all_canvas(ctx: CanvasRenderingContext2D) {
        if (!this._ctx || !this._canvas) return;

        ctx.setTransform(1, 0, 0, 1, 0, 0);
        ctx.clearRect(0, 0, this._canvas.width, this._canvas.height);
        ctx.save();
        this.applyViewport(ctx);

        for (const edge of this._edges) {
            if (edge) {
                edge.draw_canvas(ctx)
            }
        }

        for (const node of this._nodes) {
            if (node) {
                node.draw_canvas(ctx)
            }
        }

        ctx.restore();
    }

    // ==================== API ВЫБОРА (SELECTION) ====================
    getSelection(): GraphItem[] {
        return this._selectionManager.getSelectedItems();
    }

    select(item: GraphItem, mode: 'toggle' | 'add' | 'replace' = 'toggle'): boolean {
        if (!this.canSelect(item)) return false;

        const isSelected = this._selectionManager.isSelected(item);

        switch (mode) {
            case 'toggle': {
                if (isSelected) {
                    return this.deselect(item);
                } else {
                    return this._selectionManager.select(item);
                }
            }

            case 'add': {
                if (!isSelected) {
                    return this._selectionManager.select(item);
                }
                return true;
            }

            case 'replace': {
                this._selectionManager.clear();
                return this._selectionManager.select(item);
            }
        }

        return false;
    }

    deselect(item: GraphItem): boolean {
        return this._selectionManager.deselect(item);
    }

    canSelect(item: GraphItem): boolean {
        if (item._graph !== this) return false;
        return this._selectionManager.canSelect(item).can;
    }

    // Конфигурация выбора
    setSelectionConstraint(constraint: ISelectionConstraint): void {
        const constraints = this._selectionManager['_config'].constraints;
        const index = constraints.findIndex(c => c.type === constraint.type);

        if (index >= 0) {
            constraints[index] = constraint;
        } else {
            constraints.push(constraint);
        }

        this._selectionManager.updateConfig({ constraints });
    }

    getSelectionConstraint(type: string): ISelectionConstraint | undefined {
        return this._selectionManager.getConstraintForType(type);
    }

    getSelectedByType(type: string): GraphItem[] {
        return this._selectionManager.getSelectedByType(type);
    }

    getSelectionCount(type?: string): number {
        return this._selectionManager.getSelectionCount(type);
    }

    isItemSelected(item: GraphItem): boolean {
        return this._selectionManager.isSelected(item);
    }

    // Множественный выбор
    selectMultiple(items: GraphItem[], clearExisting: boolean = true): number {
        if (clearExisting) {
            this._selectionManager.clear();
        }

        let successCount = 0;
        items.forEach(item => {
            if (this.select(item, 'add')) {
                successCount++;
            }
        });

        this.requestRedraw();
        return successCount;
    }

    clearSelectionByType(type?: string): void {
        if (type) {
            const itemsToDeselect = this.getSelectedByType(type);
            itemsToDeselect.forEach(item => this.deselect(item));
        } else {
            this._selectionManager.clear();
        }

        this.requestRedraw();
    }

    // Обработка кликов
    handleClick(item: GraphItem, event: MouseEvent): void {
        if (!this.canSelect(item)) return;

        const constraint = this.getSelectionConstraint(item._type);
        let deselectMode = false;

        if (constraint) {
            switch (constraint.deselectMode) {
                case 'ctrl+click':
                    deselectMode = event.ctrlKey || event.metaKey;
                    break;
                case 'context_menu':
                    return; // Отдельная обработка через контекстное меню
                case 'click':
                    deselectMode = true;
                    break;
                case 'never':
                    deselectMode = false;
                    break;
            }
        }

        const isSelected = this._selectionManager.isSelected(item);

        if (isSelected && deselectMode) {
            this.deselect(item);
        } else {
            let mode: 'toggle' | 'add' | 'replace' = 'toggle';

            if (event.ctrlKey || event.metaKey) {
                mode = 'add';
            } else if (event.shiftKey) {
                // TODO: Реализовать выбор диапазона
                mode = 'add';
            }

            this.select(item, mode);
        }

        this.requestRedraw();
    }

    // ==================== API АКЦЕНТА (EMPHASIZE) ====================
    emphasize(item: GraphItem): void {
        if (!this._emphasizedItems.includes(item)) {
            this._emphasizedItems.push(item);

            // Вызываем метод emphasize у элемента
            if ('emphasize' in item && typeof item.emphasize === 'function') {
                item.emphasize();
            }

            this.requestRedraw();
        }
    }

    deEmphasize(item: GraphItem): void {
        const index = this._emphasizedItems.indexOf(item);
        if (index > -1) {
            this._emphasizedItems.splice(index, 1);

            // Вызываем метод deEmphasize у элемента
            if ('deEmphasize' in item && typeof item.deEmphasize === 'function') {
                item.deEmphasize();
            }

            this.requestRedraw();
        }
    }

    clearEmphasis(): void {
        this._emphasizedItems.forEach(item => {
            if ('deEmphasize' in item && typeof item.deEmphasize === 'function') {
                item.deEmphasize();
            }
        });
        this._emphasizedItems = [];
        this.requestRedraw();
    }

    isEmphasized(item: GraphItem): boolean {
        return this._emphasizedItems.includes(item);
    }

    // ==================== API СОБЫТИЙ (EVENTS) ====================
    getAvailableEvents(item: GraphItem): string[] {
        return ['mouse_hover', 'click', 'double_click', 'mouse_down', 'mouse_up', 'selected', 'emphasized'];
    }

    getAttachedEvents(): IAttachedEvent[] {
        const events: IAttachedEvent[] = [];
        this._attachedEvents.forEach((eventInfos, itemId) => {
            const item = this.getNode(itemId) || this.getEdge(itemId);
            if (item) {
                eventInfos.forEach(eventInfo => {
                    events.push({
                        item,
                        eventInfo,
                        method: eventInfo.handler || (() => { })
                    });
                });
            }
        });
        return events;
    }

    attachEvent(item: GraphItem, eventInfo: IEventInfo): void {
        let itemEvents = this._attachedEvents.get(item._id);
        if (!itemEvents) {
            itemEvents = [];
            this._attachedEvents.set(item._id, itemEvents);
        }
        itemEvents.push(eventInfo);
    }

    removeEvent(item: GraphItem, eventType: string): void {
        const itemEvents = this._attachedEvents.get(item._id);
        if (itemEvents) {
            const index = itemEvents.findIndex(e => e.type === eventType);
            if (index > -1) {
                itemEvents.splice(index, 1);
            }
        }
    }

    // ==================== API СТИЛЕЙ (STYLES) ====================
    setStyle(item: GraphItem, eventType: string, styleInfo: IStyleInfo): void {
        let itemStyles = this._eventStyles.get(item._id);
        if (!itemStyles) {
            itemStyles = new Map();
            this._eventStyles.set(item._id, itemStyles);
        }
        itemStyles.set(eventType, styleInfo);
    }

    getEventStyle(itemId: string, eventType: string): IStyleInfo | undefined {
        const itemStyles = this._eventStyles.get(itemId);
        return itemStyles?.get(eventType);
    }

    getEventStyles(itemId: string): Map<string, IStyleInfo> | undefined {
        return this._eventStyles.get(itemId);
    }

    removeStyle(item: GraphItem, eventType: string): void {
        const itemStyles = this._eventStyles.get(item._id);
        if (itemStyles) {
            itemStyles.delete(eventType);
        }
    }

    // ==================== ВСПОМОГАТЕЛЬНЫЕ МЕТОДЫ ====================
    // Публичный доступ к менеджеру выбора (только для чтения)
    get selectionManager(): SelectionManager {
        return this._selectionManager;
    }

    // Получить все элементы графа
    getAllItems(): GraphItem[] {
        return [...this._nodes, ...this._edges];
    }

    // Найти элемент по координатам
    getItemAt(x: number, y: number): GraphItem | null {
        // Проверяем edges сначала (они поверх nodes)
        for (let i = this._edges.length - 1; i >= 0; i--) {
            const edge = this._edges[i];
            if (edge && edge.is_inside(x, y)) {
                return edge;
            }
        }

        // Проверяем nodes
        for (let i = this._nodes.length - 1; i >= 0; i--) {
            const node = this._nodes[i];
            if (node && node.is_inside(x, y)) {
                return node;
            }
        }

        return null;
    }

    // Удаление элементов
    removeNode(nodeId: string): boolean {
        const initialLength = this._nodes.length;
        this._nodes = this._nodes.filter(n => n._id !== nodeId);

        // Удаляем связанные события и стили
        this._attachedEvents.delete(nodeId);
        this._eventStyles.delete(nodeId);

        // Снимаем выделение если элемент был выбран
        const node = this.getNode(nodeId);
        if (node) {
            this.deselect(node);
            this.deEmphasize(node);
        }

        // Удаляем связанные ребра
        this._edges = this._edges.filter(edge => {
            const lineEdge = edge as Line;
            return lineEdge._source?._id !== nodeId && lineEdge._target?._id !== nodeId;
        });

        return this._nodes.length < initialLength;
    }

    removeEdge(edgeId: string): boolean {
        const initialLength = this._edges.length;
        this._edges = this._edges.filter(e => e._id !== edgeId);

        // Удаляем связанные события и стили
        this._attachedEvents.delete(edgeId);
        this._eventStyles.delete(edgeId);

        // Снимаем выделение если элемент был выбран
        const edge = this.getEdge(edgeId);
        if (edge) {
            this.deselect(edge);
            this.deEmphasize(edge);
        }

        return this._edges.length < initialLength;
    }

    // Очистка всего графа
    clear(): void {
        this._nodes = [];
        this._edges = [];
        this._selectionManager.clear();
        this._emphasizedItems = [];
        this._attachedEvents.clear();
        this._eventStyles.clear();
        this.requestRedraw();
    }
}

// ==================== БАЗОВЫЙ КЛАСС ДЛЯ ВСЕХ ЭЛЕМЕНТОВ ====================

export abstract class GraphItemBase {
    abstract _id: string;
    abstract _type: string;
    abstract _graph: Graph | null;

    // Поля для стилей и состояний
    _originalColor?: string;
    _isSelected: boolean = false;
    _isGlowing: boolean = false;
    _isEmphasized: boolean = false;
    _defaultStyle: any = {};
    _currentStyle: any = {};

    // Методы для работы со стилями
    abstract saveDefaultStyle(): void;
    abstract applyStyle(style: IStyleInfo): void;
    abstract resetStyle(): void;

    // Методы для выделения
    abstract select(): void;
    abstract deselect(): void;

    // Методы для акцента
    abstract emphasize(): void;
    abstract deEmphasize(): void;
}

export abstract class Node extends GraphItemBase  {
    abstract _id: string;
    abstract _type: string;
    abstract _dialect?: string;
    abstract _rotation?: number;
    abstract _color?: string;
    abstract _info?: string;
    abstract _wasInside: boolean;
    abstract _label_info?: ILabel;

    abstract _connectors?: IConnector[]

    //abstract _connectors?: Array<{ id: string; x: number; y: number }>;

    abstract _graph: Graph | null;

    // Привязываем ноду к графу
    abstract attachToGraph(graph: Graph): void;

    abstract draw_canvas(ctx: CanvasRenderingContext2D): void;

    abstract draw_hovered(ctx: CanvasRenderingContext2D): void;

    abstract draw_clicked(ctx: CanvasRenderingContext2D): void;

    abstract is_inside(mouseX: number, mouseY: number): boolean;

    //abstract get id(): string;

    // create basic connectors

    // Реализация методов GraphItemBase
    saveDefaultStyle(): void {
        this._defaultStyle = {
            color: this._color,
            rotation: this._rotation,
            // другие свойства по необходимости
        };
    }

    applyStyle(style: IStyleInfo): void {
        this._currentStyle = { ...style };

        if (!this._originalColor) {
            this._originalColor = this._color;
        }

        if (style.color) {
            this._color = style.color;
        }

        if (style.fillColor) {
            this._color = style.fillColor;
        }
    }

    resetStyle(): void {
        if (this._originalColor) {
            this._color = this._originalColor;
            this._originalColor = undefined;
        }

        this._currentStyle = {};
    }

    select(): void {
        this._isSelected = true;
        const style = this._graph?.getEventStyle(this._id, 'selected');
        if (style) {
            this.applyStyle(style);
        }
    }

    deselect(): void {
        this._isSelected = false;
        this.resetStyle();
    }

    emphasize(): void {
        this._isEmphasized = true;
        const style = this._graph?.getEventStyle(this._id, 'emphasized');
        if (style) {
            this.applyStyle(style);
        }
    }

    deEmphasize(): void {
        this._isEmphasized = false;
        this.resetStyle();
    }

    // Метод для обработки кликов с учетом системы ограничений
    handleSelection(mode: 'select' | 'deselect' | 'toggle', event?: MouseEvent): void {
        if (!this._graph) return;

        switch (mode) {
            case 'select': {
                this._graph.select(this, 'add');
                break;
            }

            case 'deselect': {
                this._graph.deselect(this);
                break;
            }

            case 'toggle': {
                const constraint = this._graph.getSelectionConstraint(this._type);
                let shouldDeselect = false;

                if (constraint && event) {
                    switch (constraint.deselectMode) {
                        case 'click':
                            shouldDeselect = true;
                            break;
                        case 'ctrl+click':
                            shouldDeselect = event.ctrlKey || event.metaKey;
                            break;
                    }
                }

                if (shouldDeselect && this._graph.isItemSelected(this)) {
                    this._graph.deselect(this);
                } else {
                    this._graph.select(this, 'toggle');
                }
                break;
            }
        }
    }
}

// rotation for label
export class Rectangle extends Node implements IRectangle  {
    _id: string;
    _type: string = 'rectangle';
    _dialect: string = 'base';
    _rotation?: number = 0;
    _x: number;
    _y: number;
    _width: number;
    _height: number;
    _color?: string;
    _info?: string;
    _wasInside: boolean = false;

    _isEdgeDash: boolean = false;

    _label_info?: ILabel;
    _label?: Label;

    //_connectors?: Array<{ id: string; x: number; y: number }>; 

    _connectors?: IConnector[] = []

    glowRadius: number = 0;
    glowSpeed: number = 1;
    glowMaxRadius: number = 6;

    isGlowing: boolean = false

    _image?: HTMLImageElement | null;
    _image_src?: string;
    _image_loaded: boolean = false;
    _image_scale: number = 1; //standart = 1
    _image_rotation: number = 0; // standart = 0
    //imageCache: { [key: string]: HTMLImageElement } = {};

    _graph: Graph | null = null;

    // Привязываем ноду к графу
    attachToGraph(graph: Graph) {
        this._graph = graph;
    }

    //constructor(x: number, y: number, width: number, height: number, color?: string, rotation?: number, info?: string) {
    constructor(rect: IRectangle) {
        super();
        this._id = rect.id
        //this.type = type
        if (typeof rect.rotation !== 'undefined') {
            this._rotation = rect.rotation
        }
        this._x = rect.x
        this._y = rect.y
        this._width = rect.width
        this._height = rect.height
        this._color = rect.color
        this._info = rect.info

        if (typeof rect.isEdgeDash !== 'undefined') {
            this._isEdgeDash = rect.isEdgeDash
        }

        if (typeof rect.connectors !== 'undefined') {
            this._connectors = rect.connectors.slice()
        }

        if (typeof rect.label_info !== 'undefined') {
            this._label_info = rect.label_info

            if (typeof this._label_info.startX === 'undefined') {
                this._label_info.startX = this._x
            }

            if (typeof this._label_info.startY === 'undefined') {
                this._label_info.startY = this._y + this._height / 2
            }

            if (typeof this._label_info.endX === 'undefined') {
                this._label_info.endX = this._x + this._width
            }

            if (typeof this._label_info.endY === 'undefined') {
                this._label_info.endY = this._y + this._height / 2
            }

            this._label = new Label(this._label_info)
        }

        if (rect.image_src) {
            this._image_src = rect.image_src

            if (rect.image_rotation) {
                this._image_rotation = rect.image_rotation
            }

            if (rect.image_scale) {
                this._image_scale = rect.image_scale
            }
        }
    }

    private _loadImage() {
        if (!this._image_src) return;

        this._image = new Image();
        this._image.src = this._image_src;

        this._image.onload = () => {
            this._image_loaded = true;
            if (this._graph) {
                this._graph.requestRedraw();
            }
        };

        this._image.onerror = () => {
            console.error(`Ошибка загрузки изображения: ${this._image_src}`);
            this._image = null;
        };
    }

    setImageSource(src: string) {
        if (src === this._image_src) return;

        this._image_src = src;
        this._image = null;
        this._image_loaded = false;

        if (this._graph) {
            this._graph.requestRedraw();
        }
    }

    draw_canvas(ctx: CanvasRenderingContext2D) {
        if (!ctx) return;

        ctx.save();

        // Применяем стили выбора если элемент выбран
        if (this._graph && this._graph['_selectionManager'].isSelected(this)) {
            this.applySelectionStyle(ctx);
        }

        const radian = this.rotation! * Math.PI / 180;

        ctx.translate(this.x + this.width / 2, this.y + this.height / 2);
        ctx.rotate(radian);

        if (this._isEdgeDash) {
            ctx.setLineDash([5, 3]); // Длина штриха и промежутка 
        }

        if (typeof this.color !== 'undefined' && this.color.length > 0) {
            ctx.fillStyle = this.color!;
            ctx.fillRect(this.width / 2 * (-1), this.height / 2 * (-1), this.width, this.height)
            ctx.strokeRect(this.width / 2 * (-1), this.height / 2 * (-1), this.width, this.height);

        } else {
            //ctx.setLineDash([6]); // future: dashed line
            ctx.strokeRect(this.width / 2 * (-1), this.height / 2 * (-1), this.width, this.height);
        }

        if (this._isEdgeDash) {
            ctx.setLineDash([]); // Сбрасываем пунктир 
        }

        if (typeof this._image_src !== 'undefined') {
            
            ctx.save();

            // Устанавливаем положение и размеры прямоугольника
            const rectX = -this.width / 2;
            const rectY = -this.height / 2;

            // Рисуем прямоугольник для обрезки
            ctx.beginPath();
            ctx.rect(rectX, rectY, this.width, this.height);
            ctx.clip();

            if (this._image_loaded && this._image) {
                const scale = Math.min(
                    this.width / this._image.width,
                    this.height / this._image.height
                );
                const width = this._image.width * scale * this._image_scale;
                const height = this._image.height * scale * this._image_scale;

                ctx.rotate(this._image_rotation * Math.PI / 180); 

                // Рисуем изображение в центре прямоугольника
                ctx.drawImage(this._image, rectX + (this.width - width) / 2, rectY + (this.height - height) / 2, width, height);
            } else {
                if (!this._image) {
                    this._loadImage();
                }

                // Рисуем заглушку на время загрузки
                ctx.fillStyle = "#CCCCCC";
                ctx.fillRect(rectX, rectY, this.width, this.height);
            }

            ctx.restore();
        }

        ctx.restore();

        //for now
        if (typeof this._label !== 'undefined') {
            this._label.draw_canvas(ctx)
        }

    }

    private applySelectionStyle(ctx: CanvasRenderingContext2D): void {
        const constraint = this._graph?.getSelectionConstraint(this._type);
        const selectionCount = this._graph?.getSelectionCount(this._type) || 0;
        const maxSelection = constraint?.maxSelection || 1;

        // Разные стили в зависимости от того, сколько элементов выбрано
        ctx.strokeStyle = '#007bff'; // Синий по умолчанию

        if (selectionCount >= maxSelection && maxSelection > 0) {
            // Если достигнут лимит - красная рамка
            ctx.strokeStyle = '#dc3545';
            ctx.lineWidth = 3;
        } else if (selectionCount > 1) {
            // Если выбрано несколько - зеленая рамка
            ctx.strokeStyle = '#28a745';
            ctx.lineWidth = 2;
        } else {
            // Если выбран один - синяя рамка
            ctx.strokeStyle = '#007bff';
            ctx.lineWidth = 2;
        }

        // Рисуем рамку выбора
        ctx.setLineDash([]);
        ctx.strokeRect(this.x - 3, this.y - 3, this.width + 6, this.height + 6);

        // Если нельзя снимать выделение - рисуем замок
        if (constraint && !constraint.allowDeselect) {
            ctx.fillStyle = '#ffc107';
            ctx.fillRect(this.x + this.width - 15, this.y + 5, 10, 10);
        }
    }

    draw_hovered(ctx: CanvasRenderingContext2D) {


        if (!this.isGlowing) {
            this.glowRadius = 0;
            return;
        }
        //const gradient = ctx.createLinearGradient(
        //    this.x - this.glowRadius,
        //    this.y - this.glowRadius,
        //    this.x + this.width + this.glowRadius,
        //    this.y + this.height + this.glowRadius
        //);

        //gradient.addColorStop(0, 'rgba(0, 0, 255, 1)');
        //gradient.addColorStop(1, 'rgba(102, 217, 255, 0)');

        //// Рисуем эффект подсветки
        //ctx.fillStyle = gradient;
        //ctx.fillRect(this.x - this.glowRadius, this.y - this.glowRadius, this.width + this.glowRadius * 2, this.height + this.glowRadius * 2);


        // Рисуем эффект подсветки
        for (let i = 0; i < this.glowRadius; i += 1) {
            const alpha = 1 - i / this.glowRadius;

            //const alpha = Math.sin((i / this.glowRadius) * Math.PI); 
            ctx.beginPath();
            ctx.rect(this.x - i, this.y - i, this.width + i * 2, this.height + i * 2);
            ctx.fillStyle = `rgba(0, 0, 255, ${alpha})`;
            ctx.fill();
        }

        // Увеличиваем радиус подсветки
        this.glowRadius += this.glowSpeed;

        this.draw_canvas(ctx)

        // Если радиус подсветки превышает максимальный, сбрасываем его
        if (this.glowRadius > this.glowMaxRadius) {
            this.glowRadius = 0;
            return;
        }



        // Запускаем следующий кадр анимации
        //requestAnimationFrame(this.draw_hovered(ctx));
        requestAnimationFrame(() => this.draw_hovered(ctx));

    }

    draw_clicked(ctx: CanvasRenderingContext2D) {
        if (!ctx) return;

        ctx.save();

        const radian = this.rotation! * Math.PI / 180;

        ctx.translate(this.x + this.width / 2, this.y + this.height / 2);
        ctx.rotate(radian);

        ctx.fillStyle = '#b57281';
        ctx.fillRect(this.width / 2 * (-1), this.height / 2 * (-1), this.width, this.height)

        ctx.restore();

        //for now
        if (typeof this._label !== 'undefined') {
            this._label.draw_canvas(ctx)
        }
    }

    is_inside(mouseX: number, mouseY: number) {
        const is_inside = (
            mouseX >= this.x &&
            mouseX <= this.x + this.width &&
            mouseY >= this.y &&
            mouseY <= this.y + this.height
        )

        if (this.wasInside && !is_inside) {
            this.isGlowing = false
            this._wasInside = false
        }

        if (!this.wasInside && is_inside) {
            this.isGlowing = true
            this._wasInside = true
        }

        return (
            mouseX >= this.x &&
            mouseX <= this.x + this.width &&
            mouseY >= this.y &&
            mouseY <= this.y + this.height
        )
    }

    is_clicked(mouseX: number, mouseY: number) {
        const is_inside = (
            mouseX >= this.x &&
            mouseX <= this.x + this.width &&
            mouseY >= this.y &&
            mouseY <= this.y + this.height
        )

        if (is_inside) {
            this.isGlowing = false
        }

        return (
            mouseX >= this.x &&
            mouseX <= this.x + this.width &&
            mouseY >= this.y &&
            mouseY <= this.y + this.height
        )
    }

    get type() {
        return this._type;
    }

    get rotation() {
        return this._rotation;
    }

    get x() {
        return this._x;
    }

    get y() {
        return this._y;
    }

    get width() {
        return this._width;
    }

    get height() {
        return this._height;
    }

    get color() {
        return this._color;
    }

    get info() {
        return this._info;
    }

    get wasInside() {
        return this._wasInside;
    }

    get id() {
        return this._id;
    }

    get connectors() {
        return this._connectors;
    }
}

// rotation for label
export class Circle extends Node implements ICircle {
    _id: string;
    _type: string = 'circle';
    _dialect: string = 'base';
    _rotation?: number;
    _x: number;
    _y: number;
    _radius: number;
    _color?: string;

    _isEdgeDash: boolean = false;

    _label_info?: ILabel;
    _label?: Label;

    _connectors?: IConnector[] = []

    _info?: string;
    _wasInside: boolean = false;

    _image?: HTMLImageElement | null;
    _image_src?: string;
    _image_loaded: boolean = false;
    _image_scale: number = 1; //standart = 1
    _image_rotation: number = 0; // standart = 0
    //imageCache: { [key: string]: HTMLImageElement } = {};

    glowRadius: number = 0;
    glowSpeed: number = 2;
    glowMaxRadius: number = 10;

    isGlowing: boolean = false

    _graph: Graph | null = null;

    // Привязываем ноду к графу
    attachToGraph(graph: Graph) {
        this._graph = graph;
    }

    //constructor(x: number, y: number, radius: number, color?: string, rotation?: number, info?: string) {
    //TODO: label position fix/change
    constructor(circle: ICircle) {
        super();
        this._id = circle.id
        //this.type = type
        //if (type != 'circle') return;
        if (typeof circle.rotation !== 'undefined') {
            this._rotation = circle.rotation
        }
        this._x = circle.x
        this._y = circle.y
        this._radius = circle.radius
        this._color = circle.color
        this._info = circle.info

        if (typeof circle.isEdgeDash !== 'undefined') {
            this._isEdgeDash = circle.isEdgeDash
        }

        if (typeof circle.connectors !== 'undefined') {
            this._connectors = circle.connectors.slice()
        }

        if (typeof circle.label_info !== 'undefined') {
            this._label_info = circle.label_info

            if (typeof this._label_info.startX === 'undefined') {
                this._label_info.startX = this._x - this._radius

                if (this._label_info.position && this._label_info.position === 'right') {
                    this._label_info.startX += (this._label_info.text.length + 10)
                } else if (this._label_info.position && this._label_info.position === 'left') {
                    this._label_info.startX -= (this._label_info.text.length + 10)
                }
            }

            if (typeof this._label_info.startY === 'undefined') {
                this._label_info.startY = this._y
                if (this._label_info.position && this._label_info.position === 'under') {
                    this._label_info.startY += (this._radius + 10)
                } else if (this._label_info.position && this._label_info.position === 'above') {
                    this._label_info.startY -= (this._radius + 10)
                }
            }

            if (typeof this._label_info.endX === 'undefined') {
                this._label_info.endX = this._x + this._radius

                if (this._label_info.position && this._label_info.position === 'right') {
                    this._label_info.endX += (this._label_info.text.length + 10)
                } else if (this._label_info.position && this._label_info.position === 'left') {
                    this._label_info.endX -= (this._label_info.text.length + 10)
                }
            }

            if (typeof this._label_info.endY === 'undefined') {
                this._label_info.endY = this._y
                if (this._label_info.position && this._label_info.position === 'under') {
                    this._label_info.endY += (this._radius + 10)
                } else if (this._label_info.position && this._label_info.position === 'above') {
                    this._label_info.endY -= (this._radius + 10)
                }
            }

            this._label = new Label(this._label_info)

        }

        if (circle.image_src) {
            this._image_src = circle.image_src

            if (circle.image_rotation) {
                this._image_rotation = circle.image_rotation
            }

            if (circle.image_scale) {
                this._image_scale = circle.image_scale
            }
        }

    }

    private _loadImage() {
        if (!this._image_src) return;

        this._image = new Image();
        this._image.src = this._image_src;

        this._image.onload = () => {
            this._image_loaded = true;
            if (this._graph) {
                this._graph.requestRedraw();
            }
        };

        this._image.onerror = () => {
            console.error(`Ошибка загрузки изображения: ${this._image_src}`);
            this._image = null;
        };
    }

    setImageSource(src: string) {
        if (src === this._image_src) return;

        this._image_src = src;
        this._image = null;
        this._image_loaded = false;

        if (this._graph) {
            this._graph.requestRedraw();
        }
    }


    draw_canvas(ctx: CanvasRenderingContext2D) {
        if (!ctx) return;

        const radian = this.rotation! * Math.PI / 180;

        ctx.save();
        ctx.translate(this.x, this.y);
        ctx.rotate(radian);

        if (this._isEdgeDash) {
            ctx.setLineDash([5, 3]);
        }

        ctx.beginPath();
        ctx.arc(0, 0, this.radius, 0, 2 * Math.PI)
        ctx.stroke();
        if (typeof this.color !== 'undefined' && this.color.length > 0) {
            ctx.fillStyle = this.color!;
            ctx.fill();
        }

        if (this._isEdgeDash) {
            ctx.setLineDash([]);
        }


        if (typeof this._image_src !== 'undefined') {
            // Сохраняем состояние контекста перед обрезкой
            ctx.save();

            ctx.beginPath();
            ctx.arc(0, 0, this.radius, 0, 2 * Math.PI);
            ctx.clip(); 

            if (this._image_loaded && this._image) {
                const scale = Math.min(
                    (this.radius * 2) / this._image.width,
                    (this.radius * 2) / this._image.height
                );
                const width = this._image.width * scale * this._image_scale;
                const height = this._image.height * scale * this._image_scale;

                ctx.rotate(this._image_rotation * Math.PI / 180); 

                ctx.drawImage(this._image, -width / 2, -height / 2, width, height);
            } else {
                if (!this._image) {
                    this._loadImage();
                }               

                // Рисуем заглушку на время загрузки
                ctx.fillStyle = "#CCCCCC";
                ctx.fillRect(-this.radius, -this.radius, this.radius * 2, this.radius * 2);
            }

            ctx.restore();
        }

        ctx.restore();

        //for now
        if (typeof this._label !== 'undefined') {
            this._label.draw_canvas(ctx)
        }
    }

    draw_hovered(ctx: CanvasRenderingContext2D) {
        

        if (!this.isGlowing) {
            this.glowRadius = 0;
            return;
        }

        const gradient = ctx.createRadialGradient(
            this.x,
            this.y,
            this.radius,
            this.x,
            this.y,
            this.radius + this.glowRadius
        );
        gradient.addColorStop(0, 'rgba(0, 0, 255, 1)');
        gradient.addColorStop(1, 'rgba(102, 217, 255, 0)');

        // Рисуем эффект подсветки
        ctx.beginPath();
        ctx.arc(this.x, this.y, this.radius + this.glowRadius, 0, 2 * Math.PI);
        //ctx.fillStyle = `rgba(0, 0, 255, ${(this.glowMaxRadius - this.glowRadius) / this.glowMaxRadius})`;
        ctx.fillStyle = gradient;
        ctx.fill();

        // Увеличиваем радиус подсветки
        this.glowRadius += this.glowSpeed;

        this.draw_canvas(ctx)

        // Если радиус подсветки превышает максимальный, сбрасываем его
        if (this.glowRadius > this.glowMaxRadius) {
            this.glowRadius = 0;
            return;
        }



        // Запускаем следующий кадр анимации
        //requestAnimationFrame(this.draw_hovered(ctx));
        requestAnimationFrame(() => this.draw_hovered(ctx));

    }


    draw_clicked(ctx: CanvasRenderingContext2D) {
        if (!ctx) return;

        const radian = this.rotation! * Math.PI / 180;

        ctx.save();
        ctx.translate(this.x, this.y);
        ctx.rotate(radian);

        ctx.beginPath();
        ctx.arc(0, 0, this.radius, 0, 2 * Math.PI)
        ctx.stroke();
        //if (typeof this.color !== 'undefined' && this.color.length > 0) {
        //    ctx.fillStyle = this.color!;
        //    ctx.fill();
        //}

        ctx.fillStyle = '#b57281';
        ctx.fill();

        ctx.closePath();
        ctx.restore();

        //for now
        if (typeof this._label !== 'undefined') {
            this._label.draw_canvas(ctx)
        }
    }

    is_inside(mouseX: number, mouseY: number) {
        const is_inside = (
            mouseX >= this.x - this.radius &&
            mouseX <= this.x + this.radius &&
            mouseY >= this.y - this.radius &&
            mouseY <= this.y + this.radius
        )

        if (this.wasInside && !is_inside) {
            this.isGlowing = false
            this._wasInside = false
        }

        if (!this.wasInside && is_inside) {
            this.isGlowing = true
            this._wasInside = true
        }

        return (
            mouseX >= this.x - this.radius &&
            mouseX <= this.x + this.radius &&
            mouseY >= this.y - this.radius &&
            mouseY <= this.y + this.radius
        )
    }

    is_clicked(mouseX: number, mouseY: number) {
        const is_inside = (
            mouseX >= this.x - this.radius &&
            mouseX <= this.x + this.radius &&
            mouseY >= this.y - this.radius &&
            mouseY <= this.y + this.radius
        )

        if (is_inside) {
            this.isGlowing = false
        }

        return (
            mouseX >= this.x - this.radius &&
            mouseX <= this.x + this.radius &&
            mouseY >= this.y - this.radius &&
            mouseY <= this.y + this.radius
        )
    }

    get id() {
        return this._id;
    }

    get type() {
        return this._type;
    }

    get rotation() {
        return this._rotation;
    }

    get x() {
        return this._x;
    }

    get y() {
        return this._y;
    }

    get radius() {
        return this._radius;
    }

    get color() {
        return this._color;
    }

    get info() {
        return this._info;
    }

    get wasInside() {
        return this._wasInside;
    }

    get connectors() {
        return this._connectors;
    }
}

// rotation for label
export class Ellipse extends Node implements IEllipse {
    _id: string;
    _type: string = 'ellipse';
    _dialect: string = 'base';
    _rotation: number = 0;
    _x: number;
    _y: number;
    _radius_x: number;
    _radius_y: number;
    _color?: string;

    _isEdgeDash: boolean = false;

    _label_info?: ILabel;
    _label?: Label;

    _connectors?: IConnector[] = []

    _info?: string;
    _wasInside: boolean = false;

    _image?: HTMLImageElement | null;
    _image_src?: string;
    _image_loaded: boolean = false;
    _image_scale: number = 1; //standart = 1
    _image_rotation: number = 0; // standart = 0
    //imageCache: { [key: string]: HTMLImageElement } = {};

    _graph: Graph | null = null;

    attachToGraph(graph: Graph) {
        this._graph = graph;
    }

    //constructor(x: number, y: number, radius: number, color?: string, rotation?: number, info?: string) {
    constructor(ellipse: IEllipse) {
        super();
        this._id = ellipse.id
        //this.type = type
        //if (type != 'ellipse') return;
        if (typeof ellipse.rotation !== 'undefined') {
            this._rotation = ellipse.rotation
        }
        this._x = ellipse.x
        this._y = ellipse.y
        this._radius_x = ellipse.radius_x
        this._radius_y = ellipse.radius_y
        this._color = ellipse.color
        this._info = ellipse.info

        if (typeof ellipse.isEdgeDash !== 'undefined') {
            this._isEdgeDash = ellipse.isEdgeDash
        }

        if (typeof ellipse.connectors !== 'undefined') {
            this._connectors = ellipse.connectors.slice()
        }

        if (typeof ellipse.label_info !== 'undefined') {
            this._label_info = ellipse.label_info

            if (typeof this._label_info.startX === 'undefined') {
                this._label_info.startX = this._x - this._radius_x / 2
            }

            if (typeof this._label_info.startY === 'undefined') {
                this._label_info.startY = this._y
            }

            if (typeof this._label_info.endX === 'undefined') {
                this._label_info.endX = this._x + this._radius_x / 2
            }

            if (typeof this._label_info.endY === 'undefined') {
                this._label_info.endY = this._y
            }

            this._label = new Label(this._label_info)
        }

        if (ellipse.image_src) {
            this._image_src = ellipse.image_src

            if (ellipse.image_rotation) {
                this._image_rotation = ellipse.image_rotation
            }

            if (ellipse.image_scale) {
                this._image_scale = ellipse.image_scale
            }
        }
    }

    private _loadImage() {
        if (!this._image_src) return;

        this._image = new Image();
        this._image.src = this._image_src;

        this._image.onload = () => {
            this._image_loaded = true;
            if (this._graph) {
                this._graph.requestRedraw();
            }
        };

        this._image.onerror = () => {
            console.error(`Ошибка загрузки изображения: ${this._image_src}`);
            this._image = null;
        };
    }

    setImageSource(src: string) {
        if (src === this._image_src) return;

        this._image_src = src;
        this._image = null;
        this._image_loaded = false;

        if (this._graph) {
            this._graph.requestRedraw();
        }
    }

    draw_canvas(ctx: CanvasRenderingContext2D) {
        if (!ctx) return;

        ctx.save();

        const radian = this.rotation! * Math.PI / 180;

        ctx.translate(this.x, this.y);
        ctx.rotate(this.rotation! * Math.PI / 180);
        ctx.translate(-this.x, -this.y);

        if (this._isEdgeDash) {
            ctx.setLineDash([5, 3]);
        }

        ctx.beginPath();
        ctx.ellipse(this.x, this.y, this.radius_x, this.radius_y, 0, 0, 2 * Math.PI);
        ctx.stroke();
        if (typeof this.color !== 'undefined' && this.color.length > 0) {
            ctx.fillStyle = this.color!;
            ctx.fill();
        }
        ctx.closePath();

        if (this._isEdgeDash) {
            ctx.setLineDash([]);
        }

        if (typeof this._image_src !== 'undefined') {
            // Сохраняем состояние контекста перед обрезкой
            ctx.save();

            ctx.beginPath();
            ctx.ellipse(this.x, this.y, this.radius_x, this.radius_y, 0, 0, 2 * Math.PI);
            ctx.clip();

            if (this._image_loaded && this._image) {
                // scale to figure size
                const scale = Math.min(
                    (this.radius_x * 2) / this._image.width,
                    (this.radius_y * 2) / this._image.height
                );
                const width = this._image.width * scale * this._image_scale;
                const height = this._image.height * scale * this._image_scale;

                ctx.translate(this.x, this.y);
                ctx.rotate(this._image_rotation * Math.PI / 180); 
                ctx.translate(-this.x, -this.y);

                // Рисуем изображение в центре эллипса
                ctx.drawImage(this._image, this.x - width / 2, this.y - height / 2, width, height);
            } else {
                if (!this._image) {
                    this._loadImage();
                }

                // Рисуем заглушку на время загрузки
                ctx.fillStyle = "#CCCCCC";
                ctx.fillRect(this.x - this.radius_x, this.y - this.radius_y, this.radius_x * 2, this.radius_y * 2);
            }

            ctx.restore();
        }

        ctx.restore();

        //for now
        if (typeof this._label !== 'undefined') {
            this._label.draw_canvas(ctx)
        }
    }

    draw_hovered(ctx: CanvasRenderingContext2D) {
        if (!ctx) return;

        const radian = this.rotation! * Math.PI / 180;

        ctx.beginPath();
        ctx.ellipse(this.x, this.y, this.radius_x, this.radius_y, radian, 0, 2 * Math.PI);
        ctx.stroke();
        //if (typeof this.color !== 'undefined' && this.color.length > 0) {
        //    ctx.fillStyle = this.color!;
        //    ctx.fill();
        //}

        ctx.fillStyle = 'skyblue';
        ctx.fill();

        ctx.closePath();
        ctx.restore();

        //for now
        if (typeof this._label !== 'undefined') {
            this._label.draw_canvas(ctx)
        }
    }

    draw_clicked(ctx: CanvasRenderingContext2D) {
        if (!ctx) return;

        const radian = this.rotation! * Math.PI / 180;

        ctx.beginPath();
        ctx.ellipse(this.x, this.y, this.radius_x, this.radius_y, radian, 0, 2 * Math.PI);
        ctx.stroke();
        //if (typeof this.color !== 'undefined' && this.color.length > 0) {
        //    ctx.fillStyle = this.color!;
        //    ctx.fill();
        //}

        ctx.fillStyle = '#b57281';
        ctx.fill();

        ctx.closePath();
        ctx.restore();

        //for now
        if (typeof this._label !== 'undefined') {
            this._label.draw_canvas(ctx)
        }
    }

    is_inside(mouseX: number, mouseY: number) {

        const radian = this.rotation * (Math.PI / 180)

        const x_rotated = (mouseX - this.x) * Math.cos(radian) + (mouseY - this.y) * Math.sin(radian)
        const y_rotated = -(mouseX - this.x) * Math.sin(radian) + (mouseY - this.y) * Math.cos(radian)

        return (x_rotated ** 2) / (this.radius_x ** 2) + (y_rotated ** 2) / (this.radius_y ** 2) <= 1
    }

    get id() {
        return this._id;
    }

    get type() {
        return this._type;
    }

    get rotation() {
        return this._rotation;
    }

    get x() {
        return this._x;
    }

    get y() {
        return this._y;
    }

    get radius_x() {
        return this._radius_x;
    }

    get radius_y() {
        return this._radius_y;
    }

    get color() {
        return this._color;
    }

    get info() {
        return this._info;
    }

    get wasInside() {
        return this._wasInside;
    }

    get connectors() {
        return this._connectors;
    }
}

// rotation for label
// is_inside fix
export class Rhombus extends Node implements IRhombus {
    _id: string;
    _type: string = 'rhombus';
    _dialect: string = 'base';
    _rotation?: number = 0;
    _x: number;
    _y: number;
    _width: number;
    _height: number;
    _color?: string;
    _info?: string;
    _wasInside: boolean = false;

    _isEdgeDash: boolean = false;

    _label_info?: ILabel;
    _label?: Label;

    _connectors?: IConnector[] = []

    _image?: HTMLImageElement | null;
    _image_src?: string;
    _image_loaded: boolean = false;
    _image_scale: number = 1; //standart = 1
    _image_rotation: number = 0; // standart = 0
    //imageCache: { [key: string]: HTMLImageElement } = {};

    _graph: Graph | null = null;

    attachToGraph(graph: Graph) {
        this._graph = graph;
    }

    constructor(rhomb: IRhombus) {
        super();
        this._id = rhomb.id
        //this.type = type
        if (typeof rhomb.rotation !== 'undefined') {
            this._rotation = rhomb.rotation
        }
        this._x = rhomb.x
        this._y = rhomb.y
        this._width = rhomb.width
        this._height = rhomb.height
        this._color = rhomb.color
        this._info = rhomb.info

        if (typeof rhomb.isEdgeDash !== 'undefined') {
            this._isEdgeDash = rhomb.isEdgeDash
        }

        if (typeof rhomb.connectors !== 'undefined') {
            this._connectors = rhomb.connectors.slice()
        }

        if (typeof rhomb.connectors !== 'undefined') {
            this._connectors = rhomb.connectors.slice()
        }

        if (typeof rhomb.label_info !== 'undefined') {
            this._label_info = rhomb.label_info

            if (typeof this._label_info.startX === 'undefined') {
                this._label_info.startX = this._x - this._width / 2
            }

            if (typeof this._label_info.startY === 'undefined') {
                this._label_info.startY = this._y
            }

            if (typeof this._label_info.endX === 'undefined') {
                this._label_info.endX = this._x + this._width / 2
            }

            if (typeof this._label_info.endY === 'undefined') {
                this._label_info.endY = this._y
            }

            this._label = new Label(this._label_info)
        }

        if (rhomb.image_src) {
            this._image_src = rhomb.image_src

            if (rhomb.image_rotation) {
                this._image_rotation = rhomb.image_rotation
            }

            if (rhomb.image_scale) {
                this._image_scale = rhomb.image_scale
            }
        }
    }

    private _loadImage() {
        if (!this._image_src) return;

        this._image = new Image();
        this._image.src = this._image_src;

        this._image.onload = () => {
            this._image_loaded = true;
            if (this._graph) {
                this._graph.requestRedraw();
            }
        };

        this._image.onerror = () => {
            console.error(`Ошибка загрузки изображения: ${this._image_src}`);
            this._image = null;
        };
    }

    setImageSource(src: string) {
        if (src === this._image_src) return;

        this._image_src = src;
        this._image = null;
        this._image_loaded = false;

        if (this._graph) {
            this._graph.requestRedraw();
        }
    }

    draw_canvas(ctx: CanvasRenderingContext2D) {
        if (!ctx) return;

        const radian = this.rotation! * Math.PI / 180;

        ctx.save();
        ctx.translate(this.x, this.y);
        ctx.rotate(radian);

        ctx.beginPath();

        ctx.moveTo(0, - (this.height / 2));
        ctx.lineTo(this.width / 2, 0);
        ctx.lineTo(0, this.height / 2);
        ctx.lineTo(- (this.width / 2), 0);

        ctx.closePath();

        if (this._isEdgeDash) {
            ctx.setLineDash([5, 3]);
        }

        ctx.stroke();

        if (this._isEdgeDash) {
            ctx.setLineDash([]);
        }

        if (typeof this.color !== 'undefined' && this.color.length > 0) {
            ctx.fillStyle = this.color!;
            ctx.fill();
        }

        //ctx.lineWidth = 2;
        //ctx.strokeStyle = 'black';

        if (typeof this._image_src !== 'undefined') {
            // Сохраняем состояние контекста перед обрезкой
            ctx.save();

            ctx.beginPath();

            ctx.moveTo(0, - (this.height / 2));
            ctx.lineTo(this.width / 2, 0);
            ctx.lineTo(0, this.height / 2);
            ctx.lineTo(- (this.width / 2), 0);

            ctx.closePath();
            ctx.clip();

            if (this._image_loaded && this._image) {
                const scale = Math.min(
                    this.width / this._image.width,
                    this.height / this._image.height
                );
                const width = this._image.width * scale * this._image_scale;
                const height = this._image.height * scale * this._image_scale;

                ctx.rotate(this._image_rotation * Math.PI / 180);

                ctx.drawImage(this._image, -width / 2, -height / 2, width, height);
            } else {
                if (!this._image) {
                    this._loadImage();
                }

                // Рисуем заглушку на время загрузки
                ctx.fillStyle = "#CCCCCC";
                ctx.fillRect(-this.width / 2, -this.height / 2, this.width, this.height);
            }

            ctx.restore();
        }

        ctx.restore();

        //for now
        if (typeof this._label !== 'undefined') {
            this._label.draw_canvas(ctx)
        }

    }

    draw_hovered(ctx: CanvasRenderingContext2D) {
        if (!ctx) return;

        const radian = this.rotation! * Math.PI / 180;

        ctx.save();
        ctx.translate(this.x, this.y);
        ctx.rotate(radian);

        ctx.beginPath();

        ctx.moveTo(0, - (this.height / 2));
        ctx.lineTo(this.width / 2, 0);
        ctx.lineTo(0, this.height / 2);
        ctx.lineTo(- (this.width / 2), 0);

        ctx.closePath();

        if (this._isEdgeDash) {
            ctx.setLineDash([5, 3]);
        }

        ctx.stroke();

        if (this._isEdgeDash) {
            ctx.setLineDash([]);
        }

        ctx.fillStyle = 'skyblue';

        ctx.fill();
        
        //ctx.lineWidth = 2;
        //ctx.strokeStyle = 'black';
        ctx.restore();

        //for now
        if (typeof this._label !== 'undefined') {
            this._label.draw_canvas(ctx)
        }

    }

    draw_clicked(ctx: CanvasRenderingContext2D) {
        if (!ctx) return;

        const radian = this.rotation! * Math.PI / 180;

        ctx.save();
        ctx.translate(this.x, this.y);
        ctx.rotate(radian);

        ctx.beginPath();

        ctx.moveTo(0, - (this.height / 2));
        ctx.lineTo(this.width / 2, 0);
        ctx.lineTo(0, this.height / 2);
        ctx.lineTo(- (this.width / 2), 0);

        ctx.closePath();

        if (this._isEdgeDash) {
            ctx.setLineDash([5, 3]);
        }

        ctx.stroke();

        if (this._isEdgeDash) {
            ctx.setLineDash([]);
        }

        ctx.fillStyle = '#b57281';

        ctx.fill();

        ctx.restore();

        //for now
        if (typeof this._label !== 'undefined') {
            this._label.draw_canvas(ctx)
        }
    }

    is_inside(mouseX: number, mouseY: number) {
        return (
            mouseX >= this.x - this.width / 2 &&
            mouseX <= this.x + this.width / 2 &&
            mouseY >= this.y - this.height / 2 &&
            mouseY <= this.y + this.height / 2
        )
    }

    get type() {
        return this._type;
    }

    get rotation() {
        return this._rotation;
    }

    get x() {
        return this._x;
    }

    get y() {
        return this._y;
    }

    get width() {
        return this._width;
    }

    get height() {
        return this._height;
    }

    get color() {
        return this._color;
    }

    get info() {
        return this._info;
    }

    get wasInside() {
        return this._wasInside;
    }

    get id() {
        return this._id;
    }

    get connectors() {
        return this._connectors;
    }
}

//TODO: is_inside fix
export class Triangle extends Node implements ITriangle {
    _id: string;
    _type: string = 'triangle';
    _dialect: string = 'base';
    _rotation: number = 0;
    _x_1: number
    _y_1: number
    _x_2: number
    _y_2: number
    _x_3: number
    _y_3: number
    _color?: string;

    _isEdgeDash: boolean = false;

    _label_info?: ILabel;
    _label?: Label;

    _connectors?: IConnector[] = []

    _image?: HTMLImageElement | null;
    _image_src?: string;
    _image_loaded: boolean = false;
    _image_scale: number = 1; //standart = 1
    _image_rotation: number = 0; // standart = 0
    //imageCache: { [key: string]: HTMLImageElement } = {};

    _info?: string;
    _wasInside: boolean = false;

    _graph: Graph | null = null;

    attachToGraph(graph: Graph) {
        this._graph = graph;
    }

    //constructor(x_1: number, y_1: number, x_2: number, y_2: number, x_3: number, y_3: number, color?: string, rotation?: number, info?: string) {
    constructor(triangle: ITriangle) {
        super();
        this._id = triangle.id
        //this.type = type
        //if (type != 'circle') return;
        if (typeof triangle.rotation !== 'undefined') {
            this._rotation = triangle.rotation
        }
        this._x_1 = triangle.x_1
        this._y_1 = triangle.y_1
        this._x_2 = triangle.x_2
        this._y_2 = triangle.y_2
        this._x_3 = triangle.x_3
        this._y_3 = triangle.y_3
        this._color = triangle.color
        this._info = triangle.info

        if (typeof triangle.isEdgeDash !== 'undefined') {
            this._isEdgeDash = triangle.isEdgeDash
        }

        if (typeof triangle.connectors !== 'undefined') {
            this._connectors = triangle.connectors.slice()
        }

        if (typeof triangle.label_info !== 'undefined') {
            this._label_info = triangle.label_info

            const x_center = (this._x_1 + this._x_2 + this._x_3) / 3;
            const y_center = (this._y_1 + this._y_2 + this._y_3) / 3;

            const length = 10

            if (typeof this._label_info.startX === 'undefined') {
                this._label_info.startX = x_center - length
            }

            if (typeof this._label_info.startY === 'undefined') {
                this._label_info.startY = y_center
            }

            if (typeof this._label_info.endX === 'undefined') {
                this._label_info.endX = x_center + length
            }

            if (typeof this._label_info.endY === 'undefined') {
                this._label_info.endY = y_center
            }

            this._label = new Label(this._label_info)
        }

        if (triangle.image_src) {
            this._image_src = triangle.image_src

            if (triangle.image_rotation) {
                this._image_rotation = triangle.image_rotation
            }

            if (triangle.image_scale) {
                this._image_scale = triangle.image_scale
            }
        }
    }

    private _loadImage() {
        if (!this._image_src) return;

        this._image = new Image();
        this._image.src = this._image_src;

        this._image.onload = () => {
            this._image_loaded = true;
            if (this._graph) {
                this._graph.requestRedraw();
            }
        };

        this._image.onerror = () => {
            console.error(`Ошибка загрузки изображения: ${this._image_src}`);
            this._image = null;
        };
    }

    setImageSource(src: string) {
        if (src === this._image_src) return;

        this._image_src = src;
        this._image = null;
        this._image_loaded = false;

        if (this._graph) {
            this._graph.requestRedraw();
        }
    }

    draw_canvas(ctx: CanvasRenderingContext2D) {
        if (!ctx) return;

        const radian = this.rotation! * Math.PI / 180;

        ctx.save();

        const centerX = (this.x_1 + this.x_2 + this.x_3) / 3;
        const centerY = (this.y_1 + this.y_2 + this.y_3) / 3;

        ctx.translate(centerX, centerY);
        ctx.rotate(radian);
        ctx.translate(-centerX, -centerY);

        ctx.beginPath();

        ctx.moveTo(this.x_1, this.y_1);
        ctx.lineTo(this.x_2, this.y_2);
        ctx.lineTo(this.x_3, this.y_3);

        ctx.closePath();

        // the outline
        //ctx.lineJoin = "round";
        //ctx.lineWidth = 10;
        //ctx.strokeStyle = '#666666';

        if (this._isEdgeDash) {
            ctx.setLineDash([5, 3]);
        }

        ctx.stroke();

        if (this._isEdgeDash) {
            ctx.setLineDash([]);
        }

        if (typeof this.color !== 'undefined' && this.color.length > 0) {
            ctx.fillStyle = this.color!;
            ctx.fill();
        }

        if (typeof this._image_src !== 'undefined') {
            // Сохраняем состояние контекста перед обрезкой
            ctx.save();

            ctx.beginPath();
            ctx.moveTo(this.x_1, this.y_1);
            ctx.lineTo(this.x_2, this.y_2);
            ctx.lineTo(this.x_3, this.y_3);
            ctx.closePath();

            ctx.clip();

            const minX = Math.min(this.x_1, this.x_2, this.x_3);
            const maxX = Math.max(this.x_1, this.x_2, this.x_3);
            const minY = Math.min(this.y_1, this.y_2, this.y_3);
            const maxY = Math.max(this.y_1, this.y_2, this.y_3);

            const triangle_width = maxX - minX;
            const triangle_height = maxY - minY;

            if (this._image_loaded && this._image) {
                const scale = Math.min(
                    triangle_width / this._image.width,
                    triangle_height / this._image.height
                );
                const width = this._image.width * scale * this._image_scale;
                const height = this._image.height * scale * this._image_scale;

                ctx.translate(centerX, centerY);
                ctx.rotate(this._image_rotation * Math.PI / 180);
                //ctx.translate(-centerX, -centerY);

                ctx.drawImage(this._image, -width / 2, -height / 2, width, height);
            } else {
                if (!this._image) {
                    this._loadImage();
                }

                // Рисуем заглушку на время загрузки
                ctx.fillStyle = "#CCCCCC";
                ctx.fillRect(centerX - triangle_width / 2, centerY - triangle_height / 2, triangle_width, triangle_height);
            }

            ctx.restore();
        }

        ctx.restore();

        //for now
        if (typeof this._label !== 'undefined') {
            this._label.draw_canvas(ctx)
        }
    }

    draw_hovered(ctx: CanvasRenderingContext2D) {
        if (!ctx) return;

        const radian = this.rotation! * Math.PI / 180;

        //ctx.save();
        //ctx.translate(this.x_1, this.y_1);
        //ctx.rotate(radian);

        ctx.beginPath();

        ctx.moveTo(this.x_1, this.y_1);
        ctx.lineTo(this.x_2, this.y_2);
        ctx.lineTo(this.x_3, this.y_3);
        ctx.closePath();

        // the outline
        //ctx.lineJoin = "round";
        //ctx.lineWidth = 10;
        //ctx.strokeStyle = '#666666';
        ctx.stroke();

        ctx.fillStyle = 'skyblue';
        ctx.fill();


        //ctx.restore();

        //for now
        if (typeof this._label !== 'undefined') {
            this._label.draw_canvas(ctx)
        }
    }

    draw_clicked(ctx: CanvasRenderingContext2D) {
        if (!ctx) return;

        const radian = this.rotation! * Math.PI / 180;

        //ctx.save();
        //ctx.translate(this.x_1, this.y_1);
        //ctx.rotate(radian);

        ctx.beginPath();

        ctx.moveTo(this.x_1, this.y_1);
        ctx.lineTo(this.x_2, this.y_2);
        ctx.lineTo(this.x_3, this.y_3);
        ctx.closePath();

        // the outline
        //ctx.lineJoin = "round";
        //ctx.lineWidth = 10;
        //ctx.strokeStyle = '#666666';
        ctx.stroke();

        ctx.fillStyle = '#b57281';
        ctx.fill();

        //for now
        if (typeof this._label !== 'undefined') {
            this._label.draw_canvas(ctx)
        }
    }


    is_inside(mouseX: number, mouseY: number) {
        /* Calculate area of triangle ABC */
        const area_A = this.area_of_shape(this.x_1, this.y_1, this.x_2, this.y_2, this.x_3, this.y_3)

        /* Calculate area of triangle PBC */
        const area_A1 = this.area_of_shape(mouseX, mouseY, this.x_2, this.y_2, this.x_3, this.y_3)

        /* Calculate area of triangle PAC */
        const area_A2 = this.area_of_shape(this.x_1, this.y_1, mouseX, mouseY, this.x_3, this.y_3)

        /* Calculate area of triangle PAB */
        const area_A3 = this.area_of_shape(this.x_1, this.y_1, this.x_2, this.y_2, mouseX, mouseY)

        /* Check if sum of A1, A2 and A3 is same as A */
        return (area_A == area_A1 + area_A2 + area_A3);
    }

    area_of_shape(x1: number, y1: number, x2: number, y2: number, x3: number, y3: number) {
        return Math.abs((x1 * (y2 - y3) + x2 * (y3 - y1) + x3 * (y1 - y2)) / 2.0);
    }

    get id() {
        return this._id;
    }

    get type() {
        return this._type;
    }

    get rotation() {
        return this._rotation;
    }

    get x_1() {
        return this._x_1;
    }

    get y_1() {
        return this._y_1;
    }

    get x_2() {
        return this._x_2;
    }

    get y_2() {
        return this._y_2;
    }

    get x_3() {
        return this._x_3;
    }

    get y_3() {
        return this._y_3;
    }

    get color() {
        return this._color;
    }

    get info() {
        return this._info;
    }

    get wasInside() {
        return this._wasInside;
    }

    get connectors() {
        return this._connectors;
    }
}

//TODO: image
export class RegularPolygon extends Node implements IRegularPolygon {
    _id: string;
    _type: string = 'regular polygon';
    _dialect: string = 'base';
    _rotation?: number = 0;
    _number_of_edges: number
    _x: number
    _y: number
    _radius: number;
    _color?: string;

    _isEdgeDash: boolean = false;

    _label_info?: ILabel;
    _label?: Label;

    _connectors?: IConnector[] = []


    _info?: string;
    _wasInside: boolean = false;
    points: Array<{ x: number; y: number }> = [];

    _graph: Graph | null = null;

    attachToGraph(graph: Graph) {
        this._graph = graph;
    }

    //constructor(number_of_edges: number, x: number, y: number, radius: number, color?: string, rotation?: number, info?: string) {
    constructor(polygon: IRegularPolygon) {
        super();
        this._id = polygon.id
        //this.type = type
        //if (type != 'circle') return;
        if (typeof polygon.rotation !== 'undefined') {
            this._rotation = polygon.rotation
        }
        this._number_of_edges = polygon.number_of_edges
        this._x = polygon.x
        this._y = polygon.y
        this._radius = polygon.radius
        this._color = polygon.color
        this._info = polygon.info

        if (typeof polygon.isEdgeDash !== 'undefined') {
            this._isEdgeDash = polygon.isEdgeDash
        }

        if (typeof polygon.connectors !== 'undefined') {
            this._connectors = polygon.connectors.slice()
        }

        if (typeof polygon.label_info !== 'undefined') {
            this._label_info = polygon.label_info

            //const length = 10

            if (typeof this._label_info.startX === 'undefined') {
                this._label_info.startX = this._x - this._radius
            }

            if (typeof this._label_info.startY === 'undefined') {
                this._label_info.startY = this._y
            }

            if (typeof this._label_info.endX === 'undefined') {
                this._label_info.endX = this._x + this._radius
            }

            if (typeof this._label_info.endY === 'undefined') {
                this._label_info.endY = this._y
            }

            this._label = new Label(this._label_info)
        }

        const radian = this.rotation! * Math.PI / 180;

        this.points.push({ x: (this.x + this.radius * Math.cos(0 + radian)), y: this.y + this.radius * Math.sin(0 + radian) })

        for (let i = 1; i <= this.number_of_edges; i += 1) {
            this.points.push({ x: (this.x + this.radius * Math.cos(i * 2 * Math.PI / this.number_of_edges + radian)), y: this.y + this.radius * Math.sin(i * 2 * Math.PI / this.number_of_edges + radian) })
        }
    }

    draw_canvas(ctx: CanvasRenderingContext2D) {
        if (!ctx) return;

        ctx.save();
        //const radian = this.rotation! * Math.PI / 180;
        //ctx.translate(this.x, this.y);
        //ctx.rotate(radian);
         
        ctx.beginPath();
        ctx.moveTo(this.points[0].x, this.points[0].y);

        for (let i = 1; i <= this.number_of_edges; i++) {
            ctx.lineTo(this.points[i].x, this.points[i].y);
        }

        //ctx.strokeStyle = "#000000";
        //ctx.lineWidth = 1;

        if (typeof this.color !== 'undefined' && this.color.length > 0) {
            ctx.fillStyle = this.color!;
            ctx.fill();
        }

        ctx.closePath();

        if (this._isEdgeDash) {
            ctx.setLineDash([5, 3]);
        }

        ctx.stroke();

        if (this._isEdgeDash) {
            ctx.setLineDash([]);
        }

        ctx.restore();

        //for now
        if (typeof this._label !== 'undefined') {
            this._label.draw_canvas(ctx)
        }
    }

    draw_hovered(ctx: CanvasRenderingContext2D) {
        if (!ctx) return;

        ctx.save();

        ctx.beginPath();
        ctx.moveTo(this.points[0].x, this.points[0].y);

        for (let i = 1; i <= this.number_of_edges; i++) {
            ctx.lineTo(this.points[i].x, this.points[i].y);
        }

        //ctx.strokeStyle = "#000000";
        //ctx.lineWidth = 1;

        ctx.fillStyle = 'skyblue';
        ctx.fill();

        ctx.stroke();

        ctx.closePath();

        ctx.restore();
    }

    draw_clicked(ctx: CanvasRenderingContext2D) {
        if (!ctx) return;

        ctx.save();

        ctx.beginPath();
        ctx.moveTo(this.points[0].x, this.points[0].y);

        for (let i = 1; i <= this.number_of_edges; i++) {
            ctx.lineTo(this.points[i].x, this.points[i].y);
        }

        //ctx.strokeStyle = "#000000";
        //ctx.lineWidth = 1;

        ctx.fillStyle = '#b57281';
        ctx.fill();

        ctx.stroke();

        ctx.closePath();

        ctx.restore();
    }

    //TODO:
    is_inside(mouseX: number, mouseY: number) {
        //const points: Array<{ x: number; y: number }> = [];

        //points.push({ x: (this.x + this.radius * Math.cos(0)), y: this.y + this.radius * Math.sin(0) })

        //for (let i = 1; i <= this.number_of_edges; i += 1) {
        //    points.push({ x: (this.x + this.radius * Math.cos(i * 2 * Math.PI / this.number_of_edges)), y: this.y + this.radius * Math.sin(i * 2 * Math.PI / this.number_of_edges) })
        //}

        let res: boolean = false;
        let j = this.number_of_edges - 1;
        for (let i = 0; i < this.number_of_edges; i++) {
            if ((this.points[i].y < mouseY && this.points[j].y >= mouseY || this.points[j].y < mouseY && this.points[i].y >= mouseY) &&
                (this.points[i].x + (mouseY - this.points[i].y) / (this.points[j].y - this.points[i].y) * (this.points[j].x - this.points[i].x) < mouseX)) {
                res = !res;
            }
            j = i
        }

        return res;
    }

    get id() {
        return this._id;
    }
    get type() {
        return this._type;
    }

    get rotation() {
        return this._rotation;
    }

    get x() {
        return this._x;
    }

    get y() {
        return this._y;
    }
    get number_of_edges() {
        return this._number_of_edges;
    }

    get radius() {
        return this._radius;
    }

    get color() {
        return this._color;
    }

    get info() {
        return this._info;
    }

    get wasInside() {
        return this._wasInside;
    }

    get connectors() {
        return this._connectors;
    }
}

//TODO: image
export class Cloud extends Node implements ICloud {
    _id: string;
    _type: string = 'cloud';
    _dialect: string = 'base';
    _rotation?: number = 0;
    _x_C: number;
    _y_C: number;
    _width: number;
    _height: number;
    _color?: string;
    _info?: string;
    _wasInside: boolean = false;

    _isEdgeDash: boolean = false;

    _label_info?: ILabel;
    _label?: Label;

    _connectors?: IConnector[] = []

    _graph: Graph | null = null;

    attachToGraph(graph: Graph) {
        this._graph = graph;
    }
    constructor(cloud: ICloud) {
        super();
        this._id = cloud.id
        //this.type = type
        if (typeof cloud.rotation !== 'undefined') {
            this._rotation = cloud.rotation
        }
        this._x_C = cloud.x_C
        this._y_C = cloud.y_C
        this._width = cloud.width
        this._height = cloud.height
        this._color = cloud.color
        this._info = cloud.info

        if (typeof cloud.isEdgeDash !== 'undefined') {
            this._isEdgeDash = cloud.isEdgeDash
        }

        if (typeof cloud.connectors !== 'undefined') {
            this._connectors = cloud.connectors.slice()
        }

        if (typeof cloud.connectors !== 'undefined') {
            this._connectors = cloud.connectors.slice()
        }

        if (typeof cloud.label_info !== 'undefined') {
            this._label_info = cloud.label_info

            if (typeof this._label_info.startX === 'undefined') {
                this._label_info.startX = this._x_C - this._width / 2
            }

            if (typeof this._label_info.startY === 'undefined') {
                this._label_info.startY = this._y_C
            }

            if (typeof this._label_info.endX === 'undefined') {
                this._label_info.endX = this._x_C + this._width / 2
            }

            if (typeof this._label_info.endY === 'undefined') {
                this._label_info.endY = this._y_C
            }

            this._label = new Label(this._label_info)
        }
    }

    draw_canvas(ctx: CanvasRenderingContext2D) {
        if (!ctx) return;

        ctx.save();

        const radian = this.rotation! * Math.PI / 180;

        ctx.translate(this.x_C, this.y_C);
        ctx.rotate(radian);
        
        const startX_Cloud = - this.width / 2;
        const startY_Cloud = - this.height / 2;

        const points = [
            { x: startX_Cloud + 0.25 * this.width, y: startY_Cloud + 0.25 * this.height },
            { x: startX_Cloud + 0.75 * this.width, y: startY_Cloud + 0.25 * this.height },
            { x: startX_Cloud + 0.75 * this.width, y: startY_Cloud + 0.75 * this.height },
            { x: startX_Cloud + 0.25 * this.width, y: startY_Cloud + 0.75 * this.height }
        ];

        ctx.beginPath();

        ctx.moveTo(points[0].x, points[0].y);

        ctx.bezierCurveTo(
            points[0].x, startY_Cloud,
            points[1].x, startY_Cloud,
            points[1].x, points[1].y
        );

        ctx.bezierCurveTo(
            startX_Cloud + this.width, points[1].y,
            startX_Cloud + this.width, points[2].y,
            points[2].x, points[2].y
        );

        ctx.bezierCurveTo(
            points[2].x, startY_Cloud + this.height,
            points[3].x, startY_Cloud + this.height,
            points[3].x, points[3].y
        );

        ctx.bezierCurveTo(
            startX_Cloud, points[3].y,
            startX_Cloud, points[0].y,
            points[0].x, points[0].y
        );

        ctx.closePath();

        if (typeof this.color !== 'undefined' && this.color.length > 0) {
            ctx.fillStyle = this.color!;
            ctx.fill();
        }
        //} else {
        //    ctx.fillStyle = 'rgba(255, 0, 0, 0)';
        //}
        if (this._isEdgeDash) {
            ctx.setLineDash([5, 3]); // Длина штриха и промежутка 
        }
        ctx.stroke();

        if (this._isEdgeDash) {
            ctx.setLineDash([]); // Сбрасываем пунктир 
        }

        ctx.restore();

        //for now
        if (typeof this._label !== 'undefined') {
            this._label.draw_canvas(ctx)
        }

    }

    draw_hovered(ctx: CanvasRenderingContext2D) {
        if (!ctx) return;

        ctx.save();
        const radian = this.rotation! * Math.PI / 180;
        ctx.translate(this.x_C, this.y_C);
        ctx.rotate(radian);
        if (this._isEdgeDash) {
            ctx.setLineDash([5, 3]);
        }        
        const startX_Cloud = - this.width / 2;
        const startY_Cloud = - this.height / 2;
        const points = [
            { x: startX_Cloud + 0.25 * this.width, y: startY_Cloud + 0.25 * this.height },
            { x: startX_Cloud + 0.75 * this.width, y: startY_Cloud + 0.25 * this.height },
            { x: startX_Cloud + 0.75 * this.width, y: startY_Cloud + 0.75 * this.height },
            { x: startX_Cloud + 0.25 * this.width, y: startY_Cloud + 0.75 * this.height }
        ];
        ctx.beginPath();
        ctx.moveTo(points[0].x, points[0].y);
        ctx.bezierCurveTo(
            points[0].x, startY_Cloud,
            points[1].x, startY_Cloud,
            points[1].x, points[1].y
        );
        ctx.bezierCurveTo(
            startX_Cloud + this.width, points[1].y,
            startX_Cloud + this.width, points[2].y,
            points[2].x, points[2].y
        );
        ctx.bezierCurveTo(
            points[2].x, startY_Cloud + this.height,
            points[3].x, startY_Cloud + this.height,
            points[3].x, points[3].y
        );
        ctx.bezierCurveTo(
            startX_Cloud, points[3].y,
            startX_Cloud, points[0].y,
            points[0].x, points[0].y
        );
        ctx.closePath();
        ctx.fillStyle = 'skyblue';
        ctx.fill();
        if (this._isEdgeDash) {
            ctx.setLineDash([]); // Сбрасываем пунктир 
        }
        ctx.restore();
    }

    draw_clicked(ctx: CanvasRenderingContext2D) {
        if (!ctx) return;

        ctx.save();
        const radian = this.rotation! * Math.PI / 180;
        ctx.translate(this.x_C, this.y_C);
        ctx.rotate(radian);
        if (this._isEdgeDash) {
            ctx.setLineDash([5, 3]);
        }
        const startX_Cloud = - this.width / 2;
        const startY_Cloud = - this.height / 2;
        const points = [
            { x: startX_Cloud + 0.25 * this.width, y: startY_Cloud + 0.25 * this.height },
            { x: startX_Cloud + 0.75 * this.width, y: startY_Cloud + 0.25 * this.height },
            { x: startX_Cloud + 0.75 * this.width, y: startY_Cloud + 0.75 * this.height },
            { x: startX_Cloud + 0.25 * this.width, y: startY_Cloud + 0.75 * this.height }
        ];
        ctx.beginPath();
        ctx.moveTo(points[0].x, points[0].y);
        ctx.bezierCurveTo(
            points[0].x, startY_Cloud,
            points[1].x, startY_Cloud,
            points[1].x, points[1].y
        );
        ctx.bezierCurveTo(
            startX_Cloud + this.width, points[1].y,
            startX_Cloud + this.width, points[2].y,
            points[2].x, points[2].y
        );
        ctx.bezierCurveTo(
            points[2].x, startY_Cloud + this.height,
            points[3].x, startY_Cloud + this.height,
            points[3].x, points[3].y
        );
        ctx.bezierCurveTo(
            startX_Cloud, points[3].y,
            startX_Cloud, points[0].y,
            points[0].x, points[0].y
        );
        ctx.closePath();
        ctx.fillStyle = '#b57281';
        ctx.fill();
        if (this._isEdgeDash) {
            ctx.setLineDash([]); // Сбрасываем пунктир 
        }
        ctx.restore();
    }

    is_inside(mouseX: number, mouseY: number) {
        const startX_Cloud = this.x_C - this.width / 2;
        const startY_Cloud = this.y_C - this.height / 2;
        return (mouseX >= startX_Cloud && mouseX <= startX_Cloud + this.width &&
            mouseY >= startY_Cloud && mouseY <= startY_Cloud + this.height);
    }

    get type() {
        return this._type;
    }

    get rotation() {
        return this._rotation;
    }

    get x_C() {
        return this._x_C;
    }

    get y_C() {
        return this._y_C;
    }

    get width() {
        return this._width;
    }

    get height() {
        return this._height;
    }

    get color() {
        return this._color;
    }

    get info() {
        return this._info;
    }

    get wasInside() {
        return this._wasInside;
    }

    get id() {
        return this._id;
    }

    get connectors() {
        return this._connectors;
    }
}

//TODO: image
export class Star extends Node implements IStar {
    _id: string;
    _type: string = 'star';
    _dialect: string = 'base';
    _rotation?: number = 0;
    _rad: number;
    _amount_points: number;
    _m: number;
    _x_C: number;
    _y_C: number;
    _color?: string;
    _info?: string;
    _wasInside: boolean = false;

    _isEdgeDash: boolean = false;

    _label_info?: ILabel;
    _label?: Label;

    _connectors?: IConnector[] = []

    _graph: Graph | null = null;

    attachToGraph(graph: Graph) {
        this._graph = graph;
    }

    constructor(star: IStar) {
        super();
        this._id = star.id
        //this.type = type
        if (typeof star.rotation !== 'undefined') {
            this._rotation = star.rotation
        }
        this._x_C = star.x_C
        this._y_C = star.y_C
        this._amount_points = star.amount_points
        this._m = star.m
        this._rad = star.rad
        this._color = star.color
        this._info = star.info

        if (typeof star.isEdgeDash !== 'undefined') {
            this._isEdgeDash = star.isEdgeDash
        }

        if (typeof star.connectors !== 'undefined') {
            this._connectors = star.connectors.slice()
        }

        if (typeof star.connectors !== 'undefined') {
            this._connectors = star.connectors.slice()
        }

        if (typeof star.label_info !== 'undefined') {
            this._label_info = star.label_info

            if (typeof this._label_info.startX === 'undefined') {
                this._label_info.startX = this._x_C - this._rad
            }

            if (typeof this._label_info.startY === 'undefined') {
                this._label_info.startY = this._y_C
            }

            if (typeof this._label_info.endX === 'undefined') {
                this._label_info.endX = this._x_C + this._rad
            }

            if (typeof this._label_info.endY === 'undefined') {
                this._label_info.endY = this._y_C
            }

            this._label = new Label(this._label_info)
        }
    }

    draw_canvas(ctx: CanvasRenderingContext2D) {
        if (!ctx) return;

        ctx.save();

        const radian = this.rotation! * Math.PI / 180;

        ctx.translate(this.x_C, this.y_C);
        ctx.rotate(radian);

        //ctx.lineWidth = 2;
        if (this._isEdgeDash) {
            ctx.setLineDash([5, 3]); // Длина штриха и промежутка 
        }

        //const colorWithAlpha = hexToRgba(star.color, star.colorAlpha);
        ctx.beginPath();
        const points = [];
        ctx.moveTo(0, this.rad);
        for (let i = 0; i < 2 * this.amount_points; i++) {
            const angle = Math.PI * i / this.amount_points;
            const radius = i % 2 === 0 ? this.rad : this.rad * this.m;
            const x = radius * Math.sin(angle);
            const y = radius * Math.cos(angle);
            points.push({ x, y });
            ctx.lineTo(x, y);
        }
        ctx.closePath();

        if (typeof this.color !== 'undefined' && this.color.length > 0) {
            ctx.fillStyle = this.color!;
            ctx.fill();
        }

        ctx.stroke();

        if (this._isEdgeDash) {
            ctx.setLineDash([]); // Сбрасываем пунктир 
        }

        ctx.restore();

        //for now
        if (typeof this._label !== 'undefined') {
            this._label.draw_canvas(ctx)
        }

    }

    draw_hovered(ctx: CanvasRenderingContext2D) {
        if (!ctx) return;
        ctx.save();
        const radian = this.rotation! * Math.PI / 180;
        ctx.translate(this.x_C, this.y_C);
        ctx.rotate(radian);
        if (this._isEdgeDash) {
            ctx.setLineDash([5, 3]);
        }
        ctx.beginPath();
        const points = [];
        ctx.moveTo(0, this.rad);
        for (let i = 0; i < 2 * this.amount_points; i++) {
            const angle = Math.PI * i / this.amount_points;
            const radius = i % 2 === 0 ? this.rad : this.rad * this.m;
            const x = radius * Math.sin(angle);
            const y = radius * Math.cos(angle);
            points.push({ x, y });
            ctx.lineTo(x, y);
        }
        ctx.closePath();
        
        ctx.fillStyle = 'skyblue';
        ctx.fill();
        
        if (this._isEdgeDash) {
            ctx.setLineDash([]); // Сбрасываем пунктир 
        }
        ctx.restore();
    }

    draw_clicked(ctx: CanvasRenderingContext2D) {
        if (!ctx) return;
        ctx.save();
        const radian = this.rotation! * Math.PI / 180;
        ctx.translate(this.x_C, this.y_C);
        ctx.rotate(radian);
        if (this._isEdgeDash) {
            ctx.setLineDash([5, 3]);
        }
        ctx.beginPath();
        const points = [];
        ctx.moveTo(0, this.rad);
        for (let i = 0; i < 2 * this.amount_points; i++) {
            const angle = Math.PI * i / this.amount_points;
            const radius = i % 2 === 0 ? this.rad : this.rad * this.m;
            const x = radius * Math.sin(angle);
            const y = radius * Math.cos(angle);
            points.push({ x, y });
            ctx.lineTo(x, y);
        }
        ctx.closePath();
        ctx.fillStyle = '#b57281';
        ctx.fill();
        if (this._isEdgeDash) {
            ctx.setLineDash([]); // Сбрасываем пунктир 
        }
        ctx.restore();
    }

    is_inside(mouseX: number, mouseY: number) {
        return (
            mouseX >= this.x_C - this.rad &&
            mouseX <= this.x_C + this.rad &&
            mouseY >= this.y_C - this.rad &&
            mouseY <= this.y_C + this.rad
        )
    }

    get type() {
        return this._type;
    }

    get rotation() {
        return this._rotation;
    }

    get x_C() {
        return this._x_C;
    }

    get y_C() {
        return this._y_C;
    }

    get rad() {
        return this._rad;
    }

    get m() {
        return this._m;
    }

    get amount_points() {
        return this._amount_points;
    }

    get color() {
        return this._color;
    }

    get info() {
        return this._info;
    }

    get wasInside() {
        return this._wasInside;
    }

    get id() {
        return this._id;
    }

    get connectors() {
        return this._connectors;
    }
}


//TODO: image
//rotation
//scale
export class CustomShape extends Node implements IShape {
    _id: string;
    _type: string;
    _dialect: string = 'base';
    _rotation?: number;
    _x_center: number; // отсчёт координат
    _y_center: number;
    points: Array<{ x: number; y: number }> = [];
    curve: Array<{ isCurved: boolean; cp1x: number; cp1y: number; cp2x: number; cp2y: number; }> = [];

    _isEdgeDash: boolean = false;

    _label_info?: ILabel;
    _label?: Label;

    _connectors?: IConnector[] = []

    _color?: string;
    _info?: string;
    _wasInside: boolean = false;
    _scale: number;

    _graph: Graph | null = null;

    attachToGraph(graph: Graph) {
        this._graph = graph;
    }

    constructor(custom: ICustomShape, description: ICustomDescription) {
        super();
        this._id = custom.id
        //if (custom.type != description.typeName) return;
        this._type = description.typeName

        if (typeof custom.rotation !== 'undefined') {
            this._rotation = custom.rotation
        }
        this._color = custom.color
        this._info = custom.info
        this._x_center = custom.x_center
        this._y_center = custom.y_center
        this.points = description.points.slice()

        if (typeof custom.isEdgeDash !== 'undefined') {
            this._isEdgeDash = custom.isEdgeDash
        }

        if (typeof custom.connectors !== 'undefined') {
            this._connectors = custom.connectors.slice()
        }

        // check length points == curve
        this.curve = description.curve.slice()

        this._scale = 1;

        if (typeof custom.label_info !== 'undefined') {
            this._label_info = custom.label_info

            let x_sum = 0;
            let y_sum = 0;

            // Суммируем координаты всех точек
            this.points.forEach(point => {
                x_sum += point.x;
                y_sum += point.y;
            });

            // Находим среднее значение
            const x_center = this.x_center + x_sum / this.points.length;
            const y_center = this.y_center + y_sum / this.points.length;

            const length = 10

            if (typeof this._label_info.startX === 'undefined') {
                this._label_info.startX = x_center - length
            }

            if (typeof this._label_info.startY === 'undefined') {
                this._label_info.startY = y_center
            }

            if (typeof this._label_info.endX === 'undefined') {
                this._label_info.endX = x_center + length
            }

            if (typeof this._label_info.endY === 'undefined') {
                this._label_info.endY = y_center
            }

            this._label = new Label(this._label_info)
        }
    }



    draw_canvas(ctx: CanvasRenderingContext2D) {
        if (!ctx) return;
        ctx.save();

        ctx.translate(this.x_center, this.y_center);
        //ctx.rotate(radian);

        ctx.beginPath();

        ctx.moveTo(this.points[0].x, this.points[0].y);

        for (let i = 1; i < this.points.length; i++) {
            //if (typeof this.curve[i - 1].cp1x !== 'undefined' && typeof this.curve[i - 1].cp1y !== 'undefined' && typeof this.curve[i - 1].cp2x !== 'undefined' && typeof this.curve[i - 1].cp2y !== 'undefined')
            if (this.curve[i - 1].isCurved) {
                ctx.bezierCurveTo(this.curve[i - 1].cp1x, this.curve[i - 1].cp1y, this.curve[i - 1].cp2x, this.curve[i - 1].cp2y, this.points[i].x, this.points[i].y);
            } else {
                ctx.lineTo(this.points[i].x, this.points[i].y);
            }
        }
        ctx.closePath();

        //ctx.lineWidth = 5;

        if (typeof this.color !== 'undefined' && this.color.length > 0) {
            ctx.fillStyle = this.color!;
            ctx.fill();
        }

        //ctx.strokeStyle = '#0000ff';

        if (this._isEdgeDash) {
            ctx.setLineDash([5, 3]);
        }

        ctx.stroke();

        if (this._isEdgeDash) {
            ctx.setLineDash([]);
        }

        ctx.restore();

        //for now
        if (typeof this._label !== 'undefined') {
            this._label.draw_canvas(ctx)
        }
    }

    draw_hovered(ctx: CanvasRenderingContext2D) {
        if (!ctx) return;

    }

    draw_clicked(ctx: CanvasRenderingContext2D) {
        if (!ctx) return;

    }

    //TODO:
    is_inside(mouseX: number, mouseY: number) {
        return (false
            //mouseX >= this.x - this.radius &&
            //mouseX <= this.x + this.radius &&
            //mouseY >= this.y - this.radius &&
            //mouseY <= this.y + this.radius
        )
    }

    get id() {
        return this._id;
    }

    get type() {
        return this._type;
    }

    get rotation() {
        return this._rotation;
    }

    get x_center() {
        return this._x_center;
    }

    get y_center() {
        return this._y_center;
    }

    get color() {
        return this._color;
    }
    get scale() {
        return this._scale;
    }

    get info() {
        return this._info;
    }

    get wasInside() {
        return this._wasInside;
    }

    get connectors() {
        return this._connectors;
    }
}


export abstract class Edge extends GraphItemBase {
    abstract _id: string;
    abstract _type: string;
    abstract _dialect?: string;

    abstract _color?: string;
    abstract _info?: string;
    abstract _wasInside: boolean;

    // id_target???
    abstract _target?: Node;
    abstract _source?: Node;

    abstract _startX: number;
    abstract _startY: number;
    abstract _endX: number;
    abstract _endY: number;

    abstract _lineWidth?: number;

    abstract _points?: Array<{ x: number; y: number }>; 
    abstract _is_corners_rounded?: boolean
    abstract _max_radius_of_corners: number;

    abstract _graph: Graph | null;

    abstract attachToGraph(graph: Graph): void;

    abstract draw_canvas(ctx: CanvasRenderingContext2D): void;

    abstract draw_hovered(ctx: CanvasRenderingContext2D): void;

    abstract draw_clicked(ctx: CanvasRenderingContext2D): void;

    abstract is_inside(mouseX: number, mouseY: number): boolean;

    //abstract get id(): string;

    // Реализация методов GraphItemBase
    saveDefaultStyle(): void {
        this._defaultStyle = {
            color: this._color,
            // другие свойства по необходимости
        };
    }

    applyStyle(style: IStyleInfo): void {
        this._currentStyle = { ...style };

        if (!this._originalColor) {
            this._originalColor = this._color;
        }

        if (style.color) {
            this._color = style.color;
        }

        if (style.fillColor) {
            this._color = style.fillColor;
        }
    }

    resetStyle(): void {
        if (this._originalColor) {
            this._color = this._originalColor;
            this._originalColor = undefined;
        }

        this._currentStyle = {};
    }

    select(): void {
        this._isSelected = true;
        const style = this._graph?.getEventStyle(this._id, 'selected');
        if (style) {
            this.applyStyle(style);
        }
    }

    deselect(): void {
        this._isSelected = false;
        this.resetStyle();
    }

    emphasize(): void {
        this._isEmphasized = true;
        const style = this._graph?.getEventStyle(this._id, 'emphasized');
        if (style) {
            this.applyStyle(style);
        }
    }

    deEmphasize(): void {
        this._isEmphasized = false;
        this.resetStyle();
    }

    // Метод для обработки кликов с учетом системы ограничений
    handleSelection(mode: 'select' | 'deselect' | 'toggle', event?: MouseEvent): void {
        if (!this._graph) return;

        switch (mode) {
            case 'select': {
                this._graph.select(this, 'add');
                break;
            }

            case 'deselect': {
                this._graph.deselect(this);
                break;
            }

            case 'toggle': {
                const constraint = this._graph.getSelectionConstraint(this._type);
                let shouldDeselect = false;

                if (constraint && event) {
                    switch (constraint.deselectMode) {
                        case 'click':
                            shouldDeselect = true;
                            break;
                        case 'ctrl+click':
                            shouldDeselect = event.ctrlKey || event.metaKey;
                            break;
                    }
                }

                if (shouldDeselect && this._graph.isItemSelected(this)) {
                    this._graph.deselect(this);
                } else {
                    this._graph.select(this, 'toggle');
                }
                break;
            }
        }
    }

}


// убрать rotation
// или оставить поворот --- из-за промежуточных
// нормально прикрепить расчёт привязки лейбла
export class Line extends Edge implements ILine {
    _id: string;
    _type: string = 'line';
    _dialect: string = 'base';
    _rotation?: number = 0;
    _startX: number;
    _startY: number;
    _endX: number ;
    _endY: number;
    _color?: string;
    _lineWidth: number = 1;

    _isEdgeDash: boolean = false;

    _label_info?: ILabel;
    _label?: Label;

    _info?: string;
    _wasInside: boolean = false;

    _points?: Array<{ x: number; y: number }>; //intermediate 
    _is_corners_rounded: boolean = false;
    _max_radius_of_corners: number = 7;

    // id_target???
    _target?: Node;
    _source?: Node;

    _target_connector_id?: string;
    _source_connector_id?: string;

    // for now
    _endArrow?: ArrowHead;
    _startArrow?: ArrowHead;

    _graph: Graph | null = null;

    attachToGraph(graph: Graph) {
        this._graph = graph;
    }

    //constructor(startX: number, startY: number, endX: number, endY: number, color?: string, rotation?: number, label?: string | null, info?: string) {
    constructor(line: ILine, endArrow?: string, startArrow?: string, target?: Node, source?: Node) {
        super();

        this._id = line.id
        //this.type = type
        //if (type != 'line') return;

        if (typeof line.rotation !== 'undefined') {
            this._rotation = line.rotation
        }
        this._startX = line.startX
        this._startY = line.startY
        this._endX = line.endX
        this._endY = line.endY
        this._color = line.color

        if (typeof line.isEdgeDash !== 'undefined') {
            this._isEdgeDash = line.isEdgeDash
        }

        if (typeof line.points !== 'undefined') {
            this._points = line.points.slice()
        }

        if (typeof line.is_corners_rounded !== 'undefined') {
            this._is_corners_rounded = line.is_corners_rounded
        }

        if (line.max_radius_of_corners) {
            this._max_radius_of_corners = line.max_radius_of_corners
        }

        // сделать рассчёты точек
        // делать отступ взависимости от высоты/длины стрелки
        if (typeof line.label_info !== 'undefined') {
            this._label_info = line.label_info

            if (typeof this._label_info.startX === 'undefined') {
                this._label_info.startX = this._startX
            }

            if (typeof this._label_info.startY === 'undefined') {
                this._label_info.startY = this._startY
            } 

            if (typeof this._label_info.endX === 'undefined') {
                this._label_info.endX = this._endX
            } 

            if (typeof this._label_info.endY === 'undefined') {
                this._label_info.endY = this._endY
            }

            this._label = new Label(this._label_info) 
        }

        this._info = line.info

        if (typeof line.lineWidth !== 'undefined') {
            this._lineWidth = line.lineWidth
        }

        // обработать отсутствие/наличие Вершин/координат

        if (typeof target !== 'undefined') {
            this._target = target
        }

        if (typeof source !== 'undefined') {
            this._source = source
        }

        //for now
        if (endArrow === 'triangle' || endArrow === ">") {
            this._endArrow = new TriangleArrowHead('end', this, this.color);
        } else if (endArrow === 'stick') {
            this._endArrow = new StickArrowHead('end', this, this.color);
        } else if (endArrow === 'line') {
            this._endArrow = new LineArrowHead('end', this, this.color);
        } else if (endArrow === 'crow') {
            this._endArrow = new CrowFootArrowHead('end', this, this.color);
        } 
        if (startArrow === 'triangle' || startArrow === ">") {
            this._startArrow = new TriangleArrowHead('start', this, this.color);
        } else if (startArrow === 'stick') {
            this._startArrow = new StickArrowHead('start', this, this.color);
        } else if (startArrow === 'line') {
            this._startArrow = new LineArrowHead('start', this, this.color);
        } else if (startArrow === 'crow') {
            this._startArrow = new CrowFootArrowHead('start', this, this.color);
        }

    }

    draw_canvas(ctx: CanvasRenderingContext2D) {
        if (!ctx) return;

        const radian = this.rotation! * Math.PI / 180;
        const centerX = (this.startX + this.endX) / 2;
        const centerY = (this.startY + this.endY) / 2;

        ctx.save();

        ctx.translate(centerX, centerY);
        ctx.rotate(radian);

        ctx.beginPath();
        ctx.moveTo(-((this.endX - this.startX) / 2), -((this.endY - this.startY) / 2));

        //rotation FIX
        if (typeof this.points !== 'undefined') {
            const path = [
                { x: this.startX - centerX, y: this.startY - centerY }, // Переводим в локальные координаты
                ...this.points.map(p => ({ x: p.x - centerX, y: p.y - centerY })),
                { x: this.endX - centerX, y: this.endY - centerY }
            ];

            if (this._is_corners_rounded) {
                // Рисуем первый сегмент до начала первого скругления
                let lastPoint = path[0];
                let nextDirection = { x: 0, y: 0 };

                for (let i = 0; i < path.length - 1; i++) {
                    const current = path[i];
                    const next = path[i + 1];

                    // Рассчитываем направление к следующей точке
                    const dx = next.x - current.x;
                    const dy = next.y - current.y;
                    const distance = Math.hypot(dx, dy);
                    const direction = {
                        x: dx / distance,
                        y: dy / distance
                    };

                    // Для первой точки просто двигаемся к следующей
                    if (i === 0) {
                        ctx.lineTo(current.x, current.y);
                        lastPoint = current;
                        nextDirection = direction;
                        continue;
                    }

                    // Рассчитываем безопасный радиус для скругления
                    const prevDistance = Math.hypot(
                        current.x - lastPoint.x,
                        current.y - lastPoint.y
                    );
                    const safeRadius = Math.min(
                        this._max_radius_of_corners,
                        prevDistance * 0.4,
                        distance * 0.4,
                        this._max_radius_of_corners - this.lineWidth
                    );

                    // Если радиус слишком мал - рисуем острый угол
                    if (safeRadius < 1) {
                        ctx.lineTo(current.x, current.y);
                        lastPoint = current;
                        nextDirection = direction;
                        continue;
                    }

                    // Рассчитываем точки скругления
                    const roundingStart = {
                        x: current.x - nextDirection.x * safeRadius,
                        y: current.y - nextDirection.y * safeRadius
                    };

                    const roundingEnd = {
                        x: current.x + direction.x * safeRadius,
                        y: current.y + direction.y * safeRadius
                    };

                    // Рисуем линию до начала скругления
                    ctx.lineTo(roundingStart.x, roundingStart.y);

                    // Рисуем плавное скругление как квадратичную кривую
                    ctx.quadraticCurveTo(
                        current.x,
                        current.y,
                        roundingEnd.x,
                        roundingEnd.y
                    );

                    lastPoint = roundingEnd;
                    nextDirection = direction;
                }



            } else {
                for (let i = 0; i < this.points.length; i++) {
                    const deltaX = this.points[i].x - centerX;
                    const deltaY = this.points[i].y - centerY;
                    //const newX = deltaX * Math.cos(radian) - deltaY * Math.sin(radian);
                    //const newY = deltaX * Math.sin(radian) + deltaY * Math.cos(radian);
                    //ctx.lineTo(newX, newY);
                    ctx.lineTo(deltaX, deltaY);
                }
            }            
        }

        ctx.lineTo(((this.endX - this.startX) / 2), ((this.endY - this.startY) / 2));

        if (typeof this.lineWidth !== 'undefined') {
            ctx.lineWidth = this.lineWidth;
        }

        if (typeof this.color !== 'undefined' && this.color.length > 0) {
            ctx.strokeStyle = this.color!;
        }

        if (this._isEdgeDash) {
            ctx.setLineDash([5, 3]);
        }

        ctx.stroke();
        ctx.closePath();

        if (this._isEdgeDash) {
            ctx.setLineDash([]);
        }

        //ctx.strokeStyle = '#0000ff';

        ctx.restore();

        //for now

        if (typeof this._endArrow !== 'undefined') {
            this._endArrow.draw_canvas(ctx)
        }

        if (typeof this._startArrow !== 'undefined') {
            this._startArrow.draw_canvas(ctx)
        }

        

        if (typeof this._label !== 'undefined') {
            this._label.draw_canvas(ctx)
        }
    }

    draw_hovered(ctx: CanvasRenderingContext2D) {
        if (!ctx) return;
        const radian = this.rotation! * Math.PI / 180;
        const centerX = (this.startX + this.endX) / 2;
        const centerY = (this.startY + this.endY) / 2;

        ctx.save();

        ctx.translate(centerX, centerY);
        ctx.rotate(radian);

        ctx.beginPath();
        ctx.moveTo(-((this.endX - this.startX) / 2), -((this.endY - this.startY) / 2));
        // без поворота
        if (typeof this.points !== 'undefined') {
            for (let i = 0; i < this.points.length; i++) {
                ctx.lineTo(this.points[i].x, this.points[i].y);
            }
        }
        ctx.lineTo(((this.endX - this.startX) / 2), ((this.endY - this.startY) / 2));
        if (typeof this.lineWidth !== 'undefined') {
            ctx.lineWidth = this.lineWidth;
        }
        //if (typeof this.color !== 'undefined' && this.color.length > 0) {
        ctx.strokeStyle = 'gold';
        //}
        ctx.stroke();
        ctx.closePath();

        ctx.restore();
    }

    draw_clicked(ctx: CanvasRenderingContext2D) {
        if (!ctx) return;
        const radian = this.rotation! * Math.PI / 180;
        const centerX = (this.startX + this.endX) / 2;
        const centerY = (this.startY + this.endY) / 2;

        ctx.save();

        ctx.translate(centerX, centerY);
        ctx.rotate(radian);

        ctx.beginPath();
        ctx.moveTo(-((this.endX - this.startX) / 2), -((this.endY - this.startY) / 2));
        // без поворота
        if (typeof this.points !== 'undefined') {
            for (let i = 0; i < this.points.length; i++) {
                ctx.lineTo(this.points[i].x, this.points[i].y);
            }
        }
        ctx.lineTo(((this.endX - this.startX) / 2), ((this.endY - this.startY) / 2));
        if (typeof this.lineWidth !== 'undefined') {
            ctx.lineWidth = this.lineWidth;
        }
        //if (typeof this.color !== 'undefined' && this.color.length > 0) {
        ctx.strokeStyle = '#b57281';
        //}
        ctx.stroke();
        ctx.closePath();

        ctx.restore();
    }

    //TODO:
    is_inside(mouseX: number, mouseY: number): boolean {
        // Простая проверка расстояния до линии
        const distance = this.pointToLineDistance(mouseX, mouseY,
            this._startX, this._startY, this._endX, this._endY);

        // Пороговое расстояние для клика (например, 5 пикселей)
        const threshold = 5;
        return distance <= threshold;
    }

    private pointToLineDistance(px: number, py: number,
        x1: number, y1: number,
        x2: number, y2: number): number {
        const A = px - x1;
        const B = py - y1;
        const C = x2 - x1;
        const D = y2 - y1;

        const dot = A * C + B * D;
        const lenSq = C * C + D * D;

        let param = -1;
        if (lenSq !== 0) {
            param = dot / lenSq;
        }

        let xx, yy;

        if (param < 0) {
            xx = x1;
            yy = y1;
        } else if (param > 1) {
            xx = x2;
            yy = y2;
        } else {
            xx = x1 + param * C;
            yy = y1 + param * D;
        }

        const dx = px - xx;
        const dy = py - yy;

        return Math.sqrt(dx * dx + dy * dy);
    }

    get id() {
        return this._id;
    }

    get type() {
        return this._type;
    }

    get rotation() {
        return this._rotation;
    }

    get startX() {
        return this._startX;
    }

    get startY() {
        return this._startY;
    }

    get endX() {
        return this._endX;
    }

    get endY() {
        return this._endY;
    }

    get color() {
        return this._color;
    }

    get lineWidth() {
        return this._lineWidth;
    }

    get label() {
        return this._label;
    }

    get points() {
        return this._points;
    }

    get info() {
        return this._info;
    }

    get wasInside() {
        return this._wasInside;
    }
}

export abstract class ArrowHead {
    
    abstract _type: string;

    abstract _color?: string;
    
    abstract _wasInside: boolean;

    //dialect

    //abstract _length: number;

    abstract _position: string;

    // id_edge???
    abstract _edge: Edge;

    //abstract _scale?: number;

    abstract draw_canvas(ctx: CanvasRenderingContext2D): void;

    abstract draw_hovered(ctx: CanvasRenderingContext2D): void;

    abstract draw_clicked(ctx: CanvasRenderingContext2D): void;

    abstract is_inside(mouseX: number, mouseY: number): void;
}


export class TriangleArrowHead extends ArrowHead {
    _type: string = 'triangle';

    _length: number = 10;
    //abstract _scale?: number;
    _position: string; //start, end, (custom???), midle(?)
    // id_edge???
    _edge: Edge;

    _color?: string;
    _wasInside: boolean = false;

    constructor(position: string, edge: Edge, color?: string, length?: number) {
        super();
        this._position = position;
        if (typeof length !== 'undefined') {
            this._length = length;
        }

        this._edge = edge;

        if (typeof color !== 'undefined') {
            this._color = color; // edge.color
        }

    }

    draw_canvas(ctx: CanvasRenderingContext2D) {

        let startX: number, endX: number;
        let startY: number, endY: number;

        if (this._position === 'end') {
            if (typeof this._edge._points !== 'undefined' && this._edge._points.length > 0) {
                const last_i = this._edge._points.length - 1
                startX = this._edge._points[last_i].x
                startY = this._edge._points[last_i].y
            } else {
                startX = this._edge._startX
                startY = this._edge._startY
            }
            endX = this._edge._endX
            endY = this._edge._endY
        } else if (this._position === 'start') {
            if (typeof this._edge._points !== 'undefined' && this._edge._points.length > 0) {
                startX = this._edge._points[0].x
                startY = this._edge._points[0].y
            } else {
                startX = this._edge._endX
                startY = this._edge._endY
            }
            endX = this._edge._startX
            endY = this._edge._startY
        }

        // if startX ... not defind

        // midle
        //const arrowX = (startX! + endX!) / 2;
        //const arrowY = (startY! + endY!) / 2;

        const arrowX = endX!; // - length / 2 ????
        const arrowY = endY!;

        const angle = Math.atan2(endY! - startY!, endX! - startX!);

        // Рисуем стрелку в середине линии
        ctx.beginPath();
        ctx.moveTo(arrowX, arrowY);
        ctx.lineTo(arrowX - this._length * Math.cos(angle - Math.PI / 6), arrowY - this._length * Math.sin(angle - Math.PI / 6));
        ctx.lineTo(arrowX - this._length * Math.cos(angle + Math.PI / 6), arrowY - this._length * Math.sin(angle + Math.PI / 6));
        ctx.lineTo(arrowX, arrowY);
        ctx.lineTo(arrowX - this._length * Math.cos(angle - Math.PI / 6), arrowY - this._length * Math.sin(angle - Math.PI / 6));
        ctx.closePath()

        if (typeof this._color !== 'undefined') {
            ctx.strokeStyle = this._color;
        }
        ctx.stroke();

        ctx.strokeStyle = 'black';

        if (typeof this._color !== 'undefined') {
            ctx.fillStyle = this._color;
            ctx.fill();
        }
    }

    draw_hovered(ctx: CanvasRenderingContext2D) {
        if (!ctx) return;

    }

    draw_clicked(ctx: CanvasRenderingContext2D) {
        if (!ctx) return;

    }

    //TODO:
    is_inside(mouseX: number, mouseY: number) {
        return (false
            //mouseX >= this.x - this.radius &&
            //mouseX <= this.x + this.radius &&
            //mouseY >= this.y - this.radius &&
            //mouseY <= this.y + this.radius
        )
    }
}

export class StickArrowHead extends ArrowHead {
    _type: string = 'stick';

    _length: number = 10;
    //abstract _scale?: number;
    _position: string; //start, end, (custom???), midle(?)
    // id_edge???
    _edge: Edge;

    _color?: string;
    _wasInside: boolean = false;

    constructor(position: string, edge: Edge, color?: string, length?: number) {
        super();
        this._position = position;
        if (typeof length !== 'undefined') {
            this._length = length;
        }

        this._edge = edge;

        if (typeof color !== 'undefined') {
            this._color = color; // edge.color
        }

    }

    draw_canvas(ctx: CanvasRenderingContext2D) {

        let startX: number, endX: number;
        let startY: number, endY: number;

        if (this._position === 'end') {
            if (typeof this._edge._points !== 'undefined' && this._edge._points.length > 0) {
                const last_i = this._edge._points.length - 1
                startX = this._edge._points[last_i].x
                startY = this._edge._points[last_i].y
            } else {
                startX = this._edge._startX
                startY = this._edge._startY
            }
            endX = this._edge._endX
            endY = this._edge._endY
        } else if (this._position === 'start') {
            if (typeof this._edge._points !== 'undefined' && this._edge._points.length > 0) {
                startX = this._edge._points[0].x
                startY = this._edge._points[0].y
            } else {
                startX = this._edge._endX
                startY = this._edge._endY
            }
            endX = this._edge._startX
            endY = this._edge._startY
        }

        // if startX ... not defind

        // midle
        //const arrowX = (startX! + endX!) / 2;
        //const arrowY = (startY! + endY!) / 2;

        const arrowX = endX!; // - length / 2 ????
        const arrowY = endY!;

        const angle = Math.atan2(endY! - startY!, endX! - startX!);

        ctx.beginPath();
        ctx.moveTo(arrowX, arrowY);
        ctx.lineTo(arrowX - this._length * Math.cos(angle - Math.PI / 6), arrowY - this._length * Math.sin(angle - Math.PI / 6));
        ctx.moveTo(arrowX, arrowY);
        ctx.lineTo(arrowX - this._length * Math.cos(angle + Math.PI / 6), arrowY - this._length * Math.sin(angle + Math.PI / 6));
        ctx.closePath()

        if (typeof this._color !== 'undefined') {
            ctx.strokeStyle = this._color;

        }
        ctx.stroke();
        ctx.strokeStyle = 'black';
    }

    draw_hovered(ctx: CanvasRenderingContext2D) {
        if (!ctx) return;

    }

    draw_clicked(ctx: CanvasRenderingContext2D) {
        if (!ctx) return;

    }

    //TODO:
    is_inside(mouseX: number, mouseY: number) {
        return (false
            //mouseX >= this.x - this.radius &&
            //mouseX <= this.x + this.radius &&
            //mouseY >= this.y - this.radius &&
            //mouseY <= this.y + this.radius
        )
    }
}

export class LineArrowHead extends ArrowHead {
    _type: string = 'line';

    _length: number = 6;
    //abstract _scale?: number;
    _position: string; //start, end, (custom???), midle(?)
    // id_edge???
    _edge: Edge;

    _color?: string;
    _wasInside: boolean = false;

    constructor(position: string, edge: Edge, color?: string, length?: number) {
        super();
        this._position = position;
        if (typeof length !== 'undefined') {
            this._length = length;
        }

        this._edge = edge;

        if (typeof color !== 'undefined') {
            this._color = color; // edge.color
        }

    }

    draw_canvas(ctx: CanvasRenderingContext2D) {

        let startX: number, endX: number;
        let startY: number, endY: number;

        if (this._position === 'end') {
            if (typeof this._edge._points !== 'undefined' && this._edge._points.length > 0) {
                const last_i = this._edge._points.length - 1
                startX = this._edge._points[last_i].x
                startY = this._edge._points[last_i].y
            } else {
                startX = this._edge._startX
                startY = this._edge._startY
            }
            endX = this._edge._endX
            endY = this._edge._endY
        } else if (this._position === 'start') {
            if (typeof this._edge._points !== 'undefined' && this._edge._points.length > 0) {
                startX = this._edge._points[0].x
                startY = this._edge._points[0].y
            } else {
                startX = this._edge._endX
                startY = this._edge._endY
            }
            endX = this._edge._startX
            endY = this._edge._startY
        }

        // if startX ... not defind

        // midle
        //const arrowX = (startX! + endX!) / 2;
        //const arrowY = (startY! + endY!) / 2;

        const arrowX = endX!; // - length / 2 ????
        const arrowY = endY!;

        const angle = Math.atan2(endY! - startY!, endX! - startX!);

        ctx.beginPath();
        
        ctx.moveTo(arrowX - this._length * Math.cos(angle - Math.PI / 6), arrowY - this._length * Math.sin(angle - Math.PI / 6));
        
        ctx.lineTo(arrowX - this._length * Math.cos(angle + Math.PI / 6), arrowY - this._length * Math.sin(angle + Math.PI / 6));

        ctx.closePath();

        if (typeof this._color !== 'undefined') {
            ctx.strokeStyle = this._color;
        }
        ctx.stroke();

        ctx.strokeStyle = 'black';
    }

    draw_hovered(ctx: CanvasRenderingContext2D) {
        if (!ctx) return;

    }

    draw_clicked(ctx: CanvasRenderingContext2D) {
        if (!ctx) return;

    }

    //TODO:
    is_inside(mouseX: number, mouseY: number) {
        return (false
            //mouseX >= this.x - this.radius &&
            //mouseX <= this.x + this.radius &&
            //mouseY >= this.y - this.radius &&
            //mouseY <= this.y + this.radius
        )
    }
}


export class CrowFootArrowHead extends ArrowHead {
    _type: string = 'crow'

    _length: number = 8;
    //abstract _scale?: number;
    _position: string; //start, end, (custom???), midle(?)
    // id_edge???
    _edge: Edge;

    _color?: string;
    _wasInside: boolean = false;

    constructor(position: string, edge: Edge, color?: string, length?: number) {
        super();
        this._position = position;
        if (typeof length !== 'undefined') {
            this._length = length;
        }

        this._edge = edge;

        if (typeof color !== 'undefined') {
            this._color = color; // edge.color
        }

    }

    draw_canvas(ctx: CanvasRenderingContext2D) {

        let startX: number, endX: number;
        let startY: number, endY: number;

        if (this._position === 'end') {
            if (typeof this._edge._points !== 'undefined' && this._edge._points.length > 0) {
                const last_i = this._edge._points.length - 1
                startX = this._edge._points[last_i].x
                startY = this._edge._points[last_i].y
            } else {
                startX = this._edge._startX
                startY = this._edge._startY
            }
            endX = this._edge._endX
            endY = this._edge._endY
        } else if (this._position === 'start') {
            if (typeof this._edge._points !== 'undefined' && this._edge._points.length > 0) {
                startX = this._edge._points[0].x
                startY = this._edge._points[0].y
            } else {
                startX = this._edge._endX
                startY = this._edge._endY
            }
            endX = this._edge._startX
            endY = this._edge._startY
        }

        // if startX ... not defind

        // midle
        //const arrowX = (startX! + endX!) / 2;
        //const arrowY = (startY! + endY!) / 2;

        const arrowX = endX!; // - length / 2 ????
        const arrowY = endY!;

        const angle = Math.atan2(endY! - startY!, endX! - startX!);

        ctx.save();
        ctx.translate(arrowX - this._length * Math.cos(angle), arrowY - this._length * Math.sin(angle));
        ctx.rotate(angle - Math.PI / 2); 

        ctx.beginPath();
        ctx.moveTo(0, 0);

        // Левый "палец"
        ctx.lineTo(-this._length * Math.cos(Math.PI / 3), this._length);
        ctx.moveTo(0, 0);

        // Правый "палец"
        ctx.lineTo(this._length * Math.cos(Math.PI / 3), this._length);
        
        ctx.closePath();

        if (typeof this._color !== 'undefined') {
            ctx.strokeStyle = this._color;

        }
        ctx.stroke();

        ctx.strokeStyle = 'black';

        ctx.restore(); // Восстанавливаем состояние контекста
        
    }

    draw_hovered(ctx: CanvasRenderingContext2D) {
        if (!ctx) return;

    }

    draw_clicked(ctx: CanvasRenderingContext2D) {
        if (!ctx) return;

    }

    //TODO:
    is_inside(mouseX: number, mouseY: number) {
        return (false
            //mouseX >= this.x - this.radius &&
            //mouseX <= this.x + this.radius &&
            //mouseY >= this.y - this.radius &&
            //mouseY <= this.y + this.radius
        )
    }
}


// русский
//implements ILabel
export class Label {
    _text: string
    _icon?: 'lock'
    _startX: number
    _startY: number
    _endX: number
    _endY: number

    _color?: string = 'black'
    _font?: string

    _padding: number = 0 // отступ от края

    //position:
    _alignment?: string = 'center'
    //baseline

    constructor(label: ILabel) {
        this._text = label.text
        this._icon = label.icon
        if (typeof label.startX !== 'undefined') {
            this._startX = label.startX
        } else throw Error('Invalid data');

        if (typeof label.startY !== 'undefined') {
            this._startY = label.startY
        } else throw Error('Invalid data');

        if (typeof label.endX !== 'undefined') {
            this._endX = label.endX
        } else throw Error('Invalid data');

        if (typeof label.endY !== 'undefined') {
            this._endY = label.endY
        } else throw Error('Invalid data');

        if (typeof label.color !== 'undefined') {
            this._color = label.color
        }
        if (typeof label.font !== 'undefined') {
            this._font = label.font
        }

        if (typeof label.padding !== 'undefined') {
            this._padding = label.padding
        }

        if (typeof label.alignment !== 'undefined') {
            this._alignment = label.alignment
        }

    }

    draw_canvas(ctx: CanvasRenderingContext2D) {
        if (!ctx) return;

        //if (!alignment) alignment = 'center';
        //if (!padding) padding = 0;

        const dx = this._endX - this._startX
        const dy = this._endY - this._startY
        const length = Math.sqrt(dx * dx + dy * dy) || 1

        let pointX, pointY, pad

        if (this._alignment == 'center') {
            pointX = this._startX
            pointY = this._startY
            pad = 1 / 2
        } else {
            const left = this._alignment == 'left'
            pointX = left ? this._startX : this._endX
            pointY = left ? this._startY : this._endY
            pad = this._padding / Math.sqrt(dx * dx + dy * dy) * (left ? 1 : -1);
        }

        ctx.save();
        if (['left', 'right', 'center', 'start', 'end'].includes(this._alignment!)) {
            ctx.textAlign = this._alignment as CanvasTextAlign;
        } else {
            console.error('Некорректное значение для textAlign');
        }
        ctx.translate(pointX + dx * pad, pointY + dy * pad);
        ctx.rotate(Math.atan2(dy, dx));
        if (typeof this._color !== 'undefined' && this._color.length > 0) {
            ctx.fillStyle = this._color;
        }
        if (typeof this._font !== 'undefined' && this._font.length > 0) {
            ctx.font = this._font;
        }

        ctx.textBaseline = 'middle';

        if (this._icon === 'lock') {
            this.drawLockIcon(ctx);
            ctx.restore();
            return;
        }

        //const working_text = this.stringToUnicode(this._text);

        //strokeText - будет оконтовка

        const words = this._text.split(' ');
        const lines = [];
        let line = '';

        for (let n = 0; n < words.length; n++) {
            const testLine = line + words[n] + ' ';
            const metrics = ctx.measureText(testLine);
            const testWidth = metrics.width;
            if (testWidth > dx && n > 0) {
                lines.push(line);
                line = words[n] + ' ';
            } else {
                line = testLine;
            }
        }
        if (line.trim()) lines.push(line.trim());

        // Вычисление высоты строки
        const { actualBoundingBoxAscent, actualBoundingBoxDescent } = ctx.measureText("M");
        const lineHeight = actualBoundingBoxAscent + actualBoundingBoxDescent;
        const totalTextHeight = lines.length * lineHeight;

        // Вектор перпендикуляра к линии
        const perpX = -dy / length;
        const perpY = dx / length;

        // Смещение вверх на половину высоты текста перпендикулярно линии
        const offsetY = -totalTextHeight / 2;

        // Отрисовка каждой строки
        lines.forEach((lineText, i) => {
            const y = i * lineHeight + offsetY;
            ctx.fillText(lineText, 0, y);
        });

        //ctx.fillText(this._text, 0, 0);
        ctx.restore();
    }

    private drawLockIcon(ctx: CanvasRenderingContext2D): void {
        const width = 24;
        const bodyHeight = 16;
        const bodyTop = -1;
        const shackleRadius = 7;

        ctx.save();
        ctx.lineWidth = 3;
        ctx.strokeStyle = '#455a64';
        ctx.fillStyle = '#455a64';

        ctx.beginPath();
        ctx.arc(0, bodyTop, shackleRadius, Math.PI, 0, false);
        ctx.lineTo(shackleRadius, bodyTop + 4);
        ctx.moveTo(-shackleRadius, bodyTop);
        ctx.lineTo(-shackleRadius, bodyTop + 4);
        ctx.stroke();

        ctx.fillRect(-width / 2, bodyTop + 3, width, bodyHeight);
        ctx.strokeRect(-width / 2, bodyTop + 3, width, bodyHeight);

        ctx.fillStyle = '#ffffff';
        ctx.beginPath();
        ctx.arc(0, bodyTop + 10, 2.5, 0, 2 * Math.PI);
        ctx.fill();
        ctx.fillRect(-1.25, bodyTop + 10, 2.5, 5);
        ctx.restore();
    }

    //get text () {
    //    return this._text
    //}
}
