<script setup lang="ts">
import styles from '@/modules/auth/scripts/Styles.ts'
import RegisterData from '@/modules/auth/scripts/RegisterData.ts';

const registerData: RegisterData = new RegisterData();
const username = registerData.username;
const email = registerData.email;
const password = registerData.password;
const authCode = registerData.authCode;

</script>


<template>
  <form class="space-y-6">
    <div>
      <label for="username" v-bind:class="styles.label">用户名</label>
      <input v-bind:class="styles.input" v-model.lazy="username.username.value" @blur="username.checkValidity()"
             type="text" id="username" name="username" placeholder="设置一个独特的用户名" required>
      <label v-if="username.error" v-bind:class="styles.error"> {{ username.errorMsg }}</label>
    </div>

    <div>
      <label for="email" v-bind:class="styles.label">邮箱</label>
      <input v-bind:class="styles.input" v-model.lazy="email.email.value" @blur="email.checkValidity()"
             type="email" id="email" name="email" placeholder="you@example.com" required>
      <label v-if="email.error" v-bind:class="styles.error"> {{ email.errorMsg }}</label>
    </div>

    <div>
      <label for="password" v-bind:class="styles.label">密码</label>
      <input v-bind:class="styles.input" v-model.lazy="password.password.value" @blur="password.checkValidity()"
             type="password" id="password" name="password" placeholder="请输入您的密码" required>
      <label v-if="password.error" v-bind:class="styles.error"> {{ password.errorMsg }}</label>
    </div>

    <div>
      <label for="confirm-password" v-bind:class="styles.label">确认密码</label>
      <input v-bind:class="styles.input" v-model.lazy="password.confirmPassword.value" @blur="password.checkConfirmValidity()"
             type="password" id="confirm-password" name="confirm-password" placeholder="请确认您的密码" required>
      <label v-if="password.confirmError" v-bind:class="styles.error"> {{ password.confirmErrorMsg }}</label>
    </div>

    <div>
      <label for="AuthCode" v-bind:class="styles.label">验证码</label>
      <div class="flex flex-row items-center justify-between space-x-3">
        <input v-bind:class="styles.input" v-model.lazy="authCode.authCode.value"
               @blur="authCode.checkValidity()" type="text" id="AuthCode" name="AuthCode"
               placeholder="请输入验证码" required>
        <button type="button" v-bind:class="authCode.authBtnStyle.value"
                @click.prevent="registerData.sentAuthCode()"> {{ authCode.btnMsg}}</button>
      </div>
<!--      <label v-if="authCode.authCode.value" v-bind:class="styles.AuthCodeInfo">-->
<!--        验证码已发送至您的邮箱，{{ authCode.countdown }}秒-->
<!--      </label>-->
      <label v-if="authCode.error" v-bind:class="styles.error"> {{ authCode.errorMsg }}</label>
    </div>

    <div>
      <button  @click.prevent="registerData.submit()" @keydown.enter.prevent="registerData.submit()"
               v-bind:class="registerData.submitBtnStyle.value" type="submit">
        注 册
      </button>
      <div v-if="registerData.error" v-bind:class="styles.error"> {{ registerData.errorMsg }} </div>
    </div>
  </form>
</template>
