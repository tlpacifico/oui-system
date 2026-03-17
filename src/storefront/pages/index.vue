<script setup lang="ts">
import type { Product, ProductsResponse } from '~/types'

const { get, baseUrl } = useApi()

const { data: featured } = await useAsyncData('featured', () =>
  get<ProductsResponse>('/store/products', { pageSize: 8, sort: 'newest' })
)

const { data: brands } = await useAsyncData('brands', () =>
  get<string[]>('/store/brands')
)

// Static Google reviews (real reviews from Google Maps)
const reviews = [
  { authorName: 'Evelyn Melissa', text: 'Loja incrível, curadoria de roupas perfeitas. Cheirinho de roupa cuidado com muito amor. Sou viciada nas roupas da oui, todas que tenho são minhas favoritas. Sem contar que além de peças incríveis e bem selecionadas, a Lily nos ajuda a montar diversos looks incríveis. Você sai da oui com uma aula de moda sustentável, com roupas lindas, peças exclusivas e prontinha pra dar muito close por aí. Eu amo...', rating: 5, relativeTime: 'há um ano' },
  { authorName: 'Taimara da Silva Neto', text: 'A loja é linda! Melhor curadoria, peças de qualidade e atuais. Ótimo atendimento, voltarei muitas vezes', rating: 5, relativeTime: 'há um ano' },
  { authorName: 'Cristina Lanferdini Bordignon', text: 'A melhor loja de moda circular! O cuidado com as roupas, sempre impecáveis e cheirosas, é único. Além disso, a simpatia e o carinho da Lilly conquistam o coração de todos que passam por lá, não tenho dúvidas.', rating: 5, relativeTime: 'há um ano' },
  { authorName: 'Andressa Varoni', text: 'Loja incrível, uma curadoria impecável e além disso o atendimento é simplesmente perfeito.', rating: 5, relativeTime: 'há um ano' },
  { authorName: 'Amanda Penido', text: 'Ir às compras foi tão divertido, prático e fácil. Loja linda, acessível, bem localizada, cheia de produtos excelentes e com uma curadoria de tirar o fôlego. É sempre um prazer passear na Oui Circular, impossível não se sentir a mais maravilhosa com estes looks. Obrigada pelo ótimo atendimento e pela beleza renovada! :)', rating: 5, relativeTime: 'há um ano' },
  { authorName: 'Paula Floripes', text: 'Gente...eu amo esse lugar, é lindo, super agradável e as peças de muita qualidade!! Além disso a Lilly tem um olhar fantástico pros detalhes, pra moda e estilo! Sou fã demais', rating: 5, relativeTime: 'há um ano' },
]

function photoUrl(path?: string) {
  if (!path) return ''
  const base = (baseUrl as string).replace('/api', '')
  return `${base}${path}`
}

// Scroll-reveal with IntersectionObserver
function useReveal() {
  if (import.meta.server) return

  const observer = new IntersectionObserver(
    (entries) => {
      entries.forEach((entry) => {
        if (entry.isIntersecting) {
          entry.target.classList.add('visible')
          observer.unobserve(entry.target)
        }
      })
    },
    { threshold: 0.15 }
  )

  onMounted(() => {
    document.querySelectorAll('.reveal').forEach((el) => observer.observe(el))
  })

  onBeforeUnmount(() => observer.disconnect())
}

useReveal()
</script>

<template>
  <div>
    <!-- Hero: 3-Column with Video Center -->
    <section class="grid grid-cols-1 md:grid-cols-[1fr_2fr_1fr] h-[90vh] min-h-[550px] overflow-hidden">
      <!-- Left Panel: Text + CTAs -->
      <div class="bg-cream flex flex-col justify-center px-[10%] py-12">
        <h1 class="hero-heading text-4xl md:text-5xl font-normal leading-[1.1] text-dark mb-5">
          A moda é <em class="text-sage italic">circular.</em>
        </h1>
        <p class="hero-tagline text-[15px] text-gray-400 max-w-[420px] mb-10 leading-relaxed">
          Curadoria exclusiva de peças em segunda mão com a elegância que o seu guarda-roupa merece.
        </p>
        <div class="hero-ctas flex gap-4 flex-wrap">
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
      <!-- Center: Video -->
      <div class="relative overflow-hidden">
        <video
          autoplay
          loop
          muted
          playsinline
          poster="/casaco.jpg"
          class="w-full h-full object-cover"
        >
          <source src="/oui-ensaio.mp4" type="video/mp4" />
        </video>
      </div>
      <!-- Right Panel -->
      <div class="hidden md:flex bg-cream flex-col justify-end items-center pb-12">
        <!-- Scroll indicator -->
        <div class="scroll-indicator">
          <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6 text-sage/60" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="1.5" d="M19 9l-7 7-7-7" />
          </svg>
        </div>
      </div>
    </section>

    <!-- Trust Strip -->
    <div class="grid grid-cols-1 md:grid-cols-3 text-center py-10 px-[6%] bg-white border-y border-black/[0.06]">
      <div class="reveal py-2.5 px-5" style="transition-delay: 0ms">
        <span class="block w-6 h-[2px] bg-sage mx-auto mb-3"></span>
        <h4 class="font-sans text-sm font-medium tracking-wider uppercase text-sage mb-1.5">Curadoria Própria</h4>
        <p class="text-[13px] text-gray-400">Cada peça é cuidadosamente selecionada pela nossa equipa</p>
      </div>
      <div class="reveal py-2.5 px-5" style="transition-delay: 150ms">
        <span class="block w-6 h-[2px] bg-sage mx-auto mb-3"></span>
        <h4 class="font-sans text-sm font-medium tracking-wider uppercase text-sage mb-1.5">Consignação Justa</h4>
        <p class="text-[13px] text-gray-400">Ganhe dinheiro com as peças que já não usa</p>
      </div>
      <div class="reveal py-2.5 px-5" style="transition-delay: 300ms">
        <span class="block w-6 h-[2px] bg-sage mx-auto mb-3"></span>
        <h4 class="font-sans text-sm font-medium tracking-wider uppercase text-sage mb-1.5">Moda Sustentável</h4>
        <p class="text-[13px] text-gray-400">Menos desperdício, mais estilo e consciência</p>
      </div>
    </div>

    <!-- Products: O Novo Drop -->
    <section class="py-20 px-[6%]">
      <div class="reveal text-center mb-12">
        <h2 class="text-4xl font-normal text-dark mb-3">O Novo Drop</h2>
        <span class="block w-10 h-[1px] bg-sage mx-auto mb-4"></span>
        <p class="text-[15px] text-gray-400 max-w-[500px] mx-auto">Peças selecionadas esta semana, prontas para um novo capítulo</p>
      </div>

      <div v-if="featured?.items?.length" class="grid grid-cols-2 md:grid-cols-4 gap-6">
        <NuxtLink
          v-for="(product, idx) in featured.items"
          :key="product.slug"
          :to="`/produtos/${product.slug}`"
          class="reveal group cursor-pointer"
          :style="{ transitionDelay: `${idx * 150}ms` }"
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
      <div class="reveal text-center mb-12">
        <h2 class="text-4xl font-normal text-dark">Explorar por Categoria</h2>
        <span class="block w-10 h-[1px] bg-sage mx-auto mt-4"></span>
      </div>
      <div class="grid grid-cols-1 md:grid-cols-3 gap-5">
        <NuxtLink to="/produtos?category=Casacos" class="reveal category-card relative overflow-hidden aspect-[4/5] cursor-pointer group" style="transition-delay: 0ms">
          <img
            src="/casaco.jpg"
            alt="Casacos"
            class="w-full h-full object-cover transition-transform duration-[600ms] group-hover:scale-105"
          />
          <div class="absolute inset-0 bg-gradient-to-t from-black/50 via-transparent to-transparent group-hover:from-black/60 transition-all duration-500 flex items-end p-8">
            <div>
              <h3 class="text-white text-2xl font-normal transition-transform duration-300 group-hover:-translate-y-2">Casacos &amp; Blazers</h3>
              <span class="block w-0 h-[1px] bg-white/70 transition-all duration-300 group-hover:w-12 mt-1"></span>
              <p class="text-white/0 text-sm tracking-wider mt-2 transition-all duration-300 group-hover:text-white/80">Explorar &rarr;</p>
            </div>
          </div>
        </NuxtLink>
        <NuxtLink to="/produtos?category=Vestidos" class="reveal category-card relative overflow-hidden aspect-[4/5] cursor-pointer group" style="transition-delay: 150ms">
          <img
            src="/vestido.jpg"
            alt="Vestidos"
            class="w-full h-full object-cover transition-transform duration-[600ms] group-hover:scale-105"
          />
          <div class="absolute inset-0 bg-gradient-to-t from-black/50 via-transparent to-transparent group-hover:from-black/60 transition-all duration-500 flex items-end p-8">
            <div>
              <h3 class="text-white text-2xl font-normal transition-transform duration-300 group-hover:-translate-y-2">Vestidos</h3>
              <span class="block w-0 h-[1px] bg-white/70 transition-all duration-300 group-hover:w-12 mt-1"></span>
              <p class="text-white/0 text-sm tracking-wider mt-2 transition-all duration-300 group-hover:text-white/80">Explorar &rarr;</p>
            </div>
          </div>
        </NuxtLink>
        <NuxtLink to="/produtos?category=Acessórios" class="reveal category-card relative overflow-hidden aspect-[4/5] cursor-pointer group" style="transition-delay: 300ms">
          <img
            src="/mala.jpg"
            alt="Acessórios"
            class="w-full h-full object-cover transition-transform duration-[600ms] group-hover:scale-105"
          />
          <div class="absolute inset-0 bg-gradient-to-t from-black/50 via-transparent to-transparent group-hover:from-black/60 transition-all duration-500 flex items-end p-8">
            <div>
              <h3 class="text-white text-2xl font-normal transition-transform duration-300 group-hover:-translate-y-2">Malas &amp; Acessórios</h3>
              <span class="block w-0 h-[1px] bg-white/70 transition-all duration-300 group-hover:w-12 mt-1"></span>
              <p class="text-white/0 text-sm tracking-wider mt-2 transition-all duration-300 group-hover:text-white/80">Explorar &rarr;</p>
            </div>
          </div>
        </NuxtLink>
      </div>
    </section>

    <!-- Sell / Consignment CTA -->
    <section class="grid grid-cols-1 md:grid-cols-2 min-h-[500px] overflow-hidden">
      <div
        class="h-[400px] md:h-auto bg-cover"
        style="background-image: url('/segundavida.jpeg'); background-position: center 40%"
      ></div>
      <div class="bg-terracotta text-white flex flex-col justify-center px-[8%] py-16">
        <h2 class="reveal text-3xl md:text-[40px] font-normal leading-[1.2] mb-5">Dê uma nova vida às suas peças.</h2>
        <p class="reveal text-base leading-relaxed opacity-90 mb-3 max-w-[420px]" style="transition-delay: 100ms">
          O seu armário merece espaço. As suas peças merecem uma nova história.
        </p>
        <ul class="list-none my-6 space-y-2">
          <li class="reveal flex items-center gap-3 text-[15px]" style="transition-delay: 200ms">
            <span class="w-7 h-7 border-[1.5px] border-white/50 rounded-full flex items-center justify-center text-xs font-semibold shrink-0">1</span>
            Traga as suas peças à nossa loja
          </li>
          <li class="reveal flex items-center gap-3 text-[15px]" style="transition-delay: 300ms">
            <span class="w-7 h-7 border-[1.5px] border-white/50 rounded-full flex items-center justify-center text-xs font-semibold shrink-0">2</span>
            Nós fotografamos e colocamos à venda
          </li>
          <li class="reveal flex items-center gap-3 text-[15px]" style="transition-delay: 400ms">
            <span class="w-7 h-7 border-[1.5px] border-white/50 rounded-full flex items-center justify-center text-xs font-semibold shrink-0">3</span>
            Receba quando a peça for vendida
          </li>
        </ul>
        <div class="reveal" style="transition-delay: 500ms">
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
      <div class="reveal text-center mb-12">
        <h2 class="text-4xl font-normal text-dark">O Que Dizem de Nós</h2>
        <span class="block w-10 h-[1px] bg-sage mx-auto mt-4"></span>
      </div>
      <div class="grid grid-cols-1 md:grid-cols-3 gap-10">
        <div
          v-for="(review, idx) in reviews"
          :key="idx"
          class="reveal text-center p-5"
          :style="{ transitionDelay: `${idx * 150}ms` }"
        >
          <div class="text-terracotta text-sm tracking-[3px] mb-4">
            <span v-for="s in review.rating" :key="s">&#9733;</span>
            <span v-for="s in (5 - review.rating)" :key="'e' + s" class="opacity-25">&#9733;</span>
          </div>
          <p class="font-serif text-lg italic text-dark leading-relaxed mb-5">
            &ldquo;{{ review.text }}&rdquo;
          </p>
          <p class="text-[13px] text-gray-400 tracking-wider uppercase">
            {{ review.authorName }}
            <template v-if="review.relativeTime"> &mdash; {{ review.relativeTime }}</template>
          </p>
        </div>
      </div>
    </section>

    <!-- Newsletter -->
    <section class="reveal bg-cream-dark text-center py-20 px-[6%]">
      <h2 class="text-[32px] font-normal text-dark mb-3">Fique a Par das Novidades</h2>
      <span class="block w-10 h-[1px] bg-sage mx-auto mb-4"></span>
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

<style scoped>
/* Hero text stagger */
.hero-heading {
  animation: fadeInUp 0.8s ease-out both;
}
.hero-tagline {
  animation: fadeInUp 0.8s ease-out 0.3s both;
}
.hero-ctas {
  animation: fadeInUp 0.8s ease-out 0.6s both;
}

@keyframes fadeInUp {
  from {
    opacity: 0;
    transform: translateY(24px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

/* Scroll indicator bounce */
.scroll-indicator {
  animation: bounce 2s ease-in-out infinite;
}

@keyframes bounce {
  0%, 100% {
    transform: translateX(-50%) translateY(0);
  }
  50% {
    transform: translateX(-50%) translateY(8px);
  }
}

/* Scroll-reveal */
.reveal {
  opacity: 0;
  transform: translateY(24px);
  transition: opacity 0.7s ease-out, transform 0.7s ease-out;
}

.reveal.visible {
  opacity: 1;
  transform: translateY(0);
}
</style>
