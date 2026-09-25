# Plano de Desenvolvimento MonkOrc

**Tipo de Projeto:** WEB / MOBILE
**Frameworks:** .NET 8 (Backend), Angular (Frontend)

## 📋 Visão Geral
Plano de desenvolvimento completo para o sistema web/mobile MonkOrc, utilizando .NET para o backend (com banco SQL) e Angular para o frontend. O foco é seguir o Design System em 100%.

## 🎯 Critérios de Sucesso
- API .NET funcional servindo endpoints REST.
- Front-end em Angular com roteamento estruturado.
- UI/UX pixel-perfect correspondendo às telas da pasta `Design_sistem`.
- Responsividade garantida para dispositivos móveis (PWA - Progressive Web App).

## 🛠️ Tech Stack
- **Backend:** .NET 8 Web API
- **Database:** Banco de Dados SQL Server (Entity Framework Core)
- **Frontend:** Angular 17/18
- **Styling:** Angular Material + SCSS adaptado do HTML original gerado pelo design system.

## 📁 Estrutura de Arquivos
```
MonkOrc/
├── backend/
│   └── MonkOrc.Api/
│       ├── Controllers/
│       ├── Models/
│       ├── Data/
│       └── Services/
├── frontend/
│   └── src/
│       ├── app/
│       │   ├── core/
│       │   ├── shared/
│       │   └── features/
│       └── assets/
└── Design_sistem/
```

## 📝 Detalhamento das Tarefas

### Fase 1: Fundação do Backend
- `[ ]` **Task 1.1**: Setup .NET Web API project and configure EF Core. (Agent: `backend-specialist`, Skills: `api-patterns`)
  - **INPUT**: Tipo de Banco SQL definido -> **OUTPUT**: Projeto rodando -> **VERIFY**: Swagger carregando.
- `[ ]` **Task 1.2**: Implementar estrutura de Auth com JWT. (Agent: `security-auditor`, Skills: `vulnerability-scanner`)
  - **INPUT**: Entidade Usuario -> **OUTPUT**: Login e Registro -> **VERIFY**: Token gerado com sucesso.

### Fase 2: Fundação do Frontend & Design System
- `[ ]` **Task 2.1**: Setup Angular Workspace. (Agent: `frontend-specialist`, Skills: `frontend-design`)
  - **INPUT**: Config Angular -> **OUTPUT**: Boilerplate App -> **VERIFY**: `ng serve` executado.
- `[ ]` **Task 2.2**: Estruturação base de SCSS a partir do Design System. (Agent: `frontend-specialist`, Skills: `frontend-design`)
  - **INPUT**: HTML/CSS da pasta Design_sistem -> **OUTPUT**: Variáveis/Estilos Globais no Angular -> **VERIFY**: Estilos mapeados e acessíveis nos componentes.

### Fase 3: Implementação de Funcionalidades Core
- `[ ]` **Task 3.1**: Módulo de Autenticação (Login, Cadastro, Recuperação, Perfil). (Agent: `frontend-specialist`)
  - **INPUT**: Telas correspondentes e API -> **OUTPUT**: Fluxo E2E integrado -> **VERIFY**: Login efetuado e redirecionamento.
- `[ ]` **Task 3.2**: Gestão de Orçamentos e Produtos. (Agent: `frontend-specialist` e `backend-specialist`)
  - **INPUT**: Telas de Orçamento e Cadastro de Produtos -> **OUTPUT**: CRUDs funcionais -> **VERIFY**: Dados salvos no banco e listados.
- `[ ]` **Task 3.3**: Planos e Assinatura. (Agent: `frontend-specialist` e `backend-specialist`)
  - **INPUT**: Tela de assinatura e planos -> **OUTPUT**: Regra de negócio implementada -> **VERIFY**: Mudança de plano refletida na UI.

## Fase X: Verificação
- [ ] Segurança (Audit de pacotes)
- [ ] UI/UX Audit (Comparativo de pixels com os screens de mock)
- [ ] Build de Produção frontend/backend testados
- [ ] Responsividade testada em 375px
