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

export default router
