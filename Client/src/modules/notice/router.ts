import { createRouter, createWebHistory } from 'vue-router'

import NoticeView from './views/NoticeView.vue'
import LikeDetailsView from './views/LikeDetailsView.vue'

export const noticeRouter = createRouter({
    history: createWebHistory(),
    routes: [
        {
            path: '/notice/:type(at|reply|like|repost)?',
            name: 'notice',
            component: NoticeView,
            meta: { title: '通知', navi: true, recommend: true },
        },
        {
            path: '/notice/',
            redirect: '/notice/at'
        },
        {
            path: '/notice/likeDetails/:postId',
            name: 'likeDetails',
            component: LikeDetailsView,
            meta: { title: '点赞详情', navi: true, recommend: true },
        }
    ]

})

export default noticeRouter