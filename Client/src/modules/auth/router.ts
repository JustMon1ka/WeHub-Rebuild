import { createRouter, createWebHistory } from 'vue-router'
import RegisterView from './views/RegisterView.vue'
import LoginView from './views/LoginView.vue'
import MeView from './views/MeView.vue'

export const authRouter = createRouter({
    history: createWebHistory(),
    routes: [
        {
          path: '/register',
          name: 'register',
          component: RegisterView
        },
        {
          path: '/login',
          name: 'login',
          component: LoginView
        },
        {
          path: '/me',
          name: 'me',
          component: MeView
        }
    ]
})

export default authRouter;
