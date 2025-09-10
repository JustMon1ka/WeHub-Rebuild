<template>
  <Teleport to="body">
    <div
      v-if="visible"
      class="context-menu-overview"
      @click="handleCloseClick"
      @contextmenu.prevent
    >
      <div class="context-menu" :style="menuStyle" @click.stop>
        <div
          v-for="item in menuItems"
          :key="item.key"
          class="context-menu-item"
          @click="handleMenuItemClick(item)"
        >
          <span class="menu-item-text">{{ item.text }}</span>
        </div>
      </div>
    </div>
  </Teleport>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from "vue";

export interface MenuItem {
  key: string;
  text: string;
  icon?: string;
  disabled?: boolean;
}

interface Props {
  visible: boolean;
  x: number;
  y: number;
  menuItems: MenuItem[];
}

interface Emits {
  (e: "close"): void;
  (e: "select", item: MenuItem): void;
}

const props = defineProps<Props>();
const emit = defineEmits<Emits>();

const handleCloseClick = () => {
  emit("close");
};

const handleMenuItemClick = (item: MenuItem) => {
  if (!item.disabled) {
    emit("select", item);
  }
};

// 计算菜单位置，确保菜单不会超出屏幕
const menuStyle = computed(() => {
  const menuWidth = 150;
  const menuHeight = 40 * props.menuItems.length;

  let left = props.x;
  let top = props.y;

  if (left + menuWidth > window.innerWidth) {
    left = window.innerWidth - menuWidth - 10;
  }

  if (top + menuHeight > window.innerHeight) {
    top = window.innerHeight - menuHeight - 10;
  }

  return {
    position: "fixed" as const,
    left: `${left}px`,
    top: `${top}px`,
    //zIndex: 9999,
  };
});

// 键盘事件处理
const handleKeydown = (event: KeyboardEvent) => {
  if (event.key === "Escape") {
    handleCloseClick();
  }
};

onMounted(() => {
  document.addEventListener("keydown", handleKeydown);
});

onUnmounted(() => {
  document.removeEventListener("keydown", handleKeydown);
});
</script>

<style scoped>
.context-menu-overview {
  position: fixed;
  top: 0;
  left: 0;
  width: 100vw;
  background: transparent;
}

.context-menu {
  position: absolute;
  background: white;
  border: 1px solid #ccc;
  border-radius: 8px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15), 0 2px 4px rgba(0, 0, 0, 0.1);
  padding: 4px 2px;
  min-width: 80px;
  font-size: 14px;
}

.context-menu-item {
  display: flex;
  align-items: center;
  padding: 2px 8px;
  cursor: pointer;
  border-radius: 8px;
  position: relative;
  color: #030303;
}

.context-menu-item:hover {
  background-color: #f0f2f5;
}

.menu-item-text {
  flex: 1;
  color: inherit;
}
</style>