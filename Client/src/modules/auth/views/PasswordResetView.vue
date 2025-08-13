<script setup lang="ts">
import PasswordResetForm from '@/modules/auth/components/PasswordResetForm.vue'
import styles from '@/modules/auth/scripts/Styles.ts';
</script>

<template>
  <div v-bind:class="styles.bg">
    <div class="w-full max-w-md">
      <!-- Logo 和标题 -->
      <div class="text-center mb-8 pt-12">
        <img src="@/assets/logo.svg" alt="Logo" class="mx-auto w-16 h-16 mb-4">
        <h1 class="text-3xl font-bold mt-4">重置密码</h1>
        <p class="text-slate-400 mt-2">忘记密码了？立即找回！</p>
      </div>

      <div v-bind:class="styles.card">
        <PasswordResetForm/>
      </div>

      <div class="mt-6 text-center pb-12">
        <p class="text-sm text-slate-400">
          返回
          <router-link to="/login" class="font-medium text-sky-400 hover:underline">
            登录界面
          </router-link>
        </p>
      </div>
    </div>
  </div>
</template>

