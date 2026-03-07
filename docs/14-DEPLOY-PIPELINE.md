# 14 — Pipeline de Deploy (GitHub Actions + Docker + VPS)

## Visão Geral

O deploy do OUI System é feito manualmente via GitHub Actions. Ao fazer trigger no workflow, o GitHub constrói uma imagem Docker multi-stage (Angular + .NET), publica-a no GitHub Container Registry (ghcr.io), e faz deploy na VPS via SSH.

```
Repositório GitHub
  └── Actions: "Deploy OUI System" (trigger manual)
        ├── Job 1: Build & Push
        │     ├── ng build (Angular 20, production)
        │     ├── dotnet publish (Release)
        │     └── Push → ghcr.io/<user>/oui-system:latest
        └── Job 2: Deploy VPS
              ├── (opcional) Migrations EF Core
              ├── docker compose pull
              ├── docker compose up -d
              └── Verificar health
```

---

## Ficheiros Criados

| Ficheiro | Descrição |
|----------|-----------|
| `Dockerfile` | Build multi-stage: Node → SDK .NET → Runtime |
| `.github/workflows/deploy.yml` | Workflow GitHub Actions (trigger manual) |
| `deploy/docker-compose.yml` | Compose para a VPS |
| `deploy/.env.example` | Template de variáveis de ambiente |
| `deploy/Caddyfile` | Reverse proxy com HTTPS automático |
| `deploy/setup-vps.sh` | Script de setup inicial da VPS |

---

## Setup Inicial da VPS

### 1. Executar o script de setup

```bash
# Clonar ou copiar os ficheiros de deploy para a VPS
scp -r deploy/ user@VPS_IP:/tmp/oui-deploy/

# Na VPS
cd /tmp/oui-deploy
chmod +x setup-vps.sh
sudo ./setup-vps.sh
```

O script instala Docker e Caddy, cria o utilizador `deploy`, e gera a chave SSH para GitHub Actions.

### 2. Configurar o .env

```bash
sudo nano /opt/oui-system/.env
```

Preencher todos os valores (ver `.env.example`):
- `DB_CONNECTION_STRING` — PostgreSQL já instalado na VPS
- `FIREBASE_PROJECT_ID`
- Credenciais SMTP (Email)

### 3. Configurar Caddy

```bash
# Substituir o domínio no Caddyfile
sudo cp /tmp/oui-deploy/Caddyfile /etc/caddy/Caddyfile
sudo nano /etc/caddy/Caddyfile   # substituir oui.TEUDOMINIO.com

sudo systemctl reload caddy
```

Caddy trata HTTPS automaticamente via Let's Encrypt.

---

## Configurar GitHub Secrets

No repositório GitHub: **Settings → Secrets and variables → Actions**

| Secret | Descrição | Onde obter |
|--------|-----------|------------|
| `VPS_HOST` | IP ou domínio da VPS | Painel do hosting |
| `VPS_USER` | `deploy` | Criado pelo setup-vps.sh |
| `VPS_SSH_KEY` | Chave SSH privada | Output do setup-vps.sh |
| `DB_CONNECTION_STRING` | Connection string PostgreSQL | Configurado na VPS |
| `FIREBASE_PROJECT_ID` | ID do projeto Firebase | Firebase Console |

> A `GITHUB_TOKEN` para push no ghcr.io é gerada automaticamente pelo GitHub Actions — não é necessário configurar manualmente.

---

## Fazer Deploy

1. Ir ao repositório no GitHub
2. **Actions** → **Deploy OUI System** → **Run workflow**
3. Preencher os inputs:
   - **Aplicar migrations?** — marcar `true` apenas quando há migrations novas
   - **Tag** — deixar `latest` (usa sempre o build mais recente)
4. Clicar **Run workflow**

O workflow demora ~4-6 minutos (build Angular + .NET).

---

## Primeiro Deploy

No primeiro deploy, é necessário autenticar o Docker na VPS para conseguir fazer pull do ghcr.io:

```bash
# Na VPS, como utilizador deploy
docker login ghcr.io -u GITHUB_USER -p GITHUB_PAT
```

> `GITHUB_PAT` = Personal Access Token com scope `read:packages`.
> Após o login, as credenciais ficam guardadas em `~/.docker/config.json`.

---

## Rollback

```bash
# Na VPS — voltar para uma versão anterior pelo SHA
ssh deploy@VPS_IP
cd /opt/oui-system

# Listar imagens disponíveis
docker images ghcr.io/GITHUB_USER/oui-system

# Editar docker-compose.yml para usar a tag específica
# Ex: image: ghcr.io/GITHUB_USER/oui-system:abc12345

docker compose up -d --no-deps oui-system
```

---

## Estrutura do Dockerfile

```
Stage 1 (node:22-alpine)
  ├── npm ci
  └── ng build → dist/angular-client/browser/

Stage 2 (dotnet/sdk:9.0)
  ├── dotnet restore (cache separado)
  ├── Copia Angular → src/shs.Api/wwwroot/
  └── dotnet publish → /publish

Stage 3 (dotnet/aspnet:9.0) — imagem final ~300MB
  ├── Utilizador não-root (appuser)
  ├── Volume: /app/wwwroot/uploads
  └── Porta: 8080
```

### Porquê Angular em wwwroot?

O `Program.cs` serve o SPA via `UseStaticFiles()` + `MapFallbackToFile("index.html")`, ambos a partir de `wwwroot/`. Os ficheiros Angular são copiados para `wwwroot/` durante o build Docker, antes do `dotnet publish` incluir a pasta na imagem final.

---

## Monitorização

```bash
# Estado do container
docker compose -f /opt/oui-system/docker-compose.yml ps

# Logs em tempo real
docker compose -f /opt/oui-system/docker-compose.yml logs -f oui-system

# Últimas 100 linhas
docker compose -f /opt/oui-system/docker-compose.yml logs --tail=100 oui-system

# Uso de recursos
docker stats oui-system
```

---

## Migrations EF Core

As migrations são aplicadas automaticamente no startup da app (`db.Database.MigrateAsync()` em `Program.cs`). Em alternativa, podem ser aplicadas manualmente através do input `run_migrations: true` no workflow.

Para criar uma nova migration no desenvolvimento local:

```bash
dotnet ef migrations add <Nome> \
    --project src/shs.Infrastructure \
    --startup-project src/shs.Api
```

Commitar a migration antes de fazer deploy com `run_migrations: true`.
