import CreatePostView from './views/CreatePostView.vue';

export default {
  getRoutes() {
    return [
      {
        path: '/post/create',
        name: 'post-create',
        component: {} as any, // **不需要在主 view 渲染任何组件**
        // 注意：这里不填 components.overlay，也不填 component 主视图
        meta: { title: '发布新帖子', navi: true, recommend: false , requireLogin: false},
      },
    ]
  }
}
