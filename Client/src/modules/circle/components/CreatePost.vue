<template>
  <div class="create-post-overlay" @click="handleOverlayClick">
    <div class="create-post-modal" @click.stop>
      <!-- 头部 -->
      <div class="modal-header">
        <h2 class="modal-title">创建帖子</h2>
        <button class="close-btn" @click="$emit('close')">
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

      <!-- 内容区 -->
      <div class="modal-body">
        <!-- 社区选择 -->
        <div class="form-group">
          <label class="form-label">发布到社区</label>
          <div class="community-info">
            <img class="community-avatar" :src="communityAvatar" :alt="communityName" />
            <span class="community-name">{{ communityName }}</span>
          </div>
        </div>

        <!-- 标题输入 -->
        <div class="form-group">
          <label class="form-label">标题 *</label>
          <input
            v-model="postData.title"
            type="text"
            class="form-input"
            :class="{ error: errors.title }"
            placeholder="请输入帖子标题..."
            maxlength="100"
          />
          <div class="input-footer">
            <span v-if="errors.title" class="error-text">{{ errors.title }}</span>
            <span class="char-count">{{ postData.title.length }}/100</span>
          </div>
        </div>

        <!-- 内容输入 -->
        <div class="form-group">
          <label class="form-label">内容 *</label>
          <textarea
            v-model="postData.content"
            class="form-textarea"
            :class="{ error: errors.content }"
            placeholder="分享你的想法..."
            rows="8"
            maxlength="5000"
          ></textarea>
          <div class="input-footer">
            <span v-if="errors.content" class="error-text">{{ errors.content }}</span>
            <span class="char-count">{{ postData.content.length }}/5000</span>
          </div>
        </div>

        <!-- 标签输入 -->
        <div class="form-group">
          <label class="form-label">标签 (可选)</label>
          <div class="tag-input-container">
            <div class="tag-list">
              <span v-for="(tag, index) in postData.tags" :key="index" class="tag-item">
                {{ tag }}
                <button class="tag-remove" @click="removeTag(index)">×</button>
              </span>
            </div>
            <input
              v-model="newTag"
              type="text"
              class="tag-input"
              placeholder="添加标签..."
              @keydown.enter.prevent="addTag"
              @keydown.space.prevent="addTag"
              maxlength="20"
            />
          </div>
          <p class="form-hint">按 Enter、空格或逗号添加标签，最多5个</p>
        </div>
      </div>

      <!-- 底部操作 -->
      <div class="modal-footer">
        <button class="btn btn-secondary" @click="$emit('close')" :disabled="isSubmitting">
          取消
        </button>
        <button
          class="btn btn-primary"
          @click="handleSubmit"
          :disabled="!canSubmit || isSubmitting"
        >
          {{ isSubmitting ? '发布中...' : '发布' }}
        </button>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { PostAPI } from '../api'
import { User } from '@/modules/auth/scripts/User'

// Props
interface Props {
  circleId: number
  communityName: string
  communityAvatar?: string
}

const props = defineProps<Props>()

// Emits
const emit = defineEmits<{
  close: []
  submitted: [post: any]
}>()

// 响应式数据
const postData = ref({
  title: '',
  content: '',
  tags: [] as string[],
})

const newTag = ref('')
const isSubmitting = ref(false)
const errors = ref({
  title: '',
  content: '',
})

// 🔧 添加消息提示状态
const message = ref({
  text: '',
  type: 'info' as 'success' | 'error' | 'warning' | 'info',
  show: false,
})

// 计算属性
const canSubmit = computed(() => {
  return postData.value.title.trim() && postData.value.content.trim() && !isSubmitting.value
})

// 🔧 添加 showMessage 函数
const showMessage = (text: string, type: 'success' | 'error' | 'warning' | 'info' = 'info') => {
  message.value = {
    text,
    type,
    show: true,
  }

  // 3秒后自动隐藏
  setTimeout(() => {
    message.value.show = false
  }, 3000)
}

// 🔧 添加 resetForm 函数
const resetForm = () => {
  postData.value = {
    title: '',
    content: '',
    tags: [],
  }
  newTag.value = ''
  errors.value = {
    title: '',
    content: '',
  }
}

// 检查登录状态的函数
const checkAuthStatus = (): boolean => {
  const userInstance = User.getInstance()
  if (!userInstance) {
    // 用户未登录，显示错误并可能触发登录弹窗
    errors.value.title = '请先登录后再发帖'

    // 触发登录弹窗（如果你有全局的登录弹窗方法）
    if (window.$app && window.$app.toggleLoginHover) {
      window.$app.toggleLoginHover(true)
    }

    return false
  }
  return true
}

// 获取当前用户信息
const getCurrentUserInfo = () => {
  const userInstance = User.getInstance()
  if (userInstance) {
    return {
      userId: userInstance.userAuth?.userId,
      username: userInstance.userInfo?.value.username,
      email: userInstance.userInfo?.value.email,
    }
  }
  return null
}

// 验证方法
const validateForm = (): boolean => {
  errors.value = { title: '', content: '' }
  let isValid = true

  if (!postData.value.title.trim()) {
    errors.value.title = '请输入标题'
    isValid = false
  } else if (postData.value.title.length > 100) {
    errors.value.title = '标题不能超过100个字符'
    isValid = false
  }

  if (!postData.value.content.trim()) {
    errors.value.content = '请输入内容'
    isValid = false
  } else if (postData.value.content.length > 5000) {
    errors.value.content = '内容不能超过5000个字符'
    isValid = false
  }

  return isValid
}

// 标签管理
const addTag = (): void => {
  const tag = newTag.value.trim()
  if (tag && !postData.value.tags.includes(tag) && postData.value.tags.length < 5) {
    postData.value.tags.push(tag)
    newTag.value = ''
  }
}

const removeTag = (index: number): void => {
  postData.value.tags.splice(index, 1)
}

// 提交处理
const handleSubmit = async (): Promise<void> => {
  if (!validateForm()) {
    return
  }

  // 先检查登录状态
  if (!checkAuthStatus()) {
    return
  }

  try {
    isSubmitting.value = true

    // 🔧 调用发帖API - 注意：暂时传空数组给tags，因为后端需要数字ID
    const response = await PostAPI.publishPost({
      circleId: props.circleId,
      title: postData.value.title.trim(),
      content: postData.value.content.trim(),
      tags: [], // 🔧 暂时传空数组，等确认后端标签处理方式后再修改
    })
    showMessage('帖子发布成功！', 'success')

    // 重置表单
    resetForm()

    // 🔧 修正事件名 - 通知父组件刷新
    emit('submitted', response)

    // 🔧 延迟关闭弹窗，让用户看到成功提示
    setTimeout(() => {
      emit('close')
    }, 1500)
  } catch (error: any) {
    console.error('❌ 发帖失败:', error)

    let errorMessage = '发布失败，请重试'

    // 🔧 根据错误类型显示不同消息
    if (error.message.includes('数据库连接失败')) {
      errorMessage = '服务暂时不可用，请稍后重试'
    } else if (error.message.includes('系统数据表配置异常')) {
      errorMessage = '系统配置异常，请联系技术支持'
    } else if (error.message.includes('登录已过期')) {
      errorMessage = '登录已过期，请重新登录'
      // 可以触发登录弹窗
      if (window.$app && window.$app.toggleLoginHover) {
        window.$app.toggleLoginHover(true)
      }
    } else if (error.message.includes('用户未认证')) {
      errorMessage = '请先登录后再发布内容'
    } else {
      errorMessage = error.message || '发布失败，请重试'
    }

    showMessage(errorMessage, 'error')
  } finally {
    isSubmitting.value = false
  }
}

// 覆盖层点击处理
const handleOverlayClick = (): void => {
  emit('close')
}

const communityAvatar = computed(() => {
  return (
    props.communityAvatar ||
    `https://placehold.co/32x32/1677ff/ffffff?text=${encodeURIComponent(props.communityName[0] || 'C')}`
  )
})

// 生命周期
onMounted(() => {
  // 自动聚焦到标题输入框
  setTimeout(() => {
    const titleInput = document.querySelector('.form-input') as HTMLInputElement
    titleInput?.focus()
  }, 100)
})
</script>

<style scoped>
.create-post-overlay {
  position: fixed;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: rgba(0, 0, 0, 0.7);
  display: flex;
  align-items: center;
  justify-content: center;
  z-index: 1000;
  padding: 20px;
}

.create-post-modal {
  background: #1e293b; /* slate-800 */
  border-radius: 12px;
  width: 100%;
  max-width: 600px;
  max-height: 90vh;
  display: flex;
  flex-direction: column;
  border: 1px solid #334155; /* slate-700 */
  box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.3);
}

.modal-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 20px 24px;
  border-bottom: 1px solid #334155; /* slate-700 */
}

.modal-title {
  font-size: 20px;
  font-weight: 600;
  color: #f1f5f9; /* slate-100 */
  margin: 0;
}

.close-btn {
  background: none;
  border: none;
  color: #64748b; /* slate-500 */
  cursor: pointer;
  padding: 4px;
  border-radius: 4px;
  transition: all 0.2s;
  width: 32px;
  height: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
}

.close-btn:hover {
  color: #f1f5f9; /* slate-100 */
  background: #334155; /* slate-700 */
}

.close-btn svg {
  width: 20px;
  height: 20px;
}

.modal-body {
  flex: 1;
  overflow-y: auto;
  padding: 24px;
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.form-group {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.form-label {
  font-size: 14px;
  font-weight: 500;
  color: #f1f5f9; /* slate-100 */
}

.community-info {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 12px;
  background: #334155; /* slate-700 */
  border-radius: 8px;
}

.community-avatar {
  width: 32px;
  height: 32px;
  border-radius: 50%;
}

.community-name {
  font-weight: 500;
  color: #f1f5f9; /* slate-100 */
}

.form-input,
.form-textarea {
  background: #0f172a; /* slate-900 */
  border: 1px solid #334155; /* slate-700 */
  border-radius: 8px;
  padding: 12px;
  color: #f1f5f9; /* slate-100 */
  font-size: 14px;
  transition: border-color 0.2s;
}

.form-input:focus,
.form-textarea:focus {
  outline: none;
  border-color: #0ea5e9; /* sky-500 */
}

.form-input.error,
.form-textarea.error {
  border-color: #ef4444; /* red-500 */
}

.form-textarea {
  resize: vertical;
  min-height: 120px;
  line-height: 1.5;
}

.input-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 12px;
  margin-top: 4px;
}

.error-text {
  color: #ef4444; /* red-500 */
}

.char-count {
  color: #64748b; /* slate-500 */
}

.tag-input-container {
  background: #0f172a; /* slate-900 */
  border: 1px solid #334155; /* slate-700 */
  border-radius: 8px;
  padding: 8px;
  min-height: 48px;
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
  align-items: center;
  transition: border-color 0.2s;
}

.tag-input-container:focus-within {
  border-color: #0ea5e9; /* sky-500 */
}

.tag-list {
  display: flex;
  flex-wrap: wrap;
  gap: 6px;
}

.tag-item {
  background: #0ea5e9; /* sky-500 */
  color: white;
  padding: 4px 8px;
  border-radius: 4px;
  font-size: 12px;
  display: flex;
  align-items: center;
  gap: 4px;
}

.tag-remove {
  background: none;
  border: none;
  color: white;
  cursor: pointer;
  font-size: 16px;
  line-height: 1;
  padding: 0;
  width: 16px;
  height: 16px;
  display: flex;
  align-items: center;
  justify-content: center;
  border-radius: 2px;
}

.tag-remove:hover {
  background: rgba(255, 255, 255, 0.2);
}

.tag-input {
  background: none;
  border: none;
  color: #f1f5f9; /* slate-100 */
  font-size: 14px;
  outline: none;
  flex: 1;
  min-width: 100px;
  padding: 4px;
}

.tag-input::placeholder {
  color: #64748b; /* slate-500 */
}

.form-hint {
  font-size: 12px;
  color: #64748b; /* slate-500 */
  margin: 0;
}

.modal-footer {
  display: flex;
  align-items: center;
  justify-content: flex-end;
  gap: 12px;
  padding: 20px 24px;
  border-top: 1px solid #334155; /* slate-700 */
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

.btn-secondary {
  background: #334155; /* slate-700 */
  color: #cbd5e1; /* slate-300 */
}

.btn-secondary:hover:not(:disabled) {
  background: #475569; /* slate-600 */
}

.btn-primary {
  background: #0ea5e9; /* sky-500 */
  color: white;
}

.btn-primary:hover:not(:disabled) {
  background: #0284c7; /* sky-600 */
}

/* 响应式设计 */
@media (max-width: 640px) {
  .create-post-overlay {
    padding: 10px;
  }

  .create-post-modal {
    max-height: 95vh;
  }

  .modal-header,
  .modal-body,
  .modal-footer {
    padding-left: 16px;
    padding-right: 16px;
  }
}
</style>
