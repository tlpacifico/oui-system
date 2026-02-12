# Oui Circular - Endpoints da API

## Versao: 2.0 | Ultima Atualizacao: 2026-02-11

**Base URL:** `https://api.ouisystem.com/api` (producao) | `https://localhost:5001/api` (desenvolvimento)

**Autenticacao:** Firebase JWT Bearer Token em todas as rotas (exceto Auth)

**Formato:** JSON (application/json)

**Paginacao padrao:** `?page=1&pageSize=20` | Resposta: `{ data: [], totalCount: number, page: number, pageSize: number }`

**Moeda:** EUR (Euro)

**Pais:** Portugal

---

## 1. Autenticacao

| Metodo | Endpoint | Descricao | Auth |
|--------|----------|-----------|------|
| POST | `/auth/login` | Validar token Firebase e retornar perfil do utilizador | Nao |
| POST | `/auth/refresh` | Renovar token de acesso | Sim |
| GET | `/auth/me` | Retornar dados do utilizador autenticado | Sim |

---

## 2. Dashboard

| Metodo | Endpoint | Descricao | Role Minimo |
|--------|----------|-----------|-------------|
| GET | `/dashboard/kpis` | KPIs resumidos: vendas hoje, receita mes, itens estoque, acertos pendentes | Cashier |
| GET | `/dashboard/sales-chart` | Dados para grafico de vendas (7/30 dias) | Cashier |
| GET | `/dashboard/alerts` | Alertas: consignacoes a expirar, estoque parado, caixas abertos | Cashier |
| GET | `/dashboard/top-sales` | Top 5 itens vendidos da semana | Cashier |

**Query params comuns:** `?period=today|week|month|custom&startDate=&endDate=`

---

## 3. Inventario - Itens

| Metodo | Endpoint | Descricao | Role Minimo |
|--------|----------|-----------|-------------|
| GET | `/items` | Listar itens com filtros e paginacao | Cashier |
| GET | `/items/:id` | Detalhe de um item | Cashier |
| POST | `/items` | Criar novo item | Manager |
| PUT | `/items/:id` | Atualizar item | Manager |
| DELETE | `/items/:id` | Soft-delete item | Manager |
| PATCH | `/items/:id/price` | Alterar preco (com regras de autorizacao) | Manager |
| GET | `/items/:id/price-history` | Historico de alteracoes de preco | Manager |
| GET | `/items/:id/history` | Log de acoes do item (criacao, edicao, venda) | Manager |
| POST | `/items/:id/photos` | Upload de foto(s) do item | Manager |
| DELETE | `/items/:id/photos/:photoId` | Remover foto do item | Manager |
| POST | `/items/batch/labels` | Gerar etiquetas em lote (IDs no body) | Manager |

**GET `/items` - Query params:**
```
?search=vestido          # Busca por nome/ID
&brandId=1               # Filtrar por marca
&tagIds=1,2,3            # Filtrar por tags
&size=M                  # Filtrar por tamanho
&color=Vermelho          # Filtrar por cor
&condition=Good          # Filtrar por condicao
&minPrice=50             # Preco minimo
&maxPrice=200            # Preco maximo
&status=ToSell           # Status: Evaluated, AwaitingAcceptance, ToSell, Sold, Returned
&supplierId=1            # Filtrar por fornecedor
&consignmentDateFrom=    # Data consignacao de
&consignmentDateTo=      # Data consignacao ate
&daysInStock=30          # Minimo de dias em estoque
&sortBy=name|price|date|daysInStock
&sortDir=asc|desc
&page=1&pageSize=20
```

---

## 4. Inventario - Marcas

| Metodo | Endpoint | Descricao | Role Minimo |
|--------|----------|-----------|-------------|
| GET | `/brands` | Listar marcas | Cashier |
| GET | `/brands/:id` | Detalhe da marca | Cashier |
| POST | `/brands` | Criar marca | Manager |
| PUT | `/brands/:id` | Atualizar marca | Manager |
| DELETE | `/brands/:id` | Excluir marca (se sem itens vinculados) | Manager |

---

## 5. Inventario - Tags/Categorias

| Metodo | Endpoint | Descricao | Role Minimo |
|--------|----------|-----------|-------------|
| GET | `/tags` | Listar tags | Cashier |
| POST | `/tags` | Criar tag | Manager |
| PUT | `/tags/:id` | Atualizar tag | Manager |
| DELETE | `/tags/:id` | Excluir tag | Manager |
| GET | `/categories` | Listar categorias (hierarquica) | Cashier |
| POST | `/categories` | Criar categoria | Manager |
| PUT | `/categories/:id` | Atualizar categoria | Manager |
| DELETE | `/categories/:id` | Excluir categoria | Manager |

---

## 6. Inventario - Alertas

| Metodo | Endpoint | Descricao | Role Minimo |
|--------|----------|-----------|-------------|
| GET | `/inventory/alerts` | Listar itens que precisam de acao | Manager |
| GET | `/inventory/alerts/summary` | Contagem por nivel (amarelo/laranja/vermelho) | Manager |

**Query params:** `?level=yellow|orange|red&page=1&pageSize=20`

---

## 7. Rececoes

> **Fluxo de consignacao em 2 etapas:** O cliente entrega as pecas na loja. A gerente cria uma rececao (contagem de itens e associacao ao fornecedor). Posteriormente, a gerente avalia cada peca individualmente. Apos a avaliacao, a consignacao e criada automaticamente com os itens aceites.

| Metodo | Endpoint | Descricao | Role Minimo |
|--------|----------|-----------|-------------|
| POST | `/receptions` | Criar rececao (contar itens, associar ao fornecedor) | Manager |
| GET | `/receptions` | Listar rececoes com filtros | Manager |
| GET | `/receptions/:id` | Detalhe da rececao | Manager |
| GET | `/receptions/:id/receipt` | Gerar recibo da rececao (PDF) | Manager |
| GET | `/receptions/pending-evaluation` | Listar rececoes pendentes de avaliacao | Manager |
| POST | `/receptions/:id/evaluate` | Submeter avaliacao de todos os itens | Manager |
| POST | `/receptions/:id/send-evaluation-email` | Enviar email de avaliacao ao cliente | Manager |

**POST `/receptions` - Body:**
```json
{
  "supplierExternalId": "guid",
  "receptionDate": "2026-02-11",
  "itemCount": 12,
  "notes": ""
}
```

**Response (201 Created):**
```json
{
  "id": 1,
  "supplierExternalId": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
  "supplierName": "Maria Silva",
  "receptionDate": "2026-02-11",
  "itemCount": 12,
  "status": "PendingEvaluation",
  "notes": "",
  "createdAt": "2026-02-11T10:30:00Z",
  "createdBy": "Ana Costa"
}
```

**GET `/receptions` - Query params:**
```
?supplierId=1
&status=PendingEvaluation|Evaluated|ConsignmentCreated
&dateFrom=&dateTo=
&page=1&pageSize=20
```

**POST `/receptions/:id/evaluate` - Body:**
```json
{
  "items": [
    {
      "description": "Vestido midi estampado",
      "brandId": 1,
      "size": "M",
      "color": "Estampado",
      "condition": "Excellent",
      "composition": "100% algodao",
      "evaluatedPrice": 18.00,
      "tags": [1, 2],
      "isRejected": false,
      "rejectionReason": null
    },
    {
      "description": "Casaco de la azul",
      "brandId": 3,
      "size": "L",
      "color": "Azul",
      "condition": "Good",
      "composition": "80% la, 20% poliester",
      "evaluatedPrice": 25.00,
      "tags": [1, 5],
      "isRejected": false,
      "rejectionReason": null
    },
    {
      "description": "T-shirt com nodoas",
      "brandId": 2,
      "size": "S",
      "color": "Branco",
      "condition": "Poor",
      "composition": "100% algodao",
      "evaluatedPrice": 0,
      "tags": [],
      "isRejected": true,
      "rejectionReason": "Nodoas permanentes, nao vendavel"
    }
  ]
}
```

**Response (200 OK):**
```json
{
  "receptionId": 1,
  "status": "Evaluated",
  "totalItems": 12,
  "acceptedItems": 10,
  "rejectedItems": 2,
  "totalEvaluatedValue": 185.00,
  "consignmentId": 45,
  "consignmentCreatedAutomatically": true
}
```

> **Nota:** Apos a avaliacao, se houver itens aceites, uma consignacao e criada automaticamente. Os itens rejeitados ficam registados na rececao para devolucao ao cliente.

**POST `/receptions/:id/send-evaluation-email` - Body (opcional):**
```json
{
  "customMessage": "Ola Maria, a avaliacao das suas pecas esta concluida. Por favor consulte o detalhe em anexo."
}
```

---

## 8. Consignacoes

> **Nota:** As consignacoes sao criadas automaticamente a partir de uma rececao avaliada. Nao existe endpoint POST para criacao direta de consignacoes.

| Metodo | Endpoint | Descricao | Role Minimo |
|--------|----------|-----------|-------------|
| GET | `/consignments` | Listar consignacoes com filtros | Manager |
| GET | `/consignments/:id` | Detalhe da consignacao com itens | Manager |
| PUT | `/consignments/:id` | Atualizar consignacao | Manager |
| DELETE | `/consignments/:id` | Cancelar/excluir consignacao | Manager |
| POST | `/consignments/:id/items` | Adicionar item a consignacao | Manager |
| PATCH | `/consignments/:id/renew` | Renovar consignacao (+30 dias) | Manager |
| PATCH | `/consignments/:id/return` | Devolver itens ao fornecedor | Manager |
| GET | `/consignments/:id/contract` | Gerar PDF do contrato | Manager |

**GET `/consignments` - Query params:**
```
?supplierId=1
&status=Active|Expiring|Expired|Closed
&dateFrom=&dateTo=
&page=1&pageSize=20
```

---

## 9. Fornecedores

| Metodo | Endpoint | Descricao | Role Minimo |
|--------|----------|-----------|-------------|
| GET | `/suppliers` | Listar fornecedores | Manager |
| GET | `/suppliers/:id` | Detalhe do fornecedor | Manager |
| POST | `/suppliers` | Criar fornecedor | Manager |
| PUT | `/suppliers/:id` | Atualizar fornecedor | Manager |
| DELETE | `/suppliers/:id` | Soft-delete fornecedor | Manager |
| GET | `/suppliers/:id/consignments` | Consignacoes do fornecedor | Manager |
| GET | `/suppliers/:id/items` | Itens do fornecedor | Manager |
| GET | `/suppliers/:id/settlements` | Acertos do fornecedor | Finance |
| GET | `/suppliers/:id/stats` | Estatisticas do fornecedor | Manager |

---

## 10. Ponto de Venda (POS)

### 10.1 Caixa (Cash Register)

| Metodo | Endpoint | Descricao | Role Minimo |
|--------|----------|-----------|-------------|
| POST | `/pos/register/open` | Abrir caixa (body: openingAmount) | Cashier |
| POST | `/pos/register/close` | Fechar caixa (body: countedAmount, notes) | Cashier |
| GET | `/pos/register/current` | Status do caixa atual do utilizador logado | Cashier |
| GET | `/pos/register/status` | Status de todos os caixas (para monitorizacao) | Manager |
| GET | `/pos/register/history` | Historico de aberturas/fechamentos | Manager |

### 10.2 Vendas

| Metodo | Endpoint | Descricao | Role Minimo |
|--------|----------|-----------|-------------|
| POST | `/pos/sales` | Criar/processar venda | Cashier |
| GET | `/pos/sales` | Listar vendas com filtros | Cashier |
| GET | `/pos/sales/:id` | Detalhe da venda | Cashier |
| POST | `/pos/sales/:id/void` | Cancelar/estornar venda | Manager |
| GET | `/pos/sales/:id/receipt` | Gerar recibo (PDF) | Cashier |
| GET | `/pos/sales/today` | Resumo de vendas do dia | Cashier |

**POST `/pos/sales` - Body:**
```json
{
  "items": [
    { "itemId": 123, "discount": 0 }
  ],
  "customerId": null,
  "payments": [
    { "method": "Cash", "amount": 30.00 },
    { "method": "MBWAY", "amount": 20.00 }
  ],
  "discountPercentage": 5,
  "discountReason": "Promocao quinzenal",
  "notes": ""
}
```

**Metodos de pagamento (enum):**
| Valor | Nome |
|-------|------|
| 1 | Cash |
| 2 | CreditCard |
| 3 | DebitCard |
| 4 | MBWAY |
| 5 | StoreCredit |

**GET `/pos/sales` - Query params:**
```
?dateFrom=&dateTo=
&cashierId=
&paymentMethod=Cash|CreditCard|DebitCard|MBWAY|StoreCredit
&page=1&pageSize=20
```

### 10.3 Devolucoes

| Metodo | Endpoint | Descricao | Role Minimo |
|--------|----------|-----------|-------------|
| POST | `/pos/returns` | Processar devolucao | Manager |
| GET | `/pos/returns` | Listar devolucoes | Manager |
| GET | `/pos/returns/:id` | Detalhe da devolucao | Manager |

**POST `/pos/returns` - Body:**
```json
{
  "saleId": 456,
  "items": [
    { "itemId": 123, "reason": "Defeito na costura" }
  ],
  "resolutionType": "StoreCredit",
  "notes": ""
}
```

---

## 11. Financeiro - Acertos (Settlements)

> **Modelo de comissao Oui Circular:**
> - Pagamento em dinheiro/MBWAY/transferencia: cliente recebe **40%** do valor de venda, loja fica com **60%**
> - Pagamento em credito de loja: cliente recebe **50%** do valor de venda, loja fica com **50%**

| Metodo | Endpoint | Descricao | Role Minimo |
|--------|----------|-----------|-------------|
| GET | `/finance/settlements` | Listar acertos | Finance |
| GET | `/finance/settlements/pending` | Acertos pendentes agrupados por fornecedor | Finance |
| GET | `/finance/settlements/:id` | Detalhe do acerto | Finance |
| POST | `/finance/settlements` | Criar/processar acerto | Finance |
| GET | `/finance/settlements/:id/receipt` | Gerar recibo PDF do acerto | Finance |
| POST | `/finance/settlements/preview` | Preview do calculo antes de confirmar | Finance |

**POST `/finance/settlements` - Body:**
```json
{
  "supplierId": 1,
  "periodStart": "2026-02-01",
  "periodEnd": "2026-02-28",
  "paymentType": "Cash",
  "notes": ""
}
```

**POST `/finance/settlements/preview` - Body:**
```json
{
  "supplierId": 1,
  "periodStart": "2026-02-01",
  "periodEnd": "2026-02-28",
  "paymentType": "Cash"
}
```

**Response (paymentType = Cash) - Cliente recebe 40%, loja fica 60%:**
```json
{
  "supplierId": 1,
  "supplierName": "Maria Silva",
  "items": [
    {
      "itemId": 123,
      "itemName": "Vestido Floral",
      "saleDate": "2026-02-15",
      "salePrice": 50.00,
      "commissionRate": 60,
      "storeCommission": 30.00,
      "supplierPayment": 20.00
    },
    {
      "itemId": 124,
      "itemName": "Casaco de La",
      "saleDate": "2026-02-18",
      "salePrice": 40.00,
      "commissionRate": 60,
      "storeCommission": 24.00,
      "supplierPayment": 16.00
    }
  ],
  "totalSales": 90.00,
  "totalCommission": 54.00,
  "totalPayable": 36.00,
  "paymentType": "Cash"
}
```

**Response (paymentType = StoreCredit) - Cliente recebe 50%, loja fica 50%:**
```json
{
  "supplierId": 1,
  "supplierName": "Maria Silva",
  "items": [
    {
      "itemId": 123,
      "itemName": "Vestido Floral",
      "saleDate": "2026-02-15",
      "salePrice": 50.00,
      "commissionRate": 50,
      "storeCommission": 25.00,
      "supplierPayment": 25.00
    },
    {
      "itemId": 124,
      "itemName": "Casaco de La",
      "saleDate": "2026-02-18",
      "salePrice": 40.00,
      "commissionRate": 50,
      "storeCommission": 20.00,
      "supplierPayment": 20.00
    }
  ],
  "totalSales": 90.00,
  "totalCommission": 45.00,
  "totalPayable": 45.00,
  "paymentType": "StoreCredit"
}
```

---

## 12. Financeiro - Creditos em Loja

| Metodo | Endpoint | Descricao | Role Minimo |
|--------|----------|-----------|-------------|
| GET | `/finance/credits` | Listar creditos | Finance |
| GET | `/finance/credits/:id` | Detalhe do credito com extrato | Finance |
| POST | `/finance/credits` | Criar credito manual | Finance |
| GET | `/finance/credits/search` | Buscar credito por titular (para uso no POS) | Cashier |

---

## 13. Financeiro - Fluxo de Caixa e Despesas

| Metodo | Endpoint | Descricao | Role Minimo |
|--------|----------|-----------|-------------|
| GET | `/finance/cashflow` | Fluxo de caixa (entradas/saidas) | Finance |
| GET | `/finance/cashflow/summary` | Resumo: total entradas, saidas, saldo | Finance |
| GET | `/finance/expenses` | Listar despesas | Finance |
| POST | `/finance/expenses` | Registar despesa | Finance |
| PUT | `/finance/expenses/:id` | Atualizar despesa | Finance |
| DELETE | `/finance/expenses/:id` | Excluir despesa | Finance |
| GET | `/finance/expenses/categories` | Categorias de despesa | Finance |

---

## 14. Relatorios

| Metodo | Endpoint | Descricao | Role Minimo |
|--------|----------|-----------|-------------|
| GET | `/reports/sales` | Relatorio de vendas | Manager |
| GET | `/reports/sales/export` | Exportar relatorio vendas (Excel/PDF) | Manager |
| GET | `/reports/inventory` | Relatorio de inventario | Manager |
| GET | `/reports/inventory/aging` | Analise de aging do estoque | Manager |
| GET | `/reports/inventory/sell-through` | Taxa de venda por categoria/marca | Manager |
| GET | `/reports/suppliers` | Relatorio de fornecedores | Manager |
| GET | `/reports/suppliers/ranking` | Ranking de fornecedores | Manager |
| GET | `/reports/finance` | Relatorio financeiro consolidado | Finance |
| GET | `/reports/finance/projection` | Projecao de fluxo de caixa | Finance |

**Query params comuns para relatorios:**
```
?startDate=2026-01-01
&endDate=2026-01-31
&groupBy=day|week|month
&brandId=
&categoryId=
&supplierId=
&format=json|pdf|excel
```

---

## 15. Clientes

| Metodo | Endpoint | Descricao | Role Minimo |
|--------|----------|-----------|-------------|
| GET | `/customers` | Listar clientes | Cashier |
| GET | `/customers/:id` | Detalhe do cliente | Cashier |
| POST | `/customers` | Cadastrar cliente | Cashier |
| PUT | `/customers/:id` | Atualizar cliente | Cashier |
| DELETE | `/customers/:id` | Soft-delete cliente | Manager |
| GET | `/customers/:id/purchases` | Historico de compras | Cashier |
| GET | `/customers/:id/loyalty` | Dados de fidelidade (pontos, nivel) | Cashier |
| POST | `/customers/:id/loyalty/redeem` | Resgatar pontos | Cashier |
| GET | `/customers/search` | Busca rapida (nome, NIF, telefone) | Cashier |

---

## 16. Programa de Fidelidade

| Metodo | Endpoint | Descricao | Role Minimo |
|--------|----------|-----------|-------------|
| GET | `/loyalty/dashboard` | Dashboard do programa | Manager |
| GET | `/loyalty/birthdays` | Aniversariantes do mes | Manager |
| GET | `/loyalty/config` | Configuracao do programa | Admin |
| PUT | `/loyalty/config` | Atualizar configuracao | Admin |

---

## 17. Promocoes/Campanhas

| Metodo | Endpoint | Descricao | Role Minimo |
|--------|----------|-----------|-------------|
| GET | `/promotions` | Listar campanhas | Manager |
| GET | `/promotions/:id` | Detalhe da campanha | Manager |
| POST | `/promotions` | Criar campanha | Manager |
| PUT | `/promotions/:id` | Atualizar campanha | Manager |
| DELETE | `/promotions/:id` | Excluir campanha | Manager |
| GET | `/promotions/active` | Promocoes ativas (para POS) | Cashier |
| GET | `/promotions/:id/preview` | Preview de itens afetados | Manager |

---

## 18. Administracao

### 18.1 Utilizadores

| Metodo | Endpoint | Descricao | Role Minimo |
|--------|----------|-----------|-------------|
| GET | `/admin/users` | Listar utilizadores | Admin |
| GET | `/admin/users/:id` | Detalhe do utilizador | Admin |
| POST | `/admin/users` | Criar utilizador (convite) | Admin |
| PUT | `/admin/users/:id` | Atualizar perfil/permissoes | Admin |
| PATCH | `/admin/users/:id/deactivate` | Desativar utilizador | Admin |
| PATCH | `/admin/users/:id/activate` | Reativar utilizador | Admin |

### 18.2 Configuracoes

| Metodo | Endpoint | Descricao | Role Minimo |
|--------|----------|-----------|-------------|
| GET | `/admin/settings` | Obter todas as configuracoes | Admin |
| PUT | `/admin/settings` | Atualizar configuracoes | Admin |
| GET | `/admin/settings/:group` | Obter grupo especifico (consignment, pos, loyalty, alerts, fiscal) | Admin |
| PUT | `/admin/settings/:group` | Atualizar grupo especifico | Admin |

### 18.3 Audit Log

| Metodo | Endpoint | Descricao | Role Minimo |
|--------|----------|-----------|-------------|
| GET | `/admin/audit-log` | Listar logs de auditoria | Admin |
| GET | `/admin/audit-log/export` | Exportar logs (CSV) | Admin |

**Query params:**
```
?userId=
&action=Create|Update|Delete|Login|PriceChange|Discount|Void
&module=Inventory|Reception|Consignment|POS|Finance|Admin
&dateFrom=&dateTo=
&page=1&pageSize=50
```

### 18.4 Dados da Loja

| Metodo | Endpoint | Descricao | Role Minimo |
|--------|----------|-----------|-------------|
| GET | `/admin/store` | Dados da loja | Admin |
| PUT | `/admin/store` | Atualizar dados da loja | Admin |
| POST | `/admin/store/logo` | Upload do logo | Admin |

---

## 19. Portal do Fornecedor

**Base URL:** `/portal` (autenticacao separada por token do fornecedor)

| Metodo | Endpoint | Descricao | Auth |
|--------|----------|-----------|------|
| POST | `/portal/auth/login` | Login do fornecedor | Nao |
| GET | `/portal/dashboard` | KPIs do fornecedor | Supplier |
| GET | `/portal/items` | Meus itens com filtros | Supplier |
| GET | `/portal/items/:id` | Detalhe de um item | Supplier |
| GET | `/portal/receptions` | Minhas rececoes | Supplier |
| GET | `/portal/receptions/:id` | Detalhe da rececao com avaliacao | Supplier |
| GET | `/portal/statements` | Extrato de acertos | Supplier |
| GET | `/portal/statements/:id` | Detalhe do acerto | Supplier |
| GET | `/portal/statements/:id/receipt` | Download do recibo PDF | Supplier |

---

## 20. Notificacoes

| Metodo | Endpoint | Descricao | Role Minimo |
|--------|----------|-----------|-------------|
| GET | `/notifications` | Listar notificacoes do utilizador | Cashier |
| PATCH | `/notifications/:id/read` | Marcar como lida | Cashier |
| PATCH | `/notifications/read-all` | Marcar todas como lidas | Cashier |
| GET | `/notifications/unread-count` | Contagem de nao lidas (para badge) | Cashier |

---

## Codigos de Resposta HTTP

| Codigo | Significado |
|--------|-------------|
| 200 | OK - Sucesso |
| 201 | Created - Recurso criado |
| 204 | No Content - Sucesso sem corpo |
| 400 | Bad Request - Dados invalidos (validacao) |
| 401 | Unauthorized - Token ausente ou invalido |
| 403 | Forbidden - Sem permissao para a acao |
| 404 | Not Found - Recurso nao encontrado |
| 409 | Conflict - Conflito (ex: caixa ja aberto) |
| 422 | Unprocessable Entity - Regra de negocio violada |
| 500 | Internal Server Error |

## Formato de Erro Padrao

```json
{
  "type": "ValidationError",
  "title": "Um ou mais erros de validacao ocorreram",
  "status": 400,
  "errors": {
    "Name": ["O nome e obrigatorio"],
    "Price": ["O preco deve ser maior que zero"]
  }
}
```
