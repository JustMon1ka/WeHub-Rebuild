import MessageView from './views/MessageView.vue'

export default {
    getRoutes() {
        return [
            {
                path: '/message',
                name: 'message',
                component: MessageView,
                meta: { title: '私信', navi: true, recommend: false }
            }
        ]
    }
}