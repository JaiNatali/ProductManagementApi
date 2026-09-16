# Environment Variables

The API reads configuration from `appsettings.json`, environment variables, and optional in-memory overrides.

## Common ASP.NET Core variables

- `ASPNETCORE_ENVIRONMENT` - environment name (`Development`, `Production`, `Testing`)
- `ASPNETCORE_URLS` - base URLs for the web host

## SQL Server connection

Override the SQL Server connection string with:

```text
ConnectionStrings__SqlServer=<YOUR_CONNECTION_STRING>
```

## JWT configuration

Override JWT settings with environment variables:

```text
JwtSettings__Issuer="ProductApiAssessment"
JwtSettings__Audience="ProductApiAssessment.Users"
JwtSettings__SecretKey=<YOUR_JWT_SECRET_AT_LEAST_32_CHARACTERS>
JwtSettings__AccessTokenExpirationMinutes="60"
JwtSettings__RefreshTokenExpirationInDays="7"
```

## Seed user passwords

When the application creates the seeded Admin and User accounts, provide their initial passwords through environment variables:

```text
SeedUsers__AdminPassword=<YOUR_ADMIN_PASSWORD>
SeedUsers__UserPassword=<YOUR_USER_PASSWORD>
```

## Feature control

- `EnableSwagger` - `true` or `false`
- `UseHttpsRedirection` - `true` or `false`
- `SkipDatabaseInitialization` - `true` or `false`

Example (Windows):

```powershell
set EnableSwagger=true
set UseHttpsRedirection=false
```

Example (Linux/macOS):

```bash
export EnableSwagger=true
export UseHttpsRedirection=false
```

> Note: In ASP.NET Core, colon-separated configuration keys become `__` in environment variables.
