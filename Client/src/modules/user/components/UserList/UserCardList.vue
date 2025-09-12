<script setup lang="ts">
import PlaceHolder from '@/modules/user/components/PlaceHolder.vue'
import UserCardHover from '@/modules/user/components/UserList/UserCardHover.vue'
import { ref, shallowRef, watch } from 'vue'
import UserInfo from '@/modules/user/scripts/UserInfo.ts'
import FollowButton from '@/modules/user/components/UserList/FollowButton.vue'

const { userId, followBtn } = defineProps<{
  userId: string;
  followBtn?: boolean;
}>();

const emit = defineEmits<{
  (e: 'error'): void;
  (e: 'success'): void;
}>();

const userInfo = ref(new UserInfo(userId));
userInfo.value.loadUserData().then(() => {
  if (userInfo.value.error) {
    emit('error');
  } else if (userInfo.value.profileLoaded) {
    emit('success');
  }
}).catch((error) => {
  emit('error');
});
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
  <div class="w-full p-6 border border-slate-800
    relative flex flex-row items-start justify-between w-full">
    <!-- Avatar -->
    <router-link :to="{ name: 'UserPage', params: { userId_p: userId } }"
                 @mouseenter="moveIn($event, 'avatar')"
                 @mouseleave="moveOut($event, 'avatar')" class="pr-4 py-2">
      <img v-if="!!userInfo.avatarUrl" :src="userInfo.avatarUrl"
           class="w-12 h-12 rounded-full" alt="User Avatar">
      <PlaceHolder v-else width="100" :text="userInfo.nickname" height="100" class="w-12 h-12 rounded-full"/>
    </router-link>


    <div class="flex flex-col justify-items-start space-x-2 w-full">
      <div class="flex flex-row items-center justify-between w-full">
        <!-- User info -->
        <div class="flex flex-col space-x-4 text-left">
          <router-link :to="{ name: 'UserPage', params: { userId_p: userId.toString() } }"
                       @mouseenter="moveIn($event, 'name')"
                       @mouseleave="moveOut($event, 'name')"
                       class="font-bold text-left text-xl hover:underline">{{ userInfo.nickname }}</router-link>
          <p class="text-slate-500">@{{ userInfo.username }}</p>
        </div>

        <FollowButton v-if="followBtn" :user-id="userId.toString()" class="w-24 mr-2"
                      @followed="userInfo.followerCount += 1"
                      @unfollowed="userInfo.followerCount -= 1" />
      </div>
      <slot name="content">
        <p class="mt-2 text-slate-300 text-left">{{ userInfo.bio || "这个用户很神秘，什么也没写~"}}</p>
      </slot>
    </div>


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


