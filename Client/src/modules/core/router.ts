import HomeView from './views/HomeView.vue'
import MainPage from './components/MainPage.vue'

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
        component: () => import('./views/AboutView.vue'),
        meta: { title: 'About', navi: true, recommend: true, requireLogin: false },
      },
      {
        path: '/mainPage',
        name: 'mainPage',
        component: MainPage,
        meta: { title: 'Main Page', navi: true, recommend: true, requireLogin: true },
      },
    ]
  }
}
