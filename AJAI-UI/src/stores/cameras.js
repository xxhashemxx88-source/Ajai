import { defineStore } from 'pinia'
import { ref } from 'vue'
import api from '../services/api'

export const useCamerasStore = defineStore('cameras', () => {
  const cameras = ref([])
  const loading = ref(false)

  async function fetchCameras() {
    loading.value = true
    try {
      const res = await api.get('/cameras')
      cameras.value = res.data
    } finally {
      loading.value = false
    }
  }

  async function toggleCamera(id) {
    // نحديث مباشر في الـ UI (Optimistic Update)
    const cam = cameras.value.find(c => c.id === id)
    if (cam) cam.isActive = !cam.isActive

    try {
      await api.post(`/cameras/toggle/${id}`)
    } catch {
      // إرجاع الحالة في حالة فشل الطلب
      if (cam) cam.isActive = !cam.isActive
    }
  }

  // يُستدعى من SignalR عند تغيير حالة كاميرا
  function updateCameraStatus(cameraId, status) {
    const cam = cameras.value.find(c => c.id === cameraId)
    if (cam) cam.isActive = status
  }

  return { cameras, loading, fetchCameras, toggleCamera, updateCameraStatus }
})