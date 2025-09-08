<template>
  <div class="activity-list-container">
    <!-- 加载状态 -->
    <div v-if="loading" class="loading-state">
      <div class="loading-spinner"></div>
      <p>正在加载活动...</p>
    </div>

    <!-- 错误状态 -->
    <div v-else-if="error" class="error-state">
      <p>{{ error }}</p>
      <button class="btn btn-primary" @click="loadActivities">重试</button>
    </div>

    <!-- 活动内容 -->
    <div v-else class="activity-content">
      <!-- 活动筛选 -->
      <div class="activity-filters">
        <div class="filter-left">
          <button
            v-for="filter in filters"
            :key="filter.key"
            class="filter-btn"
            :class="{ active: activeFilter === filter.key }"
            @click="setActiveFilter(filter.key)"
          >
            {{ filter.label }}
            <span class="filter-count">({{ filter.count }})</span>
          </button>
        </div>

        <div class="filter-right">
          <button @click="showCreateActivity" class="btn btn-primary btn-create">+ 创建活动</button>
        </div>
      </div>

      <!-- 活动列表 -->
      <div class="activities-grid">
        <!-- 空状态 -->
        <div v-if="filteredActivities.length === 0" class="empty-state">
          <div class="empty-icon">🎯</div>
          <h3>{{ getEmptyStateTitle() }}</h3>
          <p>{{ getEmptyStateDescription() }}</p>
        </div>

        <!-- 活动卡片 -->
        <div
          v-else
          v-for="activity in filteredActivities"
          :key="activity.activityId"
          class="activity-card"
          @click="handleActivityClick(activity)"
        >
          <!-- 活动图片 -->
          <div class="activity-image">
            <img :src="activity.imageUrl || getDefaultImage()" :alt="activity.title" />
            <div class="activity-status-overlay">
              <span class="status-badge" :class="getStatusClass(activity)">
                {{ getStatusText(activity) }}
              </span>
            </div>
          </div>

          <!-- 活动信息 -->
          <div class="activity-info">
            <h3 class="activity-title">{{ activity.title }}</h3>
            <p class="activity-description">
              {{ activity.description || '暂无描述' }}
            </p>

            <!-- 活动时间 -->
            <div class="activity-time">
              <div class="time-item">
                <span class="time-label">开始:</span>
                <span class="time-value">{{ formatTime(activity.startTime) }}</span>
              </div>
              <div class="time-item">
                <span class="time-label">结束:</span>
                <span class="time-value">{{ formatTime(activity.endTime) }}</span>
              </div>
            </div>

            <!-- 活动奖励 -->
            <div v-if="activity.reward" class="activity-reward">
              <span class="reward-icon">🎁</span>
              <span class="reward-text">{{ activity.reward }}</span>
            </div>

            <!-- 参与状态 -->
            <div class="participation-status">
              <div class="participant-count">
                <span class="count-icon">👥</span>
                <span>{{ activity.participantCount || 0 }} 人参与</span>
              </div>

              <!-- 用户参与状态 -->
              <div v-if="getUserParticipationStatus(activity)" class="user-status">
                <span class="status-icon" :class="getUserStatusClass(activity)">
                  {{ getUserStatusIcon(activity) }}
                </span>
                <span class="status-text">{{ getUserStatusText(activity) }}</span>
              </div>
            </div>

            <!-- 操作按钮 -->
            <div class="activity-actions">
              <button
                v-if="canJoinActivity(activity)"
                @click.stop="handleJoinActivity(activity)"
                :disabled="isJoining[activity.activityId]"
                class="btn btn-primary btn-sm"
              >
                {{ isJoining[activity.activityId] ? '参加中...' : '参加活动' }}
              </button>

              <button
                v-else-if="getUserParticipationStatus(activity)"
                @click.stop="handleAlreadyJoinedClick"
                class="btn btn-secondary btn-sm"
              >
                已参加
              </button>

              <button
                v-else-if="canCompleteActivity(activity)"
                @click.stop="handleCompleteActivity(activity)"
                :disabled="isCompleting[activity.activityId]"
                class="btn btn-success btn-sm"
              >
                {{ isCompleting[activity.activityId] ? '完成中...' : '完成活动' }}
              </button>

              <button
                v-else-if="canClaimReward(activity)"
                @click.stop="handleClaimReward(activity)"
                :disabled="isClaiming[activity.activityId]"
                class="btn btn-reward btn-sm"
              >
                {{ isClaiming[activity.activityId] ? '领取中...' : '领取奖励' }}
              </button>

              <button @click.stop="handleActivityClick(activity)" class="btn btn-outline btn-sm">
                查看详情
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- 活动详情弹窗 -->
    <ActivityDetail
      v-if="showActivityDetail && selectedActivity"
      :activity="selectedActivity"
      :circle-id="circleId"
      @close="showActivityDetail = false"
      @activity-updated="handleActivityUpdated"
    />

    <!-- 参与心得弹窗 -->
    <ActivityParticipation
      v-if="showParticipationForm && selectedActivity"
      :activity="selectedActivity"
      @close="showParticipationForm = false"
      @submitted="handleParticipationSubmitted"
    />
  </div>

  <!-- 创建活动弹窗 -->
  <CreateActivity
    v-if="showCreateActivityForm"
    :circle-id="circleId"
    @close="showCreateActivityForm = false"
    @saved="handleActivityCreated"
  />
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { Activity, ActivityParticipant, ParticipantStatus, RewardStatus } from '../types'
import { activityApi } from '../api'
import ActivityDetail from './ActivityDetail.vue'
import ActivityParticipation from './ActivityParticipation.vue'
import CreateActivity from './CreateActivity.vue'

interface Props {
  circleId: number
  canCreateActivity?: boolean
}

const props = defineProps<Props>()

const loading = ref(true)
const error = ref('')
const activities = ref<Activity[]>([])
const participationStatuses = ref<Map<number, ActivityParticipant>>(new Map())
const activeFilter = ref<'all' | 'active' | 'participated'>('all')

// 弹窗状态
const showActivityDetail = ref(false)
const showParticipationForm = ref(false)
const showCreateActivityForm = ref(false)
const selectedActivity = ref<Activity | null>(null)

// 操作状态
const isJoining = ref<Record<number, boolean>>({})
const isCompleting = ref<Record<number, boolean>>({})
const isClaiming = ref<Record<number, boolean>>({})

// 计算筛选器
const filters = computed(() => [
  {
    key: 'all',
    label: '全部活动',
    count: activities.value.length,
  },
  {
    key: 'active',
    label: '进行中',
    count: activities.value.filter((a) => isActivityActive(a)).length,
  },
  {
    key: 'participated',
    label: '我参与的',
    count: activities.value.filter((a) => participationStatuses.value.has(a.activityId)).length,
  },
])

// 过滤活动
const filteredActivities = computed(() => {
  switch (activeFilter.value) {
    case 'active':
      return activities.value.filter((activity) => isActivityActive(activity))
    case 'participated':
      return activities.value.filter((activity) =>
        participationStatuses.value.has(activity.activityId),
      )
    default:
      return activities.value
  }
})

// 活动状态判断
const isActivityActive = (activity: Activity) => {
  const now = new Date()
  const start = new Date(activity.startTime)
  const end = new Date(activity.endTime)
  return now >= start && now <= end
}

const isActivityUpcoming = (activity: Activity) => {
  const now = new Date()
  const start = new Date(activity.startTime)
  return now < start
}

const isActivityExpired = (activity: Activity) => {
  const now = new Date()
  const end = new Date(activity.endTime)
  return now > end
}

// 获取活动状态
const getStatusClass = (activity: Activity) => {
  if (isActivityUpcoming(activity)) return 'upcoming'
  if (isActivityActive(activity)) return 'active'
  return 'expired'
}

const getStatusText = (activity: Activity) => {
  if (isActivityUpcoming(activity)) return '即将开始'
  if (isActivityActive(activity)) return '进行中'
  return '已结束'
}

// 用户参与状态
const getUserParticipationStatus = (activity: Activity) => {
  return participationStatuses.value.get(activity.activityId)
}

const getUserStatusClass = (activity: Activity) => {
  const status = getUserParticipationStatus(activity)
  if (!status) return ''

  switch (status.status) {
    case ParticipantStatus.InProgress:
      return 'in-progress'
    case ParticipantStatus.Completed:
      return 'completed'
    default:
      return ''
  }
}

const getUserStatusIcon = (activity: Activity) => {
  const status = getUserParticipationStatus(activity)
  if (!status) return ''

  switch (status.status) {
    case ParticipantStatus.InProgress:
      return '⏳'
    case ParticipantStatus.Completed:
      return '✅'
    default:
      return ''
  }
}

const getUserStatusText = (activity: Activity) => {
  const status = getUserParticipationStatus(activity)
  if (!status) return ''

  switch (status.status) {
    case ParticipantStatus.InProgress:
      return '进行中'
    case ParticipantStatus.Completed:
      return status.rewardStatus === RewardStatus.Claimed ? '已完成·已领奖' : '已完成'
    default:
      return ''
  }
}

// 操作权限判断
const canJoinActivity = (activity: Activity) => {
  return (
    (isActivityActive(activity) || isActivityUpcoming(activity)) &&
    !participationStatuses.value.has(activity.activityId)
  )
}

const canCompleteActivity = (activity: Activity) => {
  const status = getUserParticipationStatus(activity)
  return status && status.status === ParticipantStatus.InProgress && isActivityActive(activity)
}

const canClaimReward = (activity: Activity) => {
  const status = getUserParticipationStatus(activity)
  return (
    status &&
    status.status === ParticipantStatus.Completed &&
    status.rewardStatus === RewardStatus.NotClaimed &&
    activity.reward
  )
}

// 时间格式化
const formatTime = (timeStr: string) => {
  return new Date(timeStr).toLocaleString('zh-CN', {
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
  })
}

// 默认图片
const getDefaultImage = () => {
  return 'https://placehold.co/400x200/f0f2f5/86909c?text=活动图片'
}

// 空状态文案
const getEmptyStateTitle = () => {
  switch (activeFilter.value) {
    case 'active':
      return '暂无进行中的活动'
    case 'participated':
      return '您还没有参与任何活动'
    default:
      return '暂无活动'
  }
}

const getEmptyStateDescription = () => {
  switch (activeFilter.value) {
    case 'active':
      return '当前没有正在进行的活动，请关注最新动态'
    case 'participated':
      return '快去参加感兴趣的活动吧！'
    default:
      return '还没有创建任何活动'
  }
}

// 设置筛选器
const setActiveFilter = (filter: 'all' | 'active' | 'participated') => {
  activeFilter.value = filter
}

// 加载活动列表
const loadActivities = async () => {
  loading.value = true
  error.value = ''

  try {
    const response = await activityApi.getActivitiesByCircleId(props.circleId)
    activities.value = response.data || []

    // 并行加载所有活动的参与状态
    await loadAllParticipationStatuses()
  } catch (err: any) {
    console.error('加载活动失败:', err)
    error.value = err.message || '加载失败，请重试'
  } finally {
    loading.value = false
  }
}

// 加载所有参与状态
const loadAllParticipationStatuses = async () => {
  const promises = activities.value.map(async (activity) => {
    try {
      const response = await activityApi.getUserParticipationStatus(
        props.circleId,
        activity.activityId,
      )
      if (response.data) {
        participationStatuses.value.set(activity.activityId, response.data)
      }
    } catch (error) {
      // 忽略单个活动的加载错误
      console.warn(`加载活动 ${activity.activityId} 参与状态失败:`, error)
    }
  })

  await Promise.allSettled(promises)
}

// 点击活动卡片
const handleActivityClick = (activity: Activity) => {
  selectedActivity.value = activity
  showActivityDetail.value = true
}

// 参加活动 - 修改为直接显示心得表单，不调用API
const handleJoinActivity = async (activity: Activity) => {
  console.log('点击参加活动，直接显示心得表单')

  // 直接显示参与心得表单，不调用joinActivity API
  selectedActivity.value = activity
  showParticipationForm.value = true
}

// 完成活动
const handleCompleteActivity = async (activity: Activity) => {
  isCompleting.value[activity.activityId] = true

  try {
    await activityApi.completeActivity(props.circleId, activity.activityId)

    // 重新加载参与状态
    const response = await activityApi.getUserParticipationStatus(
      props.circleId,
      activity.activityId,
    )
    if (response.data) {
      participationStatuses.value.set(activity.activityId, response.data)
    }

    alert('活动完成成功！')
  } catch (error) {
    console.error('完成活动失败:', error)
    alert('完成活动失败，请重试')
  } finally {
    isCompleting.value[activity.activityId] = false
  }
}

// 领取奖励
const handleClaimReward = async (activity: Activity) => {
  isClaiming.value[activity.activityId] = true

  try {
    await activityApi.claimReward(props.circleId, activity.activityId)

    // 重新加载参与状态
    const response = await activityApi.getUserParticipationStatus(
      props.circleId,
      activity.activityId,
    )
    if (response.data) {
      participationStatuses.value.set(activity.activityId, response.data)
    }

    alert('奖励领取成功！')
  } catch (error) {
    console.error('领取奖励失败:', error)
    alert('领取奖励失败，请重试')
  } finally {
    isClaiming.value[activity.activityId] = false
  }
}

// 处理已参加活动点击
const handleAlreadyJoinedClick = () => {
  alert('您已参加过此活动，无法重复参加')
}

// 处理活动更新
const handleActivityUpdated = (updatedActivity: Activity) => {
  const index = activities.value.findIndex((a) => a.activityId === updatedActivity.activityId)
  if (index !== -1) {
    activities.value[index] = updatedActivity
  }
}

// 处理参与心得提交
const handleParticipationSubmitted = async () => {
  showParticipationForm.value = false

  // 重新加载参与状态
  if (selectedActivity.value) {
    const response = await activityApi.getUserParticipationStatus(
      props.circleId,
      selectedActivity.value.activityId,
    )
    if (response.data) {
      participationStatuses.value.set(selectedActivity.value.activityId, response.data)
    }
  }
}

// 显示创建活动表单
const showCreateActivity = () => {
  showCreateActivityForm.value = true
}

// 处理活动创建完成
const handleActivityCreated = () => {
  showCreateActivityForm.value = false
  loadActivities() // 重新加载活动列表
}

// 监听圈子ID变化
watch(
  () => props.circleId,
  () => {
    if (props.circleId) {
      loadActivities()
    }
  },
  { immediate: true },
)

onMounted(() => {
  if (props.circleId) {
    loadActivities()
  }
})

// 暴露方法给父组件
defineExpose({
  loadActivities,
  activeFilter,
  setActiveFilter,
})

// 在原有的 defineEmits 中添加
interface Emits {
  (e: 'stats-updated', stats: ActivityStats): void
}

const emit = defineEmits<Emits>()

// 添加统计数据计算
const activityStats = computed(() => {
  const total = activities.value.length
  const active = activities.value.filter((a) => isActivityActive(a)).length
  const participated = activities.value.filter((a) =>
    participationStatuses.value.has(a.activityId),
  ).length

  return {
    total,
    active,
    participated,
  }
})

// 监听统计数据变化并发送给父组件
watch(
  activityStats,
  (stats) => {
    emit('stats-updated', stats)
  },
  { immediate: true },
)

// 添加接口类型
interface ActivityStats {
  total: number
  active: number
  participated: number
}
</script>

<style scoped>
.activity-list-container {
  background: #1e293b; /* slate-800 */
  border-radius: 12px;
  overflow: hidden;
  border: 1px solid #334155; /* slate-700 */
}

.loading-state,
.error-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  padding: 60px 20px;
  color: #64748b; /* slate-500 */
}

.loading-spinner {
  width: 32px;
  height: 32px;
  border: 3px solid #334155; /* slate-700 */
  border-top: 3px solid #0ea5e9; /* sky-500 */
  border-radius: 50%;
  animation: spin 1s linear infinite;
  margin-bottom: 16px;
}

@keyframes spin {
  0% {
    transform: rotate(0deg);
  }
  100% {
    transform: rotate(360deg);
  }
}

.activity-content {
  padding: 24px;
  background: #1e293b; /* slate-800 */
}

.activity-filters {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 24px;
  padding-bottom: 16px;
  border-bottom: 1px solid #334155; /* slate-700 */
}

.filter-left {
  display: flex;
  gap: 8px;
}

.filter-right {
  display: flex;
  gap: 8px;
}

.btn-create {
  padding: 8px 16px;
  font-size: 14px;
  border-radius: 6px;
  white-space: nowrap;
}

.btn-secondary {
  background: #334155; /* slate-700 */
  color: #cbd5e1; /* slate-300 */
  border: 1px solid #475569; /* slate-600 */
}

.btn-secondary:hover {
  background: #475569; /* slate-600 */
  border-color: #64748b; /* slate-500 */
}

.filter-btn {
  padding: 8px 16px;
  border: 1px solid #334155; /* slate-700 */
  background: #1e293b; /* slate-800 */
  border-radius: 20px;
  cursor: pointer;
  transition: all 0.2s;
  font-size: 14px;
  color: #cbd5e1; /* slate-300 */
}

.filter-btn:hover {
  border-color: #0ea5e9; /* sky-500 */
  color: #38bdf8; /* sky-400 */
}

.filter-btn.active {
  background: #0ea5e9; /* sky-500 */
  border-color: #0ea5e9; /* sky-500 */
  color: #fff;
}

.filter-count {
  margin-left: 4px;
  font-size: 12px;
}

.activities-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(350px, 1fr));
  gap: 20px;
}

.empty-state {
  grid-column: 1 / -1;
  text-align: center;
  padding: 60px 20px;
  color: #64748b; /* slate-500 */
}

.empty-icon {
  font-size: 48px;
  margin-bottom: 16px;
}

.empty-state h3 {
  color: #cbd5e1; /* slate-300 */
  margin: 0 0 8px 0;
}

.activity-card {
  border: 1px solid #334155; /* slate-700 */
  border-radius: 12px;
  overflow: hidden;
  cursor: pointer;
  transition: all 0.2s;
  background: #0f172a; /* slate-900 */
}

.activity-card:hover {
  box-shadow: 0 4px 12px rgba(14, 165, 233, 0.1);
  border-color: #0ea5e9; /* sky-500 */
}

.activity-image {
  position: relative;
  height: 160px;
  overflow: hidden;
  background: #334155; /* slate-700 */
}

.activity-image img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.activity-status-overlay {
  position: absolute;
  top: 12px;
  right: 12px;
}

.status-badge {
  padding: 4px 8px;
  border-radius: 12px;
  font-size: 12px;
  font-weight: 500;
  background: rgba(15, 23, 42, 0.9); /* slate-900 with opacity */
  backdrop-filter: blur(4px);
  border: 1px solid #334155; /* slate-700 */
}

.status-badge.upcoming {
  color: #fbbf24; /* amber-400 */
}

.status-badge.active {
  color: #86efac; /* green-300 */
}

.status-badge.expired {
  color: #9ca3af; /* gray-400 */
}

.activity-info {
  padding: 16px;
}

.activity-title {
  font-size: 16px;
  font-weight: 600;
  color: #f1f5f9; /* slate-100 */
  margin: 0 0 8px 0;
  line-height: 1.4;
  overflow: hidden;
  text-overflow: ellipsis;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
}

.activity-description {
  color: #64748b; /* slate-500 */
  font-size: 14px;
  line-height: 1.5;
  margin: 0 0 12px 0;
  overflow: hidden;
  text-overflow: ellipsis;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
}

.activity-time {
  margin-bottom: 12px;
}

.time-item {
  display: flex;
  justify-content: space-between;
  font-size: 12px;
  margin-bottom: 4px;
}

.time-label {
  color: #64748b; /* slate-500 */
}

.time-value {
  color: #cbd5e1; /* slate-300 */
  font-weight: 500;
}

.activity-reward {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 8px 12px;
  background: #1f2937; /* gray-800 */
  border-radius: 6px;
  margin-bottom: 12px;
  border: 1px solid #374151; /* gray-700 */
}

.reward-icon {
  font-size: 14px;
}

.reward-text {
  color: #fbbf24; /* amber-400 */
  font-size: 13px;
  font-weight: 500;
  flex: 1;
  overflow: hidden;
  text-overflow: ellipsis;
  white-space: nowrap;
}

.participation-status {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 16px;
  padding-top: 8px;
  border-top: 1px solid #334155; /* slate-700 */
}

.participant-count {
  display: flex;
  align-items: center;
  gap: 4px;
  font-size: 12px;
  color: #64748b; /* slate-500 */
}

.count-icon {
  font-size: 14px;
}

.user-status {
  display: flex;
  align-items: center;
  gap: 4px;
  font-size: 12px;
}

.status-icon {
  font-size: 14px;
}

.status-icon.in-progress {
  color: #38bdf8; /* sky-400 */
}

.status-icon.completed {
  color: #86efac; /* green-300 */
}

.status-text {
  color: #cbd5e1; /* slate-300 */
  font-weight: 500;
}

.activity-actions {
  display: flex;
  gap: 8px;
}

.btn {
  padding: 6px 12px;
  border: none;
  border-radius: 6px;
  font-size: 12px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s;
  flex: 1;
}

.btn-sm {
  font-size: 12px;
  padding: 6px 12px;
}

.btn-primary {
  background: #0ea5e9; /* sky-500 */
  color: #fff;
}

.btn-primary:hover:not(:disabled) {
  background: #0284c7; /* sky-600 */
}

.btn-success {
  background: #10b981; /* emerald-500 */
  color: #fff;
}

.btn-success:hover:not(:disabled) {
  background: #059669; /* emerald-600 */
}

.btn-reward {
  background: #f59e0b; /* amber-500 */
  color: #fff;
}

.btn-reward:hover:not(:disabled) {
  background: #d97706; /* amber-600 */
}

.btn-outline {
  background: #1e293b; /* slate-800 */
  color: #cbd5e1; /* slate-300 */
  border: 1px solid #334155; /* slate-700 */
}

.btn-outline:hover {
  border-color: #0ea5e9; /* sky-500 */
  color: #38bdf8; /* sky-400 */
}

.btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .activities-grid {
    grid-template-columns: 1fr;
  }

  .activity-content {
    padding: 16px;
  }

  .activity-filters {
    flex-wrap: wrap;
  }
}
</style>
