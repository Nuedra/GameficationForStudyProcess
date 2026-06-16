<template>
    <div class="tests-view">
        <div class="container">
            <section class="hero">
                <h2>Тесты</h2>
                <p>Unit и интеграционные тесты системы</p>
            </section>

            <section class="content-section">
                <div class="coming-soon">
                    <div class="icon">🧪</div>
                    <h3>Скоро будет</h3>
                    <p>Раздел "Тесты" находится в разработке</p>
                    <p>Здесь будут отображаться unit и интеграционные тесты вашей системы</p>
                </div>
            </section>
        </div>
    </div>
</template>

<script lang="ts">
import { defineComponent } from 'vue'

export default defineComponent({
  name: 'TestsView'
})
</script>

<style scoped>
    .tests-view {
        padding: 20px 0;
    }

    .hero {
        background-color: #fff3e0;
        padding: 20px;
        text-align: center;
        border-radius: 8px;
        margin-bottom: 30px;
    }

        .hero h2 {
            color: #ef6c00;
            margin-bottom: 10px;
        }

        .hero p {
            color: #555;
            font-size: 16px;
        }

    .content-section {
        background-color: white;
        padding: 40px;
        border-radius: 8px;
        box-shadow: 0 2px 8px rgba(0, 0, 0, 0.1);
        min-height: 300px;
        display: flex;
        align-items: center;
        justify-content: center;
    }

    .coming-soon {
        text-align: center;
        max-width: 400px;
    }

        .coming-soon .icon {
            font-size: 60px;
            margin-bottom: 20px;
        }

        .coming-soon h3 {
            margin-bottom: 15px;
            color: #333;
        }

        .coming-soon p {
            margin-bottom: 10px;
            color: #666;
            line-height: 1.5;
        }
</style>