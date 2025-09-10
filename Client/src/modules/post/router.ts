import type { RouteRecordRaw } from "vue-router";
import PostDetailView from "./views/PostDetailView.vue";
import TestLikeFavoriteView from './views/TestLikeFavoriteView.vue';

const routes: RouteRecordRaw[] = [
  {
    path: "/post/:id",
    name: "PostDetail",
    component: PostDetailView,
    meta: { title: "帖子详情", navi: true, recommend: true }
  },
  {
    path: "/test-like-favorite",
    name: "TestLikeFavorite",
    component: TestLikeFavoriteView,
    meta: { title: "测试点赞收藏" }
  }
];

export default routes;
