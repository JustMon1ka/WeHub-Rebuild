import './assets/main.css'

import { createApp } from 'vue'
import App from './App.vue'
import { createPinia } from 'pinia'
import router from './router.ts'

// 如果你需要设置 cookie，可以保留你写的那行
document.cookie = `auth=...`

const app = createApp(App)

// ⚠️ 这里要先创建一个 pinia 实例
const pinia = createPinia()

// ✅ 然后再 use
app.use(pinia)
app.use(router)

app.mount('#app')
