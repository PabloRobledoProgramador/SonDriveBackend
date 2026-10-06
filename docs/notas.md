  Archivos                                                                                                │ 14 +    <PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" V
                                                                                                          │    +ersion="10.0.12" />
  - Nuevos en Biblioteca: Seguridad/Roles.cs, DTO/InicioSesion/ (reemplaza el DTO/Login/LoginDTO.cs que   │ 15      <PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.12"
    estaba vacío), IAutenticacionServicio, AutenticacionServicio, IGeneradorToken y la excepción          │     />
    CredencialesInvalidasException.                                                                       │ 16      <PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="1
  - Nuevos en API: Controllers/AutenticacionController.cs, Validaciones/InicioSesion/ y la carpeta        │     0.0.12">
    Seguridad/ (generador del token, configuración, nombres de los datos del token, utilidades para leer  │ 17        <PrivateAssets>all</PrivateAssets>
    el usuario actual, inicializador del administrador y botón de Swagger).                               │ 18
  - Modificados: Program.cs, el middleware de excepciones, UsuarioController, los dos appsettings,        │
    API.http y CLAUDE.md. También agregué el paquete Microsoft.AspNetCore.Authentication.JwtBearer.       │────────────────────────────────────────────────────────────────────────────────────
                                                                                                          │API/API.http                                                                 [ ask ]
  A tener en cuenta                                                                                       │────────────────────────────────────────────────────────────────────────────────────
                                                                                                          │  1  @API_HostAddress = http://localhost:5064
  1. El frontend tiene que enviar el token. Hasta que lo haga, todas sus peticiones van a recibir 401.    │  2
  2. En producción hay que cambiar dos cosas: la clave del token (variable de entorno Jwt__Clave) y la    │  3 -GET {{API_HostAddress}}/weatherforecast/
     clave del administrador inicial. Las de appsettings.Development.json solo sirven para desarrollo. Si │  4 -Accept: application/json
     Jwt:Clave no está configurada, la API no arranca.                                                    │  3 +# 1. Iniciar sesión y copiar el "token" de la respuesta en @token
  3. Quedan rutas sin control de dueño. api/vehiculo/usuario/{id} y api/viaje/usuario/{id} todavía dejan  │  4 +POST {{API_HostAddress}}/api/autenticacion/iniciar-sesion
     que un conductor consulte datos de otro. Conviene arreglarlo cuando definas los permisos del         │  5 +Content-Type: application/json
     Conductor.                                                                                           │  6 +
  4. Un usuario desactivado sigue entrando un rato. Su token sigue siendo válido hasta que vence, como    │  7 +{
     máximo 8 horas.                                                                                      │  8 +  "correo": "admin@sondrive.com",
