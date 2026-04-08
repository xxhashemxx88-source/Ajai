<template>
  <div class="dashboard">

    <!-- ===== SOUND TOGGLE ===== -->
    <button class="sound-toggle" @click="soundEnabled = !soundEnabled" :title="soundEnabled ? 'Mute alerts' : 'Unmute alerts'">
      <svg v-if="soundEnabled" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
        <polygon points="11 5 6 9 2 9 2 15 6 15 11 19 11 5"/>
        <path d="M19.07 4.93a10 10 0 010 14.14M15.54 8.46a5 5 0 010 7.07"/>
      </svg>
      <svg v-else width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
        <polygon points="11 5 6 9 2 9 2 15 6 15 11 19 11 5"/>
        <line x1="23" y1="9" x2="17" y2="15"/><line x1="17" y1="9" x2="23" y2="15"/>
      </svg>
    </button>

    <!-- ===== THREAT FLASH OVERLAY ===== -->
    <div class="threat-flash" :class="{ 'flash-active': isFlashing }"></div>

    <!-- ===== NAVBAR ===== -->
    <nav class="navbar">
      <div class="nav-brand">
        <div class="nav-logo">
          <svg width="18" height="18" viewBox="0 0 24 24" fill="none">
            <path d="M12 2L2 7l10 5 10-5-10-5z" stroke="url(#ng)" stroke-width="1.8" stroke-linejoin="round"/>
            <path d="M2 17l10 5 10-5M2 12l10 5 10-5" stroke="url(#ng)" stroke-width="1.8" stroke-linejoin="round"/>
            <defs>
              <linearGradient id="ng" x1="0" y1="0" x2="24" y2="24">
                <stop stop-color="#3d9fe6"/><stop offset="1" stop-color="#ca46fd"/>
              </linearGradient>
            </defs>
          </svg>
        </div>
        <span class="nav-name">AJAI <span class="text-grad">Security</span></span>
      </div>

      <div class="nav-stats">
        <div class="nav-stat">
          <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="#3d9fe6" stroke-width="2">
            <path d="M15 10l4.553-2.553A1 1 0 0121 8.382v7.236a1 1 0 01-1.447.894L15 14M3 8a2 2 0 012-2h8a2 2 0 012 2v8a2 2 0 01-2 2H5a2 2 0 01-2-2V8z"/>
          </svg>
          <span>{{ activeCameras }}/{{ cameras.cameras.length }}</span>
          <span class="stat-lbl">Units</span>
        </div>
        <div class="stat-div"></div>
        <div class="nav-stat">
          <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="#c54aef" stroke-width="2">
            <path d="M10.29 3.86L1.82 18a2 2 0 001.71 3h16.94a2 2 0 001.71-3L13.71 3.86a2 2 0 00-3.42 0z"/>
          </svg>
          <span>{{ alerts.alerts.length }}</span>
          <span class="stat-lbl">Alerts</span>
        </div>
        <template v-if="alerts.newCount > 0">
          <div class="stat-div"></div>
          <div class="nav-stat">
            <span class="new-dot"></span>
            <span style="color:#f06080; font-weight:700">{{ alerts.newCount }} new</span>
          </div>
        </template>
      </div>

      <div class="nav-right">
        <div class="user-chip">
          <div class="user-avatar">{{ auth.email?.[0]?.toUpperCase() }}</div>
          <span class="user-email">{{ auth.email }}</span>
        </div>
        <button class="btn-danger" @click="handleLogout">
          <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5">
            <path d="M9 21H5a2 2 0 01-2-2V5a2 2 0 012-2h4M16 17l5-5-5-5M21 12H9"/>
          </svg>
          Sign Out
        </button>
      </div>
    </nav>

    <!-- ===== MAIN LAYOUT ===== -->
    <div class="main-layout">

      <!-- CAMERAS GRID -->
      <div class="cameras-section">
        <div class="section-header-row">
          <div class="section-label">Live Surveillance</div>
          <button class="refresh-btn" @click="cameras.fetchCameras()">
            <svg width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5">
              <polyline points="23 4 23 10 17 10"/>
              <path d="M20.49 15a9 9 0 11-2.12-9.36L23 10"/>
            </svg>
            Refresh
          </button>
        </div>

        <div class="cam-grid" :class="`grid-${cameras.cameras.length}`">
          <div
            v-for="cam in cameras.cameras"
            :key="cam.id"
            class="cam-cell"
            :class="{ 'cam-cell-active': cam.isActive }"
            @click="expandCam(cam)"
          >
            <div class="cell-feed">
              <img
                v-if="alerts.liveFrames[cam.email]"
                :src="`data:image/jpeg;base64,${alerts.liveFrames[cam.email]}`"
                class="cell-img" alt="live"
              />
              <div v-else class="cell-empty">
                <svg width="32" height="32" viewBox="0 0 24 24" fill="none"
                  :stroke="cam.isActive ? 'rgba(57,201,110,0.3)' : 'rgba(197,74,239,0.2)'" stroke-width="1">
                  <path d="M15 10l4.553-2.553A1 1 0 0121 8.382v7.236a1 1 0 01-1.447.894L15 14M3 8a2 2 0 012-2h8a2 2 0 012 2v8a2 2 0 01-2 2H5a2 2 0 01-2-2V8z"/>
                </svg>
                <span>{{ cam.isActive ? 'Waiting...' : 'Offline' }}</span>
              </div>
              <div class="cell-scanlines"></div>
              <div class="c c-tl"></div><div class="c c-tr"></div>
              <div class="c c-bl"></div><div class="c c-br"></div>
              <div class="expand-hint">
                <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="white" stroke-width="2">
                  <polyline points="15 3 21 3 21 9"/><polyline points="9 21 3 21 3 15"/>
                  <line x1="21" y1="3" x2="14" y2="10"/><line x1="3" y1="21" x2="10" y2="14"/>
                </svg>
              </div>
            </div>
            <div class="cell-footer">
              <div class="cell-info">
                <span class="status-dot" :class="cam.isActive ? 'status-active' : 'status-inactive'"></span>
                <span class="cell-name">Unit-{{ String(cam.id).padStart(3,'0') }}</span>
                <span class="cell-email">{{ cam.email }}</span>
              </div>
              <div class="cell-actions" @click.stop>
                <span v-if="cam.isActive && alerts.liveFrames[cam.email]" class="live-pill">
                  <span class="rec-dot"></span> LIVE
                </span>
                <button class="cell-toggle" :class="cam.isActive ? 'ct-off' : 'ct-on'" @click="cameras.toggleCamera(cam.id)">
                  {{ cam.isActive ? 'Disable' : 'Enable' }}
                </button>
              </div>
            </div>
          </div>

          <div v-if="cameras.cameras.length === 0" class="no-cams">
            <svg width="48" height="48" viewBox="0 0 24 24" fill="none" stroke="rgba(197,74,239,0.15)" stroke-width="1">
              <path d="M15 10l4.553-2.553A1 1 0 0121 8.382v7.236a1 1 0 01-1.447.894L15 14M3 8a2 2 0 012-2h8a2 2 0 012 2v8a2 2 0 01-2 2H5a2 2 0 01-2-2V8z"/>
            </svg>
            <p>No camera units registered</p>
          </div>
        </div>
      </div>

      <!-- ALERTS PANEL -->
      <div class="alerts-section cyber-card">
        <div class="panel-header">
          <div>
            <div class="section-label">AI Detection</div>
            <h3 class="panel-title">
              Threat Alerts
              <span v-if="alerts.newCount > 0" class="new-badge">+{{ alerts.newCount }}</span>
            </h3>
          </div>
          <!-- ✅ أزرار: Clear + Delete All -->
          <div class="panel-actions">
            <button v-if="alerts.newCount" class="clear-btn" @click="alerts.clearNewCount()">
              <svg width="10" height="10" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5">
                <line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/>
              </svg>
              Clear
            </button>
            <button v-if="alerts.alerts.length > 0" class="delete-all-btn" @click="confirmDeleteAll">
              <svg width="11" height="11" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <polyline points="3 6 5 6 21 6"/>
                <path d="M19 6l-1 14a2 2 0 01-2 2H8a2 2 0 01-2-2L5 6"/>
                <path d="M10 11v6M14 11v6"/>
              </svg>
              Delete All
            </button>
          </div>
        </div>

        <div v-if="alerts.loading" class="loading-block">
          <div class="spinner-purple"></div>
          <span>Loading...</span>
        </div>

        <div v-else class="alerts-scroll">
          <div
            v-for="alert in alerts.alerts"
            :key="alert.id"
            class="alert-item"
            :class="{ 'alert-new-item': alert.isNew }"
            @click="previewAlert = alert"
          >
            <div class="alert-thumb">
              <img v-if="alert.base64Image" :src="`data:image/jpeg;base64,${alert.base64Image}`" alt="threat"/>
              <div v-else class="thumb-empty">
                <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="rgba(240,96,128,0.4)" stroke-width="1.5">
                  <rect x="3" y="3" width="18" height="18" rx="2"/>
                  <circle cx="8.5" cy="8.5" r="1.5"/>
                  <polyline points="21 15 16 10 5 21"/>
                </svg>
              </div>
            </div>
            <div class="alert-info">
              <div class="alert-type">
                <svg width="9" height="9" viewBox="0 0 24 24" fill="#f06080" style="flex-shrink:0">
                  <path d="M10.29 3.86L1.82 18a2 2 0 001.71 3h16.94a2 2 0 001.71-3L13.71 3.86a2 2 0 00-3.42 0z"/>
                </svg>
                {{ alert.alertType }}
              </div>
              <div class="alert-meta">{{ alert.cameraEmail }}</div>
              <div class="alert-time">{{ formatDateTime(alert.createdAt) }}</div>
            </div>
            <div class="alert-sev">
              <div class="sev-dot"></div>
              <span>THREAT</span>
            </div>
            <!-- ✅ زر حذف على كل تنبيه -->
            <button class="alert-delete-btn" @click.stop="alerts.deleteAlert(alert.id)" title="Delete">
              <svg width="11" height="11" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5">
                <line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/>
              </svg>
            </button>
          </div>

          <div v-if="alerts.alerts.length === 0" class="empty-state">
            <svg width="32" height="32" viewBox="0 0 24 24" fill="none" stroke="rgba(197,74,239,0.15)" stroke-width="1">
              <path d="M12 22s8-4 8-10V5l-8-3-8 3v7c0 6 8 10 8 10z"/>
            </svg>
            <p>No threats detected</p>
          </div>
        </div>
      </div>

    </div>

    <!-- EXPANDED CAM MODAL -->
    <div v-if="expandedCam" class="fullscreen-overlay" @click.self="expandedCam = null">
      <div class="fullscreen-box">
        <div class="fs-header">
          <div class="fs-title">
            <span class="status-dot" :class="expandedCam.isActive ? 'status-active' : 'status-inactive'"></span>
            Unit-{{ String(expandedCam.id).padStart(3,'0') }}
            <span class="fs-email">{{ expandedCam.email }}</span>
            <span v-if="expandedCam.isActive && alerts.liveFrames[expandedCam.email]" class="live-pill">
              <span class="rec-dot"></span> LIVE
            </span>
          </div>
          <div class="fs-actions">
            <button class="cell-toggle" :class="expandedCam.isActive ? 'ct-off' : 'ct-on'" @click="cameras.toggleCamera(expandedCam.id)">
              {{ expandedCam.isActive ? 'Disable' : 'Enable' }}
            </button>
            <button class="close-btn" @click="expandedCam = null">
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5">
                <line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/>
              </svg>
            </button>
          </div>
        </div>
        <div class="fs-feed">
          <img v-if="alerts.liveFrames[expandedCam.email]"
            :src="`data:image/jpeg;base64,${alerts.liveFrames[expandedCam.email]}`"
            class="fs-img" alt="live"/>
          <div v-else class="fs-empty">
            <svg width="64" height="64" viewBox="0 0 24 24" fill="none"
              :stroke="expandedCam.isActive ? 'rgba(57,201,110,0.2)' : 'rgba(197,74,239,0.15)'" stroke-width="0.8">
              <path d="M15 10l4.553-2.553A1 1 0 0121 8.382v7.236a1 1 0 01-1.447.894L15 14M3 8a2 2 0 012-2h8a2 2 0 012 2v8a2 2 0 01-2 2H5a2 2 0 01-2-2V8z"/>
            </svg>
            <p>{{ expandedCam.isActive ? 'Waiting for stream...' : 'Camera offline' }}</p>
          </div>
          <div class="cell-scanlines"></div>
          <div class="c c-tl" style="width:24px;height:24px"></div>
          <div class="c c-tr" style="width:24px;height:24px"></div>
          <div class="c c-bl" style="width:24px;height:24px"></div>
          <div class="c c-br" style="width:24px;height:24px"></div>
        </div>
        <div class="fs-footer">
          <span class="fs-ping">
            <svg width="11" height="11" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <circle cx="12" cy="12" r="10"/><polyline points="12 6 12 12 16 14"/>
            </svg>
            Last ping: {{ formatTime(expandedCam.lastPing) }}
          </span>
          <span class="fs-hint">Click outside to close</span>
        </div>
      </div>
    </div>

    <!-- ALERT PREVIEW MODAL -->
    <div v-if="previewAlert" class="fullscreen-overlay" @click.self="previewAlert = null">
      <div class="alert-modal cyber-card">
        <div class="fs-header">
          <div class="fs-title" style="color:#f07090">
            <svg width="13" height="13" viewBox="0 0 24 24" fill="#f06080">
              <path d="M10.29 3.86L1.82 18a2 2 0 001.71 3h16.94a2 2 0 001.71-3L13.71 3.86a2 2 0 00-3.42 0z"/>
            </svg>
            {{ previewAlert.alertType }}
          </div>
          <button class="close-btn" @click="previewAlert = null">
            <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5">
              <line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/>
            </svg>
          </button>
        </div>
        <img v-if="previewAlert.base64Image"
          :src="`data:image/jpeg;base64,${previewAlert.base64Image}`"
          class="alert-modal-img"/>
        <div class="fs-footer">
          <span>{{ previewAlert.cameraEmail }}</span>
          <span>{{ formatDateTime(previewAlert.createdAt) }}</span>
        </div>
      </div>
    </div>

  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import { useCamerasStore } from '../stores/cameras'
import { useAlertsStore } from '../stores/alerts'
import { startSignalR, joinAdminGroup, stopSignalR } from '../services/signalr'

const router    = useRouter()
const auth      = useAuthStore()
const cameras   = useCamerasStore()
const alerts    = useAlertsStore()

const expandedCam  = ref(null)
const previewAlert = ref(null)
const soundEnabled = ref(true)
const isFlashing   = ref(false)

let audioCtx = null
function playAlertSound() {
  if (!soundEnabled.value) return
  try {
    if (!audioCtx) audioCtx = new (window.AudioContext || window.webkitAudioContext)()
    const beep = (freq, start, duration) => {
      const osc = audioCtx.createOscillator()
      const gain = audioCtx.createGain()
      osc.connect(gain)
      gain.connect(audioCtx.destination)
      osc.frequency.value = freq
      osc.type = 'square'
      gain.gain.setValueAtTime(0.3, audioCtx.currentTime + start)
      gain.gain.exponentialRampToValueAtTime(0.001, audioCtx.currentTime + start + duration)
      osc.start(audioCtx.currentTime + start)
      osc.stop(audioCtx.currentTime + start + duration)
    }
    beep(880, 0,    0.15)
    beep(660, 0.18, 0.25)
  } catch (e) {
    console.warn('[Sound] Audio failed:', e)
  }
}

function flashScreen() {
  isFlashing.value = true
  setTimeout(() => { isFlashing.value = false }, 600)
}

const activeCameras = computed(() => cameras.cameras.filter(c => c.isActive).length)
function expandCam(cam) { expandedCam.value = cam }

// ✅ تأكيد قبل حذف الكل
function confirmDeleteAll() {
  if (confirm('Delete all alerts from database?')) {
    alerts.deleteAllAlerts()
  }
}

onMounted(async () => {
  await Promise.all([cameras.fetchCameras(), alerts.fetchAlerts()])
  try {
    await startSignalR({
      onLiveFrame: ({ cameraEmail, base64Image }) =>
        alerts.updateLiveFrame(cameraEmail, base64Image),
      onNewAlert: (alert) => {
        alerts.addAlert(alert)
        playAlertSound()
        flashScreen()
      },
      onCameraStatusChanged: ({ cameraId, status }) =>
        cameras.updateCameraStatus(cameraId, status),
      onDisconnected: () => handleLogout()
    })
    await joinAdminGroup()
  } catch (e) {
    console.error('[SignalR] Connect failed:', e)
  }
})

onUnmounted(() => stopSignalR())
function handleLogout() { stopSignalR(); auth.logout(); router.push('/login') }
function formatTime(d) {
  if (!d) return '—'
  return new Date(d).toLocaleTimeString('en-US', { hour12: false })
}
function formatDateTime(d) {
  if (!d) return '—'
  return new Date(d).toLocaleString('en-US', { hour12: false, dateStyle: 'short', timeStyle: 'short' })
}
</script>

<style scoped>
.dashboard { height: 100vh; display: flex; flex-direction: column; overflow: hidden; position: relative; z-index: 1; }

.threat-flash { position: fixed; inset: 0; z-index: 999; pointer-events: none; border: 0px solid #f06080; transition: border-width 0s; }
.flash-active { border-width: 6px; animation: screen-flash 0.6s ease; }
@keyframes screen-flash {
  0%   { border-color: rgba(240,96,128,0.9); border-width: 6px; }
  50%  { border-color: rgba(240,96,128,0.4); border-width: 3px; }
  100% { border-color: rgba(240,96,128,0);   border-width: 0px; }
}

.sound-toggle {
  position: fixed; bottom: 20px; right: 20px; z-index: 100;
  width: 40px; height: 40px; border-radius: 50%;
  background: rgba(10,10,30,0.9); border: 1px solid var(--border-dim);
  color: var(--text-dim); cursor: pointer; transition: all 0.2s;
  display: flex; align-items: center; justify-content: center;
}
.sound-toggle:hover { border-color: rgba(197,74,239,0.4); color: #fff; }

.navbar {
  display: flex; align-items: center; justify-content: space-between;
  padding: 10px 20px; background: rgba(10,10,30,0.95); backdrop-filter: blur(12px);
  border-bottom: 1px solid var(--border-dim); flex-shrink: 0; gap: 12px; z-index: 10;
}
.nav-brand { display: flex; align-items: center; gap: 9px; flex-shrink: 0; }
.nav-logo { width: 32px; height: 32px; border-radius: 7px; background: linear-gradient(135deg, rgba(61,159,230,0.15), rgba(202,70,253,0.15)); border: 1px solid rgba(197,74,239,0.3); display: flex; align-items: center; justify-content: center; }
.nav-name { font-size: 15px; font-weight: 700; color: #fff; }
.nav-stats { display: flex; align-items: center; gap: 12px; padding: 5px 14px; background: rgba(197,74,239,0.05); border: 1px solid var(--border-dim); border-radius: 20px; }
.nav-stat { display: flex; align-items: center; gap: 6px; font-size: 13px; font-weight: 600; color: #fff; }
.stat-lbl { font-size: 11px; color: var(--text-dim); font-weight: 400; }
.stat-div { width: 1px; height: 14px; background: var(--border-dim); }
.new-dot { width: 7px; height: 7px; border-radius: 50%; background: #f06080; box-shadow: 0 0 6px rgba(240,96,128,0.6); animation: pa 1s infinite; }
@keyframes pa { 0%,100%{opacity:1} 50%{opacity:0.4} }
.nav-right { display: flex; align-items: center; gap: 10px; flex-shrink: 0; }
.user-chip { display: flex; align-items: center; gap: 7px; padding: 5px 12px; background: rgba(197,74,239,0.08); border: 1px solid var(--border-dim); border-radius: 20px; }
.user-avatar { width: 22px; height: 22px; border-radius: 50%; background: linear-gradient(135deg, #3d9fe6, #ca46fd); display: flex; align-items: center; justify-content: center; font-size: 10px; font-weight: 700; color: #fff; }
.user-email { font-size: 12px; color: var(--text-body); max-width: 130px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }

.main-layout { display: grid; grid-template-columns: 1fr 300px; gap: 0; flex: 1; min-height: 0; overflow: hidden; }
@media (max-width: 860px) { .main-layout { grid-template-columns: 1fr; overflow-y: auto; } }

.cameras-section { display: flex; flex-direction: column; padding: 14px; gap: 10px; min-height: 0; overflow: hidden; border-right: 1px solid var(--border-dim); }
.section-header-row { display: flex; align-items: center; justify-content: space-between; flex-shrink: 0; }
.refresh-btn { display: flex; align-items: center; gap: 6px; font-size: 11px; padding: 5px 12px; background: rgba(197,74,239,0.06); border: 1px solid var(--border-dim); border-radius: 6px; color: var(--text-dim); cursor: pointer; transition: all 0.2s; font-family: var(--font-main, sans-serif); }
.refresh-btn:hover { border-color: rgba(197,74,239,0.3); color: #c8c8e8; }

.cam-grid { flex: 1; min-height: 0; display: grid; gap: 10px; }
.grid-1 { grid-template-columns: 1fr; }
.grid-2 { grid-template-columns: 1fr 1fr; }
.grid-3 { grid-template-columns: 1fr 1fr; grid-template-rows: 1fr 1fr; }
.grid-4 { grid-template-columns: 1fr 1fr; grid-template-rows: 1fr 1fr; }
.grid-3 .cam-cell:first-child { grid-column: span 2; }
.cam-cell { display: flex; flex-direction: column; border-radius: 8px; overflow: hidden; border: 1px solid var(--border-dim); background: var(--bg-card); cursor: pointer; transition: all 0.25s; min-height: 0; }
.cam-cell:hover { border-color: rgba(197,74,239,0.4); box-shadow: 0 0 20px rgba(197,74,239,0.1); }
.cam-cell-active { border-color: rgba(57,201,110,0.2); }
.cell-feed { flex: 1; position: relative; background: #000; min-height: 0; overflow: hidden; }
.cell-img { width: 100%; height: 100%; object-fit: cover; display: block; }
.cell-empty { width: 100%; height: 100%; min-height: 80px; display: flex; flex-direction: column; align-items: center; justify-content: center; gap: 8px; color: var(--text-dim); font-size: 12px; }
.cell-scanlines { position: absolute; inset: 0; pointer-events: none; background: repeating-linear-gradient(0deg, transparent, transparent 3px, rgba(0,0,0,0.04) 3px, rgba(0,0,0,0.04) 4px); }
.c { position: absolute; width: 14px; height: 14px; border-color: rgba(197,74,239,0.5); border-style: solid; pointer-events: none; }
.c-tl { top:0; left:0; border-width:2px 0 0 2px; } .c-tr { top:0; right:0; border-width:2px 2px 0 0; }
.c-bl { bottom:0; left:0; border-width:0 0 2px 2px; } .c-br { bottom:0; right:0; border-width:0 2px 2px 0; }
.expand-hint { position: absolute; inset: 0; display: flex; align-items: center; justify-content: center; background: rgba(0,0,0,0); opacity: 0; transition: all 0.25s; }
.cam-cell:hover .expand-hint { opacity: 1; background: rgba(0,0,0,0.3); }
.cell-footer { display: flex; align-items: center; justify-content: space-between; padding: 7px 10px; flex-shrink: 0; border-top: 1px solid var(--border-dim); background: rgba(10,10,28,0.8); }
.cell-info { display: flex; align-items: center; gap: 7px; min-width: 0; }
.cell-name { font-size: 12px; font-weight: 600; color: #fff; flex-shrink: 0; }
.cell-email { font-size: 11px; color: var(--text-dim); overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.cell-actions { display: flex; align-items: center; gap: 7px; flex-shrink: 0; }
.live-pill { display: flex; align-items: center; gap: 5px; font-size: 9px; font-weight: 700; letter-spacing: 1px; padding: 3px 8px; border-radius: 4px; background: rgba(220,50,80,0.8); color: #fff; }
.rec-dot { width: 6px; height: 6px; border-radius: 50%; background: #fff; animation: blink 1s infinite; }
@keyframes blink { 0%,100%{opacity:1} 50%{opacity:0.2} }
.cell-toggle { font-size: 10px; font-weight: 700; padding: 4px 10px; border-radius: 4px; cursor: pointer; border: 1px solid; transition: all 0.2s; font-family: var(--font-main, sans-serif); }
.ct-on  { border-color: rgba(57,201,110,0.35); color: #39c96e; background: rgba(57,201,110,0.08); }
.ct-on:hover  { background: rgba(57,201,110,0.18); }
.ct-off { border-color: rgba(240,96,128,0.35); color: #f06080; background: rgba(240,96,128,0.08); }
.ct-off:hover { background: rgba(240,96,128,0.18); }
.no-cams { display: flex; flex-direction: column; align-items: center; justify-content: center; gap: 10px; color: var(--text-dim); font-size: 14px; grid-column: span 2; }
.status-dot { width: 7px; height: 7px; border-radius: 50%; display: inline-block; flex-shrink: 0; }
.status-active { background: #39c96e; box-shadow: 0 0 6px rgba(57,201,110,0.6); animation: pa 2s infinite; }
.status-inactive { background: #444; }

/* ALERTS */
.alerts-section { display: flex; flex-direction: column; border-radius: 0; min-height: 0; overflow: hidden; }
.panel-header { display: flex; align-items: flex-start; justify-content: space-between; padding: 14px 14px 10px; flex-shrink: 0; border-bottom: 1px solid var(--border-dim); }
.panel-title { font-size: 15px; font-weight: 700; color: #fff; margin-top: 4px; display: flex; align-items: center; gap: 8px; }
.new-badge { font-size: 11px; font-weight: 700; background: rgba(240,96,128,0.15); border: 1px solid rgba(240,96,128,0.3); color: #f06080; padding: 2px 8px; border-radius: 12px; animation: pa 1s infinite; }

/* ✅ أزرار الحذف */
.panel-actions { display: flex; align-items: center; gap: 6px; }
.clear-btn { display: flex; align-items: center; gap: 5px; font-size: 11px; padding: 5px 10px; border-radius: 5px; border: 1px solid rgba(240,96,128,0.3); color: #f06080; background: rgba(240,96,128,0.06); cursor: pointer; transition: all 0.2s; font-family: var(--font-main, sans-serif); }
.clear-btn:hover { background: rgba(240,96,128,0.14); }
.delete-all-btn { display: flex; align-items: center; gap: 5px; font-size: 11px; padding: 5px 10px; border-radius: 5px; border: 1px solid rgba(255,80,80,0.3); color: #ff6060; background: rgba(255,80,80,0.06); cursor: pointer; transition: all 0.2s; font-family: var(--font-main, sans-serif); }
.delete-all-btn:hover { background: rgba(255,80,80,0.18); border-color: rgba(255,80,80,0.6); }
.alert-delete-btn { width: 24px; height: 24px; border-radius: 4px; border: 1px solid rgba(240,96,128,0.2); background: transparent; color: rgba(240,96,128,0.4); cursor: pointer; transition: all 0.2s; display: flex; align-items: center; justify-content: center; flex-shrink: 0; }
.alert-delete-btn:hover { background: rgba(240,96,128,0.15); border-color: rgba(240,96,128,0.5); color: #f06080; }

.loading-block { display: flex; align-items: center; gap: 10px; padding: 20px 14px; color: var(--text-dim); font-size: 13px; }
.spinner-purple { width: 15px; height: 15px; border: 2px solid rgba(197,74,239,0.2); border-top-color: #c54aef; border-radius: 50%; animation: spin 0.8s linear infinite; }
@keyframes spin { to { transform: rotate(360deg); } }
.alerts-scroll { flex: 1; overflow-y: auto; padding: 8px 10px; display: flex; flex-direction: column; gap: 7px; }
.alert-item { display: flex; align-items: center; gap: 9px; padding: 8px 10px; background: rgba(240,96,128,0.04); border: 1px solid rgba(240,96,128,0.15); border-left: 3px solid #f06080; border-radius: 6px; cursor: pointer; transition: all 0.2s; }
.alert-item:hover { background: rgba(240,96,128,0.09); }
.alert-thumb { width: 52px; height: 38px; border-radius: 4px; overflow: hidden; flex-shrink: 0; }
.alert-thumb img { width: 100%; height: 100%; object-fit: cover; }
.thumb-empty { width: 100%; height: 100%; background: rgba(240,96,128,0.05); border: 1px dashed rgba(240,96,128,0.2); display: flex; align-items: center; justify-content: center; }
.alert-info { flex: 1; min-width: 0; }
.alert-type { font-size: 11px; font-weight: 700; color: #f07090; display: flex; align-items: center; gap: 5px; }
.alert-meta { font-size: 10px; color: var(--text-dim); margin-top: 2px; overflow: hidden; text-overflow: ellipsis; white-space: nowrap; }
.alert-time { font-size: 10px; color: rgba(180,180,220,0.3); margin-top: 2px; }
.alert-sev { display: flex; flex-direction: column; align-items: center; gap: 3px; font-size: 8px; font-weight: 700; letter-spacing: 1.5px; color: rgba(240,96,128,0.5); flex-shrink: 0; }
.sev-dot { width: 8px; height: 8px; border-radius: 50%; background: #f06080; box-shadow: 0 0 8px rgba(240,96,128,0.5); animation: pa 1.5s infinite; }
.empty-state { display: flex; flex-direction: column; align-items: center; justify-content: center; flex: 1; gap: 10px; color: var(--text-dim); font-size: 13px; }

.fullscreen-overlay { position: fixed; inset: 0; z-index: 200; background: rgba(0,0,0,0.85); backdrop-filter: blur(6px); display: flex; align-items: center; justify-content: center; padding: 20px; }
.fullscreen-box { width: 90vw; max-width: 1100px; background: var(--bg-card, #10102e); border: 1px solid rgba(197,74,239,0.3); border-radius: 10px; overflow: hidden; display: flex; flex-direction: column; max-height: 90vh; box-shadow: 0 0 60px rgba(197,74,239,0.15); }
.fs-header { display: flex; align-items: center; justify-content: space-between; padding: 12px 16px; border-bottom: 1px solid var(--border-dim); flex-shrink: 0; }
.fs-title { display: flex; align-items: center; gap: 10px; font-size: 14px; font-weight: 700; color: #fff; }
.fs-email { font-size: 12px; color: var(--text-dim); font-weight: 400; }
.fs-actions { display: flex; align-items: center; gap: 10px; }
.close-btn { width: 32px; height: 32px; border-radius: 6px; background: rgba(255,255,255,0.05); border: 1px solid rgba(255,255,255,0.1); color: var(--text-dim); cursor: pointer; transition: all 0.2s; display: flex; align-items: center; justify-content: center; }
.close-btn:hover { background: rgba(255,255,255,0.1); color: #fff; }
.fs-feed { flex: 1; position: relative; background: #000; min-height: 300px; overflow: hidden; }
.fs-img { width: 100%; height: 100%; object-fit: contain; display: block; }
.fs-empty { width: 100%; height: 100%; display: flex; flex-direction: column; align-items: center; justify-content: center; gap: 14px; color: var(--text-dim); font-size: 14px; }
.fs-footer { display: flex; align-items: center; justify-content: space-between; padding: 10px 16px; border-top: 1px solid var(--border-dim); flex-shrink: 0; }
.fs-ping { display: flex; align-items: center; gap: 6px; font-size: 12px; color: var(--text-dim); }
.fs-hint { font-size: 11px; color: rgba(180,180,220,0.25); }
.alert-modal { width: 90vw; max-width: 640px; display: flex; flex-direction: column; overflow: hidden; }
.alert-modal-img { width: 100%; object-fit: contain; max-height: 60vh; background: #000; display: block; }
@keyframes slide-in { from{opacity:0;transform:translateY(-6px)} to{opacity:1;transform:translateY(0)} }
.alert-new-item { animation: slide-in 0.35s ease; }
</style>