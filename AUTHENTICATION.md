# Authentication Flow

This API uses JWT bearer authentication and supports access token issuance, refresh tokens, logout, and role-based authorization.

## JWT settings

Configured in `src/API/appsettings.json` under `JwtSettings`:

- `Issuer` - token issuer
- `Audience` - API audience
- `SecretKey` - symmetric signing key (must be at least 32 characters)
- `AccessTokenExpirationMinutes` - access token lifetime
- `RefreshTokenExpirationInDays` - refresh token lifetime

## Login

Endpoint:

```http
POST /api/v1/auth/login
```

Request body:

```json
{
  "email": "<USER_EMAIL>",
  "password": "<USER_PASSWORD>"
}
```

Successful login returns:

- `accessToken` - JWT bearer token
- `refreshToken` - opaque refresh token

## Access token usage

Include the token in the `Authorization` header:

```http
Authorization: Bearer <accessToken>
```

Protected endpoints require authentication, and some require the `Admin` role.

## Refresh token

Endpoint:

```http
POST /api/v1/auth/refresh
```

Request body:

```json
{
  "refreshToken": "<refresh-token>"
}
```

## Logout

Endpoint:

```http
POST /api/v1/auth/logout
```
```

Request body:

```json
{
  "refreshToken": "<refresh-token>"
}
```

Requires authentication.

## Current user

Endpoint:

```http
GET /api/v1/auth/me
```

Returns the current authenticated user details based on the bearer token.

## Role-based authorization

- `Admin` role is required for creating, updating, and deleting products/items.
- Regular authenticated users can read products and items.

## Test authentication

The API test suite uses a test JWT helper with configured `JwtOptions` to generate tokens for `Admin` and `User` roles.
