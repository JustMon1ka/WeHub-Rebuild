// modules/Founding/router.ts
import type { RouteRecordRaw } from "vue-router"
import FoundingView from '@/modules/Founding/views/FoundingView.vue'

const routes: RouteRecordRaw[] = [
  {
    path: "/founding",
    name: "founding",
    component: FoundingView,
    meta: { title: "发现", navi: true, recommend: true, requireLogin: true },
  },
  {
    path: "/topic/:topic",   // ✅ param 名叫 topic
    name: "topicDetail",
    component: () => import("./components/TopicDetailView.vue"),
    meta: { title: "话题详情", navi: false, recommend: false, requireLogin: true },
  }
]

export default {
  getRoutes() {
    return routes
  }
}
