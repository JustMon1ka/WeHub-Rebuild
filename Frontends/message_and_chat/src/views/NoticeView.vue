<template>
  <div class="container">
    <div class="left">
      <SideNavigationBar />
    </div>
    <div class="divider-vertical"></div>
    <div class="center">
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

      <div class="notice-information">
        <div v-if="selectedNotices.length === 0" class="empty-state">
          <p>暂无通知</p>
        </div>
        <div v-else class="notice-list">
          <NoticeItem
            v-for="notice in selectedNotices"
            :key="`${notice.sender.id}-${notice.time}-${notice.type}`"
            :notice="notice"
            :like-count="
              notice.type === 'like'
                ? likeCountMap[notice.targetPostId]
                : undefined
            "
            :like-notices="getLikeNoticesForPost(notice.targetPostId)"
            @show-like-details="handleShowLikeDetails"
          />
        </div>
      </div>
    </div>
    <div class="divider-vertical"></div>
    <div class="right">
      <!--todo 搜索框-->
      <input v-model="searchText" type="text" placeholder="搜索..." />
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from "vue";
import { useRouter } from "vue-router";
import { type notice } from "../types/notice";
import SideNavigationBar from "../components/SideNavigationBar.vue";
import NoticeItem from "../components/NoticeItem.vue";
import { noticeList as noticeData } from "../data/noticeData";

const selectedNoticeType = ref(0);
const searchText = ref("");
const noticeTypeTexts = ["全部消息", "@我", "回复我的", "收到的赞"];
const router = useRouter();

const noticeList = ref<notice[]>(noticeData);

// 筛选通知
const selectedNotices = computed(() => {
  let selectedNoticeList = noticeList.value;

  if (selectedNoticeType.value > 0) {
    const typeMap = [null, "at", "comment", "like"];
    const selectedType = typeMap[selectedNoticeType.value];
    if (selectedType !== null) {
      selectedNoticeList = selectedNoticeList.filter(
        (notice) => notice.type === selectedType
      );
    }
  }

  if (searchText.value.trim()) {
    const searchLower = searchText.value.toLowerCase();
    selectedNoticeList = selectedNoticeList.filter(
      (notice) =>
        notice.sender.nickname.toLowerCase().includes(searchLower) ||
        notice.targetPostTitle.toLowerCase().includes(searchLower)
    );
  }

  // 对于点赞通知，合并同一个帖子的多个点赞
  if (selectedNoticeType.value === 3 || selectedNoticeType.value === 0) {
    // 收到的赞 或 全部消息
    const likeNoticesByPost = new Map<number, notice[]>();

    // 按帖子ID分组点赞通知
    selectedNoticeList.forEach((notice) => {
      if (notice.type === "like") {
        if (!likeNoticesByPost.has(notice.targetPostId)) {
          likeNoticesByPost.set(notice.targetPostId, []);
        }
        likeNoticesByPost.get(notice.targetPostId)!.push(notice);
      }
    });

    // 处理每个帖子的点赞通知
    const processedNotices: notice[] = [];

    // 先处理非点赞通知
    selectedNoticeList.forEach((notice) => {
      if (notice.type !== "like") {
        processedNotices.push(notice);
      }
    });

    // 再处理点赞通知
    likeNoticesByPost.forEach((likes, postId) => {
      if (likes.length > 1) {
        // 如果这个帖子有多个点赞，只添加最新的一个，并修改消息内容
        const lastLiker = likes[0]; // 最新的点赞者

        // 创建合并后的通知
        const mergedNotice: notice = {
          ...lastLiker,
          sender: {
            ...lastLiker.sender,
            nickname: `${lastLiker.sender.nickname} 等${likes.length}人`,
          },
        };

        processedNotices.push(mergedNotice);
      } else {
        // 单个点赞，直接添加
        processedNotices.push(likes[0]);
      }
    });

    return processedNotices;
  }

  return selectedNoticeList;
});

// 获取点赞数
const likeCountMap = computed(() => {
  const map: Record<number, number> = {};
  noticeList.value.forEach((notice) => {
    if (notice.type === "like") {
      map[notice.targetPostId] = (map[notice.targetPostId] || 0) + 1;
    }
  });
  return map;
});

// 获取指定帖子的点赞信息
const getLikeNoticesForPost = (postId: number) => {
  return noticeList.value.filter(
    (notice) => notice.type === "like" && notice.targetPostId === postId
  );
};

// 显示点赞详情
const handleShowLikeDetails = (postId: number) => {
  router.push(`/notice/likeDetails/${postId}`);
};
</script>

<style scoped>
.container {
  display: flex;
  height: 100vh;
  width: 1200px;
  max-width: 100%;
  margin: 0 auto;
  box-sizing: border-box;
}

.left {
  width: 20%;
  display: flex;
  align-items: center;
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
  flex: 1;
  display: flex;
  padding-left: 32px;
}

.notice-type {
  flex: 1;
  display: flex;
}

.notice-type button {
  flex: 1;
  align-items: center;
  justify-content: center;
  text-align: center;
}

.notice-type button.active {
  font-weight: bold;
}

.notice-information {
  flex: 14;
  display: flex;
  flex-direction: column;
}

.notice-list {
  display: flex;
  flex-direction: column;
  width: 100%;
}

.right {
  width: 20%;
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