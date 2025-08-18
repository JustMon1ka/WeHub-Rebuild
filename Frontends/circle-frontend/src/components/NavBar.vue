<template>
  <nav class="navbar">
    <div class="nav-container">
      <router-link to="/community" class="nav-brand">论坛</router-link>
      <ul class="nav-links">
        <li>
          <router-link
            to="/community"
            :class="{ active: $route.path === '/community' || $route.path === '/' }"
          >
            首页
          </router-link>
        </li>
        <li><a href="#">发现</a></li>
        <li>
          <router-link to="/community" :class="{ active: $route.path === '/community' }">
            社区
          </router-link>
        </li>
        <li><a href="#">通知</a></li>
        <li><a href="#">私信</a></li>
        <li><a href="#">个人主页</a></li>
      </ul>
      <div class="nav-actions">
        <button class="btn btn-primary">发帖</button>
        <div class="user-info">
          <span>用户名</span>
          <span>@username</span>
        </div>
      </div>
    </div>
  </nav>
</template>

<script setup lang="ts"></script>
