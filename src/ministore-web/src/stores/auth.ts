import { defineStore } from 'pinia'
import { ref } from 'vue'
import { api } from '@/api/client'
import type { CurrentUserDto } from '@/types'

export const useAuthStore = defineStore('auth', () => {
  const token = ref<string>(localStorage.getItem('ministore_token') || '')
  
  const getInitialUser = (): CurrentUserDto | null => {
    const raw = localStorage.getItem('ministore_user')
    try {
      return raw ? JSON.parse(raw) : null
    } catch {
      return null
    }
  }

  const user = ref<CurrentUserDto | null>(getInitialUser())

  const isAuthenticated = () => !!token.value

  const setLogin = (newToken: string, newUser: CurrentUserDto) => {
    token.value = newToken
    user.value = newUser
    localStorage.setItem('ministore_token', newToken)
    localStorage.setItem('ministore_user', JSON.stringify(newUser))
  }

  const logout = () => {
    token.value = ''
    user.value = null
    localStorage.removeItem('ministore_token')
    localStorage.removeItem('ministore_user')
  }

  const fetchCurrentUser = async () => {
    if (!token.value) return null
    try {
      const data = await api.get('/admin/auth/me') as CurrentUserDto
      user.value = data
      localStorage.setItem('ministore_user', JSON.stringify(data))
      return data
    } catch {
      logout()
      return null
    }
  }

  return {
    token,
    user,
    isAuthenticated,
    setLogin,
    logout,
    fetchCurrentUser
  }
})
