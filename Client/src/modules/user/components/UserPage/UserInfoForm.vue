<template>

  <!-- 表单 -->
  <form class="p-4 space-y-6">
    <div>
      <label for="name" class="block text-sm font-medium text-slate-400">昵称</label>
      <input type="text" id="name" value="用户名" class="mt-1 block w-full bg-slate-800 border border-slate-700 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-sky-500 focus:border-sky-500 sm:text-sm">
    </div>
    <div>
      <label for="bio" class="block text-sm font-medium text-slate-400">个人简介</label>
      <textarea id="bio" rows="3" class="mt-1 block w-full bg-slate-800 border border-slate-700 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-sky-500 focus:border-sky-500 sm:text-sm">一个热爱游戏、代码和摄影的普通人。欢迎交流！</textarea>
    </div>
    <div>
      <label for="location" class="block text-sm font-medium text-slate-400">位置</label>
      <input type="text" id="location" value="上海" class="mt-1 block w-full bg-slate-800 border border-slate-700 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-sky-500 focus:border-sky-500 sm:text-sm">
    </div>
    <div>
      <label for="website" class="block text-sm font-medium text-slate-400">网站</label>
      <input type="url" id="website" value="https://example.com" class="mt-1 block w-full bg-slate-800 border border-slate-700 rounded-md shadow-sm py-2 px-3 focus:outline-none focus:ring-sky-500 focus:border-sky-500 sm:text-sm">
    </div>
  </form>
</template>
