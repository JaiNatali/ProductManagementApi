# Deployment Instructions

## Build for production

From the repository root:

```bash
dotnet publish src/API/API.csproj -c Release -o ./publish
```

## Configure production settings

Create or override configuration values for production:

- `ConnectionStrings__SqlServer`
- `JwtSettings__Issuer`
- `JwtSettings__Audience`
- `JwtSettings__SecretKey`
- `JwtSettings__AccessTokenExpirationMinutes`
- `JwtSettings__RefreshTokenExpirationInDays`
- `EnableSwagger=false`
- `UseHttpsRedirection=true`
- `SkipDatabaseInitialization=false`
- `SeedUsers__AdminPassword` when the seeded Admin account is created
- `SeedUsers__UserPassword` when the seeded User account is created

## Running the published app

```bash
cd publish
dotnet API.dll
```

## Container deployment

A `docker-compose.yml` file is included for local SQL Server testing. For production, build and deploy a container image from the repository root:

```bash
docker build -t productapiassessment:latest .
```

Then run with environment variables:

```bash
docker run -d -p 5000:80 \
  -e ConnectionStrings__SqlServer="Server=<host>;Database=ProductApiAssessment;User Id=<user>;Password=<password>;TrustServerCertificate=True;" \
  -e JwtSettings__SecretKey="<long-secret>" \
  productapiassessment:latest
```

## Cloud deployment notes

- Ensure `ASPNETCORE_ENVIRONMENT=Production`.
- Use secure secrets management for JWT secret and connection strings.
- Disable Swagger in production unless explicitly required.
- Set up HTTPS termination and configure `UseHttpsRedirection` accordingly.

## Logging

The app uses Serilog and writes logs to console and file by default. Adjust `Serilog` settings in `appsettings.json` or override with environment-specific configuration.
