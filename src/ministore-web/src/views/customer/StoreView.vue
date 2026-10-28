<template>
  <div class="store-view">
    <!-- Loading State -->
    <div v-if="loading" class="loading-state">
      <div class="spinner"></div>
      <p>正在加载门店信息...</p>
    </div>

    <!-- Error State -->
    <div v-else-if="error" class="error-state">
      <div class="error-card">
        <div class="error-icon">⚠️</div>
        <h3>未能找到门店</h3>
        <p>{{ error }}</p>
        <button class="btn btn-primary" @click="fetchStore">重试</button>
      </div>
    </div>

    <!-- Main Content -->
    <div v-else-if="store" class="store-container">
      <!-- TAB 1: SERVICES VIEW (选项目) -->
      <div v-if="currentTab === 'services'" class="tab-services">
        <!-- Banner & Header -->
        <div class="store-header">
          <div class="banner-wrapper">
            <img :src="store.banner || defaultBanner" alt="Banner" class="store-banner" />
            <div class="banner-overlay"></div>
          </div>

          <div class="header-card">
            <div class="profile-row">
              <img :src="store.logo || defaultLogo" alt="Logo" class="store-logo" />
              <div class="store-titles">
                <h1 class="store-name">{{ store.name }}</h1>
                <div class="store-tags">
                  <span class="tag">预约制工作室</span>
                  <span class="tag">正品保障</span>
                </div>
              </div>
            </div>

            <!-- Announcement -->
            <div v-if="store.announcement" class="announcement-bar">
              <span class="announcement-icon">📢</span>
              <span class="announcement-text">{{ store.announcement }}</span>
            </div>

            <!-- Info Details -->
            <div class="store-meta">
              <div class="meta-item">
                <span class="meta-icon">🕒</span>
                <span class="meta-text">{{ store.hours || '10:00 - 21:00' }}</span>
              </div>
              <div class="meta-item" v-if="store.phone">
                <span class="meta-icon">📞</span>
                <a :href="`tel:${store.phone}`" class="meta-link">{{ store.phone }} (点击拨打)</a>
              </div>
              <div class="meta-item" v-if="store.address">
                <span class="meta-icon">📍</span>
                <span class="meta-text address" @click="copyAddress">{{ store.address }} <small class="copy-hint">(点击复制)</small></span>
              </div>
            </div>
          </div>
        </div>

        <!-- Categories Tabs -->
        <div class="category-tabs-sticky">
          <div class="category-tabs">
            <button
              v-for="cat in store.categories"
              :key="cat.id"
              :class="['tab-btn', { active: activeCategoryId === cat.id }]"
              @click="activeCategoryId = cat.id"
            >
              {{ cat.name }}
              <span class="cat-count">{{ cat.services.length }}</span>
            </button>
          </div>
        </div>

        <!-- Services List -->
        <div class="services-list-container">
          <div v-if="currentCategory && currentCategory.services.length === 0" class="empty-category">
            <p>该分类下暂无服务项目</p>
          </div>

          <div v-else class="services-grid">
            <div
              v-for="service in currentServices"
              :key="service.id"
              class="service-card"
            >
              <div class="service-img-wrapper">
                <img :src="service.imageUrl || defaultServiceImg" :alt="service.name" class="service-img" />
                <span class="duration-badge">{{ service.durationMinutes }}分钟</span>
              </div>
              <div class="service-info">
                <h3 class="service-title">{{ service.name }}</h3>
                <p class="service-desc">{{ service.description || '高品质体验，专业技师贴心服务' }}</p>
                <div class="service-bottom">
                  <div class="price-wrap">
                    <span class="currency">¥</span>
                    <span class="amount">{{ service.price }}</span>
                  </div>
                  <button class="btn btn-primary book-btn" @click="openBookModal(service)">
                    预约
                  </button>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- TAB 2: MY ORDERS & PROFILE (我的中心) -->
      <div v-else-if="currentTab === 'my'" class="tab-my">
        <!-- Not logged in / No phone state -->
        <div v-if="!savedPhone" class="login-prompt-card">
          <div class="lp-icon">👤</div>
          <h3>查看我的预约与会员卡</h3>
          <p>请输入您预约时填写的手机号码进行查询</p>
          <div class="phone-input-wrap">
            <input
              v-model="inputPhone"
              type="tel"
              maxlength="11"
              placeholder="请输入11位手机号码"
              class="form-input"
            />
            <button class="btn btn-primary" @click="handleConfirmPhone">
              确认进入
            </button>
          </div>
        </div>

        <!-- Profile & Orders Content -->
        <div v-else class="my-content">
          <!-- User Profile Card -->
          <div class="my-profile-card">
            <div class="mp-top">
              <div class="mp-avatar">
                {{ (customerProfile?.name || '客')[0] }}
              </div>
              <div class="mp-info">
                <div class="mp-name-row">
                  <h3 class="mp-name">{{ customerProfile?.name || '尊敬的顾客' }}</h3>
                  <button class="btn-edit-profile" @click="openEditProfile">
                    ✏️ 编辑
                  </button>
                </div>
                <p class="mp-phone">
                  📱 {{ savedPhone }}
                  <span class="switch-phone" @click="promptSwitchPhone">(切换)</span>
                </p>
              </div>
            </div>

            <div class="mp-stats-row">
              <div class="mp-stat-item">
                <span class="stat-num">{{ customerProfile?.visitCount || 0 }}</span>
                <span class="stat-txt">累计到店</span>
              </div>
              <div class="mp-stat-item">
                <span class="stat-num">{{ myAppointments.length }}</span>
                <span class="stat-txt">历史预约</span>
              </div>
              <div class="mp-stat-item">
                <span class="stat-num">{{ activeCardsCount }}</span>
                <span class="stat-txt">有效次卡</span>
              </div>
            </div>

            <div v-if="customerProfile?.notes" class="mp-notes-box">
              <span class="notes-tag">个人偏好</span>
              <span class="notes-txt">{{ customerProfile.notes }}</span>
            </div>
          </div>

          <!-- Member Cards (if any) -->
          <div v-if="customerProfile?.cards && customerProfile.cards.length > 0" class="my-cards-section">
            <h4 class="section-title">💳 我的会员次卡</h4>
            <div class="my-cards-list">
              <div v-for="card in customerProfile.cards" :key="card.id" class="my-card-item">
                <div class="mci-header">
                  <span class="mci-name">{{ card.cardName }}</span>
                  <span class="mci-count">剩余 {{ card.remainingCount }} 次</span>
                </div>
                <div class="mci-footer">
                  <span class="mci-expire">有效期至：{{ formatDate(card.expireAt) }}</span>
                  <span class="mci-tip">到店出示手机号核销</span>
                </div>
              </div>
            </div>
          </div>

          <!-- Appointments Orders List -->
          <div class="my-orders-section">
            <div class="orders-header">
              <h4 class="section-title">📅 我的预约订单</h4>
              <div class="sub-filter-tabs">
                <button
                  :class="['sub-tab', { active: orderFilter === 'all' }]"
                  @click="orderFilter = 'all'"
                >
                  全部
                </button>
                <button
                  :class="['sub-tab', { active: orderFilter === 'active' }]"
                  @click="orderFilter = 'active'"
                >
                  待到店
                </button>
                <button
                  :class="['sub-tab', { active: orderFilter === 'history' }]"
                  @click="orderFilter = 'history'"
                >
                  历史
                </button>
              </div>
            </div>

            <div v-if="loadingOrders" class="orders-loading">
              正在加载预约记录...
            </div>

            <div v-else-if="filteredAppointments.length === 0" class="empty-orders">
              <p>暂无相关预约订单</p>
              <button class="btn btn-outline" @click="currentTab = 'services'">
                去挑选服务项目 →
              </button>
            </div>

            <div v-else class="orders-list">
              <div
                v-for="order in filteredAppointments"
                :key="order.id"
                class="order-card"
              >
                <div class="order-card-header">
                  <span class="order-time">{{ formatOrderTime(order.appointmentAt) }}</span>
                  <span :class="['badge', `badge-${order.status}`]">
                    {{ formatStatus(order.status) }}
                  </span>
                </div>

                <div class="order-card-body">
                  <div class="order-srv-name">{{ order.serviceName }}</div>
                  <div class="order-price">¥{{ order.price }}</div>
                </div>

                <div v-if="order.notes" class="order-notes">
                  备注：{{ order.notes }}
                </div>

                <div class="order-card-footer">
                  <span class="order-created">下单时间：{{ formatCreated(order.createdAt) }}</span>
                  <button
                    v-if="order.status === 'pending' || order.status === 'confirmed'"
                    class="btn-cancel-order"
                    @click="cancelOrder(order.id)"
                  >
                    取消预约
                  </button>
                  <span v-else-if="order.status === 'cancelled'" class="order-state-text cancelled">
                    已取消
                  </span>
                  <span v-else-if="order.status === 'completed'" class="order-state-text completed">
                    已服务完成
                  </span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Bottom Navigation Tabbar -->
      <nav class="bottom-tabbar">
        <button
          :class="['tabbar-item', { active: currentTab === 'services' }]"
          @click="currentTab = 'services'"
        >
          <span class="tb-icon">💅</span>
          <span class="tb-label">精选项目</span>
        </button>

        <button
          :class="['tabbar-item', { active: currentTab === 'my' }]"
          @click="switchTabToMy"
        >
          <span class="tb-icon">👤</span>
          <span class="tb-label">我的中心</span>
          <span v-if="pendingOrdersCount > 0" class="tb-badge">{{ pendingOrdersCount }}</span>
        </button>
      </nav>

      <!-- Booking Modal -->
      <div v-if="showBookModal && selectedService" class="modal-overlay" @click.self="showBookModal = false">
        <div class="modal-content">
          <div class="modal-header">
            <h3>在线预约</h3>
            <button class="close-btn" @click="showBookModal = false">✕</button>
          </div>

          <div class="service-summary">
            <img :src="selectedService.imageUrl || defaultServiceImg" class="summary-img" />
            <div class="summary-info">
              <h4>{{ selectedService.name }}</h4>
              <p class="summary-meta">¥{{ selectedService.price }} · {{ selectedService.durationMinutes }}分钟</p>
            </div>
          </div>

          <!-- Form -->
          <div class="form-body">
            <!-- Date Selection -->
            <div class="form-item">
              <label>预约日期</label>
              <div class="date-pills">
                <button
                  v-for="d in dateOptions"
                  :key="d.dateStr"
                  :class="['date-pill', { active: selectedDate === d.dateStr }]"
                  @click="selectedDate = d.dateStr"
                >
                  <span class="pill-day">{{ d.label }}</span>
                  <span class="pill-date">{{ d.displayDate }}</span>
                </button>
              </div>
            </div>

            <!-- Time Slots -->
            <div class="form-item">
              <label>预约时段</label>
              <div class="time-slots-grid">
                <button
                  v-for="slot in timeSlots"
                  :key="slot"
                  :class="['slot-btn', { active: selectedTime === slot }]"
                  @click="selectedTime = slot"
                >
                  {{ slot }}
                </button>
              </div>
            </div>

            <!-- Customer Details -->
            <div class="form-item">
              <label>顾客姓名 <span class="required">*</span></label>
              <input
                v-model="customerName"
                type="text"
                placeholder="请输入您的称呼（如：林小姐）"
                class="form-input"
              />
            </div>

            <div class="form-item">
              <label>联系手机 <span class="required">*</span></label>
              <input
                v-model="customerPhone"
                type="tel"
                maxlength="11"
                placeholder="便于接收预约确认通知"
                class="form-input"
              />
            </div>

            <div class="form-item">
              <label>特殊备注 (选填)</label>
              <textarea
                v-model="customerNotes"
                rows="2"
                placeholder="例如：需卸甲、偏好款式等需求"
                class="form-textarea"
              ></textarea>
            </div>

            <div v-if="submitError" class="form-error">
              {{ submitError }}
            </div>

            <button
              class="btn btn-primary submit-btn"
              :disabled="submitting"
              @click="submitBooking"
            >
              {{ submitting ? '提交中...' : '确认预约' }}
            </button>
          </div>
        </div>
      </div>

      <!-- Booking Result Drawer/Modal -->
      <div v-if="bookingResult" class="modal-overlay" @click.self="bookingResult = null">
        <div class="modal-content result-modal">
          <div class="success-icon">✓</div>
          <h2>预约已提交成功！</h2>
          <p class="result-msg">{{ bookingResult.message }}</p>

          <div class="ticket-card">
            <div class="ticket-row">
              <span class="ticket-label">门店</span>
              <span class="ticket-val">{{ bookingResult.storeName }}</span>
            </div>
            <div class="ticket-row">
              <span class="ticket-label">服务</span>
              <span class="ticket-val">{{ bookingResult.serviceName }}</span>
            </div>
            <div class="ticket-row">
              <span class="ticket-label">时间</span>
              <span class="ticket-val highlight">{{ formatBookingTime(bookingResult.appointmentAt) }}</span>
            </div>
            <div class="ticket-row">
              <span class="ticket-label">顾客</span>
              <span class="ticket-val">{{ bookingResult.customerName }} ({{ bookingResult.customerPhone }})</span>
            </div>
            <div class="ticket-row">
              <span class="ticket-label">参考金额</span>
              <span class="ticket-val price">¥{{ bookingResult.price }}</span>
            </div>
          </div>

          <div class="result-actions">
            <button class="btn btn-primary full-btn" @click="goToMyOrders">
              查看我的预约单 →
            </button>
            <button class="btn btn-outline full-btn" @click="bookingResult = null">
              继续逛逛
            </button>
          </div>
        </div>
      </div>

      <!-- Edit Profile Modal -->
      <div v-if="showEditProfileModal" class="modal-overlay" @click.self="showEditProfileModal = false">
        <div class="modal-content">
          <div class="modal-header">
            <h3>编辑个人偏好资料</h3>
            <button class="close-btn" @click="showEditProfileModal = false">✕</button>
          </div>
          <div class="form-body">
            <div class="form-item">
              <label>您的称呼</label>
              <input v-model="editName" type="text" class="form-input" placeholder="例如：林小雨" />
            </div>
            <div class="form-item">
              <label>美甲/护肤偏好备注</label>
              <textarea
                v-model="editNotes"
                rows="3"
                class="form-textarea"
                placeholder="例如：指甲偏软易断、偏好裸色猫眼、对酒精轻微过敏"
              ></textarea>
            </div>
            <button class="btn btn-primary submit-btn" @click="saveProfile">
              保存资料
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRoute } from 'vue-router'
import { api } from '@/api/client'
import type {
  StorePublicDto,
  ServicePublicDto,
  CategoryPublicDto,
  AppointmentResultDto,
  CustomerProfileDto,
  CustomerAppointmentDto
} from '@/types'

const route = useRoute()
const slug = computed(() => (route.params.slug as string) || 'muse-nail')

const loading = ref(true)
const error = ref('')
const store = ref<StorePublicDto | null>(null)
const activeCategoryId = ref<string>('')

// Current Tab: 'services' | 'my'
const currentTab = ref<'services' | 'my'>('services')

const defaultBanner = 'https://images.unsplash.com/photo-1519014816548-bf5fe059798b?w=1200&auto=format&fit=crop&q=80'
const defaultLogo = 'https://images.unsplash.com/photo-1604654894610-df63bc536371?w=200&auto=format&fit=crop&q=80'
const defaultServiceImg = 'https://images.unsplash.com/photo-1632345031435-8727f6897d53?w=500&auto=format&fit=crop&q=80'

// Booking state
const showBookModal = ref(false)
const selectedService = ref<ServicePublicDto | null>(null)
const selectedDate = ref('')
const selectedTime = ref('14:00')
const customerName = ref(localStorage.getItem('ministore_cust_name') || '')
const customerPhone = ref(localStorage.getItem('ministore_cust_phone') || '')
const customerNotes = ref('')
const submitting = ref(false)
const submitError = ref('')
const bookingResult = ref<AppointmentResultDto | null>(null)

// Customer Profile & Orders state
const savedPhone = ref(localStorage.getItem('ministore_cust_phone') || '')
const inputPhone = ref('')
const customerProfile = ref<CustomerProfileDto | null>(null)
const myAppointments = ref<CustomerAppointmentDto[]>([])
const loadingOrders = ref(false)
const orderFilter = ref<'all' | 'active' | 'history'>('all')

// Edit profile
const showEditProfileModal = ref(false)
const editName = ref('')
const editNotes = ref('')

const activeCardsCount = computed(() => {
  return customerProfile.value?.cards?.filter(c => c.remainingCount > 0).length || 0
})

const pendingOrdersCount = computed(() => {
  return myAppointments.value.filter(a => a.status === 'pending' || a.status === 'confirmed').length
})

const filteredAppointments = computed(() => {
  if (orderFilter.value === 'active') {
    return myAppointments.value.filter(a => a.status === 'pending' || a.status === 'confirmed')
  } else if (orderFilter.value === 'history') {
    return myAppointments.value.filter(a => a.status === 'completed' || a.status === 'cancelled')
  }
  return myAppointments.value
})

// Date Options (Today, Tomorrow, After Tomorrow)
const dateOptions = computed(() => {
  const options = []
  const today = new Date()
  const daysOfWeek = ['周日', '周一', '周二', '周三', '周四', '周五', '周六']

  for (let i = 0; i < 4; i++) {
    const d = new Date()
    d.setDate(today.getDate() + i)
    const yyyy = d.getFullYear()
    const mm = String(d.getMonth() + 1).padStart(2, '0')
    const dd = String(d.getDate()).padStart(2, '0')
    const dateStr = `${yyyy}-${mm}-${dd}`
    
    let label = `${mm}/${dd}`
    if (i === 0) label = '今天'
    else if (i === 1) label = '明天'
    else if (i === 2) label = '后天'
    else label = daysOfWeek[d.getDay()]

    options.push({
      dateStr,
      label,
      displayDate: `${mm}.${dd} ${daysOfWeek[d.getDay()]}`
    })
  }
  return options
})

const timeSlots = [
  '10:30', '11:30', '13:00', '14:00', '15:30', '16:30', '17:30', '19:00', '20:00'
]

const currentCategory = computed<CategoryPublicDto | undefined>(() => {
  if (!store.value) return undefined
  return store.value.categories.find((c: CategoryPublicDto) => c.id === activeCategoryId.value)
})

const currentServices = computed<ServicePublicDto[]>(() => {
  return currentCategory.value?.services || []
})

const fetchStore = async () => {
  loading.value = true
  error.value = ''
  try {
    const data = await api.get(`/s/${slug.value}`) as StorePublicDto
    store.value = data
    if (data.categories.length > 0) {
      activeCategoryId.value = data.categories[0].id
    }
  } catch (err: any) {
    error.value = err.message || '加载门店失败'
  } finally {
    loading.value = false
  }
}

const copyAddress = () => {
  if (store.value?.address) {
    navigator.clipboard.writeText(store.value.address)
    alert('已复制门店地址到剪贴板！')
  }
}

const openBookModal = (service: ServicePublicDto) => {
  selectedService.value = service
  if (!selectedDate.value && dateOptions.value.length > 0) {
    selectedDate.value = dateOptions.value[0].dateStr
  }
  submitError.value = ''
  showBookModal.value = true
}

const submitBooking = async () => {
  submitError.value = ''
  if (!customerName.value.trim()) {
    submitError.value = '请填写您的姓名'
    return
  }
  if (!customerPhone.value.trim() || customerPhone.value.trim().length < 8) {
    submitError.value = '请填写正确的手机号码'
    return
  }
  if (!selectedDate.value || !selectedTime.value) {
    submitError.value = '请选择预约日期与时间'
    return
  }

  submitting.value = true
  try {
    const appointmentAt = `${selectedDate.value}T${selectedTime.value}:00`
    const res = await api.post(`/s/${slug.value}/book`, {
      slug: slug.value,
      serviceId: selectedService.value!.id,
      appointmentAt,
      customerName: customerName.value.trim(),
      customerPhone: customerPhone.value.trim(),
      notes: customerNotes.value.trim()
    }) as AppointmentResultDto

    // Save phone and name to localStorage for seamless customer center experience!
    savedPhone.value = customerPhone.value.trim()
    localStorage.setItem('ministore_cust_phone', savedPhone.value)
    localStorage.setItem('ministore_cust_name', customerName.value.trim())

    bookingResult.value = res
    showBookModal.value = false
    await fetchCustomerData()
  } catch (err: any) {
    submitError.value = err.message || '预约提交失败，请重试'
  } finally {
    submitting.value = false
  }
}

const goToMyOrders = () => {
  bookingResult.value = null
  currentTab.value = 'my'
}

const switchTabToMy = () => {
  currentTab.value = 'my'
  if (savedPhone.value) {
    fetchCustomerData()
  }
}

const handleConfirmPhone = () => {
  if (!inputPhone.value.trim() || inputPhone.value.trim().length < 8) {
    alert('请输入正确的手机号码')
    return
  }
  savedPhone.value = inputPhone.value.trim()
  localStorage.setItem('ministore_cust_phone', savedPhone.value)
  customerPhone.value = savedPhone.value
  fetchCustomerData()
}

const promptSwitchPhone = () => {
  const p = prompt('请输入要切换查询的手机号码：', savedPhone.value)
  if (p && p.trim()) {
    savedPhone.value = p.trim()
    localStorage.setItem('ministore_cust_phone', savedPhone.value)
    customerPhone.value = savedPhone.value
    fetchCustomerData()
  }
}

const fetchCustomerData = async () => {
  if (!savedPhone.value) return
  loadingOrders.value = true
  try {
    const [profile, appts] = await Promise.all([
      api.get(`/s/${slug.value}/customer/profile?phone=${savedPhone.value}`) as Promise<CustomerProfileDto>,
      api.get(`/s/${slug.value}/customer/appointments?phone=${savedPhone.value}`) as Promise<CustomerAppointmentDto[]>
    ])
    customerProfile.value = profile
    myAppointments.value = appts
    if (profile?.name && !customerName.value) {
      customerName.value = profile.name
    }
  } catch (err) {
    console.error('Fetch customer data failed', err)
  } finally {
    loadingOrders.value = false
  }
}

const openEditProfile = () => {
  editName.value = customerProfile.value?.name || ''
  editNotes.value = customerProfile.value?.notes || ''
  showEditProfileModal.value = true
}

const saveProfile = async () => {
  try {
    await api.put(`/s/${slug.value}/customer/profile?phone=${savedPhone.value}`, {
      name: editName.value.trim(),
      notes: editNotes.value.trim()
    })
    showEditProfileModal.value = false
    customerName.value = editName.value.trim()
    localStorage.setItem('ministore_cust_name', editName.value.trim())
    await fetchCustomerData()
    alert('资料修改成功！')
  } catch (err: any) {
    alert(err.message || '修改失败')
  }
}

const cancelOrder = async (orderId: string) => {
  if (!confirm('确认取消本次预约吗？')) return
  try {
    await api.put(`/s/${slug.value}/customer/appointments/${orderId}/cancel?phone=${savedPhone.value}`, {})
    alert('预约已取消')
    await fetchCustomerData()
  } catch (err: any) {
    alert(err.message || '取消预约失败')
  }
}

const formatBookingTime = (timeStr: string) => {
  if (!timeStr) return ''
  const d = new Date(timeStr)
  return `${d.getMonth() + 1}月${d.getDate()}日 ${String(d.getHours()).padStart(2, '0')}:${String(d.getMinutes()).padStart(2, '0')}`
}

const formatOrderTime = (timeStr: string) => {
  if (!timeStr) return ''
  const d = new Date(timeStr)
  return `${d.getMonth() + 1}月${d.getDate()}日 ${String(d.getHours()).padStart(2, '0')}:${String(d.getMinutes()).padStart(2, '0')}`
}

const formatCreated = (timeStr: string) => {
  if (!timeStr) return ''
  const d = new Date(timeStr)
  return `${d.getMonth() + 1}/${d.getDate()} ${String(d.getHours()).padStart(2, '0')}:${String(d.getMinutes()).padStart(2, '0')}`
}

const formatDate = (timeStr?: string) => {
  if (!timeStr) return '-'
  const d = new Date(timeStr)
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`
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
  fetchStore()
  if (savedPhone.value) {
    fetchCustomerData()
  }
})
</script>

<style scoped>
.store-view {
  min-height: 100vh;
  background-color: #f8fafc;
  padding-bottom: 70px; /* Space for bottom tabbar */
}

.loading-state, .error-state {
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  min-height: 60vh;
  gap: 16px;
  color: var(--text-muted);
}

.spinner {
  width: 36px;
  height: 36px;
  border: 3px solid #e2e8f0;
  border-top-color: var(--primary);
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}

@keyframes spin {
  to { transform: rotate(360deg); }
}

.store-container {
  max-width: 540px;
  margin: 0 auto;
  background-color: #ffffff;
  min-height: 100vh;
  box-shadow: 0 0 20px rgba(0, 0, 0, 0.05);
}

/* Header */
.store-header {
  position: relative;
}

.banner-wrapper {
  position: relative;
  width: 100%;
  height: 180px;
  overflow: hidden;
}

.store-banner {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.banner-overlay {
  position: absolute;
  inset: 0;
  background: linear-gradient(to bottom, rgba(0,0,0,0.1), rgba(0,0,0,0.6));
}

.header-card {
  position: relative;
  margin-top: -40px;
  background: #ffffff;
  border-radius: 20px 20px 0 0;
  padding: 18px 16px 12px;
  box-shadow: 0 -4px 12px rgba(0, 0, 0, 0.04);
}

.profile-row {
  display: flex;
  align-items: center;
  gap: 12px;
}

.store-logo {
  width: 64px;
  height: 64px;
  border-radius: 14px;
  border: 3px solid #ffffff;
  box-shadow: var(--shadow);
  object-fit: cover;
}

.store-titles {
  flex: 1;
}

.store-name {
  font-size: 17px;
  font-weight: 700;
  color: #0f172a;
  line-height: 1.3;
}

.store-tags {
  display: flex;
  gap: 6px;
  margin-top: 4px;
}

.tag {
  font-size: 11px;
  padding: 1px 6px;
  background-color: #fee2e2;
  color: #e11d48;
  border-radius: 4px;
  font-weight: 500;
}

.announcement-bar {
  display: flex;
  align-items: center;
  gap: 6px;
  margin-top: 12px;
  padding: 8px 12px;
  background-color: #fff1f2;
  border-radius: 8px;
  font-size: 13px;
  color: #9f1239;
}

.announcement-text {
  flex: 1;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

.store-meta {
  margin-top: 12px;
  display: flex;
  flex-direction: column;
  gap: 6px;
  font-size: 13px;
  color: #475569;
}

.meta-item {
  display: flex;
  align-items: center;
  gap: 6px;
}

.meta-link {
  color: var(--primary);
  font-weight: 500;
}

.meta-text.address {
  cursor: pointer;
}

.copy-hint {
  color: var(--primary);
  margin-left: 4px;
}

/* Category Tabs */
.category-tabs-sticky {
  position: sticky;
  top: 0;
  z-index: 100;
  background: #ffffff;
  border-bottom: 1px solid #f1f5f9;
  padding: 8px 12px;
}

.category-tabs {
  display: flex;
  gap: 8px;
  overflow-x: auto;
  scrollbar-width: none;
}

.category-tabs::-webkit-scrollbar {
  display: none;
}

.tab-btn {
  white-space: nowrap;
  padding: 8px 14px;
  border-radius: 20px;
  background-color: #f1f5f9;
  color: #475569;
  font-size: 14px;
  font-weight: 500;
  display: flex;
  align-items: center;
  gap: 6px;
  transition: all 0.2s;
}

.tab-btn.active {
  background-color: var(--primary);
  color: #ffffff;
}

.cat-count {
  font-size: 11px;
  opacity: 0.8;
}

/* Services */
.services-list-container {
  padding: 16px;
}

.services-grid {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

.service-card {
  display: flex;
  background: #ffffff;
  border: 1px solid #f1f5f9;
  border-radius: 12px;
  overflow: hidden;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.05);
}

.service-img-wrapper {
  position: relative;
  width: 110px;
  height: 110px;
  flex-shrink: 0;
}

.service-img {
  width: 100%;
  height: 100%;
  object-fit: cover;
}

.duration-badge {
  position: absolute;
  bottom: 4px;
  left: 4px;
  background: rgba(0, 0, 0, 0.6);
  color: #ffffff;
  font-size: 10px;
  padding: 2px 6px;
  border-radius: 4px;
}

.service-info {
  flex: 1;
  padding: 10px 12px;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
}

.service-title {
  font-size: 15px;
  font-weight: 600;
  color: #1e293b;
  line-height: 1.3;
}

.service-desc {
  font-size: 12px;
  color: #64748b;
  margin-top: 4px;
  display: -webkit-box;
  -webkit-line-clamp: 2;
  -webkit-box-orient: vertical;
  overflow: hidden;
}

.service-bottom {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-top: 8px;
}

.price-wrap {
  color: #e11d48;
  font-weight: 700;
}

.currency {
  font-size: 13px;
}

.amount {
  font-size: 18px;
}

.book-btn {
  padding: 6px 14px;
  font-size: 13px;
  border-radius: 6px;
}

/* Bottom Tabbar */
.bottom-tabbar {
  position: fixed;
  bottom: 0;
  left: 50%;
  transform: translateX(-50%);
  width: 100%;
  max-width: 540px;
  height: 56px;
  background: #ffffff;
  border-top: 1px solid #e2e8f0;
  display: flex;
  z-index: 999;
  box-shadow: 0 -2px 10px rgba(0, 0, 0, 0.05);
}

.tabbar-item {
  flex: 1;
  display: flex;
  flex-direction: column;
  align-items: center;
  justify-content: center;
  position: relative;
  color: #64748b;
  gap: 2px;
}

.tabbar-item.active {
  color: var(--primary);
}

.tb-icon {
  font-size: 18px;
}

.tb-label {
  font-size: 11px;
  font-weight: 600;
}

.tb-badge {
  position: absolute;
  top: 6px;
  right: calc(50% - 20px);
  background: #e11d48;
  color: #ffffff;
  font-size: 10px;
  padding: 1px 5px;
  border-radius: 10px;
  font-weight: bold;
}

/* MY PROFILE TAB */
.tab-my {
  padding: 16px;
  min-height: calc(100vh - 70px);
  background-color: #f8fafc;
}

.login-prompt-card {
  background: #ffffff;
  border-radius: 16px;
  padding: 32px 20px;
  text-align: center;
  margin-top: 40px;
  border: 1px solid #e2e8f0;
}

.lp-icon {
  font-size: 40px;
  margin-bottom: 12px;
}

.login-prompt-card h3 {
  font-size: 17px;
  color: #0f172a;
}

.login-prompt-card p {
  font-size: 13px;
  color: #64748b;
  margin: 6px 0 20px;
}

.phone-input-wrap {
  display: flex;
  flex-direction: column;
  gap: 10px;
  max-width: 280px;
  margin: 0 auto;
}

.my-content {
  display: flex;
  flex-direction: column;
  gap: 16px;
}

/* Profile Card */
.my-profile-card {
  background: linear-gradient(135deg, #fff1f2 0%, #ffffff 100%);
  border-radius: 16px;
  padding: 20px 16px;
  border: 1px solid #fecdd3;
  box-shadow: 0 2px 6px rgba(244, 63, 94, 0.06);
}

.mp-top {
  display: flex;
  align-items: center;
  gap: 14px;
}

.mp-avatar {
  width: 52px;
  height: 52px;
  border-radius: 50%;
  background: var(--primary);
  color: #ffffff;
  font-size: 20px;
  font-weight: 700;
  display: flex;
  align-items: center;
  justify-content: center;
  box-shadow: 0 4px 6px -1px rgba(244, 63, 94, 0.3);
}

.mp-info {
  flex: 1;
}

.mp-name-row {
  display: flex;
  align-items: center;
  gap: 8px;
}

.mp-name {
  font-size: 16px;
  font-weight: 700;
  color: #0f172a;
}

.btn-edit-profile {
  font-size: 12px;
  color: var(--primary);
  background: #ffe4e6;
  padding: 2px 8px;
  border-radius: 12px;
  font-weight: 500;
}

.mp-phone {
  font-size: 13px;
  color: #64748b;
  margin-top: 4px;
}

.switch-phone {
  color: #0284c7;
  cursor: pointer;
  margin-left: 6px;
}

.mp-stats-row {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 8px;
  margin-top: 16px;
  padding-top: 14px;
  border-top: 1px dashed #fecdd3;
  text-align: center;
}

.mp-stat-item {
  display: flex;
  flex-direction: column;
}

.stat-num {
  font-size: 18px;
  font-weight: 700;
  color: #be123c;
}

.stat-txt {
  font-size: 11px;
  color: #64748b;
  margin-top: 2px;
}

.mp-notes-box {
  margin-top: 12px;
  background: #ffffff;
  padding: 8px 12px;
  border-radius: 8px;
  font-size: 12px;
  display: flex;
  align-items: center;
  gap: 6px;
  border: 1px solid #fee2e2;
}

.notes-tag {
  background: #ffe4e6;
  color: #be123c;
  padding: 1px 6px;
  border-radius: 4px;
  font-size: 10px;
  font-weight: 600;
}

.notes-txt {
  color: #475569;
  flex: 1;
  white-space: nowrap;
  overflow: hidden;
  text-overflow: ellipsis;
}

/* My Cards */
.my-cards-section h4 {
  font-size: 15px;
  font-weight: 600;
  color: #1e293b;
  margin-bottom: 8px;
}

.my-cards-list {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.my-card-item {
  background: linear-gradient(135deg, #1e293b, #334155);
  color: #ffffff;
  padding: 14px 16px;
  border-radius: 12px;
  box-shadow: 0 4px 6px -1px rgba(0, 0, 0, 0.1);
}

.mci-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.mci-name {
  font-size: 15px;
  font-weight: 600;
}

.mci-count {
  font-size: 14px;
  font-weight: 700;
  color: #facc15;
}

.mci-footer {
  margin-top: 10px;
  display: flex;
  justify-content: space-between;
  align-items: center;
  font-size: 11px;
  opacity: 0.8;
}

/* My Orders */
.orders-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 12px;
}

.section-title {
  font-size: 15px;
  font-weight: 600;
  color: #1e293b;
}

.sub-filter-tabs {
  display: flex;
  gap: 4px;
}

.sub-tab {
  padding: 4px 10px;
  border-radius: 12px;
  background: #e2e8f0;
  color: #475569;
  font-size: 12px;
  font-weight: 500;
}

.sub-tab.active {
  background: var(--primary);
  color: #ffffff;
}

.orders-loading, .empty-orders {
  background: #ffffff;
  border-radius: 12px;
  padding: 32px;
  text-align: center;
  color: #94a3b8;
  border: 1px solid #e2e8f0;
}

.empty-orders p {
  font-size: 13px;
  margin-bottom: 12px;
}

.orders-list {
  display: flex;
  flex-direction: column;
  gap: 12px;
}

.order-card {
  background: #ffffff;
  border-radius: 12px;
  padding: 14px 16px;
  border: 1px solid #e2e8f0;
  box-shadow: 0 1px 3px rgba(0, 0, 0, 0.04);
}

.order-card-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
  border-bottom: 1px solid #f1f5f9;
  padding-bottom: 8px;
}

.order-time {
  font-size: 14px;
  font-weight: 700;
  color: #0f172a;
}

.order-card-body {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 10px 0;
}

.order-srv-name {
  font-size: 14px;
  font-weight: 600;
  color: #334155;
}

.order-price {
  font-size: 15px;
  font-weight: 700;
  color: #e11d48;
}

.order-notes {
  font-size: 12px;
  color: #64748b;
  background: #f8fafc;
  padding: 6px 10px;
  border-radius: 6px;
  margin-bottom: 8px;
}

.order-card-footer {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding-top: 8px;
  border-top: 1px dashed #f1f5f9;
}

.order-created {
  font-size: 11px;
  color: #94a3b8;
}

.btn-cancel-order {
  padding: 4px 10px;
  background: #fee2e2;
  color: #dc2626;
  border-radius: 4px;
  font-size: 12px;
  font-weight: 500;
}

.order-state-text {
  font-size: 12px;
  font-weight: 500;
}

.order-state-text.cancelled { color: #94a3b8; }
.order-state-text.completed { color: #16a34a; }

/* Modals */
.modal-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  margin-bottom: 16px;
}

.close-btn {
  font-size: 18px;
  color: #94a3b8;
}

.service-summary {
  display: flex;
  gap: 12px;
  padding: 12px;
  background-color: #f8fafc;
  border-radius: 8px;
  margin-bottom: 16px;
}

.summary-img {
  width: 50px;
  height: 50px;
  border-radius: 6px;
  object-fit: cover;
}

.summary-info h4 {
  font-size: 14px;
  color: #0f172a;
}

.summary-meta {
  font-size: 13px;
  color: #e11d48;
  font-weight: 600;
  margin-top: 4px;
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

.date-pills {
  display: grid;
  grid-template-columns: repeat(4, 1fr);
  gap: 6px;
}

.date-pill {
  padding: 8px 4px;
  border: 1px solid #e2e8f0;
  border-radius: 8px;
  text-align: center;
  background: #ffffff;
  display: flex;
  flex-direction: column;
  gap: 2px;
}

.date-pill.active {
  border-color: var(--primary);
  background-color: #fff1f2;
}

.pill-day {
  font-size: 13px;
  font-weight: 600;
  color: #0f172a;
}

.pill-date {
  font-size: 10px;
  color: #64748b;
}

.time-slots-grid {
  display: grid;
  grid-template-columns: repeat(3, 1fr);
  gap: 6px;
}

.slot-btn {
  padding: 8px;
  border: 1px solid #e2e8f0;
  border-radius: 6px;
  background: #ffffff;
  font-size: 13px;
  font-weight: 500;
  color: #334155;
}

.slot-btn.active {
  border-color: var(--primary);
  background-color: var(--primary);
  color: #ffffff;
}

.form-input, .form-textarea {
  width: 100%;
  padding: 10px 12px;
  border: 1px solid #cbd5e1;
  border-radius: 8px;
  font-size: 14px;
  outline: none;
}

.form-input:focus, .form-textarea:focus {
  border-color: var(--primary);
}

.form-error {
  color: #e11d48;
  font-size: 13px;
  margin-bottom: 12px;
}

.submit-btn {
  width: 100%;
  padding: 12px;
  font-size: 15px;
  border-radius: 8px;
  margin-top: 8px;
}

/* Result Modal */
.result-modal {
  text-align: center;
}

.success-icon {
  width: 60px;
  height: 60px;
  border-radius: 50%;
  background: #dcfce7;
  color: #16a34a;
  font-size: 32px;
  display: flex;
  align-items: center;
  justify-content: center;
  margin: 0 auto 16px;
}

.result-msg {
  color: #64748b;
  font-size: 14px;
  margin-top: 6px;
}

.ticket-card {
  margin: 20px 0;
  background: #f8fafc;
  border-radius: 12px;
  padding: 16px;
  text-align: left;
  border: 1px dashed #cbd5e1;
}

.ticket-row {
  display: flex;
  justify-content: space-between;
  margin-bottom: 10px;
  font-size: 14px;
}

.ticket-row:last-child {
  margin-bottom: 0;
}

.ticket-label {
  color: #64748b;
}

.ticket-val {
  font-weight: 500;
  color: #0f172a;
}

.ticket-val.highlight {
  color: var(--primary);
  font-weight: 700;
}

.ticket-val.price {
  color: #e11d48;
  font-weight: 700;
}

.result-actions {
  display: flex;
  flex-direction: column;
  gap: 8px;
}

.full-btn {
  width: 100%;
  padding: 12px;
}
</style>
