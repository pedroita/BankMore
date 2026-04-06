# BankMore — Plataforma de Banco Digital

## Stack
- **.NET 8** | **Clean Architecture** | **CQRS + MediatR** | **DDD**
- **Dapper** + **SQLite** | **JWT** | **BCrypt** | **Refit** | **Swagger**
- **xUnit** + **NSubstitute** + **FluentAssertions**
- **Docker** + **Docker Compose**

---

## Arquitetura

```
BankMore/
├── src/
│   ├── BankMore.ContaCorrente.Domain/          # Entidades, Value Objects, Interfaces, Enums
│   ├── BankMore.ContaCorrente.Application/     # Commands, Queries, Handlers (MediatR)
│   ├── BankMore.ContaCorrente.Infrastructure/  # Dapper, JWT, BCrypt, SQLite
│   ├── BankMore.ContaCorrente.API/             # Controllers, Swagger, Middleware
│   ├── BankMore.Transferencia.Domain/          # Entidades, Interfaces, Enums
│   ├── BankMore.Transferencia.Application/     # Commands, Handlers (MediatR)
│   ├── BankMore.Transferencia.Infrastructure/  # Dapper, Refit, SQLite
│   └── BankMore.Transferencia.API/             # Controllers, Swagger, Middleware
└── tests/
    ├── BankMore.ContaCorrente.Tests/            # Unitários — xUnit + NSubstitute
    ├── BankMore.Transferencia.Tests/            # Unitários — xUnit + NSubstitute
```

### Padrões aplicados
- **Clean Architecture** — Domain → Application → Infrastructure → API
- **CQRS** — Commands (escrita) e Queries (leitura) separados via MediatR
- **DDD** — Entidades ricas, Value Objects (CPF), interfaces no domínio, DomainException
- **Idempotência** — todas as operações de escrita protegidas por `IdRequisicao`

---

## Microsserviços

### API Conta Corrente — porta 5001
Responsável por cadastro, autenticação, movimentações e saldo.

### API Transferência — porta 5002
Responsável por transferências entre contas. Comunica-se com a API Conta Corrente via **Refit**.

```
[Cliente] → [API Transferência] → [API Conta Corrente]
                                       ↓
                                  [Débito Origem]
                                  [Crédito Destino]
                                  [Estorno se falhar]
```

---

## Como executar

### Localmente

```bash
# API Conta Corrente
cd src/BankMore.ContaCorrente.API
dotnet run
# Swagger: http://localhost:5000/swagger

# API Transferência (em outro terminal)
cd src/BankMore.Transferencia.API
dotnet run
# Swagger: http://localhost:5100/swagger
```

### Via Docker (1 instância)

```bash
docker compose up -d --build
# Conta Corrente: http://localhost:5001/swagger
# Transferência:  http://localhost:5002/swagger
```

### Via Docker (2 instâncias — alta disponibilidade)

```bash
docker compose up -d --build --scale conta-corrente-api=2 --scale transferencia-api=2
```

A idempotência garante que múltiplas instâncias processando a mesma requisição não duplicam operações.

### Executar testes

```bash
# Todos os testes
dotnet test

# Com detalhes
dotnet test --verbosity normal

# Só unitários
dotnet test tests/BankMore.ContaCorrente.Tests
dotnet test tests/BankMore.Transferencia.Tests

# Só integração
dotnet test tests/BankMore.ContaCorrente.IntegrationTests
dotnet test tests/BankMore.Transferencia.IntegrationTests
```

---

## API Conta Corrente — Endpoints

### POST /api/conta-corrente/cadastrar
Cadastra uma nova conta corrente.

**Request:**
```json
{
  "cpf": "529.982.247-25",
  "nome": "Ana Silva",
  "senha": "Senha@123"
}
```

**Respostas:**
| Status | Descrição |
|--------|-----------|
| 200 | Conta criada — retorna `{ "numeroConta": "12345678" }` |
| 400 | CPF inválido — `{ "mensagem": "...", "tipo": "INVALID_DOCUMENT" }` |

---

### POST /api/conta-corrente/login
Autentica e retorna token JWT.

**Request:**
```json
{
  "identificador": "12345678",
  "senha": "Senha@123"
}
```
> `identificador` pode ser o número da conta **ou** o CPF.

**Respostas:**
| Status | Descrição |
|--------|-----------|
| 200 | Login OK — retorna `{ "token": "eyJ..." }` |
| 401 | Credenciais inválidas — `{ "mensagem": "...", "tipo": "USER_UNAUTHORIZED" }` |

---

### PATCH /api/conta-corrente/inativar 🔒
Inativa a conta corrente do usuário logado.

**Request:**
```json
{
  "senha": "Senha@123"
}
```

**Respostas:**
| Status | Descrição |
|--------|-----------|
| 204 | Conta inativada com sucesso |
| 400 | Senha inválida — `{ "mensagem": "...", "tipo": "USER_UNAUTHORIZED" }` |
| 403 | Token inválido ou expirado |

---

### POST /api/conta-corrente/movimentar 🔒
Registra crédito ou débito na conta corrente.

**Request:**
```json
{
  "idRequisicao": "req-001",
  "numeroConta": null,
  "valor": 100.00,
  "tipo": "C"
}
```
> `numeroConta`: null usa a conta do token. `tipo`: C = Crédito, D = Débito.
> `idRequisicao`: gerado pelo cliente — garante idempotência em retentativas.

**Respostas:**
| Status | Descrição |
|--------|-----------|
| 204 | Movimentação registrada |
| 400 | `INVALID_ACCOUNT` — conta não encontrada |
| 400 | `INACTIVE_ACCOUNT` — conta inativa |
| 400 | `INVALID_VALUE` — valor não positivo |
| 400 | `INVALID_TYPE` — tipo inválido ou débito em conta alheia |
| 400 | `INSUFFICIENT_FUNDS` — saldo insuficiente para débito |
| 403 | Token inválido ou expirado |

---

### GET /api/conta-corrente/saldo 🔒
Consulta o saldo da conta do usuário logado.

**Respostas:**
| Status | Descrição |
|--------|-----------|
| 200 | Retorna saldo |
| 400 | `INVALID_ACCOUNT` / `INACTIVE_ACCOUNT` |
| 403 | Token inválido ou expirado |

**Response 200:**
```json
{
  "numeroConta": "12345678",
  "nomeTitular": "Ana Silva",
  "dataHoraConsulta": "2026-04-06T10:00:00Z",
  "saldo": 350.00
}
```

---

## API Transferência — Endpoints

### POST /api/transferencia/transferir 🔒
Efetua transferência entre contas da mesma instituição.

**Request:**
```json
{
  "idRequisicao": "transf-001",
  "numeroContaDestino": "87654321",
  "valor": 150.00
}
```
> A conta de origem é extraída automaticamente do token JWT.

**Fluxo interno:**
1. Valida idempotência pelo `idRequisicao`
2. Débita na conta de origem via API Conta Corrente
3. Credita na conta de destino via API Conta Corrente
4. Em caso de falha no crédito → **estorno automático** na origem
5. Persiste na tabela `transferencia`
6. Registra na tabela `idempotencia`

**Respostas:**
| Status | Descrição |
|--------|-----------|
| 204 | Transferência realizada com sucesso |
| 400 | `INVALID_VALUE` — valor não positivo |
| 400 | `INVALID_ACCOUNT` — origem igual ao destino |
| 400 | `TRANSFER_ERROR` — falha no débito ou crédito (com estorno automático) |
| 403 | Token inválido ou expirado |

---

## Testes

### Unitários
| Projeto | Cenários |
|---------|----------|
| ContaCorrente.Tests | CPF válido/inválido, idempotência, saldo insuficiente, tipo inválido, débito em conta alheia, login inválido |
| Transferencia.Tests | Idempotência, valor negativo, mesma conta, falha no crédito com estorno |

### Integração (diferencial)
| Projeto | Cenários |
|---------|----------|
| ContaCorrente.IntegrationTests | Fluxo completo cadastro → login → movimentar → saldo → inativar, todos os erros |
| Transferencia.IntegrationTests | Transferência válida, idempotência, estorno automático, todos os erros |

> Os testes de integração sobem a API em memória via `WebApplicationFactory` — não precisam de nenhum serviço externo rodando.


---

## Infraestrutura

### Rodando na VPS com 2 instâncias

```
http://69.169.109.42:5001/swagger  → API Conta Corrente
http://69.169.109.42:5002/swagger  → API Transferência
```


