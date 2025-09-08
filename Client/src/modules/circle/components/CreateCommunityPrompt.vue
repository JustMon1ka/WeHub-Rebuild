<template>
  <div class="sidebar-card">
    <!-- 创建社区提示区域 -->
    <div class="create-community-card">
      <h3>创建新社区</h3>
      <p>有想法想要分享？创建一个属于你的社区吧！</p>
      <button class="btn btn-primary btn-large" style="width: 100%" @click="goToCreatePage">
        创建社区
      </button>
    </div>
  </div>
</template>

<script setup lang="ts">
import { useRouter } from 'vue-router'

const router = useRouter()

const goToCreatePage = () => {
  router.push('/create-community')
}
</script>

<style scoped>
.sidebar-card {
  background: #1e293b; /* slate-800 */
  border-radius: 12px;
  padding: 20px;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.2);
  border: 1px solid #334155; /* slate-700 */
}

.create-community-card {
  text-align: center;
  padding: 12px 0;
}

.create-community-card h3 {
  color: #f1f5f9; /* slate-100 */
  margin-bottom: 8px;
  font-size: 18px;
  font-weight: 600;
}

.create-community-card p {
  color: #64748b; /* slate-500 */
  margin-bottom: 20px;
  line-height: 1.5;
  font-size: 14px;
}

/* 按钮样式 */
.btn {
  padding: 8px 16px;
  border: none;
  border-radius: 6px;
  font-size: 14px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s;
  text-align: center;
  text-decoration: none;
  display: inline-block;
}

.btn-primary {
  background: #0ea5e9; /* sky-500 */
  color: #fff;
}

.btn-primary:hover {
  background: #0284c7; /* sky-600 */
}

.btn-large {
  padding: 12px 24px;
  font-size: 16px;
}
</style>
