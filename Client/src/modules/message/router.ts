import { createRouter, createWebHistory } from 'vue-router'
import MessageView from './views/MessageView.vue'

export const messageRouter = createRouter({
    history: createWebHistory(),
    routes: [
        {
            path: '/message',
            name: 'message',
            component: MessageView,
            meta: { title: '私信', navi: true, recommend: false, requireLogin: true }
        },
        {
            path: '/message/:userId',
            name: 'message-user',
            component: MessageView,
            meta: { title: '私信', navi: true, recommend: false, requireLogin: true }
        }
    ]
})

export default messageRouter
