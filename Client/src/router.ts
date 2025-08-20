import { createRouter, createWebHistory } from 'vue-router'
import { toggleLoginHover } from './App.vue'
import { User } from './modules/auth/scripts/User.ts'
import authRouter from './modules/auth/router.ts'
import coreRouter from './modules/core/router.ts'
import notFound from './NotFound.vue'
import userRouter from './modules/user/router.ts'
import { toggleNavigationBar, toggleRecommendBar } from '@/App.vue'
// import yourRouter from './yourModule/router.ts'


const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    ...coreRouter.getRoutes(),
    ...authRouter.getRoutes(),
    ...userRouter.getRoutes(),
    // ...yourRouter.getRoutes(),
    // your router must be imported before NotFound route
    {
      path: '/:pathMatch(.*)*',
      name: 'not-found',
      component: notFound,
      meta: { title: '404', navi: true, recommend: false , requireLogin: false },
    },
  ],
})


router.beforeEach((to, from, next) => {
  // yourRouter.beforeEach((to, from, next) => {}); // 假如有其他模块的路由trigger，可以在这里调用

  document.title = (to.meta.title ? to.meta.title as string + ' - ' : '' )+ 'WeHub';
  toggleNavigationBar(!!to.meta.navi);
  toggleRecommendBar(!!to.meta.recommend);

  const checkLogin = () => {
    if (!User.getInstance()) {
      toggleLoginHover(true);
      if (!from.name) { // 如果没有from.name，说明是首次加载
        next('/');
      }
      return;
    }
    toggleLoginHover(false);
    next();
  }

  if (to.meta.requireLogin) {
    if (User.loading){
      User.afterLoadCallbacks.push(checkLogin);
      return;
    } else {
      checkLogin();
      return;
    }
  } else {
    toggleLoginHover(false);
    next();
  }
})

export default router
