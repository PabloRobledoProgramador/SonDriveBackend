# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Overview

SonDrive backend: an ASP.NET Core Web API (.NET 10) for an intercity ride-sharing service around Sonsón (Antioquia, Colombia). Drivers (`Usuario`) register vehicles, publish trips (`Viaje`) between cities, and seats (`Puesto`) on those trips are booked by passengers (`Ocupacion` + `Pasajero`). A separate React frontend consumes it (CORS policy `PermitirFrontend` only allows the origins listed in `Cors:Origenes`).

The codebase is written in Spanish — identifiers, comments, exception messages, and commit messages. Keep new code in Spanish and follow the existing naming (`Servicio`, `Repositorio`, `ObtenerPorIdAsync`, `CrearAsync`, `ActualizarAsync`, `EliminarAsync`, etc.).

## Commands

Run from the repo root (solution file is `SonDriveBackend.slnx`):

```bash
dotnet build SonDriveBackend.slnx
dotnet test SonDriveBackend.slnx             # Biblioteca.Pruebas + API.Pruebas; no SQL Server needed
dotnet run --project API                     # http://localhost:5064, Swagger UI at /swagger (Development only)
dotnet run --project API --launch-profile https   # https://localhost:7112

# EF Core migrations — DbContext lives in Biblioteca, startup/config in API
dotnet ef migrations add <Nombre> --project Biblioteca --startup-project API
dotnet ef database update --project Biblioteca --startup-project API
```

`API/API.http` has sample requests (add one for every new or changed endpoint).

### Tests

Two xUnit projects, both in the solution; they never touch the local SQL Server, the network or a specific clock value:

- **`Biblioteca.Pruebas`** — service tests against the real repositories over **SQLite in memory** (`Infraestructura/BaseDatosEnMemoria`: opens a `:memory:` connection, `EnsureCreated()`, seeds users with BCrypt). Doubles are hand-written (`GeneradorTokenFalso`); no mocking library. Inside this project the namespace `Biblioteca.Conexion` shadows the `Conexion` class, so use an alias (`using ConexionSonDrive = Biblioteca.Conexion.Conexion;`) after the file-scoped namespace.
- **`API.Pruebas`** — HTTP tests through `Infraestructura/FabricaApiSonDrive` (`WebApplicationFactory<Program>`, hence `public partial class Program;` at the end of `Program.cs`). It runs in environment `Pruebas`, sets `Jwt:*` and `Cors:Origenes` with `UseSetting` (applied before `Program.cs` reads them), and swaps SQL Server for SQLite: it must remove **both** `DbContextOptions<Conexion>` and `IDbContextOptionsConfiguration<Conexion>` (EF Core 9+ otherwise registers two providers). The schema is created in the factory constructor because `CrearAdministradorInicialAsync` queries users before `app.Run()`. Use `SembrarUsuarioAsync` and `CrearClienteAutenticadoAsync`; give each test a unique e-mail since a factory is shared per test class.
- Naming: `Metodo_Escenario_ResultadoEsperado`, in Spanish. Constitution principle X requires tests for every service/validator/endpoint a module creates or touches, including the role access matrix (anonymous, Conductor, Administrador).

The connection string `ConnectionStrings:string_conexion` in `API/appsettings.json` points to a developer's local SQL Server instance (Windows auth). Override it locally (user secrets are enabled on the API project) rather than editing the committed value.

## Architecture

Two projects:

- **`Biblioteca`** (class library) — all domain and data logic: `Modelo` (EF entities), `Conexion` (DbContext + `IConexion`), `Repositorios`, `Servicios`, `DTO`, `Mapeo` (AutoMapper profiles), `Excepciones`, `Migrations`.
- **`API`** (web host) — `Controllers`, `Validaciones` (FluentValidation validators), `Middleware`, and DI wiring in `Program.cs`.

Request flow: **Controller → Servicio → Repositorio → `IConexion` (EF Core / SQL Server)**.

Key conventions spanning files:

- **Data access goes through `IConexion`**, not `Conexion` directly. It exposes the `DbSet`s and `SaveChangesAsync`; `Program.cs` registers it as the same scoped `Conexion` instance. Adding an entity means adding the `DbSet` to both `Conexion` and `IConexion`, plus Fluent API config in `Conexion.OnModelCreating` (all model configuration — tables, keys, lengths, indexes, delete behaviors, seed data — lives there, not in attributes).
- **Every new service/repository must be registered manually** with `AddScoped` in `API/Program.cs`. Validators and AutoMapper profiles are picked up automatically by assembly scanning (validators from the API assembly, profiles from the Biblioteca assembly).
- **Validation happens in controllers**: controllers inject `IValidator<TDto>`, call `ValidateAsync`, and return a 400 `ValidationProblemDetails` via a private `ProblemaDeValidacion` helper. There is no automatic validation filter.
- **Every error response is ProblemDetails**: `AddProblemDetails()` + `app.UseStatusCodePages()` give a `application/problem+json` body to responses that would otherwise be empty (the 401/403 from the authorization pipeline).
- **Errors are signaled with exceptions** from `Biblioteca/Excepciones`: services throw `NoEncontradoException` (→ 404), `ReglaNegocioException` (→ 409) or `CredencialesInvalidasException` (→ 401); `ManejoExcepcionesMiddleware` converts them to ProblemDetails. Anything else becomes a logged 500. Controllers therefore don't try/catch — they just return `Ok`/`CreatedAtAction`/`NoContent`. Services typically use an `ObtenerOFallarAsync` helper that throws `NoEncontradoException`.
- **Mapping** is mixed: Usuario, Vehiculo and Falla use AutoMapper profiles in `Biblioteca/Mapeo`; Viaje and Ocupacion build DTOs by hand inside the service (`AResumen`/`AResponse`).
- Async methods take a `CancellationToken ct` that controllers pass through from the action.
- Repositories use `AsNoTracking()` for reads; detail queries use `AsSplitQuery()` with deep `Include`/`ThenInclude` chains.

### Seat model (Puesto / Ocupacion)

- Seats belong to the **vehicle**, not the trip. `MapaPuestos.Estandar` defines the standard layout (Conductor, Copiloto, Izquierda, Centro, Derecha + trunk slots C1–C4 of type `Cajuela`). `PuestoServicio.AsegurarMapaAsync` lazily creates any missing seats for a vehicle (and the `CodigoPuesto`/`TipoPuesto` lookup rows) when a trip is created/updated.
- A trip's seat availability is derived from its `Ocupaciones`: unique index on `(IdViaje, IdPuesto)`. The driver seat is always reported as occupied and cannot be booked; `CuposLibres` excludes driver and trunk slots, `CajuelasLibres` counts trunk slots.
- Ocupacion endpoints are nested: `api/viaje/{idViaje}/ocupaciones/puesto/{idPuesto}`.

### Other domain notes

- `Viaje.FechaSalida` holds date + time combined (the old `Hora` entity was removed); DTOs receive `Fecha` and `Hora` separately and the service combines them. A vehicle can't have two trips at the same `FechaSalida`. Trip searches only return future trips.
- Cities and routes (Sonsón → La Unión / Rionegro / La Ceja / Medellín) are seeded via `HasData` in `Conexion`.
- Passwords are hashed with BCrypt in `UsuarioServicio`.

### Authentication / authorization (JWT)

- Login: `POST api/autenticacion/iniciar-sesion` (`InicioSesionDTO` → `InicioSesionRespuestaDTO` with token + user). `AutenticacionServicio` (Biblioteca) checks credentials and `Estado`, throwing `CredencialesInvalidasException` (→ 401 in the middleware). Token creation is behind `IGeneradorToken` (Biblioteca) implemented by `API/Seguridad/GeneradorTokenJwt`, so Biblioteca has no JWT dependency.
- **A fallback policy requires a valid token on every endpoint.** Public endpoints need `[AllowAnonymous]`; role-restricted ones use `[Authorize(Roles = Roles.Administrador)]`. Role names live in `Biblioteca/Seguridad/Roles` (`Administrador`, `Conductor`) — never hardcode the strings.
- Claims use short names (`MapInboundClaims = false`): `sub` (user id), `email`, `nombre`, `rol` — see `API/Seguridad/NombresClaims`. In controllers use `User.ObtenerIdUsuario()`, `User.EsAdministrador()`, `User.PuedeAccederAUsuario(id)` from `ExtensionesUsuarioActual`.
- CORS: `Cors:Origenes` (string array) feeds `WithOrigins`; an empty list allows no origin. Development allows `http://localhost:5173` (Vite); other environments must configure it. `AllowAnyOrigin()` is not allowed (constitution principle IV).
- Config: section `Jwt` (`Clave` ≥ 32 chars, `Emisor`, `Audiencia`, `MinutosExpiracion`); the app refuses to start without `Jwt:Clave`. The dev key lives in `appsettings.Development.json`; other environments must supply `Jwt__Clave` via env var/user secrets.
- On startup, `CrearAdministradorInicialAsync` creates an admin from the `AdministradorInicial` config section if no user with role `Administrador` exists.
- Swagger UI has an "Authorize" button (Bearer scheme added by `TransformadorSeguridadOpenApi`).
- Endpoints that take a user id must start with `if (!User.PuedeAccederAUsuario(id)) return Forbid();` (owner or admin only) — see `UsuarioController`, `VehiculoController.ListarPorUsuario`, `ViajeController.ListarPorUsuario`.
- `docs/autenticacion.md` is the long-form (Spanish) explanation of the auth design; keep it in sync when changing auth behavior.
