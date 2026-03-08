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
    },
  },
})
