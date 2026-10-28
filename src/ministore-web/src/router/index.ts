import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/',
      redirect: '/s/muse-nail'
    },
    {
      path: '/s/:slug',
      name: 'CustomerStore',
      component: () => import('@/views/customer/StoreView.vue')
    },
    {
      path: '/admin/login',
      name: 'AdminLogin',
      component: () => import('@/views/admin/LoginView.vue')
    },
    {
      path: '/admin',
      component: () => import('@/layouts/AdminLayout.vue'),
      children: [
        {
          path: '',
          redirect: '/admin/dashboard'
        },
        {
          path: 'dashboard',
          name: 'AdminDashboard',
          component: () => import('@/views/admin/DashboardView.vue')
        },
        {
          path: 'appointments',
          name: 'AdminAppointments',
          component: () => import('@/views/admin/AppointmentsView.vue')
        },
        {
          path: 'customers',
          name: 'AdminCustomers',
          component: () => import('@/views/admin/CustomersView.vue')
        },
        {
          path: 'services',
          name: 'AdminServices',
          component: () => import('@/views/admin/ServicesView.vue')
        },
        {
          path: 'cards',
          name: 'AdminCards',
          component: () => import('@/views/admin/CardsView.vue')
        },
        {
          path: 'settings',
          name: 'AdminSettings',
          component: () => import('@/views/admin/SettingsView.vue')
        }
      ]
    },
    {
      path: '/:pathMatch(.*)*',
      redirect: '/s/muse-nail'
    }
  ]
})

router.beforeEach((to, _from, next) => {
  const authStore = useAuthStore()
  if (to.path.startsWith('/admin') && to.path !== '/admin/login') {
    if (!authStore.isAuthenticated()) {
      next('/admin/login')
      return
    }
  }
  next()
})

export default router
