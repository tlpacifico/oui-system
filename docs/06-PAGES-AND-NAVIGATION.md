# Oui Circular - Paginas do Sistema e Navegacao

## Versao: 2.0 | Ultima Atualizacao: 2026-02-11

---

## 1. Mapa de Navegacao

```
┌─────────────────────────────────────────────────────────────────────┐
│                        LAYOUT PRINCIPAL                             │
│  ┌──────────┐  ┌──────────────────────────────────────────────────┐ │
│  │          │  │  Header: Logo | Pesquisa Global | Notificacoes | │ │
│  │          │  │          Perfil do Utilizador                    │ │
│  │          │  ├──────────────────────────────────────────────────┤ │
│  │  SIDEBAR │  │                                                  │ │
│  │          │  │              CONTEUDO DA PAGINA                  │ │
│  │  Menu de │  │                                                  │ │
│  │ Navegacao│  │                                                  │ │
│  │          │  │                                                  │ │
│  │          │  │                                                  │ │
│  └──────────┘  └──────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────────┘
```

---

## 2. Estrutura do Menu Lateral (Sidebar)

```
Dashboard
│
├── Inventario
│   ├── Pecas
│   ├── Marcas
│   ├── Tags/Categorias
│   └── Alertas de Stock
│
├── Consignacoes
│   ├── Rececoes
│   ├── Avaliacoes Pendentes
│   ├── Consignacoes
│   ├── Devolucoes ao Fornecedor
│   └── Fornecedores
│
├── Ponto de Venda (POS)
│   ├── Caixa
│   ├── Vendas do Dia
│   └── Devolucoes/Trocas
│
├── Financeiro
│   ├── Acertos com Fornecedores
│   ├── Creditos em Loja
│   ├── Fluxo de Caixa
│   └── Despesas
│
├── Relatorios
│   ├── Vendas
│   ├── Inventario
│   ├── Fornecedores
│   └── Financeiro
│
├── Clientes
│   ├── Lista de Clientes
│   └── Programa de Fidelidade
│
├── Promocoes
│   ├── Campanhas Ativas
│   └── Nova Campanha
│
└── Administracao
    ├── Utilizadores
    ├── Configuracoes
    ├── Audit Log
    └── A Minha Loja
```

---

## 3. Fluxo de Navegacao por Modulo

### 3.1 Fluxo: Login -> Dashboard

```
Login ──► Dashboard (Home)
             │
             ├──► KPIs do dia (vendas, receita, pecas)
             ├──► Alertas pendentes (consignacoes a expirar, stock parado)
             ├──► Grafico de vendas da semana
             └──► Acoes rapidas (nova venda, nova rececao, pesquisar peca)
```

### 3.2 Fluxo: Rececao e Avaliacao de Consignacao

O processo de consignacao esta dividido em dois passos distintos: **Rececao** (contagem e recibo) e **Avaliacao** (definicao de precos e condicao). Isto permite receber pecas rapidamente na loja e avaliar mais tarde com calma.

```
Sidebar: Consignacoes > Rececoes
    │
    ══════════════════════════════════════════════
    PASSO 1 - RECECAO DE PECAS (/consignments/receive)
    ══════════════════════════════════════════════
    │
    ├── 1. Selecionar/Registar Fornecedor
    │       └── [Modal] Registo rapido de fornecedor (se novo)
    │
    ├── 2. Contar e registar pecas recebidas
    │       ├── Preencher: descricao breve, quantidade
    │       ├── [Opcional] Foto rapida de cada peca
    │       └── Sistema gera ID automatico para cada peca
    │
    ├── 3. Confirmar rececao
    │       ├── Resumo: fornecedor, nr. de pecas, data de entrada
    │       └── Imprimir recibo de rececao (entrega ao fornecedor)
    │
    │       ↓ (mais tarde)
    │
    ══════════════════════════════════════════════
    PASSO 2 - AVALIAR PECAS (/consignments/:id/evaluate)
    ══════════════════════════════════════════════
    │
    ├── 4. Aceder via Sidebar: Consignacoes > Avaliacoes Pendentes
    │       └── Lista de rececoes aguardando avaliacao
    │
    ├── 5. Selecionar rececao para avaliar
    │
    ├── 6. Avaliar cada peca (loop)
    │       ├── Definir: nome completo, marca, tamanho, cor
    │       ├── Definir: condicao (Novo, Como Novo, Bom, Aceitavel)
    │       ├── Definir: preco de venda (€)
    │       ├── Definir: comissao do fornecedor (%)
    │       ├── [Opcional] Upload de fotos adicionais
    │       ├── [Opcional] Rejeitar peca (com motivo)
    │       └── Repetir para proxima peca
    │
    ├── 7. Rever avaliacao completa
    │
    └── 8. Finalizar avaliacao
            ├── Enviar email de avaliacao ao fornecedor
            │       └── Email inclui: lista de pecas aceites com precos,
            │           pecas rejeitadas com motivo, prazo de resposta
            ├── Aguardar aprovacao do fornecedor
            └── Apos aprovacao: pecas ficam disponiveis para venda
```

**Resumo do fluxo:**
```
Rececao de Pecas (contagem + recibo)
    ↓ (mais tarde)
Avaliar Pecas (preco + condicao)
    ↓
Enviar Email de Avaliacao
    ↓
Pecas Disponiveis para Venda
```

### 3.3 Fluxo: Processo de Venda (POS)

```
Sidebar: POS > Caixa
    │
    ├── [Se caixa fechada] Abrir Caixa
    │       └── Informar valor de abertura (troco)
    │
    ├── Ecra de Venda
    │   ├── Pesquisar peca (scanner/texto)
    │   ├── Adicionar ao carrinho
    │   ├── [Opcional] Aplicar desconto
    │   ├── [Opcional] Identificar cliente fidelidade
    │   ├── Finalizar venda
    │   │   ├── Selecionar forma de pagamento
    │   │   │   ├── Dinheiro
    │   │   │   ├── Cartao de Credito
    │   │   │   ├── Cartao de Debito
    │   │   │   ├── MBWAY
    │   │   │   └── Credito em Loja
    │   │   ├── Processar pagamento
    │   │   └── Gerar recibo
    │   └── [Opcional] Imprimir recibo
    │
    └── Fechar Caixa
            ├── Contar dinheiro
            ├── Informar valor contado
            ├── Justificar discrepancia (se houver)
            └── Gerar relatorio de fecho
```

### 3.4 Fluxo: Acerto com Fornecedor

```
Sidebar: Financeiro > Acertos com Fornecedores
    │
    ├── 1. Lista de acertos pendentes (agrupados por fornecedor)
    │
    ├── 2. Selecionar fornecedor
    │       └── Sistema mostra pecas vendidas no periodo
    │
    ├── 3. Rever calculo
    │       ├── Lista de pecas vendidas
    │       ├── Comissao por peca
    │       └── Total a pagar (€)
    │
    ├── 4. Selecionar forma de pagamento (dinheiro ou credito)
    │
    └── 5. Confirmar pagamento
            ├── Gerar recibo de acerto
            └── Atualizar estado das pecas
```

### 3.5 Fluxo: Devolucao/Troca

```
Sidebar: POS > Devolucoes/Trocas
    │
    ├── 1. Pesquisar venda original (por numero ou data)
    │
    ├── 2. Selecionar pecas para devolucao
    │
    ├── 3. Motivo da devolucao
    │
    ├── 4. Tipo de resolucao
    │       ├── Troca por outra peca ──► Ecra de nova venda com credito
    │       └── Credito em loja ──► Gerar credito para o cliente
    │
    └── 5. Confirmar e processar
```

---

## 4. Hierarquia de Paginas Completa

| # | Pagina | Rota Angular | Modulo | Acesso Minimo |
|---|--------|-------------|--------|---------------|
| 1 | Login | `/login` | Auth | Publico |
| 2 | Dashboard | `/dashboard` | Core | Cashier |
| 3 | Lista de Pecas | `/inventory/items` | M1 | Cashier (leitura) |
| 4 | Detalhe da Peca | `/inventory/items/:id` | M1 | Cashier (leitura) |
| 5 | Registo/Edicao de Peca | `/inventory/items/:id/edit` | M1 | Manager |
| 6 | Marcas | `/inventory/brands` | M1 | Manager |
| 7 | Tags/Categorias | `/inventory/tags` | M1 | Manager |
| 8 | Alertas de Stock | `/inventory/alerts` | M1 | Manager |
| 9 | Lista de Consignacoes | `/consignments` | M2 | Manager |
| 10 | Rececao de Pecas | `/consignments/receive` | M2 | Manager |
| 11 | Avaliacoes Pendentes | `/consignments/pending-evaluations` | M2 | Manager |
| 12 | Avaliar Rececao | `/consignments/:id/evaluate` | M2 | Manager |
| 13 | Detalhe da Consignacao | `/consignments/:id` | M2 | Manager |
| 13b | Devolucoes ao Fornecedor | `/consignments/returns` | M2 | Manager |
| 14 | Lista de Fornecedores | `/suppliers` | M2 | Manager |
| 15 | Detalhe do Fornecedor | `/suppliers/:id` | M2 | Manager |
| 16 | Registo/Edicao Fornecedor | `/suppliers/:id/edit` | M2 | Manager |
| 17 | POS - Caixa | `/pos` | M3 | Cashier |
| 18 | POS - Abrir Caixa | `/pos/open` | M3 | Cashier |
| 19 | POS - Fechar Caixa | `/pos/close` | M3 | Cashier |
| 20 | POS - Vendas do Dia | `/pos/sales` | M3 | Cashier |
| 21 | POS - Detalhe da Venda | `/pos/sales/:id` | M3 | Cashier |
| 22 | POS - Devolucoes | `/pos/returns` | M3 | Manager |
| 23 | POS - Nova Devolucao | `/pos/returns/new` | M3 | Manager |
| 24 | Acertos com Fornecedores | `/finance/settlements` | M4 | Finance |
| 25 | Novo Acerto | `/finance/settlements/new` | M4 | Finance |
| 26 | Detalhe do Acerto | `/finance/settlements/:id` | M4 | Finance |
| 27 | Creditos em Loja | `/finance/credits` | M4 | Finance |
| 28 | Fluxo de Caixa | `/finance/cashflow` | M4 | Finance |
| 29 | Despesas | `/finance/expenses` | M4 | Finance |
| 30 | Relatorio de Vendas | `/reports/sales` | M5 | Manager |
| 31 | Relatorio de Inventario | `/reports/inventory` | M5 | Manager |
| 32 | Relatorio de Fornecedores | `/reports/suppliers` | M5 | Manager |
| 33 | Relatorio Financeiro | `/reports/finance` | M5 | Finance |
| 34 | Lista de Clientes | `/customers` | M6 | Cashier |
| 35 | Detalhe do Cliente | `/customers/:id` | M6 | Cashier |
| 36 | Programa de Fidelidade | `/customers/loyalty` | M6 | Manager |
| 37 | Campanhas/Promocoes | `/promotions` | Extra | Manager |
| 38 | Nova Campanha | `/promotions/new` | Extra | Manager |
| 39 | Utilizadores | `/admin/users` | M8 | Admin |
| 40 | Configuracoes do Sistema | `/admin/settings` | M8 | Admin |
| 41 | Audit Log | `/admin/audit-log` | M8 | Admin |
| 42 | Dados da Loja | `/admin/store` | M8 | Admin |
| 43 | Portal do Fornecedor - Dashboard | `/portal/dashboard` | Portal | Supplier |
| 44 | Portal do Fornecedor - Pecas | `/portal/items` | Portal | Supplier |
| 45 | Portal do Fornecedor - Extrato | `/portal/statements` | Portal | Supplier |

---

## 5. Navegacao por Perfil de Utilizador

### Caixa (Cashier)
```
Dashboard ─── POS (Caixa) ─── Vendas do Dia
              Inventario (somente leitura)
              Clientes (registo basico)
```

### Gerente (Manager)
```
Dashboard ─── Inventario (completo)
              Consignacoes (completo: rececoes + avaliacoes)
              POS (completo + devolucoes)
              Relatorios (vendas, inventario, fornecedores)
              Promocoes
              Clientes
```

### Financeiro (Finance)
```
Dashboard ─── Acertos com Fornecedores
              Creditos em Loja
              Fluxo de Caixa
              Despesas
              Relatorios Financeiros
```

### Admin
```
Acesso total a todas as paginas + Administracao
```

### Fornecedor (Supplier) - Portal Externo
```
Portal Dashboard ─── As Minhas Pecas ─── O Meu Extrato
```

---

## 6. Componentes de Navegacao Globais

| Componente | Localizacao | Descricao |
|-----------|-------------|-----------|
| **Sidebar** | Esquerda | Menu principal colapsavel com icones e texto |
| **Header** | Topo | Logo, pesquisa global, notificacoes (badge), avatar do utilizador |
| **Breadcrumb** | Abaixo do header | Caminho de navegacao: Dashboard > Consignacoes > #CON-001 |
| **Pesquisa Global** | Header | Pesquisa unificada: pecas, fornecedores, vendas, clientes |
| **Notificacoes** | Header (sino) | Dropdown com alertas: avaliacoes pendentes, consignacoes a expirar, stock parado, caixas abertas |
| **Quick Actions** | Dashboard + FAB | Botao flutuante com acoes rapidas: Nova Venda, Nova Rececao, Pesquisar Peca |
