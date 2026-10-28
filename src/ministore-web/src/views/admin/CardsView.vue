<template>
  <div class="cards-view">
    <!-- Header -->
    <div class="action-card">
      <div class="header-titles">
        <h2>会员次卡与快捷核销</h2>
        <p>支持创建次卡套餐（如5次美甲卡）、给客户绑卡并在到店时一键核销</p>
      </div>

      <div class="btn-group">
        <button class="btn btn-secondary" @click="showTemplateModal = true">
          + 新建次卡模板
        </button>
        <button class="btn btn-primary" @click="openBindModal">
          💳 为客户开卡/绑卡
        </button>
      </div>
    </div>

    <!-- Templates Overview -->
    <div class="templates-section">
      <h3 class="section-title">次卡套餐模板 ({{ templates.length }})</h3>
      <div class="templates-grid">
        <div v-for="t in templates" :key="t.id" class="tpl-card">
          <div class="tpl-top">
            <span class="tpl-type">{{ t.type === 'times' ? '计次卡' : '储值卡' }}</span>
            <span class="tpl-price">¥{{ t.price }}</span>
          </div>
          <h4 class="tpl-name">{{ t.name }}</h4>
          <p class="tpl-count">包含权益：<strong>{{ t.totalCount }} 次</strong> · 有效期 {{ t.validDays }} 天</p>
          <p class="tpl-desc">{{ t.description || '店铺通用优惠次卡' }}</p>
          <div class="tpl-footer">
            <span class="tpl-active">当前持有中：{{ t.activeCardsCount }} 张</span>
            <button class="btn-del-tpl" @click="deleteTemplate(t.id)">删除</button>
          </div>
        </div>
      </div>
    </div>

    <!-- Customer Cards Table (With Deduct Button) -->
    <div class="table-card">
      <div class="table-header">
        <h3>已售客户次卡列表 ({{ customerCards.length }})</h3>
      </div>

      <div v-if="loading" class="loading-box">加载次卡数据...</div>
      <div v-else-if="customerCards.length === 0" class="empty-box">暂无客户购卡记录</div>
      <div v-else class="table-responsive">
        <table class="data-table">
          <thead>
            <tr>
              <th>客户姓名</th>
              <th>联系电话</th>
              <th>次卡名称</th>
              <th>剩余次数</th>
              <th>到期时间</th>
              <th>开卡时间</th>
              <th>核销操作</th>
            </tr>
          </thead>
          <tbody>
            <tr v-for="cc in customerCards" :key="cc.id">
              <td class="font-bold">{{ cc.customerName }}</td>
              <td>{{ cc.customerPhone }}</td>
              <td>{{ cc.cardName }}</td>
              <td>
                <span :class="['count-badge', cc.remainingCount > 0 ? 'valid' : 'exhausted']">
                  剩余 {{ cc.remainingCount }} 次
                </span>
              </td>
              <td>{{ formatDate(cc.expireAt) }}</td>
              <td class="text-muted">{{ formatDate(cc.createdAt) }}</td>
              <td>
                <button
                  class="btn btn-primary deduct-btn"
                  :disabled="cc.remainingCount <= 0"
                  @click="openDeductModal(cc)"
                >
                  到店核销 -1次
                </button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>
    </div>

    <!-- Create Template Modal -->
    <div v-if="showTemplateModal" class="modal-overlay" @click.self="showTemplateModal = false">
      <div class="modal-content">
        <div class="modal-header">
          <h3>新建次卡套餐模板</h3>
          <button class="close-btn" @click="showTemplateModal = false">✕</button>
        </div>

        <form @submit.prevent="createTemplate" class="form-body">
          <div class="form-item">
            <label>套餐名称 <span class="required">*</span></label>
            <input v-model="formTpl.name" type="text" placeholder="例如：美甲畅享5次卡" class="form-input" required />
          </div>

          <div class="form-row">
            <div class="form-item half">
              <label>售卖价格 (元) <span class="required">*</span></label>
              <input v-model.number="formTpl.price" type="number" min="0" class="form-input" required />
            </div>
            <div class="form-item half">
              <label>包含次数 <span class="required">*</span></label>
              <input v-model.number="formTpl.totalCount" type="number" min="1" class="form-input" required />
            </div>
          </div>

          <div class="form-item">
            <label>有效期 (天数)</label>
            <input v-model.number="formTpl.validDays" type="number" min="1" class="form-input" required />
          </div>

          <div class="form-item">
            <label>使用说明/权益介绍</label>
            <textarea v-model="formTpl.description" rows="2" placeholder="可用于哪些指定项目、注意事项" class="form-textarea"></textarea>
          </div>

          <button type="submit" class="btn btn-primary submit-btn">保存次卡模板</button>
        </form>
      </div>
    </div>

    <!-- Bind Customer Card Modal -->
    <div v-if="showBindModal" class="modal-overlay" @click.self="showBindModal = false">
      <div class="modal-content">
        <div class="modal-header">
          <h3>为客户开通次卡</h3>
          <button class="close-btn" @click="showBindModal = false">✕</button>
        </div>

        <form @submit.prevent="submitBindCard" class="form-body">
          <div class="form-item">
            <label>选择客户 <span class="required">*</span></label>
            <select v-model="bindCustomerId" class="form-input" required>
              <option disabled value="">请选择购卡客户</option>
              <option v-for="c in customers" :key="c.id" :value="c.id">
                {{ c.name }} ({{ c.phone }})
              </option>
            </select>
          </div>

          <div class="form-item">
            <label>选择次卡套餐 <span class="required">*</span></label>
            <select v-model="bindCardId" class="form-input" required>
              <option disabled value="">请选择次卡模板</option>
              <option v-for="t in templates" :key="t.id" :value="t.id">
                {{ t.name }} - ¥{{ t.price }} (共{{ t.totalCount }}次)
              </option>
            </select>
          </div>

          <button type="submit" class="btn btn-primary submit-btn">确认开卡</button>
        </form>
      </div>
    </div>

    <!-- Deduct Modal -->
    <div v-if="deductCardTarget" class="modal-overlay" @click.self="deductCardTarget = null">
      <div class="modal-content">
        <div class="modal-header">
          <h3>次卡核销确认</h3>
          <button class="close-btn" @click="deductCardTarget = null">✕</button>
        </div>

        <div class="deduct-info">
          <p><strong>客户：</strong>{{ deductCardTarget.customerName }}</p>
          <p><strong>次卡：</strong>{{ deductCardTarget.cardName }}</p>
          <p><strong>当前剩余：</strong><span class="remain-txt">{{ deductCardTarget.remainingCount }} 次</span></p>
        </div>

        <div class="form-body">
          <div class="form-item">
            <label>本次核销次数</label>
            <input v-model.number="deductCount" type="number" min="1" :max="deductCardTarget.remainingCount" class="form-input" />
          </div>

          <div class="form-item">
            <label>核销备注</label>
            <input v-model="deductRemark" type="text" placeholder="例如：到店做晕染款美甲1次" class="form-input" />
          </div>

          <button class="btn btn-primary submit-btn" @click="submitDeduct">
            确认扣除次数并记录
          </button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { api } from '@/api/client'
import type { MemberCardDto, CustomerCardDto, CustomerListItemDto } from '@/types'

const loading = ref(true)
const templates = ref<MemberCardDto[]>([])
const customerCards = ref<CustomerCardDto[]>([])
const customers = ref<CustomerListItemDto[]>([])

// Create Template
const showTemplateModal = ref(false)
const formTpl = ref({
  name: '',
  type: 'times',
  price: 399,
  totalCount: 5,
  validDays: 180,
  description: ''
})

// Bind Card
const showBindModal = ref(false)
const bindCustomerId = ref('')
const bindCardId = ref('')

// Deduct
const deductCardTarget = ref<CustomerCardDto | null>(null)
const deductCount = ref(1)
const deductRemark = ref('')

const fetchAll = async () => {
  loading.value = true
  try {
    const [tpls, cards, custs] = await Promise.all([
      api.get('/admin/cards/templates') as Promise<MemberCardDto[]>,
      api.get('/admin/cards/customer-cards') as Promise<CustomerCardDto[]>,
      api.get('/admin/customers') as Promise<CustomerListItemDto[]>
    ])
    templates.value = tpls
    customerCards.value = cards
    customers.value = custs
  } catch (err: any) {
    alert(err.message || '加载次卡数据失败')
  } finally {
    loading.value = false
  }
}

const createTemplate = async () => {
  try {
    await api.post('/admin/cards/templates', formTpl.value)
    showTemplateModal.value = false
    formTpl.value = {
      name: '',
      type: 'times',
      price: 399,
      totalCount: 5,
      validDays: 180,
      description: ''
    }
    await fetchAll()
  } catch (err: any) {
    alert(err.message || '创建卡模板失败')
  }
}

const deleteTemplate = async (id: string) => {
  if (confirm('确认删除该次卡模板吗？')) {
    try {
      await api.delete(`/admin/cards/templates/${id}`)
      await fetchAll()
    } catch (err: any) {
      alert(err.message || '删除失败')
    }
  }
}

const openBindModal = () => {
  bindCustomerId.value = customers.value.length > 0 ? customers.value[0].id : ''
  bindCardId.value = templates.value.length > 0 ? templates.value[0].id : ''
  showBindModal.value = true
}

const submitBindCard = async () => {
  if (!bindCustomerId.value || !bindCardId.value) return
  try {
    await api.post('/admin/cards/bind', {
      customerId: bindCustomerId.value,
      cardId: bindCardId.value
    })
    showBindModal.value = false
    await fetchAll()
    alert('开卡成功！')
  } catch (err: any) {
    alert(err.message || '开卡失败')
  }
}

const openDeductModal = (card: CustomerCardDto) => {
  deductCardTarget.value = card
  deductCount.value = 1
  deductRemark.value = '到店核销服务1次'
}

const submitDeduct = async () => {
  if (!deductCardTarget.value) return
  try {
    await api.post('/admin/cards/deduct', {
      customerCardId: deductCardTarget.value.id,
      deductCount: deductCount.value,
      remark: deductRemark.value
    })
    deductCardTarget.value = null
    await fetchAll()
    alert('次卡核销成功！')
  } catch (err: any) {
    alert(err.message || '核销失败')
  }
}

const formatDate = (str?: string) => {
  if (!str) return '-'
  const d = new Date(str)
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`
}

onMounted(() => {
  fetchAll()
})
</script>

<style scoped>
.cards-view {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.action-card {
  background: #ffffff;
  border-radius: 12px;
  padding: 16px 20px;
  border: 1px solid #e2e8f0;
  display: flex;
  justify-content: space-between;
  align-items: center;
  flex-wrap: wrap;
  gap: 12px;
}

.header-titles h2 {
  font-size: 18px;
  font-weight: 700;
  color: #0f172a;
}

.header-titles p {
  font-size: 13px;
  color: #64748b;
  margin-top: 2px;
}

.btn-group {
  display: flex;
  gap: 8px;
}

/* Templates */
.section-title {
  font-size: 15px;
  font-weight: 600;
  color: #334155;
  margin-bottom: 12px;
}

.templates-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(260px, 1fr));
  gap: 14px;
}

.tpl-card {
  background: linear-gradient(135deg, #fff1f2 0%, #ffffff 100%);
  border: 1px solid #fecdd3;
  border-radius: 12px;
  padding: 16px;
  box-shadow: 0 1px 3px rgba(0,0,0,0.03);
}

.tpl-top {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.tpl-type {
  font-size: 11px;
  background: #ffe4e6;
  color: #be123c;
  padding: 2px 8px;
  border-radius: 4px;
  font-weight: 600;
}

.tpl-price {
  font-size: 18px;
  font-weight: 700;
  color: #e11d48;
}

.tpl-name {
  font-size: 15px;
  font-weight: 600;
  color: #0f172a;
  margin-top: 8px;
}

.tpl-count {
  font-size: 13px;
  color: #475569;
  margin-top: 4px;
}

.tpl-desc {
  font-size: 12px;
  color: #64748b;
  margin-top: 4px;
}

.tpl-footer {
  margin-top: 14px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 12px;
}

.tpl-active {
  color: #059669;
  font-weight: 500;
}

.btn-del-tpl {
  color: #dc2626;
}

/* Table */
.table-card {
  background: #ffffff;
  border-radius: 12px;
  border: 1px solid #e2e8f0;
  overflow: hidden;
}

.table-header {
  padding: 16px 20px;
  border-bottom: 1px solid #f1f5f9;
}

.table-header h3 {
  font-size: 15px;
  font-weight: 600;
  color: #0f172a;
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

.count-badge {
  display: inline-block;
  padding: 3px 8px;
  border-radius: 4px;
  font-size: 12px;
  font-weight: 600;
}

.count-badge.valid { background: #dcfce7; color: #15803d; }
.count-badge.exhausted { background: #f1f5f9; color: #94a3b8; }

.deduct-btn {
  padding: 4px 10px;
  font-size: 12px;
}

.deduct-btn:disabled {
  opacity: 0.5;
  cursor: not-allowed;
}

.deduct-info {
  background: #f8fafc;
  padding: 14px;
  border-radius: 8px;
  margin: 12px 0;
  font-size: 14px;
  display: flex;
  flex-direction: column;
  gap: 6px;
}

.remain-txt {
  color: #e11d48;
  font-weight: 700;
}

/* Modals */
.form-body {
  margin-top: 12px;
}

.form-item {
  margin-bottom: 12px;
}

.form-item label {
  display: block;
  font-size: 13px;
  font-weight: 500;
  color: #334155;
  margin-bottom: 4px;
}

.form-row {
  display: flex;
  gap: 12px;
}

.form-item.half {
  flex: 1;
}

.form-input, .form-textarea {
  width: 100%;
  padding: 8px 10px;
  border: 1px solid #cbd5e1;
  border-radius: 6px;
  font-size: 13px;
}

.submit-btn {
  width: 100%;
  padding: 10px;
  margin-top: 10px;
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

.loading-box, .empty-box {
  padding: 40px;
  text-align: center;
  color: #94a3b8;
}
</style>
