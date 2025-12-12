import api from '@/services/api/apiService.js'

export const AuthController = {
  async login(email, password) {
    const { data } = await api.post('/Auth/login', { email, password })
    return data
  },

  async register(username, email, password) {
    const { data } = await api.post('/Auth/register', { username, email, password })
    return data
  },

  async requestPasswordReset(email) {
    const { data } = await api.post('/Auth/reset-password/request', { email })
    return data
  },

  async confirmPasswordReset(token, newPassword) {
    const { data } = await api.post('/Auth/reset-password/confirm', { token, newPassword })
    return data
  },
  async finishFirstLogin(payload) {
    const { data } = await api.post('/Auth/complete-profile-update', payload)
    return data
  },
}
