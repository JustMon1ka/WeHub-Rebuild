import { createRouter, createWebHistory } from 'vue-router'

export const userRouter = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/user_page',
      name: 'UserPage',
      component: () =>  import ('@/modules/user/views/UserPageView.vue'),
      meta: { title: 'User Page', navi: true, recommend: true, requiredLogin: false }
    },
    {
      path: '/user_guide',
      name: 'UserGuide',
      component: () =>  import('@/modules/user/views/UserGuideView.vue'),
      meta: { title: 'User Guide', navi: false, recommend: false, requiredLogin: false }
    },
    {
      path: '/follow_list',
      name: 'FollowList',
      component: () =>  import ('@/modules/user/views/FollowListView.vue'),
      meta: { title: 'Follow List', navi: true, recommend: true, requiredLogin: false }
    },
  ]
})

export default userRouter;
