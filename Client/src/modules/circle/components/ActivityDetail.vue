<template>
  <div
    class="fixed inset-0 bg-black bg-opacity-60 flex items-center justify-center p-4 z-50"
    style="transform: translateY(50px)"
  >
    <div class="bg-slate-900 rounded-2xl shadow-2xl w-full max-w-4xl max-h-[90vh] overflow-hidden">
      <!-- 加载状态 -->
      <div v-if="loading" class="loading-container">
        <div class="loading-spinner"></div>
        <p>正在加载活动详情...</p>
      </div>

      <!-- 活动详情内容 -->
      <div v-else class="activity-detail-container">
        <!-- 头部 -->
        <div class="detail-header">
          <button @click="$emit('close')" class="close-btn">←</button>
          <h1 class="activity-title">{{ activity.title }}</h1>
          <div class="activity-status">
            <span class="status-badge" :class="statusClass">{{ statusText }}</span>
          </div>
        </div>

        <!-- 活动图片 -->
        <div v-if="activity.imageUrl" class="activity-image">
          <img :src="processedImageUrl" :alt="activity.title" />
        </div>

        <!-- 添加这个默认图片占位符 -->
        <div v-else class="default-image">
          <div class="default-image-content">
            <svg
              class="w-16 h-16 text-slate-600"
              fill="none"
              stroke="currentColor"
              viewBox="0 0 24 24"
            >
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M19 11H5m14 0a2 2 0 012 2v6a2 2 0 01-2 2H5a2 2 0 01-2-2v-6a2 2 0 012-2m14 0V9a2 2 0 00-2-2M5 11V9a2 2 0 012-2m0 0V5a2 2 0 012-2h6a2 2 0 012 2v2M7 7h10"
              ></path>
            </svg>
            <p class="text-slate-500 mt-2">活动图片</p>
          </div>
        </div>

        <!-- 活动信息 -->
        <div class="activity-content">
          <div class="content-grid">
            <!-- 左侧信息 -->
            <div class="activity-info">
              <div class="info-section">
                <h3>活动描述</h3>
                <p class="description">{{ activity.description || '暂无描述' }}</p>
              </div>

              <div class="info-section" v-if="activity.reward">
                <h3>活动奖励</h3>
                <p class="reward">{{ activity.reward }}</p>
              </div>

              <div class="info-section">
                <h3>活动时间</h3>
                <div class="time-info">
                  <p><strong>开始时间：</strong>{{ formatTime(activity.startTime) }}</p>
                  <p><strong>结束时间：</strong>{{ formatTime(activity.endTime) }}</p>
                  <p><strong>活动时长：</strong>{{ calculateDuration() }}</p>
                </div>
              </div>
            </div>

            <!-- 右侧操作区域 -->
            <div class="activity-actions">
              <div class="action-card">
                <h3>参与活动</h3>

                <!-- 未参加状态 -->
                <div v-if="!participationStatus" class="not-joined">
                  <p class="action-description">点击下方按钮参加此活动</p>
                  <button
                    @click="handleJoinActivity"
                    :disabled="!canJoinActivity || isJoining"
                    class="btn btn-primary action-btn"
                  >
                    {{ isJoining ? '参加中...' : '参加活动' }}
                  </button>
                  <p v-if="!canJoinActivity" class="warning-text">
                    {{ !isActive ? '活动已结束' : '活动尚未开始' }}
                  </p>
                </div>

                <!-- 已参加状态 -->
                <div v-else class="joined">
                  <div class="participation-info">
                    <p><strong>参加时间：</strong>{{ formatTime(participationStatus.joinTime) }}</p>
                    <p>
                      <strong>当前状态：</strong>
                      <span class="status-text" :class="participationStatusClass">
                        {{ participationStatusText }}
                      </span>
                    </p>
                  </div>

                  <!-- 完成活动按钮 -->
                  <button
                    v-if="canCompleteActivity"
                    @click="handleCompleteActivity"
                    :disabled="isCompleting"
                    class="btn btn-success action-btn"
                  >
                    {{ isCompleting ? '完成中...' : '完成活动' }}
                  </button>

                  <!-- 领取奖励按钮 -->
                  <button
                    v-if="canClaimReward"
                    @click="handleClaimReward"
                    :disabled="isClaiming"
                    class="btn btn-reward action-btn"
                  >
                    {{ isClaiming ? '领取中...' : '领取奖励' }}
                  </button>

                  <!-- 已完成状态 -->
                  <div v-if="isActivityCompleted" class="completed-status">
                    <div class="success-icon">✓</div>
                    <p>活动已完成！</p>
                    <p v-if="isRewardClaimed" class="reward-claimed">奖励已领取</p>
                  </div>
                </div>
              </div>

              <!-- 活动统计 -->
              <div class="stats-card">
                <h3>活动统计</h3>
                <div class="stats-grid">
                  <div class="stat-item">
                    <span class="stat-number">{{ activity.participantCount || 0 }}</span>
                    <span class="stat-label">参与人数</span>
                  </div>
                  <div class="stat-item">
                    <span class="stat-number">{{ activity.completedCount || 0 }}</span>
                    <span class="stat-label">完成人数</span>
                  </div>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- 参与心得弹窗 -->
    <ActivityParticipation
      v-if="showParticipationForm"
      :activity="activity"
      @close="showParticipationForm = false"
      @submitted="handleParticipationSubmitted"
    />
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { Activity, ActivityParticipant, ParticipantStatus, RewardStatus } from '../types'
import { activityApi, getProxiedImageUrl } from '../api'
import ActivityParticipation from './ActivityParticipation.vue'

interface Props {
  activity: Activity
  circleId: number
}

interface Emits {
  (e: 'close'): void
  (e: 'activity-updated', activity: Activity): void
}

const props = defineProps<Props>()
const emit = defineEmits<Emits>()

const loading = ref(false)
const participationStatus = ref<ActivityParticipant | null>(null)
const processedImageUrl = ref('')
const showParticipationForm = ref(false)

// 操作状态
const isJoining = ref(false)
const isCompleting = ref(false)
const isClaiming = ref(false)

// 活动状态计算
const now = computed(() => new Date())
const startTime = computed(() => new Date(props.activity.startTime))
const endTime = computed(() => new Date(props.activity.endTime))

const isUpcoming = computed(() => now.value < startTime.value)
const isActive = computed(() => now.value >= startTime.value && now.value <= endTime.value)
const isExpired = computed(() => now.value > endTime.value)

const statusClass = computed(() => {
  if (isUpcoming.value) return 'upcoming'
  if (isActive.value) return 'active'
  return 'expired'
})

const statusText = computed(() => {
  if (isUpcoming.value) return '即将开始'
  if (isActive.value) return '进行中'
  return '已结束'
})

// 参与状态计算
const canJoinActivity = computed(() => {
  return (isActive.value || isUpcoming.value) && !participationStatus.value
})

const canCompleteActivity = computed(() => {
  return (
    participationStatus.value &&
    participationStatus.value.status === ParticipantStatus.InProgress &&
    isActive.value
  )
})

const canClaimReward = computed(() => {
  return (
    participationStatus.value &&
    participationStatus.value.status === ParticipantStatus.Completed &&
    participationStatus.value.rewardStatus === RewardStatus.NotClaimed &&
    props.activity.reward
  )
})

const isActivityCompleted = computed(() => {
  return (
    participationStatus.value && participationStatus.value.status === ParticipantStatus.Completed
  )
})

const isRewardClaimed = computed(() => {
  return (
    participationStatus.value && participationStatus.value.rewardStatus === RewardStatus.Claimed
  )
})

const participationStatusClass = computed(() => {
  if (!participationStatus.value) return ''

  switch (participationStatus.value.status) {
    case ParticipantStatus.InProgress:
      return 'in-progress'
    case ParticipantStatus.Completed:
      return 'completed'
    default:
      return ''
  }
})

const participationStatusText = computed(() => {
  if (!participationStatus.value) return '未参加'

  switch (participationStatus.value.status) {
    case ParticipantStatus.InProgress:
      return '进行中'
    case ParticipantStatus.Completed:
      return '已完成'
    default:
      return '未知状态'
  }
})

// 时间格式化
const formatTime = (timeStr: string) => {
  return new Date(timeStr).toLocaleString('zh-CN', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
  })
}

// 计算活动时长
const calculateDuration = () => {
  const diffMs = endTime.value.getTime() - startTime.value.getTime()
  const hours = Math.floor(diffMs / (1000 * 60 * 60))
  const minutes = Math.floor((diffMs % (1000 * 60 * 60)) / (1000 * 60))

  if (hours > 0) {
    return minutes > 0 ? `${hours}小时${minutes}分钟` : `${hours}小时`
  } else {
    return `${minutes}分钟`
  }
}

// 处理图片URL
const processImageUrl = async () => {
  if (props.activity.imageUrl) {
    processedImageUrl.value = await getProxiedImageUrl(props.activity.imageUrl)
  }
}

// 加载参与状态
const loadParticipationStatus = async () => {
  try {
    const response = await activityApi.getUserParticipationStatus(
      props.circleId,
      props.activity.activityId,
    )
    participationStatus.value = response.data
  } catch (error) {
    console.error('加载参与状态失败:', error)
  }
}

// 参加活动
const handleJoinActivity = async () => {
  isJoining.value = true
  try {
    await activityApi.joinActivity(props.circleId, props.activity.activityId)
    showParticipationForm.value = true
  } catch (error) {
    console.error('参加活动失败:', error)
    alert('参加活动失败，请重试')
  } finally {
    isJoining.value = false
  }
}

// 完成活动
const handleCompleteActivity = async () => {
  isCompleting.value = true
  try {
    await activityApi.completeActivity(props.circleId, props.activity.activityId)
    await loadParticipationStatus()
    alert('活动完成成功！')
  } catch (error) {
    console.error('完成活动失败:', error)
    alert('完成活动失败，请重试')
  } finally {
    isCompleting.value = false
  }
}

// 领取奖励
const handleClaimReward = async () => {
  isClaiming.value = true
  try {
    await activityApi.claimReward(props.circleId, props.activity.activityId)
    await loadParticipationStatus()
    alert('奖励领取成功！')
  } catch (error) {
    console.error('领取奖励失败:', error)
    alert('领取奖励失败，请重试')
  } finally {
    isClaiming.value = false
  }
}

// 处理参与心得提交
const handleParticipationSubmitted = async () => {
  showParticipationForm.value = false
  await loadParticipationStatus()
}

onMounted(async () => {
  loading.value = true
  try {
    await Promise.all([processImageUrl(), loadParticipationStatus()])
  } finally {
    loading.value = false
  }
})
</script>

<style scoped>
/* 活动详情样式 */
<style scoped>
/* 在现有样式基础上添加或替换这些 */
.activity-detail-container {
  background: #0f172a; /* slate-900 */
  border-radius: 16px;
  overflow: hidden;
  color: #e2e8f0; /* slate-200 */
}

.detail-header {
  display: flex;
  align-items: center;
  padding: 20px 24px;
  border-bottom: 1px solid #334155; /* slate-700 */
  background: #1e293b; /* slate-800 */
}

.close-btn {
  background: none;
  border: none;
  color: #94a3b8; /* slate-400 */
  font-size: 20px;
  cursor: pointer;
  padding: 8px;
  border-radius: 8px;
  transition: background 0.2s;
  margin-right: 16px;
}

.close-btn:hover {
  background: #334155; /* slate-700 */
}

.activity-title {
  flex: 1;
  font-size: 24px;
  font-weight: 600;
  color: #f1f5f9; /* slate-100 */
  margin: 0;
}

.status-badge.upcoming {
  background: #0c4a6e; /* sky-900 */
  color: #7dd3fc; /* sky-300 */
}

.status-badge.active {
  background: #14532d; /* green-900 */
  color: #86efac; /* green-300 */
}

.status-badge.expired {
  background: #374151; /* gray-700 */
  color: #9ca3af; /* gray-400 */
}

/* 默认图片样式 */
.default-image {
  width: 100%;
  height: 300px;
  background: #1e293b; /* slate-800 */
  display: flex;
  align-items: center;
  justify-content: center;
  border-bottom: 1px solid #334155; /* slate-700 */
}

.default-image-content {
  text-align: center;
}

.activity-content {
  padding: 24px;
  background: #0f172a; /* slate-900 */
}

.info-section h3 {
  font-size: 18px;
  font-weight: 600;
  color: #f1f5f9; /* slate-100 */
  margin-bottom: 12px;
}

.description {
  color: #cbd5e1; /* slate-300 */
  line-height: 1.6;
  white-space: pre-wrap;
}

.reward {
  color: #fbbf24; /* amber-400 */
  font-weight: 500;
  background: #1f2937; /* gray-800 */
  padding: 12px 16px;
  border-radius: 8px;
  border-left: 4px solid #fbbf24; /* amber-400 */
}

.time-info p {
  margin: 8px 0;
  color: #cbd5e1; /* slate-300 */
}

.action-card,
.stats-card {
  background: #1e293b; /* slate-800 */
  border: 1px solid #334155; /* slate-700 */
  border-radius: 12px;
  padding: 20px;
  margin-bottom: 20px;
}

.action-card h3,
.stats-card h3 {
  font-size: 16px;
  font-weight: 600;
  color: #f1f5f9; /* slate-100 */
  margin-bottom: 16px;
}

.btn-primary {
  background: #0ea5e9; /* sky-500 */
  color: #fff;
}

.btn-primary:hover:not(:disabled) {
  background: #0284c7; /* sky-600 */
}

.stat-number {
  display: block;
  font-size: 24px;
  font-weight: 600;
  color: #38bdf8; /* sky-400 */
  margin-bottom: 4px;
}

.stat-item {
  text-align: center;
  padding: 16px;
  background: #0f172a; /* slate-900 */
  border-radius: 8px;
  border: 1px solid #334155; /* slate-700 */
}

/* 响应式设计 */
@media (max-width: 768px) {
  .content-grid {
    grid-template-columns: 1fr;
    gap: 20px;
  }

  .activity-content {
    padding: 16px;
  }

  .detail-header {
    padding: 16px;
  }

  .stats-grid {
    grid-template-columns: 1fr;
  }
}
</style>
