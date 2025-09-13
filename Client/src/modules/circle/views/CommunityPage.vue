<template>
  <div>
    <!-- 主要内容 -->
    <div class="main-container">
      <!-- 中间主内容区 -->
      <main class="main-content">
        <!-- 页面标题 -->
        <div class="page-header">
          <h1 class="page-title">社区广场</h1>
          <p class="page-subtitle">发现和加入感兴趣的社区</p>
        </div>

        <!-- 搜索和筛选 -->
        <div class="search-section">
          <div class="search-bar">
            <input
              v-model="searchQuery"
              type="text"
              placeholder="搜索社区..."
              class="search-input"
              @input="handleSearch"
            />
            <button class="search-btn">
              <svg fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z"
                ></path>
              </svg>
            </button>
          </div>
          <button class="btn btn-primary" @click="handleCreateCommunity">创建社区</button>
        </div>

        <!-- 内容切换 Tab -->
        <div class="content-tabs">
          <a
            href="#"
            class="tab-link"
            :class="{ active: activeTab === 'all' }"
            @click.prevent="changeTab('all')"
          >
            全部社区
          </a>
          <a
            href="#"
            class="tab-link"
            :class="{ active: activeTab === 'joined' }"
            @click.prevent="changeTab('joined')"
          >
            已加入
          </a>
          <a
            href="#"
            class="tab-link"
            :class="{ active: activeTab === 'recommended' }"
            @click.prevent="changeTab('recommended')"
          >
            推荐
          </a>
        </div>

        <!-- 加载状态 -->
        <div v-if="loading" class="loading-state">
          <div class="loading-spinner"></div>
          <p>正在加载社区列表...</p>
        </div>

        <!-- 错误状态 -->
        <div v-else-if="error" class="error-state">
          <p>{{ error }}</p>
          <button class="btn btn-primary" @click="loadCommunities">重试</button>
        </div>

        <!-- 社区列表 -->
        <div v-else class="communities-grid">
          <!-- 暂无社区 -->
          <div v-if="filteredCommunities.length === 0" class="empty-communities">
            <div class="empty-icon">🏘️</div>
            <h3>暂无社区</h3>
            <p>还没有找到符合条件的社区，创建一个新社区吧！</p>
            <button class="btn btn-primary" @click="handleCreateCommunity">创建社区</button>
          </div>

          <!-- 社区卡片 -->
          <div
            v-else
            v-for="community in filteredCommunities"
            :key="community.id"
            class="community-card"
            @click="handleCommunityClick(community.id)"
          >
            <div class="community-banner">
              <img
                :src="
                  processedImages[community.id]?.banner ||
                  `https://placehold.co/300x120/1677ff/ffffff?text=${encodeURIComponent(community.name)}`
                "
                :alt="`${community.name} banner`"
              />
            </div>

            <div class="community-info">
              <div class="community-header">
                <img
                  class="community-avatar"
                  :src="
                    processedImages[community.id]?.avatar ||
                    `https://placehold.co/60x60/1677ff/ffffff?text=${encodeURIComponent(community.name[0] || 'C')}`
                  "
                  :alt="`${community.name} avatar`"
                />
                <div class="community-meta">
                  <h3 class="community-name">{{ community.name }}</h3>
                  <p class="community-members">
                    {{ formatMemberCount(community.memberCount) }} 成员
                  </p>
                </div>
              </div>

              <p class="community-description">
                {{ community.description || '暂无描述' }}
              </p>

              <div class="community-tags">
                <span class="community-tag">{{ community.category || '通用' }}</span>
                <span v-if="community.isPrivate" class="community-tag private">私有</span>
              </div>

              <div class="community-actions">
                <button
                  class="btn btn-sm"
                  :class="[getButtonClass(community)]"
                  @click.stop="toggleJoinCommunity(community.id)"
                  :disabled="communityStore.getCommunityLoadingState(community.id)"
                >
                  {{ getButtonText(community) }}
                </button>
              </div>
            </div>
          </div>
        </div>
      </main>

      <!-- 右侧边栏 -->
      <aside class="right-sidebar">
        <div class="sidebar-content space-y-6">
          <!-- 热门社区 -->
          <div class="sidebar-card">
            <h2 class="sidebar-title">热门社区</h2>
            <div v-if="popularCommunities.length === 0" class="empty-popular">
              <p>暂无热门社区</p>
            </div>
            <ul v-else class="popular-list">
              <li v-for="community in popularCommunities" :key="community.id" class="popular-item">
                <div class="popular-info" @click="handleCommunityClick(community.id)">
                  <h4 class="popular-name">{{ community.name }}</h4>
                  <p class="popular-members">{{ formatMemberCount(community.memberCount) }} 成员</p>
                </div>
              </li>
            </ul>
          </div>

          <!-- 分类导航 -->
          <div class="sidebar-card">
            <h2 class="sidebar-title">快速导航</h2>
            <ul class="category-list">
              <li class="category-item">
                <a href="#" @click.prevent="changeTab('all')" class="category-link"> 全部社区 </a>
              </li>
              <li class="category-item">
                <a href="#" @click.prevent="changeTab('joined')" class="category-link">
                  我的社区
                </a>
              </li>
              <li class="category-item">
                <a href="#" @click.prevent="handleCreateCommunity" class="category-link">
                  创建社区
                </a>
              </li>
            </ul>
          </div>
        </div>
      </aside>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { CircleAPI, getProxiedImageUrl } from '../api.ts'
import { useCommunityStore } from '../store.ts'
import request from '../utils/request.ts'

// 类型定义
interface Community {
  id: number
  name: string
  description: string
  memberCount: number
  category: string
  isPrivate: boolean
  isJoined: boolean
  isLoading: boolean
  createdAt: string
  ownerId?: number
  avatarUrl?: string // 新增
  bannerUrl?: string
}

// 路由
const router = useRouter()

const communityStore = useCommunityStore()

// 状态
const activeTab = ref<'all' | 'joined' | 'recommended'>('all')
const loading = ref(true)
const error = ref<string | null>(null)
const searchQuery = ref('')

// 数据
const allCommunities = ref<Community[]>([])
const joinedCommunities = ref<Community[]>([])

// 当前用户ID（与后端硬编码保持一致）
const currentUserId = 2

const processedImages = ref<Record<number, { avatar: string; banner: string }>>({})

// 添加图片处理函数（和详情页完全一样）
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

// 添加处理图片URLs的函数（和详情页完全一样）
const processImageUrls = async (community: Community): Promise<void> => {
  const processedAvatar = ref<string>('')
  const processedBanner = ref<string>('')

  // 处理头像
  if (community.avatarUrl) {
    processedAvatar.value = await getProxiedImageUrl(community.avatarUrl)
  }

  // 处理横幅
  if (community.bannerUrl) {
    processedBanner.value = await getProxiedImageUrl(community.bannerUrl)
  }

  // 存储处理后的图片URL
  processedImages.value[community.id] = {
    avatar: processedAvatar.value,
    banner: processedBanner.value,
  }
}

// 计算属性
const filteredCommunities = computed(() => {
  let communities: Community[] = []

  // 根据标签页选择数据源
  if (activeTab.value === 'joined') {
    communities = joinedCommunities.value
  } else if (activeTab.value === 'recommended') {
    // 推荐逻辑：按成员数排序
    communities = allCommunities.value
      .slice()
      .sort((a, b) => b.memberCount - a.memberCount)
      .slice(0, 10)
  } else {
    communities = allCommunities.value
  }

  // 按搜索关键词筛选
  if (searchQuery.value.trim()) {
    const query = searchQuery.value.toLowerCase()
    communities = communities.filter(
      (c) =>
        c.name.toLowerCase().includes(query) ||
        (c.description && c.description.toLowerCase().includes(query)),
    )
  }

  return communities
})

const popularCommunities = computed(() => {
  return allCommunities.value
    .slice()
    .sort((a, b) => b.memberCount - a.memberCount)
    .slice(0, 5)
})

// 加载所有社区
const loadAllCommunities = async (): Promise<void> => {
  try {
    const response = await CircleAPI.getCircles()

    if (response && response.code === 200 && Array.isArray(response.data)) {
      const communities = await Promise.all(
        response.data.map(async (item: any) => {
          const community: Community = {
            id: item.circleId || item.id,
            name: item.name || '未知社区',
            description: item.description || '',
            memberCount: item.memberCount || 0,
            category: item.categories || item.category || '通用',
            isPrivate: item.isPrivate || false,
            isJoined: false,
            isLoading: false,
            createdAt: item.createdAt || new Date().toISOString(),
            ownerId: item.ownerId,
            // 兼容多种命名方式 - 和详情页完全一样
            avatarUrl: item.avatarUrl || item.avatar_url || item.AVATAR_URL,
            bannerUrl: item.bannerUrl || item.banner_url || item.BANNER_URL,
          }

          // 检查用户是否已加入该社区
          try {
            community.isJoined = await CircleAPI.checkMembership(community.id)
          } catch (error) {
            console.error(`检查社区 ${community.id} 成员状态失败:`, error)
            community.isJoined = false
          }

          // 处理图片URL - 和详情页完全一样的方式
          await processImageUrls(community)

          return community
        }),
      )

      // 使用store管理状态
      communityStore.setAllCommunities(communities)
      allCommunities.value = communities
    } else {
      throw new Error('社区列表数据格式错误')
    }
  } catch (err) {
    console.error('加载所有社区失败:', err)
    throw err
  }
}

// 加载已加入的社区
const loadJoinedCommunities = async (): Promise<void> => {
  try {
    const response = await CircleAPI.getUserJoinedCircles(currentUserId)

    if (response && response.code === 200 && Array.isArray(response.data)) {
      const communities = await Promise.all(
        response.data.map(async (item: any) => {
          const community: Community = {
            id: item.circleId || item.id,
            name: item.name || '未知社区',
            description: item.description || '',
            memberCount: item.memberCount || 0,
            category: item.categories || item.category || '通用',
            isPrivate: item.isPrivate || false,
            isJoined: true,
            isLoading: false,
            createdAt: item.createdAt || new Date().toISOString(),
            ownerId: item.ownerId,
            // 兼容多种命名方式 - 和详情页完全一样
            avatarUrl: item.avatarUrl || item.avatar_url || item.AVATAR_URL,
            bannerUrl: item.bannerUrl || item.banner_url || item.BANNER_URL,
          }

          // 处理图片URL - 和详情页完全一样的方式
          await processImageUrls(community)

          return community
        }),
      )

      // 使用store管理状态
      communityStore.setJoinedCommunities(communities)
      joinedCommunities.value = communities
    } else {
      joinedCommunities.value = []
      communityStore.setJoinedCommunities([])
    }
  } catch (err) {
    console.error('加载已加入社区失败:', err)
    joinedCommunities.value = []
    communityStore.setJoinedCommunities([])
  }
}

// 加载社区数据
const loadCommunities = async (): Promise<void> => {
  try {
    loading.value = true
    error.value = null

    // 并行加载所有社区和已加入社区
    await Promise.all([loadAllCommunities(), loadJoinedCommunities()])
  } catch (err) {
    console.error('加载社区数据失败:', err)
    error.value = '加载社区列表失败，请稍后重试'
  } finally {
    loading.value = false
  }
}

// 方法
const changeTab = (tab: 'all' | 'joined' | 'recommended'): void => {
  activeTab.value = tab
}

const handleSearch = (): void => {
  // 搜索逻辑已在 computed 中处理
}

const formatMemberCount = (count: number): string => {
  if (count >= 10000) {
    return (count / 10000).toFixed(1) + '万'
  }
  return count.toLocaleString()
}

const handleCreateCommunity = (): void => {
  router.push('/create-community')
}

const handleCommunityClick = (communityId: number): void => {
  router.push(`/community/${communityId}`)
}

const toggleJoinCommunity = async (communityId: number): Promise<void> => {
  try {
    await communityStore.toggleCommunityMembership(communityId)
  } catch (error) {
    console.error('操作失败:', error)
  }
}

// 生命周期
onMounted(() => {
  loadCommunities()
})

const getButtonText = (community: Community): string => {
  if (communityStore.getCommunityLoadingState(community.id)) {
    return '处理中...'
  }

  if (activeTab.value === 'joined') {
    return '退出'
  }

  const isJoined = communityStore.getCommunityJoinStatus(community.id)
  return isJoined ? '退出' : '加入'
}

const getButtonClass = (community: Community): string => {
  if (activeTab.value === 'joined') {
    return 'btn-secondary'
  }

  const isJoined = communityStore.getCommunityJoinStatus(community.id)
  return isJoined ? 'btn-secondary' : 'btn-primary'
}
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

.main-content {
  background: #1e293b; /* slate-800 */
  border-radius: 12px;
  border: 1px solid #334155; /* slate-700 */
  padding: 24px;
}

.page-header {
  margin-bottom: 24px;
}

.page-title {
  font-size: 28px;
  font-weight: 700;
  margin: 0 0 8px 0;
  color: #f1f5f9; /* slate-100 */
}

.page-subtitle {
  font-size: 16px;
  color: #64748b; /* slate-500 */
  margin: 0;
}

.search-section {
  display: flex;
  gap: 16px;
  margin-bottom: 24px;
  align-items: center;
}

.search-bar {
  flex: 1;
  position: relative;
}

.search-input {
  width: 100%;
  padding: 12px 48px 12px 16px;
  border: 1px solid #475569; /* slate-600 */
  border-radius: 8px;
  font-size: 14px;
  transition: border-color 0.2s;
  background: #0f172a; /* slate-900 */
  color: #e2e8f0; /* slate-200 */
}

.search-input:focus {
  outline: none;
  border-color: #0ea5e9; /* sky-500 */
}

.search-btn {
  position: absolute;
  right: 12px;
  top: 50%;
  transform: translateY(-50%);
  background: none;
  border: none;
  color: #64748b; /* slate-500 */
  cursor: pointer;
}

.search-btn svg {
  width: 20px;
  height: 20px;
}

.content-tabs {
  display: flex;
  border-bottom: 1px solid #334155; /* slate-700 */
  margin-bottom: 24px;
}

.tab-link {
  padding: 16px 24px;
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
}

.loading-state,
.error-state {
  text-align: center;
  padding: 60px 20px;
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

.communities-grid {
  display: flex; /* 改为flex布局 */
  flex-direction: column; /* 垂直排列 */
  gap: 20px; /* 保持间距 */
}

.empty-communities {
  grid-column: 1 / -1;
  text-align: center;
  padding: 60px 20px;
  color: #64748b; /* slate-500 */
}

.empty-icon {
  font-size: 48px;
  margin-bottom: 16px;
}

.community-card {
  border: 1px solid #334155;
  border-radius: 12px;
  overflow: hidden;
  cursor: pointer;
  transition: all 0.2s;
  background: #0f172a;
  display: flex; /* 改为水平flex布局 */
  align-items: stretch; /* 让内容高度一致 */
  width: 100%; /* 占满容器宽度 */
}

.community-card:hover {
  border-color: #0ea5e9; /* sky-500 */
  box-shadow: 0 4px 12px rgba(14, 165, 233, 0.1);
}

.community-banner {
  width: 200px; /* 固定横幅宽度 */
  height: 140px; /* 稍微增加高度 */
  overflow: hidden;
  background: #334155;
  flex-shrink: 0; /* 防止压缩 */
  display: flex; /* 新增：用于居中图片 */
  align-items: center; /* 新增：垂直居中 */
  justify-content: center; /* 新增：水平居中 */
}

.community-banner img {
  width: 100%;
  height: 100%;
  object-fit: cover;
  object-position: center; /* 新增：确保图片居中裁剪 */
}

.community-info {
  padding: 20px; /* 稍微增加内边距 */
  flex: 1; /* 占据剩余空间 */
  display: flex;
  flex-direction: column;
  justify-content: space-between; /* 让内容分布均匀 */
}

.community-header {
  display: flex;
  align-items: center;
  margin-bottom: 12px;
}

.community-avatar {
  width: 48px;
  height: 48px;
  border-radius: 50%;
  margin-right: 12px;
  background: #475569;
  object-fit: cover; /* 新增：确保头像正确显示 */
  object-position: center; /* 新增：头像居中 */
}

.community-meta {
  flex: 1;
}

.community-name {
  font-size: 16px;
  font-weight: 600;
  margin: 0 0 4px 0;
  color: #f1f5f9; /* slate-100 */
}

.community-members {
  font-size: 12px;
  color: #64748b; /* slate-500 */
  margin: 0;
}

.community-description {
  font-size: 14px;
  color: #cbd5e1;
  line-height: 1.5; /* 稍微增加行高 */
  margin-bottom: 12px;
  display: -webkit-box;
  -webkit-line-clamp: 2; /* 保持2行 */
  -webkit-box-orient: vertical;
  overflow: hidden;
  flex: 1; /* 让描述占据可用空间 */
}

.community-tags {
  display: flex;
  gap: 8px;
  margin-bottom: 16px;
}

.community-tag {
  padding: 4px 8px;
  background: #334155; /* slate-700 */
  color: #cbd5e1; /* slate-300 */
  font-size: 12px;
  border-radius: 4px;
}

.community-tag.private {
  background: #7f1d1d; /* red-900 */
  color: #f87171; /* red-400 */
}

.community-actions {
  display: flex;
  justify-content: flex-end;
  align-items: center; /* 新增：垂直居中按钮 */
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

.btn-sm {
  padding: 6px 16px;
  font-size: 12px;
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
  margin-bottom: 16px;
}

.popular-list,
.category-list {
  list-style: none;
  padding: 0;
  margin: 0;
}

.popular-item,
.category-item {
  margin-bottom: 12px;
}

.popular-info {
  cursor: pointer;
  padding: 8px;
  border-radius: 6px;
  transition: background-color 0.2s;
}

.popular-info:hover {
  background: #334155; /* slate-700 */
}

.popular-name {
  font-size: 14px;
  font-weight: 600;
  margin: 0 0 4px 0;
  color: #f1f5f9; /* slate-100 */
}

.popular-members {
  font-size: 12px;
  color: #64748b; /* slate-500 */
  margin: 0;
}

.category-link {
  display: block;
  padding: 8px 12px;
  color: #cbd5e1; /* slate-300 */
  text-decoration: none;
  border-radius: 6px;
  transition: all 0.2s;
}

.category-link:hover {
  background: #334155; /* slate-700 */
  color: #38bdf8; /* sky-400 */
}

.empty-popular {
  text-align: center;
  color: #64748b; /* slate-500 */
  font-size: 14px;
}

/* 响应式调整 */
@media (max-width: 768px) {
  .main-container {
    padding: 16px;
  }

  .search-section {
    flex-direction: column;
    align-items: stretch;
  }

  .community-card {
    flex-direction: column; /* 移动端改为垂直布局 */
  }

  .community-banner {
    width: 100%; /* 移动端横幅占满宽度 */
    height: 120px; /* 移动端高度稍小 */
  }

  .community-info {
    padding: 16px;
  }
}

@media (max-width: 1024px) {
  .main-container {
    grid-template-columns: 1fr;
  }
}
</style>
