import { createRouter, createWebHistory } from 'vue-router'

import NoticeView from './views/NoticeView.vue'
import LikeDetailsView from './views/LikeDetailsView.vue'

export default {
    getRoutes() {
        return [
            {
                path: '/notice',
                name: 'notice',
                component: NoticeView,
                meta: { title: '通知', navi: true, recommend: true },
            },
            {
                path: '/notice/likeDetails/:postId',
                name: 'likeDetails',
                component: LikeDetailsView,
                meta: { title: '点赞详情', navi: true, recommend: true },
            }
        ]
    }
}
