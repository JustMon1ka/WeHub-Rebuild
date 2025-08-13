<script setup lang="ts">
import LoginForm from '@/modules/auth/components/LoginForm.vue'
import styles from '@/modules/auth/scripts/Styles.ts'

const hover = defineModel<boolean>('showHover', { default: false });
</script>

<template>
  <div v-if="hover" id="LoginHover" class="md:h-screen md:w-screen w-full h-full z-50 flex items-center justify-center
         transition-all transition-discrete">
    <div class="relative bg-slate-950/90 text-slate-200 flex h-4/5 w-4/5 rounded-3xl">
      <button @click.prevent="hover = !hover" class="absolute top-5 right-5 w-8 h-8 bg-slate-800 rounded-xl p-1">
        <img src="@/assets/close.svg" alt="Close" class="w-6 h-6">
      </button>

      <div class="w-full h-full flex flex-col space-y-12 p-12 md:space-y-4 md:flex-row md:space-x-24 justify-center">
        <!-- Logo 和标题 -->
        <div class="text-center self-center justify-self-center flex flex-col">
          <img src="@/assets/logo.svg" alt="Logo" class="mx-auto w-24 h-24 mb-6">
          <h1 class="text-3xl font-bold mt-4">登录到您的账户</h1>
          <p class="text-slate-400 mt-2">欢迎回来！请输入您的凭据。</p>
        </div>

        <div class="self-center md:justify-self-center flex flex-col w-7/8 md:w-3/5">
          <div v-bind:class="styles.card">
            <LoginForm/>
          </div>
          <div class="mt-6 text-center">
            <p class="text-sm text-slate-400">
              还没有账户？
              <router-link to="/register" class="font-medium text-sky-400 hover:underline">
                立即注册
              </router-link>
            </p>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
