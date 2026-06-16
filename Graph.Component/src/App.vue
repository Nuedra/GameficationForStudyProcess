<template>
    <div id="app">
        <!-- Header / Navigation -->
        <header class="site-header">
            <div class="container">
                <h1>Graph Viewer</h1>
                <nav>
                    <router-link to="/">Главная</router-link> |
                    <router-link to="/api-test">API тестирование</router-link> |
                    <router-link to="/tests">Тесты</router-link> |
                    <router-link to="/generator">Генератор</router-link>
                    <router-link to="/frame-generator">Генератор Фреймов</router-link>
                </nav>
            </div>
        </header>

        <!-- Main Content -->
        <main class="main-content">
            <!-- Сюда будут подставляться страницы -->
            <router-view />
        </main>

        <!-- Footer -->
        <footer class="site-footer">
            <div class="container">
                <p>&copy; 2025 Vue Graph Viewer. Все права защищены.</p>
            </div>
        </footer>
    </div>
</template>

<script lang="ts">
    import { defineComponent } from 'vue'

    export default defineComponent({
        name: 'App'
    })
</script>

<style>
    /* Ваши стили из оригинального App.vue */
    * {
        margin: 0;
        padding: 0;
        box-sizing: border-box;
    }

    body {
        font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
        line-height: 1.6;
        background-color: #f4f4f4;
        color: #333;
    }

    .container {
        width: 90%;
        max-width: 1000px;
        margin: auto;
    }

    /* Header */
    .site-header {
        background-color: #1976d2;
        color: white;
        padding: 5px 0;
    }

        .site-header h1 {
            text-align: center;
            margin-bottom: 5px;
        }

        .site-header nav {
            text-align: center;
            font-size: 14px;
        }

        .site-header a {
            color: white;
            text-decoration: none;
            margin: 0 5px;
        }

            .site-header a:hover {
                text-decoration: underline;
            }

            .site-header a.router-link-active {
                font-weight: bold;
                text-decoration: underline;
            }

    /* Main Content */
    .main-content {
        min-height: calc(100vh - 120px); /* Учитываем высоту header и footer */
        padding: 20px 0;
    }

    /* Footer */
    .site-footer {
        background-color: #1976d2;
        color: white;
        padding: 10px 0;
        text-align: center;
        margin-top: auto;
    }

    /* Responsive */
    @media (max-width: 768px) {
        .site-header nav {
            font-size: 12px;
        }

        .site-header a {
            margin: 0 3px;
        }
    }
</style>