import { defineStore } from 'pinia'
import { ref } from 'vue'
import api from '../services/api'

export const useAlertsStore = defineStore('alerts', () => {
  const alerts = ref([])
  const newCount = ref(0)
  const loading = ref(false)

  // Buffer وحدة لكل كاميرا (email → base64)
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

  // يُستدعى من SignalR - فريم حي (يُستبدل دائماً)
  function updateLiveFrame(cameraEmail, base64Image) {
    liveFrames.value[cameraEmail] = base64Image
  }

  // يُستدعى من SignalR - تهديد حقيقي (يُضاف للقائمة)
  function addAlert(alert) {
    const newAlert = { ...alert, isNew: true }
    alerts.value.unshift(newAlert)
    newCount.value++
    setTimeout(() => {
      const found = alerts.value.find(a => a.id === alert.id)
      if (found) found.isNew = false
    }, 4000)
  }

  // الكاميرا ترسل فريم حي للسيرفر
  async function sendLiveFrame(cameraEmail, base64Image) {
    await api.post('/alerts/stream', { cameraEmail, alertType: 'LIVE_FRAME', base64Image })
  }

  // الـ AI يرسل تهديد للسيرفر (يُحفظ في DB)
  async function sendAlert(cameraEmail, alertType, base64Image) {
    const res = await api.post('/alerts', { cameraEmail, alertType, base64Image })
    return res.data
  }

  function clearNewCount() {
    newCount.value = 0
  }

  return {
    alerts, newCount, loading, liveFrames,
    fetchAlerts, updateLiveFrame, addAlert,
    sendLiveFrame, sendAlert, clearNewCount
  }
})