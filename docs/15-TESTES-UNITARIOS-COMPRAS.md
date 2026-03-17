# Testes Unitarios para Compras (POS)

## Regras de Negocio Base

O fornecedor configura a sua preferencia de recebimento ao registar-se (uma OU outra):

| Opcao do fornecedor | Fornecedor recebe | Loja fica com |
|---------------------|-------------------|---------------|
| **Credito em loja** (PorcInLoja) | 50% em credito para compras na loja | 50% |
| **Resgate em dinheiro** (PorcInDinheiro) | 40% em dinheiro | 60% |

A loja oferece mais em credito (50% vs 40%) para incentivar o fornecedor a gastar na propria loja.

**Item proprio:** Loja fica com 100% do valor de venda. Lucro = Preco de venda - Custo de aquisicao.

---

## Cenarios de Compra

### 1. Cliente comum + Peca propria da loja (€10,00)

**Dados de entrada:**
- Comprador: Cliente comum (nao e fornecedor)
- Item: Vestido, aquisicao propria da loja
- Preco de venda: €10,00
- Custo de aquisicao: €4,00
- Pagamento: €10,00 em dinheiro

**Resultado esperado:**
- Lucro da loja: €6,00 (€10,00 - €4,00)
- Credito fornecedor: nenhum (peca nao e consignacao)
- Acerto: nenhum

**Efeitos colaterais:**
- Item status: `ToSell` → `Sold`
- Se publicado no e-commerce → removido automaticamente
- Pedidos pendentes no e-commerce → cancelados com motivo "Item vendido na loja fisica"
- Numero de venda gerado: `V{YYYYMMDD}-{SEQ}` (ex: `V20260316-001`)

---

### 2a. Cliente comum + Peca de consignacao (€20,00) — Fornecedor optou por credito

**Dados de entrada:**
- Comprador: Cliente comum
- Item: Casaco, consignado pela Fornecedora Maria
- Maria configurada com: PorcInLoja = 50%
- Preco de venda: €20,00
- Pagamento: €20,00 em cartao

**Resultado esperado:**
- Credito em loja para Maria: €10,00 (€20,00 x 50%)
- Lucro da loja: €10,00 (€20,00 x 50%)
- Acerto: registro criado para Maria com €10,00 de credito

**Efeitos colaterais:**
- Item status: `ToSell` → `Sold`
- Maria ganha €10,00 de credito em loja (expira em 180 dias)
- Se publicado no e-commerce → removido automaticamente

---

### 2b. Cliente comum + Peca de consignacao (€20,00) — Fornecedor optou por dinheiro

**Dados de entrada:**
- Comprador: Cliente comum
- Item: Casaco, consignado pela Fornecedora Maria
- Maria configurada com: PorcInDinheiro = 40%
- Preco de venda: €20,00
- Pagamento: €20,00 em dinheiro

**Resultado esperado:**
- Dinheiro para Maria: €8,00 (€20,00 x 40%)
- Lucro da loja: €12,00 (€20,00 x 60%)
- Acerto: registro criado para Maria com €8,00 em dinheiro

**Efeitos colaterais:**
- Item status: `ToSell` → `Sold`
- Saldo de resgate de Maria atualizado: +€8,00
- Se publicado no e-commerce → removido automaticamente

---

### 3. Fornecedor compra peca propria da loja (€15,00)

**Dados de entrada:**
- Comprador: Fornecedora Maria (que tambem e cliente)
- Item: Sapatos, aquisicao propria da loja (NAO e consignacao)
- Preco de venda: €15,00
- Custo de aquisicao: €6,00
- Pagamento: €15,00 em cartao

**Resultado esperado:**
- Lucro da loja: €9,00 (€15,00 - €6,00)
- Credito fornecedor: nenhum (peca nao e consignacao)
- Acerto: nenhum

**Efeitos colaterais:**
- Item status: `ToSell` → `Sold`
- Mesmo sendo fornecedora, Maria e tratada como cliente comum nesta compra
- Se publicado no e-commerce → removido automaticamente

---

### 4.1. Fornecedor compra peca de outro fornecedor + Pagamento normal (€25,00)

**Dados de entrada:**
- Comprador: Fornecedora Maria
- Item: Bolsa, consignada pela Fornecedora Ana (PorcInLoja = 50%)
- Preco de venda: €25,00
- Pagamento: €25,00 em dinheiro (Maria paga normalmente)

**Resultado esperado:**
- **Ana (dona da peca):** recebe €12,50 em credito de loja (€25,00 x 50%)
- **Loja:** fica com €12,50 (€25,00 x 50%)
- **Maria (compradora):** nao ganha credito nesta venda — e apenas compradora

**Efeitos colaterais:**
- Item status: `ToSell` → `Sold`
- Credito em loja da Ana: +€12,50 (expira em 180 dias)
- Saldo da Maria: inalterado
- Se publicado no e-commerce → removido automaticamente

---

### 4.2. Fornecedor compra peca de outro fornecedor + Paga com credito (€20,00)

**Dados de entrada:**
- Comprador: Fornecedora Maria (tem €25,00 de credito em loja de vendas anteriores)
- Item: Saia, consignada pela Fornecedora Ana (PorcInLoja = 50%)
- Preco de venda: €20,00
- Pagamento: €20,00 usando credito em loja da Maria

**Resultado esperado:**
- **Ana (dona da peca):** recebe €10,00 em credito de loja (€20,00 x 50%)
- **Loja:** fica com €10,00 (€20,00 x 50%)
- **Maria (compradora):** Credito antes: €25,00 → Credito depois: €5,00

**Efeitos colaterais:**
- Item status: `ToSell` → `Sold`
- Credito em loja da Ana: +€10,00 (expira em 180 dias)
- Credito em loja da Maria: -€20,00 (consumido via FIFO dos creditos mais antigos)
- Se o credito da Maria chegasse a €0,00 → status `FullyUsed`
- Se publicado no e-commerce → removido automaticamente

---

### 4.3. Fornecedor compra peca de outro fornecedor + Pagamento misto (€30,00)

**Dados de entrada:**
- Comprador: Fornecedora Maria (tem €12,00 de credito em loja)
- Item: Jaqueta, consignada pela Fornecedora Ana (PorcInDinheiro = 40%)
- Preco de venda: €30,00
- Pagamento: €12,00 credito em loja + €18,00 cartao

**Resultado esperado:**
- **Ana (dona da peca):** recebe €12,00 em dinheiro (€30,00 x 40%)
- **Loja:** fica com €18,00 (€30,00 x 60%)
- **Maria (compradora):** Credito antes: €12,00 → Credito depois: €0,00 → status `FullyUsed`

**Efeitos colaterais:**
- Item status: `ToSell` → `Sold`
- Saldo de resgate da Ana: +€12,00
- Credito em loja da Maria: zerado, status `FullyUsed`
- Se publicado no e-commerce → removido automaticamente

---

## Fluxo Geral de Venda

1. **Verificar se o item esta publicado**
   - 1.1 Se sim, remover do e-commerce
   - 1.2 Cancelar pedidos pendentes/confirmados com motivo "Item vendido na loja fisica"

2. **Mudar o Status para "Vendido" no Inventory**
   - Item status: `ToSell` → `Sold`
   - Registar: `SoldAt`, `FinalSalePrice`, `SaleId`

3. **Verificar se o item e de consignacao**
   - 3.1 Se sim e fornecedor optou por credito → gerar credito em loja (50% do valor de venda, expira em 180 dias)
   - 3.2 Se sim e fornecedor optou por dinheiro → gerar saldo de resgate (40% do valor de venda)

4. **Processar pagamento**
   - Validar metodo(s) de pagamento (maximo 2)
   - Se pagamento com credito em loja → deduzir saldo (FIFO), criar `StoreCreditTransaction`
   - Se pagamento em dinheiro e valor pago > total → calcular troco

5. **Gerar Venda e Acerto (Atualizar saldo)**
   - Criar `SaleEntity` com numero `V{YYYYMMDD}-{SEQ}`
   - Criar `SaleItemEntity` para cada item
   - Criar `SalePaymentEntity` para cada pagamento
   - Se consignacao → registar no acerto do fornecedor
