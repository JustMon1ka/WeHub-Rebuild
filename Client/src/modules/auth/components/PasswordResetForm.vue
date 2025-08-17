<script setup lang="ts">
import styles from '@/modules/auth/scripts/Styles.ts'
import { AuthData, AuthType } from '@/modules/auth/scripts/AuthData.ts'

const passwordResetData: AuthData = new AuthData();
passwordResetData.changeAuthType(AuthType.PasswordResetVerify)
const email = passwordResetData.email;
const authCode = passwordResetData.authCode;
const password = passwordResetData.password;
</script>

<template>
  <form v-if="!(passwordResetData.verified.value)" class="space-y-6"
        @keydown.enter.capture.prevent.stop="passwordResetData.submit()">
    <div>
      <label for="email" v-bind:class="styles.label">邮箱</label>
      <input v-bind:class="styles.input" v-model="email.email.value" @blur="email.checkValidity()"
             type="email" id="email" name="email" placeholder="you@example.com" required>
      <label v-if="email.error" v-bind:class="styles.error"> {{ email.errorMsg }}</label>
    </div>

    <div>
      <label for="AuthCode" v-bind:class="styles.label">验证码</label>
      <div class="flex flex-row items-center justify-between space-x-3">
        <input v-bind:class="styles.input" v-model="authCode.authCode.value"
               @blur="authCode.checkValidity()" type="text" id="AuthCode" name="AuthCode"
               placeholder="请输入验证码" required>
        <button type="button" v-bind:class="authCode.authBtnStyle.value"
                @click.prevent="passwordResetData.sendAuthCode()"> {{ authCode.btnMsg}}</button>
      </div>
      <label v-if="authCode.error" v-bind:class="styles.error"> {{ authCode.errorMsg }}</label>
    </div>

    <div>
      <button  @click.prevent="passwordResetData.submit()"
               v-bind:class="passwordResetData.submitBtnStyle.value" type="submit">
        验 证
      </button>
      <div v-if="passwordResetData.error" v-bind:class="styles.error"> {{ passwordResetData.errorMsg }} </div>
    </div>
  </form>

  <form v-else class="space-y-6"
        @keydown.enter.capture.prevent.stop="passwordResetData.submit()">
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
      <button @click.prevent="passwordResetData.submit()"
              v-bind:class="passwordResetData.submitBtnStyle.value" type="submit">
        修改密码
      </button>
      <div v-if="passwordResetData.error" v-bind:class="styles.error"> {{ passwordResetData.errorMsg }} </div>
    </div>
  </form>
</template>
