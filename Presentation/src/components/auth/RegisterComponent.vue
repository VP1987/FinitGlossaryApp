<script setup>
import { computed } from 'vue'

const props = defineProps({
  username: String,
  email: String,
  password: String,
  confirmPassword: String,
  loading: Boolean,
  error: String,
  success: String,
})

const emit = defineEmits([
  'update:username',
  'update:email',
  'update:password',
  'update:confirmPassword',
  'submit-register',
  'go-login',
])

const modelUsername = computed({
  get: () => props.username,
  set: (v) => emit('update:username', v),
})

const modelEmail = computed({
  get: () => props.email,
  set: (v) => emit('update:email', v),
})

const modelPassword = computed({
  get: () => props.password,
  set: (v) => emit('update:password', v),
})

const modelConfirm = computed({
  get: () => props.confirmPassword,
  set: (v) => emit('update:confirmPassword', v),
})
</script>

<template>
  <div v-if="props.success">
    <div class="w-full max-w-md mx-auto mt-5 mb-5">
      <div class="flex items-center justify-center px-6 py-10">
        <div class="bg-green-600 text-white font-bold text-sm rounded-lg px-6 py-3 text-center">
          {{ props.success }}
        </div>
      </div>
    </div>
  </div>
  <div v-else>
    <div class="flex items-center justify-between px-6 py-4 border-b border-gray-200">
      <h2 class="text-lg font-semibold text-gray-900">Create Account</h2>
    </div>

    <div class="p-6">
      <form @submit.prevent="emit('submit-register')" class="space-y-5">
        <div>
          <label class="block mb-1 text-gray-700">Username</label>
          <input
            v-model="modelUsername"
            type="text"
            required
            class="w-full px-4 py-2 border rounded-lg"
          />
        </div>

        <div>
          <label class="block mb-1 text-gray-700">Email</label>
          <input
            v-model="modelEmail"
            type="email"
            required
            class="w-full px-4 py-2 border rounded-lg"
          />
        </div>

        <div>
          <label class="block mb-1 text-gray-700">Password</label>
          <input
            v-model="modelPassword"
            type="password"
            required
            class="w-full px-4 py-2 border rounded-lg"
          />
        </div>

        <div>
          <label class="block mb-1 text-gray-700">Confirm Password</label>
          <input
            v-model="modelConfirm"
            type="password"
            required
            class="w-full px-4 py-2 border rounded-lg"
          />
        </div>

        <div
          v-if="props.error"
          class="bg-red-600 text-white font-bold text-sm rounded-lg px-4 py-2"
        >
          {{ props.error }}
        </div>
        <div class="flex justify-between text-sm">
          <button
            type="button"
            @click="emit('go-login')"
            class="text-purple-600 hover:text-purple-800"
          >
            Already have an account?
          </button>
        </div>

        <div class="flex justify-end gap-3 mt-6">
          <button type="button" @click="emit('go-login')" class="px-4 py-2 bg-gray-200 rounded-lg">
            Cancel
          </button>

          <button
            type="submit"
            :disabled="props.loading"
            class="px-6 py-2 bg-purple-600 text-white font-semibold rounded-lg disabled:opacity-60"
          >
            <span v-if="!props.loading">Register</span>
            <span v-else>Registering...</span>
          </button>
        </div>
      </form>
    </div>
  </div>
</template>
