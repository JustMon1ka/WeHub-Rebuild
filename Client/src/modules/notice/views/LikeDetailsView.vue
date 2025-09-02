<template>
  <div class="page-content-wrapper">
    <div class="divider-vertical"></div>
    <div class="center">
      <div class="divider-horizontal"></div>
      <div class="notice-heading">
        <span class="separator">通知 > </span>
        <span @click="$router.go(-1)" class="back-link">收到的赞</span>
        <span> > 点赞详情</span>
      </div>
      <div class="divider-horizontal"></div>
      <div class="post-info">
        <span class="post-title">帖子：{{ postTitle }}</span>
      </div>
      <div class="divider-horizontal"></div>

      <div class="like-users-list">
        <div v-for="user in likeUsers" :key="user.id" class="like-user-item">
          <div class="item-left">
            <div class="user-avater">
              <img :src="user.avatar" :alt="user.username" />
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
      <div class="divider-horizontal"></div>
    </div>
    <div class="divider-vertical"></div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { getLikeUsersByPostId, getPostTitleById } from '../noticeData'
import { useRoute } from 'vue-router'

const route = useRoute()
const postId = ref(Number(route.params.postId))
const postTitle = ref(getPostTitleById(postId.value))
const searchText = ref('')

// 获取点赞用户列表
const likeUsers = computed(() => {
  const users = getLikeUsersByPostId(postId.value)

  if (searchText.value.trim()) {
    const searchLower = searchText.value.toLowerCase()
    return users.filter((user) => user.username.toLowerCase().includes(searchLower))
  }

  return users
})
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
  word-break: break-word;
}

.notice-heading {
  flex: 1;
  display: flex;
  padding-left: 32px;
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
  background-color: #f8f9fa;
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

.user-details {
  flex: 1;
  display: flex;
  flex-direction: column;
  gap: 4px;
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

.empty-state {
  display: flex;
  justify-content: center;
  align-items: center;
  height: 200px;
  color: #999;
  font-size: 16px;
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