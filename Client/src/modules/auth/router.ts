import { createRouter, createWebHistory } from 'vue-router'
import RegisterView from './views/RegisterView.vue'
import LoginView from './views/LoginView.vue'

export const authRouter = createRouter({
    history: createWebHistory(),
    routes: [
        {
          path: '/register',
          name: 'register',
          component: RegisterView,
          meta: { title: 'Register', navi: false, recommend: false}
        },
        {
          path: '/login',
          name: 'login',
          component: LoginView,
          meta: { title: 'Login', navi: false, recommend: false }
        }
    ]
})

export default authRouter;
