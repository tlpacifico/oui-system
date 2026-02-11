# SHS - Páginas do Sistema e Navegação

## Versão: 1.0 | Última Atualização: 2026-02-11

---

## 1. Mapa de Navegação

```
┌─────────────────────────────────────────────────────────────────────┐
│                        LAYOUT PRINCIPAL                             │
│  ┌──────────┐  ┌──────────────────────────────────────────────────┐ │
│  │          │  │  Header: Logo | Busca Global | Notificações |   │ │
│  │          │  │          Perfil do Usuário                      │ │
│  │          │  ├──────────────────────────────────────────────────┤ │
│  │  SIDEBAR │  │                                                  │ │
│  │          │  │              CONTEÚDO DA PÁGINA                  │ │
│  │  Menu de │  │                                                  │ │
│  │ Navegação│  │                                                  │ │
│  │          │  │                                                  │ │
│  │          │  │                                                  │ │
│  └──────────┘  └──────────────────────────────────────────────────┘ │
└─────────────────────────────────────────────────────────────────────┘
```

---

## 2. Estrutura do Menu Lateral (Sidebar)

```
📊 Dashboard
│
├── 👕 Inventário
│   ├── Itens
│   ├── Marcas
│   ├── Tags/Categorias
│   └── Alertas de Estoque
│
├── 📋 Consignações
│   ├── Lista de Consignações
│   ├── Nova Consignação
│   ├── Fornecedores
│   └── Contratos
│
├── 💰 Ponto de Venda (POS)
│   ├── Caixa
│   ├── Vendas do Dia
│   └── Devoluções/Trocas
│
├── 💵 Financeiro
│   ├── Acertos com Fornecedores
│   ├── Créditos em Loja
│   ├── Fluxo de Caixa
│   └── Despesas
│
├── 📈 Relatórios
│   ├── Vendas
│   ├── Inventário
│   ├── Fornecedores
│   └── Financeiro
│
├── 👥 Clientes
│   ├── Lista de Clientes
│   └── Programa de Fidelidade
│
├── 🏪 Promoções
│   ├── Campanhas Ativas
│   └── Nova Campanha
│
└── ⚙️ Administração
    ├── Usuários
    ├── Configurações
    ├── Audit Log
    └── Minha Loja
```

---

## 3. Fluxo de Navegação por Módulo

### 3.1 Fluxo: Login → Dashboard

```
Login ──► Dashboard (Home)
             │
             ├──► KPIs do dia (vendas, receita, itens)
             ├──► Alertas pendentes (consignações expirando, estoque parado)
             ├──► Gráfico de vendas da semana
             └──► Ações rápidas (nova venda, nova consignação, buscar item)
```

### 3.2 Fluxo: Recebimento de Consignação

```
Sidebar: Consignações > Nova Consignação
    │
    ├── 1. Selecionar/Cadastrar Fornecedor
    │       └── [Modal] Cadastro rápido de fornecedor (se novo)
    │
    ├── 2. Definir dados da consignação (data, período)
    │
    ├── 3. Adicionar itens (loop)
    │       ├── Preencher: nome, marca, tamanho, cor, condição, preço
    │       ├── [Opcional] Upload de foto
    │       ├── Sistema gera ID automático
    │       └── Repetir para próximo item
    │
    ├── 4. Revisar consignação completa
    │
    └── 5. Finalizar
            ├── Gerar contrato PDF
            └── Redirecionar para lista de consignações
```

### 3.3 Fluxo: Processo de Venda (POS)

```
Sidebar: POS > Caixa
    │
    ├── [Se caixa fechado] Abrir Caixa
    │       └── Informar valor de abertura (troco)
    │
    ├── Tela de Venda
    │   ├── Buscar item (scanner/texto)
    │   ├── Adicionar ao carrinho
    │   ├── [Opcional] Aplicar desconto
    │   ├── [Opcional] Identificar cliente fidelidade
    │   ├── Finalizar venda
    │   │   ├── Selecionar forma de pagamento
    │   │   ├── Processar pagamento
    │   │   └── Gerar recibo
    │   └── [Opcional] Imprimir recibo
    │
    └── Fechar Caixa
            ├── Contar dinheiro
            ├── Informar valor contado
            ├── Justificar discrepância (se houver)
            └── Gerar relatório de fechamento
```

### 3.4 Fluxo: Acerto com Fornecedor

```
Sidebar: Financeiro > Acertos com Fornecedores
    │
    ├── 1. Lista de acertos pendentes (agrupados por fornecedor)
    │
    ├── 2. Selecionar fornecedor
    │       └── Sistema mostra itens vendidos no período
    │
    ├── 3. Revisar cálculo
    │       ├── Lista de itens vendidos
    │       ├── Comissão por item
    │       └── Total a pagar
    │
    ├── 4. Selecionar forma de pagamento (dinheiro ou crédito)
    │
    └── 5. Confirmar pagamento
            ├── Gerar recibo de acerto
            └── Atualizar status dos itens
```

### 3.5 Fluxo: Devolução/Troca

```
Sidebar: POS > Devoluções/Trocas
    │
    ├── 1. Buscar venda original (por número ou data)
    │
    ├── 2. Selecionar itens para devolução
    │
    ├── 3. Motivo da devolução
    │
    ├── 4. Tipo de resolução
    │       ├── Troca por outro item ──► Tela de nova venda com crédito
    │       └── Crédito em loja ──► Gerar crédito para o cliente
    │
    └── 5. Confirmar e processar
```

---

## 4. Hierarquia de Páginas Completa

| # | Página | Rota Angular | Módulo | Acesso Mínimo |
|---|--------|-------------|--------|---------------|
| 1 | Login | `/login` | Auth | Público |
| 2 | Dashboard | `/dashboard` | Core | Cashier |
| 3 | Lista de Itens | `/inventory/items` | M1 | Cashier (leitura) |
| 4 | Detalhe do Item | `/inventory/items/:id` | M1 | Cashier (leitura) |
| 5 | Cadastro/Edição de Item | `/inventory/items/:id/edit` | M1 | Manager |
| 6 | Marcas | `/inventory/brands` | M1 | Manager |
| 7 | Tags/Categorias | `/inventory/tags` | M1 | Manager |
| 8 | Alertas de Estoque | `/inventory/alerts` | M1 | Manager |
| 9 | Lista de Consignações | `/consignments` | M2 | Manager |
| 10 | Nova Consignação | `/consignments/new` | M2 | Manager |
| 11 | Detalhe da Consignação | `/consignments/:id` | M2 | Manager |
| 12 | Lista de Fornecedores | `/suppliers` | M2 | Manager |
| 13 | Detalhe do Fornecedor | `/suppliers/:id` | M2 | Manager |
| 14 | Cadastro/Edição Fornecedor | `/suppliers/:id/edit` | M2 | Manager |
| 15 | Contratos | `/consignments/contracts` | M2 | Manager |
| 16 | POS - Caixa | `/pos` | M3 | Cashier |
| 17 | POS - Abrir Caixa | `/pos/open` | M3 | Cashier |
| 18 | POS - Fechar Caixa | `/pos/close` | M3 | Cashier |
| 19 | POS - Vendas do Dia | `/pos/sales` | M3 | Cashier |
| 20 | POS - Detalhe da Venda | `/pos/sales/:id` | M3 | Cashier |
| 21 | POS - Devoluções | `/pos/returns` | M3 | Manager |
| 22 | POS - Nova Devolução | `/pos/returns/new` | M3 | Manager |
| 23 | Acertos com Fornecedores | `/finance/settlements` | M4 | Finance |
| 24 | Novo Acerto | `/finance/settlements/new` | M4 | Finance |
| 25 | Detalhe do Acerto | `/finance/settlements/:id` | M4 | Finance |
| 26 | Créditos em Loja | `/finance/credits` | M4 | Finance |
| 27 | Fluxo de Caixa | `/finance/cashflow` | M4 | Finance |
| 28 | Despesas | `/finance/expenses` | M4 | Finance |
| 29 | Relatório de Vendas | `/reports/sales` | M5 | Manager |
| 30 | Relatório de Inventário | `/reports/inventory` | M5 | Manager |
| 31 | Relatório de Fornecedores | `/reports/suppliers` | M5 | Manager |
| 32 | Relatório Financeiro | `/reports/finance` | M5 | Finance |
| 33 | Lista de Clientes | `/customers` | M6 | Cashier |
| 34 | Detalhe do Cliente | `/customers/:id` | M6 | Cashier |
| 35 | Programa de Fidelidade | `/customers/loyalty` | M6 | Manager |
| 36 | Campanhas/Promoções | `/promotions` | Extra | Manager |
| 37 | Nova Campanha | `/promotions/new` | Extra | Manager |
| 38 | Usuários | `/admin/users` | M8 | Admin |
| 39 | Configurações do Sistema | `/admin/settings` | M8 | Admin |
| 40 | Audit Log | `/admin/audit-log` | M8 | Admin |
| 41 | Dados da Loja | `/admin/store` | M8 | Admin |
| 42 | Portal do Fornecedor - Dashboard | `/portal/dashboard` | Portal | Supplier |
| 43 | Portal do Fornecedor - Itens | `/portal/items` | Portal | Supplier |
| 44 | Portal do Fornecedor - Extrato | `/portal/statements` | Portal | Supplier |

---

## 5. Navegação por Perfil de Usuário

### Caixa (Cashier)
```
Dashboard ─── POS (Caixa) ─── Vendas do Dia
              Inventário (somente leitura)
              Clientes (cadastro básico)
```

### Gerente (Manager)
```
Dashboard ─── Inventário (completo)
              Consignações (completo)
              POS (completo + devoluções)
              Relatórios (vendas, inventário, fornecedores)
              Promoções
              Clientes
```

### Financeiro (Finance)
```
Dashboard ─── Acertos com Fornecedores
              Créditos em Loja
              Fluxo de Caixa
              Despesas
              Relatórios Financeiros
```

### Admin
```
Acesso total a todas as páginas + Administração
```

### Fornecedor (Supplier) - Portal Externo
```
Portal Dashboard ─── Meus Itens ─── Meu Extrato
```

---

## 6. Componentes de Navegação Globais

| Componente | Localização | Descrição |
|-----------|-------------|-----------|
| **Sidebar** | Esquerda | Menu principal colapsável com ícones e texto |
| **Header** | Topo | Logo, busca global, notificações (badge), avatar do usuário |
| **Breadcrumb** | Abaixo do header | Caminho de navegação: Dashboard > Consignações > #CON-001 |
| **Busca Global** | Header | Pesquisa unificada: itens, fornecedores, vendas, clientes |
| **Notificações** | Header (sino) | Dropdown com alertas: consignações expirando, estoque parado, caixas abertos |
| **Quick Actions** | Dashboard + FAB | Botão flutuante com ações rápidas: Nova Venda, Nova Consignação, Buscar Item |
