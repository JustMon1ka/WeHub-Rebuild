<template>
  <nav class="sidebar">
    <div class="nav-item">首页</div>
    <br />
    <div class="nav-item">发现</div>
    <br />
    <div class="nav-item">社区</div>
    <br />
    <router-link
      class="nav-item"
      active-class="active"
      to="/notice"
      :class="{ active: isNoticeActive }"
      >通知</router-link
    ><br />
    <router-link class="nav-item" active-class="active" to="/message"
      >私信</router-link
    ><br />
    <div class="nav-item">个人主页</div>
  </nav>
</template>

<script setup lang="ts">
import { computed } from "vue";
import { useRoute } from "vue-router";

const route = useRoute();

const isNoticeActive = computed(() => {
  return route.path === "/notice" || route.path.startsWith("/notice/");
});
</script>

<style scoped>
.sidebar {
  display: flex;
  flex-direction: column;

  padding: 20px;
  min-width: 150px;

  box-sizing: border-box;
}

.nav-item {
  padding: 8px 12px;
  border-radius: 6px;
  transition: background-color 0.2s, color 0.2s;
  cursor: pointer;
  font-size: 20px;
  font-weight: bold;
  text-decoration: none;
  color: inherit;
  white-space: nowrap;
}

.nav-item:hover {
  background-color: rgba(25, 118, 210, 0.1);
  color: #1976d2;
}

.active {
  color: #1976d2;
  font-weight: bold;
}
</style>