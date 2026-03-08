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
        olive: {
          50: '#f4f6f0',
          100: '#e6ebe0',
          200: '#cdd7c1',
          300: '#adbf9b',
          400: '#8fae7e',
          500: '#6b7c5e',
          600: '#5c6b4f',
          700: '#4a5640',
          800: '#3d4735',
          900: '#333c2d',
        },
        cream: {
          50: '#faf8f4',
          100: '#f5f0e8',
          200: '#ede5d5',
          300: '#e0d4bc',
          400: '#d4c5a9',
          500: '#c4b08e',
          600: '#a89570',
          700: '#8c7a58',
          800: '#74654a',
          900: '#5f523e',
        },
      },
    },
  },
} satisfies Config
