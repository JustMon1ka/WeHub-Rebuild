 <template>
  <div class="flex flex-row">
    <div class="divider-vertical" aria-hidden="true"></div>

    <div class="center w-full">
      <div class="divider-horizontal"></div>

      <div class="text-3xl font-bold px-10 py-4 text-slate-200 flex justify-left items-center">
        <span>通知</span>
      </div>

      <div class="flex flex-row justify-around">
        <button
          v-for="(text, index) in noticeTypeTexts"
          :key="index"
          :class="{ active: selectedNoticeType === index }"
          @click="onTabChange(index)"
          class="h-12 text-l hover:bg-slate-800 w-full transition-colors duration-200 text-slate-200"
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

      <div class="text-red-500 text-sm text-center" v-if="unreadError">{{ unreadError }}</div>

      <div class="notice-information">
        <div v-if="selectedNotices.length === 0 && !unreadError" class="text-center text-slate-500 text-md py-3">
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
import { ref, computed, onMounted, watch, defineAsyncComponent, nextTick, watchEffect } from 'vue'
import { useRoute, useRouter } from 'vue-router'

const NoticeItem = defineAsyncComponent(() => import('../components/NoticeItem.vue'))
import {
  type notice,
  type unreadNoticeCount,
  type commentNoticeItem,
  type replyNoticeItem,
  type repostNoticeItem,
  type atNoticeItem,
  type likeNoticeItem,
  unwrap,
} from '../types'
import {
  getUnreadNoticeCount,
  markNotificationsRead,
  getLikeNotices,
  getCommentNotices,
  getReplyNotices,
  getRepostNotices,
  getAtNotices,
  getPostDetail,
  getCommentDetail,
  getLikersByTarget,
} from '../api'
import { getUserDetail } from '../../message/api'
import {
  indexToType,
  optimisticMarkRead,
  getLikeNoticesForPost,
  displayUnreadNoticeCount,
  getUnreadCountByType,
} from '../utils/noticeUtils'

const selectedNoticeType = ref(0)
const searchText = ref('')
const noticeTypeTexts = ['收到的赞', '评论我的', '回复我的', '@我', '转发我的']
const noticeList = ref<notice[]>([])

// 缓存用户信息和帖子信息
const userCache = ref<Map<number, { nickname: string; avatar: string }>>(new Map())
const postCache = ref<Map<number, { title: string; image?: string }>>(new Map())

const loadingUnread = ref(false)

type UnreadSummaryData = unreadNoticeCount['data']
const unreadSummary = ref<UnreadSummaryData | null>(null)
const unreadError = ref<string | null>(null)
const readOnce = ref<Set<string>>(new Set())

const likeItems = ref<likeNoticeItem[]>([])
const commentItems = ref<commentNoticeItem[]>([])
const replyItems = ref<replyNoticeItem[]>([])
const repostItems = ref<repostNoticeItem[]>([])
const atItems = ref<atNoticeItem[]>([])

const route = useRoute()
const router = useRouter()

// 强制设置默认选择为"收到的赞"
if (!route.params.type) {
  selectedNoticeType.value = 0
}

// 使用 watchEffect 强制监听路由变化
watchEffect(() => {
  if (!route.params.type && selectedNoticeType.value !== 0) {
    selectedNoticeType.value = 0
  }
})

const typeToIndex: Record<string, number> = { like: 0, comment: 1, reply: 2, at: 3, repost: 4 }
const idxToType: Record<number, string> = {
  0: 'like',
  1: 'comment',
  2: 'reply',
  3: 'at',
  4: 'repost',
}

// 获取用户信息的辅助函数
async function getUserInfo(userId: number): Promise<{ nickname: string; avatar: string }> {
  if (userCache.value.has(userId)) {
    return userCache.value.get(userId)!
  }

  try {
    const userDetail = await getUserDetail(userId)
    const userInfo = {
      nickname: userDetail.nickname,
      avatar: userDetail.avatarUrl,
    }
    userCache.value.set(userId, userInfo)
    return userInfo
  } catch (error) {
    console.error('获取用户信息失败:', error)
    return { nickname: `用户${userId}`, avatar: '' }
  }
}

// 获取帖子信息的辅助函数
async function getPostInfo(postId: number): Promise<{ title: string; image?: string }> {
  if (postCache.value.has(postId)) {
    return postCache.value.get(postId)!
  }

  try {
    const postDetailResp = await getPostDetail(postId)
    const postDetail = unwrap(postDetailResp)
    const postInfo = {
      title: postDetail.title,
      image: undefined, // API响应中没有图片字段，可以根据需要添加
    }
    postCache.value.set(postId, postInfo)
    return postInfo
  } catch (error) {
    console.error('获取帖子信息失败:', error)
    return { title: `帖子${postId}` }
  }
}

// 路由 → tab，同步初始与后续变化
watch(
  () => route.params.type,
  (t) => {
    const mapped = typeToIndex[String(t)] ?? 0 // 无type时默认 like
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
const handleShowLikeDetailsClick = (params: {
  targetType: 'post' | 'comment'
  targetId: number
}) => {
  // 跳转到点赞详情页面，传递目标类型和目标ID
  const url = `/notice/likeDetails/${params.targetType}/${params.targetId}`
  router.push(url)
}

onMounted(async () => {
  // 确保默认选择"收到的赞"
  if (!route.params.type) {
    selectedNoticeType.value = 0
  }

  // 使用 nextTick 确保在 DOM 更新后再次检查
  await nextTick()
  if (!route.params.type && selectedNoticeType.value !== 0) {
    selectedNoticeType.value = 0
  }

  loadingUnread.value = true
  try {
    // 并行请求：未读汇总 + 各种通知列表
    const [unreadResp, likeResp, commentResp, replyResp, repostResp, atResp] = await Promise.all([
      getUnreadNoticeCount(),
      getLikeNotices({ page: 1, pageSize: 20 }),
      getCommentNotices({ page: 1, pageSize: 20, unreadOnly: false }),
      getReplyNotices({ page: 1, pageSize: 20, unreadOnly: false }),
      getRepostNotices({ page: 1, pageSize: 20, unreadOnly: false }),
      getAtNotices({ page: 1, pageSize: 20, unreadOnly: false }),
    ])

    // 处理未读数据
    unreadSummary.value = unwrap(unreadResp)
    for (const t of ['at', 'comment', 'reply', 'like', 'repost'] as const) {
      if (readOnce.value.has(t)) {
        unreadSummary.value.unreadByType[t] = 0
      }
    }

    // 存储原始API数据
    commentItems.value = unwrap(commentResp).items
    replyItems.value = unwrap(replyResp).items
    repostItems.value = unwrap(repostResp).items
    atItems.value = unwrap(atResp).items

    // 合并已读和未读的点赞数据
    const likeData = unwrap(likeResp)
    likeItems.value = [...likeData.unread, ...likeData.read.items]

    // 转换API数据为组件需要的格式
    await convertApiDataToNoticeList()
  } catch (err: any) {
    unreadError.value = err?.message ?? '加载通知失败'
  } finally {
    loadingUnread.value = false
  }
})

// 将API数据转换为通知列表格式
async function convertApiDataToNoticeList() {
  const convertedNotices: notice[] = []

  // 处理评论通知
  // 去重：根据commentId去重
  const uniqueComments = new Map<number, commentNoticeItem>()
  for (const comment of commentItems.value) {
    if (!uniqueComments.has(comment.commentId)) {
      uniqueComments.set(comment.commentId, comment)
    }
  }

  for (const comment of uniqueComments.values()) {
    const userInfo = await getUserInfo(comment.userId)
    // 根据新接口格式，现在有postId字段
    const postId = comment.postId
    const postInfo = await getPostInfo(postId)

    const notice = {
      noticeId: comment.commentId,
      type: 'comment' as const,
      sender: {
        id: comment.userId,
        nickname: userInfo.nickname,
        avatar: userInfo.avatar,
        url: `/user/${comment.userId}`,
      },
      time: comment.createdAt,
      isRead: false, // 不区分已读未读
      objectType: 'post' as const,
      targetPostId: postId,
      targetPostTitle: postInfo.title,
      targetPostTitleImage: postInfo.image || '',
      newCommentContent: comment.contentPreview,
    }

    convertedNotices.push(notice)
  }

  // 处理回复通知
  // 去重：根据replyId去重
  const uniqueReplies = new Map<number, replyNoticeItem>()
  for (const reply of replyItems.value) {
    if (!uniqueReplies.has(reply.replyId)) {
      uniqueReplies.set(reply.replyId, reply)
    }
  }

  for (const reply of uniqueReplies.values()) {
    const userInfo = await getUserInfo(reply.replyPoster)

    // 由于回复通知API中没有postId字段，且/comments/{commentId}接口不存在
    // 暂时使用降级方案，显示评论ID
    // TODO: 需要后端API支持，在replyNoticeItem中添加postId字段
    const postInfo = { title: `评论${reply.commentId}`, image: '' }
    const targetPostId = reply.commentId

    convertedNotices.push({
      noticeId: reply.replyId,
      type: 'reply',
      sender: {
        id: reply.replyPoster,
        nickname: userInfo.nickname,
        avatar: userInfo.avatar,
        url: `/user/${reply.replyPoster}`,
      },
      time: reply.createdAt,
      isRead: false,
      objectType: 'comment',
      targetPostId: targetPostId,
      targetPostTitle: postInfo.title,
      targetPostTitleImage: postInfo.image,
      replyContent: reply.contentPreview,
    })
  }

  // 处理转发通知
  // 去重：根据repostId去重
  const uniqueReposts = new Map<number, repostNoticeItem>()
  for (const repost of repostItems.value) {
    if (!uniqueReposts.has(repost.repostId)) {
      uniqueReposts.set(repost.repostId, repost)
    }
  }

  for (const repost of uniqueReposts.values()) {
    const userInfo = await getUserInfo(repost.userId)
    const postInfo = await getPostInfo(repost.postId)

    convertedNotices.push({
      noticeId: repost.repostId,
      type: 'repost',
      sender: {
        id: repost.userId,
        nickname: userInfo.nickname,
        avatar: userInfo.avatar,
        url: `/user/${repost.userId}`,
      },
      time: repost.createdAt,
      isRead: false, // 不区分已读未读
      objectType: 'post',
      targetPostId: repost.postId,
      targetPostTitle: postInfo.title,
      targetPostTitleImage: postInfo.image || '',
      repostContent: repost.commentPreview || '',
    })
  }

  // 处理@通知
  // 去重：根据targetId和targetType组合去重
  const uniqueAts = new Map<string, atNoticeItem>()
  for (const at of atItems.value) {
    const key = `${at.targetType}-${at.targetId}`
    if (!uniqueAts.has(key)) {
      uniqueAts.set(key, at)
    }
  }

  for (const at of uniqueAts.values()) {
    const userInfo = await getUserInfo(at.userId)

    let targetTitle = ''
    let targetPostId = at.targetId
    let targetPostTitleImage = ''

    try {
      if (at.targetType === 'post') {
        // 如果是@帖子，直接获取帖子信息
        const postInfo = await getPostInfo(at.targetId)
        targetTitle = postInfo.title
        targetPostTitleImage = postInfo.image || ''
      } else if (at.targetType === 'comment') {
        // 如果是@评论，通过评论ID获取评论详情，然后获取帖子信息
        const commentDetailResp = await getCommentDetail(at.targetId)
        const commentDetail = unwrap(commentDetailResp)
        const postInfo = await getPostInfo(commentDetail.postId)

        targetTitle = postInfo.title
        targetPostId = commentDetail.postId
        targetPostTitleImage = postInfo.image || ''
      }
    } catch (error) {
      console.error('获取@通知的目标信息失败:', error)
      // 如果API失败，使用降级方案
      if (at.targetType === 'post') {
        targetTitle = `帖子${at.targetId}`
      } else {
        targetTitle = `评论${at.targetId}`
      }
    }

    // 获取@的具体内容
    let atContent = `@你`
    try {
      if (at.targetType === 'post') {
        // 如果是@帖子，显示帖子标题
        atContent = `"${targetTitle}"`
      } else if (at.targetType === 'comment') {
        // 如果是@评论，显示评论内容
        const commentDetailResp = await getCommentDetail(at.targetId)
        const commentDetail = unwrap(commentDetailResp)
        atContent = `"${
          commentDetail.content.length > 50
            ? commentDetail.content.substring(0, 50) + '...'
            : commentDetail.content
        }"`
      }
    } catch (error) {
      console.error('获取@通知的具体内容失败:', error)
      atContent = `@你`
    }

    convertedNotices.push({
      noticeId: at.targetId, // 使用targetId作为通知ID
      type: 'at',
      sender: {
        id: at.userId,
        nickname: userInfo.nickname,
        avatar: userInfo.avatar,
        url: `/user/${at.userId}`,
      },
      time: at.createdAt,
      isRead: false,
      objectType: at.targetType,
      targetPostId: targetPostId,
      targetPostTitle: targetTitle,
      targetPostTitleImage: targetPostTitleImage,
      atContent: atContent,
    })
  }

  // 处理点赞通知
  for (const like of likeItems.value) {
    // 根据目标类型获取不同的信息
    let postInfo = { title: `目标${like.targetId}`, image: '' }
    let targetPostId = like.targetId

    if (like.targetType === 'post') {
      // 如果是点赞帖子，直接获取帖子信息
      const fetchedPostInfo = await getPostInfo(like.targetId)
      postInfo = {
        title: fetchedPostInfo.title,
        image: fetchedPostInfo.image || '',
      }
    } else if (like.targetType === 'comment') {
      // 如果是点赞评论，先获取评论详情，再获取帖子信息
      try {
        const commentDetailResp = await getCommentDetail(like.targetId)
        const commentDetail = unwrap(commentDetailResp)
        targetPostId = commentDetail.postId
        const fetchedPostInfo = await getPostInfo(commentDetail.postId)
        postInfo = {
          title: fetchedPostInfo.title,
          image: fetchedPostInfo.image || '',
        }
      } catch (error) {
        console.error('获取评论详情失败:', error)
        // 如果API失败，使用降级方案
        postInfo = { title: `评论${like.targetId}`, image: '' }
      }
    }

    // 使用新的API获取详细的点赞者信息
    try {
      const likersResp = await getLikersByTarget({
        targetType: like.targetType,
        targetId: like.targetId,
        page: 1,
        pageSize: 20,
      })
      const likersData = unwrap(likersResp)

      if (likersData.items.length > 0) {
        // 获取第一个点赞者的信息作为主要显示
        const firstLikerInfo = await getUserInfo(likersData.items[0])

        // 验证时间字段，如果无效则使用当前时间
        const validTime = (() => {
          const timeDate = new Date(like.lastLikedAt)
          if (isNaN(timeDate.getTime())) {
            console.warn('点赞通知时间无效，使用当前时间:', like.lastLikedAt)
            return new Date().toISOString()
          }
          return like.lastLikedAt
        })()

        convertedNotices.push({
          noticeId: like.targetId,
          type: 'like',
          sender: {
            id: likersData.items[0],
            nickname:
              likersData.items.length > 1
                ? `${firstLikerInfo.nickname} 等${likersData.total}人`
                : firstLikerInfo.nickname,
            avatar: firstLikerInfo.avatar,
            url: `/user/${likersData.items[0]}`,
          },
          time: validTime,
          isRead: false, // 不区分已读未读
          objectType: like.targetType,
          targetPostId: targetPostId,
          targetPostTitle: postInfo.title,
          targetPostTitleImage: postInfo.image || '',
        })
      }
    } catch (error) {
      console.error('获取点赞者详情失败:', error)
      // 如果API失败，使用原有的likerIds作为备选
      if (like.likerIds.length > 0) {
        const firstLikerInfo = await getUserInfo(like.likerIds[0])

        // 验证时间字段，如果无效则使用当前时间
        const validTime = (() => {
          const timeDate = new Date(like.lastLikedAt)
          if (isNaN(timeDate.getTime())) {
            console.warn('点赞通知时间无效，使用当前时间:', like.lastLikedAt)
            return new Date().toISOString()
          }
          return like.lastLikedAt
        })()

        convertedNotices.push({
          noticeId: like.targetId,
          type: 'like',
          sender: {
            id: like.likerIds[0],
            nickname:
              like.likerIds.length > 1
                ? `${firstLikerInfo.nickname} 等${like.likeCount}人`
                : firstLikerInfo.nickname,
            avatar: firstLikerInfo.avatar,
            url: `/user/${like.likerIds[0]}`,
          },
          time: validTime,
          isRead: false,
          objectType: like.targetType,
          targetPostId: targetPostId,
          targetPostTitle: postInfo.title,
          targetPostTitleImage: postInfo.image || '',
        })
      }
    }
  }

  // 按时间排序（最新的在前）
  convertedNotices.sort((a, b) => new Date(b.time).getTime() - new Date(a.time).getTime())

  noticeList.value = convertedNotices
}

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
  const typeMap = ['like', 'comment', 'reply', 'at', 'repost'] as const
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
.center {
  display: flex;
  flex-direction: column;
  overflow-wrap: break-word;
  word-break: break-all;
}

.active {
  font-weight: bold;
  border-bottom: 2px solid #00aeec;
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
