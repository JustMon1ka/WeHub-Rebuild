
export default {
  getRoutes() {
    return [
      {
        path: '/message',
        name: 'message',
        component: () => import('./views/MessageView.vue'),
        meta: { title: '私信', navi: true, recommend: false, requireLogin: true }
      },
      {
        path: '/message/:userId',
        name: 'message-user',
        component: () => import('./views/MessageView.vue'),
        meta: { title: '私信', navi: true, recommend: false, requireLogin: true }
      }
    ]
  }
}
