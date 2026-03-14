<script setup lang="ts">
import { useCartStore } from '~/stores/cart'
import type { OrderResponse } from '~/types'

const { post } = useApi()
const cart = useCartStore()
const router = useRouter()

const name = ref('')
const email = ref('')
const phone = ref('')
const notes = ref('')
const submitting = ref(false)
const error = ref('')

async function submitOrder() {
  if (!name.value.trim()) { error.value = 'Nome é obrigatório.'; return }
  if (!email.value.trim()) { error.value = 'Email é obrigatório.'; return }
  if (cart.isEmpty) { error.value = 'O carrinho está vazio.'; return }

  error.value = ''
  submitting.value = true

  try {
    const response = await post<OrderResponse>('/store/orders', {
      customerName: name.value.trim(),
      customerEmail: email.value.trim(),
      customerPhone: phone.value.trim() || null,
      productSlugs: cart.items.map(i => i.slug),
      notes: notes.value.trim() || null,
    })

    cart.clear()
    await router.push(`/encomenda/${response.externalId}`)
  } catch (err: any) {
    error.value = err?.data?.error || 'Erro ao criar encomenda. Tente novamente.'
    submitting.value = false
  }
}

useHead({ title: 'Checkout - Oui Circular' })
</script>

<template>
  <div class="max-w-xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <h1 class="text-2xl font-normal text-gray-900 mb-2">Finalizar Encomenda</h1>
    <p class="text-sm text-gray-500 mb-8">
      Preencha os seus dados para reservar os artigos. O pagamento é feito na loja.
    </p>

    <div v-if="cart.isEmpty" class="text-center py-12 text-gray-500">
      O carrinho está vazio.
      <NuxtLink to="/produtos" class="text-sage-dark hover:underline block mt-2">Ver produtos</NuxtLink>
    </div>

    <form v-else @submit.prevent="submitOrder">
      <!-- Order summary -->
      <div class="bg-cream-dark rounded-lg p-4 mb-6">
        <h2 class="text-sm font-bold text-gray-900 mb-3 font-sans">Resumo ({{ cart.count }} artigo{{ cart.count !== 1 ? 's' : '' }})</h2>
        <div v-for="item in cart.items" :key="item.slug" class="flex justify-between text-sm py-1">
          <span class="text-gray-600 truncate mr-4">{{ item.title }}</span>
          <span class="font-medium text-gray-900 whitespace-nowrap">&euro;{{ item.price.toFixed(2) }}</span>
        </div>
        <div class="border-t border-cream-dark mt-3 pt-3 flex justify-between">
          <span class="font-bold text-gray-900">Total</span>
          <span class="font-bold text-gray-900">&euro;{{ cart.total.toFixed(2) }}</span>
        </div>
      </div>

      <!-- Form fields -->
      <div class="space-y-4 mb-6">
        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1">Nome *</label>
          <input
            v-model="name"
            type="text"
            required
            class="w-full px-4 py-2.5 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-sage"
            placeholder="O seu nome completo"
          />
        </div>

        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1">Email *</label>
          <input
            v-model="email"
            type="email"
            required
            class="w-full px-4 py-2.5 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-sage"
            placeholder="email@exemplo.pt"
          />
        </div>

        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1">Telefone</label>
          <input
            v-model="phone"
            type="tel"
            class="w-full px-4 py-2.5 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-sage"
            placeholder="+351 XXX XXX XXX"
          />
        </div>

        <div>
          <label class="block text-sm font-medium text-gray-700 mb-1">Notas</label>
          <textarea
            v-model="notes"
            rows="3"
            class="w-full px-4 py-2.5 border border-gray-300 rounded-lg text-sm focus:outline-none focus:ring-2 focus:ring-sage resize-none"
            placeholder="Alguma observação sobre a encomenda..."
          />
        </div>
      </div>

      <!-- Info -->
      <div class="bg-sage/10 rounded-lg p-4 mb-6 text-sm text-dark">
        <p class="font-medium mb-1">Como funciona?</p>
        <ul class="space-y-1 text-sage-dark">
          <li>1. Os artigos ficam reservados durante 48 horas.</li>
          <li>2. Receberá confirmação por email.</li>
          <li>3. Dirija-se à loja para pagamento e levantamento.</li>
        </ul>
      </div>

      <!-- Error -->
      <div v-if="error" class="bg-red-50 text-red-700 p-3 rounded-lg mb-4 text-sm">
        {{ error }}
      </div>

      <!-- Submit -->
      <button
        type="submit"
        :disabled="submitting"
        class="w-full bg-sage text-white py-3 rounded-lg font-semibold hover:bg-sage-dark transition disabled:opacity-50 disabled:cursor-not-allowed"
      >
        {{ submitting ? 'A processar...' : 'Reservar Artigos' }}
      </button>
    </form>
  </div>
</template>
