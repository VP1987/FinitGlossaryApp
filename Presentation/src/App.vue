<script setup>
import HeaderComponent from '@/components/common/HeaderComponent.vue'
</script>

<template>
  <div class="bg-gray-50 min-h-screen flex flex-col">
    <HeaderComponent />
    <main class="flex-grow **h-full**">
      <router-view />
    </main>
  </div>
</template>
