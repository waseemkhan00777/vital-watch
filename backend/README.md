# VitalCare Backend (.NET 8 + MySQL)

ASP.NET Core Web API for the VitalWatch RPM (Remote Patient Monitoring) app. Uses **MySQL** with **Entity Framework Core** and **JWT** authentication.

## Requirements

- .NET 8 SDK
- MySQL 8.x (local or remote)

## Setup

1. **Create MySQL database**
   ```bash
   mysql -u root -p -e "CREATE DATABASE VitalCare CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;"
   ```

2. **Configure connection**
   - Edit `VitalCare.Api/appsettings.json` or `appsettings.Development.json` and set `ConnectionStrings:DefaultConnection`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=localhost;Port=3306;Database=VitalCare;User=root;Password=;"
   }
   ```

3. **Apply migrations and run**
   ```bash
   cd backend
   dotnet restore
   dotnet run --project VitalCare.Api
   ```
   On first run (Development), the API will apply migrations and seed default users and alert rules.

4. **Optional: apply migrations manually**
   ```bash
   dotnet ef database update --project VitalCare.Api
   ```
   (Requires: `dotnet tool install --global dotnet-ef`)

## API Base URL

- Default: **http://localhost:5000**
- Swagger UI: **http://localhost:5000/swagger**

## Authentication (HIPAA-aligned)

- **POST /api/auth/login** — Body: `{ "email", "password" }`. Returns `{ "token", "user" }`. Sets **httpOnly, Secure, SameSite** cookie `access_token` with JWT (no token in response needed for browser; use cookie).
- **POST /api/auth/register** — Body: `{ "email", "password", "name", "role" }`. Role defaults to `patient`.
- **GET /api/auth/me** — Returns current user. Accepts token from **cookie** or `Authorization: Bearer <token>`.
- **POST /api/auth/logout** — Clears `access_token` cookie. Call with credentials so cookie is sent.

Frontend must use `credentials: 'include'` on all API requests so the cookie is sent.

## Seeded users (after first run)

| Email                     | Password   | Role      |
|---------------------------|------------|-----------|
| admin@vitalwatch.demo     | admin123   | admin     |
| nurse@vitalwatch.demo     | nurse123   | clinician |
| patient@vitalwatch.demo   | patient123 | patient   |
| caregiver@vitalwatch.demo | caregiver123 | caregiver |

## Main endpoints

| Area        | Endpoints |
|------------|-----------|
| **Vitals** | `GET/POST /api/vitals` (patient: own; clinician/admin: by patientId; caregiver: consented patients) |
| **Alerts** | `GET /api/alerts`, `GET /api/alerts/{id}`, `PATCH /api/alerts/{id}` (state, clinicalNote) |
| **Users**  | `GET /api/users`, `GET /api/users/{id}` (admin/clinician) |
| **Caregiver** | `GET /api/caregiverlinks`, `POST /api/caregiverlinks` (invite), `POST /api/caregiverlinks/{id}/revoke` |
| **Alert rules** | `GET/POST/PUT/DELETE /api/alertrules` (admin only) |
| **Audit**  | `GET /api/audit` (filter by userId, resource, resourceId, from, to) |

## Security (HIPAA-oriented)

- **Row-level security**: Controllers enforce role and ownership (patient: own data; caregiver: consented patients only; clinician/admin: allowed scopes). No cross-tenant data access.
- **PHI at rest**: User names (PHI) are encrypted in the database using AES-256 (see `IEncryptionService`). Set `Encryption:Key` in appsettings (base64, 32 bytes) in production; dev uses a fallback.
- **Audit**: All auth and sensitive actions are logged to `AuditLogs` (user, resource, action, IP, timestamp).
- **Cookies**: JWT in httpOnly cookie to reduce XSS exposure; SameSite=Lax, Secure in production.

## Frontend integration

Set the API base URL in your Next.js app (e.g. `.env.local`):

```env
NEXT_PUBLIC_API_URL=http://localhost:5000
```

Use the same request/response shapes as in `frontend/src/lib/types.ts` and `vitals-schema.ts` (vital submission is a discriminated union by `type`). All API calls must use `credentials: 'include'` for cookie-based auth.

## Project structure

```
backend/
├── VitalCare.sln
└── VitalCare.Api/
    ├── Controllers/     # Auth, Vitals, Alerts, Users, CaregiverLinks, AlertRules, Audit
    ├── Data/            # ApplicationDbContext, Entities, DbSeeder
    ├── DTOs/            # Request/response DTOs aligned with frontend types
    ├── Services/        # AuthService, AlertEvaluationService, AuditService
    ├── Migrations/      # EF Core migrations
    ├── Program.cs
    └── appsettings.json
```

## Alert rules and SLA

- Alert rules are evaluated when a vital is submitted. Matching rules create alerts with:
  - **Critical** → SLA 1 hour
  - **High** → SLA 4 hours
  - Others → 24 hours
- Default seeded rules cover blood_pressure, heart_rate, oxygen_saturation, blood_glucose thresholds.
