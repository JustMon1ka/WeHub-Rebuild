<template>
  <div class="fixed inset-0 bg-black bg-opacity-60 flex items-center justify-center p-4 z-50">
    <div class="participation-form w-full max-w-4xl max-h-[95vh] overflow-hidden">
      <!-- 整个内容区域可滑动 -->
      <div class="form-scroll-container">
        <!-- 头部区域 -->
        <div class="form-header">
          <button @click="$emit('close')" class="close-btn">
            <svg class="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M6 18L18 6M6 6l12 12"
              ></path>
            </svg>
          </button>

          <div class="header-content">
            <div class="icon-container">
              <svg class="w-6 h-6 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M11 5H6a2 2 0 00-2 2v11a2 2 0 002 2h11a2 2 0 002-2v-5m-1.414-9.414a2 2 0 112.828 2.828L11.828 15H9v-2.828l8.586-8.586z"
                ></path>
              </svg>
            </div>
            <div class="title-section">
              <h3 class="form-title">参加活动</h3>
              <p class="form-subtitle">分享您的想法和期待</p>
            </div>
          </div>

          <!-- 活动信息卡片 -->
          <div class="activity-summary">
            <h4 class="activity-title">{{ activity.title }}</h4>
            <div class="activity-time">
              <svg class="w-4 h-4 mr-2" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M8 7V3m8 4V3m-9 8h10M5 21h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v12a2 2 0 002 2z"
                ></path>
              </svg>
              {{ formatTime(activity.startTime) }} - {{ formatTime(activity.endTime) }}
            </div>
          </div>
        </div>

        <!-- 表单区域 -->
        <div class="form-content">
          <form @submit.prevent="handleSubmit" class="form-body">
            <!-- 参加心得 -->
            <div class="form-group">
              <label class="form-label">
                <span class="required-mark">*</span>
                参加心得
              </label>
              <div class="textarea-container">
                <textarea
                  v-model="form.content"
                  @input="watchContent"
                  rows="6"
                  class="form-textarea"
                  :class="{ error: errors.content }"
                  placeholder="请分享您对这个活动的想法、期待或感受...&#10;&#10;比如：&#10;• 为什么想参加这个活动？&#10;• 您期待从中获得什么？&#10;• 您有什么相关的经验分享？"
                  maxlength="500"
                ></textarea>
                <div class="char-count" :class="{ warning: form.content.length < 50 }">
                  {{ form.content.length }}/500
                  <span v-if="form.content.length < 50" class="char-hint">
                    (还需{{ 50 - form.content.length }}字)
                  </span>
                </div>
              </div>
              <p v-if="errors.content" class="error-text">{{ errors.content }}</p>
            </div>

            <!-- 联系方式 -->
            <div class="form-group">
              <label class="form-label">联系方式 (可选)</label>
              <input
                v-model="form.contact"
                type="text"
                class="form-input"
                placeholder="手机号、微信号或其他联系方式"
                maxlength="100"
              />
              <p class="help-text">便于活动组织者与您联系</p>
            </div>

            <!-- 同意条款 -->
            <div class="agreement-section">
              <div class="checkbox-container">
                <input v-model="form.agreeToTerms" type="checkbox" class="form-checkbox" />
                <label class="agreement-text">
                  我已阅读并同意活动相关规则，承诺真实参与活动并按要求完成相关任务
                </label>
              </div>
            </div>

            <!-- 错误提示 -->
            <div v-if="error" class="error-banner">
              <p class="error-message">{{ error }}</p>
            </div>
          </form>
        </div>

        <!-- 底部按钮 -->
        <div class="form-footer">
          <div class="button-group">
            <button type="button" @click="$emit('close')" class="btn btn-secondary">取消</button>
            <button
              @click="handleSubmit"
              :disabled="!isFormValid || isSubmitting"
              class="btn btn-primary"
            >
              <svg v-if="isSubmitting" class="loading-icon" fill="none" viewBox="0 0 24 24">
                <circle
                  class="opacity-25"
                  cx="12"
                  cy="12"
                  r="10"
                  stroke="currentColor"
                  stroke-width="4"
                ></circle>
                <path
                  class="opacity-75"
                  fill="currentColor"
                  d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
                ></path>
              </svg>
              {{ isSubmitting ? '提交中...' : '确认参加' }}
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { Activity } from '../types'
import { activityApi } from '../api'

interface Props {
  activity: Activity
  circleId?: number
}

interface Emits {
  (e: 'close'): void
  (e: 'submitted'): void
}

const props = defineProps<Props>()
const emit = defineEmits<Emits>()

const isSubmitting = ref(false)
const error = ref('')

const form = ref({
  content: '',
  contact: '',
  agreeToTerms: false,
})

const errors = ref({
  content: '',
})

// 表单验证
const isFormValid = computed(() => {
  return form.value.content.trim().length >= 50 && form.value.agreeToTerms && !errors.value.content
})

// 实时验证内容
const validateContent = () => {
  const content = form.value.content.trim()

  if (!content) {
    errors.value.content = '请填写参加心得'
  } else if (content.length < 50) {
    errors.value.content = '参加心得至少需要50个字符'
  } else if (content.length > 500) {
    errors.value.content = '参加心得不能超过500个字符'
  } else {
    errors.value.content = ''
  }
}

// 监听输入变化
const watchContent = () => {
  validateContent()
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

// 提交表单
const handleSubmit = async () => {
  validateContent()

  if (!isFormValid.value) {
    return
  }

  isSubmitting.value = true
  error.value = ''

  try {
    // 实际调用API提交参与心得
    await activityApi.submitParticipationNote(
      0, // 暂时传0，因为当前Activity类型中没有circleId
      props.activity.activityId,
      {
        content: form.value.content.trim(),
        contact: form.value.contact.trim() || undefined,
      },
    )

    console.log('参与心得提交成功')
    emit('submitted')
  } catch (err: any) {
    console.error('提交参与心得失败:', err)
    error.value = err.message || '提交失败，请重试'
  } finally {
    isSubmitting.value = false
  }
}
</script>

<style scoped>
/* 主容器 */
.participation-form {
  background: #0f172a;
  border-radius: 20px;
  color: #e2e8f0;
  box-shadow: 0 25px 50px -12px rgba(0, 0, 0, 0.25);
  display: flex;
  flex-direction: column;
}

/* 滚动容器 */
.form-scroll-container {
  overflow-y: auto;
  max-height: 95vh;
  display: flex;
  flex-direction: column;
}

/* 头部区域 */
.form-header {
  background: linear-gradient(135deg, #1e293b 0%, #0f172a 100%);
  padding: 2rem;
  position: relative;
  flex-shrink: 0;
}

.close-btn {
  position: absolute;
  top: 1.25rem;
  right: 1.25rem;
  background: none;
  border: none;
  color: #94a3b8;
  cursor: pointer;
  padding: 0.5rem;
  border-radius: 8px;
  transition: all 0.2s ease;
}

.close-btn:hover {
  color: #f1f5f9;
  background: #334155;
}

.header-content {
  display: flex;
  align-items: center;
  gap: 1rem;
  margin-bottom: 1.5rem;
}

.icon-container {
  width: 2.5rem;
  height: 2.5rem;
  background: #0ea5e9;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  flex-shrink: 0;
}

.title-section {
  flex: 1;
}

.form-title {
  font-size: 1.25rem;
  font-weight: 600;
  color: #f1f5f9;
  margin: 0 0 0.25rem 0;
  line-height: 1.2;
}

.form-subtitle {
  color: #94a3b8;
  margin: 0;
  font-size: 0.875rem;
}

.activity-summary {
  background: #1e293b;
  padding: 1.25rem;
  border-radius: 12px;
  border: 1px solid #334155;
}

.activity-title {
  font-weight: 500;
  color: #f1f5f9;
  margin: 0 0 0.75rem 0;
  font-size: 1rem;
  line-height: 1.4;
}

.activity-time {
  display: flex;
  align-items: center;
  color: #94a3b8;
  font-size: 0.875rem;
}

/* 表单内容区域 */
.form-content {
  flex: 1;
  padding: 2rem;
}

.form-body {
  display: flex;
  flex-direction: column;
  gap: 2rem;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 0.75rem;
}

.form-label {
  font-weight: 500;
  color: #f1f5f9;
  font-size: 0.95rem;
  display: flex;
  align-items: center;
}

.required-mark {
  color: #f87171;
  margin-right: 0.5rem;
  font-weight: 600;
}

.textarea-container {
  position: relative;
}

.form-textarea,
.form-input {
  width: 100%;
  padding: 1rem 1.25rem;
  border: 1px solid #334155;
  border-radius: 12px;
  font-size: 0.95rem;
  line-height: 1.6;
  transition: all 0.2s ease;
  box-sizing: border-box;
  font-family: inherit;
  background: #1e293b;
  color: #e2e8f0;
}

.form-textarea {
  resize: none;
  min-height: 140px;
}

.form-textarea::placeholder,
.form-input::placeholder {
  color: #64748b;
  line-height: 1.5;
}

.form-textarea:focus,
.form-input:focus {
  outline: none;
  border-color: #0ea5e9;
  box-shadow: 0 0 0 3px rgba(14, 165, 233, 0.1);
}

.form-textarea.error,
.form-input.error {
  border-color: #ef4444;
}

.form-textarea.error:focus,
.form-input.error:focus {
  border-color: #ef4444;
  box-shadow: 0 0 0 3px rgba(239, 68, 68, 0.1);
}

.char-count {
  position: absolute;
  bottom: 0.75rem;
  right: 0.75rem;
  background: rgba(15, 23, 42, 0.9);
  color: #94a3b8;
  padding: 0.375rem 0.625rem;
  border-radius: 6px;
  font-size: 0.8rem;
  backdrop-filter: blur(4px);
}

.char-count.warning {
  color: #f59e0b;
}

.char-hint {
  color: #f59e0b;
  margin-left: 0.5rem;
}

.error-text {
  color: #f87171;
  font-size: 0.875rem;
  margin: 0;
}

.help-text {
  color: #94a3b8;
  font-size: 0.875rem;
  margin: 0;
}

/* 同意条款区域 */
.agreement-section {
  background: rgba(30, 41, 59, 0.5);
  border: 1px solid rgba(51, 65, 85, 0.5);
  border-radius: 12px;
  padding: 1.25rem;
}

.checkbox-container {
  display: flex;
  align-items: flex-start;
  gap: 0.75rem;
}

.form-checkbox {
  width: 1.125rem;
  height: 1.125rem;
  margin-top: 0.125rem;
  accent-color: #0ea5e9;
  border-radius: 4px;
  flex-shrink: 0;
}

.agreement-text {
  color: #cbd5e1;
  line-height: 1.6;
  font-size: 0.9rem;
  cursor: pointer;
}

/* 错误横幅 */
.error-banner {
  background: rgba(127, 29, 29, 0.5);
  border: 1px solid #ef4444;
  border-radius: 12px;
  padding: 1rem;
}

.error-message {
  color: #fecaca;
  margin: 0;
  font-size: 0.9rem;
}

/* 底部按钮区域 */
.form-footer {
  padding: 1.5rem 2rem;
  background: rgba(30, 41, 59, 0.3);
  border-top: 1px solid #334155;
  flex-shrink: 0;
}

.button-group {
  display: flex;
  gap: 0.75rem;
}

.btn {
  flex: 1;
  padding: 0.75rem 1.25rem;
  border: none;
  border-radius: 10px;
  font-size: 0.95rem;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s ease;
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 0.5rem;
}

.btn-secondary {
  background: #374151;
  color: #e5e7eb;
}

.btn-secondary:hover {
  background: #4b5563;
}

.btn-primary {
  background: #0ea5e9;
  color: #ffffff;
}

.btn-primary:hover:not(:disabled) {
  background: #0284c7;
}

.btn-primary:disabled {
  background: #374151;
  color: #9ca3af;
  cursor: not-allowed;
}

.loading-icon {
  width: 1rem;
  height: 1rem;
  animation: spin 1s linear infinite;
}

@keyframes spin {
  from {
    transform: rotate(0deg);
  }
  to {
    transform: rotate(360deg);
  }
}

/* 响应式调整 */
@media (max-width: 768px) {
  .form-header,
  .form-content,
  .form-footer {
    padding: 1.5rem;
  }

  .form-body {
    gap: 1.75rem;
  }

  .header-content {
    gap: 0.75rem;
    margin-bottom: 1.25rem;
  }

  .icon-container {
    width: 2.25rem;
    height: 2.25rem;
  }

  .form-title {
    font-size: 1.125rem;
  }
}
</style>
