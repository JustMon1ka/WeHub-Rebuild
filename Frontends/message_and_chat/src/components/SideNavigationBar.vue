<template>
  <nav class="sidebar">
    <div class="nav-item">首页</div>
    <div class="nav-item">发现</div>
    <div class="nav-item">社区</div>
    <router-link class="nav-item" active-class="active" to="/notice" :class="{ active: isNoticeActive }">通知</router-link><br/>
    <router-link class="nav-item" active-class="active" to="/message">私信</router-link><br/>
    <div class="nav-item">个人主页</div>
  </nav>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import { useRoute } from 'vue-router';

const route = useRoute();

const isNoticeActive = computed(() => {
  return route.path === '/notice' || route.path.startsWith('/notice/');
});
</script>

<style scoped>
.active {
  color: #1976d2; 
  font-weight: bold;
}
</style>