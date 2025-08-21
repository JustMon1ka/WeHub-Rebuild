import ReportView from './views/ReportView.vue'

export default {
    getRoutes() {
        return [
            {
                path: '/report',
                name: 'report',
                component: ReportView,
                meta: { title: '举报', navi: false, recommend: false }
            }
        ]
    }
}