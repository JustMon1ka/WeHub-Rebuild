<script setup lang="ts">
import UserCardPage from '@/modules/user/components/UserPage/UserCardPage.vue'
import UserInfoEdit from '@/modules/user/components/UserPage/UserInfoEdit.vue'
import { User } from '@/modules/auth/public.ts'
import UserInfo from '@/modules/user/scripts/UserInfo.ts'
import TabController from '@/modules/user/scripts/TabController.ts'
import type { TabLabel} from '@/modules/user/scripts/TabController.ts'
import { ref } from 'vue'
import FollowList from '@/modules/user/components/UserPage/FollowList.vue'
import router from '@/router.ts'
import PrivacyView from '@/modules/auth/views/PrivacyView.vue'

const { userId_p } = defineProps<{
  userId_p: string;
}>();

let userId = userId_p || 'Me';
if (userId === 'Me'){
  userId = User?.getInstance()?.userAuth.userId || 'unknown';
}

const editMode = ref(false);
const followMode = ref(false);
const followTab = ref(0);

let userInfo = ref(new UserInfo(userId));
let userInfoTemp = ref(userInfo.value.copy());

const tabLabels : Array<TabLabel> = [
  { key: 'post', label: '帖子'},
  { key: 'reply', label: '评论'},
  { key: 'like', label: '点赞'},
];

const Tabs = new TabController(tabLabels);

function onCancel(){
  userInfoTemp.value = userInfo.value.copy();
  editMode.value = false;
}

function onSave(){
  userInfo.value = userInfoTemp.value.copy();
  editMode.value = false;
}

</script>

<template>
  <main class="flex flex-col border-x border-slate-800 col-span-1 md:col-span-8
              lg:col-span-7 h-screen w-full overflow-hidden">
    <transition name="slide"
                enter-active-class="transition duration-200 ease-out"
                enter-from-class="opacity-100 translate-y-full"
                enter-to-class="opacity-100 translate-y-0"
                leave-active-class="transition duration-200 ease-in"
                leave-from-class="opacity-100 translate-y-0"
                leave-to-class="opacity-100 translate-y-full">
      <div v-show="userInfo.isMe && editMode">
        <UserInfoEdit v-on:editCancel="onCancel" v-on:editSave="onSave"
                      v-model:user-info="userInfoTemp"/>
      </div>
    </transition>

    <transition name="slide"
                enter-active-class="transition duration-200 ease-out"
                enter-from-class="opacity-100 translate-y-full"
                enter-to-class="opacity-100 translate-y-0"
                leave-active-class="transition duration-200 ease-in"
                leave-from-class="opacity-100 translate-y-0"
                leave-to-class="opacity-100 translate-y-full">
      <FollowList v-show="followMode" @close="followMode=false" :user-id_p="userId"
                  v-model:curTab="followTab" :nick-name="userInfo.nickName"/>
    </transition>

    <div v-show="!editMode && !followMode">
      <!-- 页面头部 -->
      <div class="sticky top-0 flex flex-row p-4 border-b border-slate-800 bg-slate-900/75 backdrop-blur-md">
        <div class="p-2 rounded-full hover:bg-slate-800 mr-4">
          <img src="@/assets/close.svg" alt="关闭" class="w-6 h-6 cursor-pointer" @click="router.back()">
        </div>
        <h1 class="text-xl p-1 font-bold">{{ userInfo.nickName }}</h1>
      </div>
      <UserCardPage user-id="userId" v-model:user-info="userInfo"
                    @editProfile="editMode=true"
                    @toFollowing="() => {followMode=true; followTab=0;}"
                    @toFollower="() => {followMode=true; followTab=1;}"/>

      <div class="flex flex-row border-b border-slate-800">
        <button v-for="({ key, label }, index) in tabLabels" :key="key"
                @click="Tabs.switchTab(index)"
                v-bind:class="Tabs.buttons[index].class.value">
          {{ label }}
        </button>
      </div>
      <!-- Tab 切换 -->
      <div v-for="( tab , index) in Tabs.tablabels" :key="index">
        <div v-show="Tabs.currentTab === index">
          <!-- 内容切换 Tab -->
          <privacy-view/>
        </div>
      </div>
    </div>
  </main>
</template>
