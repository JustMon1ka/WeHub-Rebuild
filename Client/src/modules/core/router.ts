import MainView from './views/MainView.vue'

export default {
  getRoutes() {
    return [
      {
        path: '/',
        name: 'home',
        component: MainView,
        meta: { title: 'Home', navi: true, recommend: true, requireLogin: false },
      },
      {
        path: '/about',
        name: 'about',
        // route level code-splitting
        component: () => import('./views/AboutView.vue'),
        meta: { title: 'About', navi: true, recommend: true, requireLogin: false },
      },
    ]
  }
}
