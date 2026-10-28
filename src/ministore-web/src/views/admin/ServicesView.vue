<template>
  <div class="services-view">
    <!-- Top Action Row -->
    <div class="action-card">
      <div class="header-titles">
        <h2>服务项目与价格表</h2>
        <p>上架的服务将直接显示在顾客扫码 H5 页面中</p>
      </div>

      <div class="btn-group">
        <button class="btn btn-secondary" @click="showCategoryModal = true">
          📁 分类管理 ({{ categories.length }})
        </button>
        <button class="btn btn-primary" @click="openAddService">
          + 新增服务项目
        </button>
      </div>
    </div>

    <!-- Category Filter Tabs -->
    <div class="category-filter">
      <button
        :class="['cat-tab', { active: selectedCatId === '' }]"
        @click="selectedCatId = ''"
      >
        全部项目 ({{ items.length }})
      </button>
      <button
        v-for="cat in categories"
        :key="cat.id"
        :class="['cat-tab', { active: selectedCatId === cat.id }]"
        @click="selectedCatId = cat.id"
      >
        {{ cat.name }} ({{ cat.servicesCount }})
      </button>
    </div>

    <!-- Services Grid/List -->
    <div class="services-table-card">
      <div v-if="loading" class="loading-box">加载服务列表中...</div>
      <div v-else-if="filteredItems.length === 0" class="empty-box">暂无服务项目</div>
      <div v-else class="items-grid">
        <div v-for="item in filteredItems" :key="item.id" class="service-item-card">
          <div class="sic-img-wrap">
            <img :src="item.imageUrl || defaultImg" class="sic-img" />
            <span :class="['sic-status', item.enabled ? 'active' : 'disabled']">
              {{ item.enabled ? '已上架' : '已下架' }}
            </span>
          </div>

          <div class="sic-content">
            <div class="sic-header">
              <span class="sic-cat">{{ item.categoryName }}</span>
              <span class="sic-price">¥{{ item.price }}</span>
            </div>
            <h4 class="sic-name">{{ item.name }}</h4>
            <p class="sic-meta">时长：{{ item.durationMinutes }}分钟</p>
            <p class="sic-desc">{{ item.description || '暂无描述' }}</p>

            <div class="sic-actions">
              <button
                :class="['btn-toggle', item.enabled ? 'off' : 'on']"
                @click="toggleItem(item.id)"
              >
                {{ item.enabled ? '下架' : '上架' }}
              </button>
              <button class="btn-act-sub edit" @click="openEditService(item)">编辑</button>
              <button class="btn-act-sub delete" @click="deleteItem(item.id)">删除</button>
            </div>
          </div>
        </div>
      </div>
    </div>

    <!-- Add/Edit Service Modal -->
    <div v-if="showServiceModal" class="modal-overlay" @click.self="showServiceModal = false">
      <div class="modal-content">
        <div class="modal-header">
          <h3>{{ editingItemId ? '编辑服务项目' : '新增服务项目' }}</h3>
          <button class="close-btn" @click="showServiceModal = false">✕</button>
        </div>

        <form @submit.prevent="saveService" class="form-body">
          <div class="form-item">
            <label>所属分类 <span class="required">*</span></label>
            <select v-model="formService.categoryId" class="form-input" required>
              <option disabled value="">请选择所属分类</option>
              <option v-for="c in categories" :key="c.id" :value="c.id">{{ c.name }}</option>
            </select>
          </div>

          <div class="form-item">
            <label>项目名称 <span class="required">*</span></label>
            <input v-model="formService.name" type="text" placeholder="例如：日式手部单色美甲" class="form-input" required />
          </div>

          <div class="form-row">
            <div class="form-item half">
              <label>价格 (元) <span class="required">*</span></label>
              <input v-model.number="formService.price" type="number" min="0" step="0.5" class="form-input" required />
            </div>
            <div class="form-item half">
              <label>服务耗时 (分钟) <span class="required">*</span></label>
              <input v-model.number="formService.durationMinutes" type="number" min="5" step="5" class="form-input" required />
            </div>
          </div>

          <div class="form-item">
            <label>展示图片链接 (URL)</label>
            <input v-model="formService.imageUrl" type="url" placeholder="https://..." class="form-input" />
          </div>

          <div class="form-item">
            <label>项目特色描述</label>
            <textarea v-model="formService.description" rows="2" placeholder="包含哪些步骤、使用什么胶水/材料等" class="form-textarea"></textarea>
          </div>

          <button type="submit" class="btn btn-primary submit-btn">保存服务项目</button>
        </form>
      </div>
    </div>

    <!-- Category Management Modal -->
    <div v-if="showCategoryModal" class="modal-overlay" @click.self="showCategoryModal = false">
      <div class="modal-content">
        <div class="modal-header">
          <h3>分类管理</h3>
          <button class="close-btn" @click="showCategoryModal = false">✕</button>
        </div>

        <div class="add-cat-row">
          <input v-model="newCatName" type="text" placeholder="输入新分类名称 (如：脚部美甲)" class="form-input" />
          <button class="btn btn-primary" @click="addCategory">添加</button>
        </div>

        <div class="cat-list-box">
          <div v-for="cat in categories" :key="cat.id" class="cat-list-row">
            <span class="cat-name">{{ cat.name }} ({{ cat.servicesCount }}项)</span>
            <button class="btn-del-cat" @click="deleteCategory(cat.id)">删除</button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { api } from '@/api/client'
import type { ServiceCategoryDto, ServiceItemDto } from '@/types'

const defaultImg = 'https://images.unsplash.com/photo-1632345031435-8727f6897d53?w=500&auto=format&fit=crop&q=80'

const loading = ref(true)
const categories = ref<ServiceCategoryDto[]>([])
const items = ref<ServiceItemDto[]>([])
const selectedCatId = ref('')

// Add/Edit Service State
const showServiceModal = ref(false)
const editingItemId = ref<string | null>(null)
const formService = ref({
  categoryId: '',
  name: '',
  price: 98,
  durationMinutes: 60,
  imageUrl: '',
  description: '',
  enabled: true,
  sortOrder: 0
})

// Category modal state
const showCategoryModal = ref(false)
const newCatName = ref('')

const filteredItems = computed(() => {
  if (!selectedCatId.value) return items.value
  return items.value.filter((i: ServiceItemDto) => i.categoryId === selectedCatId.value)
})

const fetchAll = async () => {
  loading.value = true
  try {
    const [cats, serviceItems] = await Promise.all([
      api.get('/admin/services/categories') as Promise<ServiceCategoryDto[]>,
      api.get('/admin/services/items') as Promise<ServiceItemDto[]>
    ])
    categories.value = cats
    items.value = serviceItems
  } catch (err: any) {
    alert(err.message || '加载服务失败')
  } finally {
    loading.value = false
  }
}

const toggleItem = async (id: string) => {
  try {
    await api.patch(`/admin/services/items/${id}/toggle`, {})
    await fetchAll()
  } catch (err: any) {
    alert(err.message || '状态切换失败')
  }
}

const openAddService = () => {
  editingItemId.value = null
  formService.value = {
    categoryId: categories.value.length > 0 ? categories.value[0].id : '',
    name: '',
    price: 98,
    durationMinutes: 60,
    imageUrl: '',
    description: '',
    enabled: true,
    sortOrder: 0
  }
  showServiceModal.value = true
}

const openEditService = (item: ServiceItemDto) => {
  editingItemId.value = item.id
  formService.value = {
    categoryId: item.categoryId,
    name: item.name,
    price: item.price,
    durationMinutes: item.durationMinutes,
    imageUrl: item.imageUrl || '',
    description: item.description || '',
    enabled: item.enabled,
    sortOrder: item.sortOrder
  }
  showServiceModal.value = true
}

const saveService = async () => {
  try {
    if (editingItemId.value) {
      await api.put(`/admin/services/items/${editingItemId.value}`, formService.value)
    } else {
      await api.post('/admin/services/items', formService.value)
    }
    showServiceModal.value = false
    await fetchAll()
  } catch (err: any) {
    alert(err.message || '保存失败')
  }
}

const deleteItem = async (id: string) => {
  if (confirm('确认删除该服务项目吗？')) {
    try {
      await api.delete(`/admin/services/items/${id}`)
      await fetchAll()
    } catch (err: any) {
      alert(err.message || '删除失败')
    }
  }
}

const addCategory = async () => {
  if (!newCatName.value.trim()) return
  try {
    await api.post('/admin/services/categories', {
      name: newCatName.value.trim(),
      sortOrder: categories.value.length + 1
    })
    newCatName.value = ''
    await fetchAll()
  } catch (err: any) {
    alert(err.message || '添加分类失败')
  }
}

const deleteCategory = async (id: string) => {
  if (confirm('确认删除该分类吗？')) {
    try {
      await api.delete(`/admin/services/categories/${id}`)
      await fetchAll()
    } catch (err: any) {
      alert(err.message || '删除分类失败')
    }
  }
}

onMounted(() => {
  fetchAll()
})
</script>

<style scoped>
.services-view {
  display: flex;
  flex-direction: column;
  gap: 16px;
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

.category-filter {
  display: flex;
  gap: 8px;
  overflow-x: auto;
}

.cat-tab {
  padding: 8px 16px;
  border-radius: 20px;
  background: #ffffff;
  border: 1px solid #e2e8f0;
  color: #475569;
  font-size: 13px;
  font-weight: 500;
  white-space: nowrap;
}

.cat-tab.active {
  background: var(--primary);
  color: #ffffff;
  border-color: var(--primary);
}

.services-table-card {
  background: #ffffff;
  border-radius: 12px;
  border: 1px solid #e2e8f0;
  padding: 20px;
}

.items-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(280px, 1fr));
  gap: 16px;
}

.service-item-card {
  border: 1px solid #e2e8f0;
  border-radius: 10px;
  overflow: hidden;
  background: #ffffff;
  display: flex;
  flex-direction: column;
  transition: all 0.2s;
}

.service-item-card:hover {
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.05);
}

.sic-img-wrap {
  position: relative;
  width: 100%;
  height: 140px;
}

.sic-img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.sic-status {
  position: absolute;
  top: 8px;
  right: 8px;
  font-size: 11px;
  padding: 2px 8px;
  border-radius: 4px;
  font-weight: 600;
}

.sic-status.active { background: #dcfce7; color: #16a34a; }
.sic-status.disabled { background: #fee2e2; color: #dc2626; }

.sic-content {
  padding: 12px;
  display: flex;
  flex-direction: column;
  flex: 1;
}

.sic-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 4px;
}

.sic-cat {
  font-size: 11px;
  color: #64748b;
  background: #f1f5f9;
  padding: 2px 6px;
  border-radius: 4px;
}

.sic-price {
  font-size: 16px;
  font-weight: 700;
  color: #e11d48;
}

.sic-name {
  font-size: 14px;
  font-weight: 600;
  color: #1e293b;
  margin-top: 4px;
}

.sic-meta {
  font-size: 12px;
  color: #64748b;
  margin-top: 4px;
}

.sic-desc {
  font-size: 12px;
  color: #94a3b8;
  margin-top: 4px;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
  flex: 1;
}

.sic-actions {
  display: flex;
  gap: 6px;
  margin-top: 12px;
  padding-top: 10px;
  border-top: 1px solid #f1f5f9;
}

.btn-toggle {
  padding: 4px 10px;
  border-radius: 4px;
  font-size: 12px;
  font-weight: 500;
}

.btn-toggle.on { background: #dcfce7; color: #16a34a; }
.btn-toggle.off { background: #fef3c7; color: #d97706; }

.btn-act-sub {
  padding: 4px 8px;
  border-radius: 4px;
  font-size: 12px;
}

.btn-act-sub.edit { background: #f1f5f9; color: #334155; }
.btn-act-sub.delete { background: #fee2e2; color: #dc2626; }

/* Modal forms */
.form-row {
  display: flex;
  gap: 12px;
}

.form-item.half {
  flex: 1;
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
  margin-top: 12px;
}

/* Category modal */
.add-cat-row {
  display: flex;
  gap: 8px;
  margin: 16px 0;
}

.cat-list-box {
  display: flex;
  flex-direction: column;
  gap: 8px;
  max-height: 240px;
  overflow-y: auto;
}

.cat-list-row {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 8px 12px;
  background: #f8fafc;
  border-radius: 6px;
  font-size: 13px;
}

.btn-del-cat {
  color: #dc2626;
  font-size: 12px;
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
