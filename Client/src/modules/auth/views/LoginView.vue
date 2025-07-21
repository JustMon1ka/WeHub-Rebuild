<template>
  <body class="bg-slate-900 text-slate-200">
  <div class="flex items-center justify-center h-screen px-4">
    <div class="w-full max-w-[500px]">
      <!-- Logo 和标题 -->
      <div class="text-center mb-8">
        <img src="@/assets/logo.svg" alt="Logo" class="mx-auto w-16 h-16 mb-4">
        <h1 class="text-3xl font-bold mt-4">登录到您的账户</h1>
        <p class="text-slate-400 mt-2">欢迎回来！请输入您的凭据。</p>
      </div>

      <!-- 登录表单 -->
      <LoginForm/>
    </div>
  </div>
  </body>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import axios from 'axios'
import { useRouter } from 'vue-router'
import LoginForm from "../components/LoginForm.vue";

const form = ref({
  username: '',
  password: ''
})
const message = ref('')
const router = useRouter()

async function login() {
  try {
    const res = await axios.post('http://localhost:5001/api/auth/login', form.value)
    const token = res.data.token
    localStorage.setItem('token', token)
    router.push('/me')
  } catch (err) {
    message.value = err.response?.data?.message || '登录失败'
  }
}
</script>

