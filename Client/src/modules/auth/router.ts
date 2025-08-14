import { createRouter, createWebHistory } from 'vue-router'

export const authRouter = createRouter({
    history: createWebHistory(),
    routes: [
      {
        path: '/register',
        name: 'register',
        // lazy load the component
        component: import('@/modules/auth/views/RegisterView.vue'),
        meta: { title: 'Register', navi: false, recommend: false, requiredLogin: false }
      },
      {
        path: '/login',
        name: 'login',
        component: import('@/modules/auth/views/LoginView.vue'),
        meta: { title: 'Login', navi: false, recommend: false, requiredLogin: false }
      },
      {
        path: '/password_reset',
        name: 'PasswordReset',
        component: import ('@/modules/auth/views/PasswordResetView.vue'),
        meta: { title: 'Reset Your Password', navi: false, recommend: false, requiredLogin: false }
      },
      {
        path: '/privacy',
        name: 'privacy',
        component: import ('@/modules/auth/views/PrivacyView.vue'),
        meta: { title: 'Privacy', navi: false, recommend: false, requiredLogin: false }
      }
    ]
})

export default authRouter;
