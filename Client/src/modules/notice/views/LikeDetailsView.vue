<template>
  <div class="flex flex-row">
    <div class="divider-vertical" aria-hidden="true"></div>

    <div class="center w-full">
      <div class="divider-horizontal"></div>

      <div
        class="text-3xl font-bold px-10 py-4 text-slate-200 flex justify-left items-center gap-4"
      >
        <button
          @click="goBackToNotice"
          class="flex items-center gap-2 text-blue-400 hover:text-blue-300 transition-colors text-lg font-normal"
          title="返回通知页面"
        >
          <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
            <path
              stroke-linecap="round"
              stroke-linejoin="round"
              stroke-width="2"
              d="M15 19l-7-7 7-7"
            ></path>
          </svg>
          <span>返回</span>
        </button>
        <span>点赞详情</span>
      </div>

      <div class="divider-horizontal"></div>

      <div class="bg-slate-900/30 p-4 md:p-6">
        <div class="flex items-center gap-3">
          <span class="text-slate-200 font-medium">
            {{ targetType === 'post' ? '帖子' : '评论' }}：
          </span>
          <span
            class="text-blue-400 cursor-pointer hover:text-blue-300 transition-colors"
            @click="goToTarget"
            :title="`点击查看${targetType === 'post' ? '帖子' : '评论'}详情`"
          >
            {{ targetTitle }}
          </span>
        </div>
      </div>

      <div class="divider-horizontal"></div>

      <div class="notice-information">
        <div v-if="loading" class="text-center text-slate-500 text-md py-3">
          <p>加载中...</p>
        </div>
        <div v-else-if="error" class="text-center text-red-500 text-md py-3">
          <p>{{ error }}</p>
        </div>
        <div
          v-else-if="filteredLikeUsers.length === 0"
          class="text-center text-slate-500 text-md py-3"
        >
          <p>暂无点赞用户</p>
        </div>
        <div v-else class="notice-list">
          <div v-for="user in filteredLikeUsers" :key="user.id" class="notice-item">
            <div class="notice-main-content">
              <!-- 左侧图标和头像 -->
              <div class="notice-left">
                <span class="icon">👍</span>
                <img
                  class="user-avatar clickable-avatar"
                  v-if="user.avatar && user.avatar.trim() !== ''"
                  :src="user.avatar"
                  :alt="user.username"
                  @click="handleAvatarClick(user)"
                  @error="handleAvatarError"
                />
                <span
                  v-else
                  class="clickable-avatar avatar-fallback"
                  @click="handleAvatarClick(user)"
                >
                  {{ user.username.charAt(0).toUpperCase() }}
                </span>
              </div>

              <!-- 通知内容 -->
              <div class="notice-content">
                <div class="notice-main">
                  <span class="username">{{ user.username }}</span>
                  <span class="action">赞了我</span>
                </div>

                <div class="other-info">
                  <span class="notice-time">{{ user.time }}</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div class="divider-horizontal"></div>
    </div>

    <div class="divider-vertical" aria-hidden="true"></div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { getLikersByTarget, getPostDetail, getCommentDetail, getUserInfo } from '../api'
import { getUserDetail } from '../../message/api'
import { unwrap } from '../types'

const route = useRoute()
const router = useRouter()
const targetType = ref(route.params.targetType as 'post' | 'comment')
const targetId = ref(Number(route.params.targetId))
const targetTitle = ref('')
const searchText = ref('')
const likeUsers = ref<
  Array<{
    id: number
    username: string
    avatar: string
    time: string
  }>
>([])
const loading = ref(false)
const error = ref<string | null>(null)

// 获取目标标题（帖子或评论）
async function getTargetTitle() {
  try {
    if (targetType.value === 'post') {
      const postDetailResp = await getPostDetail(targetId.value)
      const postDetail = unwrap(postDetailResp)
      targetTitle.value = postDetail.title || `帖子${targetId.value}`
    } else if (targetType.value === 'comment') {
      const commentDetailResp = await getCommentDetail(targetId.value)
      const commentDetail = unwrap(commentDetailResp)
      // 评论标题显示为评论内容的前50个字符
      targetTitle.value =
        commentDetail.content.length > 50
          ? commentDetail.content.substring(0, 50) + '...'
          : commentDetail.content
    }
  } catch (err) {
    console.error('[LikeDetailsView] 获取目标标题失败:', err)
    targetTitle.value = `${targetType.value === 'post' ? '帖子' : '评论'}${
      targetId.value
    } (资源不存在)`
  }
}

// 获取点赞用户列表
async function getLikeUsersList() {
  loading.value = true
  error.value = null

  try {
    // 直接调用API获取点赞者信息
    const likersResp = await getLikersByTarget({
      targetType: targetType.value,
      targetId: targetId.value,
      page: 1,
      pageSize: 100,
    })
    const likersData = unwrap(likersResp)

    if (likersData.items.length === 0) {
      likeUsers.value = []
      return
    }

    // 获取所有点赞者的详细信息
    const MEDIA_BASE_URL = 'http://localhost:5000/api/media'
    likeUsers.value = await Promise.all(
      likersData.items.map(async (userId) => {
        const userDetail = await getUserDetail(userId)
        const avatarUrl = userDetail.avatarUrl
          ? `${MEDIA_BASE_URL}/${userDetail.avatarUrl}`
          : 'https://placehold.co/100x100/facc15/78350f?text=U'

        return {
          id: userId,
          username: userDetail.nickname,
          avatar: avatarUrl,
          time: '刚刚', // API中没有提供点赞时间，使用默认值
        }
      })
    )
  } catch (err: any) {
    console.error('[LikeDetailsView] 获取点赞用户列表失败:', err)
    console.error('[LikeDetailsView] 错误详情:', {
      message: err?.message,
      status: err?.response?.status,
      statusText: err?.response?.statusText,
      data: err?.response?.data,
    })
    error.value = err?.message ?? '获取点赞用户列表失败'
  } finally {
    loading.value = false
  }
}

// 过滤后的点赞用户列表
const filteredLikeUsers = computed(() => {
  if (searchText.value.trim()) {
    const searchLower = searchText.value.toLowerCase()
    return likeUsers.value.filter((user) => user.username.toLowerCase().includes(searchLower))
  }
  return likeUsers.value
})

// 返回到通知页面
const goBackToNotice = () => {
  router.push('/notice/like')
}

// 跳转到目标页面（帖子或评论）
const goToTarget = () => {
  if (targetType.value === 'post') {
    // 跳转到帖子详情页面
    router.push(`/post/${targetId.value}`)
  } else if (targetType.value === 'comment') {
    // 跳转到评论详情页面（如果有的话）
    router.push(`/comment/${targetId.value}`)
  }
}

// 点击用户头像
const handleAvatarClick = (user: any) => {
  // 跳转到用户主页
  window.open(`/user/${user.id}`, '_blank')
}

// 头像加载失败处理
const handleAvatarError = (event: Event) => {
  console.warn('头像加载失败:', (event.target as HTMLImageElement)?.src)
  // 隐藏图片，显示fallback文字
  const img = event.target as HTMLImageElement
  img.style.display = 'none'
}

// 调试函数：手动测试API调用
const testApiCalls = async () => {
  console.log('[LikeDetailsView] 开始手动测试API调用')

  try {
    console.log('[LikeDetailsView] 测试getLikersByTarget...')
    const testResp = await getLikersByTarget({
      targetType: 'post',
      targetId: 99999,
      page: 1,
      pageSize: 10,
    })
    console.log('[LikeDetailsView] getLikersByTarget测试结果:', testResp)
  } catch (error) {
    console.error('[LikeDetailsView] getLikersByTarget测试失败:', error)
  }

  try {
    console.log('[LikeDetailsView] 测试getPostDetail...')
    const testResp = await getPostDetail(99999)
    console.log('[LikeDetailsView] getPostDetail测试结果:', testResp)
  } catch (error) {
    console.error('[LikeDetailsView] getPostDetail测试失败:', error)
  }
}

// 暴露测试函数到全局，方便在控制台调用
if (typeof window !== 'undefined') {
  ;(window as any).testLikeDetailsApi = testApiCalls
}

onMounted(async () => {
  try {
    await Promise.all([getTargetTitle(), getLikeUsersList()])
  } catch (error) {
    console.error('[LikeDetailsView] 数据获取失败:', error)
  }
})
</script>



<style scoped>
.center {
  display: flex;
  flex-direction: column;
  overflow-wrap: break-word;
  word-break: break-all;
}

.notice-information {
  flex: 1;
  display: flex;
  flex-direction: column;
  overflow-y: auto;
}

.notice-list {
  display: flex;
  flex-direction: column;
  width: 100%;
}

.notice-item {
  display: flex;
  flex-direction: column;
  width: 100%;
  padding: 12px 16px;
  border-bottom: 1px solid #1e293b;
  transition: background-color 0.2s;
  cursor: pointer;
  box-sizing: border-box;
}

.notice-item:hover {
  background-color: #1e293b;
}

.notice-main-content {
  display: flex;
}

.notice-left {
  display: flex;
  align-items: center;
  margin-right: 24px;
}

.icon {
  margin-right: 8px;
  font-size: 24px;
  color: #4a9eff;
}

.user-avatar {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 48px;
  height: 48px;
  border-radius: 100%;
}

.avatar-fallback {
  display: flex;
  align-items: center;
  justify-content: center;
  width: 48px;
  height: 48px;
  border-radius: 100%;
  background-color: #4a9eff;
  color: white;
  font-weight: bold;
  font-size: 18px;
}

.clickable-avatar {
  cursor: pointer;
  transition: opacity 0.2s ease;
}

.clickable-avatar:hover {
  opacity: 0.8;
}

.notice-content {
  display: flex;
  flex: 1;
  flex-direction: column;
  justify-content: center;
  align-items: flex-start;
}

.notice-main {
  display: flex;
  align-items: center;
  font-size: 14px;
  gap: 8px;
}

.username {
  font-weight: bold;
}

.action {
  color: #a0aec0;
}

.other-info {
  display: flex;
  align-items: center;
  width: 100%;
  padding: 0px;
}

.notice-time {
  color: #9499a0;
  font-size: 12px;
  padding: 0px 4px;
}

.divider-horizontal {
  height: 1px;
  background: #444c5c;
  width: 100%;
}

.divider-vertical {
  width: 1px;
  background-color: #444c5c;
  align-self: stretch;
}
</style>