/* eslint-disable @typescript-eslint/no-unused-vars */
import { createRouter, createWebHistory } from 'vue-router'
import { User } from './modules/auth/scripts/User.ts'
import authRouter from './modules/auth/router.ts'
import coreRouter from './modules/core/router.ts'
import circleRouter from './modules/circle/router.ts'
import messageRouter from './modules/message/router.ts'
import noticeRouter from './modules/notice/router.ts'
import reportRouter from './modules/report/router.ts'
import notFound from './NotFound.vue'
import postRouter from './modules/post/router.ts'
import postCreateRouter from './modules/postCreate/router.ts'
import userRouter from './modules/user/router.ts'
import { showHoverLogin, showNavigationBar, showRecommendBar } from './App.vue'
// import yourRouter from './yourModule/router.ts'

export function toggleLoginHover(value: boolean | undefined) {
  showHoverLogin.value = value !== undefined ? value : !showHoverLogin.value
}

export function toggleNavigationBar(value: boolean | undefined) {
  showNavigationBar.value = value !== undefined ? value : !showNavigationBar.value
}

export function toggleRecommendBar(value: boolean | undefined) {
  showRecommendBar.value = value !== undefined ? value : !showRecommendBar.value
}

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    ...postCreateRouter.getRoutes(),             // postCreate 模块路由数组
    // ...yourRouter.getRoutes(),     // 如果有其他模块路由
    ...coreRouter.getRoutes(),
    ...authRouter.getRoutes(),
    ...userRouter.getRoutes(),
    ...circleRouter.getRoutes(),
    ...messageRouter.getRoutes(),
    ...noticeRouter.getRoutes(),
    ...reportRouter.getRoutes(),
    ...postRouter,                   // post 模块路由数组
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

  // 用响应式变量控制显示，不再操作 DOM
  toggleNavigationBar(!!to.meta.navi);
  toggleRecommendBar(!!to.meta.recommend);

  const checkLogin = () => {
    if (!User.getInstance()) {
      toggleLoginHover(true);
      if (!from.name) {
        next('/'); // 首次加载时跳转首页
      }
      return;
    }
    toggleLoginHover(false);
    next();
  }

  if (to.meta.requireLogin) {
    if (User.loading) {
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
});

export default router
