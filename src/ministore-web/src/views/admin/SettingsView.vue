<template>
  <div class="settings-view">
    <div class="settings-grid">
      <!-- Store Profile Form -->
      <div class="settings-card">
        <div class="card-header">
          <h3>门店基础资料设置</h3>
          <p>修改后，顾客扫码打开的 H5 页面将即时同步更新</p>
        </div>

        <form @submit.prevent="saveSettings" class="settings-form">
          <div class="form-item">
            <label>门店全称 <span class="required">*</span></label>
            <input v-model="form.name" type="text" class="form-input" required />
          </div>

          <div class="form-item">
            <label>唯一标识 Slug (顾客访问网址后缀)</label>
            <input :value="form.slug" type="text" class="form-input" disabled />
            <small class="hint">当前公开链接：{{ publicStoreUrl }}</small>
          </div>

          <div class="form-row">
            <div class="form-item half">
              <label>联系电话</label>
              <input v-model="form.phone" type="text" class="form-input" />
            </div>
            <div class="form-item half">
              <label>营业时间</label>
              <input v-model="form.hours" type="text" placeholder="10:00 - 21:30" class="form-input" />
            </div>
          </div>

          <div class="form-item">
            <label>门店详细地址</label>
            <input v-model="form.address" type="text" class="form-input" />
          </div>

          <div class="form-item">
            <label>顶部滚动公告</label>
            <textarea v-model="form.announcement" rows="2" class="form-textarea" placeholder="例如：新客到店享8.8折优惠，提前预约无需排队！"></textarea>
          </div>

          <div class="form-item">
            <label>店标 Logo 图片链接</label>
            <div class="upload-row">
              <input v-model="form.logo" type="text" class="form-input" />
              <label class="btn btn-secondary upload-btn">
                本地上传
                <input type="file" accept="image/*" class="hidden-file" @change="uploadImage($event, 'logo')" />
              </label>
            </div>
          </div>

          <div class="form-item">
            <label>顶部 Banner 封面图链接</label>
            <div class="upload-row">
              <input v-model="form.banner" type="text" class="form-input" />
              <label class="btn btn-secondary upload-btn">
                本地上传
                <input type="file" accept="image/*" class="hidden-file" @change="uploadImage($event, 'banner')" />
              </label>
            </div>
          </div>

          <button type="submit" class="btn btn-primary save-btn" :disabled="saving">
            {{ saving ? '保存中...' : '保存所有设置' }}
          </button>
        </form>
      </div>

      <!-- QR Code & Printable Stand generator -->
      <div class="qr-stand-card">
        <div class="card-header">
          <h3>门店二维码与立牌物料</h3>
          <p>打印后放置于收银台或门口，顾客扫码即可查看价目与预约</p>
        </div>

        <!-- Printable Stand Preview -->
        <div class="stand-preview-box" id="standBadge">
          <div class="stand-banner">
            <span class="stand-sub">扫码在线预约</span>
            <h2 class="stand-store-name">{{ form.name || 'MUSE NAIL' }}</h2>
          </div>

          <div class="qr-image-wrapper">
            <img v-if="qrCodeDataUrl" :src="qrCodeDataUrl" alt="Store QR Code" class="qr-img" />
            <div v-else class="qr-placeholder">正在生成二维码...</div>
          </div>

          <div class="stand-footer">
            <p class="scan-tip">📷 微信 / 手机相机 扫一扫</p>
            <p class="perks">提前预约 · 专属服务 · 免排队</p>
            <p class="stand-meta" v-if="form.address">地址：{{ form.address }}</p>
            <p class="stand-meta" v-if="form.phone">电话：{{ form.phone }}</p>
          </div>
        </div>

        <!-- Stand Actions -->
        <div class="stand-actions">
          <button class="btn btn-primary full-btn" @click="downloadQrCode">
            💾 下载二维码图片 (PNG)
          </button>
          <a :href="publicStoreUrl" target="_blank" class="btn btn-outline full-btn">
            🌐 直接打开顾客端 H5 链接
          </a>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, watch, onMounted } from 'vue'
import QRCode from 'qrcode'
import { api } from '@/api/client'
import type { StoreSettingsDto } from '@/types'

const saving = ref(false)
const form = ref<StoreSettingsDto>({
  id: '',
  name: '',
  slug: 'muse-nail',
  logo: '',
  banner: '',
  phone: '',
  address: '',
  hours: '10:00 - 21:30',
  announcement: ''
})

const qrCodeDataUrl = ref('')

const publicStoreUrl = computed(() => {
  const origin = window.location.origin
  return `${origin}/s/${form.value.slug}`
})

const generateQRCode = async () => {
  try {
    const url = publicStoreUrl.value
    const dataUrl = await QRCode.toDataURL(url, {
      width: 400,
      margin: 2,
      color: {
        dark: '#0f172a',
        light: '#ffffff'
      }
    })
    qrCodeDataUrl.value = dataUrl
  } catch (err) {
    console.error('Failed to generate QR code', err)
  }
}

const fetchSettings = async () => {
  try {
    const data = await api.get('/admin/settings') as StoreSettingsDto
    form.value = data
    await generateQRCode()
  } catch (err: any) {
    console.error('Fetch settings failed', err)
  }
}

const saveSettings = async () => {
  saving.value = true
  try {
    await api.put('/admin/settings', {
      name: form.value.name,
      logo: form.value.logo,
      banner: form.value.banner,
      phone: form.value.phone,
      address: form.value.address,
      hours: form.value.hours,
      announcement: form.value.announcement
    })
    alert('门店资料保存成功！')
    await fetchSettings()
  } catch (err: any) {
    alert(err.message || '保存失败')
  } finally {
    saving.value = false
  }
}

const uploadImage = async (event: any, field: 'logo' | 'banner') => {
  const file = event.target.files?.[0]
  if (!file) return

  const formData = new FormData()
  formData.append('file', file)

  try {
    const res = await api.post('/admin/upload', formData, {
      headers: { 'Content-Type': 'multipart/form-data' }
    }) as any

    form.value[field] = res.url
    alert('图片上传成功！')
  } catch (err: any) {
    alert(err.message || '图片上传失败')
  }
}

const downloadQrCode = () => {
  if (!qrCodeDataUrl.value) return
  const link = document.createElement('a')
  link.href = qrCodeDataUrl.value
  link.download = `${form.value.slug}-qrcode.png`
  link.click()
}

watch(() => form.value.slug, () => {
  generateQRCode()
})

onMounted(() => {
  fetchSettings()
})
</script>

<style scoped>
.settings-view {
  display: flex;
  flex-direction: column;
  gap: 20px;
}

.settings-grid {
  display: grid;
  grid-template-columns: 1fr 380px;
  gap: 24px;
}

@media (max-width: 900px) {
  .settings-grid {
    grid-template-columns: 1fr;
  }
}

.settings-card, .qr-stand-card {
  background: #ffffff;
  border-radius: 12px;
  border: 1px solid #e2e8f0;
  padding: 24px;
}

.card-header h3 {
  font-size: 16px;
  font-weight: 700;
  color: #0f172a;
}

.card-header p {
  font-size: 13px;
  color: #64748b;
  margin-top: 4px;
  margin-bottom: 20px;
}

.settings-form {
  display: flex;
  flex-direction: column;
  gap: 14px;
}

.form-item label {
  display: block;
  font-size: 13px;
  font-weight: 500;
  color: #334155;
  margin-bottom: 4px;
}

.required {
  color: #e11d48;
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
  padding: 9px 12px;
  border: 1px solid #cbd5e1;
  border-radius: 6px;
  font-size: 13px;
}

.hint {
  font-size: 11px;
  color: var(--primary);
  margin-top: 4px;
  display: block;
}

.upload-row {
  display: flex;
  gap: 8px;
}

.upload-btn {
  white-space: nowrap;
  position: relative;
  cursor: pointer;
}

.hidden-file {
  position: absolute;
  inset: 0;
  opacity: 0;
  cursor: pointer;
}

.save-btn {
  padding: 12px;
  margin-top: 10px;
}

/* Stand Preview */
.stand-preview-box {
  background: #ffffff;
  border: 2px solid #f43f5e;
  border-radius: 16px;
  padding: 24px 20px;
  text-align: center;
  box-shadow: 0 10px 25px -5px rgba(244, 63, 94, 0.15);
  margin-bottom: 20px;
}

.stand-banner {
  background: linear-gradient(135deg, #f43f5e, #be123c);
  color: #ffffff;
  padding: 14px 10px;
  border-radius: 10px;
  margin-bottom: 20px;
}

.stand-sub {
  font-size: 12px;
  opacity: 0.9;
  letter-spacing: 2px;
  text-transform: uppercase;
}

.stand-store-name {
  font-size: 18px;
  font-weight: 700;
  margin-top: 4px;
}

.qr-image-wrapper {
  background: #ffffff;
  padding: 12px;
  border-radius: 12px;
  display: inline-block;
  box-shadow: 0 2px 8px rgba(0, 0, 0, 0.08);
}

.qr-img {
  width: 200px;
  height: 200px;
  display: block;
}

.qr-placeholder {
  width: 200px;
  height: 200px;
  display: flex;
  align-items: center;
  justify-content: center;
  color: #94a3b8;
  font-size: 13px;
}

.stand-footer {
  margin-top: 16px;
}

.scan-tip {
  font-size: 15px;
  font-weight: 700;
  color: #0f172a;
}

.perks {
  font-size: 12px;
  color: #e11d48;
  font-weight: 600;
  margin: 4px 0 10px;
}

.stand-meta {
  font-size: 11px;
  color: #64748b;
  margin-top: 2px;
}

.stand-actions {
  display: flex;
  flex-direction: column;
  gap: 10px;
}

.full-btn {
  width: 100%;
  padding: 10px;
}
</style>
