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
          <span
            v-if="getUnreadCountByType(index, unreadSummary, readOnce) > 0"
            class="unread-notice-type-count"
          >
            {{ displayUnreadNoticeCount(getUnreadCountByType(index, unreadSummary, readOnce)) }}
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
            :like-notices="getLikeNoticesForPost(notice.targetPostId, noticeList)"
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

const NoticeItem = defineAsyncComponent(() => import('../components/NoticeItem.vue'))
import { noticeList as noticeData } from '../noticeData'
import {
  type notice,
  type unreadNoticeCount,
  type replyNoticeItem,
  type repostNoticeItem,
  type replyNoticeResponse,
  type atNoticeItem,
  type likeNoticeItem,
  unwrap,
} from '../types'
import {
  getUnreadNoticeCount,
  markNotificationsRead,
  getLikeNotices,
  getReplyNotices,
  getRepostNotices,
  getAtNotices,
} from '../api'
import {
  indexToType,
  optimisticMarkRead,
  getLikeNoticesForPost,
  displayUnreadNoticeCount,
  getUnreadCountByType,
} from '../utils/noticeUtils'

const selectedNoticeType = ref(0)
const searchText = ref('')
const noticeTypeTexts = ['@我', '回复我的', '收到的赞', '转发我的']
const noticeList = ref<notice[]>(noticeData)

const loadingUnread = ref(false)

type UnreadSummaryData = unreadNoticeCount['data']
const unreadSummary = ref<UnreadSummaryData | null>(null)
const unreadError = ref<string | null>(null)
const readOnce = ref<Set<string>>(new Set())

const likeItems = ref<likeNoticeItem[]>([])
const replyItems = ref<replyNoticeItem[]>([])
const repostItems = ref<repostNoticeItem[]>([])
const atItems = ref<atNoticeItem[]>([])

const route = useRoute()
const router = useRouter()

const typeToIndex: Record<string, number> = { at: 0, reply: 1, like: 2, repost: 3 }
const idxToType: Record<number, string> = { 0: 'at', 1: 'reply', 2: 'like', 3: 'repost' }

// 路由 → tab，同步初始与后续变化
watch(
  () => route.params.type,
  (t) => {
    const mapped = typeToIndex[String(t)] ?? 0 // 无type时默认 at
    selectedNoticeType.value = mapped
    const type = indexToType(mapped) as 'at' | 'reply' | 'like' | 'repost' | null
    if (type) {
      optimisticMarkRead(type, readOnce.value, unreadSummary.value)
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

// 显示点赞详情
const handleShowLikeDetailsClick = (postId: number) => {
  router.push(`/notice/likeDetails/${postId}`)
}

onMounted(async () => {
  loadingUnread.value = true
  try {
    // 并行请求：未读汇总 + 点赞列表（示例）
    const [unreadResp, likeResp, replyResp, repostResp, atResp] = await Promise.all([
      getUnreadNoticeCount(),
      getLikeNotices({ page: 1, pageSize: 20 }),
      getReplyNotices({ page: 1, pageSize: 20, unreadOnly: false }),
      getRepostNotices({ page: 1, pageSize: 20, unreadOnly: false }),
      getAtNotices({ page: 1, pageSize: 20, unreadOnly: false }),
    ])

    // 处理未读数据
    unreadSummary.value = unwrap(unreadResp)
    for (const t of ['at', 'reply', 'like', 'repost'] as const) {
      if (readOnce.value.has(t)) {
        unreadSummary.value.unreadByType[t] = 0
      }
    }

    replyItems.value = unwrap(replyResp).items
    repostItems.value = unwrap(repostResp).items
    atItems.value = unwrap(atResp).items
    likeItems.value = unwrap(likeResp).unread

    // 处理点赞数据（如果需要使用）
    const likeData = unwrap(likeResp)
  } catch (err: any) {
    unreadError.value = err?.message ?? '加载通知失败'
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
    optimisticMarkRead(t, readOnce.value, unreadSummary.value)

    try {
      await markNotificationsRead(t)
    } catch (e) {
      // 可选：提示失败，不影响继续浏览
      console.error('标记已读失败', e)
    }
  },
  { immediate: false }
)

// 筛选通知
const selectedNotices = computed(() => {
  const typeMap = ['at', 'comment', 'like', 'repost'] as const
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
</script>

<style scoped>
.page-content-wrapper {
  display: flex;
  flex-direction: row;
  justify-content: center;
  padding: 20px 0; /* 上下内边距，保持与Message页一致 */
  min-height: calc(100vh - 40px); /* 内容最小高度，减去上下padding */
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