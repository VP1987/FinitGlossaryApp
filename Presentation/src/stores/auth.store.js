import { defineStore } from 'pinia'
import { ref } from 'vue'
import { AuthController } from '@/services/controllers/auth/authController'
import { getUserIdFromToken, getUsernameFromToken } from '@/utils/auth/getDataFromToken'

export const useAuthStore = defineStore('auth', () => {
  const token = ref(null)
  const userId = ref(null)
  const username = ref(null)

  const mustChangePassword = ref(false)

  const loading = ref(false)
  const error = ref(null)
  const success = ref(null)
  const resetSuccess = ref(false)

  async function register(vm) {
    loading.value = true
    error.value = null
    success.value = null
    try {
      const { username, email, password, confirmPassword } = vm

      if (!username || !email || !password) {
        error.value = 'All fields are required.'
        return null
      }

      if (password !== confirmPassword) {
        error.value = 'Passwords do not match.'
        return null
      }
      success.value = 'Registration successful. You can now log in.'
      return await AuthController.register(username, email, password)
    } catch (e) {
      error.value = e?.response?.data?.message || 'Registration failed.'
      return null
    } finally {
      loading.value = false
    }
  }

  async function login(email, password) {
    loading.value = true
    error.value = null

    try {
      const res = await AuthController.login(email, password)

      token.value = res.token
      userId.value = Number(getUserIdFromToken(res.token))
      username.value = getUsernameFromToken(res.token)

      mustChangePassword.value =
        res.flags?.mustChangePassword === true || res.mustChangePassword === true

      localStorage.setItem('token', token.value)
      localStorage.setItem('userId', String(userId.value))
      localStorage.setItem('username', username.value)

      return res
    } catch (e) {
      error.value = e?.response?.data?.message || 'Login failed'
      return null
    } finally {
      loading.value = false
    }
  }

  async function forgotPassword(email) {
    loading.value = true
    error.value = null
    success.value = null

    try {
      await AuthController.requestPasswordReset(email)
      success.value = 'Email with reset link sent.'
      return true
    } catch (e) {
      error.value = e?.response?.data?.message || 'Failed to send reset link.'
      return false
    } finally {
      loading.value = false
    }
  }

  async function resetPassword(resetToken, password, confirmPassword) {
    error.value = null
    resetSuccess.value = false

    if (password !== confirmPassword) {
      error.value = 'Passwords do not match.'
      return false
    }

    loading.value = true

    try {
      await AuthController.confirmPasswordReset(resetToken, password)
      resetSuccess.value = true
      return true
    } catch (e) {
      error.value = e?.response?.data?.message || 'Failed to reset password.'
      return false
    } finally {
      loading.value = false
    }
  }

  async function updateProfile(vm) {
    error.value = null
    loading.value = true

    try {
      if (!vm.email || !vm.username) {
        error.value = 'Email and username are required.'
        return null
      }

      if (vm.password !== vm.confirmPassword) {
        error.value = 'Passwords do not match.'
        return null
      }

      const payload = {
        userId: userId.value,
        newEmail: vm.email,
        newUsername: vm.username,
        newPassword: vm.password,
      }

      const res = await AuthController.finishFirstLogin(payload)

      mustChangePassword.value = false

      return res
    } catch (e) {
      error.value = e?.response?.data?.message || 'Update failed.'
      return null
    } finally {
      loading.value = false
    }
  }

  function clearMessages() {
    error.value = null
    success.value = null
  }

  function logout() {
    token.value = null
    userId.value = null
    username.value = null
    mustChangePassword.value = false
    error.value = null
    resetSuccess.value = false
    localStorage.clear()
  }

  return {
    token,
    userId,
    username,
    mustChangePassword,
    loading,
    error,
    success,
    resetSuccess,
    register,
    login,
    forgotPassword,
    resetPassword,
    updateProfile,
    clearMessages,
    logout,
  }
})
