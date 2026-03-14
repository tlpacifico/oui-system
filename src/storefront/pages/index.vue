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
    <section class="grid grid-cols-1 md:grid-cols-2 overflow-hidden">
      <div class="flex flex-col justify-center px-[8%] py-12 md:py-20 bg-cream">
        <h2 class="text-3xl md:text-[42px] font-normal leading-[1.15] text-dark mb-5">
          A moda é <em class="text-sage italic">circular.</em>
        </h2>
        <p class="text-[15px] text-gray-400 max-w-[420px] mb-8 leading-relaxed">
          Curadoria exclusiva de peças em segunda mão com a elegância que o seu guarda-roupa merece.
        </p>
        <div class="flex gap-4 flex-wrap">
          <NuxtLink
            to="/produtos?sort=newest"
            class="px-8 py-3.5 bg-sage text-white text-xs font-semibold tracking-[2px] uppercase hover:bg-sage-dark transition inline-block"
          >
            Ver Novidades
          </NuxtLink>
          <NuxtLink
            to="/sobre"
            class="px-8 py-3.5 border-[1.5px] border-dark text-dark text-xs font-semibold tracking-[2px] uppercase hover:bg-dark hover:text-white transition inline-block"
          >
            Como Funciona
          </NuxtLink>
        </div>
      </div>
      <div
        class="hidden md:block bg-cover bg-center min-h-[350px]"
        style="background-image: url('https://images.unsplash.com/photo-1532453288454-ba56e462945a?auto=format&fit=crop&q=80&w=1200')"
      ></div>
    </section>

    <!-- Trust Strip -->
    <div class="grid grid-cols-1 md:grid-cols-3 text-center py-10 px-[6%] bg-white border-b border-black/[0.06]">
      <div class="py-2.5 px-5">
        <h4 class="font-sans text-sm font-medium tracking-wider uppercase text-sage mb-1.5">Curadoria Própria</h4>
        <p class="text-[13px] text-gray-400">Cada peça é cuidadosamente selecionada pela nossa equipa</p>
      </div>
      <div class="py-2.5 px-5">
        <h4 class="font-sans text-sm font-medium tracking-wider uppercase text-sage mb-1.5">Consignação Justa</h4>
        <p class="text-[13px] text-gray-400">Ganhe dinheiro com as peças que já não usa</p>
      </div>
      <div class="py-2.5 px-5">
        <h4 class="font-sans text-sm font-medium tracking-wider uppercase text-sage mb-1.5">Moda Sustentável</h4>
        <p class="text-[13px] text-gray-400">Menos desperdício, mais estilo e consciência</p>
      </div>
    </div>

    <!-- Products: O Novo Drop -->
    <section class="py-20 px-[6%]">
      <div class="text-center mb-12">
        <h2 class="text-4xl font-normal text-dark mb-3">O Novo Drop</h2>
        <p class="text-[15px] text-gray-400 max-w-[500px] mx-auto">Peças selecionadas esta semana, prontas para um novo capítulo</p>
      </div>

      <div v-if="featured?.items?.length" class="grid grid-cols-2 md:grid-cols-4 gap-6">
        <NuxtLink
          v-for="(product, idx) in featured.items"
          :key="product.slug"
          :to="`/produtos/${product.slug}`"
          class="group cursor-pointer"
        >
          <div class="relative overflow-hidden bg-cream-dark aspect-[3/4] mb-4">
            <span
              v-if="idx === 0"
              class="absolute top-3.5 left-3.5 bg-sage text-white px-3 py-1 text-[10px] font-semibold tracking-wider uppercase z-10"
            >Novo</span>
            <img
              v-if="product.primaryPhotoUrl"
              :src="photoUrl(product.primaryPhotoUrl)"
              :alt="product.title"
              class="w-full h-full object-cover transition-transform duration-[600ms] group-hover:scale-105"
            />
            <div v-else class="w-full h-full flex items-center justify-center text-gray-300">
              <svg xmlns="http://www.w3.org/2000/svg" class="h-12 w-12" fill="none" viewBox="0 0 24 24" stroke="currentColor">
                <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1" d="M4 16l4.586-4.586a2 2 0 012.828 0L16 16m-2-2l1.586-1.586a2 2 0 012.828 0L20 14m-6-6h.01M6 20h12a2 2 0 002-2V6a2 2 0 00-2-2H6a2 2 0 00-2 2v12a2 2 0 002 2z" />
              </svg>
            </div>
            <div class="absolute bottom-0 left-0 right-0 bg-dark/90 text-white text-center py-3.5 text-xs font-semibold tracking-[1.5px] uppercase translate-y-full group-hover:translate-y-0 transition-transform duration-300">
              Ver Detalhes
            </div>
          </div>
          <div class="px-1">
            <p class="text-[11px] text-gray-400 uppercase tracking-[1.5px] mb-1">{{ product.brandName }}</p>
            <p class="font-serif text-[17px] font-normal text-dark mb-1.5">{{ product.title }}</p>
            <div class="flex justify-between items-center">
              <span class="text-[15px] font-semibold text-sage">&euro;{{ product.price.toFixed(2) }}</span>
              <span v-if="product.size" class="text-[11px] text-gray-400 border border-cream-dark px-2 py-0.5 tracking-[0.5px]">{{ product.size }}</span>
            </div>
          </div>
        </NuxtLink>
      </div>

      <div v-else class="text-center py-12 text-gray-500">
        Nenhum produto disponível de momento.
      </div>

      <div class="text-center mt-12">
        <NuxtLink
          to="/produtos"
          class="inline-block px-10 py-4 border-[1.5px] border-sage text-sage text-xs font-semibold tracking-[2px] uppercase hover:bg-sage hover:text-white transition"
        >
          Ver Todos os Produtos
        </NuxtLink>
      </div>
    </section>

    <!-- Categories -->
    <section class="py-20 px-[6%] bg-white">
      <div class="text-center mb-12">
        <h2 class="text-4xl font-normal text-dark">Explorar por Categoria</h2>
      </div>
      <div class="grid grid-cols-1 md:grid-cols-3 gap-5">
        <NuxtLink to="/produtos?category=Casacos" class="relative overflow-hidden aspect-[4/5] cursor-pointer group">
          <img
            src="https://images.unsplash.com/photo-1551488831-00ddcb6c6bd3?auto=format&fit=crop&q=80&w=800"
            alt="Casacos"
            class="w-full h-full object-cover transition-transform duration-[600ms] group-hover:scale-105"
          />
          <div class="absolute inset-0 bg-gradient-to-t from-black/50 via-transparent to-transparent flex items-end p-8">
            <h3 class="text-white text-2xl font-normal">Casacos &amp; Blazers</h3>
          </div>
        </NuxtLink>
        <NuxtLink to="/produtos?category=Vestidos" class="relative overflow-hidden aspect-[4/5] cursor-pointer group">
          <img
            src="https://images.unsplash.com/photo-1496747611176-843222e1e57c?auto=format&fit=crop&q=80&w=800"
            alt="Vestidos"
            class="w-full h-full object-cover transition-transform duration-[600ms] group-hover:scale-105"
          />
          <div class="absolute inset-0 bg-gradient-to-t from-black/50 via-transparent to-transparent flex items-end p-8">
            <h3 class="text-white text-2xl font-normal">Vestidos</h3>
          </div>
        </NuxtLink>
        <NuxtLink to="/produtos?category=Acessórios" class="relative overflow-hidden aspect-[4/5] cursor-pointer group">
          <img
            src="https://images.unsplash.com/photo-1566150905458-1bf1fc113f0d?auto=format&fit=crop&q=80&w=800"
            alt="Acessórios"
            class="w-full h-full object-cover transition-transform duration-[600ms] group-hover:scale-105"
          />
          <div class="absolute inset-0 bg-gradient-to-t from-black/50 via-transparent to-transparent flex items-end p-8">
            <h3 class="text-white text-2xl font-normal">Malas &amp; Acessórios</h3>
          </div>
        </NuxtLink>
      </div>
    </section>

    <!-- Sell / Consignment CTA -->
    <section class="grid grid-cols-1 md:grid-cols-2 min-h-[500px] overflow-hidden">
      <div
        class="h-[400px] md:h-auto bg-cover bg-center"
        style="background-image: url('https://images.unsplash.com/photo-1558171813-4c088753af8f?auto=format&fit=crop&q=80&w=1000')"
      ></div>
      <div class="bg-terracotta text-white flex flex-col justify-center px-[8%] py-16">
        <h2 class="text-3xl md:text-[40px] font-normal leading-[1.2] mb-5">Dê uma nova vida às suas peças.</h2>
        <p class="text-base leading-relaxed opacity-90 mb-3 max-w-[420px]">
          O seu armário merece espaço. As suas peças merecem uma nova história.
        </p>
        <ul class="list-none my-6 space-y-2">
          <li class="flex items-center gap-3 text-[15px]">
            <span class="w-7 h-7 border-[1.5px] border-white/50 rounded-full flex items-center justify-center text-xs font-semibold shrink-0">1</span>
            Traga as suas peças à nossa loja
          </li>
          <li class="flex items-center gap-3 text-[15px]">
            <span class="w-7 h-7 border-[1.5px] border-white/50 rounded-full flex items-center justify-center text-xs font-semibold shrink-0">2</span>
            Nós fotografamos e colocamos à venda
          </li>
          <li class="flex items-center gap-3 text-[15px]">
            <span class="w-7 h-7 border-[1.5px] border-white/50 rounded-full flex items-center justify-center text-xs font-semibold shrink-0">3</span>
            Receba quando a peça for vendida
          </li>
        </ul>
        <div>
          <NuxtLink
            to="/sobre"
            class="inline-block px-10 py-4 border-[1.5px] border-white text-white text-xs font-semibold tracking-[2px] uppercase hover:bg-white hover:text-sage transition"
          >
            Quero Vender Agora
          </NuxtLink>
        </div>
      </div>
    </section>

    <!-- Testimonials -->
    <section class="py-20 px-[6%] bg-white">
      <div class="text-center mb-12">
        <h2 class="text-4xl font-normal text-dark">O Que Dizem de Nós</h2>
      </div>
      <div class="grid grid-cols-1 md:grid-cols-3 gap-10">
        <div class="text-center p-5">
          <div class="text-terracotta text-sm tracking-[3px] mb-4">&#9733;&#9733;&#9733;&#9733;&#9733;</div>
          <p class="font-serif text-lg italic text-dark leading-relaxed mb-5">
            &ldquo;Encontrei um casaco Burberry em perfeito estado por uma fração do preço. Adorei a experiência!&rdquo;
          </p>
          <p class="text-[13px] text-gray-400 tracking-wider uppercase">Maria S. &mdash; Porto</p>
        </div>
        <div class="text-center p-5">
          <div class="text-terracotta text-sm tracking-[3px] mb-4">&#9733;&#9733;&#9733;&#9733;&#9733;</div>
          <p class="font-serif text-lg italic text-dark leading-relaxed mb-5">
            &ldquo;Vendi 15 peças que estavam paradas no armário. Processo fácil e transparente.&rdquo;
          </p>
          <p class="text-[13px] text-gray-400 tracking-wider uppercase">Ana P. &mdash; Matosinhos</p>
        </div>
        <div class="text-center p-5">
          <div class="text-terracotta text-sm tracking-[3px] mb-4">&#9733;&#9733;&#9733;&#9733;&#9733;</div>
          <p class="font-serif text-lg italic text-dark leading-relaxed mb-5">
            &ldquo;A curadoria é fantástica. Cada visita à loja é uma surpresa.&rdquo;
          </p>
          <p class="text-[13px] text-gray-400 tracking-wider uppercase">Joana R. &mdash; Gaia</p>
        </div>
      </div>
    </section>

    <!-- Newsletter -->
    <section class="bg-cream-dark text-center py-20 px-[6%]">
      <h2 class="text-[32px] font-normal text-dark mb-3">Fique a Par das Novidades</h2>
      <p class="text-[15px] text-gray-400 mb-8">Receba em primeira mão os novos drops e ofertas exclusivas.</p>
      <form class="flex flex-col md:flex-row max-w-[480px] mx-auto" @submit.prevent>
        <input
          type="email"
          placeholder="O seu email"
          class="flex-1 px-5 py-4 border-[1.5px] border-sage md:border-r-0 bg-white font-sans text-sm outline-none placeholder:text-gray-300"
        />
        <button
          type="submit"
          class="px-8 py-4 bg-sage border-[1.5px] border-sage text-white font-sans text-xs font-semibold tracking-[1.5px] uppercase hover:bg-sage-dark transition cursor-pointer"
        >
          Subscrever
        </button>
      </form>
    </section>
  </div>
</template>
