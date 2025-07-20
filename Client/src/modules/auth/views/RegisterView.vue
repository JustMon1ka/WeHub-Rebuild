<template>
  <div>
    <h2>注册</h2>
    <form @submit.prevent="register">
      <input v-model="form.username" placeholder="用户名" />
      <input v-model="form.email" placeholder="邮箱" />
      <input v-model="form.phone" placeholder="手机号" />
      <input v-model="form.password" type="password" placeholder="密码" />
      <button type="submit">注册</button>
    </form>
    <p v-if="message">{{ message }}</p>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import axios from 'axios'
import { useRouter } from 'vue-router'

const form = ref({
  username: '',
  email: '',
  phone: '',
  password: ''
})
const message = ref('')
const router = useRouter()

async function register() {
  try {
    const res = await axios.post('http://localhost:5001/api/auth/register', form.value)
    message.value = res.data.message || '注册成功'
    router.push('/login')
  } catch (err) {
    message.value = err.response?.data?.message || '注册失败'
  }
}
</script>
