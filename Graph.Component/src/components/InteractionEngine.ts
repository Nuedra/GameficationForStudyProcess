// InteractionEngine.ts
import { Graph, GraphItem } from './Graph';
import { EnhancedDialect, IReactivityRule, IRuleContext, IStyleInfo } from './Dialect';

export interface IInteractionEngine {
    processEvent(eventName: string, target: GraphItem, eventData?: any): Promise<void>;
    registerCustomAction(name: string, handler: (context: IRuleContext) => void): void;
    getActiveRules(): IReactivityRule[];
    enable(): void;
    disable(): void;
    isEnabled(): boolean;
}

export class InteractionEngine implements IInteractionEngine {
    private graph: Graph;
    private dialect: EnhancedDialect;
    private enabled: boolean = true;
    private ruleIndex: Map<string, IReactivityRule[]> = new Map();
    private customActions: Map<string, (context: IRuleContext) => void> = new Map();
    private eventListeners: Map<string, Function[]> = new Map();
    private lastProcessedEvent: { eventName: string; targetId: string; timestamp: number } | null = null;

    constructor(graph: Graph, dialect: EnhancedDialect) {
        this.graph = graph;
        this.dialect = dialect;
        this.buildRuleIndex();
        this.registerDefaultActions();
    }

    private buildRuleIndex(): void {
        this.ruleIndex.clear();
        this.dialect.reactivityRules.forEach(rule => {
            const key = `${rule.trigger}:${rule.targetType}`;
            if (!this.ruleIndex.has(key)) {
                this.ruleIndex.set(key, []);
            }
            this.ruleIndex.get(key)!.push(rule);

            // Также индексируем по только событию для wildcard правил
            const eventKey = `${rule.trigger}:*`;
            if (!this.ruleIndex.has(eventKey)) {
                this.ruleIndex.set(eventKey, []);
            }
            this.ruleIndex.get(eventKey)!.push(rule);
        });
    }

    private registerDefaultActions(): void {
        // Действие: установить свойство
        this.registerCustomAction('setProperty', (context: IRuleContext) => {
            const { target, actionParams } = context;
            if (target && actionParams) {
                const { property, value } = actionParams;
                if (property && value !== undefined) {
                    // Динамическое свойство
                    if (property.startsWith('custom.')) {
                        const customProp = property.substring(7);
                        if (target.setCustomAttribute) {
                            target.setCustomAttribute(customProp, value);
                        }
                    } else {
                        // Стандартное свойство
                        const propName = `_${property}`;
                        if (propName in target) {
                            (target as any)[propName] = value;
                        }
                    }
                    if (this.graph.requestRedraw) {
                        this.graph.requestRedraw();
                    }
                }
            }
        });

        // Действие: применить стиль
        this.registerCustomAction('applyStyle', (context: IRuleContext) => {
            const { target, actionParams } = context;
            if (target && actionParams) {
                const { styleName } = actionParams;
                if (styleName) {
                    const style = this.dialect.getStyleTemplate(styleName);
                    if (style && target.applyStyle) {
                        target.applyStyle(style);
                        if (this.graph.requestRedraw) {
                            this.graph.requestRedraw();
                        }
                    }
                }
            }
        });

        // Действие: выделить элемент
        this.registerCustomAction('select', (context: IRuleContext) => {
            const { target, graph } = context;
            if (target && graph && graph.canSelect) {
                graph.canSelect(target);
            }
        });

        // Действие: снять выделение
        this.registerCustomAction('deselect', (context: IRuleContext) => {
            const { target, graph } = context;
            if (target && graph && graph.deselect) {
                graph.deselect(target);
            }
        });

        // Действие: добавить акцент
        this.registerCustomAction('emphasize', (context: IRuleContext) => {
            const { target, graph } = context;
            if (target && graph && graph.emphasize) {
                graph.emphasize(target);
            }
        });

        // Действие: снять акцент
        this.registerCustomAction('deemphasize', (context: IRuleContext) => {
            const { target, graph } = context;
            if (target && graph && graph.deEmphasize) {
                graph.deEmphasize(target);
            }
        });

        // Действие: переключить свойство
        this.registerCustomAction('toggleProperty', (context: IRuleContext) => {
            const { target, actionParams } = context;
            if (target && actionParams) {
                const { property } = actionParams;
                if (property) {
                    const propName = `_${property}`;
                    if (propName in target) {
                        (target as any)[propName] = !(target as any)[propName];
                        if (this.graph.requestRedraw) {
                            this.graph.requestRedraw();
                        }
                    }
                }
            }
        });

        // Действие: вызвать событие
        this.registerCustomAction('emitEvent', (context: IRuleContext) => {
            const { actionParams } = context;
            if (actionParams) {
                const { eventName, eventData } = actionParams;
                this.emit('customEvent', { eventName, eventData, context });
            }
        });

        // Действие: анимировать элемент
        this.registerCustomAction('animate', (context: IRuleContext) => {
            const { target, actionParams } = context;
            if (target && actionParams) {
                const { animationType, duration = 1000 } = actionParams;
                // Простая реализация анимации
                if (animationType === 'pulse') {
                    this.animatePulse(target, duration);
                }
            }
        });
    }

    private animatePulse(target: any, duration: number): void {
        const originalColor = target._color;
        const steps = 10;
        const stepDuration = duration / steps;

        let step = 0;
        const animateStep = () => {
            if (step >= steps) {
                target._color = originalColor;
                if (this.graph.requestRedraw) {
                    this.graph.requestRedraw();
                }
                return;
            }

            const intensity = Math.sin((step / steps) * Math.PI);
            target._color = this.adjustColorBrightness(originalColor, intensity * 0.5);
            if (this.graph.requestRedraw) {
                this.graph.requestRedraw();
            }

            step++;
            setTimeout(animateStep, stepDuration);
        };

        animateStep();
    }

    private adjustColorBrightness(color: string, factor: number): string {
        // Простая реализация изменения яркости цвета
        if (color.startsWith('#')) {
            const hex = color.substring(1);
            const rgb = parseInt(hex, 16);
            const r = Math.min(255, Math.max(0, ((rgb >> 16) & 0xFF) * (1 + factor)));
            const g = Math.min(255, Math.max(0, ((rgb >> 8) & 0xFF) * (1 + factor)));
            const b = Math.min(255, Math.max(0, (rgb & 0xFF) * (1 + factor)));
            return `#${Math.round(r).toString(16).padStart(2, '0')}${Math.round(g).toString(16).padStart(2, '0')}${Math.round(b).toString(16).padStart(2, '0')}`;
        }
        return color;
    }

    async processEvent(eventName: string, target: GraphItem, eventData?: any): Promise<void> {
        if (!this.enabled) return;

        // Защита от слишком частых событий
        const now = Date.now();
        if (this.lastProcessedEvent &&
            this.lastProcessedEvent.targetId === target._id &&
            this.lastProcessedEvent.eventName === eventName &&
            now - this.lastProcessedEvent.timestamp < 50) {
            return;
        }

        this.lastProcessedEvent = {
            eventName,
            targetId: target._id,
            timestamp: now
        };

        // Получаем правила для этого события и типа цели
        const rules = this.getRulesForEvent(eventName, target._type);

        // Выполняем правила по порядку приоритета
        for (const rule of rules) {
            try {
                await this.executeRule(rule, target, eventData);
            } catch (error) {
                console.error(`Ошибка выполнения правила ${rule.id}:`, error);
            }
        }

        // Вызываем пользовательские обработчики событий
        this.emit(eventName, { target, eventData });
    }

    private getRulesForEvent(eventName: string, targetType: string): IReactivityRule[] {
        const specificKey = `${eventName}:${targetType}`;
        const wildcardKey = `${eventName}:*`;

        const specificRules = this.ruleIndex.get(specificKey) || [];
        const wildcardRules = this.ruleIndex.get(wildcardKey) || [];

        // Объединяем и сортируем по приоритету
        return [...specificRules, ...wildcardRules]
            .sort((a, b) => (b.priority || 0) - (a.priority || 0))
            .filter((rule, index, self) =>
                index === self.findIndex(r => r.id === rule.id)
            );
    }

    private async executeRule(rule: IReactivityRule, target: GraphItem, eventData?: any): Promise<boolean> {
        // Проверяем условие
        if (rule.condition) {
            try {
                const conditionResult = this.evaluateCondition(rule.condition, target, eventData);
                if (!conditionResult) {
                    return false;
                }
            } catch (error) {
                console.error(`Ошибка оценки условия в правиле ${rule.id}:`, error);
                return false;
            }
        }

        // Выполняем действие
        const context: IRuleContext = {
            target,
            eventData,
            graph: this.graph,
            dialect: this.dialect,
            actionParams: rule.actionParams
        };

        const actionHandler = this.customActions.get(rule.action);
        if (actionHandler) {
            try {
                await actionHandler(context);
                this.emit('ruleExecuted', { rule, target, success: true });
                return true;
            } catch (error) {
                console.error(`Ошибка выполнения действия в правиле ${rule.id}:`, error);
                this.emit('ruleExecuted', { rule, target, success: false, error });
                return false;
            }
        } else {
            console.warn(`Действие "${rule.action}" не найдено в правиле ${rule.id}`);
            return false;
        }
    }

    private evaluateCondition(condition: string, target: GraphItem, eventData?: any): boolean {
        // Создаем безопасный контекст для оценки условий
        const context = {
            target: this.createSafeTargetProxy(target),
            event: eventData,
            graph: this.graph,
            // Безопасные математические функции
            Math: {
                abs: Math.abs,
                floor: Math.floor,
                ceil: Math.ceil,
                round: Math.round,
                max: Math.max,
                min: Math.min,
                random: Math.random,
                sqrt: Math.sqrt
            }
        };

        try {
            // Используем Function constructor для безопасной оценки
            const func = new Function('ctx', `with(ctx) { return ${condition} }`);
            return !!func(context);
        } catch (error) {
            console.error('Ошибка оценки условия:', error);
            return false;
        }
    }

    private createSafeTargetProxy(target: GraphItem): any {
        return new Proxy(target, {
            get(obj, prop) {
                // Разрешаем доступ только к определенным свойствам
                const safeProps = ['_id', '_type', '_color', '_isSelected', '_isEmphasized', 'x', 'y', 'width', 'height', 'radius'];
                if (typeof prop === 'string' && safeProps.includes(prop)) {
                    return (obj as any)[prop];
                }
                if (prop === 'getCustomAttribute' && typeof (obj as any).getCustomAttribute === 'function') {
                    return (key: string) => (obj as any).getCustomAttribute(key);
                }
                return undefined;
            },
            set() {
                return false; // Запрещаем изменения
            }
        });
    }

    registerCustomAction(name: string, handler: (context: IRuleContext) => void): void {
        this.customActions.set(name, handler);
    }

    on(event: string, handler: Function): void {
        if (!this.eventListeners.has(event)) {
            this.eventListeners.set(event, []);
        }
        this.eventListeners.get(event)!.push(handler);
    }

    off(event: string, handler?: Function): void {
        if (!this.eventListeners.has(event)) return;

        if (handler) {
            const handlers = this.eventListeners.get(event)!;
            const index = handlers.indexOf(handler);
            if (index > -1) {
                handlers.splice(index, 1);
            }
        } else {
            this.eventListeners.delete(event);
        }
    }

    private emit(event: string, data?: any): void {
        if (this.eventListeners.has(event)) {
            this.eventListeners.get(event)!.forEach(handler => {
                try {
                    handler(data);
                } catch (error) {
                    console.error(`Ошибка в обработчике события ${event}:`, error);
                }
            });
        }
    }

    getActiveRules(): IReactivityRule[] {
        return this.dialect.reactivityRules;
    }

    enable(): void {
        this.enabled = true;
    }

    disable(): void {
        this.enabled = false;
    }

    isEnabled(): boolean {
        return this.enabled;
    }

    updateDialect(newDialect: EnhancedDialect): void {
        this.dialect = newDialect;
        this.buildRuleIndex();
    }

    addRule(rule: IReactivityRule): void {
        this.dialect.addRule(rule);
        this.buildRuleIndex();
    }

    removeRule(ruleId: string): boolean {
        const result = this.dialect.removeRule(ruleId);
        if (result) {
            this.buildRuleIndex();
        }
        return result;
    }

    getRuleById(ruleId: string): IReactivityRule | undefined {
        return this.dialect.reactivityRules.find(rule => rule.id === ruleId);
    }

    clearAllRules(): void {
        this.dialect.reactivityRules = [];
        this.buildRuleIndex();
    }
}