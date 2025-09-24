# Mottu Fleet API — C# (Pátio)

> API RESTful em **.NET 8** para gestão de **motos**, **tags BLE** e **funcionários** do pátio.  
> Foco em boas práticas REST, **paginação**, **HATEOAS**, documentação **Swagger/OpenAPI** e **EF Core** com Oracle.

<p align="left">
  <img src="https://img.shields.io/badge/.NET-8.0-5C2D91?logo=.net&logoColor=white" />
  <img src="https://img.shields.io/badge/Swagger-OpenAPI-green?logo=swagger" />
  <img src="https://img.shields.io/badge/EF%20Core-Oracle-orange" />
</p>

---

## 📌 Sumário
- [Integrantes](#integrantes)
- [Resumo do Projeto](#resumo-do-projeto)
- [Requisitos Atendidos](#requisitos-atendidos)
- [Arquitetura & Tecnologias](#arquitetura--tecnologias)
- [Entidades do Domínio](#entidades-do-domínio)
- [Paginação & HATEOAS](#paginação--hateoas)
- [Como Executar](#como-executar)
- [Configuração (Connection String)](#configuração-connection-string)
- [Migrations (EF Core)](#migrations-ef-core)
- [Documentação Swagger](#documentação-swagger)
- [Endpoints & Exemplos de Payloads](#endpoints--exemplos-de-payloads)
- [Estrutura do Repositório](#estrutura-do-repositório)
- [Códigos de Status & Erros](#códigos-de-status--erros)
- [Testes](#testes)

---

## 👥 Integrantes
- **Amanda Mesquita Cirino da Silva** — RM559177  
- **Beatriz Ferreira Cruz** — RM555698  
- **Journey Tiago Lopes Ferreira** — RM556071  

---

## 🧭 Resumo do Projeto
Esta API expõe operações de CRUD para três entidades principais (**Moto**, **TagBle**, **Funcionario**), com:
- **Paginação** nas listagens (`page`, `pageSize`) e metadados em `X-Pagination`.
- **HATEOAS** por recurso (links de `self`, `update`, `delete`).
- **Swagger/OpenAPI** com exemplos e mapeamento de enums como string.
- Persistência via **Entity Framework Core** com **Oracle**.

---

## ✅ Requisitos Atendidos
- **3 entidades principais** (Moto, TagBle, Funcionario).  
- **CRUD completo** para as 3 entidades com boas práticas REST.  
- **Paginação** e **HATEOAS** nas listagens.  
- **Swagger** com descrição de endpoints, modelos e exemplos.  
- **README** com instruções de execução e exemplos de uso.

---

## 🏗️ Arquitetura & Tecnologias
- **Camadas**: Controllers ➜ Services ➜ Data (EF Core) ➜ Entities/DTOs  
- **Principais pacotes**:
  - `Swashbuckle.AspNetCore` (Swagger)
  - `Oracle.EntityFrameworkCore` (EF Core provider)
- **Middlewares**: `GlobalExceptionHandlerMiddleware` (padroniza respostas de erro)
- **Padrões**:
  - DTOs para entrada/saída
  - `CreatedAtRoute` em POST
  - `ProducesResponseType` e XML docs nos endpoints

---

## 🗂️ Entidades do Domínio
- **Moto**: `Id`, `Placa`, `Modelo` (enum), `StatusMoto` (enum), `DataCriacaoRegistro`, `Tag` (opcional)  
- **TagBle**: `Id`, `CodigoUnicoTag` (único), `NivelBateria`  
- **Funcionario**: `Id`, `Nome`, `Email` (único), `Cargo`

> Enums serializados como **string** no JSON (configurado em `Program.cs`).

---

## 🔁 Paginação & HATEOAS
- **Query params**: `page` (default: 1), `pageSize` (default: 10), `q` (busca livre)  
- **Header**: `X-Pagination` com `{ TotalItems, Page, PageSize, TotalPages }`  
- **Links HATEOAS**: cada item retorna `links: [ { rel, href, method } ]`

Exemplo de `X-Pagination`:
```json
{"TotalItems": 42, "Page": 1, "PageSize": 10, "TotalPages": 4}
```

---

## ▶️ Como Executar
### 1) Requisitos
- .NET 8 SDK  
- Banco Oracle acessível (ou container local)  
- Connection string configurada (veja abaixo)

### 2) Restaurar, migrar e subir
```bash
dotnet restore
dotnet build

# aplica migrations no banco configurado
dotnet ef database update

# executa a API
dotnet run --project Csharp.Api
```

Acesse o Swagger em: **http://localhost:5000/swagger**  
> A porta pode variar; verifique o console ao iniciar.

---

## 🔧 Configuração (Connection String)
A aplicação busca a string **`OracleConnection`** em `appsettings.json` (ou variáveis de ambiente).

Exemplo:
```json
{
  "ConnectionStrings": {
    "OracleConnection": "User Id=USR;Password=SENHA;Data Source=HOST:1521/ORCLPDB1"
  }
}
```

Via variável de ambiente (recomendado em produção):
```bash
# Windows
setx ConnectionStrings__OracleConnection "User Id=USR;Password=SENHA;Data Source=HOST:1521/ORCLPDB1"

# Linux/macOS (exemplo de sessão atual)
export ConnectionStrings__OracleConnection="User Id=USR;Password=SENHA;Data Source=HOST:1521/ORCLPDB1"
```

---

## 🧱 Migrations (EF Core)
Gerar e aplicar migrations:
```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

> As migrations também são aplicadas automaticamente no startup (`Database.Migrate()`).

---

## 📚 Documentação Swagger
- Habilitado em `/swagger`  
- Lê **XML docs** gerado no build  
- Enums exibidos como **string** com lista de valores  
- Header **`X-Pagination`** documentado nos GETs (OperationFilter)

---

## 🔌 Endpoints & Exemplos de Payloads

### Motos
**Listar (com paginação):**
```
GET /api/motos?page=1&pageSize=10&status=Disponivel&placa=ABC
```

**Criar:**
```http
POST /api/motos
Content-Type: application/json

{
  "placa": "ABC1D23",
  "modelo": "CG160",            // TipoModeloMoto (exemplo)
  "statusMoto": "Disponivel"    // TipoStatusMoto
}
```

**Atualizar:**
```http
PUT /api/motos/{id}
Content-Type: application/json

{
  "placa": "ABC1D23",
  "modelo": "CG160",
  "statusMoto": "Manutencao"
}
```

**Remover:**
```
DELETE /api/motos/{id}
```

---

### Tags BLE
**Listar:**
```
GET /api/tags?page=1&pageSize=10&q=TAG
```

**Criar:**
```http
POST /api/tags
Content-Type: application/json

{
  "codigoUnicoTag": "TAG-0001-AAA",
  "nivelBateria": 92
}
```

**Atualizar:**
```http
PUT /api/tags/{id}
Content-Type: application/json

{
  "codigoUnicoTag": "TAG-0001-AAA",
  "nivelBateria": 80
}
```

**Remover:**
```
DELETE /api/tags/{id}
```

---

### Funcionários
**Listar:**
```
GET /api/funcionarios?page=1&pageSize=10&q=beatriz
```

**Criar:**
```http
POST /api/funcionarios
Content-Type: application/json

{
  "nome": "Beatriz Ferreira Cruz",
  "email": "beatriz@example.com",
  "cargo": "Analista"
}
```

**Atualizar:**
```http
PUT /api/funcionarios/{id}
Content-Type: application/json

{
  "nome": "Beatriz F. Cruz",
  "email": "beatriz@example.com",
  "cargo": "Especialista"
}
```

**Remover:**
```
DELETE /api/funcionarios/{id}
```

---

## 🗃️ Estrutura do Repositório
```
Csharp.Api/
 ├─ Controllers/
 │   ├─ MotosController.cs
 │   ├─ TagsController.cs
 │   └─ FuncionariosController.cs
 ├─ Data/
 │   └─ AppDbContext.cs
 ├─ DTOs/
 │   ├─ MotoDtos.cs
 │   ├─ TagBleDtos.cs
 │   ├─ FuncionarioDtos.cs
 │   └─ Pagination.cs           # PagedResult, LinkDto, HateoasBuilder
 ├─ Entities/
 │   ├─ Moto.cs
 │   ├─ TagBle.cs
 │   └─ Funcionario.cs
 ├─ Middleware/
 │   └─ GlobalExceptionHandlerMiddleware.cs
 ├─ Services/
 │   ├─ IMotoService.cs / MotoService.cs
 │   ├─ ITagBleService.cs / TagBleService.cs
 │   └─ IFuncionarioService.cs / FuncionarioService.cs
 ├─ Swagger/
 │   └─ PaginationHeaderOperationFilter.cs
 ├─ Migrations/
 ├─ Program.cs
 └─ README.md
```

---

## 🧾 Códigos de Status & Erros
- `200 OK` – consultas/atualizações  
- `201 Created` – criação (com `Location` e corpo retornado)  
- `204 No Content` – deleção  
- `400 Bad Request` – validações  
- `404 Not Found` – recurso inexistente  
- `500 Internal Server Error` – erros não tratados  

> Erros seguem `ProblemDetails` via `GlobalExceptionHandlerMiddleware`.

---

## 🧪 Testes
Opcional para o escopo desta entrega. Sugestão: incluir projeto `xUnit` com testes de serviço e controller básico em uma próxima iteração.
