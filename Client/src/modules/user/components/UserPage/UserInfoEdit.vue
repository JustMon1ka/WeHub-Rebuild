<script setup lang="ts">
import { ref, type Ref, watch } from 'vue'
import { UserInfo } from '@/modules/user/scripts/UserInfo.ts'
import styles from '@/modules/user/scripts/Styles.ts'
import VueFlatpickr from 'vue-flatpickr-component'
import 'flatpickr/dist/flatpickr.css'
import 'flatpickr/dist/themes/dark.css'
import 'flatpickr/dist/l10n/zh.js'
import PlaceHolder from '@/modules/user/components/PlaceHolder.vue'
import TagSetForm from '@/modules/user/components/Tag/TagSetForm.vue'

const userInfo : Ref<UserInfo> = defineModel<UserInfo>('userInfo', { required: true });
const emit = defineEmits<{
  (e: 'editSave'): void;
  (e: 'editCancel'): void;
}>();

const submitBtn = ref(styles.value.btnShape + styles.value.submitBtn);
const editingTags = ref(false);

async function onSave(){
  submitBtn.value = styles.value.btnShape + styles.value.loadingBtn;
  const result = await userInfo.value.updateProfile();
  if (result){
    emit('editSave');
    userInfo.value.changed = false;
  }
  submitBtn.value = styles.value.btnShape + styles.value.submitBtn;
}

// Read more at https://flatpickr.js.org/options/
const dateConfig = ref({
  maxDate: new Date().toISOString(),
  minDate: '1900-01-01',
  locale: 'zh',
});
</script>

<template>
  <div class="sticky top-0 z-10 bg-slate-900/75 backdrop-blur-md flex items-center p-4 border-b border-slate-800 space-x-4">
    <button @click="$emit('editCancel')" class="p-2 rounded-full hover:bg-slate-800 mr-4">
      <img src="@/assets/close.svg" alt="关闭编辑" class="w-6 h-6">
    </button>
    <h1 class="text-xl font-bold flex-grow">编辑个人资料</h1>
    <transition enter-active-class="transition-opacity transition-transform duration-200"
                enter-from-class="opacity-0 translate-x-1/2"
                enter-to-class="opacity-100 translate-x-0"
                leave-active-class="transition-opacity transition-transform duration-200"
                leave-from-class="opacity-100 translate-x-0"
                leave-to-class="opacity-0 translate-x-1/2">
      <button v-if="userInfo.changed" @click="onSave()" v-bind:class="submitBtn">
        保存
      </button>
    </transition>
    <button @click="$emit('editCancel')" v-bind:class="styles.btnShape + styles.normalBtn">
      取消
    </button>
  </div>
  <!-- 个人资料编辑区 -->
  <div>
    <!-- 背景横幅 -->
    <div class="h-48 bg-slate-700 relative">
      <input @change="userInfo.uploadPicture($event, 'profile')"
             type="file" accept="image/jpeg, image/png, image/jpg" class="hidden" id="bgInput">
      <img v-if="!!userInfo?.profileURL" v-bind:src="userInfo?.profileURL"
           class="w-full h-full object-cover" alt="Profile banner">
      <img v-else src="@/modules/user/assets/default_background.svg"
           class="w-full h-full object-cover" alt="Profile banner">
      <label for="bgInput" class="bg-black/50 absolute inset-0 flex items-center justify-center">
        <label for="bgInput" class="p-3 bg-slate-900/50 rounded-full hover:bg-slate-900/75 transition-colors">
          <img src="@/modules/user/assets/camera.svg" class="w-6 h-6 cursor-pointer" alt="profile">
        </label>
      </label>
    </div>

    <!-- 头像 -->
    <div class="px-4 -mt-16">
      <div class="w-32 h-32 rounded-full border-4 border-slate-900 bg-slate-800 relative">
        <input @change="userInfo.uploadPicture($event, 'avatar')"
               type="file" accept="image/*" class="hidden" id="avatarInput">
        <img v-if="!!userInfo?.avatarURL" v-bind:src="userInfo?.avatarURL"
             v-bind:class="styles.userPic" alt="User avatar">
        <PlaceHolder width="150"  height="150" :text="userInfo.nickname"
                     v-bind:class="styles.userPic"></PlaceHolder>
        <label for="avatarInput" class="absolute inset-0 bg-black/50 flex items-center justify-center rounded-full">
          <label for="avatarInput" class="p-3 bg-slate-900/50 rounded-full hover:bg-slate-900/75 transition-colors">
            <img src="@/modules/user/assets/camera.svg" class="w-6 h-6 cursor-pointer" alt="profile">
          </label>
        </label>
      </div>
    </div>

    <!-- 表单 -->
    <form class="p-4 space-y-6">
      <label v-if="userInfo.error" v-bind:class="styles.error">{{ userInfo.errorMsg }}</label>
      <div>
        <label for="nickname" v-bind:class="styles.label">👤 昵称</label>
        <input @change="userInfo.changed = true" v-model.lazy="userInfo.nickname"
               type="text" id="nickname" v-bind:class="styles.input" tabindex="0">
      </div>
      <div>
        <label for="bio" v-bind:class="styles.label">📃 个人简介</label>
        <textarea @change="userInfo.changed = true" v-model.lazy="userInfo.bio"
                  id="bio" rows="3" v-bind:class="styles.input" tabindex="1"></textarea>
      </div>
      <div>
        <label for="location" v-bind:class="styles.label">📍 地址</label>
        <input @change="userInfo.changed = true" v-model="userInfo.location"
               type="text" id="location" v-bind:class="styles.input" tabindex="2">
      </div>
      <div>
        <label for="birthday" v-bind:class="styles.label">🎂 出生日期</label>
        <vue-flatpickr :config="dateConfig" id="birthday" v-model="userInfo.birthday" @change="userInfo.changed = true"
                       v-bind:class="styles.input" placeholder="Select date" name="date" tabindex="3"/>
      </div>
      <div>
        <div class="flex flex-row justify-between items-center">
          <label for="tags" v-bind:class="styles.label">🏷️ 标签</label>
          <button @click.prevent.stop="editingTags = !editingTags"
                  class="rounded-full transition-colors px-2.5 py-0.5 text-sm border-slate-500 border-2
                  duration-200 hover:bg-slate-800 hover:border-slate-400">
            {{ editingTags ?  '停止编辑' : '编辑标签' }}
          </button>
        </div>

        <TagSetForm v-bind:class="styles.input" tabindex="4"
                    v-model:editing="editingTags" @changed="userInfo.changed = true"
                    v-model:selected-tags="userInfo.userTags"/>
      </div>
    </form>
  </div>
</template>
