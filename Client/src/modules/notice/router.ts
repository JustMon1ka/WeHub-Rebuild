import NoticeView from './views/NoticeView.vue'
import LikeDetailsView from './views/LikeDetailsView.vue'

export default {
    getRoutes() {
        return [
            {
                path: '/notice/',
                redirect: '/notice/like'
            },
            {
                path: '/notice/:type(at|comment|reply|like|repost)?',
                name: 'notice',
                component: NoticeView,
                meta: { title: '通知', navi: true, recommend: true, requireLogin: true },
            },
            {
                path: '/notice/likeDetails/:targetType(post|comment)/:targetId',
                name: 'likeDetails',
                component: LikeDetailsView,
                meta: { title: '点赞详情', navi: true, recommend: true, requireLogin: true },
            }
        ]
    }
}
