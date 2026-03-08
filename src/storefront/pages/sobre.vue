<script setup lang="ts">
useHead({ title: 'Sobre - Oui Circular' })
</script>

<template>
  <div class="max-w-3xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
    <h1 class="text-3xl font-bold text-gray-900 mb-6">Sobre a Oui Circular</h1>

    <div class="prose prose-gray max-w-none">
      <p class="text-lg text-gray-600 leading-relaxed mb-6">
        A Oui Circular é uma loja de moda circular e sustentável em Portugal.
        Acreditamos que a moda pode ser bonita, acessível e amiga do ambiente.
      </p>

      <h2 class="text-xl font-bold text-gray-900 mt-8 mb-4">O que fazemos</h2>
      <p class="text-gray-600 mb-4">
        Recebemos peças em consignação de clientes que querem dar uma nova vida às suas roupas.
        Cada peça é cuidadosamente avaliada e selecionada para garantir qualidade.
      </p>

      <h2 class="text-xl font-bold text-gray-900 mt-8 mb-4">Como funciona a compra online</h2>
      <div class="space-y-4">
        <div class="flex gap-4 items-start">
          <div class="w-8 h-8 bg-olive-100 rounded-full flex items-center justify-center flex-shrink-0 text-olive-600 font-bold text-sm">1</div>
          <div>
            <h3 class="font-medium text-gray-900">Escolha</h3>
            <p class="text-sm text-gray-500">Navegue pelo nosso catálogo e adicione peças ao carrinho.</p>
          </div>
        </div>
        <div class="flex gap-4 items-start">
          <div class="w-8 h-8 bg-olive-100 rounded-full flex items-center justify-center flex-shrink-0 text-olive-600 font-bold text-sm">2</div>
          <div>
            <h3 class="font-medium text-gray-900">Reserve</h3>
            <p class="text-sm text-gray-500">Faça a sua encomenda online. Os artigos ficam reservados durante 48 horas.</p>
          </div>
        </div>
        <div class="flex gap-4 items-start">
          <div class="w-8 h-8 bg-olive-100 rounded-full flex items-center justify-center flex-shrink-0 text-olive-600 font-bold text-sm">3</div>
          <div>
            <h3 class="font-medium text-gray-900">Levante</h3>
            <p class="text-sm text-gray-500">Dirija-se à loja para experimentar, pagar e levar as suas peças.</p>
          </div>
        </div>
      </div>

      <h2 class="text-xl font-bold text-gray-900 mt-8 mb-4">Visite-nos</h2>
      <div class="bg-cream-100 rounded-lg p-6">
        <p class="text-gray-600 mb-2"><strong>Morada:</strong> Lisboa, Portugal</p>
        <p class="text-gray-600 mb-2"><strong>Horário:</strong> Seg-Sex 10h-19h, Sáb 10h-14h</p>
        <p class="text-gray-600 mb-2"><strong>WhatsApp:</strong> +351 XXX XXX XXX</p>
        <p class="text-gray-600"><strong>Email:</strong> info@ouicircular.pt</p>
      </div>
    </div>
  </div>
</template>
