import { Concept, Instance, FrameInstance } from './Core';

export class FrameGraphXmlGenerator {
    // Конфигурация позиционирования
    private config = {
        frameX: 200,      // Левая колонка - фреймы
        instanceX: 500,   // Средняя колонка - инстансы
        conceptX: 700,    // Правая колонка - концепты
        frameSpacing: 150,
        instanceSpacing: 120,
        conceptSpacing: 120,
        frameWidth: 130,
        frameHeight: 60,
        instanceRadius: 35,
        conceptRadiusX: 50,
        conceptRadiusY: 30
    };

    private state = {
        currentFrameY: 100,
        currentInstanceY: 100,
        currentConceptY: 100,
        edgeId: 1,
        colorIndex: 0
    };

    private nodePositions = new Map<string, {
        x: number;
        y: number;
        width: number;
        height: number;
        centerX: number;
        centerY: number;
    }>();

    private frameColorMap = new Map<string, string>();

    private readonly frameColors = [
        "#4CAF50", "#2196F3", "#9C27B0", "#FF9800", "#F44336",
        "#00BCD4", "#8BC34A", "#FF5722", "#795548", "#607D8B"
    ];

    private readonly conceptColorMap: Record<string, string> = {
        "Person": "#4CAF50",
        "User": "#2196F3",
        "Employee": "#FF9800",
        "Cook": "#9C27B0",
        "Courier": "#03A9F4",
        "Order": "#795548",
        "Student": "#4CAF50",
        "Teacher": "#2196F3",
        "Course": "#FF9800",
        "Grade": "#9C27B0",
        "default": "#607D8B"
    };

    private readonly instanceColorMap: Record<string, string> = {
        "Person": "#C8E6C9",
        "User": "#BBDEFB",
        "Employee": "#FFE0B2",
        "Cook": "#E1BEE7",
        "Courier": "#B3E5FC",
        "Order": "#D7CCC8",
        "Student": "#C8E6C9",
        "Teacher": "#BBDEFB",
        "Course": "#FFE0B2",
        "Grade": "#E1BEE7",
        "default": "#F5F5F5"
    };

    /**
     * Генерация XML диаграммы
     */
    generateDiagramXml(
        concepts: Concept[],
        instances: Instance[],
        relations: FrameInstance[]
    ): string {
        this.resetState();

        const xmlBuilder = new XmlBuilder();

        // Корневой элемент
        const graph = xmlBuilder.createElement("graph");

        // Canvas
        graph.appendChild(xmlBuilder.createElement("canvas", {
            height: "800",
            width: "800"
        }));

        // 1. Фреймы (прямоугольники слева)
        relations.forEach(relation => {
            graph.appendChild(this.createFrameNode(relation, xmlBuilder));
            this.state.currentFrameY += this.config.frameSpacing;
        });

        // 2. Экземпляры (круги посередине)
        instances.forEach(instance => {
            graph.appendChild(this.createInstanceNode(instance, xmlBuilder));
            this.state.currentInstanceY += this.config.instanceSpacing;
        });

        // 3. Концепты (овалы справа)
        concepts.forEach(concept => {
            graph.appendChild(this.createConceptNode(concept, xmlBuilder));
            this.state.currentConceptY += this.config.conceptSpacing;
        });

        // 4. Связи фрейм → инстанс
        relations.forEach(relation => {
            relation.roleValues.forEach((instance, roleName) => {
                const edge = this.createFrameToInstanceEdge(relation, roleName, instance, xmlBuilder);
                if (edge) graph.appendChild(edge);
            });
        });

        // 5. Связи инстанс → концепт (i)
        instances.forEach(instance => {
            const edge = this.createInstanceOfEdge(instance, xmlBuilder);
            if (edge) graph.appendChild(edge);
        });

        // 6. AND-треугольник (если есть фреймы)
        if (relations.length > 0) {
            graph.appendChild(this.createAndTriangle(relations, xmlBuilder));
            relations.forEach(relation => {
                const edge = this.createAndToFrameEdge(relation, xmlBuilder);
                if (edge) graph.appendChild(edge);
            });
        }

        return xmlBuilder.serialize(graph);
    }

    /**
     * Создание узла фрейма (прямоугольник)
     */
    private createFrameNode(frame: FrameInstance, xmlBuilder: XmlBuilder): Element {
        const frameId = `frame_${frame.id}`;
        const yPos = this.state.currentFrameY - this.config.frameHeight / 2;

        const node = xmlBuilder.createElement("node", {
            id: frameId,
            type: "rectangle",
            label: frame.frameType.name,
            rotation: "0"
        });

        node.appendChild(xmlBuilder.createElement("geometry", {
            x: this.config.frameX.toString(),
            y: yPos.toString(),
            width: this.config.frameWidth.toString(),
            height: this.config.frameHeight.toString()
        }));

        node.appendChild(xmlBuilder.createElement("background", {
            color: "#FF9800"  // Оранжевый для фреймов
        }));

        node.appendChild(xmlBuilder.createElement("edgeStyle", {
            isEdgeDash: "false"
        }));

        node.appendChild(xmlBuilder.createElement("labelSettings", {
            font: "Bold 14px Arial",
            color: "white"
        }));

        // Сохраняем позицию
        const centerX = this.config.frameX + this.config.frameWidth / 2;
        const centerY = this.state.currentFrameY;

        this.nodePositions.set(frameId, {
            x: this.config.frameX,
            y: yPos,
            width: this.config.frameWidth,
            height: this.config.frameHeight,
            centerX,
            centerY
        });

        return node;
    }

    /**
     * Создание узла экземпляра (круг)
     */
    private createInstanceNode(instance: Instance, xmlBuilder: XmlBuilder): Element {
        const instanceId = `instance_${instance.id}`;
        const color = this.instanceColorMap[instance.type.name] || this.instanceColorMap.default;

        const node = xmlBuilder.createElement("node", {
            id: instanceId,
            type: "ellipse",
            label: instance.name,
            rotation: "0"
        });

        node.appendChild(xmlBuilder.createElement("geometry", {
            x: this.config.instanceX.toString(),
            y: this.state.currentInstanceY.toString(),
            radius_x: this.config.instanceRadius.toString(),
            radius_y: this.config.instanceRadius.toString()
        }));

        node.appendChild(xmlBuilder.createElement("background", {
            color: color
        }));

        node.appendChild(xmlBuilder.createElement("edgeStyle", {
            isEdgeDash: "false"
        }));

        node.appendChild(xmlBuilder.createElement("labelSettings", {
            font: "14px Arial",
            color: "black"
        }));

        // Сохраняем позицию
        this.nodePositions.set(instanceId, {
            x: this.config.instanceX,
            y: this.state.currentInstanceY,
            width: this.config.instanceRadius * 2,
            height: this.config.instanceRadius * 2,
            centerX: this.config.instanceX,
            centerY: this.state.currentInstanceY
        });

        return node;
    }

    /**
     * Создание узла концепта (овал)
     */
    private createConceptNode(concept: Concept, xmlBuilder: XmlBuilder): Element {
        const conceptId = `concept_${concept.id}`;
        const color = this.conceptColorMap[concept.name] || this.conceptColorMap.default;

        const node = xmlBuilder.createElement("node", {
            id: conceptId,
            type: "ellipse",
            label: concept.name,
            rotation: "0"
        });

        node.appendChild(xmlBuilder.createElement("geometry", {
            x: this.config.conceptX.toString(),
            y: this.state.currentConceptY.toString(),
            radius_x: this.config.conceptRadiusX.toString(),
            radius_y: this.config.conceptRadiusY.toString()
        }));

        node.appendChild(xmlBuilder.createElement("background", {
            color: color
        }));

        node.appendChild(xmlBuilder.createElement("edgeStyle", {
            isEdgeDash: "false"
        }));

        node.appendChild(xmlBuilder.createElement("labelSettings", {
            font: "Bold 16px Arial",
            color: "white"
        }));

        // Сохраняем позицию
        this.nodePositions.set(conceptId, {
            x: this.config.conceptX,
            y: this.state.currentConceptY,
            width: this.config.conceptRadiusX * 2,
            height: this.config.conceptRadiusY * 2,
            centerX: this.config.conceptX,
            centerY: this.state.currentConceptY
        });

        return node;
    }

    /**
     * Создание связи фрейм → инстанс
     */
    private createFrameToInstanceEdge(
        frame: FrameInstance,
        roleName: string,
        instance: Instance,
        xmlBuilder: XmlBuilder
    ): Element | null {
        const frameId = `frame_${frame.id}`;
        const instanceId = `instance_${instance.id}`;

        if (!this.nodePositions.has(frameId) || !this.nodePositions.has(instanceId)) {
            return null;
        }

        const framePos = this.nodePositions.get(frameId)!;
        const instancePos = this.nodePositions.get(instanceId)!;

        // От правого центра фрейма к левому краю инстанса
        const startX = framePos.centerX + framePos.width / 2;
        const startY = framePos.centerY;
        const endX = instancePos.centerX - instancePos.width / 2;
        const endY = instancePos.centerY;

        const edgeId = `edge_${this.state.edgeId++}`;
        const edgeColor = this.getFrameEdgeColor(frame.id);

        const edge = xmlBuilder.createElement("edge", {
            id: edgeId,
            type: "line",
            label: roleName,
            rotation: "0",
            endArrow: "triangle",
            startArrow: "none"
        });

        edge.appendChild(xmlBuilder.createElement("geometry", {
            startX: startX.toString(),
            startY: startY.toString(),
            endX: endX.toString(),
            endY: endY.toString()
        }));

        edge.appendChild(xmlBuilder.createElement("background", {
            color: edgeColor
        }));

        edge.appendChild(xmlBuilder.createElement("edgeStyle", {
            lineWidth: "2",
            isRounded: "true",
            isEdgeDash: "false",
            maxRadiusOfCorners: "10"
        }));

        edge.appendChild(xmlBuilder.createElement("labelSettings", {
            font: "12px Arial",
            color: "red"
        }));

        return edge;
    }

    /**
     * Создание связи инстанс → концепт (i)
     */
    private createInstanceOfEdge(instance: Instance, xmlBuilder: XmlBuilder): Element | null {
        const instanceId = `instance_${instance.id}`;
        const conceptId = `concept_${instance.type.id}`;

        if (!this.nodePositions.has(instanceId) || !this.nodePositions.has(conceptId)) {
            return null;
        }

        const instancePos = this.nodePositions.get(instanceId)!;
        const conceptPos = this.nodePositions.get(conceptId)!;

        const startX = instancePos.centerX + instancePos.width / 2;
        const startY = instancePos.centerY;
        const endX = conceptPos.centerX - conceptPos.width / 2;
        const endY = conceptPos.centerY;

        const edgeId = `edge_${this.state.edgeId++}`;

        const edge = xmlBuilder.createElement("edge", {
            id: edgeId,
            type: "line",
            label: "i",
            rotation: "0",
            endArrow: "triangle",
            startArrow: "none"
        });

        edge.appendChild(xmlBuilder.createElement("geometry", {
            startX: startX.toString(),
            startY: startY.toString(),
            endX: endX.toString(),
            endY: endY.toString()
        }));

        edge.appendChild(xmlBuilder.createElement("background", {
            color: "#666666"
        }));

        edge.appendChild(xmlBuilder.createElement("edgeStyle", {
            lineWidth: "2",
            isRounded: "false",
            isEdgeDash: "false"
        }));

        edge.appendChild(xmlBuilder.createElement("labelSettings", {
            font: "Bold 14px Arial",
            color: "#666666"
        }));

        return edge;
    }

    /**
     * Создание AND-треугольника
     */
    private createAndTriangle(relations: FrameInstance[], xmlBuilder: XmlBuilder): Element {
        const triangleX = this.config.frameX - 100;
        const triangleY = 250;

        const topX = triangleX;
        const topY = triangleY - 40;
        const leftX = triangleX - 40;
        const leftY = triangleY + 40;
        const rightX = triangleX + 40;
        const rightY = triangleY + 40;

        const node = xmlBuilder.createElement("node", {
            id: "and_triangle",
            type: "triangle",
            label: "AND",
            rotation: "0"
        });

        node.appendChild(xmlBuilder.createElement("geometry", {
            x1: topX.toString(),
            y1: topY.toString(),
            x2: leftX.toString(),
            y2: leftY.toString(),
            x3: rightX.toString(),
            y3: rightY.toString()
        }));

        node.appendChild(xmlBuilder.createElement("background", {
            color: "#9C27B0"
        }));

        node.appendChild(xmlBuilder.createElement("labelSettings", {
            font: "Bold 16px Arial",
            color: "white"
        }));

        // Сохраняем позицию
        this.nodePositions.set("and_triangle", {
            x: triangleX,
            y: triangleY,
            width: 80,
            height: 80,
            centerX: triangleX,
            centerY: triangleY
        });

        return node;
    }

    /**
     * Создание связи AND-треугольник → фрейм
     */
    private createAndToFrameEdge(frame: FrameInstance, xmlBuilder: XmlBuilder): Element | null {
        const frameId = `frame_${frame.id}`;

        if (!this.nodePositions.has("and_triangle") || !this.nodePositions.has(frameId)) {
            return null;
        }

        const andPos = this.nodePositions.get("and_triangle")!;
        const framePos = this.nodePositions.get(frameId)!;

        const startX = andPos.centerX + 30;
        const startY = andPos.centerY + 40;
        const endX = framePos.centerX - framePos.width / 2;
        const endY = framePos.centerY;

        const edgeId = `edge_${this.state.edgeId++}`;

        const edge = xmlBuilder.createElement("edge", {
            id: edgeId,
            type: "line",
            label: "",
            rotation: "0",
            endArrow: "triangle",
            startArrow: "none"
        });

        edge.appendChild(xmlBuilder.createElement("geometry", {
            startX: startX.toString(),
            startY: startY.toString(),
            endX: endX.toString(),
            endY: endY.toString()
        }));

        edge.appendChild(xmlBuilder.createElement("background", {
            color: "#9C27B0"
        }));

        edge.appendChild(xmlBuilder.createElement("edgeStyle", {
            lineWidth: "2",
            isRounded: "true",
            isEdgeDash: "false",
            maxRadiusOfCorners: "10"
        }));

        // Контрольная точка для изгиба
        const internalPoints = xmlBuilder.createElement("internalPoints");
        internalPoints.appendChild(xmlBuilder.createElement("internalPoint", {
            x: (endX - 50).toString(),
            y: endY.toString()
        }));
        edge.appendChild(internalPoints);

        edge.appendChild(xmlBuilder.createElement("labelSettings", {
            font: "12px Arial",
            color: "#9C27B0"
        }));

        return edge;
    }

    /**
     * Получить цвет для связей фрейма
     */
    private getFrameEdgeColor(frameId: string): string {
        if (this.frameColorMap.has(frameId)) {
            return this.frameColorMap.get(frameId)!;
        }

        const color = this.frameColors[this.state.colorIndex % this.frameColors.length];
        this.frameColorMap.set(frameId, color);
        this.state.colorIndex++;

        return color;
    }

    /**
     * Сброс состояния генератора
     */
    private resetState(): void {
        this.state = {
            currentFrameY: 100,
            currentInstanceY: 100,
            currentConceptY: 100,
            edgeId: 1,
            colorIndex: 0
        };
        this.nodePositions.clear();
        this.frameColorMap.clear();
    }
}

/**
 * Вспомогательный класс для построения XML
 */
class XmlBuilder {
    private doc: Document;

    constructor() {
        this.doc = document.implementation.createDocument(null, null, null);
    }

    createElement(tagName: string, attributes: Record<string, string> = {}): Element {
        const element = this.doc.createElement(tagName);

        Object.entries(attributes).forEach(([key, value]) => {
            element.setAttribute(key, value);
        });

        return element;
    }

    appendChild(parent: Element, child: Element): void {
        parent.appendChild(child);
    }

    serialize(element: Element): string {
        const xmlString = new XMLSerializer().serializeToString(element);
        return `<?xml version="1.0" encoding="utf-8"?>\n${xmlString}`;
    }
}