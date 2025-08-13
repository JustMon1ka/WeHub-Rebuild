<script setup lang="ts">
import RegisterForm from '@/modules/auth/components/RegisterForm.vue';
import styles from '@/modules/auth/scripts/Styles.ts';
</script>

<template>
  <div v-bind:class="styles.bg">
    <div class="w-full max-w-md">
      <!-- Logo 和标题 -->
      <div class="text-center mb-8 pt-16">
        <img src="@/assets/logo.svg" alt="Logo" class="mx-auto w-16 h-16 mb-4">
        <h1 class="text-3xl font-bold mt-4">创建新账户</h1>
        <p class="text-slate-400 mt-2">加入我们，开启您的论坛之旅！</p>
      </div>

      <div v-bind:class="styles.card">
        <RegisterForm/>
      </div>

      <div class="mt-6 text-center pb-16">
        <p class="text-sm text-slate-400">
          已经有账户了？
          <a href="/login" class="font-medium text-sky-400 hover:underline">
            立即登录
          </a>
        </p>
      </div>
    </div>
  </div>
</template>

