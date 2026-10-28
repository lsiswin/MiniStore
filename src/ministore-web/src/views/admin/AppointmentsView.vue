<template>
  <div class="appointments-view">
    <!-- Top Filter Bar -->
    <div class="filter-card">
      <div class="filter-row">
        <div class="search-box">
          <input
            v-model="searchQuery"
            type="text"
            placeholder="搜索顾客姓名、手机号或服务项目..."
            class="form-input"
            @keyup.enter="fetchAppointments"
          />
        </div>

        <div class="status-tabs">
          <button
            v-for="s in statusOptions"
            :key="s.value"
            :class="['status-tab', { active: selectedStatus === s.value }]"
            @click="changeStatus(s.value)"
          >
            {{ s.label }}
          </button>
        </div>

        <button class="btn btn-primary refresh-btn" @click="fetchAppointments">
          刷新列表
        </button>
      </div>
    </div>

    <!-- Table Card -->
    <div class="table-card">
      <div v-if="loading" class="loading-box">
        加载中...
      </div>

      <div v-else-if="appointments.length === 0" class="empty-box">
        暂无符合条件的预约记录
      </div>

      <div v-else class="table-responsive">
        <table class="data-table">
          <thead>
            <tr>
              <th>预约时间</th>
              <th>顾客姓名</th>
              <th>手机号码</th>
              <th>预约项目</th>
              <th>价格</th>
              <th>顾客备注</th>
              <th>状态</th>
              <th>下单时间</th>
              <th>操作</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="item in appointments" :key="item.id">
              <td class="font-bold">{{ formatTime(item.appointmentAt) }}</td>
              <td>{{ item.customerName }}</td>
              <td>
                <a :href="`tel:${item.customerPhone}`" class="phone-link">{{ item.customerPhone }}</a>
              </td>
              <td>{{ item.serviceName }}</td>
              <td class="price">¥{{ item.price }}</td>
              <td class="notes">{{ item.notes || '-' }}</td>
              <td>
                <span :class="['badge', `badge-${item.status}`]">
                  {{ formatStatus(item.status) }}
                </span>
              </td>
              <td class="text-muted">{{ formatCreated(item.createdAt) }}</td>
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
import type { AppointmentListItemDto } from '@/types'

const loading = ref(true)
const appointments = ref<AppointmentListItemDto[]>([])
const searchQuery = ref('')
const selectedStatus = ref('')

const statusOptions = [
  { label: '全部状态', value: '' },
  { label: '待确认', value: 'pending' },
  { label: '已确认', value: 'confirmed' },
  { label: '已完成', value: 'completed' },
  { label: '已取消', value: 'cancelled' }
]

const changeStatus = (val: string) => {
  selectedStatus.value = val
  fetchAppointments()
}

const fetchAppointments = async () => {
  loading.value = true
  try {
    const params: any = {}
    if (selectedStatus.value) params.status = selectedStatus.value
    if (searchQuery.value.trim()) params.search = searchQuery.value.trim()

    const data = await api.get('/admin/appointments', { params }) as AppointmentListItemDto[]
    appointments.value = data
  } catch (err: any) {
    alert(err.message || '加载预约失败')
  } finally {
    loading.value = false
  }
}

const updateStatus = async (id: string, status: string) => {
  try {
    await api.put(`/admin/appointments/${id}/status`, { status })
    await fetchAppointments()
  } catch (err: any) {
    alert(err.message || '状态更新失败')
  }
}

const formatTime = (timeStr: string) => {
  const d = new Date(timeStr)
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')} ${String(d.getHours()).padStart(2, '0')}:${String(d.getMinutes()).padStart(2, '0')}`
}

const formatCreated = (timeStr: string) => {
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
  fetchAppointments()
})
</script>

<style scoped>
.appointments-view {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.filter-card {
  background: #ffffff;
  border-radius: 12px;
  padding: 16px;
  border: 1px solid #e2e8f0;
}

.filter-row {
  display: flex;
  flex-wrap: wrap;
  align-items: center;
  gap: 12px;
}

.search-box {
  flex: 1;
  min-width: 240px;
}

.status-tabs {
  display: flex;
  gap: 6px;
  overflow-x: auto;
}

.status-tab {
  padding: 8px 12px;
  border-radius: 6px;
  font-size: 13px;
  background: #f1f5f9;
  color: #475569;
  font-weight: 500;
  white-space: nowrap;
}

.status-tab.active {
  background: var(--primary);
  color: #ffffff;
}

.table-card {
  background: #ffffff;
  border-radius: 12px;
  border: 1px solid #e2e8f0;
  overflow: hidden;
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

.font-bold {
  font-weight: 600;
}

.phone-link {
  color: #0284c7;
  text-decoration: underline;
}

.price {
  color: #e11d48;
  font-weight: 600;
}

.notes {
  max-width: 160px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  color: #64748b;
}

.text-muted {
  color: #94a3b8;
  font-size: 12px;
}

.action-btns {
  display: flex;
  gap: 6px;
}

.btn-act {
  padding: 4px 8px;
  border-radius: 4px;
  font-size: 12px;
  font-weight: 500;
}

.btn-act.confirm { background: #e0f2fe; color: #0284c7; }
.btn-act.complete { background: #dcfce7; color: #16a34a; }
.btn-act.cancel { background: #fee2e2; color: #dc2626; }

.loading-box, .empty-box {
  padding: 40px;
  text-align: center;
  color: #94a3b8;
}
</style>
