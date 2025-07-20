import { createRouter, createWebHistory } from 'vue-router'
import authRouter from './modules/auth/router.ts'
import coreRouter from './modules/core/router.ts'
// import yourRouter from './yourModule/router.ts'


const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    ...coreRouter.getRoutes(),
    ...authRouter.getRoutes(),
    // ...yourRouter.getRoutes(),
  ],
})


router.beforeEach((to, from, next) => {
  // yourRouter.beforeEach((to, from, next) => {}); // 假如有其他模块的路由trigger，可以在这里调用

  document.title = (to.meta.title ? to.meta.title as string + ' - ' : '' )+ 'WeHub';

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
