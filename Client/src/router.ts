import { createRouter, createWebHistory } from 'vue-router'
import { toggleLoginHover } from './App.vue'
import { User } from './modules/auth/scripts/User.ts'
import authRouter from './modules/auth/router.ts'
import coreRouter from './modules/core/router.ts'
import notFound from './NotFound.vue'
import userRouter from './modules/user/router.ts'
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
      meta: { title: '404', navi: true, recommend: false , requiredLogin: false },
    },
  ],
})


router.beforeEach((to, from, next) => {
  // yourRouter.beforeEach((to, from, next) => {}); // 假如有其他模块的路由trigger，可以在这里调用

  document.title = (to.meta.title ? to.meta.title as string + ' - ' : '' )+ 'WeHub';

  if (to.meta.requiredLogin) {
    if (!User.getInstance()) {
      toggleLoginHover(true);
      if (from === undefined || from.meta.requiredLogin) {
        // 如果当前路由需要登录，但用户未登录且上一个路由也需要登录，则跳转回首页
        next('/');
        return;
      }
      if (from.path === '/login' || from.path === '/register' || from.path === '/password_reset') {
        toggleLoginHover(false);
      }
      return;
    }
  }

  if (to.path === '/login' || to.path === '/register' || to.path === '/password_reset') {
    toggleLoginHover(false);
  }

  if (to.meta.navi) {
    document.getElementById('navigation-pc')!.classList.remove('hidden');
  } else {
    document.getElementById('navigation-pc')!.classList.add('hidden');
  }
  if (to.meta.recommend) {
    document.getElementById('recommend')!.classList.remove('hidden');
  } else {
    document.getElementById('recommend')!.classList.add('hidden');
  }
  next();
})

export default router
