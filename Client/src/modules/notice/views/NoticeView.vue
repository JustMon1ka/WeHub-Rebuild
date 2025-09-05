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
          @click="onTabChange(index)"
          class="notice-tab-button"
        >
          {{ text }}
          <span v-if="getUnreadCountByType(index) > 0" class="unread-notice-type-count">
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
import { ref, computed, onMounted, watch, defineAsyncComponent } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { type notice, type unreadNoticeCount } from '../types'
const NoticeItem = defineAsyncComponent(() => import('../components/NoticeItem.vue'))
import { noticeList as noticeData } from '../noticeData'
import { getUnreadNoticeCount, markNotificationsRead } from '../api'

const selectedNoticeType = ref(-1)
const searchText = ref('')
const noticeTypeTexts = ['@我', '回复我的', '收到的赞']
const noticeList = ref<notice[]>(noticeData)
const unreadSummary = ref<unreadNoticeCount | null>(null)
const loadingUnread = ref(false)
const unreadError = ref<string | null>(null)
const readOnce = ref<Set<string>>(new Set())

const route = useRoute()
const router = useRouter()

const typeToIndex: Record<string, number> = { mention: 0, reply: 1, like: 2 }
const idxToType: Record<number, string> = { 0: 'mention', 1: 'reply', 2: 'like' }

// 提前声明，避免在路由监听中未定义导致白屏
function indexToType(idx: number): 'mention' | 'reply' | 'like' | null {
  if (idx === 0) return 'mention'
  if (idx === 1) return 'reply'
  if (idx === 2) return 'like'
  return null
}

function optimisticMarkRead(type: 'mention' | 'reply' | 'like') {
  if (readOnce.value.has(type)) return
  readOnce.value.add(type)
  if (unreadSummary.value) {
    const delta = unreadSummary.value.unreadByType[type] || 0
    unreadSummary.value.unreadByType[type] = 0
    unreadSummary.value.totalUnread = Math.max(0, (unreadSummary.value.totalUnread || 0) - delta)
  }
}

// 路由 → tab，同步初始与后续变化
watch(
  () => route.params.type,
  (t) => {
    const mapped = typeToIndex[String(t)] ?? 0 // 无type时默认 mention
    selectedNoticeType.value = mapped
    const type = indexToType(mapped) as 'mention' | 'reply' | 'like' | null
    if (type) {
      optimisticMarkRead(type)
      markNotificationsRead(type).catch((e) => console.error('标记已读失败', e))
    }
  },
  { immediate: true }
)

// tab → 路由
function onTabChange(index: number) {
  selectedNoticeType.value = index
  const t = idxToType[index]
  if (t) router.replace({ path: `/notice/${t}` })
}

// 筛选通知
const selectedNotices = computed(() => {
  const typeMap = ['at', 'comment', 'like'] as const
  const selectedType = typeMap[selectedNoticeType.value]
  let selectedNoticeList = noticeList.value.filter((n) => n.type === selectedType)

  if (searchText.value.trim()) {
    const searchLower = searchText.value.toLowerCase()
    selectedNoticeList = selectedNoticeList.filter(
      (n) =>
        n.sender.nickname.toLowerCase().includes(searchLower) ||
        n.targetPostTitle.toLowerCase().includes(searchLower)
    )
  }
  // 只有在“点赞”页才合并同帖多次点赞
  if (selectedType === 'like') {
    const likeNoticesByPost = new Map<number, notice[]>()
    selectedNoticeList.forEach((n) => {
      if (!likeNoticesByPost.has(n.targetPostId)) likeNoticesByPost.set(n.targetPostId, [])
      likeNoticesByPost.get(n.targetPostId)!.push(n)
    })

    const processed: notice[] = []
    likeNoticesByPost.forEach((likes) => {
      if (likes.length > 1) {
        likes.sort((a, b) => +new Date(b.time) - +new Date(a.time))
        const lastLiker = likes[0]
        processed.push({
          ...lastLiker,
          sender: {
            ...lastLiker.sender,
            nickname: `${lastLiker.sender.nickname} 等${likes.length}人`,
          },
        })
      } else {
        processed.push(likes[0])
      }
    })
    return processed.sort((a, b) => +new Date(b.time) - +new Date(a.time))
  }

  return selectedNoticeList.sort((a, b) => +new Date(b.time) - +new Date(a.time))
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
  const type = indexToType(index)
  if (type && readOnce.value.has(type)) return 0
  if (!unreadSummary.value) return 0
  const u = unreadSummary.value.unreadByType
  if (index === 0) return u.mention || 0
  if (index === 1) return u.reply || 0
  if (index === 2) return u.like || 0
  return 0
}

// 显示未读通知数量
const displayUnreadNoticeCount = (n: number) => (n > 99 ? '99+' : n)

onMounted(async () => {
  loadingUnread.value = true
  try {
    unreadSummary.value = await getUnreadNoticeCount()
    // 若该类型在本地已标记已读，避免接口旧数据回显红点
    for (const t of ['mention', 'reply', 'like'] as const) {
      if (readOnce.value.has(t)) {
        unreadSummary.value.unreadByType[t] = 0
      }
    }
    unreadSummary.value.totalUnread =
      (unreadSummary.value.unreadByType.mention || 0) +
      (unreadSummary.value.unreadByType.reply || 0) +
      (unreadSummary.value.unreadByType.like || 0)
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

    // 先本地乐观清零，立刻隐藏红点
    readOnce.value.add(t)
    if (unreadSummary.value) {
      const delta = unreadSummary.value.unreadByType[t] || 0
      unreadSummary.value.unreadByType[t] = 0
      unreadSummary.value.totalUnread = Math.max(0, (unreadSummary.value.totalUnread || 0) - delta)
    }

    try {
      await markNotificationsRead(t)
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
  align-self: stretch;
}
</style>