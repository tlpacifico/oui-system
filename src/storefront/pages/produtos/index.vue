<script setup lang="ts">
import type { ProductsResponse } from '~/types'
import { useCartStore } from '~/stores/cart'

const { get, baseUrl } = useApi()
const cart = useCartStore()
const route = useRoute()
const router = useRouter()

// Filters from query params
const search = ref((route.query.search as string) || '')
const brand = ref((route.query.brand as string) || '')
const category = ref((route.query.category as string) || '')
const size = ref((route.query.size as string) || '')
const sort = ref((route.query.sort as string) || 'newest')
const page = ref(Number(route.query.page) || 1)

const { data: brands } = await useAsyncData('filter-brands', () => get<string[]>('/store/brands'))
const { data: categories } = await useAsyncData('filter-categories', () => get<string[]>('/store/categories'))

const { data: products, refresh, status } = await useAsyncData(
  'products',
  () => get<ProductsResponse>('/store/products', {
    search: search.value || undefined,
    brand: brand.value || undefined,
    category: category.value || undefined,
    size: size.value || undefined,
    sort: sort.value,
    page: page.value,
    pageSize: 20,
  }),
  { watch: [page] }
)

function applyFilters() {
  page.value = 1
  updateQuery()
  refresh()
}

function clearFilters() {
  search.value = ''
  brand.value = ''
  category.value = ''
  size.value = ''
  sort.value = 'newest'
  page.value = 1
  updateQuery()
  refresh()
}

function updateQuery() {
  router.replace({
    query: {
      ...(search.value && { search: search.value }),
      ...(brand.value && { brand: brand.value }),
      ...(category.value && { category: category.value }),
      ...(size.value && { size: size.value }),
      ...(sort.value !== 'newest' && { sort: sort.value }),
      ...(page.value > 1 && { page: String(page.value) }),
    },
  })
}

function goToPage(p: number) {
  page.value = p
  updateQuery()
}

function photoUrl(path?: string) {
  if (!path) return ''
  const base = (baseUrl as string).replace('/api', '')
  return `${base}${path}`
}

const sizes = ['XS', 'S', 'M', 'L', 'XL', 'XXL']
</script>

<template>
  <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <h1 class="text-2xl font-bold text-gray-900 mb-6">Produtos</h1>

    <!-- Filters -->
    <div class="bg-white rounded-lg border border-cream-300 p-4 mb-6">
      <div class="flex flex-wrap gap-3 items-end">
        <div class="flex-1 min-w-[200px]">
          <label class="block text-xs font-medium text-gray-500 mb-1">Pesquisar</label>
          <input
            v-model="search"
            type="text"
            placeholder="Nome ou marca..."
            class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-olive-400"
            @keyup.enter="applyFilters"
          />
        </div>

        <div class="w-40">
          <label class="block text-xs font-medium text-gray-500 mb-1">Marca</label>
          <select v-model="brand" class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-olive-400" @change="applyFilters">
            <option value="">Todas</option>
            <option v-for="b in brands" :key="b" :value="b">{{ b }}</option>
          </select>
        </div>

        <div class="w-40">
          <label class="block text-xs font-medium text-gray-500 mb-1">Categoria</label>
          <select v-model="category" class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-olive-400" @change="applyFilters">
            <option value="">Todas</option>
            <option v-for="c in categories" :key="c" :value="c">{{ c }}</option>
          </select>
        </div>

        <div class="w-28">
          <label class="block text-xs font-medium text-gray-500 mb-1">Tamanho</label>
          <select v-model="size" class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-olive-400" @change="applyFilters">
            <option value="">Todos</option>
            <option v-for="s in sizes" :key="s" :value="s">{{ s }}</option>
          </select>
        </div>

        <div class="w-36">
          <label class="block text-xs font-medium text-gray-500 mb-1">Ordenar</label>
          <select v-model="sort" class="w-full px-3 py-2 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-olive-400" @change="applyFilters">
            <option value="newest">Mais recentes</option>
            <option value="oldest">Mais antigos</option>
            <option value="price_asc">Preço: menor</option>
            <option value="price_desc">Preço: maior</option>
          </select>
        </div>

        <button
          class="px-4 py-2 bg-olive-500 text-white text-sm rounded-lg hover:bg-olive-600 transition"
          @click="applyFilters"
        >
          Filtrar
        </button>
        <button
          class="px-4 py-2 text-gray-500 text-sm hover:text-gray-700 transition"
          @click="clearFilters"
        >
          Limpar
        </button>
      </div>
    </div>

    <!-- Results count -->
    <p v-if="products" class="text-sm text-gray-500 mb-4">
      {{ products.totalCount }} produto{{ products.totalCount !== 1 ? 's' : '' }} encontrado{{ products.totalCount !== 1 ? 's' : '' }}
    </p>

    <!-- Loading -->
    <div v-if="status === 'pending'" class="text-center py-20 text-gray-400">A carregar...</div>

    <!-- Product Grid -->
    <div v-else-if="products?.items?.length" class="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-6">
      <NuxtLink
        v-for="product in products.items"
        :key="product.slug"
        :to="`/produtos/${product.slug}`"
        class="group bg-white rounded-lg border border-cream-300 overflow-hidden hover:shadow-md transition"
      >
        <div class="aspect-square bg-cream-100 overflow-hidden">
          <img
            v-if="product.primaryPhotoUrl"
            :src="photoUrl(product.primaryPhotoUrl)"
            :alt="product.title"
            class="w-full h-full object-cover group-hover:scale-105 transition duration-300"
          />
          <div v-else class="w-full h-full flex items-center justify-center text-cream-400">
            <svg xmlns="http://www.w3.org/2000/svg" class="h-12 w-12" fill="none" viewBox="0 0 24 24" stroke="currentColor">
              <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1" d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
            </svg>
          </div>
        </div>
        <div class="p-3">
          <h3 class="text-sm font-medium text-gray-900 group-hover:text-olive-600 transition truncate">{{ product.title }}</h3>
          <p class="text-xs text-gray-500 mt-0.5">{{ product.brandName }} <span v-if="product.size">&middot; {{ product.size }}</span></p>
          <p class="text-sm font-bold text-gray-900 mt-1">&euro;{{ product.price.toFixed(2) }}</p>
        </div>
      </NuxtLink>
    </div>

    <!-- Empty -->
    <div v-else class="text-center py-20 text-gray-500">
      <p class="text-lg mb-2">Nenhum produto encontrado</p>
      <p class="text-sm">Tente ajustar os filtros.</p>
    </div>

    <!-- Pagination -->
    <div v-if="products && products.totalPages > 1" class="flex items-center justify-center gap-2 mt-10">
      <button
        :disabled="page <= 1"
        class="px-3 py-2 border border-gray-300 rounded-lg text-sm disabled:opacity-30"
        @click="goToPage(page - 1)"
      >
        Anterior
      </button>
      <span class="text-sm text-gray-500">
        Página {{ products.page }} de {{ products.totalPages }}
      </span>
      <button
        :disabled="page >= products.totalPages"
        class="px-3 py-2 border border-gray-300 rounded-lg text-sm disabled:opacity-30"
        @click="goToPage(page + 1)"
      >
        Seguinte
      </button>
    </div>
  </div>
</template>
