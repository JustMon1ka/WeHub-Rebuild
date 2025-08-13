import { createRouter, createWebHistory } from 'vue-router'

export const authRouter = createRouter({
    history: createWebHistory(),
    routes: [
      {
        path: '/register',
        name: 'register',
        // lazy load the component
        component: import('@/modules/auth/views/RegisterView.vue'),
        meta: { title: 'Register', navi: false, recommend: false}
      },
      {
        path: '/login',
        name: 'login',
        component: import('@/modules/auth/views/LoginView.vue'),
        meta: { title: 'Login', navi: false, recommend: false }
      },
      {
        path: '/passwordReset',
        name: 'passwordReset',
        component: import ('@/modules/auth/views/PasswordResetView.vue'),
        meta: { title: 'Login', navi: false, recommend: false }
      }
    ]
})

export default authRouter;
