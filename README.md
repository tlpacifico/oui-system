## OUI System - Backend & Frontend Base

Este repositório contém o ERP **OUI System** para lojas de moda circular, com backend em **.NET 9** e frontend em **Angular 20**.

### Estrutura atual

- `shs.sln` – solução .NET
- `src/shs.Api` – API ASP.NET Core (Minimal APIs)
- `src/shs.Application` – camada de aplicação (CQRS, validação, serviços)
- `src/shs.Domain` – entidades de domínio, enums, contratos
- `src/shs.Infrastructure` – EF Core, repositórios, DbContext
- `src/shs.Api/angular-client` – aplicação Angular 20 (SPA)

### Pré-requisitos

- .NET SDK 9 instalado
- Node.js 20+ e npm
- PostgreSQL em execução (para as próximas fases)

### Como rodar o backend (dev)

```bash
cd src/shs.Api
dotnet run
```

Por padrão a API sobe em `https://localhost:5001` (ajustar depois conforme configuração).

### Como rodar o frontend (dev)

```bash
cd src/shs.Api/angular-client
npm install
npm start  # ou: npm run start
```

Por padrão o Angular sobe em `http://localhost:4200`. Em fases posteriores será configurado um proxy para `/api`.

