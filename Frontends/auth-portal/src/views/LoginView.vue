<template>
  <div>
    <h2>登录</h2>
    <form @submit.prevent="login">
      <input v-model="form.username" placeholder="用户名" />
      <input v-model="form.password" type="password" placeholder="密码" />
      <button type="submit">登录</button>
    </form>
    <p v-if="message">{{ message }}</p>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import axios from 'axios'
import { useRouter } from 'vue-router'

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
