<template>
  <div class="login-page">
    <!-- Decorative orbs -->
    <div class="orb orb-1"></div>
    <div class="orb orb-2"></div>

    <div class="login-box cyber-card">
      <!-- Logo / Brand -->
      <div class="brand">
        <div class="brand-icon">
          <svg width="28" height="28" viewBox="0 0 24 24" fill="none">
            <path d="M12 2L2 7l10 5 10-5-10-5z" stroke="url(#g1)" stroke-width="1.8" stroke-linejoin="round"/>
            <path d="M2 17l10 5 10-5" stroke="url(#g1)" stroke-width="1.8" stroke-linejoin="round"/>
            <path d="M2 12l10 5 10-5" stroke="url(#g1)" stroke-width="1.8" stroke-linejoin="round"/>
            <defs>
              <linearGradient id="g1" x1="0" y1="0" x2="24" y2="24">
                <stop stop-color="#3d9fe6"/>
                <stop offset="1" stop-color="#ca46fd"/>
              </linearGradient>
            </defs>
          </svg>
        </div>
        <div>
          <div class="brand-name">AJAI <span class="text-grad">Security</span></div>
          <div class="brand-sub">AI Surveillance Management</div>
        </div>
      </div>

      <div class="divider-line"></div>

      <h2 class="login-title">Welcome Back</h2>
      <p class="login-desc">Sign in to access your security dashboard</p>

      <!-- Form -->
      <form @submit.prevent="handleLogin" class="login-form">
        <div class="field">
          <label class="field-label">Email Address</label>
          <div class="input-wrap">
            <span class="input-icon">
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <path d="M20 4H4a2 2 0 00-2 2v12a2 2 0 002 2h16a2 2 0 002-2V6a2 2 0 00-2-2z"/>
                <polyline points="22,6 12,13 2,6"/>
              </svg>
            </span>
            <input v-model="email" type="email" class="cyber-input with-icon" placeholder="admin@system.io" required />
          </div>
        </div>

        <div class="field">
          <label class="field-label">Password</label>
          <div class="input-wrap">
            <span class="input-icon">
              <svg width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
                <rect x="3" y="11" width="18" height="11" rx="2" ry="2"/>
                <path d="M7 11V7a5 5 0 0110 0v4"/>
              </svg>
            </span>
            <input v-model="password" type="password" class="cyber-input with-icon" placeholder="••••••••" required />
          </div>
        </div>

        <!-- Error -->
        <div v-if="errorMsg" class="error-msg">
          <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
            <circle cx="12" cy="12" r="10"/><line x1="12" y1="8" x2="12" y2="12"/><line x1="12" y1="16" x2="12.01" y2="16"/>
          </svg>
          {{ errorMsg }}
        </div>

        <button type="submit" class="btn-primary login-btn" :disabled="loading">
          <span v-if="!loading">Sign In →</span>
          <span v-else class="loader-wrap">
            <span class="spinner"></span> Authenticating...
          </span>
        </button>
      </form>

      <!-- Footer note -->
      <div class="login-footer">
        <span class="status-dot status-active"></span>
        <span class="footer-note">All systems operational</span>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../stores/auth'

const router = useRouter()
const auth = useAuthStore()

const email = ref('')
const password = ref('')
const loading = ref(false)
const errorMsg = ref('')

async function handleLogin() {
  loading.value = true
  errorMsg.value = ''
  try {
    const data = await auth.login(email.value, password.value)
    router.push(data.role === 'User' ? '/dashboard' : '/simulator')
  } catch (err) {
    errorMsg.value = err.response?.data || 'Authentication failed. Check your credentials.'
  } finally {
    loading.value = false
  }
}
</script>

<style scoped>
.login-page {
  min-height: 100vh;
  display: flex;
  align-items: center;
  justify-content: center;
  padding: 24px;
  position: relative;
  overflow: hidden;
}

/* Decorative blobs */
.orb {
  position: fixed;
  border-radius: 50%;
  filter: blur(80px);
  pointer-events: none;
  z-index: 0;
}
.orb-1 {
  width: 400px; height: 400px;
  background: rgba(197, 74, 239, 0.12);
  top: -100px; left: -100px;
}
.orb-2 {
  width: 350px; height: 350px;
  background: rgba(61, 159, 230, 0.1);
  bottom: -80px; right: -80px;
}

.login-box {
  width: 100%;
  max-width: 440px;
  padding: 40px 36px;
  position: relative;
  z-index: 1;
  animation: appear 0.5s ease;
}

@keyframes appear {
  from { opacity: 0; transform: translateY(24px); }
  to   { opacity: 1; transform: translateY(0); }
}

/* Brand */
.brand {
  display: flex;
  align-items: center;
  gap: 14px;
  margin-bottom: 24px;
}
.brand-icon {
  width: 48px; height: 48px;
  background: linear-gradient(135deg, rgba(61,159,230,0.15), rgba(202,70,253,0.15));
  border: 1px solid rgba(197,74,239,0.3);
  border-radius: 10px;
  display: flex; align-items: center; justify-content: center;
}
.brand-name { font-size: 20px; font-weight: 700; color: #fff; }
.brand-sub  { font-size: 12px; color: var(--text-dim); }

.divider-line {
  height: 1px;
  background: linear-gradient(90deg, transparent, rgba(197,74,239,0.4), rgba(61,159,230,0.4), transparent);
  margin-bottom: 24px;
}

.login-title { font-size: 24px; margin-bottom: 6px; }
.login-desc  { font-size: 14px; color: var(--text-dim); margin-bottom: 28px; }

/* Form */
.login-form { display: flex; flex-direction: column; gap: 18px; }

.field { display: flex; flex-direction: column; gap: 7px; }
.field-label { font-size: 13px; font-weight: 600; color: #c8c8e8; }

.input-wrap { position: relative; }
.input-icon {
  position: absolute;
  left: 14px; top: 50%; transform: translateY(-50%);
  color: var(--text-dim);
  display: flex; align-items: center;
  pointer-events: none;
}
.with-icon { padding-left: 42px !important; }

.error-msg {
  display: flex;
  align-items: center;
  gap: 8px;
  padding: 10px 14px;
  background: rgba(220,50,80,0.08);
  border: 1px solid rgba(220,50,80,0.3);
  border-radius: var(--radius-sm);
  color: #f07080;
  font-size: 13px;
}

.login-btn { width: 100%; padding: 13px; font-size: 15px; margin-top: 4px; }

.loader-wrap { display: flex; align-items: center; gap: 10px; }
.spinner {
  width: 16px; height: 16px;
  border: 2px solid rgba(255,255,255,0.3);
  border-top-color: #fff;
  border-radius: 50%;
  animation: spin 0.8s linear infinite;
}
@keyframes spin { to { transform: rotate(360deg); } }

.login-footer {
  display: flex;
  align-items: center;
  gap: 8px;
  margin-top: 24px;
  padding-top: 18px;
  border-top: 1px solid var(--border-dim);
}
.footer-note { font-size: 12px; color: var(--text-dim); }
</style>