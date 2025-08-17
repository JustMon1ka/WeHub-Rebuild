// Client/src/modules/post/router.ts
import type { RouteRecordRaw } from "vue-router";
import PostDetailView from "./views/PostDetailView.vue";

const routes: RouteRecordRaw[] = [
  {
    path: "/post/:id",
    name: "PostDetail",
    component: PostDetailView,
    meta: { title: "帖子详情", navi: true, recommend: true }
  }
];

export default routes;
