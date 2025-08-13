<script setup lang="ts">
import styles from '@/modules/auth/scripts/Styles.ts'
import { AuthData, AuthType } from '@/modules/auth/scripts/AuthData.ts'

const loginData: AuthData = AuthData.getInstance();
loginData.changeAuthType(AuthType.PasswordLogin);
const userName = loginData.userName;
const password = loginData.password;
const authCode = loginData.authCode;
const email = loginData.email;
</script>

<template>
  <form class="space-y-6" v-if="loginData.useAuthCode.value">
    <div>
      <div class="flex justify-between">
        <label for="email" v-bind:class="styles.label">邮箱</label>
        <button class="text-sm text-sky-400 hover:underline"
                @click.prevent="loginData.changeAuthType(AuthType.PasswordLogin)">
          {{ '使用密码登录'  }}
        </button>
      </div>
      <input v-bind:class="styles.input" v-model.lazy="email.email.value" @blur="email.checkValidity()"
             type="email" id="email" name="email" placeholder="you@example.com" required>
      <label v-if="email.error" v-bind:class="styles.error"> {{ email.errorMsg }}</label>
    </div>

    <div>
      <label for="AuthCode" v-bind:class="styles.label">验证码</label>
      <div class="flex flex-row items-center justify-between space-x-3">
        <input v-bind:class="styles.input" v-model.lazy="authCode.authCode.value"
               @blur="authCode.checkValidity()" type="text" id="AuthCode" name="AuthCode"
               placeholder="请输入验证码" required>
        <button type="button" v-bind:class="authCode.authBtnStyle.value"
                @click.prevent="loginData.sendAuthCode()"> {{ authCode.btnMsg}}</button>
      </div>
      <label v-if="authCode.error" v-bind:class="styles.error"> {{ authCode.errorMsg }}</label>
    </div>

    <div>
      <button @click.prevent="loginData.submit()" @keydown.enter.prevent="loginData.submit()"
              v-bind:class="loginData.submitBtnStyle.value" type="submit" >
        登 录
      </button>
      <div v-if="loginData.error" v-bind:class="styles.error">
        {{ loginData.errorMsg }}
      </div>
    </div>
  </form>

  <form class="space-y-6" v-else>
    <div>
      <div class="flex justify-between">
        <label for="user" v-bind:class="styles.label">邮箱或用户名</label>
        <button class="text-sm text-sky-400 hover:underline"
                @click.prevent="loginData.changeAuthType(AuthType.AuthCodeLogin)">
          {{ '使用验证码登录' }}
        </button>
      </div>
      <input v-model.lazy="userName.userName.value" v-bind:class="styles.input"
             @blur="userName.checkEmpty()" id="user" name="user" placeholder="您的邮箱 / 用户名" required>
      <label v-if="userName.error" v-bind:class="styles.error"> {{ userName.errorMsg }}</label>
    </div>

    <div>
      <div class="flex justify-between items-baseline">
        <label for="password" v-bind:class="styles.label">密码</label>
        <router-link to="/passwordReset" class="text-sm text-sky-400 hover:underline">忘记密码？</router-link>
      </div>
      <input v-model.lazy="password.password.value" v-bind:class="styles.input"
             @blur="password.checkEmpty()" type="password" id="password" name="password"
             placeholder = "请输入您的密码" required>
      <label v-if="password.error" v-bind:class="styles.error"> {{ password.errorMsg }}</label>
    </div>

    <div>
      <button @click.prevent="loginData.submit()" @keydown.enter.prevent="loginData.submit()"
              v-bind:class="loginData.submitBtnStyle.value" type="submit" >
        登 录
      </button>
      <div v-if="loginData.error" v-bind:class="styles.error">
        {{ loginData.errorMsg }}
      </div>
    </div>
  </form>
</template>
