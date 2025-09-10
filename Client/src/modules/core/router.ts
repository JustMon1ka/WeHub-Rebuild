import HomeView from './views/HomeView.vue'

export default {
  getRoutes() {
    return [
      {
        path: '/',
        name: 'home',
        component: HomeView,
        meta: { title: 'Home', navi: true, recommend: true, requireLogin: false },
      },
      {
        path: '/about',
        name: 'about',
        // route level code-splitting
        // this generates a separate chunk (About.[hash].js) for this route
        // which is lazy-loaded when the route is visited.
        component: () => import('./views/AboutView.vue'),
        meta: { title: 'About', navi: true, recommend: true, requireLogin: false },
      },
    ]
  }
}
