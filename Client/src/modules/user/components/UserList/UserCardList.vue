<script setup lang="ts">
import PlaceHolder from '@/modules/user/components/PlaceHolder.vue'
import UserCardHover from '@/modules/user/components/UserList/UserCardHover.vue'
import { ref } from 'vue'
import UserInfo from '@/modules/user/scripts/UserInfo.ts'
import FollowButton from '@/modules/user/components/UserList/FollowButton.vue'

const { userId, followBtn } = defineProps<{
  userId: string;
  followBtn?: boolean;
}>();

const userInfo = ref(new UserInfo(userId));
const hoverCard = ref(false);

const cardStyle = ref({
  top: '',
  right: '',
  bottom: '',
  left: '',
});
let timeoutId : any = null;
const mouseOver = ref({
  avatar: false,
  name: false,
  card: false,
});

function moveIn(event: MouseEvent, mouseOverType: 'avatar' | 'name' | 'card') {
  mouseOver.value[mouseOverType] = true;

  if (timeoutId) {
    clearTimeout(timeoutId);
  }
  if (hoverCard.value) {
    return;
  }

  timeoutId = setTimeout(() => {
    // Calculate card position
    const windowWidth = window.innerWidth;
    const windowHeight = window.innerHeight;
    const mouseX = event.clientX;
    const mouseY = event.clientY;

    // Reset all positions first
    cardStyle.value = { top: '', right: '', bottom: '', left: '' };

    if (mouseX < windowWidth * 2 / 3 && mouseY < windowHeight/ 2) {
      // Top-left quadrant
      cardStyle.value.top = `${mouseY + 10}px`;
      cardStyle.value.left = `${mouseX + 10}px`;
    } else if (mouseX >= windowWidth * 2 / 3 && mouseY < windowHeight / 2) {
      // Top-right quadrant
      cardStyle.value.top = `${mouseY + 10}px`;
      cardStyle.value.right = `${windowWidth - mouseX + 10}px`;
    } else if (mouseX < windowWidth * 2 / 3 && mouseY >= windowHeight / 2) {
      // Bottom-left quadrant
      cardStyle.value.bottom = `${windowHeight - mouseY + 10}px`;
      cardStyle.value.left = `${mouseX + 10}px`;
    } else {
      // Bottom-right quadrant
      cardStyle.value.bottom = `${windowHeight - mouseY + 10}px`;
      cardStyle.value.right = `${windowWidth - mouseX + 10}px`;
    }

    hoverCard.value = true;
  }, 800);
}

function moveOut(event: MouseEvent, mouseOverType: 'avatar' | 'name' | 'card') {
  mouseOver.value[mouseOverType] = false;

  if (mouseOver.value.avatar || mouseOver.value.name || mouseOver.value.card) {
    return;
  }
  if (timeoutId) {
    clearTimeout(timeoutId);
  }
  timeoutId = setTimeout(() => {
    hoverCard.value = false;
  }, 500);
}
</script>

<template>
  <div class="relative flex flex-row items-start justify-between w-full">
    <!-- Avatar -->
    <!-- TODO：正确跳转 -->
    <router-link to="user_page" @mouseenter="moveIn($event, 'avatar')"
                 @mouseleave="moveOut($event, 'avatar')" class="p-4">
      <img v-if="!!userInfo.userAvatarUrl" :src="userInfo.userAvatarUrl"
           class="w-12 h-12 rounded-full" alt="User Avatar">
      <PlaceHolder v-else width="100" :text="userInfo.nickName" height="100" class="w-12 h-12 rounded-full"/>
    </router-link>

    <!-- User info -->
    <div class="flex flex-col justify-items-start space-x-2 w-full">
      <router-link to="user_page" @mouseenter="moveIn($event, 'name')"
                   @mouseleave="moveOut($event, 'name')"
                   class="font-bold text-left text-xl hover:underline">{{ userInfo.nickName }}</router-link>
      <slot name="article">
        <p class="mt-2 text-slate-300 text-left truncate w-70">{{ userInfo.bio || "这个用户很神秘，什么也没写~"}}</p>
      </slot>
    </div>

    <FollowButton v-if="followBtn" :user-id="userId" class="w-24"
                  @followed="userInfo.followerCount += 1"
                  @unfollowed="userInfo.followerCount -= 1" />

    <Teleport to="main">
      <div
        class="fixed z-50"
        :style="{
          top: cardStyle.top,
          right: cardStyle.right,
          bottom: cardStyle.bottom,
          left: cardStyle.left,
        }"
        v-if="hoverCard">
        <UserCardHover v-model:user-info="userInfo"
          @mouseenter="moveIn($event, 'card')"
          @mouseleave="moveOut($event, 'card')"
        />
      </div>
    </Teleport>
  </div>
</template>


