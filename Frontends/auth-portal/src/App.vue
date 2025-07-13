<script setup>
import HelloWorld from './components/HelloWorld.vue'
</script>

<template>
  <div>
    <h1>Auth Portal</h1>
    <router-link to="/register">注册</router-link> |
    <router-link to="/login">登录</router-link> |
    <router-link to="/me">我的信息</router-link>
    <router-view />
  </div>
</template>


<style scoped>
.logo {
  height: 6em;
  padding: 1.5em;
  will-change: filter;
  transition: filter 300ms;
}
.logo:hover {
  filter: drop-shadow(0 0 2em #646cffaa);
}
.logo.vue:hover {
  filter: drop-shadow(0 0 2em #42b883aa);
}
</style>
