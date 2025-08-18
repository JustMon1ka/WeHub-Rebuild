import { createRouter, createWebHistory } from 'vue-router'
import CommunityPage from '@/views/CommunityPage.vue'
import CommunityDetailPage from '@/views/CommunityDetailPage.vue'
import CreateCommunity from '@/components/CreateCommunity.vue'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/',
      redirect: '/community',
    },
    {
      path: '/community',
      name: 'community',
      component: CommunityPage,
    },
    {
      path: '/communities',
      name: 'communities',
      component: CommunityPage,
    },
    {
      path: '/community/:id',
      name: 'community-detail',
      component: CommunityDetailPage,
      props: true,
    },
    {
      path: '/create-community',
      name: 'create-community',
      component: CreateCommunity,
    },
  ],
})

export default router
