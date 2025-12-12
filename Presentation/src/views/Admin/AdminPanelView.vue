<script setup>
import { useAuthStore } from '@/stores/auth.store'
import { useRouter } from 'vue-router'

const authStore = useAuthStore()
const router = useRouter()

function logout() {
  authStore.logout()
  router.push('/auth')
}
</script>

<template>
  <div class="min-h-screen bg-gray-100 p-8">
    <div class="max-w-5xl mx-auto bg-white rounded-xl shadow p-6">
      <div class="flex justify-between items-center mb-6">
        <h1 class="text-2xl font-bold text-gray-900">Admin Panel</h1>

        <button
          @click="logout"
          class="px-4 py-2 bg-red-600 text-white rounded-lg hover:bg-red-700 font-semibold"
        >
          Logout
        </button>
      </div>

      <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
        <div class="p-4 rounded-lg border">
          <h2 class="font-semibold text-gray-800 mb-2">User</h2>
          <p class="text-sm text-gray-600">ID: {{ authStore.userId }}</p>
          <p class="text-sm text-gray-600">Username: {{ authStore.username }}</p>
        </div>

        <div class="p-4 rounded-lg border">
          <h2 class="font-semibold text-gray-800 mb-2">Status</h2>
          <p class="text-sm text-green-600 font-medium">Authenticated</p>
        </div>

        <div class="p-4 rounded-lg border">
          <h2 class="font-semibold text-gray-800 mb-2">Actions</h2>
          <p class="text-sm text-gray-600">Admin actions go here</p>
        </div>
      </div>
    </div>
  </div>
</template>
