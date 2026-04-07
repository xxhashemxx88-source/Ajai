<template>
  <div class="cam-page">
    <header class="top-bar">
      <div class="top-brand">
        <div class="brand-dot"></div>
        <span>AJAI <span class="text-grad">CAM</span></span>
      </div>
      <div class="top-center">
        <span class="unit-label">{{ auth.email }}</span>
      </div>
      <div class="top-right">
        <div class="conn-pill" :class="signalrConnected ? 'conn-live' : 'conn-off'">
          <span class="status-dot" :class="signalrConnected ? 'status-active' : 'status-inactive'"></span>
          {{ signalrConnected ? 'LINKED' : 'OFFLINE' }}
        </div>
        <button class="logout-btn" @click="handleLogout">
          <svg width="13" height="13" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5">
            <path d="M9 21H5a2 2 0 01-2-2V5a2 2 0 012-2h4M16 17l5-5-5-5M21 12H9"/>
          </svg>
          Logout
        </button>
      </div>
    </header>

    <main class="main-area">
      <!-- HTTPS Warning -->
      <div v-if="showHttpsWarning" class="https-warning">
        <div class="warning-icon">⚠</div>
        <div class="warning-body">
          <div class="warning-title">Camera Blocked — HTTPS Required</div>
          <p>Safari على iPhone يرفض الكاميرا على HTTP.</p>
          <code>ngrok http 3000</code>
        </div>
      </div>

      <!-- Feed -->
      <div class="feed-wrapper" v-else>
        <video ref="videoEl" class="feed-video" autoplay playsinline muted></video>
        <canvas ref="canvasEl" class="hidden-canvas"></canvas>

        <div v-if="permissionDenied" class="overlay-msg">
          <div class="overlay-icon">🚫</div>
          <div class="overlay-title">Camera Access Denied</div>
          <button class="btn-primary" @click="startCamera">Retry</button>
        </div>
        <div v-if="cameraLoading && !permissionDenied" class="overlay-msg">
          <div class="spinner-cam"></div>
          <div class="overlay-title">Initializing Camera...</div>
        </div>
        <div v-if="disabledByAdmin" class="overlay-msg overlay-admin">
          <div class="overlay-icon">🔒</div>
          <div class="overlay-title">Stream Disabled by Admin</div>
        </div>

        <div v-if="isStreaming" class="stream-badge">
          <span class="rec-dot"></span> STREAMING
        </div>
        <div v-if="!isStreaming && cameraReady" class="standby-badge">◉ STANDBY</div>

        <div class="scan-lines"></div>
        <div class="corner corner-tl"></div>
        <div class="corner corner-tr"></div>
        <div class="corner corner-bl"></div>
        <div class="corner corner-br"></div>
      </div>
    </main>

    <!-- BOTTOM BAR -->
    <footer class="bottom-bar" v-if="!showHttpsWarning">
      <div class="control-group">
        <span class="ctrl-label">FRAME RATE</span>
        <div class="fps-chips">
          <button v-for="fps in fpsOptions" :key="fps" class="fps-chip"
            :class="{ 'fps-active': selectedFps === fps }" @click="changeFps(fps)">
            {{ fps }}<span class="fps-unit">fps</span>
          </button>
        </div>
      </div>

      <div class="ctrl-divider"></div>

      <div class="control-group">
        <span class="ctrl-label">SESSION</span>
        <div class="stats-row">
          <div class="stat-box">
            <span class="stat-num">{{ framesSent }}</span>
            <span class="stat-lbl">Frames</span>
          </div>
          <div class="stat-box">
            <span class="stat-num">{{ sessionTime }}</span>
            <span class="stat-lbl">Duration</span>
          </div>
          <div class="stat-box">
            <span class="stat-num" :style="{ color: isStreaming ? '#c54aef' : 'inherit' }">{{ selectedFps }} fps</span>
            <span class="stat-lbl">Rate</span>
          </div>
        </div>
      </div>

      <div class="ctrl-divider"></div>

      <div class="control-group">
        <span class="ctrl-label">CONTROL</span>
        <button class="stream-toggle"
          :class="isStreaming ? 'toggle-stop' : 'toggle-start'"
          :disabled="!cameraReady || disabledByAdmin"
          @click="toggleStream">
          <svg width="13" height="13" viewBox="0 0 24 24" fill="currentColor" style="display:inline;vertical-align:-2px;margin-right:5px">
            <polygon v-if="!isStreaming" points="5 3 19 12 5 21 5 3"/>
            <rect v-else x="6" y="4" width="4" height="16"/>
            <rect v-if="isStreaming" x="14" y="4" width="4" height="16"/>
          </svg>
          {{ isStreaming ? 'STOP' : 'START STREAM' }}
        </button>
      </div>
    </footer>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, onUnmounted } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'
import { useAlertsStore } from '../stores/alerts'
import { startSignalR, joinCameraGroup, stopSignalR } from '../services/signalr'

const router = useRouter()
const auth = useAuthStore()
const alertsStore = useAlertsStore()

const videoEl = ref(null)
const canvasEl = ref(null)
const cameraReady = ref(false)
const cameraLoading = ref(false)
const permissionDenied = ref(false)
const isStreaming = ref(false)
const disabledByAdmin = ref(false)
const signalrConnected = ref(false)

const fpsOptions = [0.5, 1, 2, 5, 10]
const selectedFps = ref(1)
const framesSent = ref(0)
const sessionSeconds = ref(0)

let streamInterval = null
let sessionTimer = null

const showHttpsWarning = computed(() => {
  const isSafari = /^((?!chrome|android).)*safari/i.test(navigator.userAgent)
  return isSafari && window.location.protocol === 'http:'
})

const sessionTime = computed(() => {
  const m = Math.floor(sessionSeconds.value / 60)
  const s = sessionSeconds.value % 60
  return `${String(m).padStart(2,'0')}:${String(s).padStart(2,'0')}`
})

onMounted(async () => {
  if (!showHttpsWarning.value) await startCamera()
  try {
    await startSignalR({
      onCameraCommand: ({ command }) => {
        if (command === 'START_STREAM') { disabledByAdmin.value = false; if (!isStreaming.value && cameraReady.value) startStream() }
        else if (command === 'STOP_STREAM') { disabledByAdmin.value = true; stopStream() }
      }
    })
    await joinCameraGroup(auth.email)
    signalrConnected.value = true
  } catch (e) { console.error('[SignalR] Camera connect failed:', e) }
})

onUnmounted(() => { stopStream(); stopCamera(); stopSignalR() })

async function startCamera() {
  cameraLoading.value = true; permissionDenied.value = false
  try {
    const stream = await navigator.mediaDevices.getUserMedia({
      video: { facingMode: 'environment', width: { ideal: 1280 }, height: { ideal: 720 } },
      audio: false
    })
    if (videoEl.value) { videoEl.value.srcObject = stream; await videoEl.value.play() }
    cameraReady.value = true
  } catch { permissionDenied.value = true }
  finally { cameraLoading.value = false }
}

function stopCamera() {
  videoEl.value?.srcObject?.getTracks().forEach(t => t.stop())
  if (videoEl.value) videoEl.value.srcObject = null
}

function startStream() {
  if (streamInterval) clearInterval(streamInterval)
  isStreaming.value = true
  sessionTimer = setInterval(() => sessionSeconds.value++, 1000)
  streamInterval = setInterval(captureAndSend, Math.round(1000 / selectedFps.value))
}

function stopStream() {
  isStreaming.value = false
  if (streamInterval) { clearInterval(streamInterval); streamInterval = null }
  if (sessionTimer) { clearInterval(sessionTimer); sessionTimer = null }
}

function toggleStream() {
  isStreaming.value ? stopStream() : startStream()
}

function changeFps(fps) {
  selectedFps.value = fps
  if (isStreaming.value) {
    clearInterval(streamInterval)
    streamInterval = setInterval(captureAndSend, Math.round(1000 / fps))
  }
}

async function captureAndSend() {
  if (!videoEl.value || !canvasEl.value) return
  const video = videoEl.value
  const canvas = canvasEl.value
  canvas.width = video.videoWidth || 640
  canvas.height = video.videoHeight || 360
  canvas.getContext('2d').drawImage(video, 0, 0, canvas.width, canvas.height)

  const base64 = canvas.toDataURL('image/jpeg', 0.5).split(',')[1]

  try {
    // يرسل للـ /stream (buffer فقط، ما يُحفظ في DB)
    await alertsStore.sendLiveFrame(auth.email, base64)
    framesSent.value++
  } catch (e) { console.warn('[Stream] Send failed:', e) }
}

function handleLogout() {
  stopStream(); stopCamera(); stopSignalR(); auth.logout(); router.push('/login')
}
</script>

<style scoped>
.cam-page { height: 100vh; display: flex; flex-direction: column; background: #04040f; overflow: hidden; position: relative; z-index: 1; }

.top-bar {
  display: flex; align-items: center; justify-content: space-between;
  padding: 10px 20px;
  background: rgba(10,8,30,0.95); border-bottom: 1px solid rgba(197,74,239,0.2);
  flex-shrink: 0; gap: 12px;
}
.top-brand { display: flex; align-items: center; gap: 10px; font-size: 16px; font-weight: 700; color: #fff; letter-spacing: 2px; }
.brand-dot { width: 10px; height: 10px; border-radius: 50%; background: linear-gradient(135deg, #3d9fe6, #ca46fd); box-shadow: 0 0 8px rgba(202,70,253,0.6); }
.top-center { flex: 1; text-align: center; }
.unit-label { font-size: 12px; color: rgba(180,180,220,0.5); }
.top-right { display: flex; align-items: center; gap: 10px; }

.conn-pill { display: flex; align-items: center; gap: 6px; font-size: 10px; font-weight: 700; letter-spacing: 2px; padding: 5px 12px; border-radius: 20px; }
.conn-live { background: rgba(57,201,110,0.1); color: #39c96e; border: 1px solid rgba(57,201,110,0.25); }
.conn-off  { background: rgba(100,100,120,0.1); color: #666;    border: 1px solid rgba(100,100,120,0.2); }

.logout-btn {
  display: flex; align-items: center; gap: 6px; padding: 7px 14px;
  background: rgba(220,50,80,0.1); border: 1px solid rgba(220,50,80,0.35);
  border-radius: 6px; color: #f06080; font-size: 12px; font-weight: 600;
  cursor: pointer; transition: all 0.2s; font-family: var(--font-main, sans-serif);
}
.logout-btn:hover { background: rgba(220,50,80,0.2); }

.main-area { flex: 1; display: flex; align-items: center; justify-content: center; padding: 12px; min-height: 0; }

.https-warning {
  display: flex; gap: 16px; padding: 24px;
  background: rgba(197,74,239,0.06); border: 1px solid rgba(197,74,239,0.3); border-radius: 10px; max-width: 440px;
}
.warning-icon { font-size: 32px; }
.warning-title { font-size: 15px; font-weight: 700; color: #fff; margin-bottom: 8px; }
code { background: rgba(0,0,0,0.3); padding: 2px 8px; border-radius: 4px; color: #39c96e; font-size: 12px; }

.feed-wrapper {
  position: relative; width: 100%; max-width: 900px; aspect-ratio: 16/9;
  background: #000; border-radius: 6px; overflow: hidden;
  border: 1px solid rgba(197,74,239,0.2);
  box-shadow: 0 0 0 1px rgba(197,74,239,0.08), 0 20px 60px rgba(0,0,0,0.6);
}
.feed-video { width: 100%; height: 100%; object-fit: cover; display: block; }
.hidden-canvas { display: none; }

.overlay-msg {
  position: absolute; inset: 0; display: flex; flex-direction: column;
  align-items: center; justify-content: center; gap: 12px;
  background: rgba(4,4,20,0.88); backdrop-filter: blur(4px);
}
.overlay-admin { background: rgba(4,4,20,0.93); }
.overlay-icon { font-size: 36px; }
.overlay-title { font-size: 16px; font-weight: 700; color: #fff; }

.spinner-cam {
  width: 32px; height: 32px;
  border: 3px solid rgba(197,74,239,0.2); border-top-color: #c54aef;
  border-radius: 50%; animation: spin 0.9s linear infinite;
}
@keyframes spin { to { transform: rotate(360deg); } }

.stream-badge {
  position: absolute; top: 12px; left: 12px;
  display: flex; align-items: center; gap: 7px;
  padding: 4px 10px; background: rgba(220,50,80,0.85); border-radius: 4px;
  font-size: 10px; font-weight: 700; letter-spacing: 2px; color: #fff;
}
.rec-dot { width: 7px; height: 7px; border-radius: 50%; background: #fff; animation: blink 1s infinite; }
@keyframes blink { 0%,100%{opacity:1} 50%{opacity:0.2} }

.standby-badge {
  position: absolute; top: 12px; left: 12px;
  padding: 4px 10px;
  background: rgba(10,8,30,0.7); border: 1px solid rgba(197,74,239,0.3); border-radius: 4px;
  font-size: 10px; font-weight: 600; letter-spacing: 2px; color: rgba(197,74,239,0.8);
}

.scan-lines {
  position: absolute; inset: 0; pointer-events: none;
  background: repeating-linear-gradient(0deg, transparent, transparent 3px, rgba(0,0,0,0.04) 3px, rgba(0,0,0,0.04) 4px);
}
.corner { position: absolute; width: 18px; height: 18px; border-color: rgba(197,74,239,0.5); border-style: solid; }
.corner-tl { top:0; left:0; border-width:2px 0 0 2px; }
.corner-tr { top:0; right:0; border-width:2px 2px 0 0; }
.corner-bl { bottom:0; left:0; border-width:0 0 2px 2px; }
.corner-br { bottom:0; right:0; border-width:0 2px 2px 0; }

/* BOTTOM BAR */
.bottom-bar {
  display: flex; align-items: center; gap: 0;
  padding: 10px 20px;
  background: rgba(10,8,30,0.95); border-top: 1px solid rgba(197,74,239,0.2);
  flex-shrink: 0; flex-wrap: wrap; gap: 6px;
}
.control-group { display: flex; flex-direction: column; gap: 7px; }
.ctrl-label { font-size: 9px; font-weight: 700; letter-spacing: 3px; color: rgba(180,180,220,0.4); }
.ctrl-divider { width: 1px; height: 44px; background: rgba(197,74,239,0.15); margin: 0 16px; align-self: center; }

.fps-chips { display: flex; gap: 5px; }
.fps-chip {
  padding: 5px 11px; border-radius: 5px;
  border: 1px solid rgba(197,74,239,0.2); background: rgba(197,74,239,0.04);
  color: rgba(180,180,220,0.5); font-size: 13px; font-weight: 600;
  cursor: pointer; transition: all 0.2s; font-family: var(--font-main, sans-serif);
}
.fps-chip:hover { border-color: rgba(197,74,239,0.4); color: rgba(180,180,220,0.8); }
.fps-active { border-color: #c54aef !important; background: rgba(197,74,239,0.15) !important; color: #c54aef !important; }
.fps-unit { font-size: 9px; margin-left: 2px; opacity: 0.7; }

.stats-row { display: flex; gap: 18px; }
.stat-box { text-align: center; }
.stat-num { display: block; font-size: 17px; font-weight: 700; color: #fff; font-variant-numeric: tabular-nums; }
.stat-lbl { display: block; font-size: 9px; letter-spacing: 1px; color: rgba(180,180,220,0.4); margin-top: 1px; }

.stream-toggle {
  padding: 9px 20px; border-radius: 6px; border: 1px solid;
  font-size: 12px; font-weight: 700; letter-spacing: 1px;
  cursor: pointer; transition: all 0.25s; font-family: var(--font-main, sans-serif);
}
.stream-toggle:disabled { opacity: 0.35; cursor: not-allowed; }
.toggle-start { background: linear-gradient(135deg, #3d9fe6, #ca46fd); border-color: transparent; color: #fff; }
.toggle-start:hover:not(:disabled) { box-shadow: 0 4px 18px rgba(202,70,253,0.4); transform: translateY(-1px); }
.toggle-stop { background: rgba(220,50,80,0.1); border-color: rgba(220,50,80,0.4); color: #f06080; }
.toggle-stop:hover:not(:disabled) { background: rgba(220,50,80,0.2); }

.status-dot { width: 7px; height: 7px; border-radius: 50%; display: inline-block; }
.status-active { background: #39c96e; box-shadow: 0 0 6px rgba(57,201,110,0.7); animation: blink 2s infinite; }
.status-inactive { background: #444; }

.btn-primary {
  background: linear-gradient(135deg, #3d9fe6, #ca46fd);
  border: none; border-radius: 6px; color: #fff;
  font-size: 14px; font-weight: 600; padding: 10px 24px;
  cursor: pointer; font-family: var(--font-main, sans-serif);
}

@media (max-width: 600px) {
  .top-bar { padding: 8px 12px; }
  .main-area { padding: 8px; }
  .bottom-bar { padding: 8px 12px; }
  .ctrl-divider { display: none; }
}
</style>