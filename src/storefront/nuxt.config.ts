// https://nuxt.com/docs/api/configuration/nuxt-config
export default defineNuxtConfig({
  compatibilityDate: '2025-07-15',
  devtools: { enabled: true },

  // Cloudflare Pages deployment
  nitro: {
    preset: 'cloudflare_pages',
  },

  modules: [
    '@nuxtjs/tailwindcss',
    '@pinia/nuxt',
  ],

  runtimeConfig: {
    public: {
      apiUrl: process.env.NUXT_PUBLIC_API_URL || 'http://localhost:5018/api',
    },
  },

  app: {
    head: {
      title: 'Oui Circular - Moda Circular & Sustentável',
      meta: [
        { charset: 'utf-8' },
        { name: 'viewport', content: 'width=device-width, initial-scale=1' },
        { name: 'description', content: 'Loja online de moda circular e sustentável em Portugal. Peças únicas de segunda mão com qualidade.' },
      ],
      htmlAttrs: {
        lang: 'pt-PT',
      },
      link: [
        { rel: 'icon', type: 'image/svg+xml', href: '/favicon.svg' },
        { rel: 'icon', type: 'image/x-icon', href: '/favicon.ico' },
        { rel: 'preconnect', href: 'https://fonts.googleapis.com' },
        { rel: 'preconnect', href: 'https://fonts.gstatic.com', crossorigin: '' },
        { rel: 'stylesheet', href: 'https://fonts.googleapis.com/css2?family=Playfair+Display:ital,wght@0,400;0,500;0,700;1,400&family=Inter:wght@300;400;500;600&display=swap' },
      ],
    },
  },
})
