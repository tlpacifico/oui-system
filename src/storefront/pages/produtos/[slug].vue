<script setup lang="ts">
import type { ProductDetail } from '~/types'
import { useCartStore } from '~/stores/cart'

const { get, baseUrl } = useApi()
const cart = useCartStore()
const route = useRoute()
const slug = route.params.slug as string

const { data: product, error } = await useAsyncData(`product-${slug}`, () =>
  get<ProductDetail>(`/store/products/${slug}`)
)

const currentPhoto = ref(0)
const addedToCart = ref(false)

function photoUrl(path?: string) {
  if (!path) return ''
  const base = (baseUrl as string).replace('/api', '')
  return `${base}${path}`
}

function addToCart() {
  if (!product.value) return
  const added = cart.add(product.value)
  if (added) addedToCart.value = true
}

const conditionLabels: Record<string, string> = {
  Excellent: 'Excelente',
  VeryGood: 'Muito Bom',
  Good: 'Bom',
  Fair: 'Razoável',
  Poor: 'Fraco',
}

useHead({
  title: product.value ? `${product.value.title} - Oui Circular` : 'Produto - Oui Circular',
})
</script>

<template>
  <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <!-- Breadcrumb -->
    <nav class="text-sm text-gray-500 mb-6">
      <NuxtLink to="/produtos" class="hover:text-olive-600">Produtos</NuxtLink>
      <span class="mx-2">/</span>
      <span class="text-gray-900">{{ product?.title }}</span>
    </nav>

    <div v-if="error" class="text-center py-20">
      <p class="text-lg text-gray-500 mb-4">Produto não encontrado.</p>
      <NuxtLink to="/produtos" class="text-olive-600 hover:underline">Voltar aos produtos</NuxtLink>
    </div>

    <div v-else-if="product" class="grid grid-cols-1 md:grid-cols-2 gap-10">
      <!-- Photos -->
      <div>
        <div class="aspect-square bg-cream-100 rounded-lg overflow-hidden mb-3">
          <img
            v-if="product.photos?.length"
            :src="photoUrl(product.photos[currentPhoto]?.url)"
            :alt="product.title"
            class="w-full h-full object-cover"
          />
          <div v-else class="w-full h-full flex items-center justify-center text-cream-400">
            <svg xmlns="http://www.w3.org/2000/svg" class="h-20 w-20" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1" d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
            </svg>
          </div>
        </div>

        <!-- Thumbnails -->
        <div v-if="product.photos?.length > 1" class="flex gap-2 overflow-x-auto">
          <button
            v-for="(photo, idx) in product.photos"
            :key="idx"
            class="w-16 h-16 rounded-md overflow-hidden border-2 flex-shrink-0 transition"
            :class="idx === currentPhoto ? 'border-olive-500' : 'border-cream-300'"
            @click="currentPhoto = idx"
          >
            <img :src="photoUrl(photo.url)" :alt="`Foto ${idx + 1}`" class="w-full h-full object-cover" />
          </button>
        </div>
      </div>

      <!-- Details -->
      <div>
        <p class="text-sm text-gray-500 mb-1">{{ product.brandName }}</p>
        <h1 class="text-2xl font-bold text-gray-900 mb-2">{{ product.title }}</h1>
        <p class="text-3xl font-bold text-olive-600 mb-6">&euro;{{ product.price.toFixed(2) }}</p>

        <!-- Attributes -->
        <div class="space-y-3 mb-8">
          <div v-if="product.size" class="flex items-center text-sm">
            <span class="w-24 text-gray-500">Tamanho</span>
            <span class="font-medium text-gray-900">{{ product.size }}</span>
          </div>
          <div v-if="product.color" class="flex items-center text-sm">
            <span class="w-24 text-gray-500">Cor</span>
            <span class="font-medium text-gray-900">{{ product.color }}</span>
          </div>
          <div v-if="product.condition" class="flex items-center text-sm">
            <span class="w-24 text-gray-500">Condição</span>
            <span class="font-medium text-gray-900">{{ conditionLabels[product.condition] || product.condition }}</span>
          </div>
          <div v-if="product.composition" class="flex items-center text-sm">
            <span class="w-24 text-gray-500">Composição</span>
            <span class="font-medium text-gray-900">{{ product.composition }}</span>
          </div>
          <div v-if="product.categoryName" class="flex items-center text-sm">
            <span class="w-24 text-gray-500">Categoria</span>
            <span class="font-medium text-gray-900">{{ product.categoryName }}</span>
          </div>
        </div>

        <!-- Description -->
        <div v-if="product.description" class="mb-8">
          <h2 class="text-sm font-bold text-gray-900 mb-2">Descrição</h2>
          <p class="text-sm text-gray-600 leading-relaxed">{{ product.description }}</p>
        </div>

        <!-- Add to Cart -->
        <div>
          <button
            v-if="!cart.has(slug) && !addedToCart"
            class="w-full bg-olive-500 text-white py-3 rounded-lg font-semibold hover:bg-olive-600 transition text-base"
            @click="addToCart"
          >
            Adicionar ao Carrinho
          </button>
          <div v-else class="space-y-3">
            <div class="w-full bg-olive-50 text-olive-600 py-3 rounded-lg font-semibold text-center text-base">
              Adicionado ao carrinho
            </div>
            <NuxtLink
              to="/carrinho"
              class="block w-full border-2 border-olive-500 text-olive-600 py-3 rounded-lg font-semibold text-center hover:bg-olive-50 transition text-base"
            >
              Ver Carrinho
            </NuxtLink>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
