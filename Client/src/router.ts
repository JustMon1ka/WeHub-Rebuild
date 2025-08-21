import { createRouter, createWebHistory } from 'vue-router'
import authRouter from './modules/auth/router.ts'
import coreRouter from './modules/core/router.ts'
import circleRouter from './modules/circle/router.ts'
import messageRouter from './modules/message/router.ts'
import noticeRouter from './modules/notice/router.ts'
import reportRouter from './modules/report/router.ts'
import notFound from './NotFound.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      redirect: '/community', // 默认重定向到社区页面
    },
    ...coreRouter.getRoutes(),
    ...authRouter.getRoutes(),
    ...circleRouter.getRoutes(),
    ...messageRouter.getRoutes(),
    ...noticeRouter.getRoutes(),
    ...reportRouter.getRoutes(),
    {
      path: '/:pathMatch(.*)*',
      name: 'not-found',
      component: notFound,
      meta: { title: '404', navi: true, recommend: false },
    },
  ],
})

router.beforeEach((to, from, next) => {
  document.title = (to.meta.title ? (to.meta.title as string) + ' - ' : '') + 'WeHub'

  if (to.meta.navi) {
    document.getElementById('navigation-pc')!.classList.remove('hidden')
  } else {
    document.getElementById('navigation-pc')!.classList.add('hidden')
  }
  if (to.meta.recommend) {
    document.getElementById('recommend')!.classList.remove('hidden')
  } else {
    document.getElementById('recommend')!.classList.add('hidden')
  }

  next()
})

export default router
