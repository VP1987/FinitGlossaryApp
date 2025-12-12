<script setup>
import { ref } from 'vue'
import { useAuthStore } from '@/stores/auth.store'

import LoginComponent from '@/components/auth/LoginComponent.vue'
import RegisterComponent from '@/components/auth/RegisterComponent.vue'
import ForgotPasswordComponent from '@/components/auth/ForgotPasswordComponent.vue'
import UpdateProfileComponent from '@/components/auth/UpdateProfileComponent.vue'

const authStore = useAuthStore()
const mode = ref('login')

const viewModel = ref({
  login: {
    email: '',
    password: '',
  },
  register: {
    username: '',
    email: '',
    password: '',
    confirmPassword: '',
  },
  forgot: {
    email: '',
  },
  updateProfile: {
    email: '',
    username: '',
    password: '',
    confirmPassword: '',
  },
})

function switchMode(m) {
  authStore.clearMessages()
  mode.value = m
}

async function handleLoginSubmit() {
  const { email, password } = viewModel.value.login
  const res = await authStore.login(email, password)

  if (!res) return

  if (res.flags?.mustChangePassword === true) {
    viewModel.value.updateProfile.email = res.email ?? email
    viewModel.value.updateProfile.username = res.username ?? ''
    mode.value = 'first-login'
    return
  }

  window.location.href = '/'
}

async function handleRegisterSubmit() {
  const vm = viewModel.value.register
  const res = await authStore.register(vm)

  if (!res) return
  viewModel.value.register = {
    username: '',
    email: '',
    password: '',
    confirmPassword: '',
  }
  setTimeout(() => {
    authStore.clearMessages()
    mode.value = 'login'
  }, 2000)
}

async function handleForgotSubmit() {
  const { email } = viewModel.value.forgot
  const res = await authStore.forgotPassword(email)

  if (!res) return
  viewModel.value.forgot = {
    email: '',
  }

  setTimeout(() => {
    authStore.clearMessages()
    mode.value = 'login'
  }, 2000)
}

async function handleUpdateProfileSubmit() {
  const vm = viewModel.value.updateProfile
  const res = await authStore.updateProfile(vm)

  if (!res) return
  mode.value = 'login'
}
</script>

<template>
  <div class="flex items-center justify-center min-h-[calc(100dvh-80px)] px-4">
    <div class="bg-white rounded-2xl shadow-lg w-full max-w-md mt-20 mb-20">
      <transition name="fade" mode="out-in">
        <!-- LOGIN -->
        <LoginComponent
          v-if="mode === 'login'"
          v-model:email="viewModel.login.email"
          v-model:password="viewModel.login.password"
          :loading="authStore.loading"
          :error="authStore.error"
          @submit-login="handleLoginSubmit"
          @go-forgot="switchMode('forgot')"
          @go-register="switchMode('register')"
        />

        <!-- REGISTER (UI postoji, logika kasnije) -->
        <RegisterComponent
          v-else-if="mode === 'register'"
          v-model:username="viewModel.register.username"
          v-model:email="viewModel.register.email"
          v-model:password="viewModel.register.password"
          v-model:confirmPassword="viewModel.register.confirmPassword"
          :loading="authStore.loading"
          :error="authStore.error"
          :success="authStore.success"
          @submit-register="handleRegisterSubmit"
          @go-login="switchMode('login')"
        />

        <!-- FORGOT PASSWORD -->
        <ForgotPasswordComponent
          v-else-if="mode === 'forgot'"
          v-model:emailForgot="viewModel.forgot.email"
          :loading="authStore.loading"
          :error="authStore.error"
          :success="authStore.success"
          @submit-forgot="handleForgotSubmit"
          @go-login="switchMode('login')"
        />

        <!-- FIRST LOGIN / UPDATE PROFILE -->
        <UpdateProfileComponent
          v-else-if="mode === 'first-login'"
          v-model:emailUpdateProfile="viewModel.updateProfile.email"
          v-model:userNameUpdateProfile="viewModel.updateProfile.username"
          v-model:passwordUpdateProfile="viewModel.updateProfile.password"
          v-model:confirmPasswordUpdateProfile="viewModel.updateProfile.confirmPassword"
          :loading="authStore.loading"
          :error="authStore.error"
          @submit-profileUpdate="handleUpdateProfileSubmit"
        />
      </transition>
    </div>
  </div>
</template>
