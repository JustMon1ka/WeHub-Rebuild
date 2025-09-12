import type { RouteRecordRaw } from 'vue-router'

const routes: RouteRecordRaw[] = [
  {
    path: '/post/:id',
    name: 'PostDetail',
    component: () => import('./views/PostDetailView.vue'),
    meta: { title: '帖子详情', navi: true, recommend: true },
  },
  {
    path: "/post/edit/:id",
    name: "EditPost",
    component: () => import("./views/EditPostView.vue"),
    meta: { title: "修改帖子", navi: true, recommend: true }
  },
  {
    path: '/test-like-favorite',
    name: 'TestLikeFavorite',
    component: () => import('./views/TestLikeFavoriteView.vue'),
    meta: { title: '测试点赞收藏' },
  },
]

export default routes
