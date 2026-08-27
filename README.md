# 123vendas — Sales Management API

A RESTful sales management platform built with .NET 10 and Domain-Driven Design. Handles multi-branch product inventory, shopping carts, the full sales lifecycle (including partial item cancellation), and publishes domain events to RabbitMQ for downstream consumers.

## Table of Contents

- [Architecture](#architecture)
- [Tech Stack](#tech-stack)
- [Project Structure](#project-structure)
- [Domain Model](#domain-model)
- [API Endpoints](#api-endpoints)
- [Event-Driven Integration](#event-driven-integration)
- [Database](#database)
- [Running with Docker](#running-with-docker)
- [Running Locally](#running-locally)
- [Testing](#testing)
- [Roadmap](#roadmap)

## Architecture

The solution is split into four projects following DDD layering:

```
┌─────────────────────────────────────────────┐
│              123vendas.Api                  │  Presentation layer — controllers,
│                                             │  Swagger, middleware, API versioning
├─────────────────────────────────────────────┤
│           123vendas.Application             │  Application services, MediatR handlers,
│                                             │  JWT, manual mapping, FluentValidation
├─────────────────────────────────────────────┤
│          123vendas.Infrastructure           │  EF Core, PostgreSQL, RabbitMQ,
│                                             │  repository implementations, seeders
├─────────────────────────────────────────────┤
│             123vendas.Domain                │  Entities, enums, interfaces,
│                                             │  validators, domain exceptions
└─────────────────────────────────────────────┘
```

Dependencies flow inward: `Api → Application → Domain ← Infrastructure`. The Domain layer has zero external dependencies — it defines interfaces that Infrastructure implements.

### Key Decisions

- **CQRS (partial)**: Users and Auth operations go through MediatR commands/queries. The heavier CRUD services (Branches, Products, Sales, Carts) use a service-based pattern for richer business logic.
- **CancellationToken everywhere**: Every async method — from controllers down to EF Core queries — accepts and propagates a `CancellationToken`.
- **Soft delete**: All entities inherit from `BaseEntity` with an `IsDeleted` flag. No records are physically removed.
- **Pagination + dynamic ordering**: List endpoints accept `page`, `size`, and `orderByClause` parameters. Ordering is applied dynamically via `IQueryable` extensions.
- **Primary constructors**: Used across services, handlers, and repositories for cleaner dependency injection.

## Tech Stack

| Category | Technology |
|----------|-----------|
| Runtime | .NET 10 |
| ORM | Entity Framework Core 10 + Npgsql |
| Database | PostgreSQL 17 |
| Message Broker | RabbitMQ 4 |
| Authentication | JWT Bearer + BCrypt |
| Validation | FluentValidation |
| Mapping | Manual (extension methods) |
| Mediation | MediatR |
| Logging | Serilog |
| API Versioning | Asp.Versioning.Mvc |
| Documentation | Swashbuckle (Swagger) |

### Test Stack

| Category | Technology |
|----------|-----------|
| Framework | xUnit |
| Assertions | FluentAssertions + Shouldly |
| Mocking | NSubstitute |
| Fake Data | Bogus |
| Integration | WebApplicationFactory + Testcontainers (PostgreSQL, RabbitMQ) |

## Project Structure

```
123sales-api/
├── src/
│   ├── 123vendas.Api/              # Controllers, middleware, Swagger config
│   ├── 123vendas.Application/       # Services, handlers, JWT, manual mappers
│   ├── 123vendas.Domain/            # Entities, enums, interfaces, validators
│   └── 123vendas.Infrastructure/    # DbContext, repositories, RabbitMQ, seeders
├── tests/
│   ├── 123vendas.Unit/              # Service and handler unit tests
│   └── 123vendas.Integration/       # Integration tests with Testcontainers
├── docker-compose.yml
├── Dockerfile
└── 123vendas-server.slnx
```

## Domain Model

| Entity | Description |
|--------|-------------|
| `Branch` | Physical store location with name, address, phone |
| `Product` | Catalog product with title, category, price, rating |
| `BranchProduct` | Per-branch inventory: stock quantity, localized pricing |
| `Cart` | Shopping cart tied to a user |
| `CartProduct` | Items inside a cart with quantity |
| `Sale` | Transaction with status (Created/Canceled), total amount |
| `SaleItem` | Individual line item with sequence, unit price, discount, cancellation |
| `User` | Account with email, role (Customer/Manager/Admin), status |

All entities share `Id`, `IsDeleted`, `CreatedAt`, and `UpdatedAt` from `BaseEntity`.

### Business Rules Implemented

- **Quantity limits**: Max 20 identical items per sale
- **Automatic discounts**: 4+ items → 10%, 9+ → 15%, 20+ → 20%
- **Stock validation**: Sale rejected if branch stock is insufficient
- **Partial cancellation**: Individual sale items can be cancelled without voiding the sale
- **Product sync**: Updating a product title/category propagates to all `BranchProduct` entries via `ExecuteUpdateAsync`

## API Endpoints

All endpoints are versioned under `/v1/`. Protected routes require JWT with `ManagerOnly` policy.

| Resource | Method | Route | Auth |
|----------|--------|-------|------|
| Auth | POST | `/v1/api/auth` | Public |
| Branches | GET | `/v1/api/branches` | Public |
| Branches | GET | `/v1/api/branches/{id}` | Public |
| Branches | POST | `/v1/api/branches` | Manager |
| Branches | PUT | `/v1/api/branches/{id}` | Manager |
| Branches | DELETE | `/v1/api/branches/{id}` | Manager |
| BranchProducts | GET | `/v1/api/branch-products` | Public |
| BranchProducts | GET | `/v1/api/branch-products/{id}` | Public |
| BranchProducts | POST | `/v1/api/branch-products` | Manager |
| BranchProducts | PUT | `/v1/api/branch-products/{id}` | Manager |
| BranchProducts | DELETE | `/v1/api/branch-products/{id}` | Manager |
| Products | GET | `/v1/api/products` | Public |
| Products | GET | `/v1/api/products/categories` | Public |
| Products | GET | `/v1/api/products/{id}` | Public |
| Products | GET | `/v1/api/products/category/{category}` | Public |
| Products | POST | `/v1/api/products` | Manager |
| Products | PUT | `/v1/api/products/{id}` | Manager |
| Products | DELETE | `/v1/api/products/{id}` | Manager |
| Carts | GET | `/v1/api/carts` | Public |
| Carts | GET | `/v1/api/carts/{id}` | Public |
| Carts | POST | `/v1/api/carts` | Public |
| Carts | PUT | `/v1/api/carts/{id}` | Public |
| Carts | DELETE | `/v1/api/carts/{id}` | Public |
| Sales | GET | `/v1/api/sales` | Public |
| Sales | GET | `/v1/api/sales/{id}` | Public |
| Sales | POST | `/v1/api/sales` | Manager |
| Sales | PUT | `/v1/api/sales/{id}` | Manager |
| Sales | DELETE | `/v1/api/sales/{id}` | Manager |
| Sales | PUT | `/v1/api/sales/{id}/cancel` | Public |
| Sales | GET | `/v1/api/sales/{id}/items/{sequence}` | Public |
| Sales | PUT | `/v1/api/sales/{id}/items/{sequence}/cancel` | Public |
| Users | GET | `/v1/api/users/{id}` | Public |
| Users | POST | `/v1/api/users` | Public |
| Users | DELETE | `/v1/api/users/{id}` | Manager |

## Event-Driven Integration

Sales events are published to a RabbitMQ direct exchange (`ex_sale`) so downstream services can react independently:

| Event | Routing Key | Trigger |
|-------|-------------|---------|
| `SaleCreatedEvent` | `SaleCreatedEvent` | New sale registered |
| `SaleUpdatedEvent` | `SaleUpdatedEvent` | Existing sale modified |
| `SaleCancelledEvent` | `SaleCancelledEvent` | Sale fully cancelled |
| `SaleItemCancelledEvent` | `SaleItemCancelledEvent` | Single item cancelled |

### Event Payloads

**SaleCreatedEvent**

```json
{
    "Id": 1,
    "Date": "2024-10-24T14:00:00Z"
}
```

**SaleUpdatedEvent**

```json
{
    "Id": 1,
    "UpdatedAt": "2024-10-24T16:00:00Z"
}
```

**SaleCancelledEvent**

```json
{
    "Id": 1,
    "CancelledAt": "2024-10-24T15:30:00Z"
}
```

**SaleItemCancelledEvent**

```json
{
    "SaleId": 1,
    "SaleItemId": 2,
    "Sequence": 1,
    "CancelledAt": "2024-10-24T15:00:00Z"
}
```

Consumers can bind their own queues to specific routing keys, picking up only the events they care about.

## Database

PostgreSQL with EF Core. Schema is auto-created on startup via `Database.EnsureCreated()`. Entity configurations are modular, each implementing `IEntityTypeConfiguration<T>`.

```csharp
public PostgreDbContext(DbContextOptions<PostgreDbContext> options) : base(options)
{
    base.Database.EnsureCreated();
}
```

![Database Diagram](https://github.com/user-attachments/assets/2bbb0886-3591-4ead-bed4-2d9dc7111b71)

A seed system populates initial data (users, branches, products, carts, sales) when `SEED_DATABASE_FLAG=true`. Each seeder checks for existing records before inserting.

## Running with Docker

**Zero config. One command.** The `docker-compose.yml` orchestrates PostgreSQL, RabbitMQ, and the API with health checks, dependency ordering, and database seeding — all pre-configured and ready to go.

```bash
docker compose up --build
```

The API image uses a multi-stage Dockerfile: the SDK builds the project in the first stage, and only the runtime + published binaries make it into the final image (no SDK bloat).

### What happens behind the scenes

1. **PostgreSQL 17** spins up with a `vendas` database and a persistent volume
2. **RabbitMQ 4** starts with management UI enabled
3. Both services pass their health checks before the API starts
4. **API builds** via multi-stage Dockerfile (SDK → runtime-only image)
5. **Database auto-creates** tables via `EnsureCreated()` and **seeds** initial data (users, branches, products, carts, sales)
6. API health check kicks in after a 15s grace period

### Accessing the services

| Service | URL | Credentials |
|---------|-----|-------------|
| Swagger UI | http://localhost:8080/swagger | — |
| Health Check | http://localhost:8080/health | — |
| RabbitMQ Management | http://localhost:15672 | guest / guest |
| PostgreSQL | localhost:5432 | postgres / postgres |

> All services have `restart: unless-stopped`, so they survive machine reboots.

### Test Users

The database seed creates two users for testing the API:

| Role | Email | Password | Access |
|------|-------|----------|--------|
| Admin | `admin@123vendas.com` | `admin123` | Full access (all endpoints) |
| Manager | `vendedor@123vendas.com` | `seller123` | Manager-only endpoints (POST/PUT/DELETE on protected resources) |

> Use the `POST /v1/api/auth` endpoint with one of the credentials above to obtain a JWT token. Include it in the `Authorization` header as a Bearer token for protected endpoints.

## Running Locally

You'll need PostgreSQL and RabbitMQ running on your machine. Configure the connection details in `src/123vendas.Api/Properties/launchSettings.json`:

```json
"environmentVariables": {
  "ASPNETCORE_ENVIRONMENT": "Development",
  "JWT_SECRETKEY": "dR8!v9Kp@zL3xWq#N5gT7mYb$FcJ2sV0",
  "POSTGRES_CONNECTION_STRING": "Host=localhost;Port=5432;Database=vendas;Username=postgres;Password=postgres",
  "RABBITMQ_HOSTNAME": "localhost",
  "RABBITMQ_USERNAME": "guest",
  "RABBITMQ_VIRTUALHOST": "/",
  "RABBITMQ_PASSWORD": "guest",
  "SEED_DATABASE_FLAG": "true",
  "LOG_LEVEL": "Information"
}
```

Then:

```bash
git clone https://github.com/LucasFernandes0101/123sales-api.git
cd 123sales-api
dotnet restore
dotnet run --project src/123vendas.Api
```

### Environment Variables

| Variable | Description |
|----------|-------------|
| `ASPNETCORE_ENVIRONMENT` | Runtime environment (Development, Staging, Production) |
| `JWT_SECRETKEY` | Symmetric key for signing JWT tokens |
| `POSTGRES_CONNECTION_STRING` | Npgsql connection string |
| `RABBITMQ_HOSTNAME` | RabbitMQ broker hostname |
| `RABBITMQ_USERNAME` | RabbitMQ auth username |
| `RABBITMQ_VIRTUALHOST` | RabbitMQ virtual host |
| `RABBITMQ_PASSWORD` | RabbitMQ auth password |
| `SEED_DATABASE_FLAG` | Enables database seeding on startup |
| `LOG_LEVEL` | Minimum Serilog log level (Information, Warning, Error, Debug) |

## Testing

```bash
# Run all tests
dotnet test 123vendas-server.slnx

# Run only unit tests
dotnet test 123vendas-server.slnx --filter "Category=Unit"

# Run only integration tests
dotnet test 123vendas-server.slnx --filter "Category=Integration"

# Run with coverage
dotnet test 123vendas-server.slnx --collect:"XPlat Code Coverage"
```

### Unit Tests

Service layer logic (Branch, BranchProduct, Cart, Product, Sale) and MediatR handlers (Auth, Users). Mocks by NSubstitute, fake data by Bogus, assertions with FluentAssertions and Shouldly.

### Integration Tests

End-to-end tests against real PostgreSQL and RabbitMQ instances spun up via Testcontainers. Each test class inherits from `BaseIntegrationTest`, which automatically resets the database before every test method via `IAsyncLifetime`. JWT authentication is handled by `AuthHelper`, and HTTP clients are configured with extension methods for token management.

- **149 tests** covering all controllers (Auth, Users, Branches, BranchProducts, Products, Carts, Sales)
- Containers: `postgres:16-alpine`, `rabbitmq:3-management-alpine`
- Database isolation: `ResetDatabaseAsync()` runs before each test via `BaseIntegrationTest.InitializeAsync`

## Roadmap

- [ ] Move secrets to a key vault (AWS Secrets Manager / Azure Key Vault)
- [x] Add integration tests with Testcontainers
- [ ] Implement a RabbitMQ consumer service as an event-driven example
- [ ] Add OpenTelemetry tracing across services
- [ ] Introduce a CI/CD pipeline with automated test + build gates
