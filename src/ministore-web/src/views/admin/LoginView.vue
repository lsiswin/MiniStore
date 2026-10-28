<template>
  <div class="login-page">
    <div class="login-card">
      <div class="brand-header">
        <div class="brand-icon">✨</div>
        <h1 class="brand-title">MiniStore 商家管理后台</h1>
        <p class="brand-subtitle">美甲 / 美睫 / 美容工作室轻量数字化系统</p>
      </div>

      <form @submit.prevent="handleLogin" class="login-form">
        <div class="form-item">
          <label>商家账号</label>
          <input
            v-model="username"
            type="text"
            placeholder="请输入账号 (演示: admin)"
            class="form-input"
            required
          />
        </div>

        <div class="form-item">
          <label>登录密码</label>
          <input
            v-model="password"
            type="password"
            placeholder="请输入密码 (演示: 123456)"
            class="form-input"
            required
          />
        </div>

        <div v-if="error" class="login-error">
          {{ error }}
        </div>

        <button type="submit" class="btn btn-primary login-btn" :disabled="loading">
          {{ loading ? '登录中...' : '立即登录' }}
        </button>

        <div class="demo-tip">
          <p>测试账号已内置，点击下方一键填入：</p>
          <button type="button" class="btn-demo-fill" @click="fillDemo">
            一键填入：admin / 123456
          </button>
        </div>
      </form>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { api } from '@/api/client'

const router = useRouter()
const authStore = useAuthStore()

const username = ref('admin')
const password = ref('123456')
const loading = ref(false)
const error = ref('')

const fillDemo = () => {
  username.value = 'admin'
  password.value = '123456'
}

const handleLogin = async () => {
  error.value = ''
  loading.value = true
  try {
    const res = await api.post('/admin/auth/login', {
      username: username.value.trim(),
      password: password.value
    }) as any

    authStore.setLogin(res.token, res.user)
    router.push('/admin/dashboard')
  } catch (err: any) {
    error.value = err.message || '登录失败，请检查账号密码'
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.login-page {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  background: linear-gradient(135deg, #fff1f2 0%, #f8fafc 100%);
  padding: 16px;
}

.login-card {
  width: 100%;
  max-width: 420px;
  background: #ffffff;
  border-radius: 16px;
  padding: 32px 28px;
  box-shadow: 0 10px 25px -5px rgba(244, 63, 94, 0.1), 0 8px 10px -6px rgba(0, 0, 0, 0.05);
  border: 1px solid #fee2e2;
}

.brand-header {
  text-align: center;
  margin-bottom: 28px;
}

.brand-icon {
  font-size: 36px;
  margin-bottom: 8px;
}

.brand-title {
  font-size: 20px;
  font-weight: 700;
  color: #0f172a;
}

.brand-subtitle {
  font-size: 13px;
  color: #64748b;
  margin-top: 4px;
}

.form-item {
  margin-bottom: 18px;
}

.form-item label {
  display: block;
  font-size: 13px;
  font-weight: 500;
  color: #334155;
  margin-bottom: 6px;
}

.form-input {
  width: 100%;
  padding: 10px 14px;
  border: 1px solid #cbd5e1;
  border-radius: 8px;
  font-size: 14px;
  outline: none;
  transition: border-color 0.2s;
}

.form-input:focus {
  border-color: var(--primary);
}

.login-error {
  padding: 8px 12px;
  background-color: #fee2e2;
  color: #b91c1c;
  font-size: 13px;
  border-radius: 6px;
  margin-bottom: 16px;
}

.login-btn {
  width: 100%;
  padding: 12px;
  font-size: 15px;
  border-radius: 8px;
}

.demo-tip {
  margin-top: 24px;
  padding-top: 18px;
  border-top: 1px dashed #e2e8f0;
  text-align: center;
  font-size: 12px;
  color: #64748b;
}

.btn-demo-fill {
  margin-top: 6px;
  color: var(--primary);
  font-weight: 600;
  font-size: 13px;
  text-decoration: underline;
}
</style>
