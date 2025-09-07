import { type notice } from './types';

const rawNoticeList = [
    {
        type: 'comment',
        sender: {
            id: 1,
            nickname: 'dev_master',
            avatar: 'https://placehold.co/100x100/34d399/064e3b?text=D',
            url: '/user/dev_master'
        },
        time: '2024-01-15 00:00',
        isRead: false,
        objectType: 'post',
        targetPostId: 123,
        targetPostTitle: '我个人更倾向于 Tailwind CSS,虽然初期学习曲线比较陡峭，但是一旦掌握后开发效率会大大提高。',
        targetPostTitleImage: 'https://placehold.co/400x200/06b6d4/ffffff?text=Tailwind+CSS',
        newCommentContent: '这个观点很有道理！'
    },
    {
        type: 'like',
        sender: {
            id: 2,
            nickname: 'code_lover',
            avatar: 'https://placehold.co/100x100/ec4899/831843?text=C',
            url: '/user/code_lover'
        },
        time: '2024-01-15 09:05',
        isRead: false,
        objectType: 'post',
        targetPostId: 124,
        targetPostTitle: 'Vue 3 的 Composition API 真的很棒，比 Options API 更加灵活和强大！',
        targetPostTitleImage: 'https://placehold.co/400x200/4fc08d/ffffff?text=Vue.js+3'
    },
    // 多个用户点赞同一个帖子 124
    {
        type: 'like',
        sender: {
            id: 11,
            nickname: 'vue_enthusiast',
            avatar: 'https://placehold.co/100x100/3b82f6/1e40af?text=V',
            url: '/user/vue_enthusiast'
        },
        time: '2024-01-15 09:20',
        isRead: false,
        objectType: 'post',
        targetPostId: 124,
        targetPostTitle: 'Vue 3 的 Composition API 真的很棒，比 Options API 更加灵活和强大！',
        targetPostTitleImage: 'https://placehold.co/400x200/4fc08d/ffffff?text=Vue.js+3'
    },
    {
        type: 'like',
        sender: {
            id: 12,
            nickname: 'js_master',
            avatar: 'https://placehold.co/100x100/f59e0b/92400e?text=J',
            url: '/user/js_master'
        },
        time: '2024-01-15 09:05',
        isRead: false,
        objectType: 'comment',
        targetPostId: 124,
        targetPostTitle: '',
        targetPostTitleImage: '',
        targetCommentId: 1001,
        targetCommentContent: '我也觉得 Composition API 很棒，特别是在复杂组件中的逻辑复用方面。',
        targetCommentAuthor: 'vue_developer'
    },
    {
        type: 'like',
        sender: {
            id: 13,
            nickname: 'frontend_guru',
            avatar: 'https://placehold.co/100x100/8b5cf6/5b21b6?text=F',
            url: '/user/frontend_guru'
        },
        time: '2024-01-15 09:00',
        isRead: false,
        objectType: 'post',
        targetPostId: 124,
        targetPostTitle: 'Vue 3 的 Composition API 真的很棒，比 Options API 更加灵活和强大！',
        targetPostTitleImage: 'https://placehold.co/400x200/4fc08d/ffffff?text=Vue.js+3'
    },
    {
        type: 'at',
        sender: {
            id: 3,
            nickname: 'web_dev',
            avatar: 'https://placehold.co/100x100/facc15/78350f?text=W',
            url: '/user/web_dev'
        },
        time: '2024-01-15 08:45',
        isRead: false,
        objectType: 'post',
        targetPostId: 125,
        targetPostTitle: '@你 这个组件写得很好，学习了！希望能多分享一些前端开发经验。',
        targetPostTitleImage: 'https://placehold.co/400x200/f59e0b/ffffff?text=Frontend+Component',
        atContent: '@你 这个组件写得很好，学习了！希望能多分享一些前端开发经验。'
    },
    {
        type: 'follow',
        sender: {
            id: 4,
            nickname: 'frontend_expert',
            avatar: 'https://placehold.co/100x100/60a5fa/1e3a8a?text=F',
            url: '/user/frontend_expert'
        },
        time: '2024-01-15 07:20',
        isRead: false,
        objectType: 'user',
        targetPostId: 0,
        targetPostTitle: '',
        targetPostTitleImage: ''
    },
    {
        type: 'comment',
        sender: {
            id: 5,
            nickname: 'react_fan',
            avatar: 'https://placehold.co/100x100/f97316/9a3412?text=R',
            url: '/user/react_fan'
        },
        time: '2024-01-15 06:30',
        isRead: false,
        objectType: 'post',
        targetPostId: 126,
        targetPostTitle: 'React 和 Vue 各有优势，选择哪个主要看项目需求和团队熟悉度。',
        targetPostTitleImage: 'https://placehold.co/400x200/61dafb/ffffff?text=React+vs+Vue',
        newCommentContent: '完全同意这个观点！'
    },
    {
        type: 'like',
        sender: {
            id: 6,
            nickname: 'ui_designer',
            avatar: 'https://placehold.co/100x100/a855f7/581c87?text=U',
            url: '/user/ui_designer'
        },
        time: '2024-01-15 05:45',
        isRead: false,
        objectType: 'post',
        targetPostId: 127,
        targetPostTitle: '设计系统真的很重要，能大大提高开发效率和用户体验的一致性。',
        targetPostTitleImage: 'https://placehold.co/400x200/8b5cf6/ffffff?text=Design+System'
    },
    // 多个用户点赞同一个帖子 127
    {
        type: 'like',
        sender: {
            id: 14,
            nickname: 'design_lover',
            avatar: 'https://placehold.co/100x100/ec4899/831843?text=D',
            url: '/user/design_lover'
        },
        time: '2024-01-15 05:40',
        isRead: false,
        objectType: 'post',
        targetPostId: 127,
        targetPostTitle: '设计系统真的很重要，能大大提高开发效率和用户体验的一致性。',
        targetPostTitleImage: 'https://placehold.co/400x200/8b5cf6/ffffff?text=Design+System'
    },
    {
        type: 'like',
        sender: {
            id: 15,
            nickname: 'ux_specialist',
            avatar: 'https://placehold.co/100x100/10b981/065f46?text=U',
            url: '/user/ux_specialist'
        },
        time: '2024-01-15 05:35',
        isRead: false,
        objectType: 'post',
        targetPostId: 127,
        targetPostTitle: '设计系统真的很重要，能大大提高开发效率和用户体验的一致性。',
        targetPostTitleImage: 'https://placehold.co/400x200/8b5cf6/ffffff?text=Design+System'
    },
    {
        type: 'like',
        sender: {
            id: 16,
            nickname: 'creative_dev',
            avatar: 'https://placehold.co/100x100/f59e0b/92400e?text=C',
            url: '/user/creative_dev'
        },
        time: '2024-01-15 05:30',
        isRead: false,
        objectType: 'post',
        targetPostId: 127,
        targetPostTitle: '设计系统真的很重要，能大大提高开发效率和用户体验的一致性。',
        targetPostTitleImage: 'https://placehold.co/400x200/8b5cf6/ffffff?text=Design+System'
    },
    {
        type: 'at',
        sender: {
            id: 7,
            nickname: 'backend_dev',
            avatar: 'https://placehold.co/100x100/ef4444/991b1b?text=B',
            url: '/user/backend_dev'
        },
        time: '2024-01-15 04:20',
        isRead: false,
        objectType: 'post',
        targetPostId: 128,
        targetPostTitle: '@你 前端和后端的协作真的很重要，API 设计要考虑到前端的便利性。',
        targetPostTitleImage: 'https://placehold.co/400x200/ef4444/ffffff?text=API+Design',
        atContent: '@你 前端和后端的协作真的很重要，API 设计要考虑到前端的便利性。'
    },
    {
        type: 'follow',
        sender: {
            id: 8,
            nickname: 'fullstack_hero',
            avatar: 'https://placehold.co/100x100/10b981/065f46?text=H',
            url: '/user/fullstack_hero'
        },
        time: '2024-01-15 03:10',
        isRead: false,
        objectType: 'user',
        targetPostId: 0,
        targetPostTitle: '',
        targetPostTitleImage: ''
    },
    // 新增：转发通知
    {
        type: 'repost',
        sender: {
            id: 37,
            nickname: 'content_curator',
            avatar: 'https://placehold.co/100x100/8b5cf6/5b21b6?text=C',
            url: '/user/content_curator'
        },
        time: '2024-01-15 02:45',
        isRead: false,
        objectType: 'post',
        targetPostId: 132,
        targetPostTitle: '前端性能优化的最佳实践总结，包含图片懒加载、代码分割等技巧',
        targetPostTitleImage: 'https://placehold.co/400x200/10b981/ffffff?text=Performance+Tips',
        repostContent: '这篇关于前端性能优化的文章写得很好，分享给大家学习！'
    },
    {
        type: 'repost',
        sender: {
            id: 38,
            nickname: 'tech_sharer',
            avatar: 'https://placehold.co/100x100/f59e0b/92400e?text=T',
            url: '/user/tech_sharer'
        },
        time: '2024-01-15 02:20',
        isRead: false,
        objectType: 'post',
        targetPostId: 133,
        targetPostTitle: 'CSS Grid 布局完全指南，从基础到高级应用',
        targetPostTitleImage: 'https://placehold.co/400x200/8b5cf6/ffffff?text=CSS+Grid',
        repostContent: 'CSS Grid 真的很强大，这个教程讲解得很详细，推荐学习！'
    },
    {
        type: 'repost',
        sender: {
            id: 39,
            nickname: 'web_architect',
            avatar: 'https://placehold.co/100x100/ef4444/991b1b?text=W',
            url: '/user/web_architect'
        },
        time: '2024-01-15 02:00',
        isRead: false,
        objectType: 'comment',
        targetPostId: 134,
        targetPostTitle: '',
        targetPostTitleImage: '',
        targetCommentId: 1008,
        targetCommentContent: '微前端架构确实能解决大型应用的维护问题',
        targetCommentAuthor: 'architect_pro',
        repostContent: '这个观点很有见地，微前端确实是未来的趋势'
    },
    {
        type: 'comment',
        sender: {
            id: 9,
            nickname: 'mobile_dev',
            avatar: 'https://placehold.co/100x100/8b5cf6/5b21b6?text=M',
            url: '/user/mobile_dev'
        },
        time: '2024-01-15 02:30',
        isRead: false,
        objectType: 'post',
        targetPostId: 129,
        targetPostTitle: '移动端开发确实有很多挑战，响应式设计和性能优化都很关键。',
        targetPostTitleImage: 'https://placehold.co/400x200/10b981/ffffff?text=Mobile+Development',
        newCommentContent: '移动端确实有很多挑战！'
    },
    {
        type: 'like',
        sender: {
            id: 10,
            nickname: 'data_scientist',
            avatar: 'https://placehold.co/100x100/06b6d4/0e7490?text=S',
            url: '/user/data_scientist'
        },
        time: '2024-01-15 01:15',
        targetPostId: 130,
        targetPostTitle: '数据可视化在前端开发中越来越重要，D3.js 和 ECharts 都是不错的选择。',
        targetPostTitleImage: 'https://placehold.co/400x200/06b6d4/ffffff?text=Data+Visualization',
        isRead: false,
        objectType: 'post'
    },
    // 多个用户点赞同一个帖子 130
    {
        type: 'like',
        sender: {
            id: 17,
            nickname: 'data_analyst',
            avatar: 'https://placehold.co/100x100/84cc16/3f6212?text=A',
            url: '/user/data_analyst'
        },
        time: '2024-01-15 01:10',
        targetPostId: 130,
        targetPostTitle: '数据可视化在前端开发中越来越重要，D3.js 和 ECharts 都是不错的选择。',
        targetPostTitleImage: 'https://placehold.co/400x200/06b6d4/ffffff?text=Data+Visualization',
        isRead: false,
        objectType: 'post'
    },
    {
        type: 'like',
        sender: {
            id: 18,
            nickname: 'chart_expert',
            avatar: 'https://placehold.co/100x100/f97316/9a3412?text=C',
            url: '/user/chart_expert'
        },
        time: '2024-01-15 01:05',
        targetPostId: 130,
        targetPostTitle: '数据可视化在前端开发中越来越重要，D3.js 和 ECharts 都是不错的选择。',
        targetPostTitleImage: 'https://placehold.co/400x200/06b6d4/ffffff?text=Data+Visualization',
        isRead: false,
        objectType: 'post'
    },
    {
        type: 'like',
        sender: {
            id: 19,
            nickname: 'visualization_pro',
            avatar: 'https://placehold.co/100x100/8b5cf6/5b21b6?text=V',
            url: '/user/visualization_pro'
        },
        time: '2024-01-15 01:00',
        targetPostId: 130,
        targetPostTitle: '数据可视化在前端开发中越来越重要，D3.js 和 ECharts 都是不错的选择。',
        targetPostTitleImage: 'https://placehold.co/400x200/06b6d4/ffffff?text=Data+Visualization',
        isRead: false,
        objectType: 'post'
    },
    {
        type: 'like',
        sender: {
            id: 20,
            nickname: 'big_data_dev',
            avatar: 'https://placehold.co/100x100/ef4444/991b1b?text=B',
            url: '/user/big_data_dev'
        },
        time: '2024-01-15 00:55',
        targetPostId: 130,
        targetPostTitle: '数据可视化在前端开发中越来越重要，D3.js 和 ECharts 都是不错的选择。',
        targetPostTitleImage: 'https://placehold.co/400x200/06b6d4/ffffff?text=Data+Visualization',
        isRead: false,
        objectType: 'post'
    },
    // 新增一个热门帖子，有更多点赞
    {
        type: 'like',
        sender: {
            id: 21,
            nickname: 'ai_researcher',
            avatar: 'https://placehold.co/100x100/06b6d4/0e7490?text=A',
            url: '/user/ai_researcher'
        },
        time: '2024-01-15 00:50',
        targetPostId: 131,
        targetPostTitle: 'AI 在前端开发中的应用越来越广泛，从代码生成到智能调试都有很大潜力。',
        targetPostTitleImage: 'https://placehold.co/400x200/8b5cf6/ffffff?text=AI+Frontend',
        isRead: false,
        objectType: 'post'
    },
    {
        type: 'like',
        sender: {
            id: 22,
            nickname: 'ml_engineer',
            avatar: 'https://placehold.co/100x100/84cc16/3f6212?text=M',
            url: '/user/ml_engineer'
        },
        time: '2024-01-15 00:45',
        targetPostId: 131,
        targetPostTitle: 'AI 在前端开发中的应用越来越广泛，从代码生成到智能调试都有很大潜力。',
        targetPostTitleImage: 'https://placehold.co/400x200/8b5cf6/ffffff?text=AI+Frontend',
        isRead: false,
        objectType: 'post'
    },
    {
        type: 'like',
        sender: {
            id: 23,
            nickname: 'tech_innovator',
            avatar: 'https://placehold.co/100x100/f59e0b/92400e?text=T',
            url: '/user/tech_innovator'
        },
        time: '2024-01-15 00:40',
        targetPostId: 131,
        targetPostTitle: 'AI 在前端开发中的应用越来越广泛，从代码生成到智能调试都有很大潜力。',
        targetPostTitleImage: 'https://placehold.co/400x200/8b5cf6/ffffff?text=AI+Frontend',
        isRead: false,
        objectType: 'post'
    },
    {
        type: 'like',
        sender: {
            id: 24,
            nickname: 'future_dev',
            avatar: 'https://placehold.co/100x100/8b5cf6/5b21b6?text=F',
            url: '/user/future_dev'
        },
        time: '2024-01-15 00:35',
        targetPostId: 131,
        targetPostTitle: 'AI 在前端开发中的应用越来越广泛，从代码生成到智能调试都有很大潜力。',
        targetPostTitleImage: 'https://placehold.co/400x200/8b5cf6/ffffff?text=AI+Frontend',
        isRead: false,
        objectType: 'post'
    },
    {
        type: 'like',
        sender: {
            id: 25,
            nickname: 'smart_coder',
            avatar: 'https://placehold.co/100x100/10b981/065f46?text=S',
            url: '/user/smart_coder'
        },
        time: '2024-01-15 00:30',
        targetPostId: 131,
        targetPostTitle: 'AI 在前端开发中的应用越来越广泛，从代码生成到智能调试都有很大潜力。',
        targetPostTitleImage: 'https://placehold.co/400x200/8b5cf6/ffffff?text=AI+Frontend',
        isRead: false,
        objectType: 'post'
    },
    // 新增：更多点赞评论的通知
    {
        type: 'like',
        sender: {
            id: 26,
            nickname: 'comment_lover',
            avatar: 'https://placehold.co/100x100/f472b6/be185d?text=C',
            url: '/user/comment_lover'
        },
        time: '2024-01-16 10:30',
        targetPostId: 123,
        targetPostTitle: '',
        targetPostTitleImage: '',
        isRead: false,
        objectType: 'comment',
        targetCommentId: 1002,
        targetCommentContent: '这个观点很有道理！',
        targetCommentAuthor: 'dev_master'
    },
    {
        type: 'like',
        sender: {
            id: 27,
            nickname: 'css_expert',
            avatar: 'https://placehold.co/100x100/06b6d4/0e7490?text=C',
            url: '/user/css_expert'
        },
        time: '2024-01-16 10:15',
        targetPostId: 126,
        targetPostTitle: '',
        targetPostTitleImage: '',
        isRead: false,
        objectType: 'comment',
        targetCommentId: 1003,
        targetCommentContent: '完全同意这个观点！',
        targetCommentAuthor: 'react_fan'
    },
    // 新增：回复评论的通知
    {
        type: 'comment',
        sender: {
            id: 28,
            nickname: 'vue_expert',
            avatar: 'https://placehold.co/100x100/10b981/065f46?text=V',
            url: '/user/vue_expert'
        },
        time: '2024-01-16 09:45',
        targetPostId: 124,
        targetPostTitle: '',
        targetPostTitleImage: '',
        isRead: false,
        objectType: 'comment',
        newCommentContent: '确实如此！而且 TypeScript 支持也更好了。',
        targetCommentId: 1001,
        targetCommentContent: '我也觉得 Composition API 很棒，特别是在复杂组件中的逻辑复用方面。',
        targetCommentAuthor: 'vue_developer'
    },
    {
        type: 'comment',
        sender: {
            id: 29,
            nickname: 'frontend_newbie',
            avatar: 'https://placehold.co/100x100/f59e0b/92400e?text=N',
            url: '/user/frontend_newbie'
        },
        time: '2024-01-16 09:30',
        targetPostId: 129,
        targetPostTitle: '',
        targetPostTitleImage: '',
        isRead: false,
        objectType: 'comment',
        newCommentContent: '请问有什么好的移动端调试工具推荐吗？',
        targetCommentId: 1004,
        targetCommentContent: '移动端确实有很多挑战！',
        targetCommentAuthor: 'mobile_dev'
    },
    // 新增：更多点赞评论的通知
    {
        type: 'like',
        sender: {
            id: 30,
            nickname: 'performance_guru',
            avatar: 'https://placehold.co/100x100/8b5cf6/5b21b6?text=P',
            url: '/user/performance_guru'
        },
        time: '2024-01-16 09:15',
        targetPostId: 129,
        targetPostTitle: '',
        targetPostTitleImage: '',
        isRead: false,
        objectType: 'comment',
        targetCommentId: 1004,
        targetCommentContent: '移动端确实有很多挑战！',
        targetCommentAuthor: 'mobile_dev'
    },
    // 新增：回复评论的通知
    {
        type: 'comment',
        sender: {
            id: 31,
            nickname: 'tailwind_fan',
            avatar: 'https://placehold.co/100x100/06b6d4/0e7490?text=T',
            url: '/user/tailwind_fan'
        },
        time: '2024-01-16 09:00',
        targetPostId: 123,
        targetPostTitle: '',
        targetPostTitleImage: '',
        isRead: false,
        objectType: 'comment',
        newCommentContent: '我也是 Tailwind 的忠实用户，配合 VS Code 插件使用体验更佳！',
        targetCommentId: 1002,
        targetCommentContent: '这个观点很有道理！',
        targetCommentAuthor: 'dev_master'
    },
    {
        type: 'like',
        sender: {
            id: 32,
            nickname: 'design_system_pro',
            avatar: 'https://placehold.co/100x100/f97316/9a3412?text=D',
            url: '/user/design_system_pro'
        },
        time: '2024-01-16 08:45',
        targetPostId: 127,
        targetPostTitle: '',
        targetPostTitleImage: '',
        isRead: false,
        objectType: 'comment',
        targetCommentId: 1005,
        targetCommentContent: '设计系统确实能提高团队协作效率，特别是在大型项目中。',
        targetCommentAuthor: 'ui_designer'
    },
    // 新增：回复评论的通知
    {
        type: 'comment',
        sender: {
            id: 33,
            nickname: 'react_developer',
            avatar: 'https://placehold.co/100x100/ef4444/991b1b?text=R',
            url: '/user/react_developer'
        },
        time: '2024-01-16 08:30',
        targetPostId: 126,
        targetPostTitle: '',
        targetPostTitleImage: '',
        isRead: false,
        objectType: 'comment',
        newCommentContent: 'React 的生态系统确实很丰富，但 Vue 的学习曲线更平缓。',
        targetCommentId: 1003,
        targetCommentContent: '完全同意这个观点！',
        targetCommentAuthor: 'react_fan'
    },
    {
        type: 'like',
        sender: {
            id: 34,
            nickname: 'fullstack_dev',
            avatar: 'https://placehold.co/100x100/84cc16/3f6212?text=F',
            url: '/user/fullstack_dev'
        },
        time: '2024-01-16 08:15',
        targetPostId: 124,
        targetPostTitle: '',
        targetPostTitleImage: '',
        isRead: false,
        objectType: 'comment',
        targetCommentId: 1006,
        targetCommentContent: '确实如此！而且 TypeScript 支持也更好了。',
        targetCommentAuthor: 'vue_expert'
    },
    // 新增：更多回复评论的通知
    {
        type: 'comment',
        sender: {
            id: 35,
            nickname: 'mobile_expert',
            avatar: 'https://placehold.co/100x100/a855f7/581c87?text=M',
            url: '/user/mobile_expert'
        },
        time: '2024-01-16 08:00',
        targetPostId: 129,
        targetPostTitle: '',
        targetPostTitleImage: '',
        isRead: false,
        objectType: 'comment',
        newCommentContent: '推荐使用 Chrome DevTools 的设备模拟功能，还有 Weinre 用于真机调试。',
        targetCommentId: 1007,
        targetCommentContent: '请问有什么好的移动端调试工具推荐吗？',
        targetCommentAuthor: 'frontend_newbie'
    },
    {
        type: 'like',
        sender: {
            id: 36,
            nickname: 'debug_master',
            avatar: 'https://placehold.co/100x100/10b981/065f46?text=D',
            url: '/user/debug_master'
        },
        time: '2024-01-16 07:45',
        targetPostId: 129,
        targetPostTitle: '',
        targetPostTitleImage: '',
        isRead: false,
        objectType: 'comment',
        targetCommentId: 1007,
        targetCommentContent: '请问有什么好的移动端调试工具推荐吗？',
        targetCommentAuthor: 'frontend_newbie'
    }
];

// ====== 追加更多本地模拟数据：用于前端分页/无限滚动测试 ======
// 说明：为避免手写大体量静态数据，这里按时间倒序（每条相差 1 分钟）
// 自动生成若干条点赞/评论/@我三类通知，便于在本地进行分页滚动测试。
function pad2(n: number) {
    return n.toString().padStart(2, '0');
}

function formatUtcToLocalString(date: Date) {
    const y = date.getUTCFullYear();
    const m = pad2(date.getUTCMonth() + 1);
    const d = pad2(date.getUTCDate());
    const hh = pad2(date.getUTCHours());
    const mm = pad2(date.getUTCMinutes());
    return `${y}-${m}-${d} ${hh}:${mm}`;
}

function createSender(i: number) {
    const id = 100 + i;
    return {
        id,
        nickname: `user_${id}`,
        avatar: 'https://placehold.co/100x100/0ea5e9/0c4a6e?text=U',
        url: `/user/user_${id}`,
    };
}

(function appendGeneratedNotices() {
    // 以 2024-01-16 10:30 的 UTC 时间为基准，之后每条记录减 1 分钟
    const baseUtcMs = Date.UTC(2024, 0, 16, 10, 30, 0);
    const total = 180; // 可按需增大

    for (let i = 0; i < total; i++) {
        const timeMs = baseUtcMs - (i + 1) * 60 * 1000;
        const date = new Date(timeMs);
        const time = formatUtcToLocalString(date);
        const sender = createSender(i);
        const targetPostId = 1000 + (i % 25);
        const targetPostTitle = `测试帖子 #${targetPostId}`;
        const targetPostTitleImage = `https://placehold.co/400x200/334155/ffffff?text=Post+${targetPostId}`;

        const mod = i % 3;
        if (mod === 0) {
            // like
            rawNoticeList.push({
                type: 'like',
                sender,
                time,
                isRead: false,
                objectType: 'post',
                targetPostId,
                targetPostTitle,
                targetPostTitleImage,
            } as any);
        } else if (mod === 1) {
            // comment
            rawNoticeList.push({
                type: 'comment',
                sender,
                time,
                isRead: false,
                objectType: 'post',
                targetPostId,
                targetPostTitle,
                targetPostTitleImage,
                newCommentContent: `这是一条测试评论 ${i}`,
            } as any);
        } else {
            // at
            rawNoticeList.push({
                type: 'at',
                sender,
                time,
                isRead: false,
                objectType: 'post',
                targetPostId,
                targetPostTitle,
                targetPostTitleImage,
                atContent: `@你 测试 @ 提醒 ${i}`,
            } as any);
        }
    }
})();

export const noticeList: notice[] = rawNoticeList.map((n, i) => {
    const { id, ...rest } = n as any;
    return { noticeId: i + 1, ...rest } as notice;
});

// 辅助函数：根据帖子ID获取点赞用户列表
export const getLikeUsersByPostId = (postId: number) => {
    return noticeList.filter(notice =>
        notice.type === 'like' && notice.targetPostId === postId
    ).map(notice => ({
        id: notice.sender.id,
        username: notice.sender.nickname,
        avatar: notice.sender.avatar,
        time: notice.time
    }));
};

type WithPost = Extract<notice, { type: 'like' | 'comment' | 'at' }>;

// 辅助函数：根据帖子ID获取帖子标题
export const getPostTitleById = (postId: number) => {
    const n = noticeList.find(
        (x): x is WithPost =>
            (x.type === 'like' || x.type === 'comment' || x.type === 'at') &&
            x.targetPostId === postId
    );
    return n?.targetPostTitle ?? '';
};