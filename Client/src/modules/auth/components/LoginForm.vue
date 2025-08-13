<script setup lang="ts">
import styles from '@/modules/auth/scripts/Styles.ts'
import LoginData from '@/modules/auth/scripts/LoginData.ts';

const loginData: LoginData = new LoginData();
</script>

<template>
  <form class="space-y-6">
    <div>
      <label for="email" v-bind:class="styles.label">邮箱</label>
      <input  v-model.lazy="loginData.userId.value" v-bind:class="styles.input"
              type="email" id="email" name="email" placeholder="you@example.com" required>
    </div>

    <div>
      <div class="flex justify-between items-baseline">
        <label for="password" v-bind:class="styles.label">密码</label>
        <a href="#" class="text-sm text-sky-400 hover:underline">忘记密码？</a>
      </div>
      <input v-model.lazy="loginData.password.value" v-bind:class="styles.input"
             type="password" id="password" name="password" placeholder = "password" required>
    </div>


    <div>
      <button @click.prevent="loginData.submit" @keydown.enter.prevent="loginData.submit"
              v-bind:class="loginData.submitBtnStyle.value" type="submit" >
        登 录
      </button>
      <div v-if="loginData.failed" v-bind:class="styles.error">
        {{ loginData.errorMsg }}
      </div>
    </div>
  </form>
  <!-- 登录表单 -->

</template>
