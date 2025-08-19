<script setup lang="ts">
import UserCardPage from '@/modules/user/components/UserPage/UserCardPage.vue'
import UserInfoEdit from '@/modules/user/components/UserPage/UserInfoEdit.vue'
import { User } from '@/modules/auth/public.ts'
import UserInfo from '@/modules/user/scripts/UserInfo.ts'
import TabController from '@/modules/user/scripts/TabController.ts'
import type { TabLabel} from '@/modules/user/scripts/TabController.ts'
import { type Ref, ref, watch } from 'vue'
import FollowList from '@/modules/user/components/UserPage/FollowList.vue'
import router from '@/router.ts'
import PrivacyView from '@/modules/auth/views/PrivacyView.vue'
import { toggleLoginHover } from '@/App.vue'

const { userId_p } = defineProps<{
  userId_p: string;
}>();

let userId = userId_p || 'Me';
let userInfo : Ref<UserInfo, UserInfo>;
if (userId === 'Me'){
  userId = User?.getInstance()?.userAuth.userId || '';
  if (!userId){
    toggleLoginHover(true);
    setTimeout(async () => await router.push('/'), 0);
  }
}
userInfo = ref<UserInfo>(new UserInfo(userId));
userInfo.value.loadUserData(); // 加载用户数据，必须在Ref包裹后调用，否则会丧失profileLoaded的响应性。

const editMode = ref(false);
const followMode = ref(false);
const followTab = ref(0);

let userInfoTemp = ref();
const tempCopied = ref(false);

const tabLabels : Array<TabLabel> = [
  { key: 'post', label: '帖子'},
  { key: 'reply', label: '评论'},
  { key: 'like', label: '点赞'},
];

const Tabs = new TabController(tabLabels);

watch( () => userId_p, (newId, oldId) => {
  if (newId !== oldId) {
    userId = newId || 'Me';
    if (userId === 'Me'){
      userId = User?.getInstance()?.userAuth.userId || '';
      if (!userId){
        toggleLoginHover(true);
        setTimeout(async () => await router.push('/'), 0);
      }
    }
    userInfo = ref<UserInfo>(new UserInfo(userId));
    userInfo.value.loadUserData(); // 加载用户数据，必须在Ref包裹后调用，否则会丧失profileLoaded的响应性。
  }
  editMode.value = false;
  followMode.value = false;
  followTab.value = 0;
  userInfoTemp.value = undefined;
  tempCopied.value = false;
  Tabs.switchTab(0);
}, { immediate: true });

watch(() => userInfo.value.profileLoaded && userInfo.value.tagsLoaded,
  (newValue, oldValue) => {
    if (newValue && !oldValue && userInfo.value.isMe) {
      console.log('UserPageView: User data loaded, copying to temp.');
      userInfoTemp.value = userInfo.value.copy();
      tempCopied.value = true;
    }
  }, { immediate: true });

function onCancel(){
  if (userInfoTemp.value.changed){
    userInfoTemp.value = userInfo.value.copy()
  }
  editMode.value = false
}

function onSave(){
  userInfo.value = userInfoTemp.value.copy(userInfo.value);
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
        <UserInfoEdit v-if="tempCopied" v-on:editCancel="onCancel" v-on:editSave="onSave"
                      v-model:user-info="userInfoTemp" :key="'Edit'+userId"/>
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
                  v-model:curTab="followTab" :nick-name="userInfo.nickname" :key="'follow'+userId"/>
    </transition>

    <div v-show="!editMode && !followMode">
      <!-- 页面头部 -->
      <div class="sticky top-0 flex flex-row p-4 border-b border-slate-800 bg-slate-900/75 backdrop-blur-md">
        <div class="p-2 rounded-full hover:bg-slate-800 mr-4">
          <img src="@/assets/back.svg" alt="关闭" class="w-6 h-6 cursor-pointer" @click="router.back()">
        </div>
        <h1 class="text-xl p-1 font-bold">{{ userInfo.nickname }}</h1>
      </div>
      <UserCardPage :user-info="userInfo"
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
          <privacy-view :key="'post'+index+userId"/>
        </div>
      </div>
    </div>
  </main>
</template>
