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

### Docker (whole stack with one command)

`docker-compose.yml` (in this repo) starts SQL Server 2022, applies migrations, and runs the API and the frontend dev server. It expects `SonDriveFrontend` cloned next to this repo (override with `RUTA_FRONTEND`).

```bash
cp .env.example .env        # once; set your own SQL_CLAVE_SA (never committed)
docker compose up --build   # db :14330, API :5064 (Swagger at /swagger), frontend :5173
docker compose down         # stops everything; the data stays in db/
```

- The database files (`.mdf`/`.ldf`, including system databases) live in **`db/`** in this repo (bind mount on `/var/opt/mssql/data`, git- and docker-ignored, like the innovacion-curricular project). Data survives `down`/`up`; delete `db/` to start from an empty database.
- The `db` healthcheck only passes when every database can actually be opened (`DATABASEPROPERTYEX(name,'Collation')` is NULL while one is still recovering): after a restart `SonDriveDB` recovers a few seconds after `master`, and the migrations would otherwise fail to log in. Because `db/` is a bind mount, on Windows a cold start can take over 6 minutes before every database is recovered, so `start_period` is 10 minutes (failures during it don't count; the container turns healthy as soon as the check passes). Don't shorten it, or `docker compose up` fails with "container sondrive-db-1 is unhealthy".
- Each developer runs their own database container; credentials live only in the local `.env`. The committed `appsettings*.json` (including `string_conexion`) are **not** used for the connection: compose overrides `ConnectionStrings__string_conexion` (or `CADENA_CONEXION` to point at your own SQL Server via `host.docker.internal`).
- Migrations run in the `migraciones` service: an EF Core bundle built by the `migraciones` stage of `API/Dockerfile` (placed before `final`, so Visual Studio's default target is unchanged). The bundle runs `Program.cs` up to building the host, so it gets a throwaway `Jwt__Clave`.
- The API runs as `Development` (dev JWT key and `AdministradorInicial` from `appsettings.Development.json`); `Cors__Origenes__0` follows `FRONTEND_PUERTO`.
- After changing `package.json` in the frontend, rebuild with `docker compose up --build -V` so the container's `node_modules` volume is recreated.

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
- **Mapping** is mixed: Usuario and Falla use AutoMapper profiles in `Biblioteca/Mapeo`; Viaje, Ocupacion and Ruta build DTOs by hand inside the service (`AResumen`/`AResponse`/`ADto`). Vehiculo does both — AutoMapper for DTO → entity, a hand-written `Mapear` for the response because it computes fields — which the constitution (principle II) forbids within one aggregate; a known deviation, not a pattern to copy.
- **The clock is read directly** (`DateTime.Now`/`DateTime.Today`) in `ViajeRepositorio` (future-trip filters), `OcupacionServicio`, `VehiculoServicio.Mapear` and the Viaje/Vehiculo/Falla validators; nothing injects `TimeProvider`. Tests for that code must therefore use dates relative to "now" (or introduce the injection first) to stay deterministic.
- Async methods take a `CancellationToken ct` that controllers pass through from the action.
- Repositories use `AsNoTracking()` for reads; detail queries use `AsSplitQuery()` with deep `Include`/`ThenInclude` chains.

### Seat model (Puesto / Ocupacion)

- Seats belong to the **vehicle**, not the trip. `MapaPuestos.Estandar` defines the standard layout (Conductor, Copiloto, Izquierda, Centro, Derecha + trunk slots C1–C4 of type `Cajuela`). `PuestoServicio.AsegurarMapaAsync` creates any missing seats for a vehicle (and the `CodigoPuesto`/`TipoPuesto` lookup rows); it runs when the vehicle is created and again when a trip is created/updated, so older vehicles get their map lazily.
- A trip's seat availability is derived from its `Ocupaciones`: unique index on `(IdViaje, IdPuesto)`. The driver seat is always reported as occupied and cannot be booked; `CuposLibres` excludes driver and trunk slots, `CajuelasLibres` counts trunk slots.
- Ocupacion endpoints are nested: `api/viaje/{idViaje}/ocupaciones/puesto/{idPuesto}` (POST occupies, PUT edits the passenger without freeing the seat, DELETE frees it). Seats can't be changed once the trip has departed, and a trip can't switch vehicle while it has occupied seats.
- Seat data is exposed in two tiers: the anonymous `GET api/viaje/{id}` (`ViajeResponseDTO`) lists `Puestos` with only `Ocupado`/`EsConductor` plus the driver's phone; passenger data (`PasajeroOcupacionDTO`: name, phone, address) is returned only by `GET api/viaje/{idViaje}/ocupaciones`, which requires a token. Keep passenger data out of the public DTO.

### Other domain notes

- `Viaje.FechaSalida` holds date + time combined (the old `Hora` entity was removed); DTOs receive `Fecha` and `Hora` separately and the service combines them. A vehicle can't have two trips at the same `FechaSalida`. Trip searches only return future trips.
- Cities and routes are seeded via `HasData` in `Conexion`; changing them needs a migration. Only the four routes **from** Sonsón (→ La Unión / Rionegro / La Ceja / Medellín) exist — there are no return routes — each with `DistanciaKm`, `TiempoEstimado` (free text) and `PrecioBase`. `CiudadController` and `RutaController` are read-only lists.
- Vehicles: the plate is normalized (no spaces, upper case) and unique; the owner must be an active `Conductor`; a vehicle with trips can't be deleted. Editing (`PUT api/vehiculo/{id}`, admin only, the same ten fields as creation) validates the driver only when `IdUsuario` changes, so a vehicle whose driver was disabled can still be corrected. A `Viaje` does not store its driver — it comes from `Viaje.Vehiculo.Usuario` — so reassigning a vehicle moves its upcoming trips (with their occupied seats) to the new driver; no extra code does that. `VehiculoResponseDTO.Estado` (`Crítico`/`Alerta`/`Óptimo`) is derived only from the severity of unresolved `Falla`s, and each document (`Soat`, `Tecnomecanica`, `Licencia`) carries `DiasRestantes`; the frontend combines both in `vehiculos/estadoVehiculo.ts`, so the two "states" are not the same thing. A resolved fault must be reopened (`PATCH api/falla/{id}/resuelta?resuelta=false`) before it can be edited.
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
- Anonymous endpoints today: `POST api/autenticacion/iniciar-sesion`, `POST api/usuario/registro`, `GET api/viaje` (search), `GET api/viaje/{id}`, `GET api/ciudad`, `GET api/ruta`. `GET api/autenticacion/perfil` returns the current user from the token.
- Editing a driver: `PUT api/usuario/{id}/conductor` (admin only, `ConductorActualizarDto`: `Nombre`, `Apellido`, `Correo`, `Celular`, `Foto`). The DTO has no `Estado`, `Rol` or `Clave`, so the edit can't change them; the action trims the texts before validating, and `UsuarioServicio.ActualizarConductorAsync` rejects non-`Conductor` accounts and e-mails used by another user (both 409). This is the endpoint the frontend uses — not `PUT api/usuario/{id}`.
- The photo rule (optional; if present, absolute `http`/`https` URL, max 500) lives in `API/Validaciones/Usuario/ValidacionFoto` and is shared by `UsuarioCrearDtoValidador` and `ConductorActualizarDtoValidador`; keep new user validators on it.
- Public sign-up: `POST api/usuario/registro` reuses `UsuarioCrearDto` and its validator but forces `Rol = Conductor`, `Estado = true`, `Foto = null` before validating, whatever the body says. `POST api/usuario` (any role) stays admin-only.

### Known gaps against the constitution

Existing deviations — don't copy them and don't "fix" them in passing; raise them, since the resolution is a team decision:

- Public sign-up is allowed by principle IV (constitution v1.5.0: always `Conductor`, never an administrator), and documented in `docs/autenticacion.md`, but it has no sample request in `API/API.http`.
- `VehiculoController` (except `Actualizar`, which is admin-only, and `ListarPorUsuario`), `FallaController`, `OcupacionController` and the `ViajeController` writes have no role or ownership check: any valid token — including a self-registered driver — can create or delete any vehicle and manage any fault, trip or passenger list. In particular `POST api/viaje` doesn't check that the vehicle belongs to the caller, so a driver can still publish a trip with a vehicle that was reassigned to someone else. The matrix says vehicles are admin-only and trips/seat sheets are owner-or-admin.
- `GET api/usuario` is admin-only and returns the full `UsuarioDto`; the public driver directory of the matrix has no endpoint or public DTO yet (`DTO/Conductor/ConductorResponseDTO` exists).
- `PUT api/usuario/{id}` (`UsuarioActualizarDto`) is risky and unused by the frontend: `Estado` is a non-nullable `bool`, so a body without it disables the user; a `Rol` in the body is applied, so it can turn a driver into an administrator; it works on any user and has no tests. Prefer `PUT api/usuario/{id}/conductor`.
- Tests exist only for authentication (`AutenticacionServicioPruebas`, `AutenticacionControllerPruebas`, `InicioSesionDtoValidadorPruebas`), sign-up (`UsuarioRegistroControllerPruebas`), driver editing (`UsuarioServicioPruebas` — only `ActualizarConductorAsync` —, `UsuarioConductorControllerPruebas`, `ConductorActualizarDtoValidadorPruebas`), the photo rule of user creation (`UsuarioCrearDtoValidadorPruebas`), vehicle editing (`VehiculoServicioPruebas` — only `ActualizarAsync` —, `VehiculoEdicionControllerPruebas` — only the `PUT` —, `VehiculoDatosDtoValidatorPruebas`) and CORS (`CorsPruebas`). `BaseDatosEnMemoria` also has `AgregarVehiculoAsync` and `AgregarFallaAsync`. Every other service, validator and endpoint is untested; principle X requires adding those tests when a module touches them.
- SignalR (`DisponibilidadHub`, `INotificadorDisponibilidad`, principle IX) is not built.
