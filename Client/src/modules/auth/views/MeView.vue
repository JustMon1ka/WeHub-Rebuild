<template>
  <div>
    <h2>我的信息</h2>
    <p v-if="user">{{ user }}</p>
    <button @click="logout">退出登录</button>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import axios from 'axios'
import { useRouter } from 'vue-router'

const user = ref('')
const router = useRouter()

onMounted(async () => {
  const token = localStorage.getItem('token')
  if (!token) {
    return router.push('/login')
  }

  try {
    const res = await axios.get('http://localhost:5001/api/auth/me', {
      headers: {
        Authorization: `Bearer ${token}`
      }
    })
    user.value = res.data.username
  } catch (err) {
    user.value = '获取用户信息失败'
  }
})

function logout() {
  localStorage.removeItem('token')
  router.push('/login')
}
</script>
