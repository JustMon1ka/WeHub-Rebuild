<script setup lang="ts">
import UserList from '@/modules/user/components/UserList/UserList.vue'
import TabController, { type TabLabel } from '@/modules/user/scripts/TabController.ts'
import { User } from '@/modules/auth/public.ts'
import { watch } from 'vue'

const { userId_p } = defineProps<{
  userId_p: string;
}>();

const emit = defineEmits<{
  (e: 'close'): void;
}>();

const curTab = defineModel('curTab', { default: 0 });

let userId = userId_p || 'Me';
if (userId === 'Me'){
  userId = User?.getInstance()?.userAuth.userId || 'unknown';
}

const tabs = [UserList, UserList];

const tabLabels : Array<TabLabel> = [
  { key: 'following', label: '正在关注'},
  { key: 'follower', label: '关注者'},
];

const Tabs = new TabController(tabs, tabLabels);
Tabs.switchTab(curTab.value);

watch(curTab, (newTab) => {
  console.log('切换到 Tab:', newTab);
  Tabs.switchTab(newTab);
});
</script>

<template>
  <main class="col-span-1 md:col-span-8 lg:col-span-7 border-x border-slate-800 min-h-screen">
    <div class="sticky top-0 z-10 bg-slate-900/80 backdrop-blur-md">
      <!-- 顶部导航栏 -->
      <div class="flex items-center p-3 border-b border-slate-800">
        <button @click="$emit('close')" class="p-2 rounded-full hover:bg-slate-800 mr-4">
          <img src="@/assets/close.svg" alt="Back" class="w-6 h-6">
        </button>
        <div class="flex flex-row items-center space-x-2">
          <h1 class="text-xl font-bold">用户名</h1>
          <p class="text-md text-slate-500">@username</p>
        </div>
      </div>

      <!-- Tab 切换按钮 -->
      <div class="flex border-b border-slate-800 sticky top-0">
        <button v-for="({ key, label }, index) in tabLabels" :key="key"
                @click="curTab=index"
                v-bind:class="Tabs.buttons[index].class.value">
          {{ label }}
        </button>
      </div>
    </div>

    <!-- 内容切换 Tab -->
    <keep-alive>
      <component :is="Tabs.currentTab"> </component>
    </keep-alive>
  </main>
</template>
