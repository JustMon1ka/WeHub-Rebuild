<template>
  <div class="page-content-wrapper">
    <div class="left">
      <SideNavigationBar />
    </div>

    <div class="divider-vertical" aria-hidden="true"></div>

    <div class="center">
      <div class="divider-horizontal"></div>
      <div class="notice-heading">
        <span>通知 > {{ noticeTypeTexts[selectedNoticeType] }}</span>
      </div>
      <div class="divider-horizontal"></div>

      <div class="notice-type">
        <button
          v-for="(text, index) in noticeTypeTexts"
          :key="index"
          :class="{ active: selectedNoticeType === index }"
          @click="selectedNoticeType = index"
        >
          {{ text }}
        </button>
      </div>
      <div class="divider-horizontal"></div>

      <div class="notice-information">
        <div v-if="selectedNotices.length === 0" class="empty-state">
          <p>暂无通知</p>
        </div>
        <div v-else class="notice-list">
          <NoticeItem
            v-for="notice in selectedNotices"
            :key="`${notice.sender.id}-${notice.time}-${notice.type}`"
            :notice="notice"
            :like-count="notice.type === 'like' ? likeCountMap[notice.targetPostId] : undefined"
            :like-notices="getLikeNoticesForPost(notice.targetPostId)"
            @show-like-details="handleShowLikeDetailsClick"
          />
        </div>
      </div>

      <div class="divider-horizontal"></div>
    </div>

    <div class="divider-vertical" aria-hidden="true"></div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRouter } from 'vue-router'
import { type notice } from '../types'
import NoticeItem from '../components/NoticeItem.vue'
import { noticeList as noticeData } from '../noticeData'

const selectedNoticeType = ref(0)
const searchText = ref('')
const noticeTypeTexts = ['全部消息', '@我', '回复我的', '收到的赞']
const router = useRouter()

const noticeList = ref<notice[]>(noticeData)

// 筛选通知
const selectedNotices = computed(() => {
  let selectedNoticeList = noticeList.value

  if (selectedNoticeType.value > 0) {
    const typeMap = [null, 'at', 'comment', 'like']
    const selectedType = typeMap[selectedNoticeType.value]
    if (selectedType !== null) {
      selectedNoticeList = selectedNoticeList.filter((notice) => notice.type === selectedType)
    }
  }

  if (searchText.value.trim()) {
    const searchLower = searchText.value.toLowerCase()
    selectedNoticeList = selectedNoticeList.filter(
      (notice) =>
        notice.sender.nickname.toLowerCase().includes(searchLower) ||
        notice.targetPostTitle.toLowerCase().includes(searchLower)
    )
  }

  // 对于点赞通知，合并同一个帖子的多个点赞
  if (selectedNoticeType.value === 3 || selectedNoticeType.value === 0) {
    // 收到的赞 或 全部消息
    const likeNoticesByPost = new Map<number, notice[]>()

    // 按帖子ID分组点赞通知
    selectedNoticeList.forEach((notice) => {
      if (notice.type === 'like') {
        if (!likeNoticesByPost.has(notice.targetPostId)) {
          likeNoticesByPost.set(notice.targetPostId, [])
        }
        likeNoticesByPost.get(notice.targetPostId)!.push(notice)
      }
    })

    // 处理每个帖子的点赞通知
    const processedNotices: notice[] = []

    // 先处理非点赞通知
    selectedNoticeList.forEach((notice) => {
      if (notice.type !== 'like') {
        processedNotices.push(notice)
      }
    })

    // 再处理点赞通知
    likeNoticesByPost.forEach((likes, postId) => {
      if (likes.length > 1) {
        // 如果这个帖子有多个点赞，先按时间排序，找到最新的点赞
        const sortedLikes = likes.sort((a, b) => {
          const timeA = new Date(a.time).getTime()
          const timeB = new Date(b.time).getTime()
          return timeB - timeA // 降序排列，最新的在前
        })

        const lastLiker = sortedLikes[0] // 最新的点赞者

        // 创建合并后的通知
        const mergedNotice: notice = {
          ...lastLiker,
          sender: {
            ...lastLiker.sender,
            nickname: `${lastLiker.sender.nickname} 等${likes.length}人`,
          },
        }

        processedNotices.push(mergedNotice)
      } else {
        // 单个点赞，直接添加
        processedNotices.push(likes[0])
      }
    })

    // 对处理后的通知按时间排序
    return processedNotices.sort((a, b) => {
      const timeA = new Date(a.time).getTime()
      const timeB = new Date(b.time).getTime()
      return timeB - timeA // 降序排列，新通知在前
    })
  }

  // 对非点赞通知也按时间排序
  return selectedNoticeList.sort((a, b) => {
    const timeA = new Date(a.time).getTime()
    const timeB = new Date(b.time).getTime()
    return timeB - timeA // 降序排列，新通知在前
  })
})

// 获取点赞数
const likeCountMap = computed(() => {
  const map: Record<number, number> = {}
  noticeList.value.forEach((notice) => {
    if (notice.type === 'like') {
      map[notice.targetPostId] = (map[notice.targetPostId] || 0) + 1
    }
  })
  return map
})

// 获取指定帖子的点赞信息
const getLikeNoticesForPost = (postId: number) => {
  return noticeList.value.filter(
    (notice) => notice.type === 'like' && notice.targetPostId === postId
  )
}

// 显示点赞详情
const handleShowLikeDetailsClick = (postId: number) => {
  router.push(`/notice/likeDetails/${postId}`)
}
</script>

<style scoped>
.page-content-wrapper {
  display: flex;
  flex-direction: row;
  /* 不要用 gap，否则分割线不会贴合 */
}

.left {
  width: 20%;
  display: flex;
  align-items: center;
  justify-content: center;
  padding-left: 16px;
}

.center {
  width: 60%;
  display: flex;
  flex-direction: column;
  overflow-wrap: break-word;
  word-break: break-all;
}

.notice-type {
  display: flex;
}
.notice-type button {
  flex: 1;
  text-align: center;
}
.notice-type button.active {
  font-weight: bold;
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

.divider-horizontal {
  height: 1px;
  background: #444c5c;
  width: 100%;
}

.divider-vertical {
  width: 1px;
  background-color: #444c5c;
  align-self: stretch; /* 让垂直线充满父容器高度并贴合 */
}
</style>