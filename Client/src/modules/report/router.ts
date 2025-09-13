
export default {
  getRoutes() {
    return [
      {
        path: '/report/:type/:id',
        name: 'report',
        component: () => import('./views/ReportView.vue'),
        meta: { title: '举报', navi: false, recommend: false, requireLogin: true },
        props: true
      }
    ]
  }
}
