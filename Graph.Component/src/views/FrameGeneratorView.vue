<template>
    <div class="frame-generator-view">
        <div class="container">
            <section class="hero">
                <h2>Конструктор предметной области</h2>
                <p>Создавайте концепты, фреймы и их экземпляры, генерируйте диаграммы</p>
            </section>

            <div class="ontology-builder">
                <!-- Левая панель: Управление -->
                <div class="control-panel">
                    <div class="panel-header">
                        <h3>Управление онтологией</h3>
                        <div class="panel-actions">
                            <button @click="loadExample('delivery')" class="btn-example">
                                <i class="fas fa-utensils"></i> Пример доставки
                            </button>
                            <button @click="clearAll" class="btn-clear">
                                <i class="fas fa-trash"></i> Очистить все
                            </button>
                        </div>
                    </div>

                    <div class="tabs">
                        <button v-for="tab in tabs"
                                :key="tab.id"
                                :class="{ active: activeTab === tab.id }"
                                @click="activeTab = tab.id">
                            <i :class="tab.icon"></i> {{ tab.label }}
                        </button>
                    </div>

                    <!-- Содержимое вкладок -->
                    <div class="tab-content">
                        <!-- Концепты -->
                        <div v-if="activeTab === 'concepts'" class="tab-pane">
                            <div class="form-section">
                                <h4>Создать новый концепт</h4>
                                <form @submit.prevent="createConcept" class="create-form">
                                    <div class="form-group">
                                        <label>ID концепта:</label>
                                        <input v-model="newConcept.id"
                                               placeholder="CONCEPT_PERSON"
                                               required>
                                    </div>
                                    <div class="form-group">
                                        <label>Название:</label>
                                        <input v-model="newConcept.name"
                                               placeholder="Person"
                                               required>
                                    </div>
                                    <div class="form-group">
                                        <label>Родительский концепт:</label>
                                        <select v-model="newConcept.parentId">
                                            <option value="">Без родителя</option>
                                            <option v-for="concept in concepts"
                                                    :key="concept.id"
                                                    :value="concept.id">
                                                {{ concept.name }}
                                            </option>
                                        </select>
                                    </div>
                                    <button type="submit" class="btn-submit">
                                        <i class="fas fa-plus"></i> Создать концепт
                                    </button>
                                </form>

                                <div class="list-section">
                                    <h4>Существующие концепты ({{ concepts.length }})</h4>
                                    <div v-if="concepts.length === 0" class="empty-list">
                                        <i class="fas fa-shapes"></i>
                                        <p>Концепты еще не созданы</p>
                                    </div>
                                    <div v-else class="concepts-list">
                                        <div v-for="concept in concepts"
                                             :key="concept.id"
                                             class="concept-item">
                                            <div class="concept-info">
                                                <span class="concept-name">{{ concept.name }}</span>
                                                <span class="concept-id">({{ concept.id }})</span>
                                                <span v-if="concept.parent" class="concept-parent">
                                                    ← {{ concept.parent.name }}
                                                </span>
                                            </div>
                                            <button @click="deleteConcept(concept.id)"
                                                    class="btn-delete">
                                                <i class="fas fa-trash"></i>
                                            </button>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>

                        <!-- Фреймы -->
                        <div v-if="activeTab === 'frames'" class="tab-pane">
                            <div class="form-section">
                                <h4>Создать новый фрейм</h4>
                                <form @submit.prevent="createFrame" class="create-form">
                                    <div class="form-group">
                                        <label>ID фрейма:</label>
                                        <input v-model="newFrame.id"
                                               placeholder="FRAME_EXECUTES_ORDER"
                                               required>
                                    </div>
                                    <div class="form-group">
                                        <label>Название:</label>
                                        <input v-model="newFrame.name"
                                               placeholder="ВыполняетЗаказ"
                                               required>
                                    </div>
                                    <div class="form-group">
                                        <label>Родительский фрейм:</label>
                                        <select v-model="newFrame.parentId">
                                            <option value="">Без родителя</option>
                                            <option v-for="frame in frames"
                                                    :key="frame.id"
                                                    :value="frame.id">
                                                {{ frame.name }}
                                            </option>
                                        </select>
                                    </div>

                                    <div class="roles-section">
                                        <h5>Роли фрейма:</h5>
                                        <div v-for="(role, index) in newFrame.roles"
                                             :key="index"
                                             class="role-item">
                                            <input v-model="role.name"
                                                   placeholder="Имя роли (например: a)"
                                                   class="role-name">
                                            <select v-model="role.conceptId"
                                                    class="role-concept">
                                                <option value="">Выберите концепт</option>
                                                <option v-for="concept in concepts"
                                                        :key="concept.id"
                                                        :value="concept.id">
                                                    {{ concept.name }}
                                                </option>
                                            </select>
                                            <button @click="removeRole(index)"
                                                    class="btn-remove-role">
                                                <i class="fas fa-times"></i>
                                            </button>
                                        </div>
                                        <button type="button"
                                                @click="addRole"
                                                class="btn-add-role">
                                            <i class="fas fa-plus"></i> Добавить роль
                                        </button>
                                    </div>

                                    <button type="submit" class="btn-submit">
                                        <i class="fas fa-plus"></i> Создать фрейм
                                    </button>
                                </form>

                                <div class="list-section">
                                    <h4>Существующие фреймы ({{ frames.length }})</h4>
                                    <div v-if="frames.length === 0" class="empty-list">
                                        <i class="fas fa-square"></i>
                                        <p>Фреймы еще не созданы</p>
                                    </div>
                                    <div v-else class="frames-list">
                                        <div v-for="frame in frames"
                                             :key="frame.id"
                                             class="frame-item">
                                            <div class="frame-info">
                                                <span class="frame-name">{{ frame.name }}</span>
                                                <span class="frame-id">({{ frame.id }})</span>
                                                <span v-if="frame.parent" class="frame-parent">
                                                    ← {{ frame.parent.name }}
                                                </span>
                                            </div>
                                            <div class="frame-roles">
                                                <span v-for="[roleName, roleConcept] in frame.getAllRoles()"
                                                      :key="roleName"
                                                      class="role-tag">
                                                    {{ roleName }}: {{ roleConcept.name }}
                                                </span>
                                            </div>
                                            <button @click="deleteFrame(frame.id)"
                                                    class="btn-delete">
                                                <i class="fas fa-trash"></i>
                                            </button>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>

                        <!-- Экземпляры -->
                        <div v-if="activeTab === 'instances'" class="tab-pane">
                            <div class="form-section">
                                <h4>Создать новый экземпляр</h4>
                                <form @submit.prevent="createInstance" class="create-form">
                                    <div class="form-group">
                                        <label>ID экземпляра:</label>
                                        <input v-model="newInstance.id"
                                               placeholder="USER_001"
                                               required>
                                    </div>
                                    <div class="form-group">
                                        <label>Название:</label>
                                        <input v-model="newInstance.name"
                                               placeholder="Анна Карпова"
                                               required>
                                    </div>
                                    <div class="form-group">
                                        <label>Тип (концепт):</label>
                                        <select v-model="newInstance.typeId" required>
                                            <option value="">Выберите тип</option>
                                            <option v-for="concept in concepts"
                                                    :key="concept.id"
                                                    :value="concept.id">
                                                {{ concept.name }}
                                            </option>
                                        </select>
                                    </div>
                                    <button type="submit" class="btn-submit">
                                        <i class="fas fa-plus"></i> Создать экземпляр
                                    </button>
                                </form>

                                <div class="list-section">
                                    <h4>Существующие экземпляры ({{ instances.length }})</h4>
                                    <div v-if="instances.length === 0" class="empty-list">
                                        <i class="fas fa-circle"></i>
                                        <p>Экземпляры еще не созданы</p>
                                    </div>
                                    <div v-else class="instances-list">
                                        <div v-for="instance in instances"
                                             :key="instance.id"
                                             class="instance-item">
                                            <div class="instance-info">
                                                <span class="instance-name">{{ instance.name }}</span>
                                                <span class="instance-id">({{ instance.id }})</span>
                                                <span class="instance-type">
                                                    тип: {{ instance.type.name }}
                                                </span>
                                            </div>
                                            <button @click="deleteInstance(instance.id)"
                                                    class="btn-delete">
                                                <i class="fas fa-trash"></i>
                                            </button>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>

                        <!-- Отношения -->
                        <div v-if="activeTab === 'relations'" class="tab-pane">
                            <div class="form-section">
                                <h4>Создать новое отношение</h4>
                                <form @submit.prevent="createRelation" class="create-form">
                                    <div class="form-group">
                                        <label>ID отношения:</label>
                                        <input v-model="newRelation.id"
                                               placeholder="REL_001"
                                               required>
                                    </div>
                                    <div class="form-group">
                                        <label>Название:</label>
                                        <input v-model="newRelation.name"
                                               placeholder="Размещение заказа"
                                               required>
                                    </div>
                                    <div class="form-group">
                                        <label>Фрейм:</label>
                                        <select v-model="newRelation.frameId" required>
                                            <option value="">Выберите фрейм</option>
                                            <option v-for="frame in frames"
                                                    :key="frame.id"
                                                    :value="frame.id">
                                                {{ frame.name }}
                                            </option>
                                        </select>
                                    </div>

                                    <div class="roles-section">
                                        <h5>Заполнение ролей:</h5>
                                        <div v-for="(role, index) in newRelation.roleValues"
                                             :key="index"
                                             class="role-filler-item">
                                            <label class="role-label">
                                                {{ getRoleName(role.roleName) || 'Выберите роль' }}:
                                            </label>
                                            <select v-model="role.roleName"
                                                    @change="updateAvailableInstances()"
                                                    class="role-select">
                                                <option value="">Выберите роль</option>
                                                <option v-for="roleName in availableRoles"
                                                        :key="roleName"
                                                        :value="roleName">
                                                    {{ roleName }}
                                                </option>
                                            </select>
                                            <select v-model="role.instanceId"
                                                    class="instance-select"
                                                    :disabled="!role.roleName">
                                                <option value="">Выберите экземпляр</option>
                                                <option v-for="instance in getInstancesForRole(role.roleName)"
                                                        :key="instance.id"
                                                        :value="instance.id">
                                                    {{ instance.name }} ({{ instance.type.name }})
                                                </option>
                                            </select>
                                            <button @click="removeRoleFiller(index)"
                                                    class="btn-remove-role">
                                                <i class="fas fa-times"></i>
                                            </button>
                                        </div>
                                        <button type="button"
                                                @click="addRoleFiller"
                                                class="btn-add-role"
                                                :disabled="!newRelation.frameId">
                                            <i class="fas fa-plus"></i> Добавить роль
                                        </button>
                                    </div>

                                    <div v-if="validationErrors.length > 0" class="validation-errors">
                                        <p v-for="error in validationErrors" :key="error" class="error">
                                            <i class="fas fa-exclamation-circle"></i> {{ error }}
                                        </p>
                                    </div>

                                    <button type="submit" class="btn-submit">
                                        <i class="fas fa-plus"></i> Создать отношение
                                    </button>
                                </form>

                                <div class="list-section">
                                    <h4>Существующие отношения ({{ relations.length }})</h4>
                                    <div v-if="relations.length === 0" class="empty-list">
                                        <i class="fas fa-link"></i>
                                        <p>Отношения еще не созданы</p>
                                    </div>
                                    <div v-else class="relations-list">
                                        <div v-for="relation in relations"
                                             :key="relation.id"
                                             class="relation-item">
                                            <div class="relation-info">
                                                <span class="relation-name">{{ relation.name }}</span>
                                                <span class="relation-id">({{ relation.id }})</span>
                                                <span class="relation-frame">
                                                    фрейм: {{ relation.frameType.name }}
                                                </span>
                                            </div>
                                            <div class="relation-roles">
                                                <span v-for="[roleName, instance] in relation.roleValues"
                                                      :key="roleName"
                                                      class="role-value-tag">
                                                    {{ roleName }}: {{ instance.name }}
                                                </span>
                                            </div>
                                            <button @click="deleteRelation(relation.id)"
                                                    class="btn-delete">
                                                <i class="fas fa-trash"></i>
                                            </button>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>

                <!-- Правая панель: Диаграмма -->
                <div class="diagram-panel">
                    <div class="panel-header">
                        <h3>Диаграмма предметной области</h3>
                        <div class="panel-actions">
                            <button @click="generateDiagram" class="btn-generate" :disabled="!canGenerate">
                                <i class="fas fa-project-diagram"></i> Сгенерировать
                            </button>
                            <button @click="exportXml" class="btn-export" :disabled="!generatedXml">
                                <i class="fas fa-download"></i> Экспорт XML
                            </button>
                            <button @click="saveProject" class="btn-save">
                                <i class="fas fa-save"></i> Сохранить
                            </button>
                        </div>
                    </div>

                    <div class="diagram-container">
                        <!-- Ваш компонент для отображения графа -->
                        <GraphComponent v-if="generatedXml && showDiagram"
                                        ref="graphComponent"
                                        :xml-data="generatedXml"
                                        @node-clicked="handleNodeClick" />

                        <div v-else class="diagram-placeholder">
                            <i class="fas fa-project-diagram"></i>
                            <p v-if="!canGenerate">
                                Создайте хотя бы один концепт и экземпляр для генерации диаграммы
                            </p>
                            <p v-else>
                                Нажмите "Сгенерировать" для создания диаграммы
                            </p>
                        </div>
                    </div>

                    <div class="diagram-stats">
                        <div class="stat-card">
                            <div class="stat-icon concept">
                                <i class="fas fa-shapes"></i>
                            </div>
                            <div class="stat-content">
                                <span class="stat-value">{{ concepts.length }}</span>
                                <span class="stat-label">Концептов</span>
                            </div>
                        </div>
                        <div class="stat-card">
                            <div class="stat-icon frame">
                                <i class="fas fa-square"></i>
                            </div>
                            <div class="stat-content">
                                <span class="stat-value">{{ frames.length }}</span>
                                <span class="stat-label">Фреймов</span>
                            </div>
                        </div>
                        <div class="stat-card">
                            <div class="stat-icon instance">
                                <i class="fas fa-circle"></i>
                            </div>
                            <div class="stat-content">
                                <span class="stat-value">{{ instances.length }}</span>
                                <span class="stat-label">Экземпляров</span>
                            </div>
                        </div>
                        <div class="stat-card">
                            <div class="stat-icon relation">
                                <i class="fas fa-link"></i>
                            </div>
                            <div class="stat-content">
                                <span class="stat-value">{{ relations.length }}</span>
                                <span class="stat-label">Отношений</span>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        </div>
    </div>
</template>

<script lang="ts">
import { defineComponent, ref } from 'vue'
import GraphComponent from '@/components/GraphComponent.vue'
import {
    Concept,
    Instance,
    Frame,
    FrameInstance,
    createDeliveryExample,
    createUniversityExample,
    ConceptFormData,
    InstanceFormData,
    FrameFormData,
    RelationFormData
} from '@/components/FrameLogic/Core'
import { FrameGraphXmlGenerator } from '@/components/FrameLogic/FrameXmlGenerator'

export default defineComponent({
    name: 'FrameGeneratorView',
    components: {
        GraphComponent
    },
    data() {
        return {
            // Данные онтологии
            concepts: [] as Concept[],
            frames: [] as Frame[],
            instances: [] as Instance[],
            relations: [] as FrameInstance[],

            // Активная вкладка
            activeTab: 'concepts',
            tabs: [
                { id: 'concepts', label: 'Концепты', icon: 'fas fa-shapes' },
                { id: 'frames', label: 'Фреймы', icon: 'fas fa-square' },
                { id: 'instances', label: 'Экземпляры', icon: 'fas fa-circle' },
                { id: 'relations', label: 'Отношения', icon: 'fas fa-link' }
            ],

            // Формы для создания
            newConcept: {
                id: '',
                name: '',
                parentId: ''
            } as ConceptFormData,

            newFrame: {
                id: '',
                name: '',
                parentId: '',
                roles: [] as Array<{ name: string; conceptId: string }>
            } as FrameFormData,

            newInstance: {
                id: '',
                name: '',
                typeId: ''
            } as InstanceFormData,

            newRelation: {
                id: '',
                name: '',
                frameId: '',
                roleValues: [] as Array<{ roleName: string; instanceId: string }>
            } as RelationFormData,

            // Доступные роли для текущего фрейма
            availableRoles: [] as string[],

            // Ошибки валидации
            validationErrors: [] as string[],

            // Сгенерированный XML
            generatedXml: '',
            showDiagram: false,
            //graphComponent: ref<any>(null),
            graphComponent: ref<any>(null),

            // Генератор XML
            xmlGenerator: new FrameGraphXmlGenerator()
        }
    },
    computed: {
        canGenerate(): boolean {
            return this.concepts.length > 0 && this.instances.length > 0
        },

        // Вспомогательные методы для поиска
        getConceptById() {
            return (id: string) => this.concepts.find(c => c.id === id)
        },

        getFrameById() {
            return (id: string) => this.frames.find(f => f.id === id)
        },

        getInstanceById() {
            return (id: string) => this.instances.find(i => i.id === id)
        }
    },
    methods: {
        // === Методы для концептов ===
        createConcept() {
            try {
                const parent = this.newConcept.parentId
                    ? this.getConceptById(this.newConcept.parentId)
                    : undefined

                const concept = new Concept(
                    this.newConcept.id,
                    this.newConcept.name,
                    parent
                )

                this.concepts.push(concept)
                this.resetConceptForm()
                this.showSuccess('Концепт успешно создан!')
            } catch (error) {
                this.showError('Ошибка при создании концепта')
            }
        },

        deleteConcept(id: string) {
            // Проверяем, используется ли концепт
            const isUsedInInstances = this.instances.some(i => i.type.id === id)
            const isUsedInFrames = this.frames.some(f =>
                Array.from(f.getAllRoles().values()).some(c => c.id === id)
            )

            if (isUsedInInstances || isUsedInFrames) {
                this.showError('Концепт используется и не может быть удален')
                return
            }

            this.concepts = this.concepts.filter(c => c.id !== id)
            this.showSuccess('Концепт удален')
        },

        resetConceptForm() {
            this.newConcept = {
                id: '',
                name: '',
                parentId: ''
            }
        },

        // === Методы для фреймов ===
        createFrame() {
            try {
                const parent = this.newFrame.parentId
                    ? this.getFrameById(this.newFrame.parentId)
                    : undefined

                const frame = new Frame(
                    this.newFrame.id,
                    this.newFrame.name,
                    parent
                )

                // Добавляем роли
                this.newFrame.roles.forEach(role => {
                    const concept = this.getConceptById(role.conceptId)
                    if (concept) {
                        frame.addRole(role.name, concept)
                    }
                })

                this.frames.push(frame)
                this.resetFrameForm()
                this.showSuccess('Фрейм успешно создан!')
            } catch (error) {
                this.showError('Ошибка при создании фрейма')
            }
        },

        deleteFrame(id: string) {
            // Проверяем, используется ли фрейм
            const isUsedInRelations = this.relations.some(r => r.frameType.id === id)

            if (isUsedInRelations) {
                this.showError('Фрейм используется в отношениях и не может быть удален')
                return
            }

            this.frames = this.frames.filter(f => f.id !== id)
            this.showSuccess('Фрейм удален')
        },

        addRole() {
            this.newFrame.roles.push({ name: '', conceptId: '' })
        },

        removeRole(index: number) {
            this.newFrame.roles.splice(index, 1)
        },

        resetFrameForm() {
            this.newFrame = {
                id: '',
                name: '',
                parentId: '',
                roles: []
            }
        },

        // === Методы для экземпляров ===
        createInstance() {
            try {
                const type = this.getConceptById(this.newInstance.typeId)
                if (!type) {
                    this.showError('Выберите тип концепта')
                    return
                }

                const instance = new Instance(
                    this.newInstance.id,
                    this.newInstance.name,
                    type
                )

                this.instances.push(instance)
                this.resetInstanceForm()
                this.showSuccess('Экземпляр успешно создан!')
            } catch (error) {
                this.showError('Ошибка при создании экземпляра')
            }
        },

        deleteInstance(id: string) {
            // Проверяем, используется ли экземпляр
            const isUsedInRelations = this.relations.some(r =>
                Array.from(r.roleValues.values()).some(i => i.id === id)
            )

            if (isUsedInRelations) {
                this.showError('Экземпляр используется в отношениях и не может быть удален')
                return
            }

            this.instances = this.instances.filter(i => i.id !== id)
            this.showSuccess('Экземпляр удален')
        },

        resetInstanceForm() {
            this.newInstance = {
                id: '',
                name: '',
                typeId: ''
            }
        },

        // === Методы для отношений ===
        createRelation() {
            try {
                const frame = this.getFrameById(this.newRelation.frameId)
                if (!frame) {
                    this.showError('Выберите фрейм')
                    return
                }

                // Собираем Map с заполнителями ролей
                const roleFillers = new Map<string, Instance>()
                this.newRelation.roleValues.forEach(role => {
                    const instance = this.getInstanceById(role.instanceId)
                    if (instance && role.roleName) {
                        roleFillers.set(role.roleName, instance)
                    }
                })

                // Проверяем возможность создания
                const validation = frame.canInstantiate(roleFillers)
                if (!validation.valid) {
                    this.validationErrors = validation.errors
                    this.showError('Ошибки в заполнении ролей')
                    return
                }

                // Создаем отношение
                const relation = frame.instantiate(
                    this.newRelation.id,
                    this.newRelation.name,
                    roleFillers
                )

                this.relations.push(relation)
                this.resetRelationForm()
                this.validationErrors = []
                this.showSuccess('Отношение успешно создано!')
            } catch (error: any) {
                this.showError(`Ошибка при создании отношения: ${error.message}`)
            }
        },

        deleteRelation(id: string) {
            this.relations = this.relations.filter(r => r.id !== id)
            this.showSuccess('Отношение удалено')
        },

        addRoleFiller() {
            this.newRelation.roleValues.push({ roleName: '', instanceId: '' })
            this.updateAvailableRoles()
        },

        removeRoleFiller(index: number) {
            this.newRelation.roleValues.splice(index, 1)
            this.updateAvailableRoles()
        },

        updateAvailableRoles() {
            if (!this.newRelation.frameId) {
                this.availableRoles = []
                return
            }

            const frame = this.getFrameById(this.newRelation.frameId)
            if (frame) {
                // Получаем все роли фрейма
                const allRoles = Array.from(frame.getAllRoles().keys())
                // Исключаем уже выбранные роли
                const usedRoles = new Set(this.newRelation.roleValues.map(r => r.roleName))
                this.availableRoles = allRoles.filter(role => !usedRoles.has(role))
            }
        },

        getInstancesForRole(roleName: string): Instance[] {
            if (!roleName || !this.newRelation.frameId) return []

            const frame = this.getFrameById(this.newRelation.frameId)
            if (!frame) return []

            const roleConcept = frame.getRoleType(roleName)
            if (!roleConcept) return []

            // Возвращаем экземпляры, которые являются экземплярами этого концепта
            return this.instances.filter(instance =>
                instance.isInstanceOf(roleConcept)
            )
        },

        getRoleName(roleName: string): string {
            const frame = this.getFrameById(this.newRelation.frameId)
            if (!frame || !roleName) return ''

            const roleConcept = frame.getRoleType(roleName)
            return roleConcept ? `${roleName} (${roleConcept.name})` : roleName
        },

        resetRelationForm() {
            this.newRelation = {
                id: '',
                name: '',
                frameId: '',
                roleValues: []
            }
            this.availableRoles = []
        },

        // === Генерация диаграммы ===
        generateDiagram() {
            try {
                const newXml = this.xmlGenerator.generateDiagramXml(
                    this.concepts,
                    this.instances,
                    this.relations
                )

                // Если граф уже отображается, обновляем его данные
                if (this.showDiagram && this.graphComponent?.value.updateGraphDataXML) {
                    // Метод updateGraphDataXML существует в GraphComponent
                    this.$nextTick(() => {
                        this.graphComponent.value.updateGraphDataXML(newXml);
                    });
                    this.generatedXml = newXml;
                } else {
                    // Первый раз показываем граф
                    this.generatedXml = newXml;
                    this.showDiagram = true;
                }

                //this.showDiagram = true

                //if (this.graphComponent && this.graphComponent.updateGraphDataXML) {
                //    this.graphComponent.updateGraphDataXML(this.generatedXml);
                //}

                this.showSuccess('Диаграмма успешно сгенерирована!')
            } catch (error: any) {
                this.showError(`Ошибка генерации диаграммы: ${error.message}`)
            }
        },

        exportXml() {
            if (!this.generatedXml) {
                this.showError('Сначала сгенерируйте диаграмму')
                return
            }

            const blob = new Blob([this.generatedXml], { type: 'application/xml' })
            const url = URL.createObjectURL(blob)
            const a = document.createElement('a')
            a.href = url
            a.download = `ontology-diagram-${new Date().toISOString().slice(0, 10)}.xml`
            document.body.appendChild(a)
            a.click()
            document.body.removeChild(a)
            URL.revokeObjectURL(url)

            this.showSuccess('XML успешно экспортирован')
        },

        saveProject() {
            const project = {
                concepts: this.concepts.map(c => ({
                    id: c.id,
                    name: c.name,
                    parentId: c.parent?.id
                })),
                frames: this.frames.map(f => ({
                    id: f.id,
                    name: f.name,
                    parentId: f.parent?.id,
                    roles: Array.from(f.getAllRoles().entries()).map(([name, concept]) => ({
                        name,
                        conceptId: concept.id
                    }))
                })),
                instances: this.instances.map(i => ({
                    id: i.id,
                    name: i.name,
                    typeId: i.type.id
                })),
                relations: this.relations.map(r => ({
                    id: r.id,
                    name: r.name,
                    frameId: r.frameType.id,
                    roleValues: Array.from(r.roleValues.entries()).map(([role, instance]) => ({
                        role,
                        instanceId: instance.id
                    }))
                })),
                timestamp: new Date().toISOString()
            }

            const blob = new Blob([JSON.stringify(project, null, 2)], {
                type: 'application/json'
            })
            const url = URL.createObjectURL(blob)
            const a = document.createElement('a')
            a.href = url
            a.download = `ontology-project-${new Date().toISOString().slice(0, 10)}.json`
            document.body.appendChild(a)
            a.click()
            document.body.removeChild(a)
            URL.revokeObjectURL(url)

            this.showSuccess('Проект успешно сохранен')
        },

        loadExample(type: 'delivery' | 'university') {
            const example = type === 'delivery'
                ? createDeliveryExample()
                : createUniversityExample()

            this.concepts = example.concepts
            this.frames = example.frames
            this.instances = example.instances
            this.relations = example.relations

            this.showSuccess(`Пример "${type === 'delivery' ? 'Доставка еды' : 'Университет'}" загружен`)
        },

        clearAll() {
            if (confirm('Вы уверены, что хотите очистить все данные?')) {
                this.concepts = []
                this.frames = []
                this.instances = []
                this.relations = []
                this.generatedXml = ''
                this.showDiagram = false
                this.resetAllForms()
                this.showSuccess('Все данные очищены')
            }
        },

        resetAllForms() {
            this.resetConceptForm()
            this.resetFrameForm()
            this.resetInstanceForm()
            this.resetRelationForm()
        },

        handleNodeClick(nodeId: string) {
            console.log('Клик по узлу:', nodeId)
            // Здесь можно добавить логику выделения элементов
        },

        // === Вспомогательные методы ===
        showSuccess(message: string) {
            alert(`✅ ${message}`)
        },

        showError(message: string) {
            alert(`❌ ${message}`)
        }
    },
    watch: {
        'newRelation.frameId': {
            handler() {
                this.updateAvailableRoles()
                this.newRelation.roleValues = []
            },
            immediate: false
        }
    }
})
</script>

<style scoped>
    .frame-generator-view {
        padding: 20px 0;
        background-color: #f5f5f5;
        min-height: 100vh;
    }

    .container {
        max-width: 1600px;
        margin: 0 auto;
        padding: 0 20px;
    }

    .hero {
        background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
        color: white;
        padding: 30px;
        text-align: center;
        border-radius: 12px;
        margin-bottom: 30px;
        box-shadow: 0 4px 15px rgba(0, 0, 0, 0.1);
    }

        .hero h2 {
            font-size: 2.5rem;
            margin-bottom: 10px;
            font-weight: 600;
        }

        .hero p {
            font-size: 1.1rem;
            opacity: 0.9;
        }

    .ontology-builder {
        display: grid;
        grid-template-columns: 1fr 1.2fr;
        gap: 30px;
        margin-top: 20px;
    }

    .control-panel,
    .diagram-panel {
        background: white;
        border-radius: 12px;
        box-shadow: 0 2px 12px rgba(0, 0, 0, 0.08);
        overflow: hidden;
    }

    .panel-header {
        padding: 20px;
        border-bottom: 1px solid #eaeaea;
        display: flex;
        justify-content: space-between;
        align-items: center;
        background: linear-gradient(135deg, #f8f9fa 0%, #e9ecef 100%);
    }

        .panel-header h3 {
            margin: 0;
            color: #333;
            font-size: 1.4rem;
        }

    .panel-actions {
        display: flex;
        gap: 10px;
    }

    .tabs {
        display: flex;
        background: #f8f9fa;
        border-bottom: 1px solid #eaeaea;
    }

        .tabs button {
            flex: 1;
            padding: 15px 20px;
            border: none;
            background: none;
            cursor: pointer;
            font-size: 14px;
            font-weight: 500;
            color: #666;
            display: flex;
            align-items: center;
            justify-content: center;
            gap: 8px;
            transition: all 0.3s ease;
        }

            .tabs button:hover {
                background: #e9ecef;
                color: #333;
            }

            .tabs button.active {
                background: white;
                color: #667eea;
                border-bottom: 3px solid #667eea;
                font-weight: 600;
            }

    .tab-content {
        padding: 20px;
        max-height: 600px;
        overflow-y: auto;
    }

    .form-section {
        margin-bottom: 30px;
    }

        .form-section h4 {
            margin-bottom: 20px;
            color: #333;
            font-size: 1.2rem;
            display: flex;
            align-items: center;
            gap: 10px;
        }

    .create-form {
        background: #f8f9fa;
        padding: 20px;
        border-radius: 8px;
        margin-bottom: 20px;
    }

    .form-group {
        margin-bottom: 15px;
    }

        .form-group label {
            display: block;
            margin-bottom: 5px;
            font-weight: 500;
            color: #555;
        }

        .form-group input,
        .form-group select {
            width: 100%;
            padding: 10px 12px;
            border: 1px solid #ddd;
            border-radius: 6px;
            font-size: 14px;
            transition: border-color 0.3s ease;
        }

            .form-group input:focus,
            .form-group select:focus {
                outline: none;
                border-color: #667eea;
                box-shadow: 0 0 0 3px rgba(102, 126, 234, 0.1);
            }

    .roles-section {
        background: white;
        padding: 15px;
        border-radius: 6px;
        margin: 20px 0;
        border: 1px solid #eaeaea;
    }

        .roles-section h5 {
            margin: 0 0 15px 0;
            color: #555;
        }

    .role-item,
    .role-filler-item {
        display: flex;
        gap: 10px;
        margin-bottom: 10px;
        align-items: center;
    }

    .role-name,
    .role-concept,
    .role-select,
    .instance-select {
        flex: 1;
        padding: 8px 12px;
        border: 1px solid #ddd;
        border-radius: 4px;
        font-size: 14px;
    }

    .role-label {
        min-width: 100px;
        font-weight: 500;
        color: #555;
    }

    .btn-remove-role {
        padding: 8px 12px;
        background: #ff6b6b;
        color: white;
        border: none;
        border-radius: 4px;
        cursor: pointer;
        transition: background 0.3s ease;
    }

        .btn-remove-role:hover {
            background: #ff5252;
        }

    .btn-add-role {
        padding: 10px 20px;
        background: #4CAF50;
        color: white;
        border: none;
        border-radius: 4px;
        cursor: pointer;
        font-size: 14px;
        display: inline-flex;
        align-items: center;
        gap: 8px;
        transition: background 0.3s ease;
    }

        .btn-add-role:hover {
            background: #43a047;
        }

        .btn-add-role:disabled {
            background: #ccc;
            cursor: not-allowed;
        }

    .btn-submit {
        width: 100%;
        padding: 12px;
        background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
        color: white;
        border: none;
        border-radius: 6px;
        font-size: 16px;
        font-weight: 600;
        cursor: pointer;
        display: flex;
        align-items: center;
        justify-content: center;
        gap: 10px;
        transition: transform 0.3s ease;
    }

        .btn-submit:hover {
            transform: translateY(-2px);
        }

    .list-section {
        margin-top: 30px;
    }

        .list-section h4 {
            margin-bottom: 15px;
            color: #333;
            font-size: 1.1rem;
        }

    .empty-list {
        text-align: center;
        padding: 40px 20px;
        color: #888;
    }

        .empty-list i {
            font-size: 48px;
            margin-bottom: 15px;
            opacity: 0.5;
        }

    .concepts-list,
    .frames-list,
    .instances-list,
    .relations-list {
        display: flex;
        flex-direction: column;
        gap: 10px;
    }

    .concept-item,
    .frame-item,
    .instance-item,
    .relation-item {
        background: white;
        padding: 15px;
        border: 1px solid #eaeaea;
        border-radius: 6px;
        display: flex;
        justify-content: space-between;
        align-items: center;
        transition: all 0.3s ease;
    }

        .concept-item:hover,
        .frame-item:hover,
        .instance-item:hover,
        .relation-item:hover {
            border-color: #667eea;
            box-shadow: 0 2px 8px rgba(102, 126, 234, 0.1);
        }

    .concept-info,
    .frame-info,
    .instance-info,
    .relation-info {
        flex: 1;
    }

    .concept-name,
    .frame-name,
    .instance-name,
    .relation-name {
        font-weight: 600;
        color: #333;
        margin-right: 10px;
    }

    .concept-id,
    .frame-id,
    .instance-id,
    .relation-id {
        color: #888;
        font-size: 12px;
        margin-right: 10px;
    }

    .concept-parent,
    .frame-parent,
    .instance-type,
    .relation-frame {
        color: #666;
        font-size: 13px;
        margin-left: 10px;
    }

    .frame-roles,
    .relation-roles {
        display: flex;
        flex-wrap: wrap;
        gap: 5px;
        margin-top: 5px;
    }

    .role-tag,
    .role-value-tag {
        background: #e3f2fd;
        color: #1976d2;
        padding: 2px 8px;
        border-radius: 12px;
        font-size: 12px;
    }

    .btn-delete {
        padding: 6px 12px;
        background: #ff6b6b;
        color: white;
        border: none;
        border-radius: 4px;
        cursor: pointer;
        transition: background 0.3s ease;
    }

        .btn-delete:hover {
            background: #ff5252;
        }

    .validation-errors {
        background: #ffebee;
        border: 1px solid #ffcdd2;
        border-radius: 6px;
        padding: 15px;
        margin: 15px 0;
    }

    .error {
        color: #d32f2f;
        margin: 5px 0;
        display: flex;
        align-items: center;
        gap: 8px;
    }

    .diagram-container {
        height: 800px;
        width: 100%;
/*        height: 100%;*/
        background: #f8f9fa;
        border: 2px dashed #ddd;
        border-radius: 8px;
        display: flex;
        align-items: center;
        justify-content: center;
        margin: 20px;
        overflow: hidden;
    }

    .diagram-placeholder {
        text-align: center;
        color: #888;
    }

        .diagram-placeholder i {
            font-size: 64px;
            margin-bottom: 15px;
            opacity: 0.3;
        }

        .diagram-placeholder p {
            font-size: 16px;
            max-width: 300px;
        }

    .diagram-stats {
        display: grid;
        grid-template-columns: repeat(4, 1fr);
        gap: 15px;
        padding: 20px;
        background: #f8f9fa;
        border-top: 1px solid #eaeaea;
    }

    .stat-card {
        background: white;
        padding: 15px;
        border-radius: 8px;
        display: flex;
        align-items: center;
        gap: 15px;
        box-shadow: 0 2px 4px rgba(0, 0, 0, 0.05);
    }

    .stat-icon {
        width: 40px;
        height: 40px;
        border-radius: 50%;
        display: flex;
        align-items: center;
        justify-content: center;
        font-size: 18px;
    }

        .stat-icon.concept {
            background: #4CAF50;
            color: white;
        }

        .stat-icon.frame {
            background: #FF9800;
            color: white;
        }

        .stat-icon.instance {
            background: #2196F3;
            color: white;
        }

        .stat-icon.relation {
            background: #9C27B0;
            color: white;
        }

    .stat-content {
        flex: 1;
    }

    .stat-value {
        display: block;
        font-size: 24px;
        font-weight: 600;
        color: #333;
    }

    .stat-label {
        display: block;
        font-size: 12px;
        color: #666;
        text-transform: uppercase;
        letter-spacing: 0.5px;
    }

    /* Кнопки действий */
    .btn-example,
    .btn-clear,
    .btn-generate,
    .btn-export,
    .btn-save {
        padding: 8px 16px;
        border: none;
        border-radius: 6px;
        font-size: 14px;
        font-weight: 500;
        cursor: pointer;
        display: inline-flex;
        align-items: center;
        gap: 8px;
        transition: all 0.3s ease;
    }

    .btn-example {
        background: #4CAF50;
        color: white;
    }

        .btn-example:hover {
            background: #43a047;
        }

    .btn-clear {
        background: #f44336;
        color: white;
    }

        .btn-clear:hover {
            background: #e53935;
        }

    .btn-generate {
        background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
        color: white;
    }

        .btn-generate:hover:not(:disabled) {
            transform: translateY(-2px);
            box-shadow: 0 4px 12px rgba(102, 126, 234, 0.3);
        }

        .btn-generate:disabled {
            opacity: 0.5;
            cursor: not-allowed;
        }

    .btn-export {
        background: #2196F3;
        color: white;
    }

        .btn-export:hover:not(:disabled) {
            background: #1976d2;
        }

        .btn-export:disabled {
            opacity: 0.5;
            cursor: not-allowed;
        }

    .btn-save {
        background: #FF9800;
        color: white;
    }

        .btn-save:hover {
            background: #f57c00;
        }

    /* Адаптивность */
    @media (max-width: 1200px) {
        .ontology-builder {
            grid-template-columns: 1fr;
        }

        .diagram-stats {
            grid-template-columns: repeat(2, 1fr);
        }
    }

    @media (max-width: 768px) {
        .panel-header {
            flex-direction: column;
            gap: 15px;
            text-align: center;
        }

        .tabs {
            flex-direction: column;
        }

        .diagram-stats {
            grid-template-columns: 1fr;
        }

        .role-item,
        .role-filler-item {
            flex-direction: column;
            align-items: stretch;
        }

        .role-label {
            margin-bottom: 5px;
        }
    }
</style>