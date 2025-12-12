<script setup>
import { ref, computed } from 'vue'

const props = defineProps({
  emailUpdateProfile: String,
  passwordUpdateProfile: String,
  confirmPasswordUpdateProfile: String,
  userNameUpdateProfile: String,
  loading: Boolean,
  error: String,
})

const emit = defineEmits([
  'update:emailUpdateProfile',
  'update:passwordUpdateProfile',
  'update:confirmPasswordUpdateProfile',
  'update:userNameUpdateProfile',
  'submit-profileUpdate',
  'go-login',
])

const localError = ref('')

const modelEmail = computed({
  get: () => props.emailUpdateProfile,
  set: (val) => emit('update:emailUpdateProfile', val),
})

const modelPassword = computed({
  get: () => props.passwordUpdateProfile,
  set: (val) => emit('update:passwordUpdateProfile', val),
})

const modelConfirmPassword = computed({
  get: () => props.confirmPasswordUpdateProfile,
  set: (val) => emit('update:confirmPasswordUpdateProfile', val),
})

const modelUserName = computed({
  get: () => props.userNameUpdateProfile,
  set: (val) => emit('update:userNameUpdateProfile', val),
})

function handleSubmit() {
  localError.value = ''
  if (modelPassword.value !== modelConfirmPassword.value) {
    localError.value = 'Passwords do not match.'
    return
  }
  emit('submit-profileUpdate')
}
</script>

<template>
  <div>
    <div class="flex items-center justify-between px-6 py-4 border-b border-gray-200">
      <h2 class="text-lg font-semibold text-gray-900">Security profile update</h2>
    </div>

    <div class="p-6">
      <form @submit.prevent="handleSubmit" class="space-y-5">
        <div>
          <label class="block text-gray-700 mb-1">Email</label>
          <input
            v-model="modelEmail"
            type="email"
            class="w-full px-4 py-2 border rounded-lg"
            required
          />
        </div>

        <div>
          <label class="block text-gray-700 mb-1">Username</label>
          <input
            v-model="modelUserName"
            type="text"
            class="w-full px-4 py-2 border rounded-lg"
            required
          />
        </div>

        <div>
          <label class="block text-gray-700 mb-1">Password</label>
          <input
            v-model="modelPassword"
            type="password"
            class="w-full px-4 py-2 border rounded-lg"
            required
          />
        </div>

        <div>
          <label class="block text-gray-700 mb-1">Confirm password</label>
          <input
            v-model="modelConfirmPassword"
            type="password"
            class="w-full px-4 py-2 border rounded-lg"
            required
          />
        </div>

        <div v-if="localError" class="bg-red-600 text-white font-bold text-sm rounded-lg px-4 py-2">
          {{ localError }}
        </div>

        <div
          v-if="props.error"
          class="bg-red-600 text-white font-bold text-sm rounded-lg px-4 py-2"
        >
          {{ props.error }}
        </div>

        <div class="flex justify-end mt-6">
          <button
            type="submit"
            class="px-6 py-2 bg-purple-600 text-white rounded-lg disabled:opacity-60"
            :disabled="props.loading"
          >
            <span v-if="!props.loading">Update profile</span>
            <span v-else>Updating...</span>
          </button>
        </div>
      </form>
    </div>
  </div>
</template>
