# SonDrive

Plataforma web de viajes compartidos intermunicipales alrededor de Sonsón (Antioquia). Los
pasajeros consultan salidas y disponibilidad sin cuenta; los conductores gestionan sus viajes y
la administradora controla conductores y flota.

Este repositorio (`SonDriveBackend`) contiene la API (ASP.NET Core, .NET 10) y la configuración
de Docker que levanta **todo el proyecto**: base de datos, API y frontend. El frontend vive en el
repositorio [`SonDriveFrontend`](https://github.com/PabloRobledoProgramador/SonDriveFrontend).

---

## 1. Levantar el proyecto con Docker

### Requisitos

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) abierto (en Windows, con WSL 2).
- Los dos repositorios clonados **uno al lado del otro**:

```text
SonDrive/
├─ SonDriveBackend/     ← este repositorio (aquí está docker-compose.yml)
└─ SonDriveFrontend/
```

```bash
git clone https://github.com/PabloRobledoProgramador/SonDriveBackend.git
git clone https://github.com/PabloRobledoProgramador/SonDriveFrontend.git
```

### Primera vez

```bash
cd SonDriveBackend
cp .env.example .env          # en PowerShell: Copy-Item .env.example .env
```

Abre `.env` y cambia `SQL_CLAVE_SA` por una clave **tuya**: mínimo 8 caracteres, con mayúsculas,
minúsculas, números y símbolos, y sin `;`. Este archivo no se sube al repositorio: cada
desarrollador tiene su propio servidor y su propia clave.

### Levantar todo

```bash
docker compose up --build
```

Con un solo comando se levanta, en este orden:

| Servicio | Qué hace | Dirección en tu equipo |
|---|---|---|
| `db` | SQL Server 2022 (Developer) | `localhost,14330` |
| `migraciones` | Crea o actualiza la base `SonDriveDB` y termina | — |
| `api` | API en modo Development | http://localhost:5064 · Swagger: http://localhost:5064/swagger |
| `frontend` | Sitio web con recarga en caliente | http://localhost:5173 |

La primera vez tarda varios minutos porque descarga las imágenes (unos 2 GB). La base tarda
alrededor de 1–2 minutos en estar lista; la API espera a que las migraciones terminen.

Para dejarlo en segundo plano: `docker compose up --build -d`.

### Usuario de prueba

Al arrancar, la API crea el **administrador inicial** si no existe ninguno:

| Correo | Contraseña |
|---|---|
| `admin@sondrive.com` | `Admin12345*` |

Para probar como **Conductor**, créalo desde Swagger:

1. `POST /api/autenticacion/iniciar-sesion` con el administrador y copia el `token`.
2. Pulsa **Authorize**, pega el token y acepta.
3. `POST /api/usuario` con `"rol": "Conductor"`.
4. Para probar una cuenta desactivada: `PATCH /api/usuario/{id}/estado`.

### Comandos del día a día

| Acción | Comando |
|---|---|
| Levantar (reconstruyendo si cambió el código del backend) | `docker compose up --build` |
| Ver el estado | `docker compose ps -a` |
| Ver los logs de un servicio | `docker compose logs -f api` (o `db`, `frontend`, `migraciones`) |
| Detener todo (los datos se conservan) | `docker compose down` |
| Reconstruir tras cambiar `package.json` del frontend | `docker compose up --build -V` |

### Dónde se guarda la base de datos

Los archivos de la base (`SonDriveDB.mdf`, `SonDriveDB_log.ldf` y las bases del sistema) quedan
en la carpeta **`db/`** de este repositorio. Esa carpeta está ignorada por git.

- Los datos **se conservan** al hacer `docker compose down` / `up`, al reiniciar Docker o el equipo.
- Para **empezar de cero**: `docker compose down` y luego borra la carpeta `db/`.
- Las migraciones **no** se generan solas: el servicio `migraciones` solo aplica las que ya existen
  en `Biblioteca/Migrations` y que la base aún no tiene.

Para consultar la base con SSMS, Azure Data Studio o VS Code (extensión SQL Server):

| Campo | Valor |
|---|---|
| Servidor | `localhost,14330` |
| Usuario | `sa` |
| Contraseña | la `SQL_CLAVE_SA` de tu `.env` |
| Opciones | marcar *Trust server certificate* |

Copia de seguridad (queda en `db/`):

```bash
docker exec sondrive-db-1 /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "<tu clave>" -C \
  -Q "BACKUP DATABASE SonDriveDB TO DISK='/var/opt/mssql/data/SonDriveDB.bak'"
```

> En Git Bash antepón `MSYS_NO_PATHCONV=1` a los comandos `docker exec` con rutas de Linux.

### Configuración opcional (`.env`)

| Variable | Para qué |
|---|---|
| `SQL_PUERTO`, `API_PUERTO`, `FRONTEND_PUERTO` | Cambiar los puertos si ya están ocupados (CORS se ajusta solo) |
| `CADENA_CONEXION` | Usar tu propio SQL Server instalado en lugar del contenedor (desde Docker tu equipo es `host.docker.internal`; usa usuario y clave, no autenticación de Windows) |
| `RUTA_FRONTEND` | Ubicación del repositorio del frontend si no está junto a este |

Las credenciales de `appsettings*.json` **no** se usan ni se modifican: Docker reemplaza la
cadena de conexión con variables de entorno.

### Solución de problemas

| Síntoma | Qué hacer |
|---|---|
| `port is already allocated` | Cambia el puerto en `.env` (por ejemplo `FRONTEND_PUERTO=5174`) |
| `Falta SQL_CLAVE_SA en .env` | Copia `.env.example` como `.env` y define la clave |
| `db` nunca llega a *healthy* y `docker compose logs db` se queda en "running as user mssql" | Reinicia Docker Desktop (o `wsl --shutdown` y ábrelo de nuevo) |
| Las migraciones fallan con *Login failed for user 'sa'* después de cambiar la clave | La clave quedó fijada al crear la base: vuelve a poner la anterior o borra `db/` (se pierden los datos) |
| El frontend no refleja cambios del código | Ya usa sondeo de cambios; si persiste: `docker compose restart frontend` |

---

## 2. Ejecutar sin Docker

```bash
dotnet run --project API          # http://localhost:5064 (Swagger en /swagger)
```

Necesita SQL Server local y aplicar las migraciones:

```bash
dotnet ef database update --project Biblioteca --startup-project API
```

La cadena `ConnectionStrings:string_conexion` de `API/appsettings.json` apunta al servidor de un
integrante del equipo; **no la edites**: sobrescríbela con *user secrets*:

```bash
dotnet user-secrets set "ConnectionStrings:string_conexion" "Server=...;Database=SonDriveDB;..." --project API
```

El frontend se ejecuta aparte con `npm run dev` (ver el README de `SonDriveFrontend`).

---

## 3. Pruebas automáticas

Ninguna prueba necesita Docker, SQL Server, la API corriendo ni conexión a internet. Antes de
cerrar una tarea o un módulo deben pasar todas (definición de terminado de la constitución).

### Backend (xUnit)

Requisito: .NET SDK 10. Desde esta carpeta:

```bash
dotnet test SonDriveBackend.slnx      # todas (~30 s)
dotnet test API.Pruebas               # solo pruebas de la API
dotnet test Biblioteca.Pruebas        # solo pruebas de servicios
```

Ejecutar solo una parte:

```bash
dotnet test API.Pruebas --filter "FullyQualifiedName~AutenticacionControllerPruebas"   # una clase
dotnet test API.Pruebas --filter "DisplayName~Perfil"                                  # nombre que contiene "Perfil"
dotnet test API.Pruebas --filter "DisplayName~IniciarSesion_ClaveErrada_Devuelve401ConMismoDetalle"   # una prueba
dotnet test SonDriveBackend.slnx --logger "console;verbosity=normal"                   # ver cada prueba
```

> xUnit no admite `Name` en `--filter`: usa `DisplayName` o `FullyQualifiedName` (con `~` = contiene).

En **Visual Studio**: *Prueba → Explorador de pruebas* (`Ctrl+E, T`), donde se puede ejecutar o
depurar cada prueba. En **VS Code**: extensión C# Dev Kit, vista *Testing*.

| Proyecto | Clase | Qué prueba |
|---|---|---|
| `Biblioteca.Pruebas` | `Servicios/AutenticacionServicioPruebas` | Inicio de sesión: credenciales correctas, correo con mayúsculas, correo inexistente, clave errada, usuario desactivado |
| `API.Pruebas` | `Controladores/AutenticacionControllerPruebas` | `iniciar-sesion` (200/400/401 con Conductor y Administrador) y `perfil` (sin token, token válido, token falso) |
| `API.Pruebas` | `Validaciones/InicioSesionDtoValidadorPruebas` | Mensajes del validador de inicio de sesión |
| `API.Pruebas` | `Seguridad/CorsPruebas` | Orígenes permitidos y rechazados por CORS |

Cómo están hechas:

- **`Biblioteca.Pruebas`** usa los repositorios reales sobre **SQLite en memoria**
  (`Infraestructura/BaseDatosEnMemoria`) y dobles escritos a mano (`GeneradorTokenFalso`).
- **`API.Pruebas`** levanta la API real en memoria con `Infraestructura/FabricaApiSonDrive`
  (`WebApplicationFactory`), cambiando SQL Server por SQLite. Para datos usa
  `SembrarUsuarioAsync`, y para llamar con sesión, `CrearClienteAutenticadoAsync`.

Al escribir pruebas nuevas:

- Nombre en español con el patrón `Metodo_Escenario_ResultadoEsperado`.
- En `API.Pruebas` la base se comparte dentro de cada clase: usa un correo distinto por prueba
  (`CorreoUnico(...)`).
- Todo servicio, validador o endpoint que se cree o modifique debe tener sus pruebas, incluida la
  matriz de acceso por rol: anónimo, Conductor y Administrador (principio X de la constitución).

### Frontend (Vitest)

Requisito: Node.js 22 o superior. Desde `SonDriveFrontend`:

```bash
npm install
npm run test                                    # todas (~30 s; la primera vez ~60 s)
npx vitest                                      # modo observación: repite al guardar cambios
npx vitest run src/features/autenticacion       # solo una carpeta
npx vitest run -t "doble clic"                  # solo las pruebas cuyo nombre contenga el texto
```

También se pueden ejecutar dentro del contenedor, sin instalar Node:
`docker compose exec frontend npm run test`.

Las pruebas usan jsdom, Testing Library y **MSW** para simular la API (`src/pruebas/`); viven
junto al archivo que prueban (`Nombre.test.tsx`). Más detalles en el README de `SonDriveFrontend`.

### Comprobación completa antes de entregar

```bash
# Backend
dotnet build SonDriveBackend.slnx
dotnet test SonDriveBackend.slnx

# Frontend
cd ../SonDriveFrontend
npm run build
npm run lint
npm run test
```

---

## 4. Pruebas manuales

Con el proyecto levantado en Docker, recorre en Chrome:

1. Modo dispositivo de DevTools a **320, 360, 768 y 1280 px** en `/`, `/iniciar-sesion` y una
   dirección inexistente: sin desplazamiento horizontal, textos cortados ni elementos superpuestos.
2. "Iniciar sesión" → "Entrar" con los campos vacíos: mensajes bajo cada campo.
3. Clave errada: "Correo o clave incorrectos." y el correo se conserva.
4. Entrar con el administrador: la cabecera muestra el nombre y "Cerrar sesión".
5. Recargar y abrir otra pestaña: la sesión sigue.
6. Ir a `/iniciar-sesion` con sesión: lleva al inicio.
7. "Cerrar sesión": vuelve a visitante, también en la otra pestaña.
8. DevTools → Application → `sondrive.sesion`: poner `expiracion` en una fecha pasada y recargar:
   queda como visitante con el aviso "Tu sesión venció. Inicia sesión de nuevo.".
9. Detener la API (`docker compose stop api`) e intentar entrar: mensaje de conexión.
10. Con un Conductor desactivado: "El usuario está desactivado. Contacta al administrador.".

La lista completa del módulo está en `specs/001-base-proyecto/quickstart.md` (carpeta de
especificaciones del proyecto).

---

## Más documentación

- `CLAUDE.md` — arquitectura, convenciones del código y detalles de Docker y de las pruebas.
- `docs/autenticacion.md` — autenticación, roles, CORS y uso desde el frontend.
- `API/API.http` — peticiones de ejemplo.
