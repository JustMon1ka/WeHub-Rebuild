<template>
  <div
    class="fixed inset-0 bg-black bg-opacity-60 flex items-center justify-center p-4 z-50"
    style="transform: translateY(50px)"
  >
    <div
      class="bg-slate-900 rounded-2xl shadow-2xl w-full max-w-md max-h-[80vh] overflow-y-auto p-6"
    >
      <div class="participation-form">
        <!-- 头部 -->
        <div class="form-header">
          <h3 class="form-title">参加活动</h3>
          <button @click="$emit('close')" class="close-btn">×</button>
        </div>

        <!-- 活动信息 -->
        <div class="activity-summary">
          <h4 class="activity-name">{{ activity.title }}</h4>
          <p class="activity-time">
            {{ formatTime(activity.startTime) }} - {{ formatTime(activity.endTime) }}
          </p>
        </div>

        <!-- 表单 -->
        <form @submit.prevent="handleSubmit" class="participation-content">
          <div class="form-section">
            <label class="form-label required">参加心得</label>
            <textarea
              v-model="form.content"
              rows="6"
              class="form-textarea"
              :class="{ error: errors.content }"
              placeholder="请分享您参加此活动的想法、期待或感受（至少50字）"
              maxlength="500"
            ></textarea>
            <div class="form-meta">
              <p v-if="errors.content" class="error-message">{{ errors.content }}</p>
              <p class="char-count" :class="{ warning: form.content.length < 50 }">
                {{ form.content.length }}/500 字符
                <span v-if="form.content.length < 50" class="min-requirement">
                  (最少需要50字)
                </span>
              </p>
            </div>
          </div>

          <div class="form-section">
            <label class="form-label">联系方式 (可选)</label>
            <input
              v-model="form.contact"
              type="text"
              class="form-input"
              placeholder="便于活动组织者联系您"
              maxlength="100"
            />
            <p class="form-help">如手机号、微信号等</p>
          </div>

          <div class="form-section">
            <label class="checkbox-label">
              <input v-model="form.agreeToTerms" type="checkbox" class="form-checkbox" />
              我已阅读并同意活动相关规则
            </label>
          </div>

          <!-- 全局错误 -->
          <div v-if="error" class="error-alert">
            <p>{{ error }}</p>
          </div>

          <!-- 按钮 -->
          <div class="form-actions">
            <button type="button" @click="$emit('close')" class="btn btn-secondary">取消</button>
            <button type="submit" :disabled="!isFormValid || isSubmitting" class="btn btn-primary">
              {{ isSubmitting ? '提交中...' : '确认参加' }}
            </button>
          </div>
        </form>
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
/* 替换现有样式为深色主题 */
.participation-form {
  background: #0f172a; /* slate-900 */
  border-radius: 16px;
  color: #e2e8f0; /* slate-200 */
}

.form-title {
  font-size: 20px;
  font-weight: 600;
  color: #f1f5f9; /* slate-100 */
  margin: 0;
}

.close-btn {
  background: none;
  border: none;
  color: #94a3b8; /* slate-400 */
  font-size: 24px;
  cursor: pointer;
  padding: 4px;
  border-radius: 6px;
  transition: background 0.2s;
}

.close-btn:hover {
  background: #334155; /* slate-700 */
}

.activity-summary {
  background: #1e293b; /* slate-800 */
  padding: 16px;
  border-radius: 8px;
  margin-bottom: 24px;
  border: 1px solid #334155; /* slate-700 */
}

.form-label {
  display: block;
  font-weight: 500;
  color: #f1f5f9; /* slate-100 */
  margin-bottom: 8px;
  font-size: 14px;
}

.form-textarea,
.form-input {
  width: 100%;
  padding: 12px 16px;
  border: 1px solid #334155; /* slate-700 */
  border-radius: 8px;
  font-size: 14px;
  transition: border-color 0.2s;
  box-sizing: border-box;
  font-family: inherit;
  background: #1e293b; /* slate-800 */
  color: #e2e8f0; /* slate-200 */
}

.form-textarea:focus,
.form-input:focus {
  outline: none;
  border-color: #0ea5e9; /* sky-500 */
  box-shadow: 0 0 0 2px rgba(14, 165, 233, 0.1);
}

.btn-primary {
  background: #0ea5e9; /* sky-500 */
  color: #fff;
}

.btn-primary:hover:not(:disabled) {
  background: #0284c7; /* sky-600 */
}

.btn-secondary {
  background: #1e293b; /* slate-800 */
  color: #cbd5e1; /* slate-300 */
  border: 1px solid #334155; /* slate-700 */
}

.btn-secondary:hover {
  background: #334155; /* slate-700 */
}
</style>
