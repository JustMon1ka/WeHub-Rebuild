<script setup lang="ts">
import { ref } from 'vue'

const email = ref('')
const password = ref('')
const errorMessage = ref('')

const handleSubmit = () => {
  if (!email.value || !password.value) {
    errorMessage.value = 'Email and password are required.'
    return
  }
  // Simulate a login process
  console.log(`Logging in with ${email.value} and ${password.value}`)
  errorMessage.value = ''
}
</script>

<template>
  <div class="login-form">
    <h2>Login</h2>
    <form @submit.prevent="handleSubmit">
      <div>
        <label for="email">Email:</label>
        <input type="email" id="email" v-model="email" required />
      </div>
      <div>
        <label for="password">Password:</label>
        <input type="password" id="password" v-model="password" required />
      </div>
      <button type="submit">Login</button>
      <p v-if="errorMessage" class="error">{{ errorMessage }}</p>
    </form>
  </div>
</template>
