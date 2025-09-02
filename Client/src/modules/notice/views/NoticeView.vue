 <template>
  <div class="page-content-wrapper">
    <div class="divider-vertical" aria-hidden="true"></div>

    <div class="center">
      <div class="divider-horizontal"></div>
      <div class="notice-heading">
        <span>通知 > {{ noticeTypeTexts[selectedNoticeType] }}</span>
      </div>
      <div v-if="loadingUnread">加载中...</div>
      <div v-else-if="unreadError">{{ unreadError }}</div>
      <div class="divider-horizontal"></div>

      <div class="notice-type">
        <button
          v-for="(text, index) in noticeTypeTexts"
          :key="index"
          :class="{ active: selectedNoticeType === index }"
          @click="selectedNoticeType = index"
          class="notice-tab-button"
        >
          {{ text }}
          <span
            v-if="index !== 0 && getUnreadCountByType(index) > 0"
            class="unread-notice-type-count"
          >
            {{ displayUnreadNoticeCount(getUnreadCountByType(index)) }}
          </span>
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
import { ref, computed, onMounted, watch } from 'vue'
import { useRouter } from 'vue-router'
import { type notice, type unreadNoticeCount } from '../types'
import NoticeItem from '../components/NoticeItem.vue'
import { noticeList as noticeData } from '../noticeData'
import { getUnreadNoticeCount, markNotificationsRead } from '../api'

const selectedNoticeType = ref(0)
const searchText = ref('')
const noticeTypeTexts = ['全部消息', '@我', '回复我的', '收到的赞']
const router = useRouter()
const noticeList = ref<notice[]>(noticeData)
const unreadSummary = ref<unreadNoticeCount | null>(null)
const loadingUnread = ref(false)
const unreadError = ref<string | null>(null)
const readOnce = ref<Set<string>>(new Set())

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

// 显示未读通知数量
const getUnreadCountByType = (index: number) => {
  if (!unreadSummary.value) return 0
  const u = unreadSummary.value.unreadByType

  if (index === 1) return u.mention || 0
  if (index === 2) return u.reply || 0
  if (index === 3) return u.like || 0
  return 0
}

// 显示未读通知数量
const displayUnreadNoticeCount = (n: number) => (n > 99 ? '99+' : n)

// 将索引转换为类型
const indexToType = (idx: number): 'mention' | 'reply' | 'like' | 'repost' | null => {
  if (idx === 1) return 'mention'
  if (idx === 2) return 'reply'
  if (idx === 3) return 'like'
  return null
}

onMounted(async () => {
  loadingUnread.value = true
  try {
    unreadSummary.value = await getUnreadNoticeCount()
  } catch (err: any) {
    unreadError.value = err?.message ?? '加载未读通知失败'
  } finally {
    loadingUnread.value = false
  }
})

watch(
  selectedNoticeType,
  async (idx) => {
    const t = indexToType(idx)
    if (!t) return
    if (readOnce.value.has(t)) return
    try {
      await markNotificationsRead(t)
      readOnce.value.add(t)

      // 本地即时更新未读计数，立刻隐藏红点
      if (unreadSummary.value) {
        const delta = unreadSummary.value.unreadByType[t] || 0
        unreadSummary.value.unreadByType[t] = 0
        unreadSummary.value.totalUnread = Math.max(
          0,
          (unreadSummary.value.totalUnread || 0) - delta
        )
      }
    } catch (e) {
      // 可选：提示失败，不影响继续浏览
      console.error('标记已读失败', e)
    }
  },
  { immediate: false }
)
</script>

<style scoped>
.page-content-wrapper {
  display: flex;
  flex-direction: row;
  justify-content: center;
}

.center {
  width: 60%;
  display: flex;
  flex-direction: column;
  overflow-wrap: break-word;
  word-break: break-all;
}

.notice-heading {
  display: flex;
  padding: 12px 6px;
}

.notice-type {
  display: flex;
  padding: 16px 0;
}
.notice-type button {
  flex: 1;
  text-align: center;
  cursor: pointer;
  transition: color 0.2s ease;
}

.notice-tab-button {
  position: relative;
}

.unread-notice-type-count {
  position: absolute;
  top: -8px;
  right: 6px;
  min-width: 20px;
  height: 20px;
  border-radius: 99px;
  background: #ef4444;
  color: #fff;
  font-size: 12px;
  text-align: center;
  justify-content: center;
}

.notice-type button:hover {
  color: #00aeec;
}

.notice-type button.active {
  font-weight: bold;
  color: #00aeec;
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