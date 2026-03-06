# VitalCare ABP Backend

Backend for VitalCare built with **ABP Framework** and **PostgreSQL**.

## Requirements

- .NET 8 SDK
- PostgreSQL 12+ (local or remote)

## Solution structure

| Project | Description |
|--------|-------------|
| **VitalCare.Abp.Domain.Shared** | Constants, enums (roles, vital types, alert operators/severity/state) |
| **VitalCare.Abp.Domain** | Entities and repository interfaces |
| **VitalCare.Abp.Application.Contracts** | DTOs and application service interfaces |
| **VitalCare.Abp.Application** | Application services, encryption, audit, alert evaluation |
| **VitalCare.Abp.EntityFrameworkCore** | DbContext, PostgreSQL mappings, custom repositories, migrations, seeder |
| **VitalCare.Abp.HttpApi** | REST controllers (Auth, Vitals, Alerts, Users, CaregiverLinks, AlertRules, Audit) |
| **VitalCare.Abp.HttpApi.Host** | Host (Kestrel), JWT + cookie auth, CORS, migration + seed in Development |

## PostgreSQL setup

**Option A – Docker Compose (recommended for dev)**

From the backend solution directory:

```bash
docker compose up -d postgres
```

This creates a database `vitalcare` with user `waseem` and the password from `docker-compose.yml`. `appsettings.Development.json` is preconfigured to match. If you get *password authentication failed for user "waseem"*, the Postgres data volume may have been created with different credentials; reset it with:

```bash
docker compose down -v
docker compose up -d postgres
```

**Option B – Local PostgreSQL**

1. Create a database, e.g. `CREATE DATABASE vitalcare;`
2. Set the connection string in **Development**: `appsettings.Development.json` → `ConnectionStrings:Default`
3. **Production**: use `appsettings.json` or environment variables and set a strong password.

## Migrations

The host applies migrations automatically in Development and will re-apply if the migration history is out of sync (e.g. `__EFMigrationsHistory` has the migration but tables are missing).

To run migrations manually from the solution directory:

```bash
dotnet ef database update --project src/VitalCare.Abp.EntityFrameworkCore/VitalCare.Abp.EntityFrameworkCore.csproj --startup-project src/VitalCare.Abp.HttpApi.Host/VitalCare.Abp.HttpApi.Host.csproj
```

Requires the [EF Core tools](https://learn.microsoft.com/en-us/ef/core/cli/dotnet): `dotnet tool install -g dotnet-ef`

## Run

From the solution directory:

```bash
cd src/VitalCare.Abp.HttpApi.Host
dotnet run
```

Or from repo root:

```bash
dotnet run --project backend/VitalCare.Abp/src/VitalCare.Abp.HttpApi.Host/VitalCare.Abp.HttpApi.Host.csproj
```

- In **Development**, the host applies pending EF Core migrations and runs the seeder on startup.
- API base: **http://localhost:5000** (see `launchSettings.json`).
- Swagger UI: **http://localhost:5000/swagger**.

## Seeded users (Development)

After the first run in Development, these users are created:

| Email | Password | Role |
|-------|----------|------|
| admin@vitalwatch.demo | admin123 | admin |
| nurse@vitalwatch.demo | nurse123 | clinician |
| patient@vitalwatch.demo | patient123 | patient |
| caregiver@vitalwatch.demo | caregiver123 | caregiver |

Default alert rules are also seeded (e.g. blood pressure, heart rate, blood glucose, oxygen saturation thresholds).

## Configuration

- **Jwt** (appsettings): `Key`, `Issuer`, `Audience` for JWT validation. Auth supports both `Authorization: Bearer <token>` and cookie `access_token`.
- **Cors:Origins**: e.g. `http://localhost:3000` for the frontend.
- **Encryption:Key**: optional; if set, name encryption is used (see `IEncryptionService`). If empty, names are stored in plain text and the seeder logs a warning.

## Migrations

Migrations live in `src/VitalCare.Abp.EntityFrameworkCore/Migrations/`. To add a new migration (with `dotnet-ef` installed):

```bash
dotnet ef migrations add YourMigrationName --project src/VitalCare.Abp.EntityFrameworkCore --startup-project src/VitalCare.Abp.HttpApi.Host
```

Apply at runtime (Development) is done in `Program.cs` via `Database.MigrateAsync()`.

## ABP

This app uses **ABP Framework** (modular, DDD-style) with a custom auth model (no ABP Identity): custom `User` entity, JWT + cookie, and repository-based access. Volo.Abp packages used: Core, Ddd.Domain, Ddd.Application, EntityFrameworkCore, HttpApi, and Users.Abstractions (e.g. `ICurrentUser`).
