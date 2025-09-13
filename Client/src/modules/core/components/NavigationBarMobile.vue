<script setup lang="ts">

</script>

<template>
  <nav class="flex flex-row w-full justify-around">
    <router-link to="/" class="router-link">
      <img src="@/modules/core/assets/home.svg" alt="Home" class="svg">
    </router-link>
    <router-link to="/founding" class="router-link">
      <img src="@/modules/core/assets/discover.svg" alt="Discover" class="svg">
    </router-link>
    <router-link to="/community" class="router-link">
      <img src="@/modules/core/assets/community.svg" alt="Community" class="svg">
    </router-link>
    <router-link to="/notice" class="router-link">
      <img src="@/modules/core/assets/notifications.svg" alt="Notifications" class="svg">
    </router-link>
    <router-link to="/message" class="router-link">
      <img src="@/modules/core/assets/messages.svg" alt="Messages" class="svg">
    </router-link>
    <router-link to="/user_page/Me" class="router-link">
      <img src="@/modules/core/assets/profile.svg" alt="Profile" class="svg">
    </router-link>
  </nav>
</template>

<style scoped>
.svg {
  width: 1.5rem;
  height: 1.5rem;
  margin-right: 1rem;
  fill: currentColor;
}

.router-link {
  padding: 0.25rem;
}
</style>

