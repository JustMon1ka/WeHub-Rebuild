<script setup lang="ts">
import { computed, onMounted, ref } from 'vue';
import PlaceHolder from '@/modules/user/components/PlaceHolder.vue';
import { formatTime } from '@/modules/core/utils/time.ts';
import UserInfo from '@/modules/user/scripts/UserInfo.ts';
import type { PostListItem } from '@/modules/post/types';
import { useRouter } from 'vue-router';

const { post } = defineProps<{ post: PostListItem }>();
const userInfo = ref(new UserInfo(String(post.userId)));
const createdAtLabel = computed(()=> formatTime(String(post.createdAt)));
onMounted(()=>{ userInfo.value.loadUserData().catch(()=>{}) });

const router = useRouter();
const goDetail = () => router.push({ name:'PostDetail', params:{ id: post.postId } });
</script>

<template>
  <!-- 整卡点击进入详情 -->
  <article @click="goDetail"
           class="cursor-pointer group w-full px-4 py-3 border-b border-slate-800 hover:bg-slate-900/40 transition-colors">
    <div class="flex items-start gap-3">
      <!-- 头像：阻止冒泡，避免点头像也触发整卡跳转 -->
      <router-link @click.stop :to="{ name:'UserPage', params:{ userId_p: String(post.userId) } }" class="shrink-0">
        <img v-if="userInfo.avatarUrl" :src="userInfo.avatarUrl" class="w-10 h-10 rounded-full" alt="avatar">
        <PlaceHolder v-else width="80" height="80" :text="userInfo.nickname || ('U'+post.userId)" class="w-10 h-10 rounded-full"/>
      </router-link>

      <div class="min-w-0 flex-1">
        <div class="flex items-center gap-2 text-sm text-slate-400">
          <router-link @click.stop :to="{ name:'UserPage', params:{ userId_p: String(post.userId) } }"
                       class="font-medium text-slate-200 hover:underline truncate max-w-[40%]">
            {{ userInfo.nickname || ('用户'+post.userId) }}
          </router-link>
          <span class="text-slate-600">·</span>
          <time :datetime="post.createdAt" class="truncate">{{ createdAtLabel }}</time>
        </div>

        <!-- 标题：也允许单独点标题跳转；同时阻止冒泡，避免二次触发 -->
        <router-link @click.stop :to="{ name:'PostDetail', params:{ id: post.postId } }"
                     class="block mt-1 font-bold text-slate-100 text-[16px] leading-6 hover:text-sky-400 line-clamp-2">
          {{ post.title }}
        </router-link>

        <div class="mt-2 flex items-center gap-4 text-slate-500">
          <span class="inline-flex items-center gap-1 text-sm">
            <svg class="w-5 h-5" viewBox="0 0 24 24" fill="none" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.75"
                    d="M21 8.25c0-2.485-2.099-4.5-4.688-4.5-1.935 0-3.597 1.126-4.312 2.733-.715-1.607-2.377-2.733-4.313-2.733C5.1 3.75 3 5.765 3 8.25c0 7.125 9 12 9 12s9-4.875 9-12z"/>
            </svg>
            <span>{{ post.likes }}</span>
          </span>

          <div v-if="post.tags?.length" class="hidden md:flex items-center gap-1 flex-wrap">
            <span v-for="t in post.tags" :key="t"
                  class="px-2 py-0.5 rounded-full bg-slate-800 text-slate-400 text-xs">#{{ t }}</span>
          </div>
        </div>
      </div>
    </div>
  </article>
</template>
