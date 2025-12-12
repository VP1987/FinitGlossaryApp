<script setup>
import { computed } from 'vue'
import BaseSpinner from '@/components/common/BaseSpinner.vue'

const props = defineProps({
  email: String,
  password: String,
  loading: Boolean,
  error: String,
})

const emit = defineEmits([
  'update:email',
  'update:password',
  'submit-login',
  'go-register',
  'go-forgot',
])

const modelEmail = computed({
  get: () => props.email,
  set: (val) => emit('update:email', val),
})

const modelPassword = computed({
  get: () => props.password,
  set: (val) => emit('update:password', val),
})
</script>

<template>
  <div>
    <div class="flex items-center justify-between px-6 py-4 border-b border-gray-200">
      <h2 class="text-lg font-semibold text-gray-900">Sign In</h2>
    </div>

    <div class="p-6">
      <form @submit.prevent="emit('submit-login')" class="space-y-5">
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

        <div>
          <label class="block text-gray-700 mb-1">Password</label>
          <input
            v-model="modelPassword"
            type="password"
            required
            class="w-full px-4 py-2 border rounded-lg"
            placeholder="••••••••"
          />
        </div>

        <div
          v-if="props.error"
          class="mt-1 bg-red-600 text-white font-bold text-sm rounded-lg px-4 py-2"
        >
          {{ props.error }}
        </div>

        <div class="flex justify-between text-sm">
          <button
            type="button"
            @click="emit('go-register')"
            class="text-purple-600 hover:text-purple-800 font-medium"
          >
            Create account
          </button>

          <button
            type="button"
            @click="emit('go-forgot')"
            class="text-purple-600 hover:text-purple-800 font-medium"
          >
            Forgot password?
          </button>
        </div>

        <div class="flex justify-end mt-6">
          <button
            type="submit"
            class="relative px-6 py-2 bg-purple-600 text-white rounded-lg hover:bg-purple-700 font-semibold flex items-center justify-center"
            :disabled="props.loading"
          >
            <div v-if="props.loading" class="absolute inset-0 flex items-center justify-center">
              <BaseSpinner />
            </div>

            <span :class="{ 'opacity-0': props.loading }">Sign In</span>
          </button>
        </div>
      </form>
    </div>
  </div>
</template>
