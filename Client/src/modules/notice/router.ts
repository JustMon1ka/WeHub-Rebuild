import NoticeView from './views/NoticeView.vue'

export default {
    getRoutes() {
        return [
            {
                path: '/notice/',
                redirect: '/notice/like'
            },
            {
                path: '/notice/:type(mention|comment|reply|like|repost)?',
                name: 'notice',
                component: NoticeView,
                meta: { title: '通知', navi: true, recommend: true, requireLogin: true },
            },
            {
                path: '/notice/likeDetails/:targetType(post|comment)/:targetId',
                name: 'likeDetails',
                component: () => import('./views/LikeDetailsView.vue'),
                meta: { title: '点赞详情', navi: true, recommend: true, requireLogin: true },
            }
        ]
    }
}
