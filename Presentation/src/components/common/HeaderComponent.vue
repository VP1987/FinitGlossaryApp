<script setup>
import { ref, onMounted } from 'vue'
import { User } from 'lucide-vue-next'
import { useRouter } from 'vue-router'

const router = useRouter()
const showProfileMenu = ref(false)
const username = ref('')
const loggedIn = ref(false)

onMounted(() => {
  username.value = localStorage.getItem('username') || ''
  loggedIn.value = !!localStorage.getItem('token')
})

const goTo = (path) => {
  showProfileMenu.value = false
  router.push(path)
}

const logout = () => {
  localStorage.removeItem('token')
  localStorage.removeItem('username')
  username.value = ''
  loggedIn.value = false
  router.push('/')
}

const toggleProfileMenu = () => (showProfileMenu.value = !showProfileMenu.value)

document.addEventListener('click', (e) => {
  const menu = document.getElementById('profile-menu')
  const btn = document.getElementById('profile-button')
  if (menu && btn && !menu.contains(e.target) && !btn.contains(e.target)) {
    showProfileMenu.value = false
  }
})
</script>

<template>
  <header class="bg-white shadow-md sticky top-0 z-50">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
      <div class="flex justify-between items-center h-20">
        <div class="flex flex-col cursor-pointer select-none" @click="goTo('/')">
          <h1
            class="text-3xl font-extrabold bg-gradient-to-r from-purple-600 via-pink-600 to-blue-600 bg-clip-text text-transparent tracking-tight leading-tight"
          >
            Glossary
          </h1>
          <p class="text-gray-500 text-sm font-medium mt-0.5">
            Explore definitions and expand your knowledge
          </p>
        </div>

        <div v-if="loggedIn" class="relative">
          <button
            id="profile-button"
            @click.stop="toggleProfileMenu"
            class="p-2 rounded-full hover:bg-purple-100 transition"
          >
            <User class="w-6 h-6 text-purple-600" />
          </button>

          <transition name="fade">
            <div
              v-if="showProfileMenu"
              id="profile-menu"
              class="absolute right-0 mt-2 w-52 bg-white rounded-xl shadow-lg border border-gray-200 py-2 z-50"
            >
              <div
                class="px-4 py-2 text-center font-semibold text-gray-700 border-b border-gray-100 text-base"
              >
                {{ username }}
              </div>

              <button
                @click="goTo('/admin')"
                class="block w-full text-left px-4 py-2 text-sm text-purple-600 hover:bg-purple-50"
              >
                Admin Panel
              </button>

              <button
                @click="logout"
                class="block w-full text-left px-4 py-2 text-sm text-red-600 hover:bg-red-50"
              >
                Logout
              </button>
            </div>
          </transition>
        </div>

        <div v-else>
          <button
            @click="goTo('/auth')"
            class="px-4 py-2 rounded-md font-medium text-gray-700 hover:text-white hover:bg-purple-600 transition"
          >
            Sign In
          </button>
        </div>
      </div>
    </div>
  </header>
</template>

<style scoped>
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.25s ease;
}
.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}
</style>
