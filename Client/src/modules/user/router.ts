import { createRouter, createWebHistory } from 'vue-router'

export const userRouter = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/user_page/:userId_p',
      name: 'UserPage',
      component: () => import('@/modules/user/views/UserPageView.vue'),
      props: true,
      meta: { title: 'User Page', navi: true, recommend: true , requiredLogin: false }
    },
    {
      path: '/user_guide',
      name: 'UserGuide',
      component: () =>  import('@/modules/user/views/UserGuideView.vue'),
      meta: { title: 'User Guide', navi: false, recommend: false, requiredLogin: false }
    },
  ]
})

export default userRouter;
