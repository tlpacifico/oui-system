# Oui Circular - Documento de Casos de Uso

## Legenda

| Simbolo | Significado |
|---------|-------------|
| **[DONE]** | Ja implementado no codigo atual |
| **[TODO]** | Ainda nao implementado |
| **P0** | Obrigatorio para MVP |
| **P1** | Importante, segunda fase |
| **P2** | Nice-to-have, melhoria futura |

---

## MODULO 1: GESTAO DE INVENTARIO

### CU-01: Registar Peca no Inventario [TODO] - P0

**Ator Principal:** Funcionario da Loja
**Pre-condicoes:** Funcionario autenticado, Rececao de pecas existe (CU-08)

**Fluxo Principal:**
1. Funcionario navega ate a rececao e seleciona "Avaliar Peca"
2. Sistema apresenta formulario de avaliacao
3. Funcionario preenche:
   - Nome/descricao
   - Marca (selecionar do catalogo)
   - Tamanho
   - Cor
   - Composicao (tipo de tecido)
   - Condicao da peca (Excelente, Muito Bom, Bom, Razoavel)
   - Tags/categorias
   - Valor avaliado (preco de venda)
4. Sistema gera automaticamente numero de identificacao: `{InicialFornecedor}{YYYYMM}{Sequencia:0000}`
5. Sistema guarda a peca associada a rececao/consignacao
6. Estado da peca definido como `Avaliado (AV)`

**Fluxos Alternativos:**
- 3a. Marca nao existe: funcionario pode usar catalogo de marcas existente
- 3b. Peca com defeito: funcionario marca como `Com Defeito (CD)` e regista motivo da recusa
- 4a. Sequencia mensal reinicia no inicio de cada mes

**Pos-condicoes:** Peca disponivel no inventario com numero de identificacao unico

**Nota:** Este caso de uso e executado durante a Etapa 2 (Avaliacao), SEM o cliente presente.

---

### CU-02: Pesquisar/Consultar Inventario [TODO] - P0

**Ator Principal:** Funcionario/Gestor
**Pre-condicoes:** Utilizador autenticado

**Fluxo Principal:**
1. Utilizador acede a lista de pecas
2. Sistema apresenta pecas com filtros:
   - Nome da peca (pesquisa texto)
   - Faixa de preco (min/max)
   - Tamanho
   - Marca
   - Cor
   - Nome do fornecedor
   - Intervalo de datas de consignacao
   - Estado (Recebido, Avaliado, A Venda, Vendido, Com Defeito, Devolvido, Pago)
   - Origem (Consignacao, Compra Propria)
3. Utilizador aplica filtros desejados
4. Sistema devolve resultados paginados com: nome, marca, tamanho, preco, estado, fornecedor, origem
5. Utilizador pode navegar pelas paginas

---

### CU-03: Atualizar Preco da Peca [TODO] - P0

**Ator Principal:** Gestor
**Pre-condicoes:** Peca existe no sistema

**Fluxo Principal:**
1. Gestor encontra peca via pesquisa ou consignacao
2. Sistema apresenta detalhes da peca
3. Gestor seleciona "Editar"
4. Gestor atualiza valor avaliado e/ou outros atributos
5. Sistema guarda alteracoes com registo de auditoria (AlteradoPor, AlteradoEm)

**Regras de Negocio:**
- RN-01: Se peca e consignada, comissao deve ser recalculada na venda
- RN-02: Alteracoes de preco acima de X% devem exigir aprovacao do gestor [TODO]
- RN-03: Historico de precos deve ser mantido [TODO]

---

### CU-05: Eliminar/Remover Peca [TODO] - P0

**Ator Principal:** Gestor/Funcionario
**Pre-condicoes:** Peca existe, ainda nao vendida

**Fluxo Principal:**
1. Utilizador encontra a peca
2. Utilizador seleciona "Eliminar"
3. Sistema solicita confirmacao
4. Sistema executa soft-delete (define flag IsDeleted)
5. Peca deixa de aparecer no inventario ativo

---

### CU-06: Registar Peca de Compra Propria [TODO] - P0

**Ator Principal:** Funcionario/Gestor
**Pre-condicoes:** Utilizador autenticado

**Fluxo Principal:**
1. Funcionario seleciona "Nova Peca - Compra Propria"
2. Sistema apresenta formulario de registo
3. Funcionario preenche:
   - Nome/descricao
   - Marca
   - Tamanho
   - Cor
   - Composicao
   - Condicao
   - Preco de custo (quanto a loja pagou)
   - Preco de venda
   - Origem da compra (Humana, Vinted, H&M, Acervo Pessoal, Outro)
4. Sistema gera numero de identificacao
5. Sistema guarda peca com tipo `Compra Propria`
6. Estado definido como `A Venda (DL)`

**Pos-condicoes:** Peca disponivel no inventario para venda. Nao ha comissao a pagar - lucro total e da loja.

**Nota:** A Oui Circular tem um modelo misto: vende pecas consignadas E pecas compradas diretamente (Humana, Vinted, H&M, acervo pessoal).

---

## MODULO 2: GESTAO DE CONSIGNACAO

### CU-07: Registar Fornecedor/Consignante [TODO] - P0

**Ator Principal:** Funcionario
**Pre-condicoes:** Funcionario autenticado

**Fluxo Principal:**
1. Funcionario seleciona "Novo Fornecedor"
2. Sistema apresenta formulario de registo
3. Funcionario preenche:
   - Nome
   - Email
   - Telefone (+351 XXX XXX XXX)
   - NIF (opcional)
   - Inicial (letra para geracao de ID das pecas)
   - PorcInLoja (% do valor de venda que vira credito em loja, ex: 50)
   - PorcInDinheiro (% do valor de venda resgatavel em dinheiro, ex: 40)
4. Sistema gera codigo do fornecedor
5. Sistema guarda fornecedor

**Regras de Negocio:**
- Cada fornecedor tem percentagens configuráveis:
  - **PorcInLoja** (% do valor de venda que vira credito em loja) - ex: 50%
  - **PorcInDinheiro** (% do valor de venda que pode ser resgatado em dinheiro) - ex: 40%
- Quando um item e vendido, o fornecedor acumula AMBOS: credito em loja E valor resgatavel em dinheiro

---

### CU-08: Rececao de Pecas (Etapa 1 - Contagem) [TODO] - P0

**Ator Principal:** Funcionario
**Pre-condicoes:** Funcionario autenticado

**Fluxo Principal:**
1. Cliente chega a loja com saco/bolsa de roupas
2. Funcionario seleciona "Nova Rececao"
3. Funcionario pesquisa ou seleciona fornecedor existente
4. Se cliente novo: funcionario regista fornecedor (CU-07)
5. Funcionario CONTA as pecas recebidas (sem avaliar)
6. Funcionario introduz a quantidade de pecas no sistema
7. Sistema regista a rececao com:
   - Nome do cliente/fornecedor
   - Data de rececao
   - Quantidade de pecas recebidas
   - Estado: `Recebido`
8. Sistema gera RECIBO DE RECECAO (PDF) contendo:
   - Nome do cliente
   - Data de rececao
   - Numero do recibo (ex: REC-2026-0045)
   - Quantidade de pecas recebidas
   - **SEM valores** (pecas ainda nao foram avaliadas)
   - Termos de comissao (40% dinheiro / 50% credito)
   - Campo para assinatura do cliente
   - Campo para assinatura da loja
9. Funcionario imprime o recibo
10. Cliente assina o recibo
11. Cliente vai embora

**Fluxos Alternativos:**
- 4a. Cliente ja registado: funcionario seleciona do catalogo
- 8a. Falha na impressao: sistema permite reimprimir recibo

**Pos-condicoes:** Pecas registadas como "Recebido", aguardando avaliacao. Cliente ja saiu da loja.

**IMPORTANTE:** A avaliacao NAO acontece neste momento. O cliente NAO fica a esperar. A contagem e rapida e o cliente assina apenas a confirmacao de entrega.

---

### CU-09: Avaliar Pecas (Etapa 2 - Avaliacao) [TODO] - P0

**Ator Principal:** Funcionario
**Pre-condicoes:** Rececao existe com pecas no estado "Recebido"

**Fluxo Principal:**
1. Funcionario acede a lista de rececoes pendentes de avaliacao
2. Funcionario seleciona uma rececao
3. Sistema apresenta os dados da rececao (fornecedor, data, quantidade)
4. Para cada peca do lote, funcionario:
   a. Inspeciona a condicao (Excelente, Muito Bom, Bom, Razoavel)
   b. Identifica marca, tamanho, cor, composicao
   c. Define o valor de venda (preco avaliado)
   d. Regista no sistema (usa CU-01)
   e. Sistema gera numero de identificacao automaticamente
5. Pecas aprovadas recebem estado `Avaliado (AV)`
6. Pecas com defeito ou sem condicao de venda:
   a. Funcionario marca como `Com Defeito (CD)`
   b. Funcionario regista o motivo (mancha, fecho partido, desgaste, etc.)
   c. Pecas separadas para devolucao
7. Funcionario finaliza avaliacao do lote

**Pos-condicoes:** Todas as pecas do lote avaliadas. Pecas aprovadas com estado "Avaliado". Pecas com defeito com estado "Com Defeito". Sistema pronto para enviar email ao cliente (CU-10).

**Nota:** Esta etapa acontece POSTERIORMENTE, sem o cliente presente. Pode ser horas ou dias apos a rececao.

---

### CU-10: Enviar Email de Avaliacao (Etapa 3 - Comunicacao) [TODO] - P0

**Ator Principal:** Funcionario / Sistema
**Pre-condicoes:** Avaliacao de todas as pecas de uma rececao concluida (CU-09)

**Fluxo Principal:**
1. Apos conclusao da avaliacao, sistema gera resumo:
   - Lista de pecas aceites com respetivos valores de venda
   - Lista de pecas recusadas com motivos
   - Totais (quantidade aceite, quantidade recusada, valor total potencial)
   - Comissoes aplicaveis (40% dinheiro / 50% credito)
2. Funcionario revisa o resumo
3. Funcionario seleciona "Enviar Email ao Cliente"
4. Sistema envia email ao fornecedor/cliente com:
   - Saudacao personalizada
   - Lista de pecas aceites (codigo, descricao, condicao, valor)
   - Lista de pecas recusadas (descricao, motivo da recusa)
   - Informacao sobre levantamento de pecas recusadas
   - Comissoes (ex: "Se todas vendidas, recebe EUR X em dinheiro ou EUR Y em credito")
5. Sistema regista que email foi enviado (data/hora)

**Fluxos Alternativos:**
- 3a. Funcionario ajusta valores antes de enviar
- 4a. Email falha: sistema notifica funcionario para reenviar ou contactar por telefone
- 5a. Cliente responde com objecoes: funcionario ajusta valores e reenvia

**Pos-condicoes:** Cliente informado sobre avaliacao. Apos confirmacao do cliente, pecas passam a estado "A Venda (DL)".

**Modelo de Email:**
```
Assunto: Oui Circular - Avaliacao das suas pecas

Ola [Nome],

Concluimos a avaliacao das [X] pecas que nos entregou em [DD/MM/YYYY].

PECAS ACEITES ([N]):
| Cod  | Descricao              | Condicao  | Valor  |
|------|------------------------|-----------|--------|
| AC150| Vestido midi estampado | Excelente | 18,00 EUR |
| ...  | ...                    | ...       | ...    |
| TOTAL|                        |           | XX,XX EUR |

PECAS RECUSADAS ([M]):
- [Descricao]: [motivo]
- [Descricao]: [motivo]

As pecas recusadas ficam disponiveis para levantamento na loja.

Caso as pecas sejam vendidas, recebera:
- 40% em dinheiro (XX,XX EUR se todas vendidas)
- OU 50% em credito em loja (XX,XX EUR se todas vendidas)

Alguma questao, nao hesite em contactar-nos.

Com os melhores cumprimentos,
Oui Circular
```

---

### CU-11: Criar Consignacao (Fluxo Completo) [TODO] - P0

**Ator Principal:** Funcionario
**Pre-condicoes:** Fornecedor registado

**Fluxo Principal:**
Este caso de uso descreve o fluxo completo de consignacao, que e composto por 3 etapas sequenciais:

1. **Etapa 1 - Rececao (CU-08):**
   - Cliente chega com pecas
   - Funcionario conta as pecas (sem avaliar)
   - Se cliente novo, regista fornecedor (CU-07)
   - Imprime recibo de rececao (sem valores)
   - Cliente assina e vai embora

2. **Etapa 2 - Avaliacao (CU-09):**
   - Posteriormente, sem o cliente presente
   - Funcionario avalia cada peca: condicao, marca, tamanho, cor, preco
   - Pecas com defeito separadas para devolucao
   - Pecas aprovadas recebem estado "Avaliado"

3. **Etapa 3 - Comunicacao (CU-10):**
   - Sistema/funcionario envia email ao cliente
   - Email contem: pecas aceites + valores, pecas recusadas + motivos, comissoes
   - Apos confirmacao do cliente, pecas passam a "A Venda"

**Lifecycle de Estado:**
```
Recebido --> Avaliado --> A Venda (DL) --> Vendido (VD) --> Pago (PG)
                |                            |
           Com Defeito (CD)              Devolvido (DV)
                |
           Devolvido (DV)
```

**Regras de Negocio:**
- RN-03: Periodo de consignacao padrao: 60 dias [TODO]
- RN-04: Apos expiracao, sistema alerta para renovacao ou devolucao [TODO]
- RN-05: PorcInLoja e PorcInDinheiro configuráveis por fornecedor

---

### CU-12: Pesquisar Consignacoes [TODO] - P0

**Ator Principal:** Funcionario/Gestor

**Fluxo Principal:**
1. Utilizador acede a lista de consignacoes/rececoes
2. Sistema apresenta lista paginada com pesquisa
3. Filtros disponiveis:
   - Nome do fornecedor
   - Data de rececao
   - Estado (Pendente avaliacao, Avaliado, Em venda, etc.)
4. Utilizador pode ver detalhes da consignacao incluindo todas as pecas
5. Utilizador pode navegar para editar consignacao

---

### CU-13: Processar Acerto de Consignacao [TODO] - P1

**Ator Principal:** Gestor/Proprietario
**Pre-condicoes:** Pecas consignadas foram vendidas

**Fluxo Principal:**
1. Sistema lista pecas vendidas pendentes de pagamento, agrupadas por fornecedor
2. Gestor seleciona fornecedor e periodo
3. Sistema calcula (usando PorcInLoja e PorcInDinheiro do fornecedor):
   - Valor total de vendas
   - Credito em loja: Total x PorcInLoja (ex: 50%)
   - Valor resgatavel em dinheiro: Total x PorcInDinheiro (ex: 40%)
4. Sistema gera credito em loja automaticamente para o fornecedor
5. Gestor processa resgate em dinheiro (se solicitado pelo fornecedor)
6. Sistema regista: credito emitido + resgate em dinheiro (se aplicavel)
7. Sistema envia comprovativo ao fornecedor (via WhatsApp)
8. Sistema atualiza estado das pecas para `Pago (PG)`

**Exemplo de Calculo (PorcInLoja=50%, PorcInDinheiro=40%):**
```
Itens vendidos: Calca 20 EUR + Camisa 20 EUR = Total 40 EUR
- Credito em loja:    40 x 0,50 = 20,00 EUR (para compras na loja)
- Resgate em dinheiro: 40 x 0,40 = 16,00 EUR (pode levantar em numerario)
- Loja fica: 40 - 20 - 16 = 4,00 EUR (10%)
```

**Regras de Negocio:**
- RN-05: PorcInLoja e PorcInDinheiro configuráveis por fornecedor
- RN-06: Limite minimo de acerto (opcional): 10,00 EUR
- RN-07: Relatorio de acerto deve listar cada peca com data de venda, preco e comissao
- RN-08: Comunicacao de acertos feita via WhatsApp

---

### CU-13a: Resgatar Credito do Fornecedor em Dinheiro [TODO] - P1

**Ator Principal:** Gestor/Financeiro
**Pre-condicoes:** Fornecedor tem saldo PorcInDinheiro disponivel para resgate

**Fluxo Principal:**
1. Fornecedor solicita resgate do credito em dinheiro
2. Funcionario identifica o fornecedor no sistema
3. Sistema apresenta saldo disponivel para resgate em dinheiro (valor acumulado PorcInDinheiro)
4. Fornecedor indica valor a resgatar (ate ao saldo disponivel)
5. Funcionario confirma e processa o resgate
6. Sistema regista a transacao de resgate (data, valor, fornecedor)
7. Funcionario entrega o numerario ao fornecedor

**Regras de Negocio:**
- RN-13a: O valor resgatado nao pode exceder o saldo PorcInDinheiro do fornecedor
- RN-13b: Resgates devem ser registados para auditoria e conciliacao

---

### CU-14: Devolver Pecas ao Fornecedor [TODO] - P1

**Ator Principal:** Funcionario
**Pre-condicoes:** Pecas com estado "Com Defeito" ou periodo de consignacao expirado

**Fluxo Principal:**
1. Sistema alerta sobre pecas com defeito pendentes de devolucao ou consignacoes expiradas
2. Funcionario seleciona pecas para devolucao
3. Sistema gera guia de devolucao
4. Funcionario contacta fornecedor para levantamento (email ou WhatsApp)
5. Apos confirmacao de levantamento, sistema remove pecas do inventario
6. Sistema atualiza estado para `Devolvido (DV)`

**Regras de Negocio:**
- RN-09: Pecas com defeito identificadas na avaliacao sao imediatamente separadas
- RN-10: Pecas nao vendidas apos 60 dias geram notificacao automatica de devolucao
- RN-11: Guia de devolucao deve listar todas as pecas devolvidas com valores originais

---

## MODULO 3: PONTO DE VENDA (POS/CAIXA)

### CU-15: Abrir Caixa [TODO] - P0

**Ator Principal:** Operador de Caixa
**Pre-condicoes:** Operador autenticado

**Fluxo Principal:**
1. Operador inicia sessao no POS
2. Sistema solicita montante de abertura (fundo de caixa)
3. Operador introduz montante em caixa
4. Sistema regista abertura com data/hora e identidade do operador
5. Sistema habilita POS para transacoes de venda

**Regras de Negocio:**
- RN-12: Apenas uma caixa aberta por operador de cada vez
- RN-13: Montante de abertura deve ser registado para reconciliacao

---

### CU-16: Processar Venda [TODO] - P0

**Ator Principal:** Operador de Caixa
**Pre-condicoes:** Caixa aberta

**Fluxo Principal:**
1. Operador pesquisa peca por codigo de barras ou pesquisa manual
2. Sistema apresenta detalhes e preco da peca
3. Operador adiciona peca ao carrinho
4. Repetir passos 1-3 para pecas adicionais
5. Operador finaliza venda
6. Sistema apresenta total
7. Cliente seleciona metodo de pagamento:
   - Dinheiro
   - Cartao de Credito
   - Cartao de Debito
   - MBWAY
   - Credito em Loja (operador identifica o fornecedor para debitar do seu credito)
   - Misto (dividir entre metodos)
8. Operador processa pagamento
9. Sistema remove pecas do inventario disponivel
10. Sistema gera recibo
11. Sistema imprime recibo

**Fluxos Alternativos:**
- 4a. Aplicar desconto: operador introduz percentagem/valor (sujeito a RN-14)
- 4b. Aplicar cupao promocional: sistema valida e aplica
- 7a. Pagamento dividido: cliente divide entre dois metodos de pagamento
- 9a. Pecas consignadas: sistema regista venda para calculo de comissao
- 9b. Pecas de compra propria: lucro total e da loja (sem comissao)

**Regras de Negocio:**
- RN-14: Descontos acima de X% exigem autorizacao do gestor
- RN-15: Cada peca consignada vendida deve ser rastreada para acerto com fornecedor (gera credito PorcInLoja + PorcInDinheiro)
- RN-16: Pecas de compra propria nao geram comissao
- RN-16a: Ao usar Credito em Loja, operador deve identificar o fornecedor para debitar do saldo correto

---

### CU-17: Processar Troca/Devolucao [TODO] - P1

**Ator Principal:** Operador de Caixa
**Pre-condicoes:** Venda original no sistema, dentro do periodo de devolucao

**Fluxo Principal:**
1. Cliente apresenta recibo/comprovativo de compra
2. Operador encontra venda no sistema
3. Sistema apresenta pecas da venda
4. Operador seleciona peca para troca/devolucao
5. Cliente escolhe:
   - Trocar por outra peca
   - Credito em loja
6. Se troca: operador cria nova venda com valor original como desconto
7. Se credito em loja: sistema gera vale de credito
8. Sistema devolve peca ao inventario (se em condicao vendavel)

**Regras de Negocio:**
- RN-17: Periodo de troca: 7 dias para pecas com defeito, 30 dias para credito em loja
- RN-18: Pecas consignadas devolvidas regressam ao estado "A Venda"
- RN-19: Valor de troca nao pode exceder preco de venda original sem pagamento adicional
- RN-20: Nao ha reembolsos em dinheiro - apenas troca ou credito em loja

---

### CU-18: Fechar Caixa [TODO] - P0

**Ator Principal:** Operador de Caixa
**Pre-condicoes:** Caixa aberta

**Fluxo Principal:**
1. Operador seleciona "Fechar Caixa"
2. Sistema apresenta resumo:
   - Numero de vendas
   - Total por metodo de pagamento (dinheiro, cartao, MBWAY, credito)
   - Montante esperado em dinheiro
3. Operador conta dinheiro fisico e introduz montante
4. Sistema compara montante contado vs. esperado
5. Se existir discrepancia, operador fornece justificacao
6. Sistema gera relatorio de fecho
7. Gestor aprova fecho
8. Sistema bloqueia caixa para novas vendas

---

## MODULO 4: RELATORIOS E BUSINESS INTELLIGENCE

### CU-19: Relatorio de Vendas [TODO] - P1

**Ator Principal:** Gestor/Proprietario

**Fluxo Principal:**
1. Utilizador acede ao modulo de relatorios
2. Utilizador seleciona filtros: periodo, loja, categoria, vendedor
3. Sistema gera relatorio:
   - Receita total
   - Valor medio de ticket
   - Pecas mais vendidas por categoria/marca
   - Desagregacao por metodo de pagamento (dinheiro, cartao, MBWAY, credito)
   - Comparacao com periodo anterior
   - Receita de pecas consignadas vs. pecas de compra propria
4. Utilizador pode exportar como PDF/Excel ou ver em dashboard

---

### CU-20: Analise de Rotacao de Inventario [TODO] - P1

**Ator Principal:** Gestor

**Fluxo Principal:**
1. Gestor acede a analitica de inventario
2. Sistema apresenta:
   - Tempo medio em stock por categoria
   - Pecas estagnadas ha mais de X dias
   - Taxa de rotacao de inventario
   - Grafico de envelhecimento do stock
   - Desagregacao por origem (consignacao vs. compra propria)
3. Sistema sugere acoes:
   - Precos promocionais para pecas estagnadas
   - Devolucao ao fornecedor para consignacoes expiradas
4. Gestor pode gerar listas de acao

---

### CU-21: Dashboard Executivo [TODO] - P1

**Ator Principal:** Proprietario

**Fluxo Principal:**
1. Utilizador acede ao dashboard
2. Sistema apresenta KPIs em tempo real:
   - Receita diaria/mensal
   - Valor atual do inventario
   - Comissoes pendentes a fornecedores
   - Vendas por canal (loja fisica, online)
   - Comparacao periodo-a-periodo
   - Top 5 categorias e marcas
   - Taxa de entrada de consignacoes
   - Metricas de aquisicao de clientes
   - Margem de lucro (consignacao vs. compra propria)
   - Pecas pendentes de avaliacao (recebidas mas nao avaliadas)

---

## MODULO 5: INTEGRACAO OMNICANAL

### CU-22: Sincronizar Inventario com E-commerce [TODO] - P2

**Ator Principal:** Sistema (automatizado)

**Fluxo Principal:**
1. Sistema monitoriza alteracoes no inventario (nova peca, venda, alteracao de preco)
2. Sistema sincroniza automaticamente com plataforma de e-commerce
3. Se peca vendida online, sistema remove do inventario fisico
4. Sistema notifica funcionarios para embalagem e envio

---

### CU-23: Publicar Peca em Marketplace [TODO] - P2

**Ator Principal:** Operador de E-commerce

**Fluxo Principal:**
1. Operador seleciona pecas para publicacao no marketplace
2. Sistema valida fotos e descricoes (conforme padroes do marketplace)
3. Operador ajusta descricao/fotos se necessario
4. Sistema publica nos marketplaces integrados (Vinted, Instagram)
5. Sistema monitoriza consultas e mensagens
6. Quando vendida, sistema atualiza inventario local

**Nota:** A publicacao online e desejada logo apos a avaliacao (Etapa 2), potencialmente antes da confirmacao formal do cliente.

---

## MODULO 6: CLIENTE E FIDELIZACAO

### CU-24: Registar Cliente Comprador [TODO] - P2

**Ator Principal:** Operador de Caixa

**Fluxo Principal:**
1. Cliente aceita aderir ao programa de fidelizacao
2. Operador regista: nome, NIF (opcional), telefone, email, data de nascimento
3. Sistema gera cartao de fidelizacao (fisico ou digital)
4. Sistema regista preferencias (tamanhos, marcas favoritas)

---

### CU-25: Acumular Pontos de Fidelizacao [TODO] - P2

**Ator Principal:** Sistema (automatizado)

**Fluxo Principal:**
1. Ao concluir venda, sistema identifica cliente
2. Sistema calcula pontos (ex: 1 ponto por cada 10,00 EUR gastos)
3. Sistema credita pontos na conta do cliente
4. Sistema notifica cliente via email/SMS sobre pontos acumulados
5. Pontos podem ser trocados por descontos em compras futuras

---

## MODULO 7: ADMINISTRACAO E CONFIGURACAO

### CU-26: Gerir Utilizadores e Permissoes [TODO] - P0

**Ator Principal:** Administrador

**Fluxo Principal:**
1. Admin cria utilizador (funcionario)
2. Admin define perfil: Operador de Caixa, Gestor, Financeiro, Admin
3. Sistema atribui permissoes por perfil:
   - **Operador de Caixa:** Operacoes POS, visualizacao basica de inventario
   - **Gestor:** Inventario completo, precos, relatorios, POS, avaliacoes, rececoes
   - **Financeiro:** Acertos, pagamentos, relatorios financeiros
   - **Admin:** Acesso total, gestao de utilizadores
4. Admin pode personalizar permissoes especificas por utilizador

**Implementacao Atual:**
- Firebase Authentication existe
- Permissoes baseadas em perfis: AINDA NAO implementado (todos os utilizadores tem o mesmo acesso)

---

### CU-27: Configurar Parametros do Sistema [TODO] - P1

**Ator Principal:** Administrador

**Fluxo Principal:**
1. Admin acede a configuracao do sistema
2. Admin pode definir:
   - Periodo de consignacao padrao (dias)
   - PorcInLoja e PorcInDinheiro padrao para novos fornecedores
   - Desconto maximo sem autorizacao
   - Taxa de conversao de pontos de fidelizacao
   - Informacao da loja (nome, morada, NIF)
   - Modelos de recibos e faturas
   - Configuracoes de email (SMTP para envio de avaliacoes)
   - Integracao WhatsApp (para comunicacao de acertos)
3. Sistema guarda configuracao
4. Alteracoes entram em vigor imediatamente

---

## RESUMO DE STATUS DO LIFECYCLE

```
Recebido --> Avaliado (AV) --> A Venda (DL) --> Vendido (VD) --> Pago (PG)
                  |                                |
             Com Defeito (CD)                 Devolvido (DV)
                  |
             Devolvido (DV)
```

| Codigo | Estado | Descricao | Quando |
|--------|--------|-----------|--------|
| -- | Recebido | Peca recebida, contada, aguardando avaliacao | Etapa 1 (com cliente) |
| **AV** | Avaliado | Peca avaliada com preco definido | Etapa 2 (sem cliente) |
| **CD** | Com Defeito | Peca recusada por defeito, aguarda devolucao | Etapa 2 |
| **DL** | A Venda | Peca exposta para venda na loja/online | Etapa 4 |
| **VD** | Vendido | Peca vendida a um comprador | Etapa 5 |
| **PG** | Pago | Acerto feito com o fornecedor | Etapa 6 |
| **DV** | Devolvido | Peca devolvida ao fornecedor | Qualquer etapa |

---

## RESUMO DE COMISSOES (Oui Circular - Portugal)

**Modelo de Credito do Fornecedor:** Quando um item e vendido, o fornecedor acumula credito que pode ser usado de duas formas:

| Percentagem (por fornecedor) | Destino | Exemplo (Total 40 EUR, PorcInLoja=50%, PorcInDinheiro=40%) |
|-----------------------------|---------|------------------------------------------------------------|
| **PorcInLoja** | Credito para compras na propria loja | 40 x 0,50 = **20 EUR** de credito em loja |
| **PorcInDinheiro** | Resgate em numerario | 40 x 0,40 = **16 EUR** resgataveis em dinheiro |
| Restante | Loja | 4 EUR (10%) |

**Nota:** PorcInLoja e PorcInDinheiro variam por fornecedor. O fornecedor pode usar o credito em loja em compras OU resgatar em dinheiro (ate ao valor PorcInDinheiro acumulado).

---

## METODOS DE PAGAMENTO ACEITES

| Metodo | Descricao |
|--------|-----------|
| Dinheiro | Pagamento em numerario (EUR) |
| Cartao de Credito | Via terminal POS |
| Cartao de Debito | Via terminal POS |
| MBWAY | Pagamento instantaneo via telemovel |
| Credito em Loja | Saldo de credito do fornecedor (PorcInLoja). Operador identifica o fornecedor para debitar do seu credito |

---

## MODELO MISTO DE AQUISICAO

A Oui Circular opera com dois tipos de entrada de pecas:

| Tipo | Descricao | Comissao | Exemplos |
|------|-----------|----------|----------|
| **Consignacao** | Pecas de fornecedores/clientes | PorcInLoja (credito) + PorcInDinheiro (resgate) por fornecedor | Clientes que trazem roupas |
| **Compra Propria** | Pecas compradas pela loja | Sem comissao (lucro total da loja) | Humana, Vinted, H&M, acervo pessoal |
