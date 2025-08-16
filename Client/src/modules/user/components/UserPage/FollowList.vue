<script setup lang="ts">
import UserList from '@/modules/user/components/UserList/UserList.vue'
import TabController, { type TabLabel } from '@/modules/user/scripts/TabController.ts'
import { User } from '@/modules/auth/public.ts'
import { watch } from 'vue'

const { userId_p, nickName } = defineProps<{
  userId_p: string;
  nickName: string;
}>();

const emit = defineEmits<{
  (e: 'close'): void;
}>();

const curTab = defineModel('curTab', { default: 0 });

let userId = userId_p || 'Me';
if (userId === 'Me'){
  userId = User?.getInstance()?.userAuth.userId || 'unknown';
}

const tabLabels : Array<TabLabel> = [
  { key: 'following', label: '正在关注'},
  { key: 'follower', label: '关注者'},
];

const Tabs = new TabController(tabLabels);
Tabs.switchTab(curTab.value);

watch(curTab, (newTab) => {
  Tabs.switchTab(newTab);
});

const generator: () => AsyncGenerator<Array<string>, string, number> = async function* () {
  let num: number = yield [];
  for(let j =0; j< 3; j++){
    let users: Array<string> = [];
    for (let i = 0; i < num; i++) {
      users.push('Me');
    }
    let result = await new Promise((resolve) => {
      setTimeout(() => {
        resolve(users);
      }, 1000);
    })
    if (j > 2) {
      return "网络错误，请稍后再试";
    }
    num = yield users;
  }
  return "";
};

const generatorList = [generator(), generator()];

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
