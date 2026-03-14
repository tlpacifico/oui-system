import type { Config } from 'tailwindcss'

export default {
  content: [
    './components/**/*.{vue,ts}',
    './layouts/**/*.vue',
    './pages/**/*.vue',
    './composables/**/*.ts',
    './app.vue',
  ],
  theme: {
    extend: {
      colors: {
        sage: {
          DEFAULT: '#8A9A5B',
          dark: '#6b7a45',
          light: '#a8b87a',
        },
        cream: {
          DEFAULT: '#F9F7F2',
          dark: '#EDE9E0',
        },
        terracotta: {
          DEFAULT: '#C48B5C',
          dark: '#a87245',
        },
        dark: '#2B2B2B',
      },
      fontFamily: {
        serif: ['Playfair Display', 'Georgia', 'Cambria', 'serif'],
        sans: ['Inter', 'system-ui', 'sans-serif'],
      },
    },
  },
} satisfies Config
