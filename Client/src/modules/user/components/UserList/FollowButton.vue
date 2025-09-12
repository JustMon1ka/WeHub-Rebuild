<script setup lang="ts">
import styles from '@/modules/user/scripts/Styles.ts'
import { User } from '@/modules/auth/public.ts'
import { ref, type Ref } from 'vue'
import { toggleLoginHover } from '@/router.ts'

const { userId } = defineProps<{
  userId: string;
}>();

const emit = defineEmits<{
  (e: 'followed'): void;
  (e: 'unfollowed'): void;
  (e: 'updated'): void   // ✅ 新增事件
}>();

const login = !!User.getInstance();
let following : Ref<boolean> = ref(User.getInstance()?.followingList.has(userId) || false); // 是否关注对方
let followed: Ref<boolean> = ref(User.getInstance()?.followerList.has(userId) || false); // 是否被对方关注

function toggleFollow() {
  if (!login) {
    toggleLoginHover(true);
    return;
  }

  if (following.value) {
    User.getInstance()?.unfollowUser(userId);
    emit('unfollowed');
  } else {
    User.getInstance()?.followUser(userId);
    emit('followed');
  }
  following.value = !following.value;
  emit('updated')   // ✅ 每次切换后都通知父组件
}

</script>

<template>
  <div>
<!--    *  followed following-->
<!--    *  false    false       未关注      => 显示“关注”按钮-->
<!--    *  false    true        关注对方     => 显示“正在关注”按钮-->
<!--    *  true     false       被对方关注   => 显示“回关”按钮-->
<!--    *  true     true        双向关注     => 显示“已互关”按钮-->
    <button @click="toggleFollow" v-if="!followed && !following" :class="styles.followBtnShape + styles.followBtn">
      关 注
    </button>
    <button @click="toggleFollow" v-else-if="followed && !following" :class="styles.followBtnShape + styles.followBtn">
      回 关
    </button>
    <button @click="toggleFollow" v-else :class="styles.followBtnShape + styles.followingBtn">
      <span class="text-following">{{ followed ? '已互关' : '已关注' }}</span>
      <span class="text-unfollow">取消关注</span>
    </button>
  </div>

</template>

<style scoped>
/* 自定义“正在关注”按钮的悬停效果 */
.following-btn:hover .text-unfollow {
  display: inline;
}
.following-btn:hover .text-following {
  display: none;
}
.text-unfollow {
  display: none;
}
</style>
