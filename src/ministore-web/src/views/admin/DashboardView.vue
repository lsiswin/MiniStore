<template>
  <div class="dashboard-view">
    <!-- Stat Cards -->
    <div class="stats-grid">
      <div class="stat-card">
        <div class="stat-icon red">📅</div>
        <div class="stat-info">
          <span class="stat-label">今日预约量</span>
          <span class="stat-val">{{ summary.todayAppointmentsCount }} <small>单</small></span>
        </div>
      </div>

      <div class="stat-card">
        <div class="stat-icon yellow">⏳</div>
        <div class="stat-info">
          <span class="stat-label">待确认预约</span>
          <span class="stat-val highlight">{{ summary.pendingAppointmentsCount }} <small>单</small></span>
        </div>
      </div>

      <div class="stat-card">
        <div class="stat-icon green">💰</div>
        <div class="stat-info">
          <span class="stat-label">今日预估营收</span>
          <span class="stat-val">¥{{ summary.todayEstimatedRevenue }}</span>
        </div>
      </div>

      <div class="stat-card">
        <div class="stat-icon blue">👥</div>
        <div class="stat-info">
          <span class="stat-label">累计客户数</span>
          <span class="stat-val">{{ summary.totalCustomersCount }} <small>人</small></span>
        </div>
      </div>
    </div>

    <!-- Quick Shortcuts -->
    <div class="quick-section">
      <h3>快捷操作</h3>
      <div class="shortcuts-grid">
        <router-link to="/admin/appointments" class="shortcut-card">
          <span class="sc-icon">📋</span>
          <span class="sc-title">处理预约订单</span>
        </router-link>
        <router-link to="/admin/services" class="shortcut-card">
          <span class="sc-icon">💅</span>
          <span class="sc-title">上架/修改服务</span>
        </router-link>
        <router-link to="/admin/cards" class="shortcut-card">
          <span class="sc-icon">💳</span>
          <span class="sc-title">次卡快捷核销</span>
        </router-link>
        <router-link to="/admin/settings" class="shortcut-card">
          <span class="sc-icon">🖨️</span>
          <span class="sc-title">生成扫码台卡</span>
        </router-link>
      </div>
    </div>

    <!-- Recent Appointments Table -->
    <div class="table-card">
      <div class="table-header">
        <h3>最新到店预约</h3>
        <router-link to="/admin/appointments" class="view-all">查看全部 →</router-link>
      </div>

      <div v-if="loading" class="loading-box">
        正在获取最新数据...
      </div>

      <div v-else-if="summary.recentAppointments.length === 0" class="empty-box">
        暂无预约记录
      </div>

      <div v-else class="table-responsive">
        <table class="data-table">
          <thead>
            <tr>
              <th>预约时间</th>
              <th>顾客姓名</th>
              <th>联系电话</th>
              <th>预约项目</th>
              <th>参考价格</th>
              <th>状态</th>
              <th>操作</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="item in summary.recentAppointments" :key="item.id">
              <td class="font-medium">{{ formatTime(item.appointmentAt) }}</td>
              <td>{{ item.customerName }}</td>
              <td>{{ item.customerPhone }}</td>
              <td>{{ item.serviceName }}</td>
              <td class="price">¥{{ item.price }}</td>
              <td>
                <span :class="['badge', `badge-${item.status}`]">
                  {{ formatStatus(item.status) }}
                </span>
              </td>
              <td>
                <div class="action-btns">
                  <button
                    v-if="item.status === 'pending'"
                    class="btn-act confirm"
                    @click="updateStatus(item.id, 'confirmed')"
                  >
                    确认
                  </button>
                  <button
                    v-if="item.status === 'confirmed'"
                    class="btn-act complete"
                    @click="updateStatus(item.id, 'completed')"
                  >
                    已到店
                  </button>
                  <button
                    v-if="item.status !== 'cancelled' && item.status !== 'completed'"
                    class="btn-act cancel"
                    @click="updateStatus(item.id, 'cancelled')"
                  >
                    取消
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { api } from '@/api/client'
import type { DashboardSummaryDto } from '@/types'

const loading = ref(true)
const summary = ref<DashboardSummaryDto>({
  todayAppointmentsCount: 0,
  pendingAppointmentsCount: 0,
  totalCustomersCount: 0,
  todayEstimatedRevenue: 0,
  recentAppointments: []
})

const fetchDashboard = async () => {
  loading.value = true
  try {
    const data = await api.get('/admin/dashboard') as DashboardSummaryDto
    summary.value = data
  } catch (err) {
    console.error('Fetch dashboard failed', err)
  } finally {
    loading.value = false
  }
}

const updateStatus = async (id: string, status: string) => {
  try {
    await api.put(`/admin/appointments/${id}/status`, { status })
    await fetchDashboard()
  } catch (err: any) {
    alert(err.message || '更新失败')
  }
}

const formatTime = (timeStr: string) => {
  const d = new Date(timeStr)
  return `${d.getMonth() + 1}/${d.getDate()} ${String(d.getHours()).padStart(2, '0')}:${String(d.getMinutes()).padStart(2, '0')}`
}

const formatStatus = (s: string) => {
  const map: Record<string, string> = {
    pending: '待确认',
    confirmed: '已确认',
    completed: '已完成',
    cancelled: '已取消'
  }
  return map[s] || s
}

onMounted(() => {
  fetchDashboard()
})
</script>

<style scoped>
.dashboard-view {
  display: flex;
  flex-direction: column;
  gap: 24px;
}

/* Stats */
.stats-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
  gap: 16px;
}

.stat-card {
  background: #ffffff;
  border-radius: 12px;
  padding: 20px 16px;
  display: flex;
  align-items: center;
  gap: 16px;
  border: 1px solid #e2e8f0;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.05);
}

.stat-icon {
  width: 48px;
  height: 48px;
  border-radius: 12px;
  display: flex;
  align-items: center;
  justify-content: center;
  font-size: 22px;
}

.stat-icon.red { background: #fee2e2; }
.stat-icon.yellow { background: #fef3c7; }
.stat-icon.green { background: #dcfce7; }
.stat-icon.blue { background: #e0f2fe; }

.stat-info {
  display: flex;
  flex-direction: column;
}

.stat-label {
  font-size: 13px;
  color: #64748b;
}

.stat-val {
  font-size: 22px;
  font-weight: 700;
  color: #0f172a;
  margin-top: 2px;
}

.stat-val.highlight {
  color: #d97706;
}

.stat-val small {
  font-size: 12px;
  font-weight: normal;
  color: #64748b;
}

/* Shortcuts */
.quick-section h3 {
  font-size: 15px;
  font-weight: 600;
  color: #334155;
  margin-bottom: 12px;
}

.shortcuts-grid {
  display: grid;
  grid-template-columns: repeat(auto-fit, minmax(140px, 1fr));
  gap: 12px;
}

.shortcut-card {
  background: #ffffff;
  border: 1px solid #e2e8f0;
  border-radius: 10px;
  padding: 14px;
  display: flex;
  align-items: center;
  gap: 10px;
  transition: all 0.2s;
}

.shortcut-card:hover {
  border-color: var(--primary);
  transform: translateY(-2px);
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.05);
}

.sc-icon {
  font-size: 20px;
}

.sc-title {
  font-size: 13px;
  font-weight: 500;
  color: #1e293b;
}

/* Table Card */
.table-card {
  background: #ffffff;
  border-radius: 12px;
  border: 1px solid #e2e8f0;
  overflow: hidden;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.05);
}

.table-header {
  padding: 16px 20px;
  display: flex;
  align-items: center;
  justify-content: space-between;
  border-bottom: 1px solid #f1f5f9;
}

.table-header h3 {
  font-size: 16px;
  font-weight: 600;
  color: #0f172a;
}

.view-all {
  font-size: 13px;
  color: var(--primary);
  font-weight: 500;
}

.loading-box, .empty-box {
  padding: 40px;
  text-align: center;
  color: #94a3b8;
  font-size: 14px;
}

.table-responsive {
  overflow-x: auto;
}

.data-table {
  width: 100%;
  border-collapse: collapse;
  text-align: left;
  font-size: 13px;
}

.data-table th {
  background-color: #f8fafc;
  padding: 12px 16px;
  font-weight: 600;
  color: #475569;
  border-bottom: 1px solid #e2e8f0;
}

.data-table td {
  padding: 14px 16px;
  border-bottom: 1px solid #f1f5f9;
  color: #334155;
}

.font-medium {
  font-weight: 500;
}

.price {
  color: #e11d48;
  font-weight: 600;
}

.action-btns {
  display: flex;
  gap: 6px;
}

.btn-act {
  padding: 4px 10px;
  border-radius: 4px;
  font-size: 12px;
  font-weight: 500;
}

.btn-act.confirm {
  background: #e0f2fe;
  color: #0284c7;
}

.btn-act.complete {
  background: #dcfce7;
  color: #16a34a;
}

.btn-act.cancel {
  background: #fee2e2;
  color: #dc2626;
}
</style>
