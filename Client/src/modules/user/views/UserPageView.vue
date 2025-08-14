<script setup lang="ts">
import UserCardPage from '@/modules/user/components/UserPage/UserCardPage.vue'
import styles from '@/modules/user/scripts/Styles.ts'
import { computed, ref } from 'vue'
import UserInfo from '@/modules/user/scripts/UserInfo.ts'
import UserInfoEdit from '@/modules/user/components/UserPage/UserInfoEdit.vue'
import TabController from '@/modules/user/scripts/TabController.ts'
import type { TabLabel} from '@/modules/user/scripts/TabController.ts'
import UserInfoForm from '@/modules/user/components/UserPage/UserInfoForm.vue'
import LoginForm from '@/modules/auth/components/LoginForm.vue'
import RegisterForm from '@/modules/auth/components/RegisterForm.vue'

const { userId } = defineProps<{
  userId: string
}>();

const userInfo =new UserInfo(userId);
const editMode = ref(false);

const tabLabels : Array<TabLabel> = [
  { key: 'post', label: '帖子'},
  { key: 'reply', label: '评论'},
  { key: 'like', label: '点赞'},
];

const tabs = [UserInfoForm, LoginForm, RegisterForm];
// TODO: 将 tabs 中的组件替换为实际的用户相关组件
const props = {};
const Tabs = new TabController(tabs, tabLabels, props);
</script>

<template>
<main class="flex flex-col border-x border-slate-800 col-span-1 md:col-span-8 lg:col-span-7 min-h-screen w-full">
  <transition name="slide"
              enter-active-class="transition duration-300 ease-out"
              enter-from-class="opacity-100 translate-y-full"
              enter-to-class="opacity-100 translate-y-0"
              leave-active-class="transition duration-300 ease-in"
              leave-from-class="opacity-100 translate-y-0"
              leave-to-class="opacity-100 translate-y-full">
    <keep-alive>
      <div v-if="editMode">
        <!--      userInfo.isMe.value && -->
        <!-- 页面头部 -->
        <UserInfoEdit v-on:editOver="editMode = false" v-on:editSave="editMode = false"/>
      </div>
    </keep-alive>
  </transition>

  <keep-alive>
    <div v-if="!editMode">
      <UserCardPage user-id="userId" v-model:user-info="userInfo" v-on:editProfile="editMode=true"/>

      <!-- 内容切换 Tab -->
      <div class="flex border-b border-slate-800">
        <button v-for="({ key, label }, index) in tabLabels" :key="key"
                @click="Tabs.switchTab($event, index)"
                v-bind:class="Tabs.buttons[index].class.value">
          {{ label }}
        </button>
      </div>

      <component :is="Tabs.currentTab" class="p-4"> </component>
    </div>
  </keep-alive>
</main>
</template>
