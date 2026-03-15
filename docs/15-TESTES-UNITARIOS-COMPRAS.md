# Testes Unitários para Compras (POS)

## Cenários de Compra

### 1. Compra: Cliente NÃO é Fornecedor + Peça de NÃO consignação
- **1.1** A peça é de consignação?
  - **1.1.1** Não → fluxo normal de venda
  - **1.1.2** Sim → (cenário inválido neste caso, ver cenário 2)

### 2. Compra: Cliente NÃO é Fornecedor + Peça COM consignação
- Cliente comum compra uma peça que pertence a um fornecedor em consignação.

### 3. Compra: Cliente É Fornecedor + Peça SEM consignação
- Fornecedor compra uma peça que não está em regime de consignação.

### 4. Compra: Cliente É Fornecedor + Peça COM consignação → Pagamento
- **4.1** Pagamento sem crédito (pagamento normal)
- **4.2** Pagamento com crédito (store credit da loja)
- **4.3** Pagamento misto (crédito + pagamento normal)

---

## Fluxo Geral de Venda

1. **Verificar se o item está publicado**
   - 1.1 Se sim, remover do e-commerce

2. **Mudar o Status para "Vendido" no Inventory**

3. **Verificar se o item é de consignação**
   - 3.1 Se sim, gerar crédito para o Fornecedor

4. **Gerar Venda e Acerto (Atualizar saldo)**
