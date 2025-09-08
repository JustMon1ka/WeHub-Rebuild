<template>
  <nav class="navbar">
    <div class="nav-container">
      <router-link to="/community" class="nav-brand">论坛</router-link>
      <ul class="nav-links">
        <li>
          <router-link
            to="/community"
            :class="{ active: $route.path === '/community' || $route.path === '/' }"
          >
            首页
          </router-link>
        </li>
        <li><a href="#">发现</a></li>
        <li>
          <router-link to="/community" :class="{ active: $route.path === '/community' }">
            社区
          </router-link>
        </li>
        <li><a href="#">通知</a></li>
        <li><a href="#">私信</a></li>
        <li><a href="#">个人主页</a></li>
      </ul>
      <div class="nav-actions">
        <button class="btn btn-primary">发帖</button>
        <div class="user-info">
          <span>用户名</span>
          <span>@username</span>
        </div>
      </div>
    </div>
  </nav>
</template>

<style scoped>
.navbar {
  background: #1e293b; /* slate-800 */
  border-bottom: 1px solid #334155; /* slate-700 */
  position: sticky;
  top: 0;
  z-index: 50;
}

.nav-container {
  max-width: 1200px;
  margin: 0 auto;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 12px 20px;
}

.nav-brand {
  font-size: 24px;
  font-weight: 700;
  color: #38bdf8; /* sky-400 */
  text-decoration: none;
}

.nav-links {
  display: flex;
  list-style: none;
  margin: 0;
  padding: 0;
  gap: 24px;
}

.nav-links a {
  color: #cbd5e1; /* slate-300 */
  text-decoration: none;
  padding: 8px 16px;
  border-radius: 8px;
  transition: all 0.2s;
  font-weight: 500;
}

.nav-links a:hover,
.nav-links a.active {
  color: #f1f5f9; /* slate-100 */
  background: #334155; /* slate-700 */
}

.nav-actions {
  display: flex;
  align-items: center;
  gap: 16px;
}

.btn {
  padding: 8px 16px;
  border: none;
  border-radius: 6px;
  font-size: 14px;
  font-weight: 500;
  cursor: pointer;
  transition: all 0.2s;
}

.btn-primary {
  background: #0ea5e9; /* sky-500 */
  color: #fff;
}

.btn-primary:hover {
  background: #0284c7; /* sky-600 */
}

.user-info {
  display: flex;
  flex-direction: column;
  align-items: flex-end;
  font-size: 12px;
}

.user-info span:first-child {
  color: #f1f5f9; /* slate-100 */
  font-weight: 500;
}

.user-info span:last-child {
  color: #64748b; /* slate-500 */
}
</style>
