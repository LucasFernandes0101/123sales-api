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
│                                             │  JWT, AutoMapper, FluentValidation
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
| Mapping | AutoMapper |
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

## Project Structure

```
123sales-api/
├── src/
│   ├── 123vendas.Api/              # Controllers, middleware, Swagger config
│   ├── 123vendas.Application/       # Services, handlers, JWT, AutoMapper profiles
│   ├── 123vendas.Domain/            # Entities, enums, interfaces, validators
│   └── 123vendas.Infrastructure/    # DbContext, repositories, RabbitMQ, seeders
├── tests/
│   └── 123vendas.Unit/              # Service and handler unit tests
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

The fastest way to get the project running — one command spins up PostgreSQL, RabbitMQ, and the API with everything pre-configured:

```bash
docker compose up --build
```

That's it. The compose file handles:
- PostgreSQL 17 with health checks
- RabbitMQ 4 with management UI
- API build, dependency ordering, and health checks
- Database seeding (`SEED_DATABASE_FLAG=true`)

Once running:

| Service | URL |
|---------|-----|
| Swagger UI | http://localhost:8080/swagger |
| Health Check | http://localhost:8080/health |
| RabbitMQ Management | http://localhost:15672 (guest/guest) |

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
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

Tests cover service layer logic (Branch, BranchProduct, Cart, Product, Sale) and MediatR handlers (Auth, Users). Mocks are handled by NSubstitute, fake data by Bogus, and assertions use a mix of FluentAssertions and Shouldly.

## Roadmap

- [ ] Move secrets to a key vault (AWS Secrets Manager / Azure Key Vault)
- [ ] Add integration tests with Testcontainers
- [ ] Implement a RabbitMQ consumer service as an event-driven example
- [ ] Add OpenTelemetry tracing across services
- [ ] Introduce a CI/CD pipeline with automated test + build gates
