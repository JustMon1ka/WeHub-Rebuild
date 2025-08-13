<script setup lang="ts">
import LoginForm from "@/modules/auth/components/LoginForm.vue";
import styles from "@/modules/auth/scripts/Styles.ts";
</script>

<template>
  <div v-bind:class="styles.bg">
    <div class="w-full max-w-md">
      <!-- Logo 和标题 -->
      <div class="text-center mb-8 pt-12">
        <img src="@/assets/logo.svg" alt="Logo" class="mx-auto w-16 h-16 mb-4">
        <h1 class="text-3xl font-bold mt-4">登录到您的账户</h1>
        <p class="text-slate-400 mt-2">欢迎回来！请输入您的凭据。</p>
      </div>

      <div v-bind:class="styles.card">
        <LoginForm/>
      </div>

      <div class="mt-6 text-center pb-12">
        <p class="text-sm text-slate-400">
          还没有账户？
          <a href="/register" class="font-medium text-sky-400 hover:underline">
            立即注册
          </a>
        </p>
      </div>
    </div>
  </div>
</template>

