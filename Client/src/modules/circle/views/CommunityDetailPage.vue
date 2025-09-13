<template>
  <div>
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
                    `https://placehold.co/150x150/1677ff/ffffff?text=${encodeURIComponent(
                      communityData.name[0] || 'C',
                    )}`
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
                  <div class="join-actions">
                    <button
                      v-if="!membershipStatus.isJoined"
                      class="btn btn-primary"
                      @click="handleJoinCommunity"
                      :disabled="membershipStatus.loading"
                    >
                      {{ membershipStatus.loading ? '处理中...' : '加入' }}
                    </button>
                    <!-- 修改：已加入成员显示退出按钮 -->
                    <button
                      v-if="membershipStatus.isJoined"
                      class="btn btn-danger"
                      @click="handleLeaveCommunity"
                      :disabled="membershipStatus.loading"
                    >
                      {{ membershipStatus.loading ? '处理中...' : '退出' }}
                    </button>
                    <!-- 审核按钮 -->
                    <button
                      v-if="isOwner"
                      class="btn btn-secondary"
                      @click="showApplicationModal = true"
                    >
                      审核
                    </button>
                  </div>
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
          <div v-if="activeTab !== 'activities'" class="posts-list scrollable-content">
            <!-- 真实帖子列表 -->
            <PostList :circleId="communityData.id" ref="postListRef" />
          </div>

          <!-- 活动列表 -->

          <div v-if="activeTab === 'activities'" class="activities-container scrollable-content">
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
        <div class="sidebar-content">
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

    <!-- 审核申请弹窗 -->
    <div v-if="showApplicationModal" class="modal-overlay" @click="closeApplicationModal">
      <div class="modal-content" @click.stop>
        <div class="modal-header">
          <h3>审核申请</h3>
          <button class="modal-close" @click="closeApplicationModal">
            <svg fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M6 18L18 6M6 6l12 12"
              ></path>
            </svg>
          </button>
        </div>
        <div class="modal-body">
          <div v-if="applicationsLoading" class="loading-state">
            <div class="loading-spinner"></div>
            <p>正在加载申请列表...</p>
          </div>
          <div v-else-if="applications.length === 0" class="empty-applications">
            <p>暂无待审核申请</p>
          </div>
          <div v-else class="applications-list">
            <div
              v-for="application in applications"
              :key="application.userId"
              class="application-item"
            >
              <div class="application-info">
                <div class="user-info">
                  <img
                    class="user-avatar"
                    :src="`https://placehold.co/40x40/1677ff/ffffff?text=U${application.userId}`"
                    :alt="`用户${application.userId}`"
                  />
                  <div class="user-details">
                    <p class="user-id">用户ID: {{ application.userId }}</p>
                    <p class="apply-time">申请时间: {{ formatDateTime(application.applyTime) }}</p>
                  </div>
                </div>
                <button
                  class="btn btn-primary btn-small"
                  @click="approveApplication(application.userId)"
                  :disabled="application.processing"
                >
                  {{ application.processing ? '处理中...' : '通过' }}
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- 退出确认弹窗 -->
    <div v-if="showLeaveConfirmModal" class="modal-overlay" @click="closeLeaveConfirmModal">
      <div class="modal-content confirm-modal" @click.stop>
        <div class="modal-header">
          <h3>确认退出社区</h3>
          <button class="modal-close" @click="closeLeaveConfirmModal">
            <svg fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M6 18L18 6M6 6l12 12"
              ></path>
            </svg>
          </button>
        </div>
        <div class="modal-body">
          <div class="confirm-content">
            <div class="warning-icon">
              <svg fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.732-.833-2.464 0L3.34 16.5c-.77.833.192 2.5 1.732 2.5z"
                ></path>
              </svg>
            </div>
            <p class="confirm-text">
              你确定要退出社区 <strong>{{ communityData.name }}</strong> 吗？
            </p>
            <p class="confirm-subtitle">退出后，你将无法看到社区内容，如需重新加入需要重新申请。</p>
            <div class="confirm-actions">
              <button
                class="btn btn-secondary"
                @click="closeLeaveConfirmModal"
                :disabled="membershipStatus.loading"
              >
                取消
              </button>
              <button
                class="btn btn-danger"
                @click="confirmLeaveCommunity"
                :disabled="membershipStatus.loading"
              >
                {{ membershipStatus.loading ? '退出中...' : '确认退出' }}
              </button>
            </div>
          </div>
        </div>
      </div>
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
import ActivityParticipation from '../components/ActivityParticipation.vue'
import CreateActivity from '../components/CreateActivity.vue'
import CreatePost from '../components/CreatePost.vue'
import PostList from '../components/PostList.vue'
import { PostAPI } from '../api'
import { User } from '@/modules/auth/scripts/User'
import request from '../utils/request.ts'

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
  ownerId?: number
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

// 新增：成员状态管理
interface MembershipStatus {
  isJoined: boolean
  loading: boolean
}

// 新增：申请相关类型
interface Application {
  userId: number
  applyTime: string
  status: number
  processedTime?: string | null
  role?: number | null
  processing?: boolean
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

// 新增：成员状态管理
const membershipStatus = ref<MembershipStatus>({
  isJoined: false,
  loading: false,
})

// 新增：申请管理相关状态
const showApplicationModal = ref(false)
const applications = ref<Application[]>([])
const applicationsLoading = ref(false)
const isOwner = ref(false)

// 新增：退出确认弹窗状态
const showLeaveConfirmModal = ref(false)

const getCurrentUserId = (): number | null => {
  try {
    const userInstance = User.getInstance()
    const userId = userInstance?.userAuth?.userId

    if (!userId) {
      console.warn('⚠️ 用户未登录或用户ID不存在')
      return null
    }

    console.log('👤 当前用户ID:', userId)
    return Number(userId) // 确保返回数字类型
  } catch (error) {
    console.error('❌ 获取用户ID失败:', error)
    return null
  }
}

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
    console.error('获取图片失败:', error)
    return ''
  }
}

//  processImageUrls 函数
const processImageUrls = async (): Promise<void> => {
  console.log('开始处理图片URL...')

  // 处理头像
  if (communityData.value.avatarUrl) {
    console.log('原始头像URL:', communityData.value.avatarUrl)
    processedAvatarUrl.value = await getProxiedImageUrl(communityData.value.avatarUrl)
    console.log('处理后头像URL:', processedAvatarUrl.value)
  }

  // 处理横幅
  if (communityData.value.bannerUrl) {
    console.log('原始横幅URL:', communityData.value.bannerUrl)
    processedBannerUrl.value = await getProxiedImageUrl(communityData.value.bannerUrl)
    console.log('处理后横幅URL:', processedBannerUrl.value)
  }
}

// 检查用户成员状态和角色
const checkMembershipStatus = async (circleId: number): Promise<void> => {
  try {
    console.log(`检查用户是否为社区 ${circleId} 的成员...`)

    // 获取当前用户ID
    const currentUserId = getCurrentUserId()
    if (!currentUserId) {
      membershipStatus.value.isJoined = false
      isOwner.value = false
      return
    }

    // 获取社区成员列表
    const response = await request.get(`/api/Circles/${circleId}/members`)

    if (response.data && response.data.code === 200) {
      const members = response.data.data || []

      // 查找当前用户在成员列表中的信息
      const currentUserMember = members.find((member: any) => member.userId === currentUserId)

      if (currentUserMember) {
        membershipStatus.value.isJoined = true
        // 检查是否为圈主 (role = 1)
        isOwner.value = currentUserMember.role === 1
        console.log(`✅ 用户是社区 ${circleId} 的成员，角色: ${currentUserMember.role}`)
      } else {
        membershipStatus.value.isJoined = false
        isOwner.value = false
        console.log(`❌ 用户不是社区 ${circleId} 的成员`)
      }
    } else {
      membershipStatus.value.isJoined = false
      isOwner.value = false
    }
  } catch (error: any) {
    membershipStatus.value.isJoined = false
    isOwner.value = false
    console.error(`检查成员状态失败:`, error)
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
  return isOwner.value
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
      ownerId: circleInfo.ownerId,
    }

    // 检查用户成员状态
    await checkMembershipStatus(communityData.value.id)

    // 设置版主信息（圈主）
    const ownerId = circleInfo.ownerId
    if (ownerId) {
      if (Array.isArray(membersInfo) && membersInfo.length > 0) {
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
      } else {
        // 设置默认版主信息（圈主）
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

// 刷新帖子列表的方法
const refreshPosts = () => {
  if (postListRef.value) {
    postListRef.value.refresh()
  }
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

// 处理UTC时间转北京时间
const convertUTCToBeijingTime = (dateString: string): Date => {
  if (!dateString) return new Date()

  // 如果时间字符串没有时区标识，则认为是UTC时间
  let utcTime: Date

  if (dateString.includes('Z') || dateString.includes('+') || dateString.includes('-')) {
    // 已经有时区信息
    utcTime = new Date(dateString)
  } else {
    // 没有时区信息，手动添加Z标识表示UTC时间
    utcTime = new Date(dateString + 'Z')
  }

  return utcTime
}

// 替换你现有的 formatDateTime 函数
const formatDateTime = (dateString: string): string => {
  if (!dateString) return '未知'

  console.log('原始时间字符串:', dateString) // 调试用

  // 直接手动处理时间转换
  let date: Date

  try {
    // 如果没有时区信息，假设是UTC时间并手动加8小时
    if (!dateString.includes('Z') && !dateString.includes('+') && !dateString.includes('-')) {
      // 创建UTC时间
      const utcDate = new Date(dateString + 'Z')
      // 手动加8小时转为北京时间
      date = new Date(utcDate.getTime() + 8 * 60 * 60 * 1000)
    } else {
      // 有时区信息的情况
      const utcDate = new Date(dateString)
      date = new Date(utcDate.getTime() + 8 * 60 * 60 * 1000)
    }

    console.log('转换后时间:', date) // 调试用

    // 格式化输出
    return date.toLocaleString('zh-CN', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit',
    })
  } catch (error) {
    console.error('时间转换错误:', error)
    return '时间格式错误'
  }
}

const formatDate = (dateString: string): string => {
  if (!dateString) return '未知'

  const date = convertUTCToBeijingTime(dateString)

  return date.toLocaleDateString('zh-CN', {
    year: 'numeric',
    month: 'long',
    timeZone: 'Asia/Shanghai',
  })
}

const handleCreatePost = (): void => {
  showCreatePost.value = true
}

// 处理帖子创建完成
const handlePostCreated = async (post: any): Promise<void> => {
  console.log('新帖子已创建:', post)
  showCreatePost.value = false

  // 刷新帖子列表
  if (postListRef.value) {
    postListRef.value.refresh()
  }
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

// 修改后的加入社区方法
const handleJoinCommunity = async (): Promise<void> => {
  try {
    membershipStatus.value.loading = true

    // 调用加入社区的API
    const response = await request.post(`/api/Circles/${communityData.value.id}/join`)

    // 显示后端返回的消息
    if (response.data && response.data.msg) {
      alert(response.data.msg)
    }

    // 重新检查成员状态
    await checkMembershipStatus(communityData.value.id)
  } catch (error: any) {
    console.error('加入社区失败:', error)

    // 显示错误消息
    let errorMsg = '操作失败，请稍后重试'
    if (error.response && error.response.data && error.response.data.msg) {
      errorMsg = error.response.data.msg
    }
    alert(errorMsg)
  } finally {
    membershipStatus.value.loading = false
  }
}

// 新增：处理退出社区点击事件
const handleLeaveCommunity = (): void => {
  showLeaveConfirmModal.value = true
}

// 新增：关闭退出确认弹窗
const closeLeaveConfirmModal = (): void => {
  showLeaveConfirmModal.value = false
}

// 新增：确认退出社区
const confirmLeaveCommunity = async (): Promise<void> => {
  try {
    membershipStatus.value.loading = true

    // 调用退出社区的API - 使用新的接口
    const response = await request.delete(`/api/Circles/${communityData.value.id}/membership`)

    console.log('退出社区响应:', response.data)

    // 显示成功消息
    if (response.data && response.data.msg) {
      alert(response.data.msg)
    } else {
      alert('已成功退出社区')
    }

    // 关闭弹窗
    showLeaveConfirmModal.value = false

    // 重新检查成员状态
    await checkMembershipStatus(communityData.value.id)

    // 更新成员数量
    if (communityData.value.memberCount > 0) {
      communityData.value.memberCount -= 1
    }
  } catch (error: any) {
    console.error('退出社区失败:', error)

    // 显示错误消息
    let errorMsg = '退出失败，请稍后重试'
    if (error.response && error.response.data && error.response.data.msg) {
      errorMsg = error.response.data.msg
    }
    alert(errorMsg)
  } finally {
    membershipStatus.value.loading = false
  }
}

// 加载申请列表
const loadApplications = async (): Promise<void> => {
  try {
    applicationsLoading.value = true
    const response = await request.get(`/api/Circles/${communityData.value.id}/applications`)

    if (response.data && response.data.code === 200) {
      applications.value = response.data.data.pendingApplications || []
    } else {
      applications.value = []
    }
  } catch (error) {
    console.error('加载申请列表失败:', error)
    applications.value = []
  } finally {
    applicationsLoading.value = false
  }
}

// 关闭申请弹窗
const closeApplicationModal = (): void => {
  showApplicationModal.value = false
  applications.value = []
}

// 通过申请
const approveApplication = async (targetUserId: number): Promise<void> => {
  try {
    // 设置当前申请为处理中状态
    const application = applications.value.find((app) => app.userId === targetUserId)
    if (application) {
      application.processing = true
    }

    // 调用审核通过API
    const response = await request.put(
      `/api/Circles/${communityData.value.id}/applications/${targetUserId}`,
      null,
      {
        params: {
          approve: true,
          role: 2, // 默认设置为普通成员
        },
      },
    )

    if (response.data && response.data.code === 200) {
      // 成功后从列表中移除该申请
      applications.value = applications.value.filter((app) => app.userId !== targetUserId)

      // 更新成员数量
      communityData.value.memberCount += 1

      alert('审核通过成功')
    } else {
      throw new Error(response.data?.msg || '审核失败')
    }
  } catch (error: any) {
    console.error('审核失败:', error)

    let errorMsg = '审核失败，请稍后重试'
    if (error.response && error.response.data && error.response.data.msg) {
      errorMsg = error.response.data.msg
    }
    alert(errorMsg)
  } finally {
    // 重置处理中状态
    const application = applications.value.find((app) => app.userId !== targetUserId)
    if (application) {
      application.processing = false
    }
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

// 新增活动相关方法
const handleActivityCreated = (activity: any): void => {
  console.log('新活动已创建:', activity)
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

// 添加 PostList 引用
const postListRef = ref() // 新增这一行

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

// 监听申请弹窗显示状态，加载申请列表
watch(showApplicationModal, (show) => {
  if (show) {
    loadApplications()
  }
})

// 生命周期
onMounted(() => {
  loadCommunityData()
  loadActivityStats()
  fetchImage()
})
</script>

<style scoped>
/* 原有样式保持不变 */
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
  overflow: hidden; /* 改为hidden，因为内部有滚动区域 */
  height: calc(100vh - 48px); /* 设置固定高度，48px为容器的padding */
  display: flex;
  flex-direction: column;
}

/* 新增：可滚动内容区域的通用样式 */
.scrollable-content {
  height: calc(100vh - 400px); /* 减去头部、tab等区域的高度 */
  overflow-y: auto; /* 垂直滚动 */
  overflow-x: hidden; /* 隐藏水平滚动 */
}

/* 自定义滚动条样式 */
.scrollable-content::-webkit-scrollbar {
  width: 6px;
}

.scrollable-content::-webkit-scrollbar-track {
  background: #1e293b; /* slate-800 */
  border-radius: 3px;
}

.scrollable-content::-webkit-scrollbar-thumb {
  background: #475569; /* slate-600 */
  border-radius: 3px;
  transition: background-color 0.2s;
}

.scrollable-content::-webkit-scrollbar-thumb:hover {
  background: #64748b; /* slate-500 */
}

/* 社区头部区域保持固定，不参与滚动 */
.community-header-section {
  flex-shrink: 0; /* 防止缩放 */
}

.content-tabs {
  flex-shrink: 0; /* 防止缩放 */
}

.community-header-section {
  position: relative;
}

.community-banner {
  height: 192px;
  background: #334155; /* slate-700 */
  overflow: hidden;
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

/* 新增：加入操作区域样式 */
.join-actions {
  display: flex;
  flex-direction: column;
  gap: 8px;
  align-items: flex-end;
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

/* 新增：危险按钮样式 */
.btn-danger {
  background: #dc2626; /* red-600 */
  color: white;
  border: none;
}

.btn-danger:hover:not(:disabled) {
  background: #b91c1c; /* red-700 */
}

.btn-small {
  padding: 6px 12px;
  font-size: 12px;
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

/* 确保帖子列表和活动容器可以滚动 */
.posts-list {
  overflow: visible; /* 添加这行 */
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

/* 新增：弹窗样式 */
.modal-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.5);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
}

.modal-content {
  background: #1e293b; /* slate-800 */
  border-radius: 12px;
  border: 1px solid #334155; /* slate-700 */
  max-width: 600px;
  width: 90%;
  max-height: 80vh;
  overflow: hidden;
  display: flex;
  flex-direction: column;
}

/* 新增：确认弹窗样式 */
.confirm-modal {
  max-width: 400px;
}

.modal-header {
  padding: 20px 24px;
  border-bottom: 1px solid #334155; /* slate-700 */
  display: flex;
  align-items: center;
  justify-content: space-between;
}

.modal-header h3 {
  margin: 0;
  color: #f1f5f9; /* slate-100 */
  font-size: 18px;
  font-weight: 600;
}

.modal-close {
  background: none;
  border: none;
  color: #64748b; /* slate-500 */
  cursor: pointer;
  padding: 4px;
  border-radius: 4px;
  transition: all 0.2s;
}

.modal-close:hover {
  color: #f1f5f9; /* slate-100 */
  background: #334155; /* slate-700 */
}

.modal-close svg {
  width: 20px;
  height: 20px;
}

.modal-body {
  padding: 24px;
  overflow-y: auto;
  flex: 1;
}

/* 新增：确认内容样式 */
.confirm-content {
  text-align: center;
}

.warning-icon {
  width: 64px;
  height: 64px;
  margin: 0 auto 16px;
  color: #f59e0b; /* amber-500 */
}

.warning-icon svg {
  width: 100%;
  height: 100%;
}

.confirm-text {
  font-size: 16px;
  color: #f1f5f9; /* slate-100 */
  margin-bottom: 8px;
  line-height: 1.5;
}

.confirm-subtitle {
  font-size: 14px;
  color: #64748b; /* slate-500 */
  margin-bottom: 24px;
  line-height: 1.5;
}

.confirm-actions {
  display: flex;
  gap: 12px;
  justify-content: center;
}

.empty-applications {
  text-align: center;
  color: #64748b; /* slate-500 */
  padding: 40px 20px;
}

.applications-list {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.application-item {
  border: 1px solid #334155; /* slate-700 */
  border-radius: 8px;
  padding: 16px;
  background: #0f172a; /* slate-900 */
}

.application-info {
  display: flex;
  align-items: center;
  justify-content: space-between;
  gap: 16px;
}

.user-info {
  display: flex;
  align-items: center;
  gap: 12px;
  flex: 1;
}

.user-avatar {
  width: 40px;
  height: 40px;
  border-radius: 50%;
}

.user-details {
  flex: 1;
}

.user-id {
  margin: 0 0 4px 0;
  color: #f1f5f9; /* slate-100 */
  font-weight: 500;
  font-size: 14px;
}

.apply-time {
  margin: 0;
  color: #64748b; /* slate-500 */
  font-size: 12px;
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

  .join-actions {
    flex-direction: row;
    align-items: center;
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
    overflow: visible; /* 添加这行 */
    padding: 24px;
  }

  .modal-content {
    width: 95%;
    max-height: 90vh;
  }

  .application-info {
    flex-direction: column;
    align-items: flex-start;
    gap: 12px;
  }

  .user-info {
    width: 100%;
  }

  .confirm-actions {
    flex-direction: column;
  }
}
</style>
