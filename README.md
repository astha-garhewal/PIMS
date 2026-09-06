\# Product Inventory Management System (PIMS)



A secure RESTful Web API for managing products, categories, inventory, price adjustments, inventory transactions, and audits.



\## Tech Stack



\* .NET 8 Web API

\* C#

\* Entity Framework Core 8

\* SQL Server

\* JWT Authentication

\* Role-Based Authorization (Administrator/User)

\* Serilog

\* Swagger / OpenAPI

\* xUnit \& Moq

\* In-Memory Caching



\## Architecture



The application follows a layered architecture:



```text

PIMS

├── PIMS.API             → Controllers, Middleware, API configuration

├── PIMS.Application     → DTOs, Interfaces, Business Services

├── PIMS.Domain          → Entities and Domain Models

├── PIMS.Infrastructure  → EF Core, Repositories, External Services

└── PIMS.Tests           → Unit and Integration Tests

```



\## Key Features



\* JWT-based authentication

\* Secure password hashing using ASP.NET Core PasswordHasher

\* Role-based authorization

\* Product and category management

\* Unique SKU validation

\* Product search and category filtering

\* Individual and bulk price adjustments

\* Percentage and fixed price deductions

\* Protection against negative prices

\* Inventory creation and management

\* Add/Subtract inventory transactions

\* Transaction history with timestamps and responsible user

\* Low inventory alerts with automatic resolution

\* Manual inventory audits with audit history

\* Global exception handling

\* Serilog request and error logging

\* Product caching using IMemoryCache

\* Asynchronous database operations

\* Unit and integration testing

\* Swagger/OpenAPI documentation



\## API Endpoints



\### Authentication



```text

POST /api/v1/auth/register

POST /api/v1/auth/login

```



\### Products



```text

GET  /api/v1/products

GET  /api/v1/products/{id}

POST /api/v1/products

PUT  /api/v1/products/{id}/price

PUT  /api/v1/products/price/bulk

```



\### Categories



```text

GET  /api/v1/categories

POST /api/v1/categories

```



\### Inventory



```text

POST /api/v1/inventory

GET  /api/v1/inventory/{id}

POST /api/v1/inventory/{id}/transactions

GET  /api/v1/inventory/alerts

POST /api/v1/inventory/{id}/audits

GET  /api/v1/inventory/{id}/audits

```



\## Authorization



The system supports two roles:



\* \*\*Administrator\*\* – manages prices and performs inventory audits.

\* \*\*User\*\* – authenticated users can view products/inventory and perform permitted operations.



JWT tokens contain the user's identity and role and are used for API authorization.



\## Database Setup



The application uses SQL Server with Entity Framework Core Code First and migrations.



Configure the connection string in:



```text

PIMS.API/appsettings.json

```



Example:



```json

"ConnectionStrings": {

&#x20; "PIMSConnection": "Server=localhost;Database=PIMSDB;Trusted\_Connection=True;TrustServerCertificate=True;"

}

```



The project contains an `InitialCreate` EF Core migration.



For a new database, run:



```bash

dotnet ef database update --project PIMS.Infrastructure --startup-project PIMS.API

```



The existing development database has already been baselined against the `InitialCreate` migration.



\## JWT Configuration



The JWT signing key should \*\*not\*\* be committed to source control.



Set it as an environment variable:



```bash

export Jwt\_\_Key="your-secure-development-key"

```



On Windows PowerShell:



```powershell

$env:Jwt\_\_Key="your-secure-development-key"

```



\## Running the Application



From the solution directory:



```bash

dotnet restore

dotnet build

dotnet run --project PIMS.API

```



Swagger is available at:



```text

http://localhost:5168/swagger

```



\## Running Tests



```bash

dotnet test

```



\## Logging



Application logs are written to the console and daily rolling log files under:



```text

logs/

```



Log files are excluded from source control.



\## Security



\* Passwords are stored using secure password hashing.

\* JWT signing keys are supplied through configuration/environment variables.

\* Role-based authorization protects administrative operations.

\* Sensitive configuration values are excluded from source control.

\* Global exception handling prevents internal exception details from being exposed through API responses.



\## License



This project was developed as a technical assignment demonstrating REST API development, clean architecture, security, database design, testing, and backend engineering practices.



