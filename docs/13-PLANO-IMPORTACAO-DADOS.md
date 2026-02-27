# Plano e Guia de Importação de Dados

Este documento descreve o plano e o guia para importar os dados dos arquivos Excel existentes para a base de dados do projeto OUI System.

---

## 1. Visão Geral dos Arquivos Fonte

### 1.1 `_Oui estoque atual.xlsx`
- **Propósito:** Inventário consolidado de peças em estoque
- **Estrutura:** 1 sheet ("Página1"), ~1952 linhas
- **Colunas:**

| Coluna Excel | Descrição | Mapeamento DB |
|--------------|-----------|---------------|
| Ref Peça | Código de referência | `Item.IdentificationNumber` |
| Localização | Local físico no armazém | *(não mapeado - campo futuro)* |
| Origem | Fornecedor ou tipo (Ac. pessoal, Acp Lilly, etc.) | `Supplier` ou `AcquisitionType` |
| Situação | Estado da peça | `Item.Status` |
| Descrição | Descrição da peça | `Item.Name` |
| Marca | Marca do produto | `Brand.Name` → `Item.BrandId` |
| Condição | Estado de conservação | `Item.Condition` |
| Tam | Tamanho | `Item.Size` |
| Cor | Cor | `Item.Color` |
| Composição | Composição do tecido | `Item.Composition` |
| Data da aquisição | Data de entrada | *(Reception.ReceptionDate)* |
| Valor de referência | Valor de referência | *(opcional)* |
| Valor de Compra | Custo de compra | `Item.CostPrice` |
| Valor sugerido | Preço avaliado | `Item.EvaluatedPrice` |
| Valor venda | Preço final de venda | `Item.FinalSalePrice` |

### 1.2 `Consignados - Clientes.xlsx`
- **Propósito:** Peças consignadas por cliente/fornecedor
- **Estrutura:** 62 sheets, 1 por cliente (ex.: "Adriany (Malu)", "Andressa", "Ana Chaves")
- **Colunas (linha 2 em cada sheet):**

| Coluna Excel | Descrição | Mapeamento DB |
|--------------|-----------|---------------|
| COD / Cod. | Código da peça | `Item.IdentificationNumber` |
| Descrição da peça | Descrição | `Item.Name` |
| data de recepção | Data de recepção | `Reception.ReceptionDate` |
| valor avaliado | Preço avaliado | `Item.EvaluatedPrice` |
| situação | Estado (DL, DV, VD, PG, Devolvido) | `Item.Status` |
| Crédito em dinheiro (40%) | Valor em dinheiro | *(Settlement)* |
| Crédito em loja (50%) | Valor em crédito loja | *(StoreCredit)* |

---

## 2. Mapeamento de Valores (Enums e Status)

### 2.1 Situação → ItemStatus
| Excel | ItemStatus |
|-------|------------|
| CS, PG | `Received` (1) ou `Evaluated` (2) |
| DL, Disponivel, Disponível | `ToSell` (4) |
| VD, Vendida, Vendido | `Sold` (5) |
| DV, Devolvido | `Returned` (6) |

### 2.2 Condição → ItemCondition
| Excel | ItemCondition |
|-------|---------------|
| excelente, EX, Excelente | `Excellent` (1) |
| muito bom, MB, Muito bom | `VeryGood` (2) |
| bom, B, Bom, bom estado | `Good` (3) |
| razoavel | `Fair` (4) |
| (outros) | `Good` (3) - default |

### 2.3 Origem → AcquisitionType e Supplier
- **"Ac. pessoal", "Ac. Pessoal", "Acp", "Acp Lilly"** → `AcquisitionType.OwnPurchase` (compra própria)
- **Nomes de pessoas** (Aline, Ana Regina, etc.) → `AcquisitionType.Consignment` + criar/linkar `Supplier`

### 2.4 Situação Consignados → ItemStatus
| Excel | ItemStatus |
|-------|------------|
| PG | `Evaluated` (2) ou `AwaitingAcceptance` (3) |
| DL | `ToSell` (4) |
| VD | `Sold` (5) |
| DV, Devolvido | `Returned` (6) |

---

## 3. Ordem de Importação (Dependências)

```
1. Brands (Marcas)           ← extrair únicos do estoque
2. Categories (opcional)     ← criar "Geral" ou mapear se houver
3. Suppliers (Fornecedores)  ← sheets do Consignados + Origens do estoque
4. Receptions (opcional)     ← agrupar por supplier + data
5. Items                    ← estoque + itens consignados
```

---

## 4. Plano de Execução

### Fase 1: Preparação
1. **Backup da base de dados** antes de qualquer importação
2. **Validar encoding** dos Excel (UTF-8 / Latin-1) para caracteres portugueses (ã, ç, é)
3. **Criar ambiente de teste** com base vazia para validar import

### Fase 2: Script de Importação
Criar uma **Console Application** ou **endpoint de import** que:

1. **Importar Marcas** (`Brands`)
   - Extrair valores únicos da coluna "Marca" do estoque
   - Normalizar: trim, lowercase para comparação, manter original no Name
   - Inserir com `ExternalId`, `CreatedOn`

2. **Importar Fornecedores** (`Suppliers`)
   - **Do Consignados:** nome do sheet = nome do fornecedor
   - Gerar `Initial` (primeiras letras, ex: "AC" para Ana Chaves)
   - Gerar `Email` e `PhoneNumber` placeholder (ex: `{initial}@consignado.placeholder`)
   - **Do Estoque:** extrair Origens que são nomes de pessoas e criar Suppliers

3. **Importar Itens do Estoque** (`Items`)
   - Para cada linha do estoque:
     - Resolver BrandId pelo nome da marca
     - Resolver SupplierId pela Origem (se consignado)
     - Mapear AcquisitionType (Consignment vs OwnPurchase)
     - Mapear Status, Condition, Size, Color
     - Gerar IdentificationNumber único (ex: `IMP-{Ref}` se Ref já existir)
     - `Origin` = Consignment ou OwnPurchase conforme Origem
     - `CommissionPercentage` = 50 (default consignação)

4. **Importar Itens dos Consignados** (`Items`)
   - Para cada sheet (Supplier):
     - Resolver SupplierId pelo nome do sheet
     - Para cada linha de peça (ignorar totais, linhas vazias):
       - Extrair descrição, valor avaliado, situação
       - Gerar COD se não existir (ex: `{Initial}{número}`)
       - Evitar duplicados: checar IdentificationNumber antes de inserir

### Fase 3: Tratamento de Conflitos
- **IdentificationNumber** deve ser único; definir estratégia para duplicados (sufixo `-2`, `-3`)
- **Marcas** com grafia similar (ex: "zara" vs "Zara") → normalizar e deduplicar
- **Fornecedores** com nomes similares (ex: "Malu" vs "Adriany (Malu)") → revisão manual ou regra de merge

### Fase 4: Pós-Importação
- Validar contagens (total de Items, Brands, Suppliers)
- Verificar itens órfãos (sem Brand, sem Supplier quando Consignment)
- Relatório de erros/linhas ignoradas

---

## 5. Guia de Implementação Técnica

### 5.1 Opção A: Console App (Recomendado)
```
src/
  shs.Import/
    Program.cs
    Services/
      ExcelEstoqueReader.cs
      ExcelConsignadosReader.cs
      ImportService.cs
```
- Referenciar `shs.Infrastructure`, `shs.Domain`
- Usar `ClosedXML` ou `EPPlus` para ler Excel em .NET
- Connection string via `appsettings.json` ou variável de ambiente

### 5.2 Opção B: Endpoint de Admin
- `POST /api/admin/import/estoque`
- `POST /api/admin/import/consignados`
- Upload do arquivo ou caminho configurável
- Proteger com permissão `admin.import`

### 5.3 Pacotes NuGet Sugeridos
- `ClosedXML` - leitura de Excel (MIT, sem dependências do Excel)
- `Npgsql` - já usado pelo projeto

### 5.4 Exemplo de Código (Pseudocódigo)
```csharp
// 1. Ler marcas únicas
var marcas = estoqueRows
    .Select(r => NormalizeBrand(r["Marca"]))
    .Where(x => !string.IsNullOrWhiteSpace(x))
    .Distinct();
foreach (var m in marcas)
    await db.Brands.AddAsync(new BrandEntity { Name = m, ... });

// 2. Criar suppliers dos sheets
foreach (var sheetName in consignadosWorkbook.Worksheets)
{
    var supplier = new SupplierEntity
    {
        Name = sheetName.Name,
        Initial = GenerateInitial(sheetName.Name),
        Email = $"{initial}@consignado.placeholder",
        PhoneNumber = "+351000000000"
    };
    await db.Suppliers.AddAsync(supplier);
}

// 3. Importar items em batches
foreach (var row in estoqueRows)
{
    var item = MapToItem(row, brandCache, supplierCache);
    await db.Items.AddAsync(item);
    if (batchCount++ >= 100) { await db.SaveChangesAsync(); batchCount = 0; }
}
```

---

## 6. Checklist Pré-Importação

- [ ] Backup da base de dados
- [ ] Validar que migrations estão aplicadas
- [ ] Testar em base de desenvolvimento primeiro
- [ ] Definir estratégia para `IdentificationNumber` duplicados
- [ ] Lista de Origens do estoque que são OwnPurchase vs Consignment
- [ ] Mapeamento manual de fornecedores duplicados (ex: "Malu" = "Adriany (Malu)")

---

## 7. Riscos e Mitigações

| Risco | Mitigação |
|-------|------------|
| Encoding incorreto (ã, ç) | Usar `Encoding.GetEncoding("ISO-8859-1")` ou verificar encoding do Excel |
| Valores numéricos como texto | `decimal.TryParse`, tratar "30/22" como 22 |
| Linhas de totais/cabeçalhos | Validar que coluna numérica existe antes de processar |
| Duplicados entre estoque e consignados | Prioridade: Consignados (mais detalhado) ou Estoque; definir regra |
| Sheets com estrutura diferente | Alguns sheets têm COD, outros não; normalizar no reader |

---

## 8. Próximos Passos

1. ~~Criar projeto `shs.Import` (Console App)~~ ✅
2. ~~Implementar `ExcelEstoqueReader` e `ExcelConsignadosReader`~~ ✅
3. ~~Implementar `ImportService` com mapeamentos~~ ✅
4. Executar import em ambiente de teste
5. Ajustar mapeamentos conforme resultados
6. Documentar decisões de merge/deduplicação tomadas

---

## 9. Uso do Console App (shs.Import)

### Execução

```bash
# Com confirmação interativa
dotnet run --project src/shs.Import/shs.Import.csproj

# Sem confirmação (útil para CI/scripts)
dotnet run --project src/shs.Import/shs.Import.csproj -- --yes
```

### Configuração

- **Connection string:** `appsettings.json` ou variável de ambiente
- **Caminhos dos arquivos:** `appsettings.json` → `Import:EstoquePath` e `Import:ConsignadosPath`
- Por padrão, os arquivos são buscados em `docs/` na raiz do repositório

### Pré-requisitos

1. PostgreSQL em execução com a base `oui_system`
2. Migrations aplicadas (`dotnet ef database update` ou via API)
3. Backup da base de dados antes da primeira importação
