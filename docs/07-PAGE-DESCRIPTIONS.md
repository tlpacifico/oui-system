# Oui Circular - Descrição Detalhada das Páginas

## Versão: 2.0 | Última Atualização: 2026-02-11

---

## 1. Autenticação

### PG-01: Login (`/login`)

**Objetivo:** Autenticar o utilizador no sistema.

**Layout:**
- Ecrã centralizado com logótipo do sistema (Oui Circular)
- Campo de email
- Campo de palavra-passe
- Botão "Entrar"
- Link "Esqueci a minha palavra-passe"

**Comportamento:**
- Autenticação via Firebase Auth
- Após login, redireciona para Dashboard
- Se já autenticado, redireciona automaticamente
- Após 5 tentativas falhadas, bloqueia por 15 minutos

**Componentes:** Formulário com validação, Toast de erro

---

## 2. Core

### PG-02: Dashboard (`/dashboard`)

**Objetivo:** Visão geral do estado da loja em tempo real.

**Layout - Secções:**

| Secção | Conteúdo |
|--------|----------|
| **KPI Cards (topo)** | 4 cards: Vendas Hoje (€ e qtd), Receita do Mês, Itens em Stock, Acertos Pendentes |
| **Receções Pendentes de Avaliação** | Badge com número de receções por avaliar, link direto para PG-NEW-2 |
| **Gráfico de Vendas** | Gráfico de linha: vendas dos últimos 7/30 dias com comparativo do período anterior |
| **Alertas Pendentes** | Lista com: consignações a expirar (próx. 7 dias), itens parados 60+ dias, caixas abertos |
| **Top 5 Vendas** | Tabela: itens mais vendidos da semana com marca, categoria, preço |
| **Ações Rápidas** | Botões: Nova Venda, Receção de Peças, Buscar Item |

**Filtros:** Período (hoje, semana, mês, personalizado)

**Atualização:** Dados atualizados a cada 5 minutos (ou atualização manual)

---

## 3. Inventário (M1)

### PG-03: Lista de Itens (`/inventory/items`)

**Objetivo:** Pesquisar, filtrar e gerir todos os itens do stock.

**Layout:**
- **Barra de filtros (topo):**
  - Pesquisa por texto (nome/ID)
  - Dropdown: Marca
  - Dropdown: Categoria/Tags
  - Dropdown: Tamanho
  - Dropdown: Cor
  - Dropdown: Condição
  - Range: Preço (mín/máx)
  - Dropdown: Estado (Avaliado, À Venda, Vendido, Devolvido)
  - Dropdown: Fornecedor
  - DateRange: Data de consignação
- **Tabela de resultados:**
  - Colunas: Foto (thumb), ID, Nome, Marca, Tamanho, Preço, Estado, Fornecedor, Dias em Stock
  - Ordenação por qualquer coluna
  - Paginação (20 itens por página)
  - Indicador visual de alerta para itens parados (amarelo 30d, laranja 45d, vermelho 60d)
- **Ações por item:** Ver detalhes, Editar, Eliminar (soft-delete, ver CU-05)
- **Eliminar item (CU-05):** Ao clicar "Eliminar", sistema apresenta modal de confirmação. Após confirmação, executa soft-delete (flag IsDeleted). Item deixa de aparecer no inventário ativo. Apenas permitido se peça ainda não vendida.
- **Ações em lote:** Selecionar múltiplos → Imprimir etiquetas, Aplicar desconto em lote
- **Botão:** "+ Novo Item" (redireciona para ecrã de consignação ou item avulso)

**Exportação:** Botão para exportar lista filtrada em CSV/Excel

---

### PG-04: Detalhe do Item (`/inventory/items/:id`)

**Objetivo:** Visualizar todas as informações de um item específico.

**Layout - Secções:**

| Secção | Conteúdo |
|--------|----------|
| **Header** | Foto principal, ID, Nome, Estado (badge colorido) |
| **Informações Gerais** | Marca, Tamanho, Cor, Composição, Condição, Tags |
| **Precificação** | Preço atual, Preço original, Histórico de alterações (timeline) |
| **Consignação** | Fornecedor (link), Data de entrada, Período, Dias restantes, Estado da consignação |
| **Galeria** | Até 5 fotos do item com zoom |
| **Histórico** | Log de ações: criação, edição de preço, venda, devolução (com quem e quando) |

**Ações:** Editar, Eliminar (soft-delete com modal de confirmação, ver CU-05), Imprimir Etiqueta, Alterar Preço

---

### PG-05: Cadastro/Edição de Item (`/inventory/items/:id/edit`)

**Objetivo:** Criar ou editar um item do inventário.

**Campos do Formulário:**
- Tipo de Aquisição (radio: Consignação / Compra Própria, obrigatório) — determina quais campos adicionais são exibidos
- Nome/Descrição (texto, obrigatório)
- Marca (autocomplete com cadastro, obrigatório)
- Categoria (dropdown hierárquico)
- Tamanho (dropdown: XXS, XS, S, M, L, XL, XXL ou numérico)
- Cor (dropdown com preview colorido)
- Composição/Tecido (texto)
- Condição (dropdown: NWT, NWOT, Excelente, Bom, Regular)
- Preço Avaliado / Preço de Venda (€, obrigatório)
- Tags (chips com autocomplete)
- Fotos (upload com drag & drop, até 5)
- Notas (textarea opcional)

**Campos adicionais — Compra Própria (visíveis apenas quando Tipo = "Compra Própria", ver CU-06):**
- Origem da Compra (dropdown: Humana, Vinted, H&M, Acervo Pessoal, Outro — obrigatório)
- Preço de Custo (€, obrigatório — quanto a loja pagou pela peça)

**Campos adicionais — Consignação (visíveis apenas quando Tipo = "Consignação"):**
- Fornecedor (autocomplete, obrigatório — link para receção/consignação de origem)

**Comportamento por tipo:**
- **Consignação:** Item fica associado a uma receção/consignação. Comissão será calculada na venda. Estado inicial depende do fluxo de avaliação.
- **Compra Própria:** Item entra diretamente com estado "À Venda (DL)". Sem comissão — lucro total da loja. Margem = Preço de Venda - Preço de Custo.

**Validações:**
- Nome: mínimo 3 caracteres
- Preço de Venda: maior que €0,00
- Marca: obrigatória
- Tipo de Aquisição: obrigatório
- Se Compra Própria: Origem e Preço de Custo obrigatórios

---

### PG-06: Marcas (`/inventory/brands`)

**Objetivo:** Gerir catálogo de marcas.

**Layout:**
- Tabela com: Nome da Marca, Qtd de Itens, Ações (Editar, Eliminar)
- Pesquisa por nome
- Botão "+ Nova Marca"
- Modal para cadastro/edição (campo: Nome)

---

### PG-07: Tags/Categorias (`/inventory/tags`)

**Objetivo:** Gerir tags e categorias de itens.

**Layout:**
- Árvore de categorias (expansível): Roupa > Feminino > Vestidos
- Lista de tags flat (chips)
- Botão "+ Nova Categoria" e "+ Nova Tag"
- Modal para cadastro/edição

---

### PG-08: Alertas de Stock (`/inventory/alerts`)

**Objetivo:** Visualizar itens que requerem ação (parados, a expirar).

**Layout:**
- **Tabs:** Todos | Amarelo (30d) | Laranja (45d) | Vermelho (60d)
- **Tabela:** Item, Marca, Preço, Dias em Stock, Fornecedor, Ação Sugerida
- **Ações rápidas:** Reduzir preço, Devolver ao fornecedor, Renovar consignação

---

## 4. Consignações (M2)

### PG-09: Lista de Consignações (`/consignments`)

**Objetivo:** Listar e pesquisar consignações (receções).

**Layout:**
- **Filtros:** Fornecedor, Estado, Período
- **Tabela:** ID, Fornecedor, Data Receção, Qtd Itens, Estado Avaliação, Estado Consignação, Ações
- **Estado badges consignação:** Ativa (verde), A Expirar (amarelo), Expirada (vermelho), Encerrada (cinza)
- **Estado badges avaliação:** Pendente de Avaliação (laranja), Avaliação Concluída (verde), Avaliação Parcial (amarelo)
- **Ações:** Ver detalhes, Avaliar (se pendente)

---

### PG-10: Receção de Peças (`/consignments/receive`)

**Objetivo:** Registar a entrada de peças trazidas por um cliente/fornecedor. Esta etapa é apenas de contagem - NÃO inclui detalhes individuais dos itens nem preços.

**Layout - Formulário simples:**

**Secção 1 - Fornecedor:**
- Selecionar fornecedor existente (autocomplete por nome, email ou NIF)
- Ou cadastrar novo fornecedor (formulário inline colapsável):
  - Nome Completo (obrigatório)
  - Email (obrigatório)
  - Telefone (+351, obrigatório)
  - NIF (opcional, validação portuguesa)

**Secção 2 - Dados da Receção:**
- Data da receção (preenchida automaticamente com data atual, editável)
- Quantidade de peças entregues (campo numérico, obrigatório)
- Observações (textarea opcional - ex: "trouxe 3 sacos de roupa")

**Secção 3 - Termos de Comissão (exibido automaticamente, não editável):**
- Informação exibida:
  - "O cliente recebe 40% do valor de venda em dinheiro"
  - "O cliente recebe 50% do valor de venda em crédito em loja"
  - "(A loja retém 60% ou 50%, respetivamente)"
- Período de consignação: 90 dias (configurável nas definições do sistema)

**Secção 4 - Confirmação:**
- Resumo: Fornecedor, Data, Quantidade de peças, Termos de comissão
- Checkbox: "Gerar recibo de receção em PDF"
- Botão "Registar Receção"

**Após registo:**
- Estado da receção: "Pendente de Avaliação"
- Se checkbox ativado: gera PDF do recibo de receção contendo:
  - Logótipo e dados da loja
  - Nome do cliente/fornecedor
  - Data da receção
  - Quantidade de peças entregues
  - Termos de comissão (40% dinheiro / 50% crédito em loja)
  - Período de consignação
  - Espaço para assinatura do cliente
  - Espaço para assinatura da loja
- Opção de imprimir o recibo imediatamente
- Redireciona para lista de consignações (PG-09)

---

### PG-NEW-1: Avaliações Pendentes (`/consignments/pending-evaluations`)

**Objetivo:** Listar todas as receções que ainda necessitam de avaliação (catalogação individual dos itens).

**Layout:**
- **Barra de ações (topo):**
  - Pesquisa por nome do fornecedor
  - Filtro por data de receção (de/até)

- **Tabela de receções pendentes:**

| Coluna | Descrição |
|--------|-----------|
| ID Receção | Identificador único da receção |
| Fornecedor | Nome do cliente/fornecedor |
| Data Receção | Data em que as peças foram entregues |
| Qtd Peças | Número total de peças entregues |
| Avaliadas | Número de peças já avaliadas (ex: "3 de 10") |
| Dias Pendente | Número de dias desde a receção sem avaliação completa |
| Ação | Botão "Avaliar" (redireciona para PG-NEW-2) |

- **Indicadores visuais:**
  - Verde: receção recente (< 3 dias)
  - Amarelo: 3-7 dias sem avaliação completa
  - Vermelho: > 7 dias sem avaliação completa

- **Ordenação:** Por defeito, ordenado por dias pendente (mais antigo primeiro)

---

### PG-NEW-2: Avaliar Receção (`/consignments/:id/evaluate`)

**Objetivo:** Avaliar e catalogar individualmente cada peça de uma receção. É aqui que se registam todos os detalhes, se define o preço e se tiram fotos de cada item.

**Layout:**

**Header:**
- ID da Receção, Nome do Fornecedor, Data da Receção
- Progresso: "Avaliadas: X de Y peças" (barra de progresso visual)

**Secção - Lista de Itens Avaliados:**
- Tabela dos itens já catalogados com: Foto (thumb), Descrição, Marca, Tamanho, Preço, Estado
- Ação por item: Editar, Remover

**Secção - Formulário de Avaliação (para cada item):**
- Descrição (texto, obrigatório)
- Marca (autocomplete com cadastro, obrigatório)
- Categoria (dropdown hierárquico)
- Tamanho (dropdown: XXS, XS, S, M, L, XL, XXL ou numérico)
- Cor (dropdown com preview colorido)
- Composição/Tecido (texto)
- Condição (dropdown: NWT, NWOT, Excelente, Bom, Regular)
- Preço avaliado (€, obrigatório)
- Foto(s) (upload com drag & drop, até 5 fotos por item)
- Notas (textarea opcional)

**Opção "Com Defeito":**
- Toggle/checkbox: "Peça com Defeito"
- Se ativado:
  - Campo obrigatório: Motivo do defeito (dropdown: Mancha, Rasgão, Falta de botão, Desgaste excessivo, Cheiro, Outro)
  - Campo: Descrição adicional do defeito (textarea)
  - A peça é marcada como "Rejeitada" e não entra em stock
  - Aparece na lista com badge vermelho "Com Defeito"

**Botões de ação por item:**
- "Guardar e Adicionar Próximo" - guarda o item e limpa o formulário para o próximo
- "Guardar" - guarda o item e permanece na página

**Quando todas as peças estiverem avaliadas (X = Y):**
- Botão proeminente: "Concluir Avaliação e Enviar por Email"
- Ao clicar, abre modal de pré-visualização do email (ver abaixo)

**Modal de Pré-visualização do Email:**
- Destinatário: email do fornecedor (preenchido automaticamente, editável)
- Assunto: "Oui Circular - Avaliação das suas peças (Receção #[ID])"
- Corpo do email (pré-visualização):
  - Saudação com nome do fornecedor
  - Resumo: "Recebemos X peças no dia [data]"
  - Tabela com todos os itens avaliados: Descrição, Marca, Tamanho, Preço Avaliado
  - Se houver itens com defeito: secção separada listando itens rejeitados com motivo
  - Valor total estimado das peças aceites
  - Comissão: "Receberá 40% em dinheiro (€[valor]) ou 50% em crédito em loja (€[valor]) sobre cada peça vendida"
  - Informação sobre recolha de peças rejeitadas
  - Nota: "As peças ficarão disponíveis na loja por um período de 90 dias"
- Botão "Enviar Email"
- Botão "Cancelar" (volta à página de avaliação)

**Após envio do email:**
- Estado da receção muda para "Avaliação Concluída"
- Itens aceites ficam com estado "À Venda" e aparecem no inventário
- Regista data/hora do envio do email no histórico

---

### PG-11: Detalhe da Consignação (`/consignments/:id`)

**Objetivo:** Visualizar detalhes completos de uma consignação/receção específica.

**Layout:**

**Header:** ID, Fornecedor (link para PG-13), Data Receção, Estado da Avaliação, Estado da Consignação

**Secção - Resumo:**
- Qtd peças entregues
- Qtd peças avaliadas / aceites / rejeitadas (com defeito)
- Valor total das peças aceites
- Dias restantes de consignação
- Comissão acordada: 40% dinheiro / 50% crédito em loja

**Secção - Estado da Avaliação:**
- Badge de estado: "Pendente de Avaliação", "Avaliação Parcial (X de Y)", "Avaliação Concluída"
- Se pendente ou parcial: botão "Continuar Avaliação" (redireciona para PG-NEW-2)
- Data/hora do envio do email de avaliação (se já enviado)

**Secção - Tabela de Itens:**
- Colunas: Foto (thumb), ID, Descrição, Marca, Tamanho, Preço, Estado (badge), Dias em Loja
- Estados possíveis: À Venda (verde), Vendido (azul), Devolvido (cinza), Com Defeito (vermelho)
- Filtros inline: Estado, Marca

**Ações:**
- Adicionar item (abre formulário de avaliação)
- Devolver itens selecionados
- Renovar consignação
- Imprimir recibo de receção
- Reenviar email de avaliação

---

### PG-12: Lista de Fornecedores (`/suppliers`)

**Objetivo:** Gerir fornecedores/consignantes.

**Layout:**
- **Pesquisa:** Nome, Email, Telefone, NIF
- **Tabela:** Nome, Inicial, Email, Telefone, Comissão Dinheiro (40%), Comissão Crédito (50%), Itens Ativos, Ações
- **Ações:** Ver detalhes, Editar, Nova receção de peças
- **Botão:** "+ Novo Fornecedor"

---

### PG-13: Detalhe do Fornecedor (`/suppliers/:id`)

**Objetivo:** Visualizar perfil completo do fornecedor.

**Layout - Tabs:**

| Tab | Conteúdo |
|-----|----------|
| **Dados** | Nome, Email, Telefone (+351), NIF, Inicial, Comissões (40% dinheiro / 50% crédito), Data de cadastro |
| **Receções** | Lista de receções do fornecedor com estado de avaliação e consignação |
| **Itens** | Todos os itens deste fornecedor com filtros |
| **Acertos** | Histórico de pagamentos/acertos realizados |
| **Estatísticas** | Itens consignados, vendidos, devolvidos, tempo médio de venda, receita gerada |

---

### PG-14: Cadastro/Edição de Fornecedor (`/suppliers/:id/edit`)

**Campos:**
- Nome Completo (obrigatório)
- Email (obrigatório, validação de formato)
- Telefone (obrigatório, formato +351 XXX XXX XXX)
- NIF (opcional, validação portuguesa - 9 dígitos)
- Inicial para ID de item (1 caractere, obrigatório)
- Comissão em Dinheiro % (pré-preenchido 40%, editável, 0-100)
- Comissão em Crédito/Produto % (pré-preenchido 50%, editável, 0-100)
- Morada (opcional)
- Código Postal (opcional, formato XXXX-XXX)
- Localidade (opcional)
- Observações (textarea)

---

### PG-15: Devolução de Peças ao Fornecedor (`/consignments/returns`)

**Objetivo:** Gerir e processar devoluções de peças a fornecedores — peças com defeito identificadas na avaliação, peças não vendidas após expiração do período de consignação, ou devoluções solicitadas. (ver CU-14)

**Layout:**

**Secção 1 - Lista de Peças Pendentes de Devolução:**
- **Tabs:** Todas | Com Defeito | Consignação Expirada
- **Filtros:** Fornecedor, Data de receção (de/até)
- **Tabela:**

| Coluna | Descrição |
|--------|-----------|
| ID Item | Código de identificação da peça |
| Descrição | Nome/descrição da peça |
| Fornecedor | Nome do fornecedor/consignante |
| Motivo | Com Defeito (motivo), Consignação Expirada, Solicitação do Fornecedor |
| Data Entrada | Data em que a peça foi recebida |
| Dias em Loja | Número de dias desde a receção |
| Valor Avaliado | Preço original avaliado (€) |
| Ação | Checkbox para seleção + Botão "Processar Devolução" |

- **Indicadores visuais:**
  - Vermelho: peças com defeito (prioridade de devolução)
  - Laranja: consignação expirada (60+ dias)

**Secção 2 - Processar Devolução (modal ou painel lateral):**
- Fornecedor selecionado (preenchido automaticamente)
- Lista de peças selecionadas para devolução
- Método de contacto: Email / WhatsApp
- Campo: Observações (textarea opcional)
- Botão "Gerar Guia de Devolução"

**Após processar:**
- Sistema gera guia de devolução (PDF) contendo:
  - Nome do fornecedor
  - Data da devolução
  - Lista de peças devolvidas com valores originais (conforme RN-11)
  - Motivo de cada devolução
  - Espaço para assinatura de confirmação de levantamento
- Sistema envia notificação ao fornecedor (email ou WhatsApp) para levantamento
- Estado das peças atualizado para "Devolvido (DV)"
- Peças removidas do inventário ativo

**Ações rápidas (por peça):**
- Renovar consignação (+30 dias) — apenas para peças com consignação expirada
- Ver detalhe do item (redireciona para PG-04)

---

## 5. Ponto de Venda - POS (M3)

### PG-16: POS - Caixa (`/pos`)

**Objetivo:** Ecrã principal de vendas. Layout otimizado para utilização com scanner e teclado.

**Layout (ecrã inteiro, sem sidebar):**

```
+------------------------------------------------------------------+
|  [Logo]  Caixa #1 - Joana Silva    Aberto: 08:00    [X Fechar]   |
+----------------------------------+-------------------------------+
|                                  |                               |
|  Pesquisar item (ID ou nome)     |   CARRINHO                    |
|  +----------------------------+  |                               |
|  | Resultados da pesquisa     |  |   Item 1 - EUR 89,90      [X] |
|  | +------------------------+|  |   Item 2 - EUR 45,00      [X] |
|  | | Vestido Floral - 89,90 ||  |   Item 3 - EUR 120,00     [X] |
|  | | Maria Silva | M | Bom  ||  |                               |
|  | |      [+ Adicionar]     ||  |   Desconto: -EUR 12,74 (5%)   |
|  | +------------------------+|  |                               |
|  | +------------------------+|  |   ----------------------------  |
|  | | Blusa Seda - 45,00     ||  |   TOTAL: EUR 242,16           |
|  | | Ana Lima | S | Excelente||  |                               |
|  | |      [+ Adicionar]     ||  |   [ Finalizar Venda ]         |
|  | +------------------------+|  |                               |
|  +----------------------------+  |                               |
|                                  |                               |
|  [Cliente: Nao identificado] [+] |   Atalhos: F2=Nova F8=Busca   |
|                                  |   F4=Pagar ESC=Cancelar       |
+----------------------------------+-------------------------------+
```

**Funcionalidades:**
- Campo de pesquisa com foco automático (para scanner)
- Pesquisa por ID (código de barras) ou texto (nome/marca)
- Carrinho com itens adicionados
- Aplicar desconto (% ou valor fixo)
- Identificar cliente fidelidade
- Finalizar venda → Modal de pagamento

**Modal de Pagamento:**
- Método: Dinheiro, Cartão de Crédito, Cartão de Débito, MBWAY, Crédito em Loja
- Opção de pagamento dividido (2 métodos)
- Para dinheiro: campo "Valor recebido" com cálculo de troco
- Para MBWAY: campo "Número de telefone" (+351) para referência
- Botão "Confirmar Pagamento"
- Após confirmar: opção de imprimir recibo

---

### PG-17: POS - Abrir Caixa (`/pos/open`)

**Objetivo:** Iniciar sessão do caixa.

**Layout:**
- Informação do caixa (número, funcionário autenticado)
- Campo: Valor de abertura (troco inicial em €)
- Botão "Abrir Caixa"
- Após abrir, redireciona para POS - Caixa

---

### PG-18: POS - Fechar Caixa (`/pos/close`)

**Objetivo:** Encerrar sessão do caixa com reconciliação.

**Layout:**
- **Resumo do dia:**
  - Qtd de vendas
  - Total em Dinheiro, Cartão de Crédito, Cartão de Débito, MBWAY, Crédito em Loja
  - Valor esperado em dinheiro (abertura + vendas cash - devoluções cash)
- **Campo:** Valor contado em caixa (€)
- **Discrepância:** Sistema calcula diferença automaticamente
  - Verde: <= €5 (OK)
  - Amarelo: €5-50 (campo de justificação obrigatório)
  - Vermelho: > €50 (justificação + aprovação do gerente)
- **Botão:** "Fechar Caixa"
- **Após fechar:** Gera relatório de fecho (PDF)

---

### PG-19: Vendas do Dia (`/pos/sales`)

**Objetivo:** Listar vendas realizadas no caixa do dia.

**Layout:**
- **Filtros:** Data, Caixa, Funcionário
- **Tabela:** N.º Venda, Hora, Qtd Itens, Total, Forma Pagamento, Cliente, Ações
- **Ações:** Ver detalhes, Imprimir recibo
- **Totalizadores no topo:** Total vendido, Qtd vendas, Ticket médio

---

### PG-20: Detalhe da Venda (`/pos/sales/:id`)

**Objetivo:** Visualizar todos os detalhes de uma venda.

**Layout:**
- **Header:** N.º Venda, Data/Hora, Funcionário, Estado
- **Itens vendidos:** Tabela com ID, Nome, Marca, Preço, Fornecedor
- **Pagamento:** Forma, Valor, Troco (se dinheiro)
- **Cliente:** Nome, Pontos creditados (se fidelidade)
- **Ações:** Imprimir recibo, Processar devolução

---

### PG-21: Devoluções (`/pos/returns`)

**Objetivo:** Listar e processar devoluções.

**Layout:**
- **Tabela:** N.º Devolução, Venda Original, Data, Motivo, Tipo (Troca/Crédito), Valor, Estado
- **Botão:** "+ Nova Devolução"

---

### PG-22: Nova Devolução (`/pos/returns/new`)

**Objetivo:** Processar devolução/troca de item.

**Fluxo:**
1. Pesquisar venda original (campo: n.º da venda ou data)
2. Selecionar itens para devolver (checkbox)
3. Informar motivo
4. Escolher resolução: Troca (vai para POS com crédito) ou Crédito em Loja
5. Confirmar

---

## 6. Financeiro (M4)

### PG-23: Acertos com Fornecedores (`/finance/settlements`)

**Objetivo:** Gerir pagamentos de comissão a fornecedores.

**Layout:**
- **Tabs:** Pendentes | Processados | Todos
- **Tabela Pendentes:** Fornecedor, Período, Itens Vendidos, Valor Total Vendas, Comissão Loja, Valor a Pagar ao Fornecedor, Ação
- **Ação:** "Processar Acerto" → redireciona para PG-24

**Exemplo de cálculo exibido:**
- Item vendido a €100,00
  - Se acerto em dinheiro: Fornecedor recebe 40% = €40,00 | Loja retém 60% = €60,00
  - Se acerto em crédito em loja: Fornecedor recebe 50% = €50,00 | Loja retém 50% = €50,00

---

### PG-24: Novo Acerto (`/finance/settlements/new`)

**Objetivo:** Calcular e processar acerto com fornecedor.

**Layout:**
- **Seleção:** Fornecedor + Período (de/até)
- **Tabela de itens vendidos:** ID Item, Descrição, Data Venda, Preço Venda, Comissão Loja (%), Valor Loja (€), Valor Fornecedor (€)
- **Totais:** Total vendido, Total comissão loja, Total a pagar ao fornecedor
- **Forma de pagamento:** Dinheiro ou Crédito em Loja
  - Se dinheiro: fornecedor recebe 40% (loja retém 60%)
  - Se crédito em loja: fornecedor recebe 50% (loja retém 50%)
- **Recálculo dinâmico** ao trocar forma de pagamento

**Exemplo dinâmico (exibido na página):**
- Total de vendas: €500,00
  - Dinheiro: Fornecedor recebe €200,00 (40%) | Loja retém €300,00 (60%)
  - Crédito em Loja: Fornecedor recebe €250,00 (50%) | Loja retém €250,00 (50%)

- **Botão:** "Confirmar Acerto"
- **Gera:** Recibo de acerto (PDF) com todos os detalhes

---

### PG-25: Detalhe do Acerto (`/finance/settlements/:id`)

**Layout:**
- Header: ID, Fornecedor, Período, Estado (Pago/Pendente)
- Tabela de itens do acerto com valores detalhados (preço venda, % comissão, valor loja, valor fornecedor)
- Totais e forma de pagamento
- Botão: Imprimir recibo, Reprocessar (se erro)

---

### PG-26: Créditos em Loja (`/finance/credits`)

**Objetivo:** Gerir créditos de loja (clientes e fornecedores).

**Layout:**
- **Tabela:** Titular, Tipo (Cliente/Fornecedor), Saldo (€), Data Emissão, Validade, Estado
- **Filtros:** Tipo, Estado (Ativo/Expirado), Pesquisa por nome
- **Ações:** Ver extrato, Adicionar crédito manual

---

### PG-27: Fluxo de Caixa (`/finance/cashflow`)

**Objetivo:** Visão de entradas e saídas financeiras.

**Layout:**
- **Gráfico:** Entradas vs Saídas (barras empilhadas por semana/mês)
- **Tabela de movimentações:** Data, Tipo (Entrada/Saída), Descrição, Valor (€), Saldo Acumulado
- **Filtros:** Período, Tipo
- **Resumo:** Total Entradas, Total Saídas, Saldo Líquido

---

### PG-28: Despesas (`/finance/expenses`)

**Objetivo:** Registar e controlar despesas operacionais.

**Layout:**
- **Tabela:** Data, Categoria, Descrição, Valor (€), Recorrente (Sim/Não), Ações
- **Filtros:** Categoria, Período
- **Botão:** "+ Nova Despesa"
- **Resumo mensal:** Total por categoria (gráfico circular)

---

## 7. Relatórios (M5)

### PG-29: Relatório de Vendas (`/reports/sales`)

**Objetivo:** Análise detalhada de vendas.

**Métricas:**
- Receita total e por período (€)
- Ticket médio
- Qtd de vendas
- Top 10 itens/marcas/categorias vendidos
- Breakdown por forma de pagamento: Dinheiro, Cartão de Crédito, Cartão de Débito, MBWAY, Crédito em Loja (gráfico circular)
- Vendas por dia da semana (heatmap)
- Comparativo com período anterior (%)

**Filtros:** Período, Categoria, Marca, Funcionário, Forma de pagamento

**Exportação:** PDF, Excel

---

### PG-30: Relatório de Inventário (`/reports/inventory`)

**Objetivo:** Análise da saúde do stock.

**Métricas:**
- Total de itens em stock e valor total (€)
- Distribuição por estado (gráfico circular)
- Aging: distribuição por tempo em stock (0-15d, 15-30d, 30-45d, 45-60d, 60d+)
- Taxa de rotação (sell-through rate) por categoria/marca
- Itens com preço reduzido
- Previsão de devoluções (consignações a expirar)

---

### PG-31: Relatório de Fornecedores (`/reports/suppliers`)

**Objetivo:** Performance dos fornecedores.

**Métricas:**
- Ranking por: volume vendido, receita gerada, tempo médio de venda
- Taxa de venda (% de itens vendidos vs consignados) por fornecedor
- Taxa de devolução
- Taxa de rejeição (% de itens com defeito por fornecedor)
- Valor pendente de acerto (€)
- Ticket médio por fornecedor

---

### PG-32: Relatório Financeiro (`/reports/finance`)

**Objetivo:** Visão financeira consolidada.

**Métricas:**
- Receita bruta (total de vendas em €)
- Receita de comissões (o que a loja efetivamente ganha: 60% em acertos dinheiro, 50% em acertos crédito)
- Despesas operacionais
- Margem líquida
- Acertos pagos vs pendentes (€)
- Projeção de fluxo de caixa (próximos 30/60/90 dias)

---

## 8. Clientes (M6)

### PG-33: Lista de Clientes (`/customers`)

**Layout:**
- **Pesquisa:** Nome, NIF, Email, Telefone
- **Tabela:** Nome, Email, Telefone, Pontos, Total Gasto (€), Última Compra, Ações
- **Botão:** "+ Novo Cliente"

---

### PG-34: Detalhe do Cliente (`/customers/:id`)

**Layout - Tabs:**
- **Dados:** Nome, NIF, Email, Telefone (+351), Data de Nascimento, Morada
- **Fidelidade:** Pontos acumulados, Pontos resgatados, Saldo, Nível (Regular/VIP)
- **Compras:** Histórico de vendas vinculadas
- **Créditos:** Créditos em loja ativos

---

### PG-35: Programa de Fidelidade (`/customers/loyalty`)

**Layout:**
- Dashboard de fidelidade: total de clientes cadastrados, pontos emitidos/resgatados
- Configuração: taxa de conversão (€/ponto), mínimo de resgate, validade
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
- Tipo: Desconto por categoria, Desconto por tempo em stock, Progressivo, Compre X Leve Y
- Regras (dinâmico conforme tipo)
- Categorias/Marcas afetadas
- Desconto (% ou valor fixo em €)
- Pré-visualização: lista de itens que serão afetados

---

## 10. Administração (M8)

### PG-38: Utilizadores (`/admin/users`)

**Layout:**
- **Tabela:** Nome, Email, Perfil (Caixa/Gerente/Financeiro/Admin), Estado (Ativo/Inativo), Último Acesso
- **Ações:** Editar perfil/permissões, Desativar
- **Botão:** "+ Novo Utilizador"
- **Modal de edição:** Nome, Email, Perfil (dropdown), Permissões personalizadas (checkboxes)

---

### PG-39: Definições do Sistema (`/admin/settings`)

**Layout - Secções:**
- **Consignação:** Período padrão (dias), Comissão padrão dinheiro % (40%), Comissão padrão crédito % (50%)
- **POS:** Desconto máximo sem autorização %, Métodos de pagamento habilitados (Dinheiro, Cartão Crédito, Cartão Débito, MBWAY, Crédito em Loja)
- **Fidelidade:** Pontos por €, Conversão de resgate, Validade dos pontos
- **Alertas:** Thresholds de stock parado (30/45/60 dias configuráveis)
- **Fiscal:** NIF da empresa, Regime fiscal, CAE
- **Notificações:** Habilitar SMS, Email, WhatsApp

---

### PG-40: Registo de Auditoria (`/admin/audit-log`)

**Layout:**
- **Filtros:** Utilizador, Tipo de ação, Módulo, Período
- **Tabela:** Data/Hora, Utilizador, Ação, Módulo, Detalhe (expansível), IP
- **Tipos de ação:** Criação, Edição, Eliminação, Login, Alteração de preço, Desconto, Estorno, Avaliação de peças, Envio de email

---

### PG-41: Dados da Loja (`/admin/store`)

**Campos:**
- Nome da loja (Oui Circular)
- NIF da empresa
- CAE (Classificação de Atividade Económica)
- Morada completa
- Código Postal (formato XXXX-XXX)
- Localidade
- Telefone (+351)
- Email
- Logótipo (upload)
- Informações para recibo/fatura
- Horário de funcionamento

---

## 11. Portal do Fornecedor (Externo)

### PG-42: Portal - Dashboard (`/portal/dashboard`)

**Objetivo:** Visão geral para o fornecedor acompanhar as suas peças.

**Layout:**
- **KPI Cards:** Peças na Loja, Peças Vendidas (mês), Valor a Receber (€), Total Recebido (€)
- **Últimas vendas:** Lista das últimas 5 peças vendidas com data e preço
- **Receções ativas:** Lista com qtd de peças e valor total
- **Informação de comissão:** "Recebe 40% em dinheiro ou 50% em crédito em loja"

---

### PG-43: Portal - As Minhas Peças (`/portal/items`)

**Layout:**
- **Filtros:** Estado (Em Loja, Vendido, Devolvido, Com Defeito), Período
- **Tabela:** ID, Descrição, Marca, Preço (€), Estado, Data Entrada, Dias em Loja
- Somente visualização (sem edição)

---

### PG-44: Portal - Extrato (`/portal/statements`)

**Layout:**
- **Filtros:** Período
- **Tabela de acertos:** Data, Período, Qtd Itens, Valor Bruto (€), Comissão Loja (%), Valor Líquido (€), Forma de Pagamento
- **Totais:** Acumulado do período filtrado
- **Download:** PDF do recibo de cada acerto
