import { createRouter, createWebHistory } from 'vue-router'
import MessageView from './views/MessageView.vue'
import NoticeView from './views/NoticeView.vue'

export const router = createRouter({
    history: createWebHistory(),
    routes: [
        { path: '/', redirect: '/message' },
        { path: '/message', component: MessageView },
        { path: '/notice', component: NoticeView }
    ]
})