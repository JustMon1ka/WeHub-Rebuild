<template>
  <div
    class="notice-item"
    :class="{
      clickable: notice.type === 'like' && notice.sender.nickname.includes('等'),
    }"
    @click="handleItemClick"
  >
    <!-- 通知内容 -->
    <div class="notice-main-content">
      <!-- 左侧图标和头像 -->
      <div class="notice-left">
        <span class="icon">{{ getNoticeIcon(notice.type) }}</span>
        <img
          class="user-avatar"
          v-if="notice.sender.avatar"
          :src="notice.sender.avatar"
          :alt="notice.sender.nickname"
        />
        <span v-else>
          {{ notice.sender.nickname.charAt(0).toUpperCase() }}
        </span>
      </div>

      <!-- 通知内容 -->
      <div class="notice-content">
        <div class="notice-main">
          <span class="username">{{ notice.sender.nickname }}</span>
          <span
            class="total-person"
            v-if="
              notice.type === 'like' &&
              likeCount &&
              likeCount > 0 &&
              !notice.sender.nickname.includes('等')
            "
            @click="handleLikeCountClick"
          >
            共{{ likeCount }}人
          </span>
          <span class="action">{{ getNoticeContent(notice) }}</span>
        </div>
        <div class="post-comment-content">
          <span v-if="notice.type === 'comment'"> "{{ notice.newCommentContent }}"</span>
          <span v-else-if="notice.type === 'at'"> "{{ notice.atContent }}"</span>
        </div>

        <div class="other-info">
          <span class="notice-time">{{ diffime }}</span>
          <span
            class="reply"
            v-if="notice.type === 'comment' || notice.type === 'at'"
            @click="handleReplyClick"
            >回复</span
          >
          <span class="like" v-if="notice.type === 'comment' || notice.type === 'at'">点赞</span>
        </div>
      </div>
      <!-- 帖子/评论内容 -->
      <div class="notice-target">
        <span v-if="notice.objectType === 'post' && notice.targetPostTitleImage">
          <img class="target-post-image" :src="notice.targetPostTitleImage" alt="帖子封面" />
        </span>
        <span
          class="post-or-comment-title"
          v-else-if="notice.objectType === 'post' && !notice.targetCommentContent"
        >
          "{{ notice.targetPostTitle }}"
        </span>
        <span
          class="post-or-comment-title"
          v-else-if="notice.objectType === 'comment' && notice.targetCommentContent"
        >
          "{{ notice.targetCommentContent }}"
        </span>
      </div>
    </div>

    <!-- 回复输入框 -->
    <div v-if="showCommentInput" class="reply-input-container">
      <ReplyCommentInput
        :visible="showCommentInput"
        :placeholder="`回复${notice.sender.nickname}`"
        :useAvater="currentUserAvatar"
        :replyToUserId="notice.sender.id"
        @submit="handleSubmitComment"
        @cancel="handleCancelComment"
      />
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { type notice } from '../types'
import { formatTime } from '../../core/utils/time'
import ReplyCommentInput from './ReplyCommentInput.vue'

interface Props {
  notice: notice
  likeCount?: number
  likeNotices?: notice[]
}

const props = defineProps<Props>()
const emit = defineEmits<{
  showLikeDetails: [postId: number]
}>()
const diffime = formatTime(props.notice.time)
const showCommentInput = ref(false)
const currentUserAvatar = ref('https://placehold.co/100x100/facc15/78350f?text=F')

const getNoticeIcon = (type: string) => {
  switch (type) {
    case 'like':
      return '👍'
    case 'comment':
      return '💬'
    case 'at':
      return '@'
    case 'follow':
      return '👤'
    default:
      return '📢'
  }
}

const getNoticeContent = (notice: notice) => {
  switch (notice.type) {
    case 'like':
      if (notice.objectType === 'comment') {
        return '赞了你的评论'
      } else {
        return '赞了你的帖子' // 默认为帖子
      }
    case 'comment':
      if (notice.objectType === 'comment') {
        return '回复了你的评论'
      } else {
        return '回复了你的帖子'
      }
    case 'at':
      return '提到了你'
    case 'follow':
      return '关注了你'
    default:
      return '通知了你'
  }
}

// 点击人数
const handleLikeCountClick = () => {
  if (props.notice.type === 'like' && props.likeCount && props.likeCount > 0) {
    emit('showLikeDetails', props.notice.targetPostId)
  }
}

// 点击整个通知项
const handleItemClick = () => {
  if (props.notice.type === 'like' && props.likeCount && props.likeCount > 0) {
    emit('showLikeDetails', props.notice.targetPostId)
  }
}

// 点击回复按钮
const handleReplyClick = (event: Event) => {
  event.stopPropagation()
  showCommentInput.value = !showCommentInput.value
}

// 提交评论按钮
const handleSubmitComment = () => {
  showCommentInput.value = false
}

// 取消评论
const handleCancelComment = () => {
  showCommentInput.value = false
}
</script>

<style scoped>
.notice-item {
  display: flex;
  flex-direction: column;
  width: 100%;
  padding: 12px 16px;
  border-bottom: 1px solid #f5f5f5;
  transition: background-color 0.2s;
  cursor: pointer;
  box-sizing: border-box;
}

.notice-item:hover {
  background-color: #f5f5f5;
}

.notice-item.clickable {
  cursor: pointer;
}

.notice-item.clickable:hover {
  background-color: #f0f8ff;
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
  gap: 8px; /* 控制username和action间距 */
}

.username {
  font-weight: bold;
}

.total-person {
  cursor: pointer;
  font-weight: bold;
}

.time {
  color: #9499a0;
  font-size: 12px;
}

.post-comment-content {
  display: flex;
  text-align: left;
  color: #a0aec0;
  font-size: 14px;
  padding: 4px 0;
}

.other-info {
  display: flex;
  align-items: center;
  width: 100%;
  padding: 0px;
}

.notice-time,
.reply,
.like {
  color: #9499a0;
  font-size: 12px;
  padding: 0px 4px;
}

.reply,
.like {
  font-size: 14px;
  background: none;
  cursor: pointer;
}

.reply:hover,
.like:hover {
  color: #4a9eff;
  transition: color 0.2s;
}

.notice-target {
  display: flex;
  align-items: center;
}

.target-post-image {
  width: 72px;
  height: 72px;
  border-radius: 4px;
  object-fit: cover;
}

.post-or-comment-title {
  width: 72px;
  height: 72px;
  border-radius: 8px;
  font-size: 12px;
  color: #9499a0;
  background-color: #f8f9fa;
  border: 1px solid #e9ecef;
  padding: 6px 4px;
  display: -webkit-box;
  -webkit-line-clamp: 4;
  line-clamp: 4;
  -webkit-box-orient: vertical;
  overflow: hidden;
  line-height: 1.3;
  text-overflow: ellipsis;
  box-sizing: border-box;
}
</style>