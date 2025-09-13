// modules/Founding/router.ts
import type { RouteRecordRaw } from "vue-router"

const routes: RouteRecordRaw[] = [
  {
    path: "/founding",
    name: "founding",
    component: () => import("./views/FoundingView.vue"),
    meta: { title: "发现", navi: true, recommend: true, requireLogin: false },
  },
  {
    path: "/topic/:topic",   // ✅ param 名叫 topic
    name: "topicDetail",
    component: () => import("./components/TopicDetailView.vue"),
    meta: { title: "话题详情", navi: true, recommend: true, requireLogin: false },
  }
]

export default {
  getRoutes() {
    return routes
  }
}
