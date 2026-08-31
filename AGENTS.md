# EventTicketingSystem — Agent Guide

## Projects (use `EventTicketingSystem.slnx` for build/test; or `--project` for single projects)
- `src/API` — ASP.NET Core Web API (net10.0). Port **5123**. SQL Server via EF Core (`DbDevTicketappContext`, scaffolded from an existing DB; maps snake_case columns), JWT auth, purchase rate limiter, Azure Blob for event images.
- `src/Web` — ASP.NET Core MVC (net10.0). Port **5254**. Primary frontend. Calls API through `IHttpClientFactory` named `"API"` (base `http://localhost:5123/`, configured in `Program.cs`).
- `src/app/ETSWebapp` — Angular 22 SPA. Port **4200**. Calls the API directly (`http://localhost:5123/api/...`); CORS in the API only allows `http://localhost:4200`.

## Run / build
- API: `dotnet run --project src/API` (default profile binds `http://localhost:5123`)
- Web: `dotnet run --project src/Web` (binds `http://localhost:5254`)
- Angular: `cd src/app/ETSWebapp && npm start`
- EF migration (from `src/API`): `dotnet ef migrations add <Name> --project src/API`
- Unit tests: `test/UnitTests/API.Tests` (xUnit + Moq + FluentAssertions + EF SQLite in-memory) and `test/UnitTests/Web.Tests` (xUnit + Moq + FluentAssertions + fake HTTP handler). Run with `dotnet test` (sln-level; use `--project` for a single project). CI (`.github/workflows/ci.yml`) runs build + tests on push/PR to `dev`. No C# lint config. Verify with `dotnet build EventTicketingSystem.slnx`.

## Secrets — required to run the API
Connection strings are NOT in `appsettings.json`; the API project uses user secrets (`UserSecretsId` set in `API.csproj`). From `src/API`:
```
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "<sql-connection-string>"
dotnet user-secrets set "ConnectionStrings:AzureStorage" "<azure-blob-connection-string>"
```
`Program.cs` throws at startup if `JwtSettings:Secret` is missing; it is also expected in user secrets.

## Web auth model (important)
- Session is cookie-based, not the built-in ASP.NET auth. `AuthController` sets two cookies: `AuthToken` (HttpOnly — the API JWT) and `AuthInfo` (plaintext `idUsuario|Nombre|Correo|Role`).
- Read sessions via `Web.Helpers.AuthCookieHelper` (`ObtenerSesion`, `EsAdmin`). Admin = role contains "admin" (case-insensitive).
- Server-side calls to the API forward the JWT as `Authorization: Bearer` from the `AuthToken` cookie (see `CompraController.ProcesarPago`).
- Login/register/logout UI and fetch logic live in `Views/Shared/_Layout.cshtml` (`/Auth/Login`, `/Auth/Registrar`, `/Auth/Estado`, `/Auth/Logout`).

## Conventions
- Code, comments, DTO/model/controller names, and user-facing messages are in **Spanish** (`EventosController`, `BoletosController`, `CompraController`, `FacturasController`). Keep this.
- API layer = controllers → `Services/*Service` (scoped) → `Dtos/Dtos/<Dominio>/` → EF models.
- Purchase endpoint (`api/Boletos/comprar`) is rate-limited by the `CompraUsuario` policy (default 5/min/user, configurable under `RateLimits:CompraPorUsuario`).
- DB schema source of truth: `database/ddl/01_DDL_TicketManagementSystem_Schema.sql` + `database/dml/02_DML_Insert_Datos_Prueba.sql`. Schema diagram: `docs/desing/database.drawio`.
- Git: default branch `dev`; commits follow conventional style (`feat:`/`fix:`).
