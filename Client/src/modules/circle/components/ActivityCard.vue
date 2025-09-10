<template>
  <div class="activity-card">
    <!-- 卡片头部 -->
    <div class="card-header">
      <div class="status-and-actions">
        <span :class="['status-badge', statusBadgeClass]">
          {{ statusText }}
        </span>
        <div class="card-actions" v-if="canManage">
          <button @click="$emit('edit', activity)" class="action-btn edit-btn">
            <EditIcon class="w-4 h-4" />
          </button>
          <button @click="$emit('delete', activity)" class="action-btn delete-btn">
            <TrashIcon class="w-4 h-4" />
          </button>
        </div>
      </div>
      <h3 class="activity-title">{{ activity.title }}</h3>
    </div>

    <!-- 卡片主体 -->
    <div class="card-body">
      <!-- 活动描述 -->
      <p class="activity-description" v-if="activity.description">
        {{ activity.description }}
      </p>

      <!-- 活动时间 -->
      <div class="activity-time">
        <div class="time-item">
          <CalendarIcon class="time-icon" />
          <span>开始：{{ formatDateTime(activity.startTime) }}</span>
        </div>
        <div class="time-item">
          <CalendarIcon class="time-icon" />
          <span>结束：{{ formatDateTime(activity.endTime) }}</span>
        </div>
      </div>

      <!-- 奖励信息 -->
      <div class="activity-reward" v-if="activity.reward">
        <GiftIcon class="reward-icon" />
        <span class="reward-text">{{ activity.reward }}</span>
      </div>
    </div>

    <!-- 卡片底部操作 -->
    <div class="card-footer">
      <button
        v-if="activityStatus.canJoin"
        @click="handleJoinActivity"
        :disabled="isLoading"
        :class="['action-button', 'btn-join', { loading: isLoading }]"
      >
        {{ isLoading ? '' : '参加活动' }}
      </button>

      <button
        v-else-if="activityStatus.canComplete"
        @click="handleCompleteActivity"
        :disabled="isLoading"
        :class="['action-button', 'btn-complete', { loading: isLoading }]"
      >
        {{ isLoading ? '' : '完成任务' }}
      </button>

      <button
        v-else-if="activityStatus.canClaimReward"
        @click="handleClaimReward"
        :disabled="isLoading"
        :class="['action-button', 'btn-claim', { loading: isLoading }]"
      >
        {{ isLoading ? '' : '领取奖励' }}
      </button>

      <button v-else-if="activityStatus.hasJoined" class="action-button btn-joined" disabled>
        已参加
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue'
import { CalendarIcon, GiftIcon, EditIcon, TrashIcon } from 'lucide-vue-next'
import { Activity, ActivityStatus } from '../types'
import { activityApi } from '../api'

// 定义响应式变量
const loading = ref(false)

interface Props {
  activity: Activity
  canManage?: boolean
  participantStatus?: any
  circleId: number
}

interface Emits {
  (e: 'edit', activity: Activity): void
  (e: 'delete', activity: Activity): void
  (e: 'statusChanged'): void
  (e: 'join-activity', activity: Activity): void
}

const props = defineProps<Props>()
const emit = defineEmits<Emits>()

const isLoading = ref(false)

// 计算活动状态
const activityStatus = computed((): ActivityStatus => {
  const now = new Date()
  const startTime = new Date(props.activity.startTime)
  const endTime = new Date(props.activity.endTime)

  const isActive = now >= startTime && now <= endTime
  const isUpcoming = now < startTime
  const isExpired = now > endTime

  const hasJoined = !!props.participantStatus
  const isCompleted = props.participantStatus?.status === 'Completed'
  const rewardClaimed = props.participantStatus?.rewardStatus === 'Claimed'

  return {
    isActive,
    isUpcoming,
    isExpired,
    canJoin: isActive && !hasJoined,
    hasJoined,
    canComplete: isActive && hasJoined && !isCompleted,
    canClaimReward: hasJoined && isCompleted && !rewardClaimed && props.activity.reward,
  }
})

// 状态徽章样式
const statusBadgeClass = computed(() => {
  if (activityStatus.value.isUpcoming) return 'bg-blue-100 text-blue-800'
  if (activityStatus.value.isActive) return 'bg-green-100 text-green-800'
  if (activityStatus.value.isExpired) return 'bg-gray-100 text-gray-800'
  return 'bg-gray-100 text-gray-800'
})

// 状态文本
const statusText = computed(() => {
  if (activityStatus.value.isUpcoming) return '即将开始'
  if (activityStatus.value.isActive) return '进行中'
  if (activityStatus.value.isExpired) return '已结束'
  return ''
})

// 格式化日期时间
const formatDateTime = (dateStr: string) => {
  return new Date(dateStr).toLocaleString('zh-CN')
}

// 处理参加活动
const handleJoinActivity = async () => {
  console.log('ActivityCard: 触发参加活动事件')

  if (!props.activity) {
    console.error('activity 对象为空')
    alert('活动信息缺失')
    return
  }

  emit('join-activity', props.activity)
}

// 处理完成活动
const handleCompleteActivity = async () => {
  console.log('=== 开始完成活动调试 ===')
  console.log('活动信息:', props.activity)
  console.log('圈子ID:', props.circleId)

  if (!props.activity) {
    console.error('活动信息为空')
    return
  }

  loading.value = true
  try {
    console.log('调用 API:', `completeActivity(${props.circleId}, ${props.activity.activityId})`)
    const result = await activityApi.completeActivity(props.circleId, props.activity.activityId)
    console.log('完成活动API响应成功:', result)
    emit('statusChanged')
  } catch (error: any) {
    console.error('=== 完成活动失败详情 ===')
    console.error('错误对象:', error)
    console.error('响应状态:', error.response?.status)
    console.error('响应数据:', error.response?.data)
    console.error('请求URL:', error.config?.url)

    let errorMessage = '完成活动失败'
    if (error.response?.data?.msg) {
      errorMessage = error.response.data.msg
    } else if (error.response?.data?.message) {
      errorMessage = error.response.data.message
    } else if (typeof error.response?.data === 'string') {
      errorMessage = error.response.data
    }
    alert(errorMessage)
  } finally {
    loading.value = false
  }
}

// 处理领取奖励
const handleClaimReward = async () => {
  console.log('=== 开始领取奖励调试 ===')
  console.log('活动信息:', props.activity)
  console.log('圈子ID:', props.circleId)

  if (!props.activity) {
    console.error('活动信息为空')
    return
  }

  loading.value = true
  try {
    console.log('调用 API:', `claimReward(${props.circleId}, ${props.activity.activityId})`)
    const result = await activityApi.claimReward(props.circleId, props.activity.activityId)
    console.log('领取奖励API响应成功:', result)
    emit('statusChanged')
  } catch (error: any) {
    console.error('=== 领取奖励失败详情 ===')
    console.error('错误对象:', error)
    console.error('响应状态:', error.response?.status)
    console.error('响应数据:', error.response?.data)
    console.error('请求URL:', error.config?.url)

    let errorMessage = '领取奖励失败'
    if (error.response?.data?.msg) {
      errorMessage = error.response.data.msg
    } else if (error.response?.data?.message) {
      errorMessage = error.response.data.message
    } else if (typeof error.response?.data === 'string') {
      errorMessage = error.response.data
    }
    alert(errorMessage)
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.activity-card {
  background: white;
  border-radius: 12px;
  border: 1px solid #e4e6ea;
  overflow: hidden;
  transition: all 0.3s ease;
  position: relative;
}

.activity-card:hover {
  transform: translateY(-4px);
  box-shadow: 0 8px 25px rgba(0, 0, 0, 0.1);
  border-color: #1677ff;
}

.card-header {
  padding: 20px 20px 16px 20px;
  border-bottom: 1px solid #f5f5f5;
}

.status-and-actions {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 12px;
}

.status-badge {
  padding: 4px 12px;
  border-radius: 16px;
  font-size: 12px;
  font-weight: 600;
  text-transform: uppercase;
  letter-spacing: 0.5px;
}

.status-upcoming {
  background: linear-gradient(135deg, #e6f7ff, #bae7ff);
  color: #1677ff;
}

.status-active {
  background: linear-gradient(135deg, #f6ffed, #d9f7be);
  color: #52c41a;
}

.status-expired {
  background: linear-gradient(135deg, #f5f5f5, #e8e8e8);
  color: #8c8c8c;
}

.card-actions {
  display: flex;
  gap: 8px;
  opacity: 0;
  transition: opacity 0.2s;
}

.activity-card:hover .card-actions {
  opacity: 1;
}

.action-btn {
  width: 32px;
  height: 32px;
  border: none;
  border-radius: 6px;
  display: flex;
  align-items: center;
  justify-content: center;
  cursor: pointer;
  transition: all 0.2s;
}

.edit-btn {
  background: #f0f8ff;
  color: #1677ff;
}

.edit-btn:hover {
  background: #1677ff;
  color: white;
}

.delete-btn {
  background: #fff2f0;
  color: #ff4d4f;
}

.delete-btn:hover {
  background: #ff4d4f;
  color: white;
}

.activity-title {
  font-size: 18px;
  font-weight: 700;
  color: #1d2129;
  margin: 0;
  line-height: 1.4;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.card-body {
  padding: 16px 20px;
}

.activity-description {
  color: #4e5969;
  font-size: 14px;
  line-height: 1.6;
  margin-bottom: 16px;
  display: -webkit-box;
  -webkit-line-clamp: 3;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.activity-time {
  margin-bottom: 16px;
}

.time-item {
  display: flex;
  align-items: center;
  font-size: 13px;
  color: #86909c;
  margin-bottom: 8px;
}

.time-item:last-child {
  margin-bottom: 0;
}

.time-icon {
  width: 16px;
  height: 16px;
  margin-right: 8px;
  flex-shrink: 0;
}

.activity-reward {
  display: flex;
  align-items: center;
  background: linear-gradient(135deg, #fff7e6, #ffe58f);
  padding: 12px;
  border-radius: 8px;
  margin-bottom: 16px;
  border: 1px solid #ffd666;
}

.reward-icon {
  width: 18px;
  height: 18px;
  margin-right: 8px;
  color: #d48806;
}

.reward-text {
  font-size: 14px;
  font-weight: 500;
  color: #ad6800;
}

.card-footer {
  padding: 16px 20px;
  background: #fafafa;
  border-top: 1px solid #f0f0f0;
}

.action-button {
  width: 100%;
  border: none;
  border-radius: 8px;
  padding: 12px;
  font-weight: 600;
  font-size: 14px;
  cursor: pointer;
  transition: all 0.3s ease;
  position: relative;
  overflow: hidden;
}

.action-button:disabled {
  opacity: 0.6;
  cursor: not-allowed;
}

.btn-join {
  background: linear-gradient(135deg, #1677ff, #0958d9);
  color: white;
}

.btn-join:hover:not(:disabled) {
  background: linear-gradient(135deg, #0958d9, #003eb3);
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(22, 119, 255, 0.3);
}

.btn-complete {
  background: linear-gradient(135deg, #52c41a, #389e0d);
  color: white;
}

.btn-complete:hover:not(:disabled) {
  background: linear-gradient(135deg, #389e0d, #237804);
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(82, 196, 26, 0.3);
}

.btn-claim {
  background: linear-gradient(135deg, #faad14, #d48806);
  color: white;
}

.btn-claim:hover:not(:disabled) {
  background: linear-gradient(135deg, #d48806, #ad6800);
  transform: translateY(-2px);
  box-shadow: 0 4px 12px rgba(250, 173, 20, 0.3);
}

.btn-joined {
  background: #f5f5f5;
  color: #8c8c8c;
  cursor: default;
}

/* 加载动画 */
.action-button.loading::after {
  content: '';
  position: absolute;
  width: 16px;
  height: 16px;
  margin: auto;
  border: 2px solid transparent;
  border-top-color: currentColor;
  border-radius: 50%;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  0% {
    transform: rotate(0deg);
  }
  100% {
    transform: rotate(360deg);
  }
}
</style>
