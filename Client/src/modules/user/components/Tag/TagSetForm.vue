<script setup lang="ts">
import { onMounted, onUnmounted, ref, type Ref } from 'vue'
import { nanoid } from 'nanoid'
import styles from '@/modules/user/scripts/Styles.ts'
import UserInfo from '@/modules/user/scripts/UserInfo.ts'

const emit = defineEmits<{
  (e: 'changed'): void;
}>();

const selectedTags: Ref<Map<string, string>> = defineModel<Map<string, string>>(
  'selectedTags', { required: true, default: () => new Map<string, string>() });

const editing: Ref<boolean> = defineModel<boolean>(
  'editing', { required: true, default: false });

const input = ref<string>('');
const formId = 'tagForm' + String(nanoid(8));
let formElement = document.getElementById(formId);
const tagStyle = ref(styles.value.TagBasic);


function onRemoveTag(event: MouseEvent) {
  const target = event.target as HTMLButtonElement;
  let tagId: string = target.id;

  if (!tagId) return;
  selectedTags.value.delete(tagId);
  emit('changed');
}

async function addTag(event: Event) {
  let tagName: string = input.value.trim();

  // 替换空格为连字符
  tagName = tagName.replace(/\s+/g, '-');

  const tagId = await UserInfo.getTagId(tagName);
  if (tagName && tagId) {
    // 添加标签到集合
    selectedTags.value.set(tagId, tagName);
    emit('changed');
  }
  input.value = '';
}

onMounted(() => {
  // 确保 formElement 在组件挂载后被正确获取
  formElement = document.getElementById(formId);
  document.addEventListener('click', onfocus);
});

onUnmounted(() => {
  // 清理事件监听器
  document.removeEventListener('click', onfocus);
});

function onfocus(event: Event) {
  if (!formElement) {
    formElement = document.getElementById(formId);
  }

  const target = event.target as HTMLElement;
  editing.value = Boolean(formElement && formElement.contains(target));
}
</script>

<template>
  <div>
    <!-- 标签选择区 -->
    <div :id="formId" @click.prevent.capture="onfocus" tabindex="0">
      <div class="pt-2 space-y-2 space-x-2">
        <TransitionGroup name="list">
          <button v-for="([id ,tag], index) in selectedTags"
                  :key="'btn' + tag" :id="'btn' + id" :class="tagStyle">
            <label :for="tag" :id="id" @click.prevent.stop="onRemoveTag" v-show="editing" :class="styles.TagDelete">×</label>
            {{ tag }}
          </button>
          <input :key="input" v-model.lazy="input" @blur="addTag" @keyup.enter.prevent="addTag" type="text"
                 placeholder="输入标签并回车添加" :class="'w-24' + styles.TagBasic">
        </TransitionGroup>
      </div>
    </div>
  </div>
</template>

<style scoped>
.list-move, /* 对移动中的元素应用的过渡 */
.list-enter-active,
.list-leave-active {
  transition: all 0.5s ease;
}

.list-enter-from,
.list-leave-to {
  opacity: 0;
  transform: translateX(30px);
}

/* 确保将离开的元素从布局流中删除
  以便能够正确地计算移动的动画。 */
.list-leave-active {
  position: absolute;
}

</style>
