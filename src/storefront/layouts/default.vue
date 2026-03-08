<script setup lang="ts">
import { useCartStore } from '~/stores/cart'

const cart = useCartStore()
const menuOpen = ref(false)
</script>

<template>
  <div class="min-h-screen flex flex-col bg-cream-50">
    <!-- Header -->
    <header class="bg-white border-b border-cream-300 sticky top-0 z-50">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div class="flex items-center justify-between h-16">
          <!-- Logo -->
          <NuxtLink to="/" class="flex items-center gap-2">
            <img src="/logo.svg" alt="Oui Circular" class="h-10 w-10 rounded-full" />
            <span class="text-xl font-bold text-olive-600">Oui Circular</span>
          </NuxtLink>

          <!-- Desktop Nav -->
          <nav class="hidden md:flex items-center gap-8">
            <NuxtLink to="/produtos" class="text-sm font-medium text-gray-600 hover:text-olive-600 transition">
              Produtos
            </NuxtLink>
            <NuxtLink to="/sobre" class="text-sm font-medium text-gray-600 hover:text-olive-600 transition">
              Sobre
            </NuxtLink>
          </nav>

          <!-- Cart -->
          <div class="flex items-center gap-4">
            <NuxtLink to="/carrinho" class="relative p-2 text-gray-600 hover:text-olive-600 transition">
              <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M16 11V7a4 4 0 00-8 0v4M5 9h14l1 12H4L5 9z" />
              </svg>
              <span
                v-if="cart.count > 0"
                class="absolute -top-1 -right-1 bg-olive-500 text-white text-xs w-5 h-5 rounded-full flex items-center justify-center font-bold"
              >
                {{ cart.count }}
              </span>
            </NuxtLink>

            <!-- Mobile menu button -->
            <button class="md:hidden p-2" @click="menuOpen = !menuOpen">
              <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16M4 18h16" />
              </svg>
            </button>
          </div>
        </div>

        <!-- Mobile Nav -->
        <div v-if="menuOpen" class="md:hidden pb-4 border-t border-cream-200 mt-2 pt-2">
          <NuxtLink to="/produtos" class="block py-2 text-sm text-gray-600" @click="menuOpen = false">Produtos</NuxtLink>
          <NuxtLink to="/sobre" class="block py-2 text-sm text-gray-600" @click="menuOpen = false">Sobre</NuxtLink>
        </div>
      </div>
    </header>

    <!-- Main Content -->
    <main class="flex-1">
      <slot />
    </main>

    <!-- Footer -->
    <footer class="bg-white border-t border-cream-300 mt-auto">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
        <div class="grid grid-cols-1 md:grid-cols-3 gap-8">
          <div>
            <div class="flex items-center gap-2 mb-2">
              <img src="/logo.svg" alt="Oui Circular" class="h-8 w-8 rounded-full" />
              <h3 class="text-sm font-bold text-gray-900 uppercase tracking-wide">Oui Circular</h3>
            </div>
            <p class="mt-1 text-sm text-gray-500">Moda circular e sustentável em Portugal.</p>
          </div>
          <div>
            <h3 class="text-sm font-bold text-gray-900 uppercase tracking-wide">Contacto</h3>
            <p class="mt-2 text-sm text-gray-500">WhatsApp: +351 XXX XXX XXX</p>
            <p class="text-sm text-gray-500">Email: info@ouicircular.pt</p>
          </div>
          <div>
            <h3 class="text-sm font-bold text-gray-900 uppercase tracking-wide">Horário</h3>
            <p class="mt-2 text-sm text-gray-500">Seg-Sex: 10h - 19h</p>
            <p class="text-sm text-gray-500">Sáb: 10h - 14h</p>
          </div>
        </div>
        <div class="mt-8 pt-4 border-t border-cream-200 text-center text-xs text-gray-400">
          &copy; {{ new Date().getFullYear() }} Oui Circular. Todos os direitos reservados.
        </div>
      </div>
    </footer>
  </div>
</template>
