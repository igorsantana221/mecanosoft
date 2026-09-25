# 📋 MonkOrc - Checklist de Desenvolvimento

Acompanhamento do progresso do sistema MonkOrc (Web/PWA).

## 🛠️ Fase 1: Fundação Backend (.NET 8)
- [x] Setup do projeto Web API (.NET 8)
- [x] Configuração do Entity Framework Core + PostgreSQL
- [x] Implementação do DbContext e Models Iniciais (User)
- [x] Sistema de Autenticação (JWT + RBAC) - Concluído no Backend
- [x] Endpoints de Usuário (Registro, Login) - Controllers e JWT implementados

## 🎨 Fase 2: Fundação Frontend (Angular 18/19)
- [x] Inicialização do Workspace Angular
- [x] Configuração do Tailwind CSS v4 (Abordagem CSS-first)
- [x] Adição do Angular Material
- [x] Configuração PWA (Progressive Web App)
- [x] Integração do Design System (Cores, Fontes e Utilitários)

## 🔐 Fase 3: Funcionalidades de Autenticação
- [x] Tela de Login (UI 100% Design System)
- [x] Colocar loop de carregamento nas telas de login, registro e recuperação de senha
- [x] Tela de Registro (UI 100% Design System)
- [x] Tela de Recuperação de Senha (UI 100% Design System)
- [x] Integração do Frontend com API de Auth
- [x] Alertas de Erro e Sucesso na API
- [x] Validação de E-mail ou Celular para confirmação de cadastro
- [x] Validação de E-mail ou Celular para confirmação de redefinição de senha
- [x] Telas de registro adicionar novos campos CNPJ, Endereço e Telefone(todos campos devem ter validação e endereço tem que colocar cep e trazer o endereço automaticamente, caso não tenha o cep pode preencher manualmente)
- [x] Guards de Rota (Proteção de páginas logadas)
- [x] Criar tela de nova senha e confirme senha para recuperação de senha
- [x] Edição do produto 
- [x] Edição do clientes
- [x] Tela de editar perfil 

## 🏢 Fase 3.5: Arquitetura Multi-Tenant (Isolamento de Dados)
- [x] Criação da Entidade `Tenant` (Organização/Empresa)
- [x] Associação de `User` ao `Tenant` (Relacionamento de Propriedade)
- [x] Implementação de `TenantId` em todas as Entidades de Negócio (Clientes, Produtos, Orçamentos)
- [x] Configuração de Global Query Filters no Entity Framework (Isolamento Automático)
- [x] Middleware para captura de `TenantId` via Claims do JWT
- [x] Ajuste no Fluxo de Registro (Criação automática de Tenant para novos usuários)

## 📦 Fase 4: Core do Negócio
- [x] Cadastro de Produtos (UI 100% Design System | Backend Multi-Tenant Integrado | NF-e/NFS-e Fiscal)
- [x] Novo Orçamento (UI 100% Design System | Backend Multi-Tenant Integrado)
- [x] Lista de Orçamentos (UI 100% Design System | Backend Multi-Tenant Integrado)
- [x] Dashboard Principal (UI 100% Design System)
- [x] Tela de Cadastro de Clientes (UI 100% Design System | Backend Multi-Tenant Integrado)
- [x] Tela de Lista de Clientes (UI 100% Design System | Backend Multi-Tenant Integrado)
- [ ] Tela de edição de proposta (UI 100% Design System | Backend Multi-Tenant Integrado)


## 💳 Fase 5: Assinaturas e Planos
- [ ] Tela de Planos e Preços
- [ ] Integração de Pagamento (Assinaturas)
- [ ] Controle de acesso por plano


## Melhorias no processo 


- [ ]preciso em criar um status de aprovado/gerar OS  aonde é possivell destiguir quem é orçamento e quem virou OS,

---
*Atualizado em: 23/04/2026 às 21:13*

