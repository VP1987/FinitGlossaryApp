<script setup>
import { computed } from 'vue'
import BaseSpinner from '@/components/common/BaseSpinner.vue'

const props = defineProps({
  emailForgot: String,
  loading: Boolean,
  error: String,
  success: String,
})

const emit = defineEmits(['update:emailForgot', 'submit-forgot', 'go-login'])

const modelEmail = computed({
  get: () => props.emailForgot,
  set: (val) => emit('update:emailForgot', val),
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
      <h2 class="text-lg font-semibold text-gray-900">Reset Password</h2>
    </div>

    <div class="p-6">
      <p class="text-gray-600 text-center mb-6">
        Enter your email address and we’ll send you a link to reset your password.
      </p>

      <form class="space-y-5" @submit.prevent="emit('submit-forgot')">
        <div>
          <label class="block text-gray-700 mb-1">Email</label>
          <input
            v-model="modelEmail"
            type="email"
            required
            class="w-full px-4 py-2 border rounded-lg"
            placeholder="you@example.com"
          />
        </div>

        <div
          v-if="props.error"
          class="bg-red-600 text-white font-bold text-sm rounded-lg px-4 py-2 mt-1"
        >
          {{ props.error }}
        </div>

        <div class="flex justify-end gap-3 mt-6">
          <button
            type="button"
            @click="emit('go-login')"
            class="px-4 py-2 bg-gray-200 text-gray-700 rounded-lg hover:bg-gray-300"
          >
            Cancel
          </button>

          <button
            type="submit"
            class="relative px-6 py-2 bg-purple-600 text-white rounded-lg hover:bg-purple-700 font-semibold flex items-center justify-center"
            :disabled="props.loading"
          >
            <div v-if="props.loading" class="absolute inset-0 flex items-center justify-center">
              <BaseSpinner />
            </div>

            <span :class="{ 'opacity-0': props.loading }">Send Reset Link</span>
          </button>
        </div>

        <p class="text-center text-sm text-gray-500 mt-3">
          Remembered your password?
          <button
            type="button"
            @click="emit('go-login')"
            class="text-purple-600 hover:text-purple-800 font-medium"
          >
            Back to login
          </button>
        </p>
      </form>
    </div>
  </div>
</template>
