import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '../stores/auth'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: '/',
      redirect: '/login'
    },
    {
      path: '/login',
      name: 'Login',
      component: () => import('../views/LoginView.vue'),
      meta: { guest: true }
    },
    {
      path: '/dashboard',
      name: 'Dashboard',
      component: () => import('../views/DashboardView.vue'),
      meta: { requiresAuth: true, role: 'User' }
    },
    {
      path: '/simulator',
      name: 'CameraSimulator',
      component: () => import('../views/CameraSimulatorView.vue'),
      meta: { requiresAuth: true, role: 'Camera' }
    },
    {
      path: '/:pathMatch(.*)*',
      redirect: '/login'
    }
  ]
})

router.beforeEach((to, _from, next) => {
  const auth = useAuthStore()

  // لو سجّل دخول وحاول يفتح صفحة الضيف (Login)
  if (to.meta.guest && auth.isAuthenticated) {
    return next(auth.isAdmin ? '/dashboard' : '/simulator')
  }

  // لو محتاج تسجيل دخول
  if (to.meta.requiresAuth && !auth.isAuthenticated) {
    return next('/login')
  }

  // لو صلاحيته غلط (User يحاول يفتح /simulator مثلاً)
  if (to.meta.role && auth.role !== to.meta.role) {
    return next(auth.isAdmin ? '/dashboard' : '/simulator')
  }

  next()
})

export default router