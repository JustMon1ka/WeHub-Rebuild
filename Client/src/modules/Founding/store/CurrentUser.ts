import { ref } from "vue"

export const currentUserId = ref<string | null>(null)

export function setCurrentUserId(uid: string | number) {
  currentUserId.value = String(uid)
}
