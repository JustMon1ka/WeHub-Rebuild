<template>
  <div class="post-list">
    <!-- 加载状态 -->
    <div v-if="loading" class="loading-state">
      <div class="loading-spinner"></div>
      <p>正在加载帖子...</p>
    </div>

    <!-- 错误状态 -->
    <div v-else-if="error" class="error-state">
      <p>{{ error }}</p>
      <button class="btn btn-primary" @click="loadPosts">重试</button>
    </div>

    <!-- 空状态 -->
    <div v-else-if="posts.length === 0" class="empty-posts">
      <div class="empty-icon">📝</div>
      <h3>暂无帖子</h3>
      <p>这个分类下还没有帖子，成为第一个发帖的人吧！</p>
    </div>

    <!-- 帖子列表 -->
    <div v-else>
      <article
        v-for="post in posts"
        :key="post.postId"
        class="post-item"
        @click="handlePostClick(post.postId)"
      >
        <div class="post-vote-section">
          <button class="vote-btn vote-up">
            <svg fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M5 15l7-7 7 7"
              ></path>
            </svg>
          </button>
          <p class="vote-count">{{ post.likes }}</p>
          <button class="vote-btn vote-down">
            <svg fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M19 9l-7 7-7-7"
              ></path>
            </svg>
          </button>
        </div>
        <div class="post-content">
          <a href="#" class="post-title" @click.prevent>{{ post.title }}</a>
          <p class="post-excerpt">{{ post.content }}</p>
          <div class="post-meta">
            <span class="post-author">
              <span class="username">@用户{{ post.userId }}</span> 发布于
              {{ formatTimeAgo(post.createdAt) }}
            </span>
            <span class="post-views">{{ post.views }} 次查看</span>
            <span class="post-likes">{{ post.likes }} 个赞</span>
          </div>
          <!-- 标签 -->
          <div v-if="post.tags && post.tags.length > 0" class="post-tags">
            <span v-for="tag in post.tags" :key="tag" class="tag">
              {{ tag }}
            </span>
          </div>
        </div>
      </article>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { getCirclePosts, getPostsByIds } from '../api'
import { Post } from '../types'

interface Props {
  circleId: number
}

const props = defineProps<Props>()
const router = useRouter()

const posts = ref<Post[]>([])
const loading = ref(true)
const error = ref('')

const loadPosts = async () => {
  try {
    loading.value = true
    error.value = ''

    // 先获取圈子中的帖子ID列表
    const circlePostsResponse = await getCirclePosts(props.circleId)

    if (circlePostsResponse.code === 200 && circlePostsResponse.data.postIds.length > 0) {
      // 根据ID列表获取帖子详情
      const postsResponse = await getPostsByIds(circlePostsResponse.data.postIds)

      if (postsResponse.code === 200) {
        posts.value = postsResponse.data
      } else {
        error.value = postsResponse.msg || '获取帖子详情失败'
      }
    } else {
      posts.value = []
    }
  } catch (err) {
    error.value = '网络错误，请重试'
    console.error('加载帖子失败:', err)
  } finally {
    loading.value = false
  }
}

const handlePostClick = (postId: number) => {
  router.push(`/post/${postId}`)
}

const formatTimeAgo = (dateString: string) => {
  const date = new Date(dateString)
  const now = new Date()
  const diffInMinutes = Math.floor((now.getTime() - date.getTime()) / (1000 * 60))

  if (diffInMinutes < 60) {
    return `${diffInMinutes}分钟前`
  } else if (diffInMinutes < 1440) {
    return `${Math.floor(diffInMinutes / 60)}小时前`
  } else {
    return `${Math.floor(diffInMinutes / 1440)}天前`
  }
}

onMounted(() => {
  loadPosts()
})

// 暴露刷新方法给父组件
defineExpose({
  refresh: loadPosts,
})
</script>

<style scoped>
/* 加载和错误状态 */
.loading-state,
.error-state {
  text-align: center;
  padding: 60px 20px;
  background: #1e293b; /* slate-800 */
  border-radius: 12px;
  border: 1px solid #334155; /* slate-700 */
}

.loading-spinner {
  width: 40px;
  height: 40px;
  border: 3px solid #334155; /* slate-700 */
  border-top: 3px solid #0ea5e9; /* sky-500 */
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin: 0 auto 16px;
}

@keyframes spin {
  0% {
    transform: rotate(0deg);
  }
  100% {
    transform: rotate(360deg);
  }
}

.loading-state p,
.error-state p {
  color: #64748b; /* slate-500 */
  margin-bottom: 16px;
}

.btn {
  padding: 10px 20px;
  border: none;
  border-radius: 6px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s;
  font-size: 14px;
}

.btn-primary {
  background: #0ea5e9; /* sky-500 */
  color: white;
}

.btn-primary:hover {
  background: #0284c7; /* sky-600 */
}

/* 空状态 */
.empty-posts {
  text-align: center;
  padding: 60px 20px;
  color: #64748b; /* slate-500 */
}

.empty-icon {
  font-size: 48px;
  margin-bottom: 16px;
}

.empty-posts h3 {
  color: #f1f5f9; /* slate-100 */
  margin-bottom: 8px;
  font-size: 20px;
}

.empty-posts p {
  margin-bottom: 24px;
  line-height: 1.5;
}

/* 帖子项目样式 */
.post-item {
  padding: 20px 24px;
  display: flex;
  gap: 16px;
  transition: background-color 0.2s;
  cursor: pointer;
  border-bottom: 1px solid #334155; /* slate-700 */
}

.post-item:hover {
  background: #334155; /* slate-700 */
}

.post-item:last-child {
  border-bottom: none;
}

.post-vote-section {
  width: 48px;
  text-align: center;
  flex-shrink: 0;
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 4px;
}

.vote-btn {
  padding: 4px;
  border-radius: 4px;
  background: none;
  border: none;
  color: #64748b; /* slate-500 */
  cursor: pointer;
  transition: all 0.2s;
  width: 32px;
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.vote-btn:hover {
  background: #334155; /* slate-700 */
  color: #38bdf8; /* sky-400 */
}

.vote-btn svg {
  width: 20px;
  height: 20px;
}

.vote-count {
  font-weight: 700;
  font-size: 14px;
  margin: 4px 0;
  color: #f1f5f9; /* slate-100 */
}

.post-content {
  flex: 1;
  min-width: 0;
}

.post-title {
  font-weight: 600;
  font-size: 18px;
  color: #f1f5f9; /* slate-100 */
  text-decoration: none;
  display: block;
  margin-bottom: 8px;
  line-height: 1.4;
}

.post-title:hover {
  color: #38bdf8; /* sky-400 */
}

.post-excerpt {
  margin: 8px 0;
  color: #cbd5e1; /* slate-300 */
  font-size: 14px;
  line-height: 1.5;
  display: -webkit-box;
  -webkit-line-clamp: 3;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.post-meta {
  display: flex;
  align-items: center;
  gap: 16px;
  font-size: 12px;
  color: #64748b; /* slate-500 */
  margin-top: 8px;
  flex-wrap: wrap;
}

.username {
  font-weight: 600;
  color: #38bdf8; /* sky-400 */
}

.post-tags {
  margin-top: 8px;
  display: flex;
  flex-wrap: wrap;
  gap: 4px;
}

.tag {
  background: #0c4a6e; /* sky-900 */
  color: #38bdf8; /* sky-400 */
  font-size: 11px;
  padding: 2px 6px;
  border-radius: 4px;
  border: 1px solid #075985; /* sky-800 */
}

/* 响应式设计 */
@media (max-width: 768px) {
  .post-item {
    flex-direction: column;
    gap: 12px;
    padding: 16px;
  }

  .post-vote-section {
    flex-direction: row;
    width: auto;
    justify-content: flex-start;
  }

  .post-meta {
    flex-direction: column;
    align-items: flex-start;
    gap: 4px;
  }
}
</style>
