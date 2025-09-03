<template>
  <div class="create-community-container">
    <!-- 主要内容区域 -->
    <div class="create-community-content">
      <!-- 页面头部 -->
      <div class="page-header">
        <button class="back-btn" @click="goBack">← 返回</button>
        <h1 class="page-title">创建新社区</h1>
        <button class="btn btn-primary create-btn" @click="handleSubmit" :disabled="!isFormValid">
          创建
        </button>
      </div>

      <!-- 表单内容 -->
      <div class="create-form">
        <!-- 社区名称 -->
        <div class="form-section">
          <label class="form-label required">社区名称</label>
          <input
            v-model="form.name"
            type="text"
            class="form-input"
            :class="{ error: errors.name }"
            placeholder="这将是你的社区在论坛中的唯一标识。"
            maxlength="50"
            @input="validateField('name')"
          />
          <p class="form-help">{{ form.name.length }}/50 字符</p>
          <p v-if="errors.name" class="error-message">{{ errors.name }}</p>
        </div>

        <!-- 社区简介 -->
        <div class="form-section">
          <label class="form-label required">社区简介</label>
          <textarea
            v-model="form.description"
            class="form-textarea"
            :class="{ error: errors.description }"
            placeholder="简单介绍你的社区是做什么的,能吸引更多同好加入。"
            maxlength="200"
            rows="4"
            @input="validateField('description')"
          ></textarea>
          <p class="form-help">{{ form.description.length }}/200 字符</p>
          <p v-if="errors.description" class="error-message">{{ errors.description }}</p>
        </div>

        <!-- 社区图标 -->
        <div class="form-section">
          <label class="form-label">社区图标</label>
          <div class="upload-section">
            <div class="upload-preview" v-if="avatarPreview">
              <img :src="avatarPreview" alt="社区图标预览" class="preview-image" />
              <button type="button" class="remove-image" @click="removeAvatar">×</button>
            </div>
            <div v-else class="upload-placeholder" @click="triggerAvatarUpload">
              <div class="upload-icon">📷</div>
              <p>上传</p>
            </div>
          </div>
          <p class="form-help">推荐尺寸 200x200 像素。</p>
          <input
            ref="avatarInput"
            type="file"
            accept="image/*"
            style="display: none"
            @change="handleAvatarUpload"
          />
        </div>

        <!-- 社区横幅 -->
        <div class="form-section">
          <label class="form-label">社区横幅</label>
          <div class="upload-section banner-upload">
            <div class="banner-preview" v-if="bannerPreview">
              <img :src="bannerPreview" alt="横幅预览" class="banner-image" />
              <button type="button" class="remove-image" @click="removeBanner">×</button>
            </div>
            <div v-else class="banner-placeholder" @click="triggerBannerUpload">
              <div class="upload-icon">🖼️</div>
              <p>上传横幅</p>
            </div>
          </div>
          <p class="form-help">推荐尺寸 1200x400 像素。</p>
          <input
            ref="bannerInput"
            type="file"
            accept="image/*"
            style="display: none"
            @change="handleBannerUpload"
          />
        </div>

        <!-- 社区分类 -->
        <div class="form-section">
          <label class="form-label required">社区分类</label>
          <select
            v-model="form.category"
            class="form-select"
            :class="{ error: errors.category }"
            @change="validateField('category')"
            :disabled="categoriesLoading"
          >
            <option value="">{{ categoriesLoading ? '加载中...' : '请选择分类' }}</option>
            <option v-for="category in availableCategories" :key="category" :value="category">
              {{ category }}
            </option>
          </select>
          <p v-if="errors.category" class="error-message">{{ errors.category }}</p>
          <p v-if="categoriesError" class="error-message">{{ categoriesError }}</p>
        </div>

        <!-- 社区设置 -->
        <div class="form-section">
          <label class="form-label">社区设置</label>
          <div class="checkbox-group">
            <label class="checkbox-item">
              <input type="checkbox" v-model="form.isPrivate" class="form-checkbox" />
              <span class="checkbox-text">私密社区</span>
              <span class="checkbox-help">只有受邀请的用户才能加入</span>
            </label>
          </div>
        </div>

        <!-- 社区规则 -->
        <div class="form-section">
          <label class="form-label">社区规则 (可选)</label>
          <p class="form-help">为你的社区设立一些基本规则,有助于维持良好讨论氛围。</p>
          <div class="rules-section">
            <div v-for="(rule, index) in form.rules" :key="index" class="rule-item">
              <div class="rule-inputs">
                <input
                  v-model="rule.title"
                  type="text"
                  class="form-input rule-title"
                  placeholder="规则标题"
                  maxlength="30"
                />
                <textarea
                  v-model="rule.content"
                  class="form-textarea rule-content"
                  placeholder="规则内容"
                  maxlength="100"
                  rows="2"
                ></textarea>
              </div>
              <button
                type="button"
                class="remove-rule-btn"
                @click="removeRule(index)"
                v-if="form.rules.length > 1"
              >
                删除
              </button>
            </div>
            <button type="button" class="btn btn-outline add-rule-btn" @click="addRule">
              + 添加规则
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- 侧边栏 -->
    <div class="sidebar">
      <div class="sidebar-card">
        <h3 class="sidebar-title">创建社区指南</h3>
        <ul class="guide-list">
          <li>一个独特且相关的名称能让你的社区脱颖而出。</li>
          <li>清晰的简介和规则能帮助新成员快速融入。</li>
          <li>高质量的图标和横幅能给社区带来更好的第一印象。</li>
        </ul>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { CircleAPI } from '../api.ts'

const router = useRouter()

// 表单数据
const form = ref({
  name: '',
  description: '',
  category: '',
  isPrivate: false,
  avatarFile: null as File | null,
  bannerFile: null as File | null,
  rules: [
    { title: '友善交流', content: '保持友善和尊重的交流氛围' },
    { title: '内容相关', content: '发布与社区主题相关的内容' },
  ],
})

// 错误信息
const errors = ref({
  name: '',
  description: '',
  category: '',
})

// 分类相关状态
const availableCategories = ref<string[]>([])
const categoriesLoading = ref(false)
const categoriesError = ref('')

// 图片预览
const avatarPreview = ref('')
const bannerPreview = ref('')

// 文件输入引用
const avatarInput = ref<HTMLInputElement>()
const bannerInput = ref<HTMLInputElement>()

// 加载分类列表
const loadCategories = async () => {
  categoriesLoading.value = true
  categoriesError.value = ''

  try {
    const categories = await CircleAPI.getCategories()
    availableCategories.value = categories
  } catch (error) {
    console.error('加载分类失败:', error)
    categoriesError.value = '加载分类失败，请刷新页面重试'
    // 如果加载失败，使用默认分类作为备选
    availableCategories.value = ['技术', '生活', '娱乐', '教育', '商业', '体育', '其他']
  } finally {
    categoriesLoading.value = false
  }
}

// 页面加载时获取分类
onMounted(() => {
  loadCategories()
})

// 表单验证
const validateField = (field: string) => {
  switch (field) {
    case 'name':
      if (!form.value.name.trim()) {
        errors.value.name = '社区名称不能为空'
      } else if (form.value.name.length < 2) {
        errors.value.name = '社区名称至少需要2个字符'
      } else {
        errors.value.name = ''
      }
      break
    case 'description':
      if (!form.value.description.trim()) {
        errors.value.description = '社区描述不能为空'
      } else if (form.value.description.length < 10) {
        errors.value.description = '社区描述至少需要10个字符'
      } else {
        errors.value.description = ''
      }
      break
    case 'category':
      if (!form.value.category) {
        errors.value.category = '请选择社区分类'
      } else {
        errors.value.category = ''
      }
      break
  }
}

// 验证整个表单
const validateForm = () => {
  validateField('name')
  validateField('description')
  validateField('category')
  return Object.values(errors.value).every((error) => error === '')
}

// 表单是否有效
const isFormValid = computed(() => {
  return (
    form.value.name.trim() &&
    form.value.description.trim() &&
    form.value.category &&
    Object.values(errors.value).every((error) => error === '')
  )
})

// 头像上传
const triggerAvatarUpload = () => {
  avatarInput.value?.click()
}

const handleAvatarUpload = async (event: Event) => {
  const target = event.target as HTMLInputElement
  const file = target.files?.[0]
  if (!file) return

  // 验证文件大小（5MB）
  if (file.size > 5 * 1024 * 1024) {
    alert('头像文件大小不能超过5MB')
    return
  }

  // 验证文件类型
  const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif']
  if (!allowedTypes.includes(file.type)) {
    alert('只支持 JPG、PNG、GIF 格式的图片')
    return
  }

  // 先显示本地预览
  const reader = new FileReader()
  reader.onload = (e) => {
    avatarPreview.value = e.target?.result as string
  }
  reader.readAsDataURL(file)

  // 存储文件，等创建社区成功后上传
  form.value.avatarFile = file
}

const removeAvatar = () => {
  avatarPreview.value = ''
  if (avatarInput.value) {
    avatarInput.value.value = ''
  }
}

// 横幅上传
const triggerBannerUpload = () => {
  bannerInput.value?.click()
}

const handleBannerUpload = async (event: Event) => {
  const target = event.target as HTMLInputElement
  const file = target.files?.[0]
  if (!file) return

  // 验证文件大小（10MB）
  if (file.size > 10 * 1024 * 1024) {
    alert('横幅文件大小不能超过10MB')
    return
  }

  // 验证文件类型
  const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif']
  if (!allowedTypes.includes(file.type)) {
    alert('只支持 JPG、PNG、GIF 格式的图片')
    return
  }

  // 先显示本地预览
  const reader = new FileReader()
  reader.onload = (e) => {
    bannerPreview.value = e.target?.result as string
  }
  reader.readAsDataURL(file)

  // 存储文件，等创建社区成功后上传
  form.value.bannerFile = file
}

const removeBanner = () => {
  bannerPreview.value = ''
  if (bannerInput.value) {
    bannerInput.value.value = ''
  }
}

// 规则管理
const addRule = () => {
  form.value.rules.push({ title: '', content: '' })
}

const removeRule = (index: number) => {
  form.value.rules.splice(index, 1)
}

// 返回操作
const goBack = () => {
  router.go(-1)
}

// 提交表单
const handleSubmit = async (): Promise<void> => {
  if (!validateForm()) return

  try {
    console.log('开始创建社区...')

    const createData = {
      name: form.value.name.trim(),
      description: form.value.description.trim(),
      categories: form.value.category,
      isPrivate: form.value.isPrivate,
    }

    console.log('发送的数据:', createData)

    // 1. 创建社区
    const response = await CircleAPI.createCircle(createData)
    console.log('创建社区响应:', response)

    // 获取新创建的社区ID
    let newCircleId: number | null = null

    if (response) {
      newCircleId =
        response.circleId ||
        response.id ||
        (response.data && (response.data.circleId || response.data.id))

      console.log('提取的社区ID:', newCircleId)
    }

    if (!newCircleId) {
      throw new Error('创建成功但未获取到社区ID')
    }

    // 2. 上传头像
    if (form.value.avatarFile) {
      try {
        console.log('准备上传头像，文件信息:', {
          name: form.value.avatarFile.name,
          size: form.value.avatarFile.size,
          type: form.value.avatarFile.type,
        })
        const avatarResult = await CircleAPI.uploadCircleAvatar(newCircleId, form.value.avatarFile)
        console.log('头像上传成功，返回结果:', avatarResult)

        if (avatarResult.success && avatarResult.data?.imageUrl) {
          console.log('头像上传成功！')
          console.log('头像URL:', avatarResult.data.imageUrl)
          console.log('文件名:', avatarResult.data.fileName)
          console.log('文件大小:', avatarResult.data.fileSize)

          // 验证URL是否可访问
          const testImg = new Image()
          testImg.onload = () => console.log('头像URL可访问')
          testImg.onerror = () => console.log('头像URL无法访问')
          testImg.src = avatarResult.data.imageUrl
        } else {
          console.log('头像上传响应格式异常:', avatarResult)
        }
      } catch (error) {
        console.error('头像上传失败:', error)
        alert(`头像上传失败: ${error}`)
      }
    }

    // 3. 上传横幅
    if (form.value.bannerFile) {
      try {
        console.log('准备上传横幅，文件信息:', {
          name: form.value.bannerFile.name,
          size: form.value.bannerFile.size,
          type: form.value.bannerFile.type,
        })
        const bannerResult = await CircleAPI.uploadCircleBanner(newCircleId, form.value.bannerFile)
        console.log('横幅上传成功，返回结果:', bannerResult)

        if (bannerResult.success && bannerResult.data?.imageUrl) {
          console.log('横幅上传成功！')
          console.log('横幅URL:', bannerResult.data.imageUrl)
          console.log('文件名:', bannerResult.data.fileName)
          console.log('文件大小:', bannerResult.data.fileSize)

          // 验证URL是否可访问
          const testImg = new Image()
          testImg.onload = () => console.log('横幅URL可访问')
          testImg.onerror = () => console.log('横幅URL无法访问')
          testImg.src = bannerResult.data.imageUrl
        } else {
          console.log('横幅上传响应格式异常:', bannerResult)
        }
      } catch (error) {
        console.error('横幅上传失败:', error)
        alert(`横幅上传失败: ${error}`)
      }
    }

    // 4. 自动加入社区
    try {
      console.log('自动加入社区，ID:', newCircleId)
      await CircleAPI.joinCircle(newCircleId)
      console.log('加入社区成功')
    } catch (joinError) {
      console.error('自动加入失败:', joinError)
    }

    alert('社区创建成功！')

    // 5. 跳转到社区详情页
    await router.push(`/community/${newCircleId}`)
  } catch (error: unknown) {
    console.error('创建社区出错:', error)

    if (error instanceof Error) {
      alert(`创建失败: ${error.message}`)
    } else {
      alert('创建失败: 未知错误')
    }
  }
}
</script>

<style scoped>
/* 创建社区页面样式 */
.create-community-container {
  max-width: 1200px;
  margin: 0 auto;
  padding: 24px 20px;
  display: flex;
  flex-direction: column;
  gap: 24px;
  min-height: 100vh;
}

.create-community-content {
  background: #fff;
  border-radius: 12px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.04);
  overflow: visible;
  width: 100%;
  flex: 1;
}

/* 页面头部 */
.page-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 20px 24px;
  border-bottom: 1px solid #e4e6ea;
  background: #fafbfc;
}

.back-btn {
  background: none;
  border: none;
  color: #4e5969;
  font-size: 16px;
  cursor: pointer;
  padding: 8px 12px;
  border-radius: 6px;
  transition: background 0.2s;
}

.back-btn:hover {
  background: #f2f3f5;
}

.page-title {
  font-size: 24px;
  font-weight: 600;
  color: #1d2129;
  margin: 0;
}

.create-btn {
  padding: 10px 20px;
  font-size: 16px;
  font-weight: 500;
  border: none;
  border-radius: 6px;
  cursor: pointer;
  transition: all 0.2s;
}

.create-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

/* 表单样式 */
.create-form {
  padding: 24px;
  max-height: none;
  overflow: visible;
}

.form-section {
  margin-bottom: 24px;
}

.form-label {
  display: block;
  font-weight: 500;
  color: #1d2129;
  margin-bottom: 8px;
  font-size: 16px;
}

.form-label.required::after {
  content: ' *';
  color: #f53f3f;
}

.form-input,
.form-textarea,
.form-select {
  width: 100%;
  padding: 12px 16px;
  border: 1px solid #e4e6ea;
  border-radius: 8px;
  font-size: 14px;
  transition: border-color 0.2s;
  box-sizing: border-box;
}

.form-input:focus,
.form-textarea:focus,
.form-select:focus {
  outline: none;
  border-color: #1677ff;
  box-shadow: 0 0 0 2px rgba(22, 119, 255, 0.1);
}

.form-input.error,
.form-textarea.error,
.form-select.error {
  border-color: #f53f3f;
}

.form-select:disabled {
  background-color: #f7f8fa;
  cursor: not-allowed;
}

.form-textarea {
  resize: vertical;
  min-height: 80px;
}

.form-help {
  margin: 4px 0 0 0;
  font-size: 12px;
  color: #86909c;
}

.error-message {
  margin: 4px 0 0 0;
  font-size: 12px;
  color: #f53f3f;
}

/* 上传组件样式 */
.upload-section {
  margin-bottom: 8px;
}

.upload-placeholder,
.banner-placeholder {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  border: 2px dashed #e4e6ea;
  border-radius: 8px;
  cursor: pointer;
  transition: all 0.2s;
  background: #fafbfc;
}

.upload-placeholder {
  width: 120px;
  height: 120px;
}

.banner-placeholder {
  width: 100%;
  height: 160px;
}

.upload-placeholder:hover,
.banner-placeholder:hover {
  border-color: #1677ff;
  background: #f0f8ff;
}

.upload-icon {
  font-size: 24px;
  margin-bottom: 8px;
}

.upload-preview,
.banner-preview {
  position: relative;
  display: inline-block;
}

.preview-image {
  width: 120px;
  height: 120px;
  object-fit: cover;
  border-radius: 8px;
  border: 1px solid #e4e6ea;
}

.banner-image {
  width: 100%;
  height: 160px;
  object-fit: cover;
  border-radius: 8px;
  border: 1px solid #e4e6ea;
}

.remove-image {
  position: absolute;
  top: -8px;
  right: -8px;
  width: 24px;
  height: 24px;
  border-radius: 50%;
  background: #f53f3f;
  color: white;
  border: none;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 16px;
  line-height: 1;
}

/* 复选框样式 */
.checkbox-group {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.checkbox-item {
  display: flex;
  align-items: flex-start;
  gap: 8px;
  cursor: pointer;
}

.form-checkbox {
  margin-top: 2px;
}

.checkbox-text {
  font-weight: 500;
  color: #1d2129;
}

.checkbox-help {
  color: #86909c;
  font-size: 12px;
  margin-left: auto;
}

/* 规则管理样式 */
.rules-section {
  space-y: 16px;
}

.rule-item {
  display: flex;
  gap: 12px;
  align-items: flex-start;
  padding: 16px;
  border: 1px solid #e4e6ea;
  border-radius: 8px;
  margin-bottom: 16px;
}

.rule-inputs {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.rule-title {
  font-weight: 500;
}

.rule-content {
  font-size: 14px;
  min-height: 60px;
}

.remove-rule-btn {
  background: #f53f3f;
  color: white;
  border: none;
  padding: 6px 12px;
  border-radius: 4px;
  cursor: pointer;
  font-size: 12px;
  height: fit-content;
}

.add-rule-btn {
  width: 100%;
  margin-top: 8px;
}

/* 按钮样式 */
.btn {
  padding: 8px 16px;
  border: none;
  border-radius: 6px;
  font-size: 14px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s;
  text-align: center;
  text-decoration: none;
  display: inline-block;
}

.btn-primary {
  background: #1677ff;
  color: #fff;
}

.btn-primary:hover:not(:disabled) {
  background: #0958d9;
}

.btn-outline {
  background: transparent;
  color: #1677ff;
  border: 1px solid #1677ff;
}

.btn-outline:hover {
  background: #1677ff;
  color: #fff;
}

/* 侧边栏样式 */
.sidebar {
  display: flex;
  flex-direction: column;
  gap: 24px;
}

.sidebar-card {
  background: #fff;
  border-radius: 12px;
  padding: 20px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.04);
}

.sidebar-title {
  font-size: 18px;
  font-weight: 600;
  color: #1d2129;
  margin-bottom: 16px;
}

.guide-list {
  list-style: none;
  padding: 0;
  margin: 0;
}

.guide-list li {
  padding: 8px 0;
  color: #4e5969;
  font-size: 14px;
  line-height: 1.5;
  position: relative;
  padding-left: 20px;
}

.guide-list li::before {
  content: '•';
  color: #1677ff;
  font-weight: bold;
  position: absolute;
  left: 0;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .create-community-container {
    padding: 16px;
    gap: 16px;
    display: flex;
    flex-direction: column;
  }

  .page-header {
    padding: 16px;
    flex-wrap: wrap;
    gap: 8px;
  }

  .page-title {
    font-size: 20px;
  }

  .create-form {
    padding: 16px;
  }

  .rule-item {
    flex-direction: column;
    gap: 8px;
  }

  .remove-rule-btn {
    align-self: flex-start;
  }

  .sidebar {
    order: -1;
  }
}
</style>
