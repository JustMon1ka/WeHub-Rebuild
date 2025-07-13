import { createRouter, createWebHistory } from 'vue-router'
import RegisterView from './views/RegisterView.vue'
import LoginView from './views/LoginView.vue'
import MeView from './views/MeView.vue'

export const router = createRouter({
    history: createWebHistory(),
    routes: [
        { path: '/', redirect: '/login' },
        { path: '/register', component: RegisterView },
        { path: '/login', component: LoginView },
        { path: '/me', component: MeView }
    ]
})
