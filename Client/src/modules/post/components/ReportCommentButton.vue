<template>
  <button
    @click="handleReport"
    class="flex items-center gap-1 px-2 py-1 rounded text-xs text-slate-400 hover:text-red-400 hover:bg-red-500/10 transition-all duration-200"
    title="举报此评论"
  >
    <svg
      class="w-3 h-3"
      fill="none"
      stroke="currentColor"
      viewBox="0 0 24 24"
      xmlns="http://www.w3.org/2000/svg"
    >
      <path
        stroke-linecap="round"
        stroke-linejoin="round"
        stroke-width="2"
        d="M12 9v2m0 4h.01m-6.938 4h13.856c1.54 0 2.502-1.667 1.732-3L13.732 4c-.77-1.333-2.694-1.333-3.464 0L3.34 16c-.77 1.333.192 3 1.732 3z"
      ></path>
    </svg>
    <span>举报</span>
  </button>
</template>

<script setup lang="ts">
interface Props {
  commentId: number
}

const props = defineProps<Props>()

const handleReport = () => {
  const reportUrl = `/report/comment/${props.commentId}`
  window.open(reportUrl, '_blank')
}
</script>
