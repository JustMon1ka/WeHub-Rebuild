<template>
  <div class="fixed inset-0 bg-slate-950/50 backdrop-blur-md bg-opacity-60 flex items-center justify-center p-4 z-[9999]">
    <div class="bg-slate-900 rounded-2xl shadow-2xl  border-2 border-slate-700 w-full max-w-4xl max-h-[90vh] overflow-y-auto">
      <div class="create-activity-container">
        <!-- 页面头部 -->
        <div class="page-header">
          <h3 class="page-title">
            {{ isEditing ? '编辑活动' : '创建活动' }}
          </h3>
          <button @click="$emit('close')" class="close-btn rounded-full">×</button>
        </div>

        <!-- 表单内容 -->
        <form @submit.prevent="handleSubmit" class="create-form">
          <div class="form-grid p-8">
            <!-- 左侧表单区域 -->
            <div class="form-left space-y-8">
              <!-- 活动标题 -->
              <div class="form-section">
                <label class="form-label required">活动标题</label>
                <input
                  v-model="form.title"
                  type="text"
                  required
                  maxlength="200"
                  class="form-input"
                  :class="{ error: errors.title }"
                  placeholder="请输入活动标题"
                />
                <p v-if="errors.title" class="error-message">{{ errors.title }}</p>
                <p class="form-help">{{ form.title.length }}/200</p>
              </div>

              <!-- 活动描述 -->
              <div class="form-section">
                <label class="form-label">活动描述</label>
                <textarea
                  v-model="form.description"
                  rows="4"
                  class="form-textarea"
                  :class="{ error: errors.description }"
                  placeholder="请输入活动描述"
                ></textarea>
                <p v-if="errors.description" class="error-message">{{ errors.description }}</p>
              </div>

              <!-- 活动奖励 -->
              <div class="form-section">
                <label class="form-label">活动奖励</label>
                <input
                  v-model="form.reward"
                  type="text"
                  maxlength="200"
                  class="form-input"
                  :class="{ error: errors.reward }"
                  placeholder="请输入活动奖励"
                />
                <p v-if="errors.reward" class="error-message">{{ errors.reward }}</p>
                <p class="form-help">{{ (form.reward || '').length }}/200</p>
              </div>
            </div>

            <!-- 右侧图片和时间区域 -->
            <div class="form-right">
              <!-- 活动图片 -->
              <div class="form-section">
                <label class="form-label">活动图片</label>
                <div class="upload-section">
                  <div class="image-preview" v-if="imagePreview">
                    <img :src="imagePreview" alt="活动图片预览" class="preview-image" />
                    <button type="button" class="remove-image" @click="removeImage">×</button>
                  </div>
                  <div v-else class="image-placeholder" @click="triggerImageUpload">
                    <div class="upload-icon">📷</div>
                    <p>上传活动图片</p>
                    <p class="upload-tip">(可选)</p>
                  </div>
                </div>
                <p class="form-help">推荐尺寸 800x400 像素</p>
                <input
                  ref="imageInput"
                  type="file"
                  accept="image/*"
                  style="display: none"
                  @change="handleImageUpload"
                />
              </div>

              <!-- 时间设置 -->
              <div class="form-section">
                <label class="form-label required">开始时间</label>
                <input
                  v-model="form.startTime"
                  type="datetime-local"
                  required
                  :min="minDateTime"
                  class="form-input"
                  :class="{ error: errors.startTime }"
                />
                <p v-if="errors.startTime" class="error-message">{{ errors.startTime }}</p>
              </div>

              <div class="form-section">
                <label class="form-label required">结束时间</label>
                <input
                  v-model="form.endTime"
                  type="datetime-local"
                  required
                  :min="form.startTime || minDateTime"
                  class="form-input"
                  :class="{ error: errors.endTime }"
                />
                <p v-if="errors.endTime" class="error-message">{{ errors.endTime }}</p>
              </div>

              <!-- 时间预览 -->
              <div v-if="form.startTime && form.endTime" class="time-preview">
                <h4 class="preview-title">时间预览</h4>
                <div class="preview-content">
                  <p><strong>开始：</strong>{{ formatDisplayTime(form.startTime) }}</p>
                  <p><strong>结束：</strong>{{ formatDisplayTime(form.endTime) }}</p>
                  <p><strong>时长：</strong>{{ calculateDuration() }}</p>
                </div>
              </div>
            </div>
          </div>

          <!-- 全局错误提示 -->
          <div v-if="error" class="error-alert">
            <p>{{ error }}</p>
          </div>

          <!-- 按钮区域 -->
          <div class="form-actions flex flex-row justify-end-safe w-full px-6">
            <button type="button" @click="$emit('close')" class="btn btn-secondary">取消</button>
            <button type="submit" :disabled="isLoading || !isFormValid" class="btn btn-primary">
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
import { activityApi, CircleAPI } from '../api'

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

// 添加图片相关状态
const imagePreview = ref('')
const imageInput = ref<HTMLInputElement>()

const form = ref({
  title: '',
  description: '',
  reward: '',
  startTime: '',
  endTime: '',
  imageFile: null as File | null,
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

// 图片上传处理
const triggerImageUpload = () => {
  imageInput.value?.click()
}

const handleImageUpload = async (event: Event) => {
  const target = event.target as HTMLInputElement
  const file = target.files?.[0]
  if (!file) return

  // 验证文件大小（5MB）
  if (file.size > 5 * 1024 * 1024) {
    alert('图片文件大小不能超过5MB')
    return
  }

  // 验证文件类型
  const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif']
  if (!allowedTypes.includes(file.type)) {
    alert('只支持 JPG、PNG、GIF 格式的图片')
    return
  }

  // 显示本地预览
  const reader = new FileReader()
  reader.onload = (e) => {
    imagePreview.value = e.target?.result as string
  }
  reader.readAsDataURL(file)

  // 存储文件
  form.value.imageFile = file
}

const removeImage = () => {
  imagePreview.value = ''
  form.value.imageFile = null
  if (imageInput.value) {
    imageInput.value.value = ''
  }
}

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
  })
}

// 计算活动时长
const calculateDuration = () => {
  if (!form.value.startTime || !form.value.endTime) return ''

  const start = new Date(form.value.startTime)
  const end = new Date(form.value.endTime)
  const diffMs = end.getTime() - start.getTime()

  if (diffMs <= 0) return '时间设置有误'

  const hours = Math.floor(diffMs / (1000 * 60 * 60))
  const minutes = Math.floor((diffMs % (1000 * 60 * 60)) / (1000 * 60))

  if (hours > 0) {
    return minutes > 0 ? `${hours}小时${minutes}分钟` : `${hours}小时`
  } else {
    return `${minutes}分钟`
  }
}

// 初始化表单数据
const initForm = () => {
  if (props.activity) {
    form.value = {
      title: props.activity.title,
      description: props.activity.description || '',
      reward: props.activity.reward || '',
      startTime: formatDateTimeForInput(props.activity.startTime),
      endTime: formatDateTimeForInput(props.activity.endTime),
      imageFile: null,
    }
  } else {
    // 设置默认时间为当前时间之后1小时和2小时
    const now = new Date()
    const startTime = new Date(now.getTime() + 60 * 60 * 1000)
    const endTime = new Date(now.getTime() + 2 * 60 * 60 * 1000)

    form.value = {
      title: '',
      description: '',
      reward: '',
      startTime: startTime.toISOString().slice(0, 16),
      endTime: endTime.toISOString().slice(0, 16),
      imageFile: null,
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

    case 'reward':
      if (value && value.length > 200) {
        errors.value.reward = '奖励说明不能超过200个字符'
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
  () => form.value.reward,
  (val) => validateField('reward', val),
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
    if (key !== 'imageFile') {
      validateField(key, form.value[key as keyof typeof form.value])
    }
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
      reward: form.value.reward?.trim() || undefined,
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

    if (form.value.imageFile && result.data) {
      try {
        console.log('开始上传活动图片...')
        const uploadResult = await activityApi.uploadActivityImage(
          props.circleId,
          result.data.activityId,
          form.value.imageFile,
        )
        console.log('活动图片上传成功:', uploadResult)

        // 可以添加成功提示
        // alert('活动创建成功，图片上传成功！')
      } catch (imageError) {
        console.error('图片上传失败详情:', imageError)
        console.error('图片上传错误响应:', imageError.response?.data)

        // 给用户明确的错误提示
        const errorMsg = imageError.response?.data?.message || imageError.message || '图片上传失败'
        alert(`活动创建成功，但图片上传失败: ${errorMsg}`)
      }
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
/* 替换现有的样式为深色主题 */
.create-activity-container {
  background: #0f172a; /* slate-900 */
  border-radius: 16px;
  overflow: hidden;
  color: #e2e8f0; /* slate-200 */
}

.page-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 20px 24px;
  border-bottom: 1px solid #334155; /* slate-700 */
  background: #1e293b; /* slate-800 */
}

.page-title {
  font-size: 24px;
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
  width: 2rem;
  height: 2rem;
  border-radius: 100%;
  transition: background 0.2s;
}

.close-btn:hover {
  background: #334155; /* slate-700 */
}

.form-label {
  display: block;
  font-weight: 500;
  color: #f1f5f9; /* slate-100 */
  margin-bottom: 8px;
  font-size: 16px;
  width: fit-content;
  padding-top: 0.25rem;
  padding-bottom: 0.25rem;
}

.form-input,
.form-textarea {
  width: 100%;
  padding: 12px 16px;
  border: 1px solid #334155; /* slate-700 */
  border-radius: 8px;
  font-size: 14px;
  transition: border-color 0.2s;
  box-sizing: border-box;
  background: #1e293b; /* slate-800 */
  color: #e2e8f0; /* slate-200 */
}

.form-input:focus,
.form-textarea:focus {
  outline: none;
  border-color: #0ea5e9; /* sky-500 */
  box-shadow: 0 0 0 2px rgba(14, 165, 233, 0.1);
}

.image-placeholder {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  border: 2px dashed #334155; /* slate-700 */
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.2s;
  background: #1e293b; /* slate-800 */
  height: 200px;
  width: 100%;
  color: #cbd5e1; /* slate-300 */
}

.image-placeholder:hover {
  border-color: #0ea5e9; /* sky-500 */
  background: #0c4a6e; /* sky-900 */
}

.time-preview {
  background: #1e293b; /* slate-800 */
  padding: 16px;
  border-radius: 8px;
  margin-top: 16px;
  border: 1px solid #334155; /* slate-700 */
}

.preview-title {
  font-size: 14px;
  font-weight: 500;
  color: #f1f5f9; /* slate-100 */
  margin-bottom: 12px;
}

.preview-content {
  font-size: 13px;
  color: #cbd5e1; /* slate-300 */
  line-height: 1.5;
}

.btn-primary {
  background: #0ea5e9; /* sky-500 */
  color: #fff;
  padding: 0.5rem 1rem;
  width: 8rem;
  border-radius: 0.75rem;
  margin: 1rem;
  font-size: 1rem;
  font-weight: 600;
}

.btn-primary:hover:not(:disabled) {
  background: #0284c7; /* sky-600 */
}

.btn-secondary {
  background: #1e293b; /* slate-800 */
  color: #cbd5e1; /* slate-300 */
  border: 1px solid #334155; /* slate-700 */
  padding: 0.5rem 1rem;
  width: 8rem;
  border-radius: 0.75rem;
  margin: 1rem;
  font-size: 1rem;
  font-weight: 600;
}

.btn-secondary:hover {
  background: #334155; /* slate-700 */

}

/* 响应式设计 */
@media (max-width: 768px) {
  .form-grid {
    grid-template-columns: 1fr;
    gap: 20px;
  }

  .create-form {
    padding: 16px;
  }
}

.error-message {
  color: #f87171; /* red-400 */
  font-size: 14px;
  margin-top: 4px;
  width: fit-content;
  padding-top: 0.25rem;
  padding-bottom: 0.25rem;
}
</style>
