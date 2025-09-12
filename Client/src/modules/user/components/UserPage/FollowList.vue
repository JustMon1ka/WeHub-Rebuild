<script setup lang="ts">
import UserList from '@/modules/user/components/UserList/UserList.vue'
import TabController, { type TabLabel } from '@/modules/user/scripts/TabController.ts'
import { User } from '@/modules/auth/public.ts'
import { watch } from 'vue'
import router from '@/router.ts'
import { toggleLoginHover } from '@/router.ts'
import { getFollowerAPI, getFollowingAPI } from '@/modules/user/scripts/FollowAPI.ts'

const { userId, nickName } = defineProps<{
  userId: string;
  nickName: string;
}>();

const emit = defineEmits<{
  (e: 'close'): void;
}>();

const curTab = defineModel('curTab', { default: 0 });

const tabLabels : Array<TabLabel> = [
  { key: 'following', label: '正在关注'},
  { key: 'follower', label: '关注者'},
];

const Tabs = new TabController(tabLabels);
Tabs.switchTab(curTab.value);

watch(curTab, (newTab) => {
  Tabs.switchTab(newTab);
});

const generator: (arg0: boolean) => AsyncGenerator<Array<string>, string, number> = async function* (follower = true) {
  let num: number = yield [];
  let page: number = 1;
  while(true){
    try {
      let result;
      if (follower) {
        result = await getFollowerAPI(page, num, userId);
      } else {
        result = await getFollowingAPI(page, num, userId);
      }

      let users = [];
      for (const followData of result.items){
        if (follower) {
          users.push(followData.followerId);
        } else {
          users.push(followData.followeeId);
        }
      }
      page += 1;
      num = yield users;
      if (users.length < num) {
        return "";
      }

    } catch (error){
      return error.message;
    }
  }
};

const generatorList = [generator(false), generator(true)];

</script>

<template>
  <div class="md:col-span-8 lg:col-span-7">
    <div v-for="(gen, index) in generatorList">
      <div v-show="curTab===index" class="overflow-hidden h-screen">
        <div class="overflow-y-auto h-full">
          <div class="sticky top-0 z-50 bg-slate-900/75 backdrop-blur-md">
            <!-- 顶部导航栏 -->
            <div class="flex items-center pt-2 px-4">
              <button @click="$emit('close')" class="p-2 rounded-full hover:bg-slate-800 mr-4">
                <img src="@/assets/close.svg" alt="Back" class="w-6 h-6">
              </button>
              <div class="flex flex-row items-center space-x-2">
                <h1 class="text-xl font-bold">{{ nickName }}</h1>
              </div>
            </div>

            <!-- Tab 切换按钮 -->
            <div class="flex border-b border-slate-800">
              <button v-for="({ key, label }, index) in tabLabels" :key="key"
                      @click="curTab=index"
                      v-bind:class="Tabs.buttons[index].class.value">
                {{ label }}
              </button>
            </div>
          </div>

          <UserList :generator="gen" :follow-btn="true" class="h-full"/>
        </div>
      </div>
    </div>
  </div>
</template>
