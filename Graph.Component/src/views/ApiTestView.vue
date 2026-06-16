<template>
    <div class="api-test-view">
        <div class="container">
            <section class="hero">
                <h2>Тестирование API</h2>
                <p>Проверка API endpoints и интеграций</p>
            </section>

            <section class="api-section">
                <h3>Компонент тестирования API</h3>
                <p>Здесь вы можете тестировать различные API endpoints:</p>

                <!-- Компонент ApiTest -->
                <ApiTest />
            </section>
        </div>
    </div>
</template>

<script lang="ts">
    import { defineComponent } from 'vue'
    import ApiTest from '@/components/ApiTest.vue'

    export default defineComponent({
        name: 'ApiTestView',
        components: {
            ApiTest
        }
    })
</script>

<style scoped>
    .api-test-view {
        padding: 20px 0;
    }

    .hero {
        background-color: #e8f5e9;
        padding: 20px;
        text-align: center;
        border-radius: 8px;
        margin-bottom: 30px;
    }

        .hero h2 {
            color: #2e7d32;
            margin-bottom: 10px;
        }

        .hero p {
            color: #555;
            font-size: 16px;
        }

    .api-section {
        background-color: white;
        padding: 30px;
        border-radius: 8px;
        box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
    }

        .api-section h3 {
            margin-bottom: 15px;
            color: #333;
        }

        .api-section p {
            margin-bottom: 20px;
            color: #666;
        }
</style>