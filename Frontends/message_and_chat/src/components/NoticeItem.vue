<template>
  <div
    class="notice-item"
    :class="{
      clickable:
        notice.type === 'like' && notice.sender.nickname.includes('等'),
    }"
    @click="handleItemClick"
  >
    <!-- 左侧图标和头像 -->
    <div class="notice-left">
      <div class="notice-icon">
        <span class="icon">{{ getNoticeIcon(notice.type) }}</span>
      </div>
      <div class="user-avatar">
        <img
          v-if="notice.sender.avatar"
          :src="notice.sender.avatar"
          :alt="notice.sender.nickname"
        />
        <div v-else class="avatar-placeholder">
          {{ notice.sender.nickname.charAt(0).toUpperCase() }}
        </div>
      </div>
    </div>

    <!-- 右侧内容 -->
    <div class="notice-content">
      <div class="notice-header">
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
          <span class="action">{{ getNoticeContent(notice.type) }}</span>
        </div>
      </div>
      <div class="notice-time">{{ diffime }}</div>
      <div
        v-if="notice.targetPostTitle"
        class="post-title"
        @click="handleLikeCountClick"
      >
        "{{ notice.targetPostTitle }}"
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { type notice } from "../types/notice";
import { formatTime } from "../types/message";

interface Props {
  notice: notice;
  likeCount?: number;
  likeNotices?: notice[];
}

const props = defineProps<Props>();
const emit = defineEmits<{
  showLikeDetails: [postId: number];
}>();
const diffime = formatTime(props.notice.time);

const getNoticeIcon = (type: string) => {
  switch (type) {
    case "like":
      return "👍";
    case "comment":
      return "💬";
    case "at":
      return "@";
    case "follow":
      return "👤";
    default:
      return "📢";
  }
};

const getNoticeContent = (type: string) => {
  switch (type) {
    case "like":
      return "点赞了你的帖子";
    case "comment":
      return "回复了你";
    case "at":
      return "提到了你";
    case "follow":
      return "关注了你";
    default:
      return "通知了你";
  }
};

// 点击人数
const handleLikeCountClick = () => {
  if (props.notice.type === "like" && props.likeCount && props.likeCount > 0) {
    emit("showLikeDetails", props.notice.targetPostId);
  }
};

// 点击整个通知项
const handleItemClick = () => {
  if (props.notice.type === "like" && props.likeCount && props.likeCount > 0) {
    emit("showLikeDetails", props.notice.targetPostId);
  }
};
</script>

<style scoped>
.notice-item {
  display: flex;
  width: 100%;
  padding: 8px 16px;
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

.notice-left {
  display: flex;
  align-items: center;
  margin-right: 24px;
}

.notice-icon {
  margin-right: 8px;
}

.icon {
  font-size: 24px;
  color: #4a9eff;
}

.user-avatar {
  display: flex;
  align-items: center;
  justify-content: center;
}

.user-avatar img {
  width: 36px;
  height: 36px;
  border-radius: 100%;
}

.notice-content {
  display: flex;
  flex: 1;
  flex-direction: column;
  justify-content: center;
  align-items: flex-start;
}

.notice-header {
  display: flex;
  align-items: center;
  font-size: 16px;
  margin-bottom: 4px;
}

.notice-main {
  display: flex;
  align-items: center;
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

.post-title {
  display: flex;
  text-align: left;
  color: #a0aec0;
  font-size: 14px;
  margin-top: 4px;
}

.notice-time {
  color: #9499a0;
  font-size: 12px;
  margin-bottom: 4px;
}
</style>