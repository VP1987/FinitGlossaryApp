<script setup>
import { ref, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth.store'

const route = useRoute()
const router = useRouter()
const authStore = useAuthStore()

const token = ref('')
const password = ref('')
const confirmPassword = ref('')

onMounted(() => {
  token.value = route.query.token || ''
})

async function handleSubmit() {
  const ok = await authStore.resetPassword(token.value, password.value, confirmPassword.value)

  if (ok) {
    setTimeout(() => {
      authStore.clearMessages()
      router.push('/auth')
    }, 2000)
  }
}
</script>

<template>
  <div class="flex items-center justify-center min-h-screen bg-gray-100 px-4">
    <div class="bg-white p-8 rounded-xl shadow-md w-full max-w-md">
      <h2 class="text-2xl font-bold mb-4 text-center">Reset Password</h2>

      <div
        v-if="authStore.resetSuccess"
        class="bg-green-600 text-white font-bold text-sm rounded-lg px-4 py-2 mb-4 text-center"
      >
        Password updated successfully! Redirecting...
      </div>

      <form v-if="!authStore.resetSuccess" @submit.prevent="handleSubmit">
        <div class="mb-4">
          <label class="block mb-1 font-medium">New Password</label>
          <input
            type="password"
            v-model="password"
            required
            class="w-full border px-3 py-2 rounded-lg"
          />
        </div>

        <div class="mb-4">
          <label class="block mb-1 font-medium">Confirm Password</label>
          <input
            type="password"
            v-model="confirmPassword"
            required
            class="w-full border px-3 py-2 rounded-lg"
          />
        </div>

        <div
          v-if="authStore.error"
          class="bg-red-600 text-white font-bold text-sm rounded-lg px-4 py-2 mb-4 text-center"
        >
          {{ authStore.error }}
        </div>

        <button
          type="submit"
          :disabled="authStore.loading"
          class="w-full bg-purple-600 text-white py-2 rounded-lg font-semibold disabled:opacity-60"
        >
          <span v-if="!authStore.loading">Reset Password</span>
          <span v-else>Updating...</span>
        </button>
      </form>
    </div>
  </div>
</template>
