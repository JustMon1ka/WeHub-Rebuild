<template>
  <nav class="sidebar">
    <div class="nav-item">首页</div>
    <div class="nav-item">发现</div>
    <div class="nav-item">社区</div>
    <router-link class="nav-item" active-class="active" to="/notice">通知</router-link><br/>
    <router-link class="nav-item" active-class="active" to="/message">私信</router-link><br/>
    <div class="nav-item">个人主页</div>
  </nav>
</template>

<script setup lang="ts">

</script>

<style scoped>
.active {
  color: #1976d2; /* 高亮色，可自定义 */
  font-weight: bold;
}
</style>