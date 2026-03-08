# Oui Circular - Fluxo Real de Consignação

## Versão: 1.0 | Última Atualização: 2026-02-11
## Fonte: Entrevista com a dona da Oui Circular

---

## 1. Fluxo Real (Como Funciona Hoje)

```
┌─────────────────────────────────────────────────────────────────────┐
│                     FLUXO DE CONSIGNAÇÃO                            │
│                        Oui Circular                                 │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  ETAPA 1: RECEÇÃO DAS PEÇAS (no balcão, com o cliente presente)     │
│  ─────────────────────────────────────────────────────────────────  │
│                                                                     │
│  1. Cliente chega à loja com um saco/bolsa de roupas                │
│  2. Funcionário faz CONTAGEM das peças (sem avaliar)                │
│  3. Se cliente novo → Cadastrar (nome, email, telefone)             │
│  4. Loja emite RECIBO DE RECEÇÃO:                                   │
│     • Nome do cliente                                               │
│     • Data de receção                                               │
│     • Quantidade de peças recebidas                                 │
│     • (SEM valores — ainda não foram avaliadas)                     │
│  5. Cliente assina o recibo                                         │
│  6. Cliente vai embora                                              │
│                                                                     │
│  ⚠️ IMPORTANTE: A avaliação NÃO acontece na hora!                   │
│     O cliente não fica a esperar.                                   │
│                                                                     │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  ETAPA 2: AVALIAÇÃO (posteriormente, sem o cliente)                 │
│  ─────────────────────────────────────────────────────────────────  │
│                                                                     │
│  1. Funcionário pega as peças recebidas                             │
│  2. Para cada peça:                                                 │
│     a. Inspeciona condição (excelente, muito bom, bom, etc.)        │
│     b. Identifica marca, tamanho, cor, composição                   │
│     c. Define o valor de venda (preço avaliado)                     │
│     d. Regista no sistema com código de identificação               │
│  3. Peças com defeito ou sem condição → separadas para devolução    │
│  4. Peças aprovadas → status "Avaliado"                             │
│                                                                     │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  ETAPA 3: COMUNICAÇÃO E APROVAÇÃO DO CLIENTE                        │
│  ─────────────────────────────────────────────────────────────────  │
│                                                                     │
│  1. Ao concluir avaliação, sistema envia EMAIL automaticamente:     │
│     • Lista de peças aceites e respetivos valores de venda          │
│     • Peças recusadas (com defeito / sem condição) e motivo         │
│     • Link de aprovação (válido por 7 dias, sem login)              │
│  2. Peças ficam com status "Aguarda Aprovação"                      │
│  3. Cliente pode:                                                   │
│     a. Clicar no link → ver peças/preços → Aprovar ou Recusar      │
│     b. Confirmar via WhatsApp/telefone (staff aprova manualmente)   │
│  4. Após aprovação → peças passam a "À Venda"                      │
│                                                                     │
│  💡 O staff pode aprovar internamente sem aguardar pelo link,       │
│     bastando ter a confirmação do fornecedor por outro canal.       │
│                                                                     │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  ETAPA 4: PEÇAS DISPONÍVEIS PARA VENDA                              │
│  ─────────────────────────────────────────────────────────────────  │
│                                                                     │
│  1. Peças aceites → status "À Venda" / "Disponível em Loja"        │
│  2. Etiqueta com preço colocada na peça                             │
│  3. Peça exposta na loja física e/ou publicada online               │
│                                                                     │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  ETAPA 5: VENDA                                                     │
│  ─────────────────────────────────────────────────────────────────  │
│                                                                     │
│  1. Cliente comprador leva peça ao caixa                            │
│  2. Venda processada normalmente                                    │
│  3. Peça → status "Vendido" (VD)                                    │
│  4. Crédito gerado para o fornecedor consignante                    │
│                                                                     │
├─────────────────────────────────────────────────────────────────────┤
│                                                                     │
│  ETAPA 6: ACERTO / CRÉDITO AO FORNECEDOR                            │
│  ─────────────────────────────────────────────────────────────────  │
│                                                                     │
│  1. Por cada peça vendida, fornecedor acumula (PorcInLoja + PorcInDinheiro): │
│     • Crédito em loja (ex: 50%) - para compras na própria loja      │
│     • Valor resgatável em dinheiro (ex: 40%) - pode levantar         │
│  2. Crédito em loja: usado em compras (operador identifica fornecedor) │
│  3. Resgate em dinheiro: fornecedor solicita e loja regista         │
│  4. Status da peça → "PG" (Pago) após acerto                        │
│                                                                     │
│  Comunicação de acertos enviada via WhatsApp                        │
│                                                                     │
└─────────────────────────────────────────────────────────────────────┘
```

---

## 2. Status Reais Usados na Oui Circular

| Código | Significado | Descrição | Quando |
|--------|------------|-----------|--------|
| **—** | Recebido (sem código) | Peça recebida, contada, aguardando avaliação | Etapa 1 |
| **AV** | Avaliado | Peça avaliada com preço definido | Etapa 2 |
| **CD** | Com Defeito | Peça recusada por defeito, aguarda devolução | Etapa 2 |
| **CS** | Consignado | Peça aceite, em stock na loja | Etapa 4 |
| **DL** | Disponível em Loja | Peça exposta para venda | Etapa 4 |
| **VD** | Vendido | Peça vendida a um comprador | Etapa 5 |
| **PG** | Pago | Acerto feito com o fornecedor | Etapa 6 |
| **DV** | Devolvido | Peça devolvida ao fornecedor (não vendida ou defeito) | Qualquer etapa |

### Lifecycle Atualizado

```
Recebido ──► Avaliado ──► Aguarda Aprovação ──► Consignado/DL ──► Vendido ──► Pago
                │               │                                    │
                ▼               ▼                                    ▼
           Com Defeito     Devolvido                             Devolvido
                │
                ▼
           Devolvido
```

> **Nota:** Após a avaliação, os itens ficam no estado "Aguarda Aprovação" (AwaitingAcceptance).
> O fornecedor recebe um email com um link para aprovar os preços avaliados.
> Alternativamente, o staff pode aprovar manualmente caso o fornecedor confirme por WhatsApp/telefone.
> Só após a aprovação é que os itens passam a "À Venda" (ToSell).

---

## 3. Sistema de Créditos do Fornecedor (Oui Circular)

Quando um item de um fornecedor é vendido, o fornecedor acumula crédito na loja. Este crédito pode ser resgatado de duas formas:

| Percentagem (por fornecedor) | Destino | Exemplo (Total 40 EUR) |
|-----------------------------|---------|------------------------|
| **PorcInLoja** (ex: 50%) | Crédito para compras na própria loja | 40 × 0,50 = **20 EUR** |
| **PorcInDinheiro** (ex: 40%) | Resgate em numerário | 40 × 0,40 = **16 EUR** |
| Restante | Loja | 4 EUR (10%) |

### Exemplo (Calça 20€ + Camisa 20€ = 40€ total):
- **Crédito em loja:** 40 × 0,50 = €20,00 (para comprar itens na loja)
- **Resgate em dinheiro:** 40 × 0,40 = €16,00 (pode levantar em numerário)

### Nota:
- PorcInLoja e PorcInDinheiro variam por fornecedor
- O fornecedor acumula AMBOS quando os itens são vendidos
- Numa nova venda, o operador deve identificar o fornecedor para utilizar o seu crédito em loja
- É necessário registar tanto o **uso do crédito em compra** como o **resgate em dinheiro**

---

## 4. Diferenças vs. Documentação Anterior

| Aspeto | Documentação Anterior | Realidade Oui Circular |
|--------|----------------------|----------------------|
| **País** | Brasil (R$, CPF, CNPJ, NF-e) | Portugal (€, NIF, faturação PT) |
| **Moeda** | Real (R$) | Euro (€) |
| **Idioma UI** | Português Brasil (pt-BR) | Português Portugal (pt-PT) |
| **Avaliação** | Feita na hora com o cliente | Feita DEPOIS, sem o cliente |
| **Receção** | Contagem + avaliação juntas | Apenas contagem → recibo assinado |
| **Comunicação** | Não especificada | Email com valores + WhatsApp para acertos |
| **Comissão** | Escolha: 40% cash OU 50% crédito | PorcInLoja + PorcInDinheiro (ambos acumulam) |
| **Fiscal** | NF-e, SEFAZ, ICMS | Legislação fiscal portuguesa |
| **Status "Recebido"** | Não existia | Novo status necessário (pré-avaliação) |
| **Status "PG" (Pago)** | Não existia explicitamente | Status importante no fluxo real |
| **Recibo de receção** | Contrato de consignação completo | Recibo simples: "recebemos X peças" |
| **Publicação online** | Módulo futuro P2 | Já desejado logo após avaliação |

---

## 5. Documentos Gerados no Fluxo

### 5.1 Recibo de Receção (Etapa 1)
Gerado quando o cliente entrega as peças.

```
╔══════════════════════════════════════════╗
║         OUI CIRCULAR                     ║
║         Moda Circular & Sustentável      ║
╠══════════════════════════════════════════╣
║                                          ║
║  RECIBO DE RECEÇÃO DE PEÇAS              ║
║                                          ║
║  Data: 11/02/2026                        ║
║  Nº: REC-2026-0045                       ║
║                                          ║
║  Cliente: Ana Chaves                     ║
║  Email: ana@email.pt                     ║
║  Telefone: +351 912 345 678              ║
║                                          ║
║  Quantidade de peças recebidas: 12       ║
║                                          ║
║  As peças serão avaliadas e o cliente    ║
║  será informado dos valores por email.   ║
║                                          ║
║  Créditos (variam por fornecedor):       ║
║  • PorcInLoja: % em crédito para compras ║
║  • PorcInDinheiro: % resgatável em cash  ║
║                                          ║
║  Assinatura do cliente:                  ║
║  _____________________________           ║
║                                          ║
║  Assinatura da loja:                     ║
║  _____________________________           ║
║                                          ║
╚══════════════════════════════════════════╝
```

### 5.2 Email de Avaliação (Etapa 3)

```
Assunto: Oui Circular - Avaliação das suas peças

Olá Ana,

Concluímos a avaliação das 12 peças que nos entregou em 11/02/2026.

PEÇAS ACEITES (10):
┌─────┬──────────────────────────┬──────────┬───────┐
│ Cod │ Descrição                │ Condição │ Valor │
├─────┼──────────────────────────┼──────────┼───────┤
│AC150│ Vestido midi estampado   │ Excelente│ €18,00│
│AC151│ Blusa seda branca        │ Muito Bom│ €12,00│
│AC152│ Calça ganga slim         │ Bom      │  €8,00│
│ ... │ ...                      │ ...      │ ...   │
├─────┼──────────────────────────┼──────────┼───────┤
│     │ TOTAL                    │          │€95,00 │
└─────┴──────────────────────────┴──────────┴───────┘

PEÇAS RECUSADAS (2):
- Camisola verde: mancha permanente
- Saia preta: fecho partido

As peças recusadas ficam disponíveis para levantamento na loja.

Caso as peças sejam vendidas, receberá (exemplo PorcInLoja=50%, PorcInDinheiro=40%):
• Crédito em loja: €47,50 (50% do total)
• Resgate em dinheiro: €38,00 (40% do total)

Alguma questão, não hesite em contactar-nos.

Com os melhores cumprimentos,
Oui Circular
```

---

## 6. Dados Reais da Oui Circular (Números)

| Métrica | Valor |
|---------|-------|
| **Clientes consignantes** | 62 |
| **Total de peças em estoque** | ~519 |
| **Peças consignadas (total histórico)** | ~635 |
| **Preço médio de venda** | €12,55 |
| **Faixa de preço** | €2 - €100 |
| **Marcas mais comuns** | Zara, H&M, Mango, Stradivarius |
| **Condições mais comuns** | Excelente, Muito Bom, Bom |
| **Origens de compra** | Humana, Vinted, H&M, acervo pessoal |
| **Canais de venda** | Loja física, Instagram, Vinted |
| **Comunicação com clientes** | Email (avaliações), WhatsApp (acertos) |

---

## 7. Impacto no Sistema — Ajustes Necessários

### 7.1 Novo Status: "Recebido" (Received)
O sistema precisa de um status **antes** de "Avaliado" para representar peças que foram contadas mas ainda não avaliadas.

```
Recebido (novo!) → Avaliado → À Venda → Vendido → Pago
```

### 7.2 Recibo de Receção
O sistema precisa gerar um recibo simples (PDF) no momento da receção, com:
- Apenas a contagem de peças (sem valores)
- Campo para assinatura do cliente
- Termos de comissão

### 7.3 Email Automático de Avaliação
Após avaliar todas as peças de uma receção, o sistema deve:
- Gerar email com lista de peças aceites + valores
- Listar peças recusadas com motivo
- Enviar automaticamente ao cliente

### 7.4 Modelo Misto de Aquisição
O sistema precisa suportar dois tipos de entrada de peças:
1. **Consignação** — peças de fornecedores (com comissão)
2. **Compra própria** — peças compradas pela loja (Humana, Vinted, etc.)

### 7.5 Localização Portugal
- Moeda: Euro (€)
- Formato de data: dd/mm/yyyy
- Telefone: +351 XXX XXX XXX
- Documento fiscal: NIF (não CPF/CNPJ)
- Legislação: portuguesa/europeia
- Idioma da UI: pt-PT

### 7.6 Publicação Online Imediata
Após avaliação, opção de publicar automaticamente em:
- Instagram (foto + descrição)
- Vinted (marketplace)
- Site próprio (futuro e-commerce)
