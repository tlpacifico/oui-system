<script setup lang="ts">
import { useCartStore } from '~/stores/cart'

const cart = useCartStore()
const { baseUrl } = useApi()

function photoUrl(path?: string) {
  if (!path) return ''
  const base = (baseUrl as string).replace('/api', '')
  return `${base}${path}`
}

useHead({ title: 'Carrinho - Oui Circular' })
</script>

<template>
  <div class="max-w-3xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <h1 class="text-2xl font-bold text-gray-900 mb-6">Carrinho</h1>

    <div v-if="cart.isEmpty" class="text-center py-20">
      <p class="text-lg text-gray-500 mb-4">O seu carrinho está vazio.</p>
      <NuxtLink
        to="/produtos"
        class="inline-block bg-olive-500 text-white px-6 py-3 rounded-lg font-semibold hover:bg-olive-600 transition"
      >
        Ver Produtos
      </NuxtLink>
    </div>

    <div v-else>
      <!-- Items -->
      <div class="space-y-4 mb-8">
        <div
          v-for="item in cart.items"
          :key="item.slug"
          class="bg-white border border-cream-300 rounded-lg p-4 flex items-center gap-4"
        >
          <div class="w-20 h-20 bg-cream-100 rounded-md overflow-hidden flex-shrink-0">
            <img
              v-if="item.primaryPhotoUrl"
              :src="photoUrl(item.primaryPhotoUrl)"
              :alt="item.title"
              class="w-full h-full object-cover"
            />
          </div>

          <div class="flex-1 min-w-0">
            <NuxtLink :to="`/produtos/${item.slug}`" class="text-sm font-medium text-gray-900 hover:text-olive-600 truncate block">
              {{ item.title }}
            </NuxtLink>
            <p class="text-xs text-gray-500">{{ item.brandName }} <span v-if="item.size">&middot; {{ item.size }}</span></p>
            <p class="text-sm font-bold text-gray-900 mt-1">&euro;{{ item.price.toFixed(2) }}</p>
          </div>

          <button
            class="text-gray-400 hover:text-red-500 transition p-2"
            @click="cart.remove(item.slug)"
          >
            <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
            </svg>
          </button>
        </div>
      </div>

      <!-- Summary -->
      <div class="bg-white border border-cream-300 rounded-lg p-6">
        <div class="flex justify-between items-center mb-4">
          <span class="text-gray-600">{{ cart.count }} artigo{{ cart.count !== 1 ? 's' : '' }}</span>
          <span class="text-xl font-bold text-gray-900">&euro;{{ cart.total.toFixed(2) }}</span>
        </div>

        <NuxtLink
          to="/checkout"
          class="block w-full bg-olive-500 text-white py-3 rounded-lg font-semibold text-center hover:bg-olive-600 transition"
        >
          Finalizar Encomenda
        </NuxtLink>

        <button
          class="block w-full text-sm text-gray-500 mt-3 hover:text-red-500 transition"
          @click="cart.clear()"
        >
          Limpar carrinho
        </button>
      </div>
    </div>
  </div>
</template>
