import type { RouteRecordRaw } from 'vue-router'

const routes: RouteRecordRaw[] = [
  {
    path: '/post/:id',
    name: 'PostDetail',
    component: () => import('./views/PostDetailView.vue'),
    meta: { title: '帖子详情', navi: true, recommend: true, requireLogin: false },
  },
  {
    path: "/post/edit/:id",
    name: "EditPost",
    component: () => import("./views/EditPostView.vue"),
    meta: { title: "修改帖子", navi: true, recommend: true, requireLogin: true },
  },
  {
    path: "/search",
    name: "SearchResultPage",
    component: () => import("@/modules/post/views/SearchResultPage.vue"),
  },
]

export default routes
