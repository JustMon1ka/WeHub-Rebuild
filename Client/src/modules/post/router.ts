import type { RouteRecordRaw } from 'vue-router'
import PostDetailView from './views/PostDetailView.vue'
import EditPostView from './views/EditPostView.vue'
import TestLikeFavoriteView from './views/TestLikeFavoriteView.vue'

const routes: RouteRecordRaw[] = [
  {
    path: '/post/:id',
    name: 'PostDetail',
    component: PostDetailView,
    meta: { title: '帖子详情', navi: true, recommend: true },
  },
  {
    path: "/post/edit/:id",
    name: "EditPost",
    component: EditPostView,
    meta: { title: "修改帖子", navi: true, recommend: true }
  },
  {
    path: '/test-like-favorite',
    name: 'TestLikeFavorite',
    component: TestLikeFavoriteView,
    meta: { title: '测试点赞收藏' },
  },
]

export default routes
