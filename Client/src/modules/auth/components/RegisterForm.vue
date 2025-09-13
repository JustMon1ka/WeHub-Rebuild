<script setup lang="ts">
import styles from '@/modules/auth/scripts/Styles.ts'
import { AuthData, AuthType } from '@/modules/auth/scripts/AuthData.ts';

const registerData: AuthData = new AuthData();
registerData.changeAuthType(AuthType.Register);
const username = registerData.userName;
const email = registerData.email;
const password = registerData.password;
const authCode = registerData.authCode;
const phone = registerData.phone;
</script>

<template>
  <form class="space-y-6"
        @keydown.enter.capture.prevent.stop="registerData.submit()">
    <div>
      <label for="username" v-bind:class="styles.label">用户名</label>
      <input v-bind:class="styles.input" v-model="username.userName.value" @blur="username.checkValidity()"
             type="text" id="username" name="username" placeholder="设置一个独特的用户名" required>
      <label v-if="username.error" v-bind:class="styles.error"> {{ username.errorMsg }}</label>
    </div>

    <div>
      <label for="email" v-bind:class="styles.label">邮箱</label>
      <input v-bind:class="styles.input" v-model="email.email.value" @blur="email.checkValidity()"
             type="email" id="email" name="email" placeholder="请输入您的邮箱" required>
      <label v-if="email.error" v-bind:class="styles.error"> {{ email.errorMsg }}</label>
    </div>

    <div>
      <label for="AuthCode" v-bind:class="styles.label">验证码</label>
      <div class="flex flex-row items-center justify-between space-x-3">
        <input v-bind:class="styles.input" v-model="authCode.authCode.value"
               @blur="authCode.checkValidity()" type="text" id="AuthCode" name="AuthCode"
               placeholder="请输入验证码" required>
        <button type="button" v-bind:class="authCode.authBtnStyle.value" @keydown.prevent
                @click.prevent="registerData.sendAuthCode()"> {{ authCode.btnMsg}}</button>
      </div>
      <label v-if="authCode.error" v-bind:class="styles.error"> {{ authCode.errorMsg }}</label>
    </div>

    <div>
      <label for="phone" v-bind:class="styles.label">手机号</label>
      <input v-bind:class="styles.input" v-model="phone.phone.value" @blur="phone.checkValidity()"
             type="tel" id="phone" name="phone" placeholder="请输入您的手机号" required>
      <label v-if="phone.error" v-bind:class="styles.error"> {{ phone.errorMsg }}</label>
    </div>

    <div>
      <label for="password" v-bind:class="styles.label">密码</label>
      <input v-bind:class="styles.input" v-model="password.password.value" @blur="password.checkValidity()"
             type="password" id="password" name="password" placeholder="请输入您的密码" required>
      <label v-if="password.error" v-bind:class="styles.error"> {{ password.errorMsg }}</label>
    </div>

    <div>
      <label for="confirm-password" v-bind:class="styles.label">确认密码</label>
      <input v-bind:class="styles.input" v-model="password.confirmPassword.value" @blur="password.checkConfirmValidity()"
             type="password" id="confirm-password" name="confirm-password" placeholder="请确认您的密码" required>
      <label v-if="password.confirmError" v-bind:class="styles.error"> {{ password.confirmErrorMsg }}</label>
    </div>

    <div>
      <button @click.prevent="registerData.submit()"
              v-bind:class="registerData.submitBtnStyle.value" type="submit">
        注 册
      </button>
      <div v-if="registerData.error" v-bind:class="styles.error"> {{ registerData.errorMsg }} </div>
    </div>

    <div class="space-y-3">
      <div class="flex flex-row space-x-2">
        <input type="checkbox" id="rememberMe" v-model="registerData.rememberMe.value" class="cursor-pointer">
        <label for="rememberMe" v-bind:class="styles.label"> 自动登录 </label>
      </div>

      <div class="flex flex-row space-x-2">
        <input type="checkbox" id="privacy" v-model="registerData.agreeToTerms.value" class="cursor-pointer">
        <label for="privacy" v-bind:class="styles.label"> 我已阅读并同意 </label>
        <router-link to="/privacy" for="privacy" v-bind:class="styles.RouterLink"> 隐私政策 </router-link>
      </div>
    </div>

  </form>
</template>
