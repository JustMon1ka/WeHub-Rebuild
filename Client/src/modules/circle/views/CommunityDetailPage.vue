<template>
  <div>
    <!-- 顶部导航 -->
    <NavBar />

    <!-- 主要内容 -->
    <div class="main-container">
      <!-- 中间主内容区 -->
      <main class="main-content">
        <!-- 加载状态 -->
        <div v-if="loading" class="loading-state">
          <div class="loading-spinner"></div>
          <p>正在加载社区信息...</p>
        </div>

        <!-- 错误状态 -->
        <div v-else-if="error" class="error-state">
          <p>{{ error }}</p>
          <button class="btn btn-primary" @click="loadCommunityData">重试</button>
        </div>

        <!-- 社区内容 -->
        <div v-else>
          <!-- 社区头部 -->
          <div class="community-header-section">
            <!-- 背景横幅 -->
            <div class="community-banner">
              <img
                :src="`https://placehold.co/600x200/1677ff/ffffff?text=${encodeURIComponent(communityData.name)}`"
                :alt="`${communityData.name} banner`"
              />
            </div>

            <!-- 头像和操作按钮 -->
            <div class="community-info-section">
              <div class="community-header-content">
                <img
                  class="community-large-avatar"
                  :src="`https://placehold.co/150x150/1677ff/ffffff?text=${encodeURIComponent(communityData.name[0])}`"
                  :alt="`${communityData.name} avatar`"
                />
                <div class="community-header-actions">
                  <button class="btn btn-primary" @click="handleCreatePost">创建帖子</button>
                  <button class="btn btn-icon" @click="handleNotification">
                    <svg fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path
                        stroke-linecap="round"
                        stroke-linejoin="round"
                        stroke-width="2"
                        d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9"
                      ></path>
                    </svg>
                  </button>
                  <button
                    class="btn"
                    :class="[communityData.isJoined ? 'btn-secondary' : 'btn-primary']"
                    @click="toggleJoinCommunity"
                    :disabled="isLoading"
                  >
                    {{ isLoading ? '处理中...' : communityData.isJoined ? '已加入' : '加入' }}
                  </button>
                </div>
              </div>
              <div class="community-meta">
                <h1 class="community-title">{{ communityData.name }}</h1>
                <p class="community-member-count">
                  {{ formatMemberCount(communityData.memberCount) }} 成员
                </p>
              </div>
            </div>
          </div>

          <!-- 内容切换 Tab -->
          <div class="content-tabs">
            <a
              href="#"
              class="tab-link"
              :class="{ active: activeTab === 'hot' }"
              @click.prevent="changeTab('hot')"
            >
              热门
            </a>
            <a
              href="#"
              class="tab-link"
              :class="{ active: activeTab === 'latest' }"
              @click.prevent="changeTab('latest')"
            >
              最新
            </a>
            <a
              href="#"
              class="tab-link"
              :class="{ active: activeTab === 'featured' }"
              @click.prevent="changeTab('featured')"
            >
              精华
            </a>
          </div>

          <!-- 帖子列表 -->
          <div class="posts-list">
            <!-- 置顶帖子 -->
            <article class="post-item pinned-post">
              <div class="post-vote-section">
                <svg class="pin-icon" fill="currentColor" viewBox="0 0 20 20">
                  <path d="M5 4a2 2 0 012-2h6a2 2 0 012 2v14l-5-3.125L5 18V4z"></path>
                </svg>
              </div>
              <div class="post-content">
                <a href="#" class="post-title"
                  >【版规】{{ communityData.name }}发帖须知 (2024.07更新)</a
                >
                <div class="post-meta">
                  <span class="post-author">版主</span>
                  <span class="post-date">2024-07-01</span>
                </div>
              </div>
            </article>

            <!-- 普通帖子或暂无内容 -->
            <div v-if="filteredPosts.length === 0" class="empty-posts">
              <div class="empty-icon">📝</div>
              <h3>暂无帖子</h3>
              <p>这个分类下还没有帖子，成为第一个发帖的人吧！</p>
              <button class="btn btn-primary" @click="handleCreatePost">创建第一个帖子</button>
            </div>

            <article
              v-else
              v-for="post in filteredPosts"
              :key="post.id"
              class="post-item"
              @click="handlePostClick(post.id)"
            >
              <div class="post-vote-section">
                <button
                  class="vote-btn vote-up"
                  :class="{ active: post.userVote === 'up' }"
                  @click.stop="handleVote(post.id, 'up')"
                >
                  <svg fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path
                      stroke-linecap="round"
                      stroke-linejoin="round"
                      stroke-width="2"
                      d="M5 15l7-7 7 7"
                    ></path>
                  </svg>
                </button>
                <p class="vote-count">{{ formatVoteCount(post.votes) }}</p>
                <button
                  class="vote-btn vote-down"
                  :class="{ active: post.userVote === 'down' }"
                  @click.stop="handleVote(post.id, 'down')"
                >
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
                <p class="post-excerpt">{{ post.excerpt }}</p>
                <div class="post-meta">
                  <span class="post-author">
                    <span class="username">@{{ post.author }}</span> 发布于 {{ post.timeAgo }}
                  </span>
                  <span class="post-replies">{{ post.replies }} 回复</span>
                  <span class="post-last-reply">
                    <span class="username">@{{ post.lastReplyUser }}</span> 回复于
                    {{ post.lastReplyTime }}
                  </span>
                </div>
              </div>
            </article>
          </div>
        </div>
      </main>

      <!-- 右侧边栏 -->
      <aside class="right-sidebar">
        <div class="sidebar-content">
          <!-- 社区信息 -->
          <div class="sidebar-card">
            <h2 class="sidebar-title">关于社区</h2>
            <p class="community-description">{{ communityData.description || '暂无社区描述' }}</p>
            <hr class="sidebar-divider" />
            <div class="community-details">
              <p class="detail-item">
                <svg class="detail-icon" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="2"
                    d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"
                  ></path>
                </svg>
                创建于 {{ formatDate(communityData.createdAt) }}
              </p>
              <p class="detail-item">
                <svg class="detail-icon" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="2"
                    d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
                  ></path>
                </svg>
                {{ communityData.rulesCount || 0 }}条社区规则
              </p>
            </div>
          </div>

          <!-- 版主列表 -->
          <div class="sidebar-card">
            <h2 class="sidebar-title">版主</h2>
            <div v-if="moderators.length === 0" class="empty-moderators">
              <p>暂无版主信息</p>
            </div>
            <ul v-else class="moderator-list">
              <li v-for="moderator in moderators" :key="moderator.id" class="moderator-item">
                <img
                  class="moderator-avatar"
                  :src="moderator.avatar"
                  :alt="`${moderator.name} Avatar`"
                />
                <div class="moderator-info">
                  <p class="moderator-name">{{ moderator.name }}</p>
                  <p class="moderator-handle">@{{ moderator.handle }}</p>
                </div>
              </li>
            </ul>
          </div>
        </div>
      </aside>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import NavBar from '../components/NavBar.vue'
import { CircleAPI } from '../api.ts'
import type { Community } from '@/types/community'

// 类型定义
interface Post {
  id: number
  title: string
  excerpt: string
  author: string
  timeAgo: string
  replies: number
  lastReplyUser: string
  lastReplyTime: string
  votes: number
  userVote?: 'up' | 'down' | null
  category: 'hot' | 'latest' | 'featured'
}

interface CommunityData {
  id: number
  name: string
  description: string
  memberCount: number
  isJoined: boolean
  createdAt: string
  rulesCount: number
  category?: string
  isPrivate?: boolean
}

interface Moderator {
  id: number
  name: string
  handle: string
  avatar: string
}

// 路由和状态
const route = useRoute()
const router = useRouter()
const activeTab = ref<'hot' | 'latest' | 'featured'>('hot')
const isLoading = ref(false)
const loading = ref(true)
const error = ref<string | null>(null)

// 社区数据
const communityData = ref<CommunityData>({
  id: 0,
  name: '',
  description: '',
  memberCount: 0,
  isJoined: false,
  createdAt: '',
  rulesCount: 0,
})

// 帖子数据（模拟数据，可以后续连接后端）
const posts = ref<Post[]>([
  {
    id: 1,
    title: '这个社区的第一个帖子！',
    excerpt: '欢迎大家来到这个社区，让我们一起分享和讨论感兴趣的话题吧！',
    author: 'community_admin',
    timeAgo: '2小时前',
    replies: 25,
    lastReplyUser: 'active_user',
    lastReplyTime: '1小时前',
    votes: 45,
    userVote: null,
    category: 'hot',
  },
])

// 版主数据
const moderators = ref<Moderator[]>([
  {
    id: 1,
    name: '社区管理员',
    handle: 'admin',
    avatar: 'https://placehold.co/100x100/1677ff/ffffff?text=M1',
  },
])

// 计算属性
const filteredPosts = computed(() => {
  return posts.value.filter((post) => post.category === activeTab.value)
})

// 加载社区数据
const loadCommunityData = async (): Promise<void> => {
  try {
    loading.value = true
    error.value = null

    const communityId = route.params.id as string
    console.log('=== 开始加载社区数据 ===')
    console.log('社区ID:', communityId)

    if (!communityId || isNaN(Number(communityId))) {
      throw new Error('社区ID无效')
    }

    // 获取社区详情
    const response = await CircleAPI.getCircleDetails(Number(communityId))
    console.log('=== API响应 ===')
    console.log('完整响应:', response)

    // 检查响应状态
    if (!response) {
      throw new Error('服务器无响应')
    }

    if (!response.success || !response.data) {
      throw new Error(response.message || '获取社区信息失败')
    }

    // 根据实际的响应结构解析数据（注意是小写的 circle 和 members）
    const circleInfo = response.data.circle
    const membersInfo = response.data.members || []

    console.log('解析的圈子信息:', circleInfo)
    console.log('解析的成员信息:', membersInfo)

    if (!circleInfo) {
      throw new Error('未找到社区信息')
    }

    // 设置社区数据
    communityData.value = {
      id: circleInfo.circleId,
      name: circleInfo.name,
      description: circleInfo.description || '暂无描述',
      memberCount: circleInfo.memberCount || 0,
      isJoined: false, // 将在下面检查
      createdAt: circleInfo.createdAt,
      rulesCount: 0, // 后端暂不支持
      category: circleInfo.category || '通用', // 默认分类
      isPrivate: circleInfo.isPrivate || false, // 默认公开
    }

    // 检查用户是否已加入
    const currentUserId = 2 // 与后端硬编码保持一致
    if (Array.isArray(membersInfo) && membersInfo.length > 0) {
      communityData.value.isJoined = membersInfo.some(
        (member: any) => member.userId === currentUserId,
      )

      // 设置版主信息（圈主）
      const ownerId = circleInfo.ownerId
      if (ownerId) {
        const owner = membersInfo.find((member: any) => member.userId === ownerId)
        if (owner) {
          moderators.value = [
            {
              id: owner.userId,
              name: owner.name || `用户${owner.userId}`,
              handle: `user${owner.userId}`,
              avatar: `https://placehold.co/100x100/1677ff/ffffff?text=U${owner.userId}`,
            },
          ]
        } else {
          // 圈主不在成员列表中，创建默认版主信息
          moderators.value = [
            {
              id: ownerId,
              name: `用户${ownerId}`,
              handle: `user${ownerId}`,
              avatar: `https://placehold.co/100x100/1677ff/ffffff?text=U${ownerId}`,
            },
          ]
        }
      }
    } else {
      // 没有成员数据，尝试检查成员状态
      try {
        communityData.value.isJoined = await CircleAPI.checkMembership(Number(communityId))
      } catch (error) {
        console.error('检查成员状态失败:', error)
        communityData.value.isJoined = false
      }

      // 设置默认版主信息（圈主）
      if (circleInfo.ownerId) {
        moderators.value = [
          {
            id: circleInfo.ownerId,
            name: `用户${circleInfo.ownerId}`,
            handle: `user${circleInfo.ownerId}`,
            avatar: `https://placehold.co/100x100/1677ff/ffffff?text=U${circleInfo.ownerId}`,
          },
        ]
      }
    }

    console.log('最终的社区数据:', communityData.value)
    console.log('版主信息:', moderators.value)
  } catch (err) {
    console.error('=== 加载社区数据失败 ===')
    console.error('错误详情:', err)

    let errorMessage = '加载社区信息失败，请稍后重试'

    if (err instanceof Error) {
      if (err.message.includes('社区不存在') || err.message.includes('404')) {
        errorMessage = '社区不存在，可能已被删除或ID错误'
      } else if (err.message.includes('Failed to fetch')) {
        errorMessage = '无法连接到服务器，请检查网络连接'
      } else {
        errorMessage = err.message
      }
    }

    error.value = errorMessage
  } finally {
    loading.value = false
  }
}

// 检查成员状态
const checkMembershipStatus = async (communityId: number): Promise<void> => {
  try {
    const isMember = await CircleAPI.checkMembership(communityId)
    communityData.value.isJoined = isMember
  } catch (error) {
    console.error('检查成员状态失败:', error)
    // 如果检查失败，默认为未加入
    communityData.value.isJoined = false
  }
}

// 方法
const changeTab = (tab: 'hot' | 'latest' | 'featured'): void => {
  activeTab.value = tab
}

const formatMemberCount = (count: number): string => {
  if (count >= 10000) {
    return (count / 10000).toFixed(1) + '万'
  }
  return count.toLocaleString()
}

const formatVoteCount = (count: number): string => {
  if (count >= 1000) {
    return (count / 1000).toFixed(1) + 'k'
  }
  return count.toString()
}

const formatDate = (dateString: string): string => {
  if (!dateString) return '未知'
  const date = new Date(dateString)
  const year = date.getFullYear()
  const month = date.getMonth() + 1
  return `${year}年${month}月`
}

const handleCreatePost = (): void => {
  console.log('创建帖子')
}

const handleNotification = (): void => {
  console.log('通知')
}

const toggleJoinCommunity = async (): Promise<void> => {
  try {
    isLoading.value = true

    let response
    if (communityData.value.isJoined) {
      response = await CircleAPI.leaveCircle(communityData.value.id)
    } else {
      response = await CircleAPI.joinCircle(communityData.value.id)
    }

    if (response && response.success) {
      communityData.value.isJoined = !communityData.value.isJoined

      if (communityData.value.isJoined) {
        communityData.value.memberCount += 1
      } else {
        communityData.value.memberCount = Math.max(communityData.value.memberCount - 1, 0)
      }
    }

    console.log(`${communityData.value.isJoined ? '加入' : '退出'}社区成功`)
  } catch (error) {
    console.error('操作失败:', error)
  } finally {
    isLoading.value = false
  }
}

const handleVote = async (postId: number, voteType: 'up' | 'down'): Promise<void> => {
  try {
    const post = posts.value.find((p) => p.id === postId)
    if (!post) return

    const previousVote = post.userVote

    if (post.userVote === voteType) {
      post.userVote = null
      post.votes += voteType === 'up' ? -1 : 1
    } else {
      if (previousVote) {
        post.votes += previousVote === 'up' ? -1 : 1
      }

      post.userVote = voteType
      post.votes += voteType === 'up' ? 1 : -1
    }

    console.log(`投票 ${voteType} 帖子 ${postId}`)
  } catch (error) {
    console.error('投票失败:', error)
  }
}

const handlePostClick = (postId: number): void => {
  console.log(`点击帖子 ${postId}`)
}

// 监听路由变化
watch(
  () => route.params.id,
  (newId) => {
    if (newId) {
      loadCommunityData()
    }
  },
)

// 生命周期
onMounted(() => {
  loadCommunityData()
})
</script>

<style scoped>
.main-container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 24px 20px;
  display: grid;
  grid-template-columns: 1fr 300px;
  gap: 24px;
}

/* 加载和错误状态 */
.loading-state,
.error-state {
  text-align: center;
  padding: 60px 20px;
  background: #fff;
  border-radius: 12px;
  border: 1px solid #e4e6ea;
}

.loading-spinner {
  width: 40px;
  height: 40px;
  border: 3px solid #f3f3f3;
  border-top: 3px solid #1677ff;
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
  color: #86909c;
  margin-bottom: 16px;
}

/* 主内容区 */
.main-content {
  background: #fff;
  border-radius: 12px;
  border: 1px solid #e4e6ea;
  overflow: hidden;
}

.community-header-section {
  position: relative;
}

.community-banner {
  height: 192px;
  background: #f7f8fa;
  overflow: hidden;
}

.community-banner img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.community-info-section {
  padding: 0 24px;
  background: #fff;
}

.community-header-content {
  display: flex;
  align-items: flex-end;
  margin-top: -48px;
  position: relative;
}

.community-large-avatar {
  width: 96px;
  height: 96px;
  border-radius: 50%;
  border: 4px solid #fff;
  background: #f7f8fa;
}

.community-header-actions {
  margin-left: auto;
  display: flex;
  align-items: center;
  gap: 8px;
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

.btn:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.btn-primary {
  background: #1677ff;
  color: white;
}

.btn-primary:hover:not(:disabled) {
  background: #0958d9;
}

.btn-secondary {
  border: 1px solid #d9d9d9;
  background: #fff;
  color: #4e5969;
}

.btn-secondary:hover:not(:disabled) {
  border-color: #1677ff;
  color: #1677ff;
}

.btn-icon {
  border: 1px solid #d9d9d9;
  background: #fff;
  color: #4e5969;
  padding: 10px;
  width: 40px;
  height: 40px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.btn-icon:hover:not(:disabled) {
  border-color: #1677ff;
  color: #1677ff;
}

.btn-icon svg {
  width: 20px;
  height: 20px;
}

.community-meta {
  margin-top: 16px;
  padding-bottom: 24px;
}

.community-title {
  font-size: 24px;
  font-weight: 700;
  margin: 0 0 4px 0;
  color: #1d2129;
}

.community-member-count {
  font-size: 14px;
  color: #86909c;
  margin: 0;
}

.content-tabs {
  display: flex;
  border-bottom: 1px solid #e4e6ea;
}

.tab-link {
  flex: 1;
  text-align: center;
  padding: 16px;
  color: #86909c;
  text-decoration: none;
  border-bottom: 2px solid transparent;
  font-weight: 500;
  transition: all 0.2s;
}

.tab-link.active {
  color: #1677ff;
  border-bottom-color: #1677ff;
}

.tab-link:hover {
  color: #1677ff;
  background: #f7f8fa;
}

.posts-list {
  min-height: 400px;
}

.empty-posts {
  text-align: center;
  padding: 60px 20px;
  color: #86909c;
}

.empty-icon {
  font-size: 48px;
  margin-bottom: 16px;
}

.empty-posts h3 {
  color: #1d2129;
  margin-bottom: 8px;
  font-size: 20px;
}

.empty-posts p {
  margin-bottom: 24px;
  line-height: 1.5;
}

.post-item {
  padding: 20px 24px;
  display: flex;
  gap: 16px;
  transition: background-color 0.2s;
  cursor: pointer;
  border-bottom: 1px solid #f2f3f5;
}

.post-item:hover {
  background: #f7f8fa;
}

.post-item:last-child {
  border-bottom: none;
}

.pinned-post {
  background: #fff7e6;
  border-bottom: 1px solid #ffe58f;
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

.pin-icon {
  width: 24px;
  height: 24px;
  color: #d48806;
  margin: 0 auto;
}

.vote-btn {
  padding: 4px;
  border-radius: 4px;
  background: none;
  border: none;
  color: #86909c;
  cursor: pointer;
  transition: all 0.2s;
  width: 32px;
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.vote-btn:hover {
  background: #f2f3f5;
  color: #1677ff;
}

.vote-btn.active.vote-up {
  color: #1677ff;
  background: #f0f8ff;
}

.vote-btn.active.vote-down {
  color: #ff4d4f;
  background: #fff2f0;
}

.vote-btn svg {
  width: 20px;
  height: 20px;
}

.vote-count {
  font-weight: 700;
  font-size: 14px;
  margin: 4px 0;
  color: #1d2129;
}

.post-content {
  flex: 1;
  min-width: 0;
}

.post-title {
  font-weight: 600;
  font-size: 18px;
  color: #1d2129;
  text-decoration: none;
  display: block;
  margin-bottom: 8px;
  line-height: 1.4;
}

.post-title:hover {
  color: #1677ff;
}

.post-excerpt {
  margin: 8px 0;
  color: #4e5969;
  font-size: 14px;
  line-height: 1.5;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.post-meta {
  display: flex;
  align-items: center;
  gap: 16px;
  font-size: 12px;
  color: #86909c;
  margin-top: 8px;
  flex-wrap: wrap;
}

.username {
  font-weight: 600;
  color: #1677ff;
}

/* 右侧边栏 */
.right-sidebar {
  display: flex;
  flex-direction: column;
  gap: 24px;
}

.sidebar-card {
  background: #fff;
  border-radius: 12px;
  padding: 20px;
  border: 1px solid #e4e6ea;
}

.sidebar-title {
  font-size: 18px;
  font-weight: 600;
  color: #1d2129;
  margin-bottom: 16px;
}

.community-description {
  font-size: 14px;
  color: #4e5969;
  line-height: 1.5;
  margin-bottom: 0;
}

.sidebar-divider {
  border: none;
  height: 1px;
  background: #e4e6ea;
  margin: 16px 0;
}

.community-details {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.detail-item {
  display: flex;
  align-items: center;
  font-size: 14px;
  margin: 0;
  color: #4e5969;
}

.detail-icon {
  width: 20px;
  height: 20px;
  margin-right: 8px;
  color: #86909c;
}

.empty-moderators {
  text-align: center;
  color: #86909c;
  font-size: 14px;
}

.moderator-list {
  list-style: none;
  padding: 0;
  margin: 0;
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.moderator-item {
  display: flex;
  align-items: center;
}

.moderator-avatar {
  width: 40px;
  height: 40px;
  border-radius: 50%;
}

.moderator-info {
  margin-left: 12px;
  flex: 1;
}

.moderator-name {
  font-weight: 600;
  font-size: 14px;
  margin: 0;
  color: #1d2129;
}

.moderator-handle {
  color: #86909c;
  font-size: 12px;
  margin: 0;
}

/* 响应式设计 */
@media (max-width: 1024px) {
  .main-container {
    grid-template-columns: 1fr;
  }
}

@media (max-width: 768px) {
  .main-container {
    padding: 16px;
  }

  .community-header-content {
    flex-direction: column;
    align-items: flex-start;
    margin-top: -48px;
  }

  .community-header-actions {
    margin-left: 0;
    margin-top: 16px;
    width: 100%;
    justify-content: flex-end;
  }

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
}
</style>
