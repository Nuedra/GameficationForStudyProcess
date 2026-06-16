// FrameLogic/Core.ts

// ===== БАЗОВЫЕ ТИПЫ И ИНТЕРФЕЙСЫ =====
export interface IOntologyElement {
    id: string;
    name: string;
}

// ===== КОНЦЕПТЫ =====
export class Concept implements IOntologyElement {
    constructor(
        public id: string,
        public name: string,
        public parent?: Concept
    ) { }

    /**
     * Является ли данный концепт подтипом другого
     */
    isSubtypeOf(other: Concept): boolean {
        let current: Concept | undefined = this;
        while (current) {
            if (current.id === other.id) return true;
            current = current.parent;
        }
        return false;
    }

    /**
     * Получить цепочку наследования
     */
    getInheritanceChain(): Concept[] {
        const chain: Concept[] = [];
        let current: Concept | undefined = this;
        while (current) {
            chain.unshift(current);
            current = current.parent;
        }
        return chain;
    }

    /**
     * Проверить, является ли концепт листом в иерархии
     */
    isLeaf(concepts: Concept[]): boolean {
        return !concepts.some(c => c.parent?.id === this.id);
    }

    /**
     * Получить все дочерние концепты
     */
    getChildren(concepts: Concept[]): Concept[] {
        return concepts.filter(c => c.parent?.id === this.id);
    }

    toString(): string {
        return `Концепт[${this.id}]: ${this.name}`;
    }
}

// ===== ЭКЗЕМПЛЯРЫ =====
export class Instance implements IOntologyElement {
    attributes: Map<string, any> = new Map();

    constructor(
        public id: string,
        public name: string,
        public type: Concept
    ) { }

    /**
     * Является ли экземпляром концепта
     */
    isInstanceOf(concept: Concept): boolean {
        return this.type.isSubtypeOf(concept);
    }

    /**
     * Установить атрибут
     */
    setAttribute(key: string, value: any): void {
        this.attributes.set(key, value);
    }

    /**
     * Получить атрибут
     */
    getAttribute(key: string): any | null {
        return this.attributes.get(key) || null;
    }

    /**
     * Получить все атрибуты
     */
    getAllAttributes(): Array<{ key: string, value: any }> {
        return Array.from(this.attributes.entries()).map(([key, value]) => ({ key, value }));
    }

    /**
     * Получить полное имя с типом
     */
    getFullName(): string {
        return `${this.name} (${this.type.name})`;
    }

    toString(): string {
        return `Экземпляр[${this.id}]: ${this.name} (тип: ${this.type.name})`;
    }
}

// ===== ФРЕЙМЫ =====
export class Frame implements IOntologyElement {
    roles: Map<string, Concept> = new Map();

    constructor(
        public id: string,
        public name: string,
        public parent?: Frame
    ) {
        // Наследуем роли от родителя
        if (parent) {
            parent.roles.forEach((value, key) => {
                this.roles.set(key, value);
            });
        }
    }

    /**
     * Добавить роль
     */
    addRole(roleName: string, roleType: Concept): void {
        this.roles.set(roleName, roleType);
    }

    /**
     * Удалить роль
     */
    removeRole(roleName: string): boolean {
        return this.roles.delete(roleName);
    }

    /**
     * Получить все роли (включая унаследованные)
     */
    getAllRoles(): Map<string, Concept> {
        return new Map(this.roles);
    }

    /**
     * Получить только собственные роли (не унаследованные)
     */
    getOwnRoles(): Map<string, Concept> {
        if (!this.parent) return new Map(this.roles);

        const ownRoles = new Map<string, Concept>();
        this.roles.forEach((value, key) => {
            if (!this.parent?.roles.has(key) || this.parent.roles.get(key)?.id !== value.id) {
                ownRoles.set(key, value);
            }
        });
        return ownRoles;
    }

    /**
     * Проверить наличие роли
     */
    hasRole(roleName: string): boolean {
        return this.roles.has(roleName);
    }

    /**
     * Получить концепт роли
     */
    getRoleType(roleName: string): Concept | undefined {
        return this.roles.get(roleName);
    }

    /**
     * Является ли подфреймом другого фрейма
     */
    isSubframeOf(other: Frame): boolean {
        let current: Frame | undefined = this;
        while (current) {
            if (current.id === other.id) return true;
            current = current.parent;
        }
        return false;
    }

    /**
     * Получить цепочку наследования фреймов
     */
    getFrameInheritanceChain(): Frame[] {
        const chain: Frame[] = [];
        let current: Frame | undefined = this;
        while (current) {
            chain.unshift(current);
            current = current.parent;
        }
        return chain;
    }

    /**
     * Создать экземпляр фрейма
     */
    instantiate(id: string, name: string, roleFillers: Map<string, Instance>): FrameInstance {
        // Проверка заполнения всех обязательных ролей
        this.roles.forEach((roleType, roleName) => {
            if (!roleFillers.has(roleName)) {
                throw new Error(`Не заполнена роль '${roleName}' во фрейме '${this.name}'`);
            }

            const filler = roleFillers.get(roleName)!;
            if (!filler.isInstanceOf(roleType)) {
                throw new Error(
                    `Значение '${filler.name}' не соответствует концепту роли '${roleName}: ${roleType.name}'`
                );
            }
        });

        return new FrameInstance(id, name, this, roleFillers);
    }

    /**
     * Проверить возможность создания экземпляра
     */
    canInstantiate(roleFillers: Map<string, Instance>): { valid: boolean; errors: string[] } {
        const errors: string[] = [];

        this.roles.forEach((roleType, roleName) => {
            if (!roleFillers.has(roleName)) {
                errors.push(`Отсутствует роль '${roleName}'`);
            } else {
                const filler = roleFillers.get(roleName)!;
                if (!filler.isInstanceOf(roleType)) {
                    errors.push(`Роль '${roleName}' ожидает ${roleType.name}, получено ${filler.type.name}`);
                }
            }
        });

        // Проверка лишних ролей
        roleFillers.forEach((_, roleName) => {
            if (!this.roles.has(roleName)) {
                errors.push(`Неизвестная роль '${roleName}' для фрейма '${this.name}'`);
            }
        });

        return {
            valid: errors.length === 0,
            errors
        };
    }

    toString(): string {
        const roles = Array.from(this.roles.entries())
            .map(([name, concept]) => `${name}: ${concept.name}`)
            .join(', ');
        return `Фрейм[${this.id}]: ${this.name} (роли: ${roles})`;
    }
}

// ===== ЭКЗЕМПЛЯРЫ ФРЕЙМОВ =====
export class FrameInstance implements IOntologyElement {
    constructor(
        public id: string,
        public name: string,
        public frameType: Frame,
        public roleValues: Map<string, Instance>
    ) { }

    /**
     * Является ли экземпляром фрейма
     */
    isInstanceOf(frame: Frame): boolean {
        return this.frameType.isSubframeOf(frame);
    }

    /**
     * Получить значение роли
     */
    getRoleValue(roleName: string): Instance | undefined {
        return this.roleValues.get(roleName);
    }

    /**
     * Получить все роли со значениями
     */
    getAllRolesWithValues(): Array<{ roleName: string, roleType: Concept, value: Instance }> {
        return Array.from(this.frameType.roles.entries()).map(([roleName, roleType]) => ({
            roleName,
            roleType,
            value: this.roleValues.get(roleName)!
        }));
    }

    /**
     * Проверить, содержит ли экземпляр
     */
    containsInstance(instanceId: string): boolean {
        return Array.from(this.roleValues.values()).some(i => i.id === instanceId);
    }

    /**
     * Проверить, содержит ли роль с экземпляром типа
     */
    hasRoleWithInstanceType(conceptId: string): boolean {
        return Array.from(this.roleValues.values()).some(i => i.type.id === conceptId);
    }

    toString(): string {
        const roles = Array.from(this.roleValues.entries())
            .map(([role, instance]) => `${role}: ${instance.name}`)
            .join(', ');
        return `Отношение[${this.id}]: ${this.name} (${roles})`;
    }
}

// ===== УТИЛИТЫ ВАЛИДАЦИИ =====
export class OntologyValidator {
    /**
     * Проверить уникальность ID
     */
    static checkUniqueIds(elements: IOntologyElement[]): { valid: boolean; duplicates: string[] } {
        const ids = new Map<string, number>();
        elements.forEach(el => {
            ids.set(el.id, (ids.get(el.id) || 0) + 1);
        });

        const duplicates = Array.from(ids.entries())
            .filter(([_, count]) => count > 1)
            .map(([id]) => id);

        return {
            valid: duplicates.length === 0,
            duplicates
        };
    }

    /**
     * Проверить циклические зависимости в наследовании концептов
     */
    static checkConceptCycles(concepts: Concept[]): { valid: boolean; cycles: string[][] } {
        const cycles: string[][] = [];
        const visited = new Set<string>();
        const recursionStack = new Set<string>();

        function dfs(conceptId: string, path: string[]): void {
            if (recursionStack.has(conceptId)) {
                // Найден цикл
                const startIndex = path.indexOf(conceptId);
                cycles.push(path.slice(startIndex));
                return;
            }

            if (visited.has(conceptId)) return;

            visited.add(conceptId);
            recursionStack.add(conceptId);

            const concept = concepts.find(c => c.id === conceptId);
            if (concept?.parent) {
                dfs(concept.parent.id, [...path, concept.parent.id]);
            }

            recursionStack.delete(conceptId);
        }

        concepts.forEach(concept => {
            if (!visited.has(concept.id)) {
                dfs(concept.id, [concept.id]);
            }
        });

        return {
            valid: cycles.length === 0,
            cycles
        };
    }

    /**
     * Проверить целостность экземпляров
     */
    static validateInstances(instances: Instance[], concepts: Concept[]): string[] {
        const errors: string[] = [];
        const conceptIds = new Set(concepts.map(c => c.id));

        instances.forEach(instance => {
            if (!conceptIds.has(instance.type.id)) {
                errors.push(`Экземпляр ${instance.name} ссылается на несуществующий концепт ${instance.type.id}`);
            }
        });

        return errors;
    }

    /**
     * Проверить целостность фреймов
     */
    static validateFrames(frames: Frame[], concepts: Concept[]): string[] {
        const errors: string[] = [];
        const conceptIds = new Set(concepts.map(c => c.id));

        frames.forEach(frame => {
            frame.roles.forEach((concept, roleName) => {
                if (!conceptIds.has(concept.id)) {
                    errors.push(`Фрейм ${frame.name} имеет роль ${roleName} с несуществующим концептом ${concept.id}`);
                }
            });
        });

        return errors;
    }
}

// ===== ПРИМЕРЫ ДОМЕННЫХ ОБЛАСТЕЙ =====

/**
 * Создать пример предметной области "Доставка еды"
 */
export function createDeliveryExample() {
    // Концепты
    const person = new Concept("CONCEPT_PERSON", "Person");
    const user = new Concept("CONCEPT_USER", "User", person);
    const employee = new Concept("CONCEPT_EMPLOYEE", "Employee", person);
    const cook = new Concept("CONCEPT_COOK", "Cook", employee);
    const courier = new Concept("CONCEPT_COURIER", "Courier", employee);
    const order = new Concept("CONCEPT_ORDER", "Order");

    // Фреймы
    const executesOrder = new Frame("FRAME_EXECUTES_ORDER", "ВыполняетЗаказ");
    executesOrder.addRole("a", employee);
    executesOrder.addRole("o", order);

    const cooksOrder = new Frame("FRAME_COOKS_ORDER", "ГотовитЗаказ", executesOrder);
    cooksOrder.addRole("a", cook);

    const deliversOrder = new Frame("FRAME_DELIVERS_ORDER", "ДоставляетЗаказ", executesOrder);
    deliversOrder.addRole("a", courier);

    const placesOrder = new Frame("FRAME_PLACES_ORDER", "РазмещаетЗаказ");
    placesOrder.addRole("a", user);
    placesOrder.addRole("o", order);

    // Экземпляры
    const user1 = new Instance("USER_001", "Анна Карпова", user);
    const cook1 = new Instance("COOK_001", "Мария Иванова", cook);
    const courier1 = new Instance("COURIER_001", "Алексей Егоров", courier);
    const order1 = new Instance("ORDER_001", "Пицца Маргарита", order);
    const order2 = new Instance("ORDER_002", "Суши сет", order);

    // Экземпляры фреймов
    const placesOrder1 = placesOrder.instantiate(
        "REL_001",
        "Размещение заказа 1",
        new Map([
            ["a", user1],
            ["o", order1]
        ])
    );

    const cooksOrder1 = cooksOrder.instantiate(
        "REL_002",
        "Приготовление заказа 1",
        new Map([
            ["a", cook1],
            ["o", order1]
        ])
    );

    const deliversOrder1 = deliversOrder.instantiate(
        "REL_003",
        "Доставка заказа 1",
        new Map([
            ["a", courier1],
            ["o", order1]
        ])
    );

    const executesOrder1 = executesOrder.instantiate(
        "REL_004",
        "Выполнение заказа 2",
        new Map([
            ["a", cook1],
            ["o", order2]
        ])
    );

    return {
        concepts: [person, user, employee, cook, courier, order],
        frames: [executesOrder, cooksOrder, deliversOrder, placesOrder],
        instances: [user1, cook1, courier1, order1, order2],
        relations: [placesOrder1, cooksOrder1, deliversOrder1, executesOrder1]
    };
}

/**
 * Создать пример предметной области "Университет"
 */
export function createUniversityExample() {
    const person = new Concept("CONCEPT_PERSON", "Person");
    const student = new Concept("CONCEPT_STUDENT", "Student", person);
    const teacher = new Concept("CONCEPT_TEACHER", "Teacher", person);
    const course = new Concept("CONCEPT_COURSE", "Course");
    const grade = new Concept("CONCEPT_GRADE", "Grade");

    const teaches = new Frame("FRAME_TEACHES", "Преподает");
    teaches.addRole("teacher", teacher);
    teaches.addRole("course", course);

    const studies = new Frame("FRAME_STUDIES", "Изучает");
    studies.addRole("student", student);
    studies.addRole("course", course);

    const receivesGrade = new Frame("FRAME_RECEIVES_GRADE", "ПолучаетОценку");
    receivesGrade.addRole("student", student);
    receivesGrade.addRole("course", course);
    receivesGrade.addRole("grade", grade);

    const student1 = new Instance("STUDENT_001", "Иван Петров", student);
    const teacher1 = new Instance("TEACHER_001", "Дмитрий Сидоров", teacher);
    const course1 = new Instance("COURSE_001", "Математика", course);
    const grade1 = new Instance("GRADE_001", "A", grade);

    const teaches1 = teaches.instantiate(
        "REL_TEACHES_001",
        "Преподавание математики",
        new Map([
            ["teacher", teacher1],
            ["course", course1]
        ])
    );

    const studies1 = studies.instantiate(
        "REL_STUDIES_001",
        "Изучение математики",
        new Map([
            ["student", student1],
            ["course", course1]
        ])
    );

    return {
        concepts: [person, student, teacher, course, grade],
        frames: [teaches, studies, receivesGrade],
        instances: [student1, teacher1, course1, grade1],
        relations: [teaches1, studies1]
    };
}

// ===== ТИПЫ ДЛЯ ФОРМ =====
export interface ConceptFormData {
    id: string;
    name: string;
    parentId: string;
}

export interface InstanceFormData {
    id: string;
    name: string;
    typeId: string;
}

export interface FrameFormData {
    id: string;
    name: string;
    parentId: string;
    roles: Array<{ name: string; conceptId: string }>;
}

export interface RelationFormData {
    id: string;
    name: string;
    frameId: string;
    roleValues: Array<{ roleName: string; instanceId: string }>;
}