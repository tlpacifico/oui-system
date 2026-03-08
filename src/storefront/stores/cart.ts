import { defineStore } from 'pinia'
import type { Product } from '~/types'

interface CartItem extends Product {
  addedAt: string
}

export const useCartStore = defineStore('cart', {
  state: () => ({
    items: [] as CartItem[],
  }),

  getters: {
    count: (state) => state.items.length,
    total: (state) => state.items.reduce((sum, item) => sum + item.price, 0),
    isEmpty: (state) => state.items.length === 0,
  },

  actions: {
    add(product: Product) {
      // Each item is unique — check by slug
      if (this.items.some(i => i.slug === product.slug)) return false
      this.items.push({ ...product, addedAt: new Date().toISOString() })
      return true
    },

    remove(slug: string) {
      this.items = this.items.filter(i => i.slug !== slug)
    },

    clear() {
      this.items = []
    },

    has(slug: string): boolean {
      return this.items.some(i => i.slug === slug)
    },
  },

  // Persist to localStorage
  persist: true,
})
