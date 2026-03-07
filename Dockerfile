# ─────────────────────────────────────────────
# Stage 1 — Build Angular
# ─────────────────────────────────────────────
FROM node:22-alpine AS frontend

WORKDIR /frontend

COPY src/shs.Api/angular-client/package*.json ./
RUN npm ci --prefer-offline

COPY src/shs.Api/angular-client/ ./

# defaultConfiguration no angular.json já é "production"
RUN npm run build

# ─────────────────────────────────────────────
# Stage 2 — Build e Publish .NET
# ─────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS backend

WORKDIR /app

# Copiar solução e csproj primeiro para aproveitar cache de layers
COPY shs.sln ./
COPY src/shs.Domain/shs.Domain.csproj             src/shs.Domain/
COPY src/shs.Application/shs.Application.csproj   src/shs.Application/
COPY src/shs.Infrastructure/shs.Infrastructure.csproj src/shs.Infrastructure/
COPY src/shs.Api/shs.Api.csproj                   src/shs.Api/

RUN dotnet restore src/shs.Api/shs.Api.csproj

# Copiar código fonte
COPY src/ src/

# Copiar output Angular para wwwroot
# (UseStaticFiles + MapFallbackToFile servem a partir de wwwroot/)
COPY --from=frontend /frontend/dist/angular-client/browser/ src/shs.Api/wwwroot/

# Publicar — sem reconstruir Angular (MSBuild target ignorado porque
# o output já está em wwwroot antes do publish)
RUN dotnet publish src/shs.Api \
    --configuration Release \
    --no-restore \
    --output /publish \
    /p:SkipBuildAngularClient=true

# ─────────────────────────────────────────────
# Stage 3 — Runtime (imagem final ~300MB)
# ─────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final

WORKDIR /app

# Utilizador não-root por segurança
RUN addgroup --system appgroup && adduser --system --ingroup appgroup appuser
USER appuser

COPY --from=backend --chown=appuser:appgroup /publish .

# Volume para uploads de fotos de items
VOLUME ["/app/wwwroot/uploads"]

EXPOSE 8080

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "shs.Api.dll"]
