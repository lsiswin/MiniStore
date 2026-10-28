export interface ServicePublicDto {
  id: string
  categoryId: string
  name: string
  price: number
  durationMinutes: number
  imageUrl?: string
  description?: string
}

export interface CategoryPublicDto {
  id: string
  name: string
  sortOrder: number
  services: ServicePublicDto[]
}

export interface StorePublicDto {
  storeId: string
  name: string
  slug: string
  logo?: string
  banner?: string
  phone?: string
  address?: string
  hours?: string
  announcement?: string
  categories: CategoryPublicDto[]
}

export interface BookAppointmentRequest {
  slug: string
  serviceId: string
  appointmentAt: string
  customerName: string
  customerPhone: string
  notes?: string
}

export interface AppointmentResultDto {
  appointmentId: string
  storeName: string
  serviceName: string
  price: number
  appointmentAt: string
  customerName: string
  customerPhone: string
  status: string
  message: string
}

export interface CustomerCardItemDto {
  id: string
  cardName: string
  remainingCount: number
  expireAt?: string
}

export interface CustomerProfileDto {
  id?: string
  name: string
  phone: string
  visitCount: number
  lastVisitAt?: string
  notes?: string
  cards?: CustomerCardItemDto[]
}

export interface CustomerAppointmentDto {
  id: string
  serviceName: string
  price: number
  appointmentAt: string
  status: 'pending' | 'confirmed' | 'completed' | 'cancelled'
  notes?: string
  createdAt: string
}

export interface CurrentUserDto {
  id: string
  tenantId: string
  username: string
  displayName: string
  role: string
  storeName: string
  storeSlug: string
}

export interface DashboardSummaryDto {
  todayAppointmentsCount: number
  pendingAppointmentsCount: number
  totalCustomersCount: number
  todayEstimatedRevenue: number
  recentAppointments: AppointmentListItemDto[]
}

export interface AppointmentListItemDto {
  id: string
  appointmentAt: string
  status: 'pending' | 'confirmed' | 'completed' | 'cancelled'
  customerName: string
  customerPhone: string
  serviceName: string
  price: number
  notes?: string
  createdAt: string
}

export interface CustomerListItemDto {
  id: string
  name: string
  phone: string
  visitCount: number
  lastVisitAt?: string
  notes?: string
  createdAt: string
}

export interface ServiceCategoryDto {
  id: string
  name: string
  sortOrder: number
  servicesCount: number
}

export interface ServiceItemDto {
  id: string
  categoryId: string
  categoryName: string
  name: string
  price: number
  durationMinutes: number
  imageUrl?: string
  description?: string
  enabled: boolean
  sortOrder: number
}

export interface MemberCardDto {
  id: string
  name: string
  type: string
  price: number
  totalCount: number
  validDays: number
  description?: string
  activeCardsCount: number
}

export interface CustomerCardDto {
  id: string
  customerId: string
  customerName: string
  customerPhone: string
  cardId: string
  cardName: string
  remainingCount: number
  expireAt?: string
  createdAt: string
}

export interface StoreSettingsDto {
  id: string
  name: string
  slug: string
  logo?: string
  banner?: string
  phone?: string
  address?: string
  hours?: string
  announcement?: string
}
