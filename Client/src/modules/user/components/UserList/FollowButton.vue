<script setup lang="ts">
import styles from '@/modules/user/scripts/Styles.ts'
import { User } from '@/modules/auth/public.ts'
import { ref, type Ref } from 'vue'

const { userId } = defineProps<{
  userId: string;
}>();

const emit = defineEmits<{
  (e: 'followed'): void;
  (e: 'unfollowed'): void;
}>();

let followed : Ref<boolean> = ref(userId in (User.getInstance()?.followList || []));

function toggleFollow() {
  followed.value = !followed.value;
  if (followed.value) {
    User.getInstance()?.unfollowUser(userId);
    emit('unfollowed');
  } else {
    User.getInstance()?.followUser(userId);
    emit('followed');
  }
}

</script>

<template>
  <div>
    <button @click="toggleFollow" v-if="followed" :class="styles.followBtnShape + styles.followBtn">
      关 注
    </button>
    <button @click="toggleFollow" v-else :class="styles.followBtnShape + styles.followingBtn">
      <span class="text-following">正在关注</span>
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
