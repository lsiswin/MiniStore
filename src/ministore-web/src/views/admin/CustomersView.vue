<template>
  <div class="customers-view">
    <!-- Action Bar -->
    <div class="action-card">
      <div class="search-wrap">
        <input
          v-model="searchQuery"
          type="text"
          placeholder="按姓名或手机号搜索客户..."
          class="form-input"
          @keyup.enter="fetchCustomers"
        />
        <button class="btn btn-secondary" @click="fetchCustomers">搜索</button>
      </div>

      <button class="btn btn-primary" @click="showAddModal = true">
        + 新增客户资料
      </button>
    </div>

    <!-- Customers Table -->
    <div class="table-card">
      <div v-if="loading" class="loading-box">加载中...</div>
      <div v-else-if="customers.length === 0" class="empty-box">暂无客户资料</div>
      <div v-else class="table-responsive">
        <table class="data-table">
          <thead>
            <tr>
              <th>客户姓名</th>
              <th>手机号码</th>
              <th>累计到店</th>
              <th>最近到店</th>
              <th>客户备注</th>
              <th>建档时间</th>
              <th>操作</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="c in customers" :key="c.id">
              <td class="font-bold">{{ c.name }}</td>
              <td>{{ c.phone }}</td>
              <td>
                <span class="badge badge-confirmed">{{ c.visitCount }} 次</span>
              </td>
              <td>{{ c.lastVisitAt ? formatDate(c.lastVisitAt) : '尚未到店' }}</td>
              <td class="notes">{{ c.notes || '-' }}</td>
              <td class="text-muted">{{ formatDate(c.createdAt) }}</td>
              <td>
                <button class="btn-link" @click="viewDetail(c.id)">查看档案</button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Create Customer Modal -->
    <div v-if="showAddModal" class="modal-overlay" @click.self="showAddModal = false">
      <div class="modal-content">
        <div class="modal-header">
          <h3>新增客户档案</h3>
          <button class="close-btn" @click="showAddModal = false">✕</button>
        </div>
        <form @submit.prevent="createCustomer" class="form-body">
          <div class="form-item">
            <label>客户姓名 <span class="required">*</span></label>
            <input v-model="formName" type="text" placeholder="例如：林小雨" class="form-input" required />
          </div>
          <div class="form-item">
            <label>手机号码 <span class="required">*</span></label>
            <input v-model="formPhone" type="tel" maxlength="11" placeholder="11位手机号" class="form-input" required />
          </div>
          <div class="form-item">
            <label>喜好/特征备注</label>
            <textarea v-model="formNotes" rows="2" placeholder="如：偏好裸色款、甲面薄软" class="form-textarea"></textarea>
          </div>
          <div v-if="modalError" class="form-error">{{ modalError }}</div>
          <button type="submit" class="btn btn-primary submit-btn" :disabled="saving">
            {{ saving ? '保存中...' : '确认添加' }}
          </button>
        </form>
      </div>
    </div>

    <!-- Customer Detail Modal -->
    <div v-if="detailCustomer" class="modal-overlay" @click.self="detailCustomer = null">
      <div class="modal-content">
        <div class="modal-header">
          <h3>客户完整档案</h3>
          <button class="close-btn" @click="detailCustomer = null">✕</button>
        </div>

        <div class="cust-profile">
          <div class="cp-row">
            <span class="cp-name">{{ detailCustomer.name }}</span>
            <span class="badge badge-confirmed">到店 {{ detailCustomer.visitCount }} 次</span>
          </div>
          <p class="cp-phone">联系方式：{{ detailCustomer.phone }}</p>
          <p class="cp-notes" v-if="detailCustomer.notes">备注：{{ detailCustomer.notes }}</p>
        </div>

        <div class="detail-section">
          <h4>持有效次卡 ({{ detailCustomer.cards?.length || 0 }})</h4>
          <div v-if="!detailCustomer.cards || detailCustomer.cards.length === 0" class="empty-sub">
            暂无持卡
          </div>
          <div v-else class="cards-sub-list">
            <div v-for="card in detailCustomer.cards" :key="card.id" class="card-item">
              <span class="ci-name">{{ card.cardName }}</span>
              <span class="ci-remain">剩余 {{ card.remainingCount }} 次</span>
            </div>
          </div>
        </div>

        <div class="detail-section">
          <h4>历史到店记录 ({{ detailCustomer.appointments?.length || 0 }})</h4>
          <div v-if="!detailCustomer.appointments || detailCustomer.appointments.length === 0" class="empty-sub">
            暂无预约记录
          </div>
          <div v-else class="history-list">
            <div v-for="a in detailCustomer.appointments" :key="a.id" class="hist-row">
              <span class="hist-time">{{ formatDate(a.appointmentAt) }}</span>
              <span class="hist-srv">{{ a.serviceName }}</span>
              <span class="hist-price">¥{{ a.price }}</span>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { api } from '@/api/client'
import type { CustomerListItemDto } from '@/types'

const loading = ref(true)
const customers = ref<CustomerListItemDto[]>([])
const searchQuery = ref('')

const showAddModal = ref(false)
const formName = ref('')
const formPhone = ref('')
const formNotes = ref('')
const saving = ref(false)
const modalError = ref('')

const detailCustomer = ref<any>(null)

const fetchCustomers = async () => {
  loading.value = true
  try {
    const params: any = {}
    if (searchQuery.value.trim()) params.search = searchQuery.value.trim()
    const data = await api.get('/admin/customers', { params }) as CustomerListItemDto[]
    customers.value = data
  } catch (err: any) {
    alert(err.message || '加载客户失败')
  } finally {
    loading.value = false
  }
}

const createCustomer = async () => {
  modalError.value = ''
  saving.value = true
  try {
    await api.post('/admin/customers', {
      name: formName.value.trim(),
      phone: formPhone.value.trim(),
      notes: formNotes.value.trim()
    })
    showAddModal.value = false
    formName.value = ''
    formPhone.value = ''
    formNotes.value = ''
    await fetchCustomers()
  } catch (err: any) {
    modalError.value = err.message || '保存客户失败'
  } finally {
    saving.value = false
  }
}

const viewDetail = async (id: string) => {
  try {
    const res = await api.get(`/admin/customers/${id}`)
    detailCustomer.value = res
  } catch (err: any) {
    alert(err.message || '获取客户详情失败')
  }
}

const formatDate = (str: string) => {
  if (!str) return '-'
  const d = new Date(str)
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`
}

onMounted(() => {
  fetchCustomers()
})
</script>

<style scoped>
.customers-view {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.action-card {
  background: #ffffff;
  border-radius: 12px;
  padding: 16px;
  border: 1px solid #e2e8f0;
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-wrap: wrap;
  gap: 12px;
}

.search-wrap {
  display: flex;
  gap: 8px;
  flex: 1;
  max-width: 400px;
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

.notes {
  max-width: 200px;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
  color: #64748b;
}

.text-muted {
  color: #94a3b8;
  font-size: 12px;
}

.btn-link {
  color: var(--primary);
  font-weight: 500;
}

.loading-box, .empty-box {
  padding: 40px;
  text-align: center;
  color: #94a3b8;
}

/* Modal form */
.form-body {
  margin-top: 12px;
}

.form-item {
  margin-bottom: 14px;
}

.form-item label {
  display: block;
  font-size: 13px;
  font-weight: 500;
  color: #334155;
  margin-bottom: 6px;
}

.required {
  color: #e11d48;
}

.form-input, .form-textarea {
  width: 100%;
  padding: 10px 12px;
  border: 1px solid #cbd5e1;
  border-radius: 8px;
  font-size: 14px;
}

.form-error {
  color: #e11d48;
  font-size: 13px;
  margin-bottom: 12px;
}

.submit-btn {
  width: 100%;
  padding: 12px;
  font-size: 14px;
}

.modal-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.close-btn {
  font-size: 18px;
  color: #94a3b8;
}

/* Detail styling */
.cust-profile {
  background: #f8fafc;
  padding: 16px;
  border-radius: 10px;
  margin: 16px 0;
}

.cp-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.cp-name {
  font-size: 18px;
  font-weight: 700;
}

.cp-phone {
  font-size: 13px;
  color: #64748b;
  margin-top: 4px;
}

.cp-notes {
  font-size: 13px;
  color: #475569;
  margin-top: 4px;
}

.detail-section {
  margin-top: 18px;
}

.detail-section h4 {
  font-size: 14px;
  font-weight: 600;
  color: #1e293b;
  margin-bottom: 8px;
}

.empty-sub {
  color: #94a3b8;
  font-size: 13px;
}

.cards-sub-list {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.card-item {
  display: flex;
  justify-content: space-between;
  padding: 10px 14px;
  background: #fff1f2;
  border: 1px solid #fee2e2;
  border-radius: 8px;
  font-size: 13px;
  font-weight: 500;
}

.ci-name { color: #9f1239; }
.ci-remain { color: #e11d48; font-weight: 700; }

.history-list {
  display: flex;
  flex-direction: column;
  gap: 6px;
  max-height: 180px;
  overflow-y: auto;
}

.hist-row {
  display: flex;
  justify-content: space-between;
  padding: 8px;
  border-bottom: 1px solid #f1f5f9;
  font-size: 13px;
}

.hist-time { color: #64748b; }
.hist-srv { font-weight: 500; }
.hist-price { color: #e11d48; }
</style>
