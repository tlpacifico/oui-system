# SHS - Descrição Detalhada das Páginas

## Versão: 1.0 | Última Atualização: 2026-02-11

---

## 1. Autenticação

### PG-01: Login (`/login`)

**Objetivo:** Autenticar o usuário no sistema.

**Layout:**
- Tela centralizada com logo do sistema
- Campo de email
- Campo de senha
- Botão "Entrar"
- Link "Esqueci minha senha"

**Comportamento:**
- Autenticação via Firebase Auth
- Após login, redireciona para Dashboard
- Se já autenticado, redireciona automaticamente
- Após 5 tentativas falhas, bloqueia por 15 minutos

**Componentes:** Form com validação, Toast de erro

---

## 2. Core

### PG-02: Dashboard (`/dashboard`)

**Objetivo:** Visão geral do estado da loja em tempo real.

**Layout - Seções:**

| Seção | Conteúdo |
|-------|----------|
| **KPI Cards (topo)** | 4 cards: Vendas Hoje (R$ e qtd), Receita do Mês, Itens em Estoque, Acertos Pendentes |
| **Gráfico de Vendas** | Gráfico de linha: vendas dos últimos 7/30 dias com comparativo do período anterior |
| **Alertas Pendentes** | Lista com: consignações expirando (próx. 7 dias), itens parados 60+ dias, caixas abertos |
| **Top 5 Vendas** | Tabela: itens mais vendidos da semana com marca, categoria, preço |
| **Ações Rápidas** | Botões: Nova Venda, Nova Consignação, Buscar Item |

**Filtros:** Período (hoje, semana, mês, customizado)

**Atualização:** Dados atualizados a cada 5 minutos (ou manual refresh)

---

## 3. Inventário (M1)

### PG-03: Lista de Itens (`/inventory/items`)

**Objetivo:** Buscar, filtrar e gerenciar todos os itens do estoque.

**Layout:**
- **Barra de filtros (topo):**
  - Busca por texto (nome/ID)
  - Dropdown: Marca
  - Dropdown: Categoria/Tags
  - Dropdown: Tamanho
  - Dropdown: Cor
  - Dropdown: Condição
  - Range: Preço (min/max)
  - Dropdown: Status (Avaliado, À Venda, Vendido, Devolvido)
  - Dropdown: Fornecedor
  - DateRange: Data de consignação
- **Tabela de resultados:**
  - Colunas: Foto (thumb), ID, Nome, Marca, Tamanho, Preço, Status, Fornecedor, Dias em Estoque
  - Ordenação por qualquer coluna
  - Paginação (20 itens por página)
  - Indicador visual de alerta para itens parados (amarelo 30d, laranja 45d, vermelho 60d)
- **Ações por item:** Ver detalhes, Editar, Excluir (soft)
- **Ações em lote:** Selecionar múltiplos → Imprimir etiquetas, Aplicar desconto em lote
- **Botão:** "+ Novo Item" (redireciona para tela de consignação ou item avulso)

**Exportação:** Botão para exportar lista filtrada em CSV/Excel

---

### PG-04: Detalhe do Item (`/inventory/items/:id`)

**Objetivo:** Visualizar todas as informações de um item específico.

**Layout - Seções:**

| Seção | Conteúdo |
|-------|----------|
| **Header** | Foto principal, ID, Nome, Status (badge colorido) |
| **Informações Gerais** | Marca, Tamanho, Cor, Composição, Condição, Tags |
| **Precificação** | Preço atual, Preço original, Histórico de alterações (timeline) |
| **Consignação** | Fornecedor (link), Data de entrada, Período, Dias restantes, Status da consignação |
| **Galeria** | Até 5 fotos do item com zoom |
| **Histórico** | Log de ações: criação, edição de preço, venda, devolução (com quem e quando) |

**Ações:** Editar, Excluir, Imprimir Etiqueta, Alterar Preço

---

### PG-05: Cadastro/Edição de Item (`/inventory/items/:id/edit`)

**Objetivo:** Criar ou editar um item do inventário.

**Campos do Formulário:**
- Nome/Descrição (texto, obrigatório)
- Marca (autocomplete com cadastro, obrigatório)
- Categoria (dropdown hierárquico)
- Tamanho (dropdown: PP, P, M, G, GG, XG ou numérico)
- Cor (dropdown com preview colorido)
- Composição/Tecido (texto)
- Condição (dropdown: NWT, NWOT, Excelente, Bom, Regular)
- Preço Avaliado (R$, obrigatório)
- Tags (chips com autocomplete)
- Fotos (upload com drag & drop, até 5)
- Notas (textarea opcional)

**Validações:**
- Nome: mínimo 3 caracteres
- Preço: maior que R$0,00
- Marca: obrigatória

---

### PG-06: Marcas (`/inventory/brands`)

**Objetivo:** Gerenciar catálogo de marcas.

**Layout:**
- Tabela com: Nome da Marca, Qtd de Itens, Ações (Editar, Excluir)
- Busca por nome
- Botão "+ Nova Marca"
- Modal para cadastro/edição (campo: Nome)

---

### PG-07: Tags/Categorias (`/inventory/tags`)

**Objetivo:** Gerenciar tags e categorias de itens.

**Layout:**
- Árvore de categorias (expansível): Roupas > Feminino > Vestidos
- Lista de tags flat (chips)
- Botão "+ Nova Categoria" e "+ Nova Tag"
- Modal para cadastro/edição

---

### PG-08: Alertas de Estoque (`/inventory/alerts`)

**Objetivo:** Visualizar itens que requerem ação (parados, expirando).

**Layout:**
- **Tabs:** Todos | Amarelo (30d) | Laranja (45d) | Vermelho (60d)
- **Tabela:** Item, Marca, Preço, Dias em Estoque, Fornecedor, Ação Sugerida
- **Ações rápidas:** Reduzir preço, Devolver ao fornecedor, Renovar consignação

---

## 4. Consignações (M2)

### PG-09: Lista de Consignações (`/consignments`)

**Objetivo:** Listar e buscar consignações.

**Layout:**
- **Filtros:** Fornecedor, Status, Período
- **Tabela:** ID, Fornecedor, Data, Qtd Itens, Valor Total, Status, Ações
- **Status badges:** Ativa (verde), Expirando (amarelo), Expirada (vermelho), Encerrada (cinza)
- **Ações:** Ver detalhes, Editar

---

### PG-10: Nova Consignação (`/consignments/new`)

**Objetivo:** Registrar entrada de itens consignados.

**Layout - Wizard (3 etapas):**

**Etapa 1 - Fornecedor:**
- Selecionar fornecedor existente (autocomplete)
- Ou cadastrar novo (formulário inline colapsável)
- Mostra: comissão em dinheiro %, comissão em crédito %

**Etapa 2 - Itens:**
- Lista de itens sendo adicionados
- Botão "+ Adicionar Item" abre formulário inline
- Para cada item: nome, marca, tamanho, cor, condição, preço, foto
- Sistema gera ID automático ao salvar cada item
- Total de itens e valor total visível

**Etapa 3 - Revisão e Confirmação:**
- Resumo: fornecedor, qtd itens, valor total, período de consignação
- Checkbox: "Gerar contrato PDF"
- Botão "Finalizar Consignação"

---

### PG-11: Detalhe da Consignação (`/consignments/:id`)

**Objetivo:** Visualizar detalhes de uma consignação específica.

**Layout:**
- **Header:** ID, Fornecedor (link), Data, Status
- **Resumo:** Qtd itens, Valor total, Dias restantes, Comissão acordada
- **Tabela de itens:** ID, Nome, Marca, Preço, Status (badge), Dias em loja
- **Ações:** Adicionar item, Devolver itens selecionados, Renovar, Imprimir contrato

---

### PG-12: Lista de Fornecedores (`/suppliers`)

**Objetivo:** Gerenciar fornecedores/consignantes.

**Layout:**
- **Busca:** Nome, Email, Telefone
- **Tabela:** Nome, Inicial, Email, Telefone, Comissão Cash%, Comissão Crédito%, Itens Ativos, Ações
- **Ações:** Ver detalhes, Editar, Nova consignação
- **Botão:** "+ Novo Fornecedor"

---

### PG-13: Detalhe do Fornecedor (`/suppliers/:id`)

**Objetivo:** Visualizar perfil completo do fornecedor.

**Layout - Tabs:**

| Tab | Conteúdo |
|-----|----------|
| **Dados** | Nome, Email, Telefone, Inicial, Comissões, Data de cadastro |
| **Consignações** | Lista de consignações do fornecedor com status |
| **Itens** | Todos os itens deste fornecedor com filtros |
| **Acertos** | Histórico de pagamentos/acertos realizados |
| **Estatísticas** | Itens consignados, vendidos, devolvidos, tempo médio de venda, receita gerada |

---

### PG-14: Cadastro/Edição de Fornecedor (`/suppliers/:id/edit`)

**Campos:**
- Nome Completo (obrigatório)
- Email (obrigatório, validação formato)
- Telefone (obrigatório, máscara brasileira)
- CPF (opcional, validação)
- Inicial para ID de item (1 caractere, obrigatório)
- Comissão em Dinheiro % (obrigatório, 0-100)
- Comissão em Crédito/Produto % (obrigatório, 0-100)
- Endereço (opcional)
- Observações (textarea)

---

## 5. Ponto de Venda - POS (M3)

### PG-16: POS - Caixa (`/pos`)

**Objetivo:** Tela principal de vendas. Layout otimizado para uso com scanner e teclado.

**Layout (tela cheia, sem sidebar):**

```
┌─────────────────────────────────────────────────────────────────┐
│  [Logo]  Caixa #1 - João Silva    Aberto: 08:00    [X Fechar]  │
├───────────────────────────────────┬─────────────────────────────┤
│                                   │                             │
│  🔍 Buscar item (ID ou nome)      │   CARRINHO                  │
│  ┌─────────────────────────────┐  │                             │
│  │ Resultados da busca          │  │   Item 1 - R$ 89,90    [X] │
│  │ ┌─────────────────────────┐ │  │   Item 2 - R$ 45,00    [X] │
│  │ │ Vestido Floral - R$89,90│ │  │   Item 3 - R$ 120,00   [X] │
│  │ │ Maria Silva | M | Bom   │ │  │                             │
│  │ │        [+ Adicionar]    │ │  │   Desconto: -R$ 12,74 (5%) │
│  │ └─────────────────────────┘ │  │                             │
│  │ ┌─────────────────────────┐ │  │   ─────────────────────     │
│  │ │ Blusa Seda - R$45,00   │ │  │   TOTAL: R$ 242,16         │
│  │ │ Ana Lima | P | Excelente│ │  │                             │
│  │ │        [+ Adicionar]    │ │  │   [ 💳 Finalizar Venda ]   │
│  │ └─────────────────────────┘ │  │                             │
│  └─────────────────────────────┘  │                             │
│                                   │                             │
│  [Cliente: Não identificado] [+]  │   Atalhos: F2=Nova F8=Busca │
│                                   │   F4=Pagar ESC=Cancelar     │
└───────────────────────────────────┴─────────────────────────────┘
```

**Funcionalidades:**
- Campo de busca com foco automático (para scanner)
- Busca por ID (barcode) ou texto (nome/marca)
- Carrinho com itens adicionados
- Aplicar desconto (% ou valor fixo)
- Identificar cliente fidelidade
- Finalizar venda → Modal de pagamento

**Modal de Pagamento:**
- Método: Dinheiro, Cartão Crédito, Cartão Débito, PIX, Crédito em Loja
- Opção de pagamento dividido (2 métodos)
- Para dinheiro: campo "Valor recebido" com cálculo de troco
- Botão "Confirmar Pagamento"
- Após confirmar: opção de imprimir recibo

---

### PG-17: POS - Abrir Caixa (`/pos/open`)

**Objetivo:** Iniciar sessão do caixa.

**Layout:**
- Informação do caixa (número, funcionário logado)
- Campo: Valor de abertura (troco inicial em R$)
- Botão "Abrir Caixa"
- Após abrir, redireciona para POS - Caixa

---

### PG-18: POS - Fechar Caixa (`/pos/close`)

**Objetivo:** Encerrar sessão do caixa com reconciliação.

**Layout:**
- **Resumo do dia:**
  - Qtd de vendas
  - Total em Dinheiro, Cartão Crédito, Cartão Débito, PIX, Crédito em Loja
  - Valor esperado em dinheiro (abertura + vendas cash - devoluções cash)
- **Campo:** Valor contado em caixa (R$)
- **Discrepância:** Sistema calcula diferença automaticamente
  - Verde: ≤ R$5 (OK)
  - Amarelo: R$5-50 (campo de justificativa obrigatório)
  - Vermelho: > R$50 (justificativa + aprovação do gerente)
- **Botão:** "Fechar Caixa"
- **Após fechar:** Gera relatório de fechamento (PDF)

---

### PG-19: Vendas do Dia (`/pos/sales`)

**Objetivo:** Listar vendas realizadas no caixa do dia.

**Layout:**
- **Filtros:** Data, Caixa, Funcionário
- **Tabela:** Nº Venda, Hora, Qtd Itens, Total, Forma Pgto, Cliente, Ações
- **Ações:** Ver detalhes, Imprimir recibo
- **Totalizadores no topo:** Total vendido, Qtd vendas, Ticket médio

---

### PG-20: Detalhe da Venda (`/pos/sales/:id`)

**Objetivo:** Visualizar todos os detalhes de uma venda.

**Layout:**
- **Header:** Nº Venda, Data/Hora, Funcionário, Status
- **Itens vendidos:** Tabela com ID, Nome, Marca, Preço, Fornecedor
- **Pagamento:** Forma, Valor, Troco (se dinheiro)
- **Cliente:** Nome, Pontos creditados (se fidelidade)
- **Ações:** Imprimir recibo, Processar devolução

---

### PG-21: Devoluções (`/pos/returns`)

**Objetivo:** Listar e processar devoluções.

**Layout:**
- **Tabela:** Nº Devolução, Venda Original, Data, Motivo, Tipo (Troca/Crédito), Valor, Status
- **Botão:** "+ Nova Devolução"

---

### PG-22: Nova Devolução (`/pos/returns/new`)

**Objetivo:** Processar devolução/troca de item.

**Fluxo:**
1. Buscar venda original (campo: nº da venda ou data)
2. Selecionar itens para devolver (checkbox)
3. Informar motivo
4. Escolher resolução: Troca (vai para POS com crédito) ou Crédito em Loja
5. Confirmar

---

## 6. Financeiro (M4)

### PG-23: Acertos com Fornecedores (`/finance/settlements`)

**Objetivo:** Gerenciar pagamentos de comissão a fornecedores.

**Layout:**
- **Tabs:** Pendentes | Processados | Todos
- **Tabela Pendentes:** Fornecedor, Período, Itens Vendidos, Valor Total Vendas, Comissão Loja, Valor a Pagar, Ação
- **Ação:** "Processar Acerto" → redireciona para PG-24

---

### PG-24: Novo Acerto (`/finance/settlements/new`)

**Objetivo:** Calcular e processar acerto com fornecedor.

**Layout:**
- **Seleção:** Fornecedor + Período (de/até)
- **Tabela de itens vendidos:** ID Item, Nome, Data Venda, Preço Venda, Taxa Comissão, Comissão Loja, Valor Fornecedor
- **Totais:** Total vendido, Total comissão, Total a pagar
- **Forma de pagamento:** Dinheiro ou Crédito em Loja
  - Se dinheiro: usa taxa de comissão em cash
  - Se crédito: usa taxa de comissão em produtos (menor comissão, mais vantajoso para fornecedor)
- **Recálculo dinâmico** ao trocar forma de pagamento
- **Botão:** "Confirmar Acerto"
- **Gera:** Recibo de acerto (PDF)

---

### PG-25: Detalhe do Acerto (`/finance/settlements/:id`)

**Layout:**
- Header: ID, Fornecedor, Período, Status (Pago/Pendente)
- Tabela de itens do acerto
- Totais e forma de pagamento
- Botão: Imprimir recibo, Reprocessar (se erro)

---

### PG-26: Créditos em Loja (`/finance/credits`)

**Objetivo:** Gerenciar créditos de loja (clientes e fornecedores).

**Layout:**
- **Tabela:** Titular, Tipo (Cliente/Fornecedor), Saldo, Data Emissão, Vencimento, Status
- **Filtros:** Tipo, Status (Ativo/Vencido), Busca por nome
- **Ações:** Ver extrato, Adicionar crédito manual

---

### PG-27: Fluxo de Caixa (`/finance/cashflow`)

**Objetivo:** Visão de entradas e saídas financeiras.

**Layout:**
- **Gráfico:** Entradas vs Saídas (barras empilhadas por semana/mês)
- **Tabela de movimentações:** Data, Tipo (Entrada/Saída), Descrição, Valor, Saldo Acumulado
- **Filtros:** Período, Tipo
- **Resumo:** Total Entradas, Total Saídas, Saldo Líquido

---

### PG-28: Despesas (`/finance/expenses`)

**Objetivo:** Registrar e controlar despesas operacionais.

**Layout:**
- **Tabela:** Data, Categoria, Descrição, Valor, Recorrente (Sim/Não), Ações
- **Filtros:** Categoria, Período
- **Botão:** "+ Nova Despesa"
- **Resumo mensal:** Total por categoria (gráfico de pizza)

---

## 7. Relatórios (M5)

### PG-29: Relatório de Vendas (`/reports/sales`)

**Objetivo:** Análise detalhada de vendas.

**Métricas:**
- Receita total e por período
- Ticket médio
- Qtd de vendas
- Top 10 itens/marcas/categorias vendidos
- Breakdown por forma de pagamento (gráfico de pizza)
- Vendas por dia da semana (heatmap)
- Comparativo com período anterior (%)

**Filtros:** Período, Categoria, Marca, Funcionário, Forma de pagamento

**Exportação:** PDF, Excel

---

### PG-30: Relatório de Inventário (`/reports/inventory`)

**Objetivo:** Análise da saúde do estoque.

**Métricas:**
- Total de itens em estoque e valor total
- Distribuição por status (gráfico de pizza)
- Aging: distribuição por tempo em estoque (0-15d, 15-30d, 30-45d, 45-60d, 60d+)
- Taxa de giro (sell-through rate) por categoria/marca
- Itens com preço reduzido
- Previsão de devoluções (consignações expirando)

---

### PG-31: Relatório de Fornecedores (`/reports/suppliers`)

**Objetivo:** Performance dos fornecedores.

**Métricas:**
- Ranking por: volume vendido, receita gerada, tempo médio de venda
- Taxa de venda (% de itens vendidos vs consignados) por fornecedor
- Taxa de devolução
- Valor pendente de acerto
- Ticket médio por fornecedor

---

### PG-32: Relatório Financeiro (`/reports/finance`)

**Objetivo:** Visão financeira consolidada.

**Métricas:**
- Receita bruta (total de vendas)
- Receita de comissões (o que a loja efetivamente ganha)
- Despesas operacionais
- Margem líquida
- Acertos pagos vs pendentes
- Projeção de fluxo de caixa (próximos 30/60/90 dias)

---

## 8. Clientes (M6)

### PG-33: Lista de Clientes (`/customers`)

**Layout:**
- **Busca:** Nome, CPF, Email, Telefone
- **Tabela:** Nome, Email, Telefone, Pontos, Total Gasto, Última Compra, Ações
- **Botão:** "+ Novo Cliente"

---

### PG-34: Detalhe do Cliente (`/customers/:id`)

**Layout - Tabs:**
- **Dados:** Nome, CPF, Email, Telefone, Data Nascimento, Endereço
- **Fidelidade:** Pontos acumulados, Pontos resgatados, Saldo, Nível (Regular/VIP)
- **Compras:** Histórico de vendas vinculadas
- **Créditos:** Créditos em loja ativos

---

### PG-35: Programa de Fidelidade (`/customers/loyalty`)

**Layout:**
- Dashboard de fidelidade: total de clientes cadastrados, pontos emitidos/resgatados
- Configuração: taxa de conversão (R$/ponto), mínimo de resgate, validade
- Lista de resgates recentes
- Aniversariantes do mês (para promoção)

---

## 9. Promoções

### PG-36: Campanhas (`/promotions`)

**Layout:**
- **Tabs:** Ativas | Futuras | Encerradas
- **Cards de campanha:** Nome, Período, Tipo, Desconto, Qtd itens afetados
- **Botão:** "+ Nova Campanha"

---

### PG-37: Nova Campanha (`/promotions/new`)

**Campos:**
- Nome da campanha
- Período (data início/fim)
- Tipo: Desconto por categoria, Desconto por tempo em estoque, Progressivo, Compre X Leve Y
- Regras (dinâmico conforme tipo)
- Categorias/Marcas afetadas
- Desconto (% ou valor fixo)
- Preview: lista de itens que serão afetados

---

## 10. Administração (M8)

### PG-38: Usuários (`/admin/users`)

**Layout:**
- **Tabela:** Nome, Email, Perfil (Caixa/Gerente/Financeiro/Admin), Status (Ativo/Inativo), Último Acesso
- **Ações:** Editar perfil/permissões, Desativar
- **Botão:** "+ Novo Usuário"
- **Modal de edição:** Nome, Email, Perfil (dropdown), Permissões customizadas (checkboxes)

---

### PG-39: Configurações do Sistema (`/admin/settings`)

**Layout - Seções:**
- **Consignação:** Período padrão (dias), Comissão padrão cash %, Comissão padrão crédito %
- **POS:** Desconto máximo sem autorização %, Métodos de pagamento habilitados
- **Fidelidade:** Pontos por R$, Conversão de resgate, Validade dos pontos
- **Alertas:** Thresholds de estoque parado (30/45/60 dias configuráveis)
- **Fiscal:** CNPJ, Inscrição Estadual, Regime tributário
- **Notificações:** Habilitar SMS, Email, WhatsApp

---

### PG-40: Audit Log (`/admin/audit-log`)

**Layout:**
- **Filtros:** Usuário, Tipo de ação, Módulo, Período
- **Tabela:** Data/Hora, Usuário, Ação, Módulo, Detalhe (expandível), IP
- **Tipos de ação:** Criação, Edição, Exclusão, Login, Alteração de preço, Desconto, Estorno

---

### PG-41: Dados da Loja (`/admin/store`)

**Campos:**
- Nome da loja, CNPJ, Inscrição Estadual
- Endereço completo
- Telefone, Email
- Logo (upload)
- Informações para recibo/nota fiscal
- Horário de funcionamento

---

## 11. Portal do Fornecedor (Externo)

### PG-42: Portal - Dashboard (`/portal/dashboard`)

**Objetivo:** Visão geral para o fornecedor acompanhar seus itens.

**Layout:**
- **KPI Cards:** Itens na Loja, Itens Vendidos (mês), Valor a Receber, Total Recebido
- **Últimas vendas:** Lista dos últimos 5 itens vendidos com data e preço
- **Consignações ativas:** Lista com qtd de itens e valor total

---

### PG-43: Portal - Meus Itens (`/portal/items`)

**Layout:**
- **Filtros:** Status (Em Loja, Vendido, Devolvido), Período
- **Tabela:** ID, Nome, Marca, Preço, Status, Data Entrada, Dias em Loja
- Somente visualização (sem edição)

---

### PG-44: Portal - Extrato (`/portal/statements`)

**Layout:**
- **Filtros:** Período
- **Tabela de acertos:** Data, Período, Qtd Itens, Valor Bruto, Comissão, Valor Líquido, Forma Pgto
- **Totais:** Acumulado do período filtrado
- **Download:** PDF do recibo de cada acerto
