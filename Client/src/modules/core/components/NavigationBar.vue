<template>
  <div class="flex flex-col h-full">
    <!-- Logo -->

    <div class="h-16 flex items-center mb-4">
      <img src="@/assets/logo.svg" alt="Logo" class="w-9 h-9" />
      <span class="ml-2 text-xl font-bold">论坛</span>
    </div>
    <!-- 导航菜单 -->
    <nav class="flex-grow">
      <ul class="space-y-2">
        <li>
          <a
            href="#"
            class="flex items-center p-3 rounded-full hover:bg-slate-700/50 transition-colors duration-200"
          >
            <svg class="w-6 h-6 mr-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6"
              ></path>
            </svg>
            首页
          </a>
        </li>

        <li>
          <a
            href="#"
            class="flex items-center p-3 rounded-full hover:bg-slate-700/50 transition-colors duration-200"
          >
            <svg class="w-6 h-6 mr-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M7 20l4-16m2 16l4-16M6 9h14M4 15h14"
              ></path>
            </svg>
            发现
          </a>
        </li>

        <li>
          <RouterLink to="/community" custom v-slot="{ href, navigate, isActive }">
            <a
              :href="href"
              @click="navigate"
              :class="[
                'flex items-center p-3 rounded-full transition-colors duration-200',
                isActive ? activeClass : inactiveClass,
              ]"
              :aria-current="isActive ? 'page' : undefined"
            >
              <svg class="w-6 h-6 mr-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"
                />
              </svg>
              社区
            </a>
          </RouterLink>
        </li>

        <li>
          <RouterLink to="/notice" custom v-slot="{ href, navigate, isActive }">
            <a
              :href="href"
              @click="navigate"
              :class="[
                'flex items-center p-3 rounded-full transition-colors duration-200',
                isActive ? activeClass : inactiveClass,
              ]"
              :aria-current="isActive ? 'page' : undefined"
            >
              <svg class="w-6 h-6 mr-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M15 17h5l-1.405-1.405A2.032 2.032 0 0118 14.158V11a6.002 6.002 0 00-4-5.659V5a2 2 0 10-4 0v.341C7.67 6.165 6 8.388 6 11v3.159c0 .538-.214 1.055-.595 1.436L4 17h5m6 0v1a3 3 0 11-6 0v-1m6 0H9"
                />
              </svg>
              通知
            </a>
          </RouterLink>
        </li>

        <li>
          <RouterLink to="/message" custom v-slot="{ href, navigate, isActive }">
            <a
              :href="href"
              @click="navigate"
              :class="[
                'flex items-center p-3 rounded-full transition-colors duration-200',
                isActive ? activeClass : inactiveClass,
              ]"
              :aria-current="isActive ? 'page' : undefined"
            >
              <svg class="w-6 h-6 mr-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
                <path
                  stroke-linecap="round"
                  stroke-linejoin="round"
                  stroke-width="2"
                  d="M3 8l7.89 5.26a2 2 0 002.22 0L21 8M5 19h14a2 2 0 002-2V7a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z"
                />
              </svg>
              私信
            </a>
          </RouterLink>
        </li>
        <li>
          <a
            href="#"
            class="flex items-center p-3 rounded-full hover:bg-slate-700/50 transition-colors duration-200"
          >
            <svg class="w-6 h-6 mr-4" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path
                stroke-linecap="round"
                stroke-linejoin="round"
                stroke-width="2"
                d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"
              ></path>
            </svg>
            个人
          </a>
        </li>
      </ul>
    </nav>

    <div class="mt-auto">
      <a
        href="#"
        class="flex items-center p-3 rounded-full hover:bg-slate-700/50 transition-colors duration-200"
      >
        <img
          class="w-10 h-10 rounded-full"
          src="https://placehold.co/100x100/7dd3fc/0f172a?text=头像"
          alt="User Avatar"
        />
        <div class="ml-3">
          <p class="font-semibold text-sm">用户名</p>
          <p class="text-slate-400 text-xs">@username</p>
        </div>
        <svg
          class="w-5 h-5 ml-auto text-slate-400"
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
        >
          <path
            stroke-linecap="round"
            stroke-linejoin="round"
            stroke-width="2"
            d="M5 12h.01M12 12h.01M19 12h.01M6 12a1 1 0 11-2 0 1 1 0 012 0zm7 0a1 1 0 11-2 0 1 1 0 012 0zm7 0a1 1 0 11-2 0 1 1 0 012 0z"
          ></path>
        </svg>
      </a>
    </div>
  </div>
</template>

<script setup lang="ts">
const activeClass = 'bg-sky-500/10 text-sky-400 ring-1 ring-sky-500/20'
const inactiveClass =
  'flex items-center p-3 rounded-full hover:bg-slate-700/50 transition-colors duration-200'
</script>

<style scoped>
</style>
