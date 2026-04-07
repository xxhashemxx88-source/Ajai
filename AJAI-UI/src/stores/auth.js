import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import api from '../services/api'

export const useAuthStore = defineStore('auth', () => {
  const token = ref(localStorage.getItem('ajai_token') || null)
  const role = ref(localStorage.getItem('ajai_role') || null)
  const email = ref(localStorage.getItem('ajai_email') || null)
  const loading = ref(false)
  const error = ref(null)

  const isAuthenticated = computed(() => !!token.value)
  const isAdmin = computed(() => role.value === 'User')
  const isCamera = computed(() => role.value === 'Camera')

  async function login(emailInput, password) {
    loading.value = true
    error.value = null
    try {
      const res = await api.post('/auth/login', { email: emailInput, password })
      
      token.value = res.data.token
      role.value = res.data.role
      email.value = emailInput

      localStorage.setItem('ajai_token', token.value)
      localStorage.setItem('ajai_role', role.value)
      localStorage.setItem('ajai_email', email.value)

      return res.data
    } catch (err) {
      error.value = err.response?.data || 'Connection error'
      throw err
    } finally {
      loading.value = false
    }
  }

  function logout() {
    token.value = null
    role.value = null
    email.value = null
    localStorage.removeItem('ajai_token')
    localStorage.removeItem('ajai_role')
    localStorage.removeItem('ajai_email')
  }

  return { token, role, email, loading, error, isAuthenticated, isAdmin, isCamera, login, logout }
})