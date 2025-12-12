import { createRouter, createWebHistory } from 'vue-router'
import MainPage from '@/views/MainPage.vue'
import AuthPageView from '@/views/Auth/AuthPageView.vue'
import AdminPanelView from '@/views/Admin/AdminPanelView.vue'
import ResetPasswordPage from '@/views/Auth/ResetPasswordPage.vue'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    { path: '/', name: 'main', component: MainPage },
    { path: '/auth', name: 'auth', component: AuthPageView },
    {
      path: '/admin-panel',
      name: 'admin-panel',
      component: AdminPanelView,
      meta: { requiresAuth: true },
    },
    { path: '/reset-password', name: 'reset-password', component: ResetPasswordPage },
  ],
  scrollBehavior() {
    return { top: 0 }
  },
})

router.beforeEach((to, from, next) => {
  const token = localStorage.getItem('token')

  if (to.meta.requiresAuth && !token) {
    return next({ name: 'auth' })
  }

  if (to.name === 'auth' && token) {
    return next({ name: 'main' })
  }

  next()
})

export default router
