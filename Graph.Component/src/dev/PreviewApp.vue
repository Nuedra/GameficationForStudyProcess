<template>
    <main class="preview-page">
        <header class="preview-toolbar">
            <div>
                <h1>Achievement Graph Preview</h1>
                <p>{{ statusText }}</p>
            </div>
            <div class="toolbar-actions">
                <button
                    type="button"
                    :class="{ active: mode === 'initial' }"
                    @click="showInitial">
                    До обновления
                </button>
                <button
                    type="button"
                    :class="{ active: mode === 'refreshed' }"
                    @click="showRefreshed">
                    После обновления
                </button>
                <button type="button" @click="resetViewport">
                    Сбросить вид
                </button>
            </div>
        </header>

        <section class="graph-stage">
            <GraphComponent
                ref="graphComponent"
                :xml-data="graphXml"
                :width="920"
                :height="520" />
        </section>
    </main>
</template>

<script lang="ts">
    import { defineComponent } from 'vue';
    import GraphComponent from '../components/GraphComponent.vue';
    import {
        initialPreviewGraphXml,
        refreshedPreviewGraphXml
    } from './previewGraphXml';

    type PreviewMode = 'initial' | 'refreshed';

    export default defineComponent({
        name: 'PreviewApp',

        components: {
            GraphComponent
        },

        data() {
            return {
                mode: 'initial' as PreviewMode,
                graphXml: initialPreviewGraphXml
            };
        },

        computed: {
            statusText(): string {
                return this.mode === 'initial'
                    ? 'Старт получен, практика доступна, экзамен закрыт.'
                    : 'Практика получена, экзамен стал доступен.';
            }
        },

        methods: {
            showInitial(): void {
                this.mode = 'initial';
                this.graphXml = initialPreviewGraphXml;
            },

            showRefreshed(): void {
                this.mode = 'refreshed';
                this.graphXml = refreshedPreviewGraphXml;
            },

            resetViewport(): void {
                (this.$refs.graphComponent as any)?.resetViewport?.();
            }
        }
    });
</script>

<style>
    * {
        box-sizing: border-box;
    }

    body {
        margin: 0;
        color: #18202f;
        background: #eef2f7;
        font-family: Arial, Helvetica, sans-serif;
    }

    button {
        border: 1px solid #aeb8c8;
        border-radius: 6px;
        background: white;
        color: #1f2937;
        cursor: pointer;
        font: inherit;
        min-height: 38px;
        padding: 0 14px;
    }

    button:hover {
        border-color: #4f6f9f;
    }

    button.active {
        background: #1f6feb;
        border-color: #1f6feb;
        color: white;
    }

    .preview-page {
        min-height: 100vh;
        display: grid;
        grid-template-rows: auto 1fr;
    }

    .preview-toolbar {
        align-items: center;
        background: white;
        border-bottom: 1px solid #d9e0eb;
        display: flex;
        gap: 20px;
        justify-content: space-between;
        padding: 18px 24px;
    }

    .preview-toolbar h1 {
        font-size: 20px;
        line-height: 1.2;
        margin: 0;
    }

    .preview-toolbar p {
        color: #526173;
        font-size: 14px;
        line-height: 1.4;
        margin: 6px 0 0;
    }

    .toolbar-actions {
        display: flex;
        flex-wrap: wrap;
        gap: 8px;
        justify-content: flex-end;
    }

    .graph-stage {
        align-items: center;
        display: grid;
        justify-items: center;
        min-width: 0;
        overflow: auto;
        padding: 24px;
    }

    .graph-stage .graph-container {
        padding: 0;
    }

    .graph-stage canvas {
        border: 1px solid #c7d1df;
        box-shadow: 0 12px 32px rgba(42, 55, 78, 0.16);
        display: block;
    }

    @media (max-width: 720px) {
        .preview-toolbar {
            align-items: flex-start;
            flex-direction: column;
            padding: 14px;
        }

        .toolbar-actions {
            justify-content: flex-start;
        }

        .graph-stage {
            justify-items: start;
            padding: 14px;
        }
    }
</style>
