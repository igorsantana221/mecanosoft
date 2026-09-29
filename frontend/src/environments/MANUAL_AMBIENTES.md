# Manual de Ambientes (Environments) - Angular

Este documento explica como utilizar os arquivos de `environment` (ambiente) para separar a configuração da URL da API de acordo com o ambiente (Desenvolvimento, Homologação, Produção).

## 📁 Estrutura de Arquivos

Foram criados os seguintes arquivos dentro da pasta `src/environments/`:

- **`environment.ts`**: Ambiente padrão (Desenvolvimento local).
- **`environment.hmg.ts`**: Ambiente de Homologação (Testes integrados).
- **`environment.prod.ts`**: Ambiente de Produção (Deploy final / Clientes).

## 🛠️ Como Funciona

Em todos os *services* do Angular, a URL base da API agora não está mais fixa (hardcoded). Ao invés disso, o código importa o `environment.ts` base:

```typescript
import { environment } from '../../../environments/environment';

// Uso nos services:
private readonly apiUrl = `${environment.apiUrl}/Exemplo`;
```

**Mágica do Angular:** Quando você realiza o build do projeto informando a configuração correta (`--configuration`), o Angular substitui automaticamente o arquivo `environment.ts` pelo arquivo específico daquele ambiente (ex: `environment.prod.ts`). Assim, o código permanece o mesmo, mas a URL muda dinamicamente.

---

## 🚀 Como fazer o Build e Deploy

Para que a troca de arquivos aconteça corretamente, o arquivo `angular.json` (na raiz do projeto) precisa estar configurado para fazer a substituição (File Replacements) nos blocos de `configurations`.
*(Se o seu projeto for Angular 15+, isso geralmente já vem pronto ou requer uma configuração simples de "fileReplacements").*

### 1. Build de Produção

Para compilar o sistema para o ambiente oficial de produção:

```bash
npm run build -- --configuration production
# ou
ng build --configuration production
```

Isso fará com que o conteúdo do `environment.prod.ts` seja usado. As chamadas de API apontarão para a URL de produção definida lá.

### 2. Build de Homologação (Testes)

Para compilar o sistema para o ambiente de testes/homologação:

```bash
npm run build -- --configuration hmg
# ou
ng build --configuration hmg
```

*(Obs: Certifique-se de que o bloco de configuração `hmg` foi criado dentro do `angular.json` -> `projects.nome-do-projeto.architect.build.configurations` contendo o `fileReplacements`)*.

### 3. Rodando Localmente

Para rodar o projeto na sua máquina de desenvolvimento com a API local (`localhost`):

```bash
npm start
# ou
ng serve
```

Ele automaticamente utilizará o `environment.ts` puro, que aponta para `https://localhost:44329/api`.

---

## ✏️ Alterando as URLs

Quando você for subir a API de fato (backend), abra os arquivos de ambiente e troque as URLs de exemplo pela sua URL real:

1. **Abra `src/environments/environment.hmg.ts`** e altere:
   ```typescript
   apiUrl: 'https://api-homologacao.seudominio.com/api'
   ```
2. **Abra `src/environments/environment.prod.ts`** e altere:
   ```typescript
   apiUrl: 'https://api.seudominio.com/api'
   ```

Se a API não tiver a rota base terminando com `/api`, lembre-se de ajustar isso também, pois os services atualmente consideram que a URL termina com `/api`.
