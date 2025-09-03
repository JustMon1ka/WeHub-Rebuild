import { createRouter, createWebHistory } from 'vue-router'
import ReportView from './views/ReportView.vue'

export const reportRouter = createRouter({
    history: createWebHistory(),
    routes: [
        {
            path: '/report',
            name: 'report',
            component: ReportView,
            meta: { title: '举报', navi: false, recommend: false }
        }
    ]
})

export default reportRouter