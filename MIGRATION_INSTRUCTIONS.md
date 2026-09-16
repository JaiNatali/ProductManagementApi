# Migration Instructions

This project uses EF Core for database persistence in `src/Infrastructure`.

## Install EF tools

If needed, install the EF Core CLI tools:

```bash
dotnet tool install --global dotnet-ef
```

## Add a migration

From the solution root:

```bash
dotnet ef migrations add <MigrationName> --project src/Infrastructure/Infrastructure.csproj --startup-project src/API/API.csproj --context ApplicationDbContext
```

This creates a migration under `src/Infrastructure/Migrations`.

## Update the database

Run the migration against the configured SQL Server database:

```bash
dotnet ef database update --project src/Infrastructure/Infrastructure.csproj --startup-project src/API/API.csproj --context ApplicationDbContext
```

## Testing migrations

The `Testing` environment uses an in-memory database, so migrations are not applied there. For production and development, the SQL Server connection string from `src/API/appsettings.json` or environment variables is used.

## Troubleshooting

- Ensure the startup project is `src/API/API.csproj` because that project configures dependency injection and the DB context.
- Confirm the connection string points to a reachable SQL Server instance.
- If the secret key or JWT settings are invalid, the application may fail to start due to validation on startup.
