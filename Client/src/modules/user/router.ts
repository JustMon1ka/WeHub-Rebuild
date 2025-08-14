import { createRouter, createWebHistory } from 'vue-router'

export const userRouter = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/UserInfo',
      name: 'UserInfo',
      component: import('@/modules/user/views/UserInfoView.vue'),
      meta: { title: 'User Info', navi: true, recommend: true, requiredLogin: false }
    },
    {
      path: '/UserPage',
      name: 'UserPage',
      component: import ('@/modules/user/views/UserPageView.vue'),
      meta: { title: 'User Page', navi: true, recommend: true, requiredLogin: false }
    },
    {
      path: '/UserGuide',
      name: 'UserGuide',
      component: import('@/modules/user/views/UserGuideView.vue'),
      meta: { title: 'User Guide', navi: false, recommend: false, requiredLogin: false }
    },
    {
      path: '/FollowList',
      name: 'FollowList',
      component: import ('@/modules/user/views/FollowListView.vue'),
      meta: { title: 'Follow List', navi: true, recommend: true, requiredLogin: false }
    },
  ]
})

export default userRouter;
