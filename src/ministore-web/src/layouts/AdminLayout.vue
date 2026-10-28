<template>
  <div class="admin-layout">
    <!-- Sidebar -->
    <aside :class="['admin-sidebar', { 'mobile-open': mobileMenuOpen }]">
      <div class="sidebar-brand">
        <span class="brand-sparkle">✨</span>
        <div class="brand-text">
          <h2>MiniStore</h2>
          <span class="store-badge">{{ authStore.user?.storeName || '商家后台' }}</span>
        </div>
      </div>

      <nav class="sidebar-nav">
        <router-link
          v-for="item in navItems"
          :key="item.path"
          :to="item.path"
          class="nav-link"
          @click="mobileMenuOpen = false"
        >
          <span class="nav-icon">{{ item.icon }}</span>
          <span class="nav-title">{{ item.title }}</span>
        </router-link>
      </nav>

      <div class="sidebar-footer">
        <a :href="`/s/${authStore.user?.storeSlug || 'muse-nail'}`" target="_blank" class="preview-btn">
          <span>👀</span>
          <span>顾客端扫码预览</span>
        </a>
        <button class="logout-btn" @click="handleLogout">
          <span>🚪</span>
          <span>退出登录</span>
        </button>
      </div>
    </aside>

    <!-- Overlay for mobile drawer -->
    <div
      v-if="mobileMenuOpen"
      class="mobile-backdrop"
      @click="mobileMenuOpen = false"
    ></div>

    <!-- Main Content Area -->
    <div class="admin-main">
      <header class="admin-topbar">
        <button class="menu-toggle" @click="mobileMenuOpen = !mobileMenuOpen">
          ☰
        </button>
        <div class="topbar-title">
          {{ currentNavTitle }}
        </div>
        <div class="topbar-user">
          <span class="user-greeting">欢迎，{{ authStore.user?.displayName || '店长' }}</span>
          <a :href="`/s/${authStore.user?.storeSlug || 'muse-nail'}`" target="_blank" class="h5-link">
            查看 H5 门店 →
          </a>
        </div>
      </header>

      <main class="admin-body">
        <router-view />
      </main>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()

const mobileMenuOpen = ref(false)

const navItems = [
  { path: '/admin/dashboard', title: '经营概览', icon: '📊' },
  { path: '/admin/appointments', title: '预约管理', icon: '📅' },
  { path: '/admin/customers', title: '客户档案', icon: '👥' },
  { path: '/admin/services', title: '服务项目', icon: '💅' },
  { path: '/admin/cards', title: '会员次卡', icon: '💳' },
  { path: '/admin/settings', title: '门店与二维码', icon: '⚙️' }
]

const currentNavTitle = computed(() => {
  const item = navItems.find(n => route.path.startsWith(n.path))
  return item ? item.title : '商家管理后台'
})

const handleLogout = () => {
  if (confirm('确认退出登录吗？')) {
    authStore.logout()
    router.push('/admin/login')
  }
}
</script>

<style scoped>
.admin-layout {
  display: flex;
  min-height: 100vh;
  background-color: #f1f5f9;
}

/* Sidebar */
.admin-sidebar {
  width: 240px;
  background: #ffffff;
  border-right: 1px solid #e2e8f0;
  display: flex;
  flex-direction: column;
  position: sticky;
  top: 0;
  height: 100vh;
  z-index: 50;
  transition: transform 0.25s ease;
}

.sidebar-brand {
  padding: 20px 16px;
  display: flex;
  align-items: center;
  gap: 10px;
  border-bottom: 1px solid #f1f5f9;
}

.brand-sparkle {
  font-size: 24px;
}

.brand-text h2 {
  font-size: 16px;
  font-weight: 700;
  color: #0f172a;
}

.store-badge {
  font-size: 11px;
  color: var(--primary);
  display: block;
  max-width: 140px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.sidebar-nav {
  flex: 1;
  padding: 16px 12px;
  display: flex;
  flex-direction: column;
  gap: 4px;
  overflow-y: auto;
}

.nav-link {
  display: flex;
  align-items: center;
  gap: 10px;
  padding: 10px 14px;
  border-radius: 8px;
  font-size: 14px;
  font-weight: 500;
  color: #475569;
  transition: all 0.2s;
}

.nav-link:hover {
  background-color: #f8fafc;
  color: #0f172a;
}

.nav-link.router-link-active {
  background-color: #fff1f2;
  color: var(--primary);
  font-weight: 600;
}

.sidebar-footer {
  padding: 16px 12px;
  border-top: 1px solid #f1f5f9;
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.preview-btn {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 6px;
  padding: 8px;
  background-color: #ecfdf5;
  color: #059669;
  border-radius: 6px;
  font-size: 12px;
  font-weight: 600;
}

.logout-btn {
  display: flex;
  align-items: center;
  gap: 6px;
  padding: 8px 12px;
  font-size: 13px;
  color: #64748b;
  border-radius: 6px;
}

.logout-btn:hover {
  background-color: #fee2e2;
  color: #dc2626;
}

/* Main Area */
.admin-main {
  flex: 1;
  display: flex;
  flex-direction: column;
  min-width: 0;
}

.admin-topbar {
  height: 60px;
  background: #ffffff;
  border-bottom: 1px solid #e2e8f0;
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 0 24px;
  position: sticky;
  top: 0;
  z-index: 40;
}

.menu-toggle {
  display: none;
  font-size: 20px;
  color: #475569;
}

.topbar-title {
  font-size: 16px;
  font-weight: 700;
  color: #0f172a;
}

.topbar-user {
  display: flex;
  align-items: center;
  gap: 16px;
  font-size: 13px;
}

.user-greeting {
  color: #64748b;
}

.h5-link {
  color: var(--primary);
  font-weight: 600;
}

.admin-body {
  padding: 24px;
  flex: 1;
  overflow-y: auto;
}

/* Responsive */
@media (max-width: 768px) {
  .menu-toggle {
    display: block;
  }

  .admin-sidebar {
    position: fixed;
    top: 0;
    left: 0;
    bottom: 0;
    transform: translateX(-100%);
    box-shadow: 2px 0 10px rgba(0, 0, 0, 0.1);
  }

  .admin-sidebar.mobile-open {
    transform: translateX(0);
  }

  .mobile-backdrop {
    position: fixed;
    inset: 0;
    background: rgba(0, 0, 0, 0.4);
    z-index: 45;
  }

  .admin-body {
    padding: 16px 12px;
  }
}
</style>
