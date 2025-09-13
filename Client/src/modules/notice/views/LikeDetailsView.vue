<template>
  <div class="page-content-wrapper">
    <div class="divider-vertical"></div>
    <div class="center">
      <div class="divider-horizontal"></div>
      <div class="notice-heading">
        <span class="separator">通知 > </span>
        <span @click="goBackToNotice" class="back-link">收到的赞</span>
        <span> > 点赞详情</span>
      </div>
      <div class="divider-horizontal"></div>
      <div class="post-info">
        <span
          class="post-title clickable-title"
          @click="goToTarget"
          :title="`点击查看${targetType === 'post' ? '帖子' : '评论'}详情`"
        >
          {{ targetType === 'post' ? '帖子' : '评论' }}：{{ targetTitle }}
        </span>
      </div>
      <div class="divider-horizontal"></div>

      <div class="like-users-list">
        <div v-if="loading" class="loading-state">
          <span>加载中...</span>
        </div>
        <div v-else-if="error" class="error-state">
          <span>{{ error }}</span>
        </div>
        <div v-else-if="filteredLikeUsers.length === 0" class="empty-state">
          <span>暂无点赞用户</span>
        </div>
        <div v-else>
          <div v-for="user in filteredLikeUsers" :key="user.id" class="like-user-item">
            <div class="item-left">
              <div class="user-avater">
                <img v-if="user.avatar" :src="user.avatar" :alt="user.username" />
                <span v-else class="avatar-placeholder">
                  {{ user.username.charAt(0).toUpperCase() }}
                </span>
              </div>
            </div>
            <div class="item-right">
              <div class="item-content">
                <span class="username">{{ user.username }}</span>
                <span class="action">赞了我</span>
              </div>
              <span class="time">{{ user.time }}</span>
            </div>
          </div>
        </div>
      </div>
      <div class="divider-horizontal"></div>
    </div>
    <div class="divider-vertical"></div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { getLikersByTarget, getPostDetail, getCommentDetail, getUserInfo } from '../api'
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
    likeUsers.value = await Promise.all(
      likersData.items.map(async (userId) => {
        const userDetail = await getUserDetail(userId)
        return {
          id: userId,
          username: userDetail.nickname,
          avatar: userDetail.avatarUrl,
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
.page-content-wrapper {
  display: flex;
  flex-direction: row;
  justify-content: center;
  margin: 20px 0; /* 添加上下边距 */
  height: calc(100vh - 40px); /* 减去上下边距的高度 */
}

.center {
  width: 60%;
  display: flex;
  flex-direction: column;
  overflow-wrap: break-word;
  word-break: break-word;
}

.notice-heading {
  display: flex;
  padding: 12px 32px;
  align-items: center;
  gap: 8px;
}

.back-link {
  cursor: pointer;
}

.back-link:hover {
  color: #3b82f6;
}

.post-info {
  flex: 1;
  display: flex;
  padding: 0px 32px;
  align-items: center;
}

.post-title {
  font-weight: bold;
  color: #333;
  align-items: center;
}

.clickable-title {
  color: #3b82f6 !important; /* 使用蓝色，更亮更明显 */
  cursor: pointer;
  transition: all 0.2s ease;
  text-decoration: none;
}

.clickable-title:hover {
  color: #1d4ed8 !important; /* 悬停时更深的蓝色 */
  text-decoration: underline;
  transform: translateY(-1px); /* 轻微上移效果 */
}

.clickable-title:active {
  color: #1e40af !important; /* 点击时的颜色 */
  transform: translateY(0); /* 点击时回到原位置 */
}

.like-users-list {
  flex: 10;
  overflow-y: auto;
}

.like-user-item {
  display: flex;
  padding: 12px 32px;
  border-bottom: 1px solid #f0f0f0;
  transition: background-color 0.2s;
}

.like-user-item:hover {
  background-color: #273549;
}

.item-left {
  display: flex;
  align-items: center;
  margin-right: 24px;
}

.user-avater {
  display: flex;
  align-items: center;
  justify-content: center;
}

.user-avater img {
  width: 36px;
  height: 36px;
  border-radius: 50%;
}

.item-right {
  display: flex;
  flex: 1;
  flex-direction: column;
  justify-content: center;
  align-items: flex-start;
}

.user-avatar img {
  width: 36px;
  height: 36px;
  border-radius: 50%;
  object-fit: cover;
}

.item-content {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-bottom: 4px;
}

.username {
  font-weight: bold;
  font-size: 16px;
}

.action {
  font-size: 14px;
  color: #61666d;
}

.time {
  color: #9499a0;
  font-size: 12px;
}

.loading-state,
.error-state,
.empty-state {
  display: flex;
  justify-content: center;
  align-items: center;
  height: 200px;
  color: #999;
  font-size: 16px;
}

.error-state {
  color: #ef4444;
}

.avatar-placeholder {
  width: 36px;
  height: 36px;
  border-radius: 50%;
  background-color: #e5e7eb;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: bold;
  color: #6b7280;
}

.right {
  width: 25%;
}

.divider-horizontal {
  width: 100%;
  border-bottom: 1px solid #444c5c;
}

.divider-vertical {
  width: 1px;
  background-color: #444c5c;
  margin: 0 0px;
}
</style>
