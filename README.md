# 🛠️ MonkOrc (Mecanosoft) - Manual de Inicialização do Projeto

Este manual contém o passo a passo completo para configurar e rodar o projeto localmente logo após clonar o repositório (`git clone`).

---

## 📑 Sumário
1. [Visão Geral da Arquitetura](#-visão-geral-da-arquitetura)
2. [Pré-requisitos](#-pré-requisitos)
3. [Passo a Passo Rápido (TL;DR)](#-passo-a-passo-rápido-tldr)
4. [Passo a Passo Detalhado](#-passo-a-passo-detalhado)
   - [Passo 1: Banco de Dados (PostgreSQL)](#passo-1-banco-de-dados-postgresql)
   - [Passo 2: Backend (.NET 8 Web API)](#passo-2-backend-net-8-web-api)
   - [Passo 3: Frontend (Angular + Tailwind CSS)](#passo-3-frontend-angular--tailwind-css)
5. [Portas e URLs Importantes](#-portas-e-urls-importantes)
6. [Resolução de Problemas Comuns (Troubleshooting)](#-resolução-de-problemas-comuns-troubleshooting)

---

## 🏗️ Visão Geral da Arquitetura

O sistema é dividido em duas partes principais:
- **Backend**: API REST desenvolvida em **.NET 8 (C#)** com **Entity Framework Core** e banco de dados **PostgreSQL** (arquitetura Multi-Tenant com autenticação JWT).
- **Frontend**: Aplicação Web / PWA desenvolvida em **Angular** com **Tailwind CSS v4** e **Angular Material**.

Estrutura de pastas principal:
```text
mecanosoft/
├── backend/
│   └── MonkOrc.Api/              # Projeto Web API .NET 8
├── frontend/                     # Projeto Angular SPA / PWA
├── docker-compose.yml            # Orquestração rápida do banco PostgreSQL
└── README.md                     # Este manual de inicialização
```

---

## 💻 Pré-requisitos

Antes de iniciar, certifique-se de ter instalado em sua máquina:

1. **[.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)**
   - Teste no terminal com: `dotnet --version` (deve retornar `8.0.x`)
2. **[Node.js](https://nodejs.org/)** (Versão LTS recomendada: v18 ou v20+)
   - Teste no terminal com: `node -v` e `npm -v`
3. **Banco de Dados PostgreSQL**:
   - **Opção recomendada**: [Docker Desktop](https://www.docker.com/products/docker-desktop/) (permite subir o banco com 1 comando).
   - **Ou**: [PostgreSQL](https://www.postgresql.org/download/) instalado localmente na porta padrão `5432`.
4. *(Opcional)* **CLI do Entity Framework**:
   ```bash
   dotnet tool install --global dotnet-ef
   ```
5. *(Opcional)* **CLI do Angular**:
   ```bash
   npm install -g @angular/cli
   ```

---

## ⚡ Passo a Passo Rápido (TL;DR)

Se você já possui as ferramentas instaladas e o Docker ativo, abra 3 abas no seu terminal:

```bash
# Aba 1: Subir o Banco de Dados
docker compose up -d

# Aba 2: Subir o Backend (.NET)
cd backend/MonkOrc.Api
dotnet run

# Aba 3: Subir o Frontend (Angular)
cd frontend
npm install
npm start
```

Acesse:
- **Frontend:** [http://localhost:4200](http://localhost:4200)
- **Swagger API:** [http://localhost:5135/swagger](http://localhost:5135/swagger) ou [https://localhost:7021/swagger](https://localhost:7021/swagger)

---

## 📖 Passo a Passo Detalhado

### Passo 1: Banco de Dados (PostgreSQL)

O backend depende de um banco de dados PostgreSQL rodando na porta `5432`.

#### Opção A: Via Docker (Mais fácil)
Na raiz do projeto (`mecanosoft`), execute:
```bash
docker compose up -d
```
> O container `monkorc_postgres` será iniciado automaticamente com a senha `Lap!nh@1701` e a base `DB01_monkorc` já configuradas para bater com as credenciais padrão do backend.

#### Opção B: PostgreSQL Instalado Localmente
Se você usa o PostgreSQL instalado no seu Windows:
1. Abra o **pgAdmin** ou o terminal `psql`.
2. Certifique-se de que o serviço do PostgreSQL está rodando na porta `5432`.
3. Verifique se o usuário `postgres` e sua respectiva senha correspondem ao configurado no arquivo `backend/MonkOrc.Api/appsettings.json`. Se sua senha local for diferente, altere no `appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Host=localhost;Port=5432;Database=DB01_monkorc;Username=postgres;Password=SUA_SENHA_AQUI"
   }
   ```

---

### Passo 2: Backend (.NET 8 Web API)

1. Navegue até a pasta da API:
   ```bash
   cd backend/MonkOrc.Api
   ```

2. Restaure as dependências do projeto:
   ```bash
   dotnet restore
   ```

3. *(Opcional)* Executar migrações do banco manualmente:
   > ℹ️ **Nota:** O `Program.cs` deste projeto já executa `db.Database.MigrateAsync()` automaticamente no startup. No entanto, se preferir rodar manualmente via terminal, use:
   ```bash
   dotnet ef database update
   ```

4. Inicie o servidor da API:
   ```bash
   dotnet run
   ```
   > Se preferir reinicialização automática a cada alteração de código, use: `dotnet watch run`

5. Verifique se a API está funcionando abrindo no navegador:
   - Swagger UI: [https://localhost:7021/swagger](https://localhost:7021/swagger) ou [http://localhost:5135/swagger](http://localhost:5135/swagger)

---

### Passo 3: Frontend (Angular + Tailwind CSS)

1. Em um novo terminal, navegue até a pasta do frontend:
   ```bash
   cd frontend
   ```

2. Instale os pacotes npm:
   ```bash
   npm install
   ```
   *(Caso ocorra algum conflito de peer dependencies no Angular 21, use: `npm install --legacy-peer-deps`)*

3. **Verifique a URL da API**:
   Abra o arquivo `frontend/src/environments/environment.ts` e certifique-se de que a porta aponta para onde a sua API está rodando:
   - Se estiver rodando via `dotnet run` pelo terminal:
     ```typescript
     export const environment = {
       production: false,
       apiUrl: 'https://localhost:7021/api' // ou http://localhost:5135/api
     };
     ```
   - Se estiver rodando pelo **Visual Studio com IIS Express**:
     ```typescript
     export const environment = {
       production: false,
       apiUrl: 'https://localhost:44329/api'
     };
     ```

4. Inicie a aplicação Angular:
   ```bash
   npm start
   ```
   > O comando `npm start` compila o arquivo de estilos `src/tailwind.css` para `src/tailwind.generated.css` e inicia o servidor de desenvolvimento (`ng serve`).

5. Abra o navegador em:
   - [http://localhost:4200](http://localhost:4200)

---

## 🌐 Portas e URLs Importantes

| Serviço | URL Padrão | Observações |
| :--- | :--- | :--- |
| **Frontend (Angular)** | `http://localhost:4200` | Interface do usuário e PWA |
| **Backend (Swagger .NET)** | `https://localhost:7021/swagger` ou `http://localhost:5135/swagger` | Documentação interativa dos endpoints REST |
| **Backend (IIS Express)** | `https://localhost:44329/swagger` | Usado por padrão caso execute via Visual Studio |
| **PostgreSQL** | `localhost:5432` | Banco de dados |

---

## 🔧 Resolução de Problemas Comuns (Troubleshooting)

### 1. Erro de CORS ou tela carregando infinitamente no Frontend
- **Causa**: O frontend está tentando se comunicar com a URL/porta errada da API ou o backend não liberou a origem.
- **Solução**:
  1. Olhe no terminal onde o backend está rodando e anote a porta (ex: `https://localhost:7021` ou `http://localhost:5135`).
  2. Abra `frontend/src/environments/environment.ts` e garanta que `apiUrl` aponta para a mesma URL com o sufixo `/api`.
  3. No `Program.cs` do backend, a política de CORS já permite `http://localhost:4200`.

### 2. Erro de certificado SSL local (`NET::ERR_CERT_AUTHORITY_INVALID`)
- **Causa**: O certificado HTTPS de desenvolvimento do .NET ainda não foi confiado na sua máquina.
- **Solução**: No terminal, execute:
  ```bash
  dotnet dev-certs https --trust
  ```

### 3. Falha ao conectar no PostgreSQL (`Npgsql.NpgsqlException: Connection refused`)
- **Causa**: O serviço do banco de dados não está ativo ou a senha está incorreta.
- **Solução**:
  - Se estiver usando Docker, execute: `docker compose ps` para garantir que o container está com status `Up`.
  - Se estiver usando PostgreSQL local, confira se o serviço `postgresql-x64-XX` está em execução nos Serviços do Windows (`services.msc`) e se a senha no `appsettings.json` é idêntica à que você definiu na instalação.

### 4. Erro de execução de script no PowerShell do Windows (`Execution_Policies`)
- **Causa**: O Windows restringe a execução de scripts `.ps1` (como `ng` ou `npm`).
- **Solução**: Abra o PowerShell como Administrador e execute:
  ```powershell
  Set-ExecutionPolicy -Scope CurrentUser -ExecutionPolicy RemoteSigned
  ```

### 5. Estilos quebrados ou classes Tailwind não aplicadas
- **Causa**: O Tailwind CSS v4 compila os estilos através de `src/tailwind.generated.css`.
- **Solução**: Execute manualmente:
  ```bash
  cd frontend
  npm run prestart
  ```
  E em seguida execute `npm start`.

---

✨ **Pronto!** Seu ambiente local do MonkOrc está configurado e pronto para o desenvolvimento.
