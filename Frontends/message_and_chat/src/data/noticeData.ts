import { type notice } from '../types/notice';

export const noticeList: notice[] = [
  {
    type: 'comment',
    sender: {
      id: 1,
      nickname: 'dev_master',
      avatar: 'https://placehold.co/100x100/34d399/064e3b?text=D',
      url: '/user/dev_master'
    },
    time: '2024-01-15 10:00',
    targetPostId: 123,
    targetPostTitle: '我个人更倾向于 Tailwind CSS,虽然初期学习曲线比较陡峭，但是一旦掌握后开发效率会大大提高。',
    targetPostTitleImage: '',
    commentContent: '这个观点很有道理！',
    commentType: 'post'
  },
  {
    type: 'like',
    sender: {
      id: 2,
      nickname: 'code_lover',
      avatar: 'https://placehold.co/100x100/ec4899/831843?text=C',
      url: '/user/code_lover'
    },
    time: '2024-01-15 09:15',
    targetPostId: 124,
    targetPostTitle: 'Vue 3 的 Composition API 真的很棒，比 Options API 更加灵活和强大！',
    targetPostTitleImage: ''
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
    time: '2024-01-15 09:10',
    targetPostId: 124,
    targetPostTitle: 'Vue 3 的 Composition API 真的很棒，比 Options API 更加灵活和强大！',
    targetPostTitleImage: ''
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
    targetPostId: 124,
    targetPostTitle: 'Vue 3 的 Composition API 真的很棒，比 Options API 更加灵活和强大！',
    targetPostTitleImage: ''
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
    targetPostId: 124,
    targetPostTitle: 'Vue 3 的 Composition API 真的很棒，比 Options API 更加灵活和强大！',
    targetPostTitleImage: ''
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
    targetPostId: 125,
    targetPostTitle: '@你 这个组件写得很好，学习了！希望能多分享一些前端开发经验。',
    targetPostTitleImage: '',
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
    targetPostId: 126,
    targetPostTitle: 'React 和 Vue 各有优势，选择哪个主要看项目需求和团队熟悉度。',
    targetPostTitleImage: '',
    commentContent: '完全同意这个观点！',
    commentType: 'post'
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
    targetPostId: 127,
    targetPostTitle: '设计系统真的很重要，能大大提高开发效率和用户体验的一致性。',
    targetPostTitleImage: ''
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
    targetPostId: 127,
    targetPostTitle: '设计系统真的很重要，能大大提高开发效率和用户体验的一致性。',
    targetPostTitleImage: ''
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
    targetPostId: 127,
    targetPostTitle: '设计系统真的很重要，能大大提高开发效率和用户体验的一致性。',
    targetPostTitleImage: ''
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
    targetPostId: 127,
    targetPostTitle: '设计系统真的很重要，能大大提高开发效率和用户体验的一致性。',
    targetPostTitleImage: ''
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
    targetPostId: 128,
    targetPostTitle: '@你 前端和后端的协作真的很重要，API 设计要考虑到前端的便利性。',
    targetPostTitleImage: '',
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
    targetPostId: 0,
    targetPostTitle: '',
    targetPostTitleImage: ''
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
    targetPostId: 129,
    targetPostTitle: '移动端开发确实有很多挑战，响应式设计和性能优化都很关键。',
    targetPostTitleImage: '',
    commentContent: '移动端确实有很多挑战！',
    commentType: 'post'
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
    targetPostTitleImage: ''
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
    targetPostTitleImage: ''
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
    targetPostTitleImage: ''
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
    targetPostTitleImage: ''
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
    targetPostTitleImage: ''
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
    targetPostTitleImage: ''
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
    targetPostTitleImage: ''
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
    targetPostTitleImage: ''
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
    targetPostTitleImage: ''
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
    targetPostTitleImage: ''
  }
];

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

// 辅助函数：根据帖子ID获取帖子标题
export const getPostTitleById = (postId: number) => {
  const notice = noticeList.find(notice => notice.targetPostId === postId);
  return notice ? notice.targetPostTitle : '';
}; 