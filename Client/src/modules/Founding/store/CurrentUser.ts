import { ref } from "vue";

// 默认测试 uid=100247
export const currentUserId = ref<number>(100247);

// 登录成功后调用这个方法修改
export function setCurrentUserId(uid: number) {
  currentUserId.value = uid;
}
