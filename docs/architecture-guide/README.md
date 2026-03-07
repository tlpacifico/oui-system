# Guia Arquitetural — Modular Monolith com .NET

Documentacao de referencia baseada no projeto Evently para replicar a arquitetura em novos projetos.

## Arquivos neste guia

| Arquivo | Conteudo |
|---------|----------|
| [01-visao-geral.md](01-visao-geral.md) | Estrutura da solution, camadas e principios |
| [02-common-domain.md](02-common-domain.md) | Building blocks do dominio (Entity, Result, DomainEvent) |
| [03-common-application.md](03-common-application.md) | CQRS, Pipeline Behaviors, interfaces de abstraçao |
| [04-common-infrastructure.md](04-common-infrastructure.md) | Outbox/Inbox, Auth, Cache, EventBus, Serialization |
| [05-common-presentation.md](05-common-presentation.md) | Endpoints, ApiResults, mapeamento de erros HTTP |
| [06-modulo-template.md](06-modulo-template.md) | Como criar um novo modulo do zero |
| [07-docker-e-infra.md](07-docker-e-infra.md) | Docker Compose, Health Checks, Observabilidade |
| [08-testes.md](08-testes.md) | Estrategia de testes: Unit, Architecture, Integration |
| [09-checklist-novo-projeto.md](09-checklist-novo-projeto.md) | Checklist passo a passo para iniciar um novo projeto |
