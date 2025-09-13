import { ref } from "vue"
import { User } from '@/modules/auth/public.ts'

if (User.loading) {
  User.afterLoadCallbacks.push(() => {
    setCurrentUserId(User.getInstance()?.userAuth.userId || '');
  })
}

export const currentUserId = ref<string | null>(null)

export function setCurrentUserId(uid: string | number) {
  currentUserId.value = String(uid)
}
