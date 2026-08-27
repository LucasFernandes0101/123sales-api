# 123vendas - Sales Management Platform

## Table of Contents

* [Project Goal](#project-goal)
* [Technologies and Patterns Used](#technologies-and-patterns-used)
* [Business Entities](#business-entities)
* [Database Structure](#database-structure)
* [Event Architecture: Integration with RabbitMQ](#event-architecture-integration-with-rabbitmq)
* [Running the Project](#running-the-project)
* [Improvement Points](#improvement-points)

## Project Goal

**123vendas** is an innovative platform that simplifies the management of sales, products, and customers for companies with multiple branches. The solution centralizes product and customer management, allowing inventory and pricing customization per branch. With a robust interface, the system supports the entire sales cycle, from creating and updating orders to canceling and managing inventory.

This project demonstrates the use of modern technologies and patterns, highlighting the application of DDD, authentication and authorization with JWT, integration with RabbitMQ for sales events, and a scalable, maintainable software architecture.

## Technologies and Patterns Used

* **.NET 10**: Foundation for building scalable and robust applications.
* **MediatR**: Used to promote decoupled communication. The pattern was applied specifically to the users and authentication area.
* **Fluent Validations**: Data validation in a fluent and intuitive way.
* **BCrypt**: Secure password hashing.
* **Exception Middleware**: Centralized exception handling with appropriate HTTP responses.
* **DDD (Domain-Driven Design)**: Structuring the system with a focus on the business domain.
* **JWT**: Role-based authentication and authorization.
* **Serilog**: Structured logging for monitoring and diagnostics.
* **Automapper**: Automatic mapping between entities and DTOs.
* **Asp.Versioning.Mvc**: API version management.
* **Swagger**: Interactive and accessible documentation.
* **Entity Framework Core**: Object-relational mapping with configurations via `IEntityTypeConfiguration`.
* **IQueryable**: Dynamic and optimized queries.
* **RabbitMQ.Client**: Integration with RabbitMQ for event-based communication.

### Unit Tests

* **xUnit**: Testing framework for .NET, ensuring modularity and ease of writing tests.
* **FluentAssertions**: Fluent syntax for assertions, making tests more readable.
* **Shouldly**: Better readability for test error messages.
* **Bogus**: Fake data generation for unit tests.
* **NSubstitute**: Dependency mocking to facilitate isolated testing.

## Business Entities

The project was designed with Domain-Driven Design principles, where entities represent essential components of the sales and management domain. Instead of exposing class details, the following aspects are highlighted:

* **Base Entities**: All entities share common attributes such as a unique identifier, soft-delete control, and creation/update records, ensuring traceability and consistency.
* **Branches and Products**: Branches are mapped to allow price customization and inventory control per unit. Products are associated with branches, enabling centralized management with local flexibility.
* **Shopping Cart**: Models the selection of products made by users, keeping the relationship between chosen items and their quantities.
* **Sales and Sale Items**: Capture the details of each transaction, including the sale status, items sold, and the possibility of partial or full cancellation.
* **Users**: Store access information and personal data, defining roles that determine each user's permissions.

Each entity was carefully designed to reflect business rules and needs, establishing clear relationships and integrity in the data model.

## Database Structure

The project uses **Postgres** as the relational database. The structure is defined using **Entity Framework Core** with modular configurations via `IEntityTypeConfiguration`, promoting a clean and organized design.

Below is the database diagram used in this project:

![Diagrama\_DB\_123Vendas](https://github.com/user-attachments/assets/2bbb0886-3591-4ead-bed4-2d9dc7111b71)

> **Note:** This diagram represents the database structure and may be updated as needed.

The project has a seed system controlled by a **feature flag** called:

```
SEED_DATABASE_FLAG
```

This flag is configured in the `launchSettings.json` file, inside the `environmentVariables` section, with the default value `"false"`. To enable seeding, simply change the value to `"true"`. The application will automatically populate the database with initial data, provided the tables are empty.

---

In the **PostgreDbContext** file, the constructor ensures the database will be created automatically if it does not exist:

```csharp
public PostgreDbContext(DbContextOptions<PostgreDbContext> options) : base(options)
{
    base.Database.EnsureCreated();
}
```

## Event Architecture: Integration with RabbitMQ

The application integrates with **RabbitMQ** using a Pub/Sub architecture, allowing distributed and independent processing of sales events. The exchange **ex_sale** (type **direct**) enables consumers to create custom queues and bind to specific routing keys for the events they are interested in.

### Event Details

* **SaleCancelledEvent**

  * **Routing Key**: `SaleCancelledEvent`
  * **Description**: Fired when a sale is canceled.
  * **Sample Payload**:

    ```json
    {
        "Id": 1,
        "CancelledAt": "2024-10-24T15:30:00Z"
    }
    ```

* **SaleCreatedEvent**

  * **Routing Key**: `SaleCreatedEvent`
  * **Description**: Fired when a new sale is created.
  * **Sample Payload**:

    ```json
    {
        "Id": 1,
        "Date": "2024-10-24T14:00:00Z"
    }
    ```

* **SaleItemCancelledEvent**

  * **Routing Key**: `SaleItemCancelledEvent`
  * **Description**: Fired when a specific item of a sale is canceled.
  * **Sample Payload**:

    ```json
    {
        "SaleId": 1,
        "SaleItemId": 2,
        "Sequence": 1,
        "CancelledAt": "2024-10-24T15:00:00Z"
    }
    ```

* **SaleUpdatedEvent**

  * **Routing Key**: `SaleUpdatedEvent`
  * **Description**: Fired when an existing sale is updated.
  * **Sample Payload**:

    ```json
    {
        "Id": 1,
        "UpdatedAt": "2024-10-24T16:00:00Z"
    }
    ```

This architecture ensures that each service consumes only relevant events, optimizing performance and easing scalability.

## Running the Project

### Option 1: Docker Compose (Recommended)

The project includes a `docker-compose.yml` that spins up PostgreSQL, RabbitMQ, and the API with all environment variables pre-configured:

```bash
docker compose up --build
```

The API will be available at `http://localhost:8080`, and Swagger UI at `http://localhost:8080/swagger`.

A health check endpoint is available at `http://localhost:8080/health`.

RabbitMQ Management UI is available at `http://localhost:15672` (guest/guest).

### Option 2: Local Execution

To run the project locally, you need to configure the following environment variables, which define behavior and connections to the external services used by the application. **Note:** For local execution, these variables are configured in the `launchSettings.json` file.

```json
"environmentVariables": {
  "ASPNETCORE_ENVIRONMENT": "Development",
  "JWT_SECRETKEY": "dR8!v9Kp@zL3xWq#N5gT7mYb$FcJ2sV0",
  "POSTGRES_CONNECTION_STRING": "",
  "RABBITMQ_HOSTNAME": "",
  "RABBITMQ_USERNAME": "",
  "RABBITMQ_VIRTUALHOST": "",
  "RABBITMQ_PASSWORD": "",
  "SEED_DATABASE_FLAG": "false"
}
```

### Description of Each Variable

* **ASPNETCORE_ENVIRONMENT**: Defines the environment in which the application will run (for example, Development, Staging, or Production). This influences specific configurations like logging and error details.
* **JWT_SECRETKEY**: Secret key used to sign and validate JWT tokens, ensuring the integrity and security of the authentication mechanism.
* **POSTGRES_CONNECTION_STRING**: Connection string for the Postgres database, configuring server address, database name, access credentials, and other connection options.
* **RABBITMQ_HOSTNAME**: Hostname or IP address of the RabbitMQ server, used to publish and consume events.
* **RABBITMQ_USERNAME**: Username for authentication on the RabbitMQ server.
* **RABBITMQ_VIRTUALHOST**: Virtual host in RabbitMQ, allowing logical separation of environments or applications on the same server.
* **RABBITMQ_PASSWORD**: Password corresponding to the user defined to access RabbitMQ.
* **SEED_DATABASE_FLAG**: Flag used to control the execution of initial data seeds in the database, useful for development and testing environments.
* **LOG_LEVEL**: Defines the minimum log level for Serilog (e.g., Information, Warning, Error, Debug).

> **Notes:**
>
> * **Postgres**: Make sure Postgres is installed and properly configured on your machine.
> * **PostgreDbContext**: The constructor of `PostgreDbContext` contains the call `base.Database.EnsureCreated();`, ensuring the database will be created automatically if it does not exist.
> * **JWT Secret Key**: The JWT secret key can be changed as needed in the `launchSettings.json` file.

### Steps to Start the Project (Local)

1. Clone the repository:

   ```bash
   git clone https://github.com/LucasFernandes0101/123sales-server.git
   ```
2. Navigate to the project folder:

   ```bash
   cd 123sales-server
   ```
3. Run the project:

   ```bash
   dotnet run --project src/123vendas.Api
   ```

### Running Tests

To run the unit tests:

```bash
dotnet test
```

To run tests with coverage (if configured):

```bash
dotnet test --collect:"XPlat Code Coverage"
```

## Improvement Points

To enhance the security, scalability, and maintainability of the project, consider the following points:

* **Key Vault**: Move the JWT secret key, as well as database and RabbitMQ credentials, to a Key Vault. This way, sensitive information is managed securely.
* **Centralized Configuration**: Adopt a centralized configuration manager to ease maintenance and deployment across different environments.
* **Automated Tests**: Expand coverage for unit, functional, and integration tests to ensure system robustness and reliability.
* **Monitoring and Logging**: Integrate advanced monitoring and logging tools to proactively identify and resolve issues.
* **Pub/Sub Scalability**: Review and optimize the event architecture to support higher transaction volumes and multiple consumer services.
