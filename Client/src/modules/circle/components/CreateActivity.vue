<template>
  <div class="fixed inset-0 bg-black bg-opacity-60 flex items-center justify-center p-4 z-[9999]">
    <div class="bg-slate-900 rounded-2xl shadow-2xl w-full max-w-3xl max-h-[90vh] overflow-y-auto">
      <div class="create-activity-container">
        <!-- 页面头部 -->
        <div class="page-header">
          <div class="header-content">
            <div class="header-icon">
              <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"
                ></path>
              </svg>
            </div>
            <h3 class="page-title">
              {{ isEditing ? '编辑活动' : '创建新活动' }}
            </h3>
          </div>
          <button @click="$emit('close')" class="close-btn">
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M6 18L18 6M6 6l12 12"
              ></path>
            </svg>
          </button>
        </div>

        <!-- 表单内容 -->
        <form @submit.prevent="handleSubmit" class="create-form">
          <div class="form-content">
            <!-- 基本信息区域 -->
            <div class="info-section">
              <div class="section-header">
                <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="2"
                    d="M13 16h-1v-4h-1m1-4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
                  ></path>
                </svg>
                <span>基本信息</span>
              </div>

              <!-- 活动标题 -->
              <div class="form-section">
                <label class="form-label">
                  <span class="label-text">活动标题</span>
                  <span class="required-star">*</span>
                </label>
                <div class="input-group">
                  <input
                    v-model="form.title"
                    type="text"
                    required
                    maxlength="200"
                    class="form-input"
                    :class="{ 'input-error': errors.title }"
                    placeholder="为你的活动起一个吸引人的标题..."
                  />
                  <div class="input-footer">
                    <span v-if="errors.title" class="error-message">{{ errors.title }}</span>
                    <span class="char-count" :class="{ 'char-warning': form.title.length > 180 }">
                      {{ form.title.length }}/200
                    </span>
                  </div>
                </div>
              </div>

              <!-- 活动描述 -->
              <div class="form-section">
                <label class="form-label">
                  <span class="label-text">活动描述</span>
                </label>
                <div class="input-group">
                  <textarea
                    v-model="form.description"
                    rows="4"
                    class="form-textarea"
                    :class="{ 'input-error': errors.description }"
                    placeholder="详细描述活动内容、目的、要求和注意事项..."
                  ></textarea>
                  <span v-if="errors.description" class="error-message">{{
                    errors.description
                  }}</span>
                </div>
              </div>
            </div>

            <!-- 时间设置区域 -->
            <div class="time-section">
              <div class="section-header">
                <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                  <path
                    stroke-linecap="round"
                    stroke-linejoin="round"
                    stroke-width="2"
                    d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"
                  ></path>
                </svg>
                <span>时间安排</span>
              </div>

              <div class="time-grid">
                <div class="form-section">
                  <label class="form-label">
                    <span class="label-text">开始时间</span>
                    <span class="required-star">*</span>
                  </label>
                  <div class="input-group">
                    <input
                      v-model="form.startTime"
                      type="datetime-local"
                      required
                      :min="minDateTime"
                      class="form-input time-input"
                      :class="{ 'input-error': errors.startTime }"
                    />
                    <span v-if="errors.startTime" class="error-message">{{
                      errors.startTime
                    }}</span>
                  </div>
                </div>

                <div class="form-section">
                  <label class="form-label">
                    <span class="label-text">结束时间</span>
                    <span class="required-star">*</span>
                  </label>
                  <div class="input-group">
                    <input
                      v-model="form.endTime"
                      type="datetime-local"
                      required
                      :min="form.startTime || minDateTime"
                      class="form-input time-input"
                      :class="{ 'input-error': errors.endTime }"
                    />
                    <span v-if="errors.endTime" class="error-message">{{ errors.endTime }}</span>
                  </div>
                </div>
              </div>

              <!-- 时间预览卡片 -->
              <div v-if="form.startTime && form.endTime" class="time-preview-card">
                <div class="preview-header">
                  <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path
                      stroke-linecap="round"
                      stroke-linejoin="round"
                      stroke-width="2"
                      d="M15 12a3 3 0 11-6 0 3 3 0 016 0z"
                    ></path>
                    <path
                      stroke-linecap="round"
                      stroke-linejoin="round"
                      stroke-width="2"
                      d="M2.458 12C3.732 7.943 7.523 5 12 5c4.478 0 8.268 2.943 9.542 7-1.274 4.057-5.064 7-9.542 7-4.477 0-8.268-2.943-9.542-7z"
                    ></path>
                  </svg>
                  <span>时间预览</span>
                </div>
                <div class="preview-content">
                  <div class="time-item">
                    <div class="time-icon start">
                      <svg class="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path
                          stroke-linecap="round"
                          stroke-linejoin="round"
                          stroke-width="2"
                          d="M12 6v6m0 0v6m0-6h6m-6 0H6"
                        ></path>
                      </svg>
                    </div>
                    <div class="time-info">
                      <span class="time-label">开始时间</span>
                      <span class="time-value">{{ formatDisplayTime(form.startTime) }}</span>
                    </div>
                  </div>

                  <div class="time-arrow">
                    <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                      <path
                        stroke-linecap="round"
                        stroke-linejoin="round"
                        stroke-width="2"
                        d="M13 7l5 5m0 0l-5 5m5-5H6"
                      ></path>
                    </svg>
                  </div>

                  <div class="time-item">
                    <div class="time-icon end">
                      <svg class="w-3 h-3" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                        <path
                          stroke-linecap="round"
                          stroke-linejoin="round"
                          stroke-width="2"
                          d="M5 13l4 4L19 7"
                        ></path>
                      </svg>
                    </div>
                    <div class="time-info">
                      <span class="time-label">结束时间</span>
                      <span class="time-value">{{ formatDisplayTime(form.endTime) }}</span>
                    </div>
                  </div>
                </div>

                <div class="duration-badge">
                  <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                    <path
                      stroke-linecap="round"
                      stroke-linejoin="round"
                      stroke-width="2"
                      d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"
                    ></path>
                  </svg>
                  <span>活动时长：{{ calculateDuration() }}</span>
                </div>
              </div>
            </div>
          </div>

          <!-- 全局错误提示 -->
          <div v-if="error" class="error-alert">
            <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M12 8v4m0 4h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
              ></path>
            </svg>
            <div class="error-content">
              <strong>操作失败</strong>
              <p>{{ error }}</p>
            </div>
          </div>

          <!-- 按钮区域 -->
          <div class="form-actions">
            <button type="button" @click="$emit('close')" class="btn btn-secondary">
              <svg class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M6 18L18 6M6 6l12 12"
                ></path>
              </svg>
              取消
            </button>
            <button type="submit" :disabled="isLoading || !isFormValid" class="btn btn-primary">
              <svg
                v-if="isLoading"
                class="w-4 h-4 animate-spin"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M4 4v5h.582m15.356 2A8.001 8.001 0 004.582 9m0 0H9m11 11v-5h-.581m0 0a8.003 8.003 0 01-15.357-2m15.357 2H15"
                ></path>
              </svg>
              <svg v-else class="w-4 h-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M5 13l4 4L19 7"
                ></path>
              </svg>
              {{ isLoading ? '保存中...' : isEditing ? '更新活动' : '创建活动' }}
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { Activity, CreateActivityRequest, UpdateActivityRequest } from '../types'
import { activityApi } from '../api'

interface Props {
  circleId: number
  activity?: Activity | null
}

interface Emits {
  (e: 'close'): void
  (e: 'saved', activity?: Activity): void
}

const props = defineProps<Props>()
const emit = defineEmits<Emits>()

const isLoading = ref(false)
const error = ref('')
const errors = ref<Record<string, string>>({})

const form = ref({
  title: '',
  description: '',
  startTime: '',
  endTime: '',
})

const isEditing = computed(() => !!props.activity)

// 最小日期时间（当前时间）
const minDateTime = computed(() => {
  const now = new Date()
  return now.toISOString().slice(0, 16)
})

// 表单验证状态
const isFormValid = computed(() => {
  return (
    form.value.title.trim() &&
    form.value.startTime &&
    form.value.endTime &&
    Object.keys(errors.value).length === 0
  )
})

// 格式化日期时间为本地输入格式
const formatDateTimeForInput = (dateStr: string) => {
  const date = new Date(dateStr)
  return date.toISOString().slice(0, 16)
}

// 格式化显示时间
const formatDisplayTime = (timeStr: string) => {
  if (!timeStr) return ''
  return new Date(timeStr).toLocaleString('zh-CN', {
    year: 'numeric',
    month: '2-digit',
    day: '2-digit',
    hour: '2-digit',
    minute: '2-digit',
    weekday: 'short',
  })
}

// 计算活动时长
const calculateDuration = () => {
  if (!form.value.startTime || !form.value.endTime) return ''

  const start = new Date(form.value.startTime)
  const end = new Date(form.value.endTime)
  const diffMs = end.getTime() - start.getTime()

  if (diffMs <= 0) return '时间设置有误'

  const days = Math.floor(diffMs / (1000 * 60 * 60 * 24))
  const hours = Math.floor((diffMs % (1000 * 60 * 60 * 24)) / (1000 * 60 * 60))
  const minutes = Math.floor((diffMs % (1000 * 60 * 60)) / (1000 * 60))

  let result = []
  if (days > 0) result.push(`${days}天`)
  if (hours > 0) result.push(`${hours}小时`)
  if (minutes > 0) result.push(`${minutes}分钟`)

  return result.length > 0 ? result.join(' ') : '不足1分钟'
}

// 初始化表单数据
const initForm = () => {
  if (props.activity) {
    form.value = {
      title: props.activity.title,
      description: props.activity.description || '',
      startTime: formatDateTimeForInput(props.activity.startTime),
      endTime: formatDateTimeForInput(props.activity.endTime),
    }
  } else {
    // 设置默认时间为当前时间之后1小时和2小时
    const now = new Date()
    const startTime = new Date(now.getTime() + 60 * 60 * 1000)
    const endTime = new Date(now.getTime() + 2 * 60 * 60 * 1000)

    form.value = {
      title: '',
      description: '',
      startTime: startTime.toISOString().slice(0, 16),
      endTime: endTime.toISOString().slice(0, 16),
    }
  }

  // 清空错误
  errors.value = {}
  error.value = ''
}

// 实时验证表单
const validateField = (field: string, value: any) => {
  delete errors.value[field]

  switch (field) {
    case 'title':
      if (!value?.trim()) {
        errors.value.title = '活动标题不能为空'
      } else if (value.length > 200) {
        errors.value.title = '活动标题不能超过200个字符'
      }
      break

    case 'startTime':
      if (!value) {
        errors.value.startTime = '请选择开始时间'
      } else {
        const startDate = new Date(value)
        const now = new Date()
        if (startDate <= now && !isEditing.value) {
          errors.value.startTime = '开始时间必须晚于当前时间'
        }
      }
      break

    case 'endTime':
      if (!value) {
        errors.value.endTime = '请选择结束时间'
      } else if (form.value.startTime) {
        const startDate = new Date(form.value.startTime)
        const endDate = new Date(value)
        if (endDate <= startDate) {
          errors.value.endTime = '结束时间必须晚于开始时间'
        }
      }
      break
  }
}

// 监听表单变化进行实时验证
watch(
  () => form.value.title,
  (val) => validateField('title', val),
)
watch(
  () => form.value.startTime,
  (val) => {
    validateField('startTime', val)
    if (form.value.endTime) {
      validateField('endTime', form.value.endTime)
    }
  },
)
watch(
  () => form.value.endTime,
  (val) => validateField('endTime', val),
)

// 提交表单
const handleSubmit = async () => {
  // 最终验证
  Object.keys(form.value).forEach((key) => {
    validateField(key, form.value[key as keyof typeof form.value])
  })

  if (Object.keys(errors.value).length > 0) {
    return
  }

  isLoading.value = true
  error.value = ''

  try {
    const formatLocalTimeToISO = (localTimeString: string) => {
      return localTimeString + ':00.000Z'
    }

    const data = {
      title: form.value.title.trim(),
      description: form.value.description?.trim() || undefined,
      startTime: formatLocalTimeToISO(form.value.startTime),
      endTime: formatLocalTimeToISO(form.value.endTime),
    }

    let result
    if (isEditing.value) {
      result = await activityApi.updateActivity(
        props.circleId,
        props.activity!.activityId,
        data as UpdateActivityRequest,
      )
    } else {
      result = await activityApi.createActivity(props.circleId, data as CreateActivityRequest)
    }

    emit('saved', result.data)
    emit('close')
  } catch (err: any) {
    console.error('提交活动失败:', err)

    if (err.response?.data?.errors) {
      const backendErrors = err.response.data.errors
      Object.keys(backendErrors).forEach((key) => {
        const fieldName = key.toLowerCase()
        errors.value[fieldName] = Array.isArray(backendErrors[key])
          ? backendErrors[key][0]
          : backendErrors[key]
      })
    } else {
      error.value = err.response?.data?.message || err.response?.data?.msg || '操作失败，请重试'
    }
  } finally {
    isLoading.value = false
  }
}

onMounted(() => {
  initForm()
})
</script>

<style scoped>
.create-activity-container {
  background: #0f172a;
  border-radius: 16px;
  overflow: hidden;
  color: #e2e8f0;
}

.page-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 24px 28px;
  border-bottom: 1px solid #334155;
  background: #1e293b;
}

.header-content {
  display: flex;
  align-items: center;
  gap: 12px;
}

.header-icon {
  width: 40px;
  height: 40px;
  background: #0ea5e9;
  border-radius: 10px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
}

.page-title {
  font-size: 20px;
  font-weight: 600;
  color: #f1f5f9;
  margin: 0;
}

.close-btn {
  background: #334155;
  border: none;
  color: #94a3b8;
  cursor: pointer;
  padding: 10px;
  border-radius: 8px;
  transition: all 0.2s;
  display: flex;
  align-items: center;
  justify-content: center;
}

.close-btn:hover {
  background: #475569;
  color: #e2e8f0;
}

.create-form {
  padding: 28px;
}

.form-content {
  display: flex;
  flex-direction: column;
  gap: 28px;
}

.info-section,
.time-section {
  background: #1e293b;
  border: 1px solid #334155;
  border-radius: 12px;
  padding: 24px;
}

.section-header {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 16px;
  font-weight: 600;
  color: #0ea5e9;
  margin-bottom: 20px;
  padding-bottom: 12px;
  border-bottom: 1px solid #334155;
}

.form-section {
  margin-bottom: 20px;
}

.form-section:last-child {
  margin-bottom: 0;
}

.form-label {
  display: flex;
  align-items: center;
  gap: 4px;
  margin-bottom: 8px;
}

.label-text {
  font-weight: 500;
  color: #f1f5f9;
  font-size: 14px;
}

.required-star {
  color: #ef4444;
  font-weight: 600;
}

.input-group {
  position: relative;
}

.form-input,
.form-textarea {
  width: 100%;
  padding: 14px 16px;
  border: 1px solid #334155;
  border-radius: 8px;
  font-size: 14px;
  transition: all 0.2s;
  box-sizing: border-box;
  background: #0f172a;
  color: #e2e8f0;
}

.form-input:focus,
.form-textarea:focus {
  outline: none;
  border-color: #0ea5e9;
  box-shadow: 0 0 0 2px rgba(14, 165, 233, 0.1);
}

.form-textarea {
  resize: vertical;
  min-height: 100px;
  line-height: 1.6;
}

.input-error {
  border-color: #ef4444 !important;
  box-shadow: 0 0 0 2px rgba(239, 68, 68, 0.1) !important;
}

.input-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-top: 6px;
  min-height: 16px;
}

.error-message {
  color: #ef4444;
  font-size: 12px;
  font-weight: 500;
}

.char-count {
  color: #64748b;
  font-size: 12px;
  margin-left: auto;
}

.char-warning {
  color: #f59e0b;
}

.time-grid {
  display: grid;
  grid-template-columns: 1fr 1fr;
  gap: 20px;
  margin-bottom: 20px;
}

.time-input {
  font-family: ui-monospace, SFMono-Regular, monospace;
}

.time-preview-card {
  background: #0f172a;
  border: 1px solid #334155;
  border-radius: 10px;
  padding: 20px;
}

.preview-header {
  display: flex;
  align-items: center;
  gap: 8px;
  font-weight: 500;
  color: #0ea5e9;
  margin-bottom: 16px;
  font-size: 14px;
}

.preview-content {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 16px;
}

.time-item {
  display: flex;
  align-items: center;
  gap: 12px;
  flex: 1;
}

.time-icon {
  width: 32px;
  height: 32px;
  border-radius: 8px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: white;
}

.time-icon.start {
  background: #10b981;
}

.time-icon.end {
  background: #f59e0b;
}

.time-info {
  display: flex;
  flex-direction: column;
}

.time-label {
  font-size: 12px;
  color: #64748b;
  margin-bottom: 2px;
}

.time-value {
  font-size: 13px;
  color: #e2e8f0;
  font-weight: 500;
}

.time-arrow {
  color: #64748b;
  margin: 0 16px;
}

.duration-badge {
  display: flex;
  align-items: center;
  gap: 8px;
  background: #334155;
  padding: 10px 16px;
  border-radius: 8px;
  font-size: 13px;
  color: #cbd5e1;
  font-weight: 500;
}

.error-alert {
  background: #1e1b1b;
  border: 1px solid #ef4444;
  border-radius: 10px;
  padding: 16px;
  margin-bottom: 20px;
  display: flex;
  align-items: flex-start;
  gap: 12px;
  color: #ef4444;
}

.error-content strong {
  display: block;
  margin-bottom: 4px;
  font-size: 14px;
}

.error-content p {
  margin: 0;
  font-size: 13px;
  color: #fca5a5;
}

.form-actions {
  display: flex;
  gap: 12px;
  justify-content: flex-end;
  padding-top: 24px;
  border-top: 1px solid #334155;
}

.btn {
  padding: 12px 20px;
  border-radius: 8px;
  font-size: 14px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s;
  border: none;
  display: flex;
  align-items: center;
  gap: 8px;
  min-width: 120px;
  justify-content: center;
}

.btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.btn-primary {
  background: #0ea5e9;
  color: white;
}

.btn-primary:hover:not(:disabled) {
  background: #0284c7;
}

.btn-secondary {
  background: #334155;
  color: #cbd5e1;
  border: 1px solid #475569;
}

.btn-secondary:hover {
  background: #475569;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .time-grid {
    grid-template-columns: 1fr;
    gap: 16px;
  }

  .create-form {
    padding: 20px;
  }

  .page-header {
    padding: 20px;
  }

  .info-section,
  .time-section {
    padding: 20px;
  }

  .preview-content {
    flex-direction: column;
    gap: 16px;
  }

  .time-arrow {
    transform: rotate(90deg);
    margin: 8px 0;
  }

  .form-actions {
    flex-direction: column-reverse;
  }

  .btn {
    width: 100%;
  }
}
</style>
