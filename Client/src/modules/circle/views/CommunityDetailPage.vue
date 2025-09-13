<template>
  <div class="overflow-auto h-dvh">
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
                :src="
                  processedBannerUrl || 'https://placehold.co/800x192/f0f2f5/86909c?text=暂无横幅'
                "
                alt="社区横幅"
              />
            </div>
            <!-- 头像和操作按钮 -->
            <div class="community-info-section">
              <div class="community-header-content">
                <img
                  class="community-large-avatar"
                  :src="
                    processedAvatarUrl ||
                    `https://placehold.co/150x150/1677ff/ffffff?text=${encodeURIComponent(communityData.name[0] || 'C')}`
                  "
                  :alt="`${communityData.name} avatar`"
                />
                <div class="community-header-actions">
                  <button class="btn btn-primary" @click="handleCreatePost">创建帖子</button>
                  <!-- 创建活动按钮 -->
                  <button
                    v-if="canManageActivities"
                    class="btn btn-primary"
                    @click="showCreateActivity = true"
                  >
                    创建活动
                  </button>
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
                    :class="[
                      communityStore.getCommunityJoinStatus(communityData.id)
                        ? 'btn-secondary'
                        : 'btn-primary',
                    ]"
                    @click="toggleJoinCommunity"
                    :disabled="communityStore.getCommunityLoadingState(communityData.id)"
                  >
                    {{
                      communityStore.getCommunityLoadingState(communityData.id)
                        ? '处理中...'
                        : communityStore.getCommunityJoinStatus(communityData.id)
                          ? '退出'
                          : '加入'
                    }}
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
            <!-- 新增活动 tab -->
            <a
              href="#"
              class="tab-link"
              :class="{ active: activeTab === 'activities' }"
              @click.prevent="changeTab('activities')"
            >
              活动
            </a>
          </div>

          <!-- 帖子列表 -->
          <div v-if="activeTab !== 'activities'" class="posts-list">
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

          <!-- 活动列表 -->
          <div v-if="activeTab === 'activities'" class="activities-container">
            <ActivityList
              ref="activityListRef"
              :circle-id="communityData.id"
              :can-create-activity="canManageActivities"
              @stats-updated="handleActivityStatsUpdated"
            />
          </div>
        </div>
      </main>

      <!-- 右侧边栏 -->
      <aside class="right-sidebar">
        <div class="sidebar-content space-y-6">
          <!-- 社区信息 -->
          <div class="sidebar-card">
            <h2 class="sidebar-title">关于社区</h2>
            <p class="community-description">{{ communityData.description || '暂无社区描述' }}</p>
            <hr class="sidebar-divider" />
            <div class="community-details">
              <p class="detail-item" v-if="communityData.category">
                <svg class="detail-icon" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="2"
                    d="M7 7h.01M7 3h5c.512 0 1.024.195 1.414.586l7 7a2 2 0 010 2.828l-7 7a2 2 0 01-2.828 0l-7-7A1.994 1.994 0 013 12V7a4 4 0 014-4z"
                  ></path>
                </svg>
                分类：{{ communityData.category }}
              </p>
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
          <!-- 活动统计卡片 -->
          <div v-if="activeTab === 'activities'" class="sidebar-card">
            <h2 class="sidebar-title">活动统计</h2>
            <div class="activity-stats">
              <div
                class="stat-item clickable"
                @click="handleStatClick('all')"
                :class="{ active: currentActivityFilter === 'all' }"
              >
                <span class="stat-number">{{ activityStats.total }}</span>
                <span class="stat-label">总活动数</span>
              </div>
              <div
                class="stat-item clickable"
                @click="handleStatClick('active')"
                :class="{ active: currentActivityFilter === 'active' }"
              >
                <span class="stat-number">{{ activityStats.active }}</span>
                <span class="stat-label">进行中</span>
              </div>
              <div
                class="stat-item clickable"
                @click="handleStatClick('participated')"
                :class="{ active: currentActivityFilter === 'participated' }"
              >
                <span class="stat-number">{{ activityStats.participated }}</span>
                <span class="stat-label">我参与的</span>
              </div>
            </div>
          </div>
        </div>
      </aside>
    </div>
    <!-- 创建活动弹窗 -->
    <CreateActivity
      v-if="showCreateActivity"
      :circle-id="communityData.id"
      @close="showCreateActivity = false"
      @saved="handleActivityCreated"
    />
  </div>
  <!-- 创建帖子弹窗 -->
  <CreatePost
    v-if="showCreatePost"
    :circle-id="communityData.id"
    :community-name="communityData.name"
    :community-avatar="processedAvatarUrl"
    @close="showCreatePost = false"
    @submitted="handlePostCreated"
  />
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { CircleAPI, getSpuPage, getProxiedImageUrl } from '../api.ts'
import { useCommunityStore } from '../store.ts'
import ActivityList from '../components/ActivityList.vue'
import CreateActivity from '../components/CreateActivity.vue'
import CreatePost from '../components/CreatePost.vue'
import { PostAPI } from '../api'

const imageSrc = ref<string>('')

const fetchImage = async () => {
  const url =
    'http://120.26.118.70:5001/api/preview/big/uploads/circles/81/avatar_20250901142455_1eXD0VQr.png?inline=true&key=1756736695790'
  imageSrc.value = await getSpuPage(encodeURIComponent(url))
}

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
  avatarUrl?: string
  bannerUrl?: string
}

interface Moderator {
  id: number
  name: string
  handle: string
  avatar: string
}

interface ActivityStats {
  total: number
  active: number
  participated: number
}

// 路由和状态
const route = useRoute()
const router = useRouter()
const communityStore = useCommunityStore()
const activeTab = ref<'hot' | 'latest' | 'featured' | 'activities'>('hot')
const isLoading = ref(false)
const loading = ref(true)
const error = ref<string | null>(null)

// 发帖相关状态
const showCreatePost = ref(false)
const communityPosts = ref<any[]>([])
const postsLoading = ref(false)

// 新增活动相关状态
const showCreateActivity = ref(false)
const activityStats = ref<ActivityStats>({
  total: 0,
  active: 0,
  participated: 0,
})

// 添加图片URL状态管理
const processedAvatarUrl = ref<string>('')
const processedBannerUrl = ref<string>('')

// 获取带认证的图片 - 这里就是使用 request.get() 的地方
const getAuthenticatedImageUrl = async (imageUrl: string): Promise<string> => {
  if (!imageUrl) return ''

  try {
    // 提取相对路径部分（去掉域名）
    const imagePath = imageUrl.replace('http://120.26.118.70:5001', '')

    // 这里就是 request.get() 的使用！自动携带Cookie
    const response = await request.get(imagePath, {
      responseType: 'blob',
    })

    // 将返回的blob转换为可显示的URL
    return URL.createObjectURL(response.data)
  } catch (error) {
    return ''
  }
}

//  processImageUrls 函数
const processImageUrls = async (): Promise<void> => {
  // 处理头像
  if (communityData.value.avatarUrl) {
    processedAvatarUrl.value = await getProxiedImageUrl(communityData.value.avatarUrl)
  }

  // 处理横幅
  if (communityData.value.bannerUrl) {
    processedBannerUrl.value = await getProxiedImageUrl(communityData.value.bannerUrl)
  }
}

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

// 计算用户是否可以管理活动（圈主或管理员）
const canManageActivities = computed(() => {
  const currentUserId = 2 // 与后端硬编码保持一致
  return (
    communityData.value.ownerId === currentUserId ||
    moderators.value.some((mod) => mod.id === currentUserId)
  )
})

// 加载社区数据
const loadCommunityData = async (): Promise<void> => {
  try {
    loading.value = true
    error.value = null
    const communityId = route.params.id as string

    if (!communityId || isNaN(Number(communityId))) {
      throw new Error('社区ID无效')
    }

    // 获取社区详情
    const response = await CircleAPI.getCircleDetails(Number(communityId))

    if (!response) {
      throw new Error('服务器无响应')
    }

    if (!response.success || !response.data) {
      throw new Error(response.message || '获取社区信息失败')
    }

    // 根据实际的响应结构解析数据
    const circleInfo = response.data.circle
    const membersInfo = response.data.members || []

    if (!circleInfo) {
      throw new Error('未找到社区信息')
    }

    // 直接使用后端传来的URL，不做任何处理
    const avatarUrl = circleInfo.avatarUrl || circleInfo.avatar_url || circleInfo.AVATAR_URL
    const bannerUrl = circleInfo.bannerUrl || circleInfo.banner_url || circleInfo.BANNER_URL

    communityData.value = {
      id: circleInfo.circleId,
      name: circleInfo.name,
      description: circleInfo.description || '暂无描述',
      memberCount: circleInfo.memberCount || 0,
      isJoined: false,
      createdAt: circleInfo.createdAt,
      rulesCount: 0,
      category: circleInfo.categories || circleInfo.category || '通用',
      isPrivate: circleInfo.isPrivate || false,
      avatarUrl: avatarUrl,
      bannerUrl: bannerUrl,
    }

    // 检查用户是否已加入
    const currentUserId = 2 // 与后端硬编码保持一致
    if (Array.isArray(membersInfo) && membersInfo.length > 0) {
      const isJoined = membersInfo.some((member: any) => member.userId === currentUserId)
      communityData.value.isJoined = isJoined
      // 同步到store
      communityStore.updateCommunity(communityData.value.id, { isJoined })

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
        const isJoined = await CircleAPI.checkMembership(Number(communityId))
        communityData.value.isJoined = isJoined
        // 同步到store
        communityStore.updateCommunity(communityData.value.id, { isJoined })
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

    // 图片处理
    await processImageUrls()
  } catch (err) {
    console.error('加载社区数据失败:', err)

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

// 方法
const changeTab = (tab: 'hot' | 'latest' | 'featured' | 'activities'): void => {
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
  showCreatePost.value = true
}

// 处理帖子创建完成
const handlePostCreated = async (post: any): Promise<void> => {
  showCreatePost.value = false

  // 重新加载帖子列表
  await loadCommunityPosts()
}

// 加载社区帖子
const loadCommunityPosts = async (): Promise<void> => {
  if (!communityData.value.id) return

  try {
    postsLoading.value = true
    const response = await PostAPI.getPostsByCircle(communityData.value.id, {
      num: 20,
    })

    if (response.success) {
      communityPosts.value = response.data || []
    }
  } catch (error) {
    console.error('加载帖子失败:', error)
  } finally {
    postsLoading.value = false
  }
}

const handleNotification = (): void => {
  console.log('通知')
}

const toggleJoinCommunity = async (): Promise<void> => {
  try {
    const result = await communityStore.toggleCommunityMembership(communityData.value.id)

    if (result && result.success) {
      // 更新本地数据
      const isJoined = communityStore.getCommunityJoinStatus(communityData.value.id)
      communityData.value.isJoined = isJoined

      if (isJoined) {
        communityData.value.memberCount += 1
      } else {
        communityData.value.memberCount = Math.max(communityData.value.memberCount - 1, 0)
      }
    }
  } catch (error) {
    console.error('操作失败:', error)
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
  } catch (error) {
    console.error('投票失败:', error)
  }
}

const handlePostClick = (postId: number): void => {
  console.log(`点击帖子 ${postId}`)
}

// 新增活动相关方法
const handleActivityCreated = (activity: any): void => {
  showCreateActivity.value = false
  // 刷新活动统计
  loadActivityStats()
}

// 加载活动统计数据
const loadActivityStats = async (): Promise<void> => {
  try {
    // 这里可以调用获取活动统计的API
    // 暂时使用模拟数据
    activityStats.value = {
      total: 5,
      active: 2,
      participated: 3,
    }
  } catch (error) {
    console.error('获取活动统计失败:', error)
  }
}

// 添加 ActivityList 引用
const activityListRef = ref()

// 添加当前活动筛选状态
const currentActivityFilter = ref<'all' | 'active' | 'participated'>('all')

// 修改 activityStats 的更新逻辑
const handleActivityStatsUpdated = (stats: ActivityStats) => {
  activityStats.value = stats
}

// 处理统计点击
const handleStatClick = (filter: 'all' | 'active' | 'participated') => {
  currentActivityFilter.value = filter

  // 调用 ActivityList 组件的筛选方法
  if (activityListRef.value) {
    activityListRef.value.setActiveFilter(filter)
  }
}

// 监听活动tab切换，重置筛选状态
watch(
  () => activeTab.value,
  (newTab) => {
    if (newTab === 'activities') {
      currentActivityFilter.value = 'all'
      if (activityListRef.value) {
        activityListRef.value.setActiveFilter('all')
      }
    }
  },
)

// 监听路由变化
watch(
  () => route.params.id,
  (newId) => {
    if (newId) {
      loadCommunityData()
    }
  },
)

// 监听活动标签页切换，加载统计数据
watch(activeTab, (newTab) => {
  if (newTab === 'activities') {
    loadActivityStats()
  }
})

// 监听社区数据变化，加载帖子
watch(
  () => communityData.value.id,
  (newId) => {
    if (newId) {
      loadCommunityPosts()
    }
  },
)

// 生命周期
onMounted(() => {
  loadCommunityData()
  loadActivityStats()
  fetchImage()
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
  background: #0f172a; /* slate-900 */
}

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

/* 主内容区 */
.main-content {
  background: #1e293b; /* slate-800 */
  border-radius: 12px;
  border: 1px solid #334155; /* slate-700 */
}

.community-header-section {
  position: relative;
}

.community-banner {
  height: 192px;
  background: #334155; /* slate-700 */
}

.community-banner img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.community-info-section {
  padding: 0 24px;
  background: #1e293b; /* slate-800 */
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
  border: 4px solid #1e293b; /* slate-800 */
  background: #334155; /* slate-700 */
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
  background: #0ea5e9; /* sky-500 */
  color: white;
}

.btn-primary:hover:not(:disabled) {
  background: #0284c7; /* sky-600 */
}

.btn-secondary {
  border: 1px solid #475569; /* slate-600 */
  background: #1e293b; /* slate-800 */
  color: #cbd5e1; /* slate-300 */
}

.btn-secondary:hover:not(:disabled) {
  border-color: #0ea5e9; /* sky-500 */
  color: #0ea5e9; /* sky-500 */
}

.btn-icon {
  border: 1px solid #475569; /* slate-600 */
  background: #1e293b; /* slate-800 */
  color: #cbd5e1; /* slate-300 */
  padding: 10px;
  width: 40px;
  height: 40px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.btn-icon:hover:not(:disabled) {
  border-color: #0ea5e9; /* sky-500 */
  color: #0ea5e9; /* sky-500 */
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
  color: #f1f5f9; /* slate-100 */
}

.community-member-count {
  font-size: 14px;
  color: #64748b; /* slate-500 */
  margin: 0;
}

.content-tabs {
  display: flex;
  border-bottom: 1px solid #334155; /* slate-700 */
}

.tab-link {
  flex: 1;
  text-align: center;
  padding: 16px;
  color: #64748b; /* slate-500 */
  text-decoration: none;
  border-bottom: 2px solid transparent;
  font-weight: 500;
  transition: all 0.2s;
}

.tab-link.active {
  color: #38bdf8; /* sky-400 */
  border-bottom-color: #38bdf8; /* sky-400 */
}

.tab-link:hover {
  color: #38bdf8; /* sky-400 */
  background: #334155; /* slate-700 */
}

.posts-list {
  min-height: 400px;
}

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

.pinned-post {
  background: #1f2937; /* gray-800 */
  border-bottom: 1px solid #374151; /* gray-700 */
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
  color: #fbbf24; /* amber-400 */
  margin: 0 auto;
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

.vote-btn.active.vote-up {
  color: #38bdf8; /* sky-400 */
  background: #0c4a6e; /* sky-900 */
}

.vote-btn.active.vote-down {
  color: #f87171; /* red-400 */
  background: #7f1d1d; /* red-900 */
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
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
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

/* 右侧边栏 */
.right-sidebar {
  display: flex;
  flex-direction: column;
  gap: 24px;
}

.sidebar-card {
  background: #1e293b; /* slate-800 */
  border-radius: 12px;
  padding: 20px;
  border: 1px solid #334155; /* slate-700 */
}

.sidebar-title {
  font-size: 18px;
  font-weight: 600;
  color: #f1f5f9; /* slate-100 */
  margin-bottom: 16px;
}

.community-description {
  font-size: 14px;
  color: #cbd5e1; /* slate-300 */
  line-height: 1.5;
  margin-bottom: 0;
}

.sidebar-divider {
  border: none;
  height: 1px;
  background: #334155; /* slate-700 */
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
  margin: 8px 0;
  color: #cbd5e1; /* slate-300 */
}

.detail-icon {
  width: 20px;
  height: 20px;
  margin-right: 8px;
  color: #64748b; /* slate-500 */
  flex-shrink: 0;
}

.empty-moderators {
  text-align: center;
  color: #64748b; /* slate-500 */
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
  color: #f1f5f9; /* slate-100 */
}

.moderator-handle {
  color: #64748b; /* slate-500 */
  font-size: 12px;
  margin: 0;
}

/* 新增：活动统计样式 */
.activity-stats {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 16px;
  text-align: center;
}

.stat-item {
  display: flex;
  flex-direction: column;
  gap: 4px;
}

.stat-number {
  font-size: 24px;
  font-weight: 700;
  color: #38bdf8; /* sky-400 */
}

.stat-label {
  font-size: 12px;
  color: #64748b; /* slate-500 */
}

.stat-item.clickable {
  cursor: pointer;
  transition: all 0.2s ease;
  padding: 8px;
  border-radius: 8px;
}

.stat-item.clickable:hover {
  background: #0c4a6e; /* sky-900 */
  transform: translateY(-2px);
  box-shadow: 0 2px 8px rgba(56, 189, 248, 0.1);
}

.stat-item.clickable.active {
  background: #0ea5e9; /* sky-500 */
  color: #fff;
}

.stat-item.clickable.active .stat-number,
.stat-item.clickable.active .stat-label {
  color: #fff;
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

  .activities-container {
    padding: 24px;
    min-height: 400px;
  }
}
</style>
