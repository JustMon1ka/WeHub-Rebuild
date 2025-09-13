<template>
  <button
    @click="handleReport"
    class="flex items-center gap-2 px-4 py-2 rounded-lg border border-slate-600 hover:border-red-500 hover:bg-red-500/10 text-slate-300 hover:text-red-400 transition-all duration-200 group"
    title="举报此用户"
  >
    <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
      <path
        stroke-linecap="round"
        stroke-linejoin="round"
        stroke-width="2"
        d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-2.5L13.732 4c-.77-.833-1.964-.833-2.732 0L3.732 16.5c-.77.833.192 2.5 1.732 2.5z"
      ></path>
    </svg>
    <span class="text-sm font-medium">举报</span>
  </button>
</template>

<script setup lang="ts">
interface Props {
  userId: string
}

const props = defineProps<Props>()

const handleReport = () => {
  // 在新标签页中打开举报页面
  const reportUrl = `/report/user/${props.userId}`
  window.open(reportUrl, '_blank')
}
</script>
