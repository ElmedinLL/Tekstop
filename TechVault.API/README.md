# TechVault API

REST API for the TechVault e-commerce platform: catalog, cart, checkout, orders, payments (Stripe), reviews, wishlist, and admin tooling.

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-Web%20API-512BD4?logo=dotnet)](https://learn.microsoft.com/aspnet/core/)
[![Entity Framework Core](https://img.shields.io/badge/EF%20Core-8.0-512BD4?logo=dotnet)](https://learn.microsoft.com/ef/core/)
[![MySQL](https://img.shields.io/badge/MySQL-8-4479A1?logo=mysql&logoColor=white)](https://www.mysql.com/)
[![Swagger / OpenAPI](https://img.shields.io/badge/Swagger-OpenAPI-85EA2D?logo=swagger)](https://swagger.io/)

---

## Tech stack

| Area | Technology |
|------|------------|
| Runtime | .NET 8, ASP.NET Core Web API |
| Data | EF Core 8, Pomelo MySQL provider, dual contexts (`ApplicationDbContext` domain + `AuthDbContext` Identity) |
| Auth | ASP.NET Core Identity, JWT Bearer, refresh tokens |
| Validation | FluentValidation |
| Payments | Stripe.net |
| Email | MailKit |
| Mapping | AutoMapper |
| Logging | Serilog (console + rolling file) |
| API docs | Swashbuckle / Swagger (Development), URL-based API versioning (`Asp.Versioning`) |
| Other | Rate limiting, in-memory catalog cache, health checks |

---

## Folder structure (high level)

```
TechVault.API/
├── Admin/                 # Admin-oriented DTOs (e.g. dashboard stats)
├── Auth/                  # Identity user, JWT helpers, seed options
├── Caching/               # Catalog list cache
├── Controllers/           # API endpoints (versioned routes)
├── Data/                  # DbContexts, EF migrations, design-time factories
├── Docs/                  # Internal docs (e.g. API versioning)
├── Errors/                # ProblemDetails-style error payloads
├── Health/                # Health check helpers
├── Inventory/             # Low-stock settings & monitoring
├── Mapping/               # AutoMapper profiles
├── Middleware/            # Global exception handling, etc.
├── Models/                # Domain entities & enums
├── Notifications/         # Order email notifications
├── Orders/, Products/, Payments/, Reviews/, Users/, …  # Feature DTOs & contracts
├── Program.cs             # Composition root, middleware pipeline
├── Properties/            # launchSettings.json
├── RateLimiting/          # IP + route parsing for limiters
├── Repositories/          # Data access abstractions (incl. product catalog)
├── Services/              # Business services (orders, cart, admin, Stripe, …)
├── Swagger/               # Versioned OpenAPI configuration
├── Validation/            # FluentValidation validators
├── appsettings*.json      # Configuration (use .example as template)
└── wwwroot/               # Static files (e.g. product images)
```

---

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- MySQL 8.x (local or remote)
- Optional: [Stripe](https://stripe.com/) account for payments; SMTP for transactional email

---

## Setup

1. **Clone / open** the solution and `cd` into `TechVault.API`.

2. **Create configuration**  
   Copy the example file and fill in secrets (never commit real secrets):

   ```bash
   copy appsettings.example.json appsettings.json
   ```

   On Linux/macOS: `cp appsettings.example.json appsettings.json`

3. **Configure MySQL**  
   Set `ConnectionStrings:DefaultConnection` in `appsettings.json` (or use environment variables / user secrets) to point at your database.

4. **JWT**  
   Set `Jwt:Secret` to a random string **at least 32 characters** (required at startup).

5. **Apply EF Core migrations** (domain schema):

   ```bash
   dotnet ef database update --project TechVault.API.csproj --context ApplicationDbContext
   ```

   If you maintain Identity migrations separately, apply `AuthDbContext` the same way when applicable.

6. **Run the API**

   ```bash
   dotnet run
   ```

   Default Development URLs (see `Properties/launchSettings.json`):

   - HTTP: `http://localhost:5092`
   - HTTPS: `https://localhost:7171`

   Swagger UI: `/swagger` (Development only).

7. **Optional seed admin**  
   Set `IdentitySeed:AdminEmail` and `IdentitySeed:AdminPassword` in configuration so `IdentitySeeder` can create an **Admin** user on startup (see `appsettings.example.json`).

---

## Environment variables & configuration

Configuration follows standard ASP.NET Core: `appsettings.json`, `appsettings.{Environment}.json`, environment variables, and user secrets. Environment variables use `__` (double underscore) for nested keys, e.g. `ConnectionStrings__DefaultConnection`.

| Section / key | Purpose |
|---------------|---------|
| `ConnectionStrings:DefaultConnection` | MySQL connection string (shared DB for domain + Identity in typical setups) |
| `Jwt:Secret` | HS256 signing key (≥ 32 chars) |
| `Jwt:Issuer` | JWT issuer |
| `Jwt:Audience` | JWT audience |
| `Jwt:ExpiryInDays` | Access token lifetime hint (refresh flow also used) |
| `Stripe:SecretKey` | Stripe secret API key |
| `Stripe:PublishableKey` | Stripe publishable key (often frontend) |
| `Stripe:WebhookSecret` | Stripe webhook signing secret |
| `Stripe:Currency` | Default currency (e.g. `usd`) |
| `Smtp:*` | Outbound email (host, port, credentials, from address) |
| `Inventory:*` | Low-stock threshold and optional daily alert email |
| `IdentitySeed:*` | Optional default admin user seeding |
| `Serilog` | Optional Serilog sink configuration (see example) |
| `ASPNETCORE_ENVIRONMENT` | `Development` / `Production`, etc. |

See **`appsettings.example.json`** for the full shape and comments.

---

## API versioning

All JSON routes (except a few infrastructure paths) live under **`/api/v{version}/...`**. The current version is **v1**.

Examples:

- `GET /api/v1/products`
- `POST /api/v1/auth/login`

`AssumeDefaultVersionWhenUnspecified` is **false**: clients should send **`v1`** in the URL. More detail: **`Docs/API-VERSIONING.md`**.

---

## API endpoints (v1)

Legend: **Auth** = JWT required · **Admin** = role `Admin` · **Anon** = anonymous allowed

### Infrastructure

| Method | Path | Notes |
|--------|------|--------|
| GET | `/health` | Liveness / readiness style health (unversioned) |

### Auth (`/api/v1/auth`)

| Method | Path | Notes |
|--------|------|--------|
| POST | `/register` | Anon |
| POST | `/login` | Anon |
| POST | `/refresh` | Anon (refresh token body) |
| POST | `/logout` | Auth |
| GET | `/me` | Auth |

### Users (`/api/v1/users`)

| Method | Path | Notes |
|--------|------|--------|
| GET | `/profile` | Auth |
| PUT | `/profile` | Auth |
| PUT | `/change-password` | Auth |
| POST | `/profile/avatar` | Auth |

### Addresses (`/api/v1/addresses`)

| Method | Path | Notes |
|--------|------|--------|
| GET | `/` | Auth |
| POST | `/` | Auth |
| PUT | `/{id}` | Auth |
| DELETE | `/{id}` | Auth |
| PUT | `/{id}/default` | Auth |

### Products (`/api/v1/products`)

| Method | Path | Notes |
|--------|------|--------|
| GET | `/` | List/search (query params); Anon |
| GET | `/{id}` | Published product; Anon |
| GET | `/featured` | Anon |
| GET | `/category/{slug}` | Anon |
| POST | `/` | Admin |
| PUT | `/{id}` | Admin |
| POST | `/{id}/images` | Admin |
| DELETE | `/{id}` | Admin (soft delete) |

### Categories (`/api/v1/categories`)

| Method | Path | Notes |
|--------|------|--------|
| GET | `/` | Anon |
| GET | `/{slug}` | Anon |
| POST | `/upload-image` | Admin |
| POST | `/` | Admin |
| PUT | `/{slug}` | Admin |
| DELETE | `/{slug}` | Admin |

### Cart (`/api/v1/cart`)

| Method | Path | Notes |
|--------|------|--------|
| GET | `/` | Session / guest + user |
| POST | `/items` | Add line |
| PUT | `/items/{itemId}` | Update quantity |
| DELETE | `/items/{itemId}` | Remove line |
| DELETE | `/` | Clear cart |
| POST | `/merge` | Merge guest cart after login |

### Wishlist (`/api/v1/wishlist`)

| Method | Path | Notes |
|--------|------|--------|
| GET | `/` | Auth |
| POST | `/{productId}` | Auth |
| DELETE | `/{productId}` | Auth |

### Coupons (`/api/v1/coupons`)

| Method | Path | Notes |
|--------|------|--------|
| POST | `/validate` | Validate code (typical use: checkout) |

### Orders (`/api/v1/orders`)

| Method | Path | Notes |
|--------|------|--------|
| POST | `/` | Auth — create order |
| GET | `/` | Auth — list my orders |
| GET | `/{orderId}` | Auth |
| POST | `/{orderId}/cancel` | Auth |

### Reviews (`/api/v1/products/{productId}/reviews`)

| Method | Path | Notes |
|--------|------|--------|
| GET | `/` | Anon — approved reviews |
| GET | `/me` | Auth — purchase + my review status |
| POST | `/` | Auth — create review |
| DELETE | `/` | Auth — delete my review |

### Payments (`/api/v1/payments`)

| Method | Path | Notes |
|--------|------|--------|
| POST | `/create-intent` | Auth |
| POST | `/confirm` | Auth |
| POST | `/webhook` | Anon — Stripe webhook (signature verification) |

### Admin — stats (`/api/v1/admin/stats`)

| Method | Path | Notes |
|--------|------|--------|
| GET | `/` | Admin — dashboard aggregates |

### Admin — orders (`/api/v1/admin/orders`)

| Method | Path | Notes |
|--------|------|--------|
| GET | `/` | Admin — list (filters: `status`, `from`, `to`, `search`, `page`, `pageSize`) |
| GET | `/{orderId}` | Admin — detail |
| PUT | `/{orderId}/status` | Admin — update status (`Shipped` requires `trackingUrl`; shipped email via existing flow) |
| POST | `/{orderId}/ship` | Admin — mark shipped + tracking URL + email |

### Admin — products (`/api/v1/admin/products`)

| Method | Path | Notes |
|--------|------|--------|
| GET | `/` | Admin — product list |
| GET | `/{id}` | Admin |
| GET | `/low-stock` | Admin |

### Admin — coupons (`/api/v1/admin/coupons`)

| Method | Path | Notes |
|--------|------|--------|
| GET | `/` | Admin |
| POST | `/` | Admin |
| PUT | `/{id}` | Admin |
| DELETE | `/{id}` | Admin |

### Admin — users (`/api/v1/admin/users`)

| Method | Path | Notes |
|--------|------|--------|
| GET | `/` | Admin — list (`page`, `pageSize`, `search`) |
| PUT | `/{userId}/ban` | Admin — body: ban flag |

### Sample / demo

| Method | Path | Notes |
|--------|------|--------|
| GET | `/api/v1/WeatherForecast` | Template controller (may be removed in production) |

---

## Swagger

In **Development**, open **`/swagger`**, pick the **v1** document, and use **Authorize** with a Bearer JWT for protected endpoints.

---

## Rate limiting

Versioned API routes are subject to global rate limits (by client IP; stricter bucket for auth routes). Excessive traffic returns **429** with a JSON error body.

---

## CORS

Default policy (**Development**) allows the frontend origin **`http://localhost:5173`** with credentials. Adjust in `Program.cs` for other environments.

---

## License / ownership

Use and license terms follow your repository root policy.
