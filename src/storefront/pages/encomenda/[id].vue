<script setup lang="ts">
import type { OrderStatus } from '~/types'

const { get } = useApi()
const route = useRoute()
const id = route.params.id as string

const { data: order, error } = await useAsyncData(`order-${id}`, () =>
  get<OrderStatus>(`/store/orders/${id}`)
)

const statusLabels: Record<string, string> = {
  Pending: 'Pendente',
  Confirmed: 'Confirmada',
  Completed: 'Concluída',
  Cancelled: 'Cancelada',
}

const statusColors: Record<string, string> = {
  Pending: 'bg-yellow-100 text-yellow-800',
  Confirmed: 'bg-green-100 text-green-800',
  Completed: 'bg-blue-100 text-blue-800',
  Cancelled: 'bg-red-100 text-red-800',
}

function formatDate(dateStr?: string) {
  if (!dateStr) return '-'
  return new Date(dateStr).toLocaleString('pt-PT', {
    day: '2-digit', month: '2-digit', year: 'numeric',
    hour: '2-digit', minute: '2-digit',
  })
}

useHead({ title: order.value ? `Encomenda ${order.value.orderNumber} - Oui Circular` : 'Encomenda - Oui Circular' })
</script>

<template>
  <div class="max-w-xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
    <div v-if="error" class="text-center py-20">
      <p class="text-lg text-gray-500 mb-4">Encomenda não encontrada.</p>
      <NuxtLink to="/produtos" class="text-emerald-700 hover:underline">Ver produtos</NuxtLink>
    </div>

    <div v-else-if="order">
      <!-- Success header -->
      <div class="text-center mb-8">
        <div class="w-16 h-16 bg-emerald-100 rounded-full flex items-center justify-center mx-auto mb-4">
          <svg xmlns="http://www.w3.org/2000/svg" class="h-8 w-8 text-emerald-600" fill="none" viewBox="0 0 24 24" stroke="currentColor">
            <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7" />
          </svg>
        </div>
        <h1 class="text-2xl font-bold text-gray-900 mb-1">Encomenda Registada</h1>
        <p class="text-sm text-gray-500">{{ order.orderNumber }}</p>
      </div>

      <!-- Status -->
      <div class="bg-white border border-gray-200 rounded-lg p-6 mb-6">
        <div class="flex items-center justify-between mb-4">
          <span class="text-sm text-gray-500">Estado</span>
          <span class="px-3 py-1 rounded-full text-xs font-semibold" :class="statusColors[order.status] || 'bg-gray-100 text-gray-800'">
            {{ statusLabels[order.status] || order.status }}
          </span>
        </div>

        <div class="space-y-2 text-sm">
          <div class="flex justify-between">
            <span class="text-gray-500">Cliente</span>
            <span class="text-gray-900">{{ order.customerName }}</span>
          </div>
          <div class="flex justify-between">
            <span class="text-gray-500">Reservado em</span>
            <span class="text-gray-900">{{ formatDate(order.reservedAt) }}</span>
          </div>
          <div class="flex justify-between">
            <span class="text-gray-500">Válido até</span>
            <span class="text-gray-900 font-medium">{{ formatDate(order.expiresAt) }}</span>
          </div>
          <div v-if="order.confirmedAt" class="flex justify-between">
            <span class="text-gray-500">Confirmado em</span>
            <span class="text-gray-900">{{ formatDate(order.confirmedAt) }}</span>
          </div>
          <div v-if="order.cancelledAt" class="flex justify-between">
            <span class="text-gray-500">Cancelado em</span>
            <span class="text-gray-900">{{ formatDate(order.cancelledAt) }}</span>
          </div>
          <div v-if="order.cancellationReason" class="flex justify-between">
            <span class="text-gray-500">Motivo</span>
            <span class="text-red-600">{{ order.cancellationReason }}</span>
          </div>
        </div>
      </div>

      <!-- Items -->
      <div class="bg-white border border-gray-200 rounded-lg p-6 mb-6">
        <h2 class="text-sm font-bold text-gray-900 mb-3">Artigos</h2>
        <div v-for="item in order.items" :key="item.productTitle" class="flex justify-between text-sm py-2 border-b border-gray-50 last:border-0">
          <span class="text-gray-600">{{ item.productTitle }}</span>
          <span class="font-medium text-gray-900">&euro;{{ item.price.toFixed(2) }}</span>
        </div>
        <div class="border-t border-gray-200 mt-2 pt-3 flex justify-between">
          <span class="font-bold text-gray-900">Total</span>
          <span class="font-bold text-gray-900 text-lg">&euro;{{ order.totalAmount.toFixed(2) }}</span>
        </div>
      </div>

      <!-- Info -->
      <div class="bg-emerald-50 rounded-lg p-4 text-sm text-emerald-800">
        <p class="font-medium mb-2">Próximos passos:</p>
        <ul class="space-y-1 text-emerald-700">
          <li>1. A equipa da loja irá confirmar a sua reserva.</li>
          <li>2. Dirija-se à loja dentro de 48 horas.</li>
          <li>3. Apresente o número da encomenda no balcão.</li>
        </ul>
      </div>

      <div class="text-center mt-8">
        <NuxtLink to="/produtos" class="text-emerald-700 hover:underline text-sm">Continuar a explorar</NuxtLink>
      </div>
    </div>
  </div>
</template>
