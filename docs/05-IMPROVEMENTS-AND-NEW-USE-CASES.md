# SHS - Melhorias Sugeridas e Novos Casos de Uso

## Versão: 1.0 | Última Atualização: 2026-02-11

Baseado na análise da documentação atual e pesquisa de referência (SimpleConsign Insights, Peca Rara, ThredUp, Vinted).

---

## 1. Melhorias nos Módulos Existentes

### 1.1 Módulo de Inventário (M1)

| Melhoria | Descrição | Prioridade |
|----------|-----------|------------|
| **Fotos do Item** | Upload de até 5 fotos por item com crop/resize automático. Essencial para e-commerce e identificação visual. | P0 |
| **Código de Barras / QR Code** | Geração automática de etiqueta com QR code contendo: ID, preço, tamanho, marca. Impressão em lote. | P0 |
| **Condição Padronizada** | Escala de condição com descrições: Novo com Etiqueta (NWT), Novo sem Etiqueta (NWOT), Excelente, Bom, Regular | P0 |
| **Categorização Hierárquica** | Categorias em árvore: Roupas > Feminino > Vestidos > Midi. Melhora filtros e relatórios. | P1 |
| **Histórico de Preço** | Timeline visual de todas as alterações de preço com motivo, aprovador e data. | P1 |
| **Inventário em Lote** | Contagem de inventário físico com conferência por setor/prateleira. Relatório de divergências. | P1 |
| **Localização no Estoque** | Campo para arara/prateleira/seção, facilitando encontrar o item na loja. | P2 |

### 1.2 Módulo de Consignação (M2)

| Melhoria | Descrição | Prioridade |
|----------|-----------|------------|
| **Portal do Fornecedor** | Área web onde fornecedores acompanham status dos itens, vendas e pagamentos pendentes. Inspirado no SimpleConsign. | P1 |
| **Contrato Digital** | Geração de PDF do contrato de consignação com assinatura digital. | P1 |
| **Avaliação Rápida** | Fluxo de avaliação streamlined: foto → categoria → condição → preço sugerido (baseado em histórico de vendas por marca/categoria). | P1 |
| **Notificações Automáticas** | SMS/Email para fornecedor: item vendido, consignação expirando, pagamento disponível. | P1 |
| **Renovação Automática** | Opção de renovação automática de consignação por mais 30 dias com alerta ao fornecedor. | P2 |

### 1.3 Módulo POS (M3)

| Melhoria | Descrição | Prioridade |
|----------|-----------|------------|
| **Modo Offline** | PWA que funciona sem internet, sincronizando quando a conexão retornar. Crítico para lojas com internet instável. | P1 |
| **Gaveta de Caixa** | Integração com gaveta de caixa (cash drawer) via porta serial/USB. | P2 |
| **Venda Rápida** | Tela simplificada para vendas de alto volume: scan → confirmar → pagamento em 3 cliques. | P1 |
| **Hold/Reserva** | Reservar itens para cliente por até 24h antes de confirmar venda. | P2 |

### 1.4 Módulo Financeiro (M4)

| Melhoria | Descrição | Prioridade |
|----------|-----------|------------|
| **Fluxo de Caixa** | Visão completa de entradas/saídas com projeção para próximos 30/60/90 dias. | P1 |
| **Despesas Operacionais** | Cadastro de despesas fixas e variáveis (aluguel, salários, luz) para cálculo de lucratividade real. | P2 |
| **Contas a Pagar** | Controle de pagamentos pendentes a fornecedores com vencimento e prioridade. | P1 |
| **Integração Bancária** | Conciliação bancária automática via Open Banking / OFX. | P3 |

### 1.5 Módulo Reports & BI (M5)

| Melhoria | Descrição | Prioridade |
|----------|-----------|------------|
| **App Mobile de Insights** | Inspirado no SimpleConsign Insights: KPIs no celular do dono. Receita diária, itens vendidos, status dos caixas. | P1 |
| **Dashboard em Tempo Real** | Atualização automática a cada hora (como SimpleConsign) com: vendas do dia, ticket médio, métodos de pagamento populares. | P1 |
| **Comparativo YoY** | Comparação ano-a-ano e mês-a-mês de vendas, receita e rotação de estoque. | P1 |
| **Sell-Through Rate** | Taxa de venda por categoria/marca/fornecedor. Identifica quais fornecedores trazem os melhores itens. | P1 |
| **Monitoramento de Terminais** | Visão em tempo real do status de cada caixa: aberto/fechado, saldo, último fechamento. Inspirado no SimpleConsign. | P1 |
| **Relatório de Fornecedores** | Ranking de fornecedores por: volume vendido, tempo médio de venda, taxa de devolução. | P1 |
| **Exportação Avançada** | PDF, Excel, CSV com filtros customizáveis e agendamento de envio por email. | P2 |

---

## 2. Novos Casos de Uso Sugeridos

### CU-24: Portal do Fornecedor (Autoatendimento) - P1

**Inspiração:** SimpleConsign permite que fornecedores acompanhem seus itens online.

**Ator Principal:** Fornecedor/Consignante

**Fluxo Principal:**
1. Fornecedor acessa portal com login próprio (email + senha)
2. Dashboard mostra: itens em loja, itens vendidos, valor pendente de recebimento
3. Fornecedor pode ver detalhes de cada item (status, preço atual, dias em loja)
4. Fornecedor visualiza extrato de pagamentos recebidos
5. Fornecedor recebe notificações de vendas e pagamentos

**Regras de Negócio:**
- Acesso somente leitura (sem edição de preços ou itens)
- Dados visíveis apenas do próprio fornecedor

---

### CU-25: Precificação Inteligente (AI-Assisted) - P2

**Inspiração:** ThredUp usa IA para precificar 100K+ itens/dia.

**Ator Principal:** Funcionário/Gerente

**Fluxo Principal:**
1. Ao cadastrar item, funcionário informa: marca, categoria, condição, tamanho
2. Sistema consulta histórico de vendas de itens similares
3. Sistema sugere faixa de preço (mínimo, recomendado, máximo)
4. Funcionário ajusta e confirma preço final
5. Sistema registra se preço sugerido foi aceito ou modificado

**Dados para Sugestão:**
- Preço médio de venda de itens da mesma marca/categoria nos últimos 90 dias
- Taxa de venda (sell-through rate) por faixa de preço
- Tempo médio em estoque por faixa de preço

---

### CU-26: Gestão de Promoções e Campanhas - P1

**Ator Principal:** Gerente

**Fluxo Principal:**
1. Gerente cria campanha: nome, período, regras
2. Tipos de promoção:
   - Desconto por categoria (ex: 20% em vestidos)
   - Desconto por tempo em estoque (ex: 30% em itens com 45+ dias)
   - Compre X leve Y
   - Desconto progressivo (1 peça 10%, 2 peças 15%, 3+ peças 20%)
3. Sistema aplica automaticamente no POS durante vigência
4. Relatório de performance da campanha ao final

---

### CU-27: Gestão de Despesas e Custo Operacional - P2

**Ator Principal:** Financeiro/Admin

**Fluxo Principal:**
1. Cadastro de categorias de despesa: aluguel, salários, energia, marketing
2. Lançamento de despesas fixas (recorrentes) e variáveis
3. Dashboard de custo operacional mensal
4. Cálculo de margem líquida: Receita de Comissões - Despesas Operacionais
5. Alerta quando despesas ultrapassam X% da receita

---

### CU-28: Monitoramento de Terminais (Caixas) em Tempo Real - P1

**Inspiração:** SimpleConsign Insights mostra status dos terminais no app mobile.

**Ator Principal:** Gerente/Dono

**Fluxo Principal:**
1. Dashboard mostra todos os caixas abertos com:
   - Funcionário responsável
   - Hora de abertura
   - Número de vendas processadas
   - Valor total vendido
   - Saldo atual em dinheiro
2. Alerta quando caixa está aberto há mais de 10h (esqueceram de fechar)
3. Histórico de fechamentos: valores, discrepâncias, justificativas

---

### CU-29: Intake Rápido em Lote - P1

**Inspiração:** Peca Rara recebe 15-40 fornecedores/dia.

**Ator Principal:** Funcionário

**Fluxo Principal:**
1. Modo "intake rápido" otimizado para alto volume
2. Fornecedor selecionado uma vez, permanece durante toda a sessão
3. Para cada item: foto (câmera do tablet) → categoria → condição → preço → próximo
4. Fluxo contínuo sem navegar entre telas
5. Ao finalizar, sistema gera contrato com todos os itens

---

### CU-30: Análise de Performance por Canal - P1

**Inspiração:** SimpleConsign mostra vendas por canal (online vs. loja física).

**Ator Principal:** Gerente/Dono

**Fluxo Principal:**
1. Dashboard compara receita por canal: loja física, e-commerce, marketplace
2. Métricas por canal: ticket médio, itens vendidos, taxa de conversão
3. Tendência de crescimento por canal (gráfico temporal)
4. Identificar quais categorias vendem melhor em cada canal

---

### CU-31: Gestão de Créditos em Loja (Store Credits) - P1

**Ator Principal:** Caixa/Financeiro

**Fluxo Principal:**
1. Créditos gerados por: devolução de venda, pagamento a fornecedor em crédito
2. Sistema mantém saldo de crédito por pessoa (cliente ou fornecedor)
3. No POS, crédito aparece como forma de pagamento disponível
4. Extrato de créditos: emissão, uso parcial, saldo, vencimento
5. Alerta de créditos próximos do vencimento (180 dias)

---

### CU-32: Integração com WhatsApp Business - P2

**Ator Principal:** Sistema (automático)

**Fluxo Principal:**
1. Notificações automáticas via WhatsApp para fornecedores:
   - Item vendido
   - Pagamento disponível
   - Consignação expirando
2. Notificações para clientes fidelidade:
   - Pontos acumulados
   - Promoções ativas
   - Aniversário / desconto especial
3. Template de mensagens configuráveis pelo admin

---

### CU-33: Audit Log Detalhado - P1

**Ator Principal:** Admin

**Fluxo Principal:**
1. Log de todas as ações críticas no sistema:
   - Alterações de preço (quem, quando, de/para)
   - Descontos aplicados acima de 10%
   - Estornos e devoluções
   - Cancelamentos de venda
   - Alterações de configuração
   - Acessos ao sistema (login/logout)
2. Filtros por: usuário, tipo de ação, período, módulo
3. Exportação para auditoria externa

---

## 3. Melhorias de UX/UI Sugeridas

| Melhoria | Descrição |
|----------|-----------|
| **Temas Claro/Escuro** | Modo dark para uso prolongado, especialmente no POS |
| **Atalhos de Teclado** | Atalhos no POS: F2 (nova venda), F4 (pagamento), F8 (buscar item), ESC (cancelar) |
| **Tour Guiado** | Onboarding interativo para novos usuários do sistema |
| **Notificações In-App** | Centro de notificações com alertas de: estoque parado, consignações expirando, caixas abertos |
| **Busca Global** | Campo de busca universal no header que pesquisa: itens, fornecedores, clientes, vendas |
| **Responsividade Tablet** | Layout otimizado para tablet no POS e intake de itens |
| **Impressão de Etiquetas** | Impressão direta de etiquetas com preço/código de barras via impressora térmica |

---

## 4. Melhorias Técnicas Sugeridas

| Melhoria | Descrição |
|----------|-----------|
| **SignalR para Real-Time** | WebSocket para atualizar dashboard e status de caixas em tempo real |
| **Cache com Redis** | Cache de consultas frequentes (marcas, categorias, configurações) |
| **Background Jobs** | Hangfire/Quartz para: alertas de consignação, relatórios agendados, sync e-commerce |
| **API Rate Limiting** | Proteção contra abuso da API |
| **Health Checks** | Endpoint de saúde para monitoramento |
| **Testes Automatizados** | Unit tests para business rules, Integration tests para endpoints |
| **CI/CD Pipeline** | GitHub Actions para build, test, deploy automático |
| **Backup Automatizado** | Backup diário do PostgreSQL com retenção de 30 dias |

---

## 5. Priorização Sugerida para Próximas Sprints

### Sprint 1-2 (MVP Completo)
- Completar POS (CU-11 a CU-14)
- Gestão de Usuários e Permissões (CU-22)
- Fotos de itens
- Geração de código de barras

### Sprint 3-4
- Settlement/Acerto Financeiro (CU-09)
- Relatórios básicos de vendas (CU-15)
- Dashboard executivo (CU-17)
- Monitoramento de terminais (CU-28)

### Sprint 5-6
- Portal do Fornecedor (CU-24)
- Promoções e Campanhas (CU-26)
- Gestão de Créditos em Loja (CU-31)
- Audit Log (CU-33)

### Sprint 7-8
- Intake rápido em lote (CU-29)
- App mobile de insights (inspiração SimpleConsign)
- Análise de performance por canal (CU-30)
- Precificação inteligente (CU-25)
