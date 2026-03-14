<script setup lang="ts">
import { useCartStore } from '~/stores/cart'

const cart = useCartStore()
const menuOpen = ref(false)
</script>

<template>
  <div class="min-h-screen flex flex-col bg-cream">
    <!-- Announcement Bar -->
    <div class="bg-dark text-white text-center py-2.5 px-4 text-xs tracking-[1.5px] uppercase">
      Envio gratuito em encomendas acima de 50&euro; &middot; Moda sustentável feita em Lisboa
    </div>

    <!-- Header -->
    <header class="bg-white border-b border-black/[0.06] sticky top-0 z-50">
      <div class="px-[6%] py-4 grid grid-cols-[1fr_auto_1fr] items-center">
        <!-- Left Nav (Desktop) -->
        <nav class="hidden md:flex gap-8">
          <NuxtLink to="/produtos" class="nav-link text-dark text-[13px] font-medium uppercase tracking-[1.5px] relative pb-0.5">
            Comprar
          </NuxtLink>
          <NuxtLink to="/sobre" class="nav-link text-dark text-[13px] font-medium uppercase tracking-[1.5px] relative pb-0.5">
            Sobre
          </NuxtLink>
        </nav>
        <div class="md:hidden"></div>

        <!-- Center Logo -->
        <NuxtLink to="/" class="justify-self-center">
          <img src="/logo-horizontal.png" alt="Oui Circular" class="h-[42px] block" />
        </NuxtLink>

        <!-- Right Icons -->
        <div class="flex items-center justify-end gap-6">
          <NuxtLink to="/produtos" class="hidden md:flex items-center gap-1.5 text-dark text-[13px] font-medium tracking-[0.5px]">
            <svg class="w-5 h-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
              <circle cx="11" cy="11" r="8" /><line x1="21" y1="21" x2="16.65" y2="16.65" />
            </svg>
          </NuxtLink>
          <NuxtLink to="/carrinho" class="flex items-center gap-1.5 text-dark text-[13px] font-medium tracking-[0.5px] relative">
            <svg class="w-5 h-5" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="1.5">
              <path d="M6 2L3 6v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2V6l-3-4z" /><line x1="3" y1="6" x2="21" y2="6" /><path d="M16 10a4 4 0 0 1-8 0" />
            </svg>
            <span>({{ cart.count }})</span>
          </NuxtLink>

          <!-- Mobile menu button -->
          <button class="md:hidden p-1" @click="menuOpen = !menuOpen">
            <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="1.5">
              <path v-if="!menuOpen" stroke-linecap="round" stroke-linejoin="round" d="M4 6h16M4 12h16M4 18h16" />
              <path v-else stroke-linecap="round" stroke-linejoin="round" d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
        </div>
      </div>

      <!-- Mobile Nav -->
      <div v-if="menuOpen" class="md:hidden border-t border-cream-dark px-[6%] py-4 space-y-3">
        <NuxtLink to="/produtos" class="block text-sm text-dark uppercase tracking-wider" @click="menuOpen = false">Comprar</NuxtLink>
        <NuxtLink to="/sobre" class="block text-sm text-dark uppercase tracking-wider" @click="menuOpen = false">Sobre</NuxtLink>
      </div>
    </header>

    <!-- Main Content -->
    <main class="flex-1">
      <slot />
    </main>

    <!-- Footer -->
    <footer class="bg-dark text-white/70 pt-16 pb-8 px-[6%]">
      <div class="max-w-7xl mx-auto">
        <div class="grid grid-cols-1 md:grid-cols-[2fr_1fr_1fr_1fr] gap-10 mb-10">
          <!-- Brand -->
          <div>
            <img src="/logo-horizontal.png" alt="Oui Circular" class="h-9 brightness-0 invert mb-4" />
            <p class="text-sm leading-relaxed max-w-[280px]">Consignação de moda consciente no coração de Lisboa. Cada peça conta uma história.</p>
          </div>

          <!-- Loja -->
          <div>
            <h4 class="font-sans text-xs font-semibold tracking-[2px] uppercase text-white mb-5">Loja</h4>
            <NuxtLink to="/produtos?sort=newest" class="block text-sm text-white/60 hover:text-white transition mb-2.5">Novidades</NuxtLink>
            <NuxtLink to="/produtos" class="block text-sm text-white/60 hover:text-white transition mb-2.5">Catálogo</NuxtLink>
            <NuxtLink to="/sobre" class="block text-sm text-white/60 hover:text-white transition mb-2.5">Sobre Nós</NuxtLink>
          </div>

          <!-- Vender -->
          <div>
            <h4 class="font-sans text-xs font-semibold tracking-[2px] uppercase text-white mb-5">Vender</h4>
            <NuxtLink to="/sobre" class="block text-sm text-white/60 hover:text-white transition mb-2.5">Como Funciona</NuxtLink>
            <span class="block text-sm text-white/60 mb-2.5">FAQ</span>
          </div>

          <!-- Contacto -->
          <div>
            <h4 class="font-sans text-xs font-semibold tracking-[2px] uppercase text-white mb-5">Contacto</h4>
            <span class="block text-sm text-white/60 mb-2.5">R. Afonso Lopes Vieira 54A, 1700-264 Lisboa</span>
            <span class="block text-sm text-white/60 mb-2.5">oui.circular@gmail.com</span>
          </div>
        </div>

        <!-- Bottom bar -->
        <div class="border-t border-white/10 pt-6 flex flex-col md:flex-row justify-between items-center gap-3 text-[13px]">
          <span>&copy; {{ new Date().getFullYear() }} Oui Circular. Todos os direitos reservados.</span>
          <div class="flex gap-4">
            <a href="https://www.instagram.com/oui.circular/" target="_blank" rel="noopener" class="text-white/60 hover:text-white transition">Instagram</a>
            <a href="https://linktr.ee/ouicircular" target="_blank" rel="noopener" class="text-white/60 hover:text-white transition">Linktr.ee</a>
          </div>
        </div>
      </div>
    </footer>
  </div>
</template>

<style scoped>
.nav-link::after {
  content: '';
  position: absolute;
  bottom: 0;
  left: 0;
  width: 0;
  height: 1.5px;
  background: #8A9A5B;
  transition: width 0.3s ease;
}
.nav-link:hover::after {
  width: 100%;
}
</style>
