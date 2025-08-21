import { createRouter, createWebHistory } from 'vue-router'
import { toggleLoginHover } from './App.vue'
import { User } from './modules/auth/scripts/User.ts'
import authRouter from './modules/auth/router.ts'
import coreRouter from './modules/core/router.ts'
import postRouter from './modules/post/router.ts'
import postCreateRouter from './modules/postCreate/router.ts'
import notFound from './NotFound.vue'
import userRouter from './modules/user/router.ts'
import { toggleNavigationBar, toggleRecommendBar } from '@/App.vue'
// import yourRouter from './yourModule/router.ts'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    ...postCreateRouter.getRoutes(),             // postCreate 模块路由数组
    // ...yourRouter.getRoutes(),     // 如果有其他模块路由
    ...coreRouter.getRoutes(),
    ...authRouter.getRoutes(),
    ...userRouter.getRoutes(),
    // ...yourRouter.getRoutes(),
    // your router must be imported before NotFound route
    {
      path: '/:pathMatch(.*)*',      // 404 路由
      name: 'not-found',
      component: notFound,
      meta: { title: '404', navi: true, recommend: false , requireLogin: false },
    },
  ],
})

router.beforeEach((to, from, next) => {
  // 设置页面标题
  document.title = (to.meta.title ? (to.meta.title as string) + ' - ' : '') + 'WeHub';

  // 控制导航栏显示
  if (to.meta.navi) {
    document.getElementById('navigation-pc')!.classList.remove('hidden');
  } else {
    document.getElementById('navigation-pc')!.classList.add('hidden');
  }

  // 控制推荐模块显示
  if (to.meta.recommend) {
    document.getElementById('recommend')!.classList.remove('hidden');
  } else {
    document.getElementById('recommend')!.classList.add('hidden');
  }

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
