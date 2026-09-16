# Setup Instructions

## Prerequisites

- .NET 8 SDK
- SQL Server instance or Docker Desktop
- Optional: Visual Studio 2022/2023, Visual Studio Code

## Local database using Docker

A `docker-compose.yml` file is provided to run SQL Server and the API locally.

```bash
docker compose up -d
```

The compose file exposes SQL Server on port `1433` and the API on port `5000`.

## Configure database connection

Provide the SQL Server connection string through the `ConnectionStrings__SqlServer` environment variable or local configuration:

```json
"ConnectionStrings": {
  "SqlServer": "<YOUR_CONNECTION_STRING>"
}
```

Provide `JwtSettings__SecretKey`, `SeedUsers__AdminPassword`, and `SeedUsers__UserPassword` as local or deployment environment variables. Do not commit these values.

## Restore and run

From the repository root:

```bash
dotnet restore ProductApiAssessment.sln
dotnet run --project src/API/API.csproj
```

The API will start with settings from `src/API/appsettings.json`, and Swagger is available at one of the launch profile URLs:

```text
https://localhost:7237/swagger/index.html
```

or

```text
http://localhost:5024/swagger/index.html
```

## Run tests

- API integration tests:
  ```bash
dotnet test tests/API.Tests/API.Tests.csproj
```
- Application unit tests:
  ```bash
dotnet test tests/Application.Tests/Application.Tests.csproj
```
- Infrastructure tests:
  ```bash
dotnet test tests/Infrastructure.Tests/Infrastructure.Tests.csproj
```

## Notes

- The API uses Swagger if `EnableSwagger` is enabled in configuration.
- The `Testing` environment uses an in-memory EF Core database for API tests.
