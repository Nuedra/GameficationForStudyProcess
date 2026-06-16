<template>
    <section class="achievement-graph-panel">
        <div class="toolbar">
            <button type="button" @click="drawGraph" :disabled="isLoading">
                Отрисовать граф
            </button>
            <button type="button" @click="refreshGraph" :disabled="isLoading || !graphXml">
                Обновить граф
            </button>
            <button type="button" @click="logout" :disabled="isLoading" class="secondary">
                Выйти из аккаунта
            </button>
        </div>

        <div class="graph-window" :class="{ empty: !graphXml }">
            <GraphComponent
                v-if="graphXml"
                ref="graphComponent"
                :xml-data="graphXml"
                :width="width"
                :height="height" />
            <div v-else class="placeholder">
                Граф появится после нажатия кнопки отрисовки.
            </div>
        </div>

        <p v-if="errorMessage" class="error-message">
            {{ errorMessage }}
        </p>
    </section>
</template>

<script lang="ts">
    import { defineComponent, PropType } from 'vue';
    import GraphComponent from './GraphComponent.vue';
    import {
        getAchievementGraphXml,
        logoutStudent,
        refreshAchievementGraphXml
    } from './achievementGraphApi';

    export default defineComponent({
        name: 'AchievementGraphPanel',

        components: {
            GraphComponent
        },

        props: {
            courseId: {
                type: String as PropType<string>,
                required: true
            },
            year: {
                type: Number,
                required: true
            },
            width: {
                type: Number,
                default: 1100
            },
            height: {
                type: Number,
                default: 650
            }
        },

        emits: ['graph-loaded', 'graph-refreshed', 'logout', 'error'],

        data() {
            return {
                graphXml: '',
                isLoading: false,
                errorMessage: ''
            };
        },

        methods: {
            async drawGraph(): Promise<void> {
                await this.loadGraph(() =>
                    getAchievementGraphXml(this.courseId, this.year),
                    'graph-loaded');
            },

            async refreshGraph(): Promise<void> {
                await this.loadGraph(() =>
                    refreshAchievementGraphXml(this.courseId, this.year),
                    'graph-refreshed');
            },

            async logout(): Promise<void> {
                this.isLoading = true;
                this.errorMessage = '';

                try {
                    await logoutStudent();
                    this.graphXml = '';
                    this.$emit('logout');
                } catch (error) {
                    this.handleError(error);
                } finally {
                    this.isLoading = false;
                }
            },

            clearGraph(): void {
                this.graphXml = '';
                this.errorMessage = '';
            },

            async loadGraph(
                loader: () => Promise<string>,
                eventName: 'graph-loaded' | 'graph-refreshed'): Promise<void> {
                this.isLoading = true;
                this.errorMessage = '';

                try {
                    this.graphXml = await loader();
                    this.$emit(eventName, this.graphXml);
                } catch (error) {
                    this.handleError(error);
                } finally {
                    this.isLoading = false;
                }
            },

            handleError(error: unknown): void {
                this.errorMessage = 'Не удалось выполнить действие с графом.';
                this.$emit('error', error);
            }
        },

        expose: [
            'drawGraph',
            'refreshGraph',
            'logout',
            'clearGraph'
        ]
    });
</script>

<style scoped>
    .achievement-graph-panel {
        display: grid;
        gap: 12px;
        width: 100%;
    }

    .toolbar {
        display: flex;
        flex-wrap: wrap;
        gap: 8px;
        align-items: center;
    }

    .toolbar button {
        border: 1px solid #b8c1d1;
        border-radius: 6px;
        background: #1f6feb;
        color: white;
        font: inherit;
        padding: 8px 12px;
        cursor: pointer;
    }

    .toolbar button.secondary {
        background: white;
        color: #1f2937;
    }

    .toolbar button:disabled {
        cursor: default;
        opacity: 0.55;
    }

    .graph-window {
        min-height: 420px;
        overflow: hidden;
        border: 1px solid #d7dde8;
        border-radius: 8px;
        background: #f8fafc;
    }

    .graph-window.empty {
        display: grid;
        place-items: center;
    }

    .placeholder {
        color: #64748b;
        font-size: 15px;
    }

    .error-message {
        color: #b42318;
        margin: 0;
    }
</style>
