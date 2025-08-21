/* eslint-disable @typescript-eslint/no-unused-vars */
import { createRouter, createWebHistory } from 'vue-router'
import authRouter from './modules/auth/router.ts'
import coreRouter from './modules/core/router.ts'
import postRouter from './modules/post/router.ts'
import postCreateRouter from './modules/postCreate/router.ts'
import notFound from './NotFound.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    ...coreRouter.getRoutes(),        // 核心模块路由
    ...authRouter.getRoutes(),        // auth 模块路由
    ...postRouter,        // post 模块
    ...postCreateRouter.getRoutes(),  // postCreate 模块路由
    {
      path: '/:pathMatch(.*)*',      // 404 路由
      name: 'not-found',
      component: notFound,
      meta: { title: '404', navi: true, recommend: false },
    },
  ],
})

router.beforeEach((to, from, next) => {
  document.title = (to.meta.title ? (to.meta.title as string) + ' - ' : '') + 'WeHub';

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
