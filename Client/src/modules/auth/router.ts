
export default {
  getRoutes() {
    return [
      {
        path: '/register',
        name: 'Register',
        // lazy load the component
        component: () => import('@/modules/auth/views/RegisterView.vue'),
        meta: { title: 'Register', navi: false, recommend: false, requireLogin: false }
      },
      {
        path: '/login',
        name: 'Login',
        component: () => import('@/modules/auth/views/LoginView.vue'),
        meta: { title: 'Login', navi: false, recommend: false, requireLogin: false }
      },
      {
        path: '/password_reset',
        name: 'PasswordReset',
        component: () => import ('@/modules/auth/views/PasswordResetView.vue'),
        meta: { title: 'Reset Your Password', navi: false, recommend: false, requireLogin: false }
      },
      {
        path: '/privacy',
        name: 'Privacy',
        component: () => import ('@/modules/auth/views/PrivacyView.vue'),
        meta: { title: 'Privacy', navi: false, recommend: false, requireLogin: false }
      }
    ]
  }
}
