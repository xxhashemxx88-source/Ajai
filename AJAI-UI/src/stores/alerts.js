import { defineStore } from 'pinia'
import { ref } from 'vue'
import api from '../services/api'

const MAX_ALERTS      = 50
const KEEP_IMAGES_FOR = 20

export const useAlertsStore = defineStore('alerts', () => {
  const alerts     = ref([])
  const newCount   = ref(0)
  const loading    = ref(false)
  const liveFrames = ref({})

  async function fetchAlerts() {
    loading.value = true
    try {
      const res = await api.get('/alerts')
      alerts.value = res.data
    } finally {
      loading.value = false
    }
  }

  function updateLiveFrame(cameraEmail, base64Image) {
    liveFrames.value[cameraEmail] = base64Image
  }

  function addAlert(alert) {
    alerts.value.unshift({ ...alert, isNew: true })
    newCount.value++

    if (alerts.value.length > MAX_ALERTS) {
      alerts.value = alerts.value.slice(0, MAX_ALERTS)
    }

    alerts.value.forEach((a, index) => {
      if (index >= KEEP_IMAGES_FOR && a.base64Image) {
        a.base64Image = ''
      }
    })

    setTimeout(() => {
      const found = alerts.value.find(a => a.id === alert.id)
      if (found) found.isNew = false
    }, 4000)
  }

  async function sendLiveFrame(cameraEmail, base64Image) {
    await api.post('/alerts/stream', {
      cameraEmail,
      alertType:   'LIVE_FRAME',
      base64Image
    })
  }

  async function sendAlert(cameraEmail, alertType, base64Image) {
    const res = await api.post('/alerts', { cameraEmail, alertType, base64Image })
    return res.data
  }

  // ══════════════════════════════════════════
  // حذف تنبيه واحد من DB والذاكرة
  // ══════════════════════════════════════════
  async function deleteAlert(id) {
    try {
      await api.delete(`/alerts/${id}`)
      alerts.value = alerts.value.filter(a => a.id !== id)
    } catch (e) {
      console.error('[Alerts] Delete failed:', e)
    }
  }

  // ══════════════════════════════════════════
  // حذف كل التنبيهات من DB والذاكرة
  // ══════════════════════════════════════════
  async function deleteAllAlerts() {
    try {
      await api.delete('/alerts')
      alerts.value  = []
      newCount.value = 0
    } catch (e) {
      console.error('[Alerts] Delete all failed:', e)
    }
  }

  function clearNewCount() {
    newCount.value = 0
  }

  return {
    alerts, newCount, loading, liveFrames,
    fetchAlerts, updateLiveFrame, addAlert,
    sendLiveFrame, sendAlert,
    deleteAlert, deleteAllAlerts,
    clearNewCount
  }
})