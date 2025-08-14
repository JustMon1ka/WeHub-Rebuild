<template>
  <!-- 标签选择区 -->
  <div class="bg-slate-800 p-6 sm:p-8 rounded-2xl shadow-lg">
    <div class="space-y-8">

      <!-- 标签选择区 -->
      <div class="bg-slate-800 p-6 sm:p-8 rounded-2xl shadow-lg">
        <div class="space-y-8">

          <!-- 游戏分类 -->
          <div>
            <h2 class="flex items-center text-xl font-semibold border-b-2 border-slate-700 pb-2 mb-4">
              <svg class="w-6 h-6 mr-3 text-violet-400" fill="none" stroke="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M14.752 11.168l-3.197-2.132A1 1 0 0010 9.87v4.263a1 1 0 001.555.832l3.197-2.132a1 1 0 000-1.664z"></path><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 12a9 9 0 11-18 0 9 9 0 0118 0z"></path></svg>
              游戏
            </h2>
            <div class="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 gap-4">
              <button class="tag-card selected"><span class="checkmark">✓</span>角色扮演 (RPG)</button>
              <button class="tag-card"><span class="checkmark">✓</span>动作游戏 (ACT)</button>
              <button class="tag-card"><span class="checkmark">✓</span>射击游戏 (FPS)</button>
              <button class="tag-card"><span class="checkmark">✓</span>开放世界</button>
              <button class="tag-card"><span class="checkmark">✓</span>独立游戏</button>
              <button class="tag-card"><span class="checkmark">✓</span>任天堂</button>
              <button class="tag-card"><span class="checkmark">✓</span>PlayStation</button>
              <button class="tag-card"><span class="checkmark">✓</span>PC游戏</button>
            </div>
          </div>

          <!-- 科技数码分类 -->
          <div>
            <h2 class="flex items-center text-xl font-semibold border-b-2 border-slate-700 pb-2 mb-4">
              <svg class="w-6 h-6 mr-3 text-emerald-400" fill="none" stroke="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M9.75 17L9 20l-1 1h8l-1-1-.75-3M3 13h18M5 17h14a2 2 0 002-2V5a2 2 0 00-2-2H5a2 2 0 00-2 2v10a2 2 0 002 2z"></path></svg>
              科技数码
            </h2>
            <div class="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 gap-4">
              <button class="tag-card"><span class="checkmark">✓</span>智能手机</button>
              <button class="tag-card"><span class="checkmark">✓</span>笔记本电脑</button>
              <button class="tag-card selected"><span class="checkmark">✓</span>编程开发</button>
              <button class="tag-card"><span class="checkmark">✓</span>人工智能</button>
              <button class="tag-card"><span class="checkmark">✓</span>数码产品</button>
              <button class="tag-card"><span class="checkmark">✓</span>虚拟现实 (VR)</button>
            </div>
          </div>

          <!-- 影音娱乐分类 -->
          <div>
            <h2 class="flex items-center text-xl font-semibold border-b-2 border-slate-700 pb-2 mb-4">
              <svg class="w-6 h-6 mr-3 text-rose-400" fill="none" stroke="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M7 4v16M17 4v16M3 8h4m10 0h4M3 12h18M3 16h4m10 0h4M4 20h16a1 1 0 001-1V5a1 1 0 00-1-1H4a1 1 0 00-1 1v14a1 1 0 001 1z"></path></svg>
              影音娱乐
            </h2>
            <div class="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 gap-4">
              <button class="tag-card selected"><span class="checkmark">✓</span>电影</button>
              <button class="tag-card"><span class="checkmark">✓</span>电视剧</button>
              <button class="tag-card"><span class="checkmark">✓</span>动漫</button>
              <button class="tag-card"><span class="checkmark">✓</span>音乐</button>
              <button class="tag-card"><span class="checkmark">✓</span>综艺</button>
            </div>
          </div>

          <!-- 生活趣味分类 -->
          <div>
            <h2 class="flex items-center text-xl font-semibold border-b-2 border-slate-700 pb-2 mb-4">
              <svg class="w-6 h-6 mr-3 text-amber-400" fill="none" stroke="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M21 12a9 9 0 01-9 9m9-9a9 9 0 00-9-9m9 9H3m9 9a9 9 0 01-9-9m9 9V3m0 18a9 9 0 009-9m-9 9a9 9 0 00-9-9m9-9V3m0 0a9 9 0 019 9m-9-9a9 9 0 00-9 9"></path></svg>
              生活趣味
            </h2>
            <div class="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 gap-4">
              <button class="tag-card"><span class="checkmark">✓</span>美食</button>
              <button class="tag-card"><span class="checkmark">✓</span>旅行</button>
              <button class="tag-card"><span class="checkmark">✓</span>摄影</button>
              <button class="tag-card"><span class="checkmark">✓</span>宠物</button>
              <button class="tag-card"><span class="checkmark">✓</span>时尚穿搭</button>
              <button class="tag-card"><span class="checkmark">✓</span>汽车</button>
            </div>
          </div>

          <!-- 运动健康分类 -->
          <div>
            <h2 class="flex items-center text-xl font-semibold border-b-2 border-slate-700 pb-2 mb-4">
              <svg class="w-6 h-6 mr-3 text-lime-400" fill="none" stroke="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M13 10V3L4 14h7v7l9-11h-7z"></path></svg>
              运动健康
            </h2>
            <div class="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 gap-4">
              <button class="tag-card"><span class="checkmark">✓</span>健身</button>
              <button class="tag-card"><span class="checkmark">✓</span>篮球</button>
              <button class="tag-card"><span class="checkmark">✓</span>足球</button>
              <button class="tag-card"><span class="checkmark">✓</span>户外运动</button>
            </div>
          </div>

        </div>
      </div>
    </div>
  </div>
</template>
<script setup lang="ts">
</script>
