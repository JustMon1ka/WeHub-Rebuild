// src/modules/circle/router.ts
import CommunityPage from './views/CommunityPage.vue'
import CommunityDetailPage from './views/CommunityDetailPage.vue'
import CreateCommunity from './components/CreateCommunity.vue'
import ActivityList from './components/ActivityList.vue'

export default {
  getRoutes() {
    return [
      {
        path: '/community',
        name: 'community',
        component: CommunityPage,
        meta: { title: '社区', navi: true, recommend: false, requireLogin: false },
      },
      {
        path: '/communities',
        name: 'communities',
        component: CommunityPage,
        meta: { title: '社区列表', navi: true, recommend: true, requireLogin: false },
      },
      {
        path: '/community/:id',
        name: 'community-detail',
        component: CommunityDetailPage,
        props: true,
        meta: { title: '社区详情', navi: true, recommend: false, requireLogin: false },
      },
      {
        path: '/create-community',
        name: 'create-community',
        component: CreateCommunity,
        meta: { title: '创建社区', navi: false, recommend: false, requireLogin: true },
      },
      // 圈子活动路由
      {
        path: '/circles/:id/activities',
        name: 'CircleActivities',
        component: ActivityList,
        props: (route) => ({
          circleId: parseInt(route.params.id),
          canCreateActivity: true, // 这里可以根据用户权限动态设置
        }),
        meta: { title: '圈子活动', navi: true, recommend: false, requireLogin: false },
      },
    ]
  },
}
