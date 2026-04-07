import axios from 'axios'

const BASE_URL = `https://${window.location.hostname}:5001`

const api = axios.create({
  baseURL: `${BASE_URL}/api`,
  timeout: 10000,
  headers: { 'Content-Type': 'application/json' }
})

// أرسل التوكن مع كل طلب
api.interceptors.request.use((config) => {
  const token = localStorage.getItem('ajai_token')
  if (token) {
    config.headers.Authorization = `Bearer ${token}`
  }
  return config
})

// التعامل مع انتهاء الجلسة
api.interceptors.response.use(
  (response) => response,
  (error) => {
    if (error.response?.status === 401) {
      localStorage.removeItem('ajai_token')
      localStorage.removeItem('ajai_role')
      localStorage.removeItem('ajai_email')
      window.location.href = '/login'
    }
    return Promise.reject(error)
  }
)

export default api
export { BASE_URL }