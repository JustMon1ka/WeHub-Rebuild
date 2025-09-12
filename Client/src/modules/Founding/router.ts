// modules/Founding/router.ts
import type { RouteRecordRaw } from "vue-router"
import FoundingView from "./views/FoundingView.vue"
import TopicDetailView from "./components/TopicDetailView.vue"

const routes: RouteRecordRaw[] = [
  {
    path: "/founding",
    name: "founding",
    component: FoundingView,
    meta: { title: "发现", navi: true, recommend: false, requireLogin: true },
  },
  {
    path: "/topic/:topic",   // ✅ param 名叫 topic
    name: "topicDetail",
    component: TopicDetailView,
    meta: { title: "话题详情", navi: false, recommend: false, requireLogin: true },
  }
]

export default {
  getRoutes() {
    return routes
  }
}
