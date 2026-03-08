<script setup lang="ts">
import type { Product, ProductsResponse } from '~/types'

const { get, baseUrl } = useApi()

const { data: featured } = await useAsyncData('featured', () =>
  get<ProductsResponse>('/store/products', { pageSize: 8, sort: 'newest' })
)

const { data: brands } = await useAsyncData('brands', () =>
  get<string[]>('/store/brands')
)

function photoUrl(path?: string) {
  if (!path) return ''
  const base = (baseUrl as string).replace('/api', '')
  return `${base}${path}`
}
</script>

<template>
  <div>
    <!-- Hero -->
    <section class="bg-emerald-700 text-white">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-20 text-center">
        <h1 class="text-4xl md:text-5xl font-bold mb-4">Moda Circular & Sustentável</h1>
        <p class="text-lg md:text-xl text-emerald-100 mb-8 max-w-2xl mx-auto">
          Peças únicas de segunda mão com qualidade. Dê uma nova vida à moda.
        </p>
        <NuxtLink
          to="/produtos"
          class="inline-block bg-white text-emerald-700 px-8 py-3 rounded-lg font-semibold hover:bg-emerald-50 transition"
        >
          Ver Produtos
        </NuxtLink>
      </div>
    </section>

    <!-- Featured Products -->
    <section class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-16">
      <h2 class="text-2xl font-bold text-gray-900 mb-8">Novidades</h2>

      <div v-if="featured?.items?.length" class="grid grid-cols-2 md:grid-cols-4 gap-6">
        <NuxtLink
          v-for="product in featured.items"
          :key="product.slug"
          :to="`/produtos/${product.slug}`"
          class="group"
        >
          <div class="aspect-square bg-gray-100 rounded-lg overflow-hidden mb-3">
            <img
              v-if="product.primaryPhotoUrl"
              :src="photoUrl(product.primaryPhotoUrl)"
              :alt="product.title"
              class="w-full h-full object-cover group-hover:scale-105 transition duration-300"
            />
            <div v-else class="w-full h-full flex items-center justify-center text-gray-300">
              <svg xmlns="http://www.w3.org/2000/svg" class="h-12 w-12" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1" d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
              </svg>
            </div>
          </div>
          <h3 class="text-sm font-medium text-gray-900 group-hover:text-emerald-700 transition">{{ product.title }}</h3>
          <p class="text-xs text-gray-500">{{ product.brandName }}</p>
          <p class="text-sm font-bold text-gray-900 mt-1">&euro;{{ product.price.toFixed(2) }}</p>
        </NuxtLink>
      </div>

      <div v-else class="text-center py-12 text-gray-500">
        Nenhum produto disponível de momento.
      </div>

      <div class="text-center mt-10">
        <NuxtLink
          to="/produtos"
          class="inline-block border-2 border-emerald-700 text-emerald-700 px-8 py-3 rounded-lg font-semibold hover:bg-emerald-50 transition"
        >
          Ver Todos os Produtos
        </NuxtLink>
      </div>
    </section>

    <!-- Brands -->
    <section v-if="brands?.length" class="bg-white border-t border-gray-100">
      <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
        <h2 class="text-lg font-bold text-gray-900 mb-6 text-center">Marcas Disponíveis</h2>
        <div class="flex flex-wrap justify-center gap-4">
          <NuxtLink
            v-for="brand in brands"
            :key="brand"
            :to="`/produtos?brand=${encodeURIComponent(brand)}`"
            class="px-4 py-2 bg-gray-50 rounded-full text-sm text-gray-700 hover:bg-emerald-50 hover:text-emerald-700 transition"
          >
            {{ brand }}
          </NuxtLink>
        </div>
      </div>
    </section>
  </div>
</template>
