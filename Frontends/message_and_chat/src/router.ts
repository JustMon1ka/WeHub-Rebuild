import { createRouter, createWebHistory } from 'vue-router'
import MessageView from './views/MessageView.vue'
import NoticeView from './views/NoticeView.vue'
import OtherUserHomepage from './views/OtherUserHomepage.vue'
import PersonalHomepage from './views/PersonalHomepage.vue'
import LikeDetailsView from './views/LikeDetailsView.vue'

export const router = createRouter({
    history: createWebHistory(),
    routes: [
        { path: '/', redirect: '/message' },
        { path: '/message', component: MessageView },
        { path: '/notice', component: NoticeView },
        { path: '/otherUserHomepage', component: OtherUserHomepage },
        { path: '/personalHomepage', component: PersonalHomepage },
        { path: '/notice/likeDetails/:postId', component: LikeDetailsView }
    ]
})