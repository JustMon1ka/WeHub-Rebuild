<template>
  <div>
    <!-- 顶部导航 -->
    <NavBar />

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
                  getImageUrl(community.bannerUrl) ||
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
                    getImageUrl(community.avatarUrl) ||
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
        <div class="sidebar-content">
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
import NavBar from '../components/NavBar.vue'
import { CircleAPI } from '../api.ts'
import { useCommunityStore } from '../store.ts'

const getImageUrl = (imageUrl: string): string => {
  return CircleAPI.getImageProxyUrl(imageUrl)
}

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
    console.log('加载所有社区列表...')
    const response = await CircleAPI.getCircles()
    console.log('所有社区响应:', response)

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
            // 兼容多种命名方式
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

          return community
        }),
      )

      // 使用store管理状态
      communityStore.setAllCommunities(communities)
      allCommunities.value = communities

      console.log('处理后的所有社区列表:', allCommunities.value)
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
    console.log('加载已加入社区列表...')
    const response = await CircleAPI.getUserJoinedCircles(currentUserId)
    console.log('已加入社区响应:', response)

    if (response && response.code === 200 && Array.isArray(response.data)) {
      const communities = response.data.map((item: any) => ({
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
        // 兼容多种命名方式
        avatarUrl: item.avatarUrl || item.avatar_url || item.AVATAR_URL,
        bannerUrl: item.bannerUrl || item.banner_url || item.BANNER_URL,
      }))

      // 使用store管理状态
      communityStore.setJoinedCommunities(communities)
      joinedCommunities.value = communities

      console.log('处理后的已加入社区列表:', joinedCommunities.value)
    } else {
      console.log('没有已加入的社区或数据格式错误')
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
}

.main-content {
  background: #fff;
  border-radius: 12px;
  border: 1px solid #e4e6ea;
  padding: 24px;
}

.page-header {
  margin-bottom: 24px;
}

.page-title {
  font-size: 28px;
  font-weight: 700;
  margin: 0 0 8px 0;
  color: #1d2129;
}

.page-subtitle {
  font-size: 16px;
  color: #86909c;
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
  border: 1px solid #d9d9d9;
  border-radius: 8px;
  font-size: 14px;
  transition: border-color 0.2s;
}

.search-input:focus {
  outline: none;
  border-color: #1677ff;
}

.search-btn {
  position: absolute;
  right: 12px;
  top: 50%;
  transform: translateY(-50%);
  background: none;
  border: none;
  color: #86909c;
  cursor: pointer;
}

.search-btn svg {
  width: 20px;
  height: 20px;
}

.content-tabs {
  display: flex;
  border-bottom: 1px solid #e4e6ea;
  margin-bottom: 24px;
}

.tab-link {
  padding: 16px 24px;
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
}

.loading-state,
.error-state {
  text-align: center;
  padding: 60px 20px;
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

.communities-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(320px, 1fr));
  gap: 20px;
}

.empty-communities {
  grid-column: 1 / -1;
  text-align: center;
  padding: 60px 20px;
  color: #86909c;
}

.empty-icon {
  font-size: 48px;
  margin-bottom: 16px;
}

.community-card {
  border: 1px solid #e4e6ea;
  border-radius: 12px;
  overflow: hidden;
  cursor: pointer;
  transition: all 0.2s;
  background: #fff;
}

.community-card:hover {
  border-color: #1677ff;
  box-shadow: 0 4px 12px rgba(22, 119, 255, 0.1);
}

.community-banner {
  height: 120px;
  overflow: hidden;
}

.community-banner img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.community-info {
  padding: 16px;
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
}

.community-meta {
  flex: 1;
}

.community-name {
  font-size: 16px;
  font-weight: 600;
  margin: 0 0 4px 0;
  color: #1d2129;
}

.community-members {
  font-size: 12px;
  color: #86909c;
  margin: 0;
}

.community-description {
  font-size: 14px;
  color: #4e5969;
  line-height: 1.4;
  margin-bottom: 12px;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.community-tags {
  display: flex;
  gap: 8px;
  margin-bottom: 16px;
}

.community-tag {
  padding: 4px 8px;
  background: #f2f3f5;
  color: #4e5969;
  font-size: 12px;
  border-radius: 4px;
}

.community-tag.private {
  background: #fff2f0;
  color: #ff4d4f;
}

.community-actions {
  display: flex;
  justify-content: flex-end;
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
  background: #f7f8fa;
}

.popular-name {
  font-size: 14px;
  font-weight: 600;
  margin: 0 0 4px 0;
  color: #1d2129;
}

.popular-members {
  font-size: 12px;
  color: #86909c;
  margin: 0;
}

.category-link {
  display: block;
  padding: 8px 12px;
  color: #4e5969;
  text-decoration: none;
  border-radius: 6px;
  transition: all 0.2s;
}

.category-link:hover {
  background: #f7f8fa;
  color: #1677ff;
}

.empty-popular {
  text-align: center;
  color: #86909c;
  font-size: 14px;
}

@media (max-width: 1024px) {
  .main-container {
    grid-template-columns: 1fr;
  }

  .communities-grid {
    grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  }
}

@media (max-width: 768px) {
  .main-container {
    padding: 16px;
  }

  .search-section {
    flex-direction: column;
    align-items: stretch;
  }

  .communities-grid {
    grid-template-columns: 1fr;
  }
}
</style>
