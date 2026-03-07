#!/usr/bin/env bash
# setup-vps.sh
# Script de setup inicial da VPS Ubuntu para o OUI System.
# Executar UMA VEZ como root ou utilizador com sudo.
#
# Uso:
#   chmod +x setup-vps.sh
#   sudo ./setup-vps.sh

set -euo pipefail

DEPLOY_USER="deploy"
APP_DIR="/opt/oui-system"
GITHUB_USER="ALTERAR"   # ← substituir pelo teu utilizador GitHub

echo "════════════════════════════════════════"
echo " OUI System — Setup VPS"
echo "════════════════════════════════════════"

# ── 1. Atualizar sistema ──────────────────────────────────────────────────────
echo "[1/7] A atualizar pacotes..."
apt-get update -qq && apt-get upgrade -y -qq

# ── 2. Instalar Docker ────────────────────────────────────────────────────────
echo "[2/7] A instalar Docker..."
if ! command -v docker &>/dev/null; then
    curl -fsSL https://get.docker.com | sh
fi

# ── 3. Instalar Caddy ─────────────────────────────────────────────────────────
echo "[3/7] A instalar Caddy..."
if ! command -v caddy &>/dev/null; then
    apt-get install -y -qq debian-keyring debian-archive-keyring apt-transport-https curl
    curl -1sLf 'https://dl.cloudsmith.io/public/caddy/stable/gpg.key' \
        | gpg --dearmor -o /usr/share/keyrings/caddy-stable-archive-keyring.gpg
    curl -1sLf 'https://dl.cloudsmith.io/public/caddy/stable/debian.deb.txt' \
        | tee /etc/apt/sources.list.d/caddy-stable.list
    apt-get update -qq && apt-get install -y caddy
fi

# ── 4. Criar utilizador de deploy ─────────────────────────────────────────────
echo "[4/7] A criar utilizador '$DEPLOY_USER'..."
if ! id "$DEPLOY_USER" &>/dev/null; then
    useradd --system --create-home --shell /bin/bash "$DEPLOY_USER"
fi
usermod -aG docker "$DEPLOY_USER"

# ── 5. Criar diretório da aplicação ──────────────────────────────────────────
echo "[5/7] A criar diretório $APP_DIR..."
mkdir -p "$APP_DIR"
chown "$DEPLOY_USER:$DEPLOY_USER" "$APP_DIR"

# ── 6. Copiar ficheiros de deploy ─────────────────────────────────────────────
echo "[6/7] A copiar docker-compose.yml e .env.example..."
# Substituir GITHUB_USER no docker-compose.yml
sed "s/GITHUB_USER/$GITHUB_USER/g" "$(dirname "$0")/docker-compose.yml" \
    > "$APP_DIR/docker-compose.yml"

if [ ! -f "$APP_DIR/.env" ]; then
    cp "$(dirname "$0")/.env.example" "$APP_DIR/.env"
    echo ""
    echo "  ⚠  Editar $APP_DIR/.env com os valores reais antes do primeiro deploy!"
fi

chown -R "$DEPLOY_USER:$DEPLOY_USER" "$APP_DIR"

# ── 7. Chave SSH para GitHub Actions ─────────────────────────────────────────
echo "[7/7] A gerar chave SSH para deploy..."
SSH_DIR="/home/$DEPLOY_USER/.ssh"
mkdir -p "$SSH_DIR"
chmod 700 "$SSH_DIR"

if [ ! -f "$SSH_DIR/deploy_key" ]; then
    ssh-keygen -t ed25519 -C "github-actions-deploy" -f "$SSH_DIR/deploy_key" -N ""
    cat "$SSH_DIR/deploy_key.pub" >> "$SSH_DIR/authorized_keys"
    chmod 600 "$SSH_DIR/authorized_keys"
    chown -R "$DEPLOY_USER:$DEPLOY_USER" "$SSH_DIR"

    echo ""
    echo "════════════════════════════════════════"
    echo " Chave privada para o GitHub Secret VPS_SSH_KEY:"
    echo "════════════════════════════════════════"
    cat "$SSH_DIR/deploy_key"
    echo "════════════════════════════════════════"
    echo " Copiar o conteúdo acima para:"
    echo " GitHub → Settings → Secrets → Actions → VPS_SSH_KEY"
    echo "════════════════════════════════════════"
else
    echo " Chave SSH já existe. A saltar geração."
fi

echo ""
echo "════════════════════════════════════════"
echo " Setup concluído!"
echo ""
echo " Próximos passos:"
echo "  1. Editar $APP_DIR/.env com os valores reais"
echo "  2. Copiar deploy/Caddyfile para /etc/caddy/Caddyfile"
echo "     e substituir o domínio"
echo "  3. sudo systemctl reload caddy"
echo "  4. Configurar os Secrets no GitHub (ver docs/14-DEPLOY-PIPELINE.md)"
echo "  5. Executar o primeiro deploy manualmente no GitHub Actions"
echo "════════════════════════════════════════"
