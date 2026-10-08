# Consulte https://aka.ms/customizecontainer para aprender a personalizar su contenedor de depuración y cómo Visual Studio usa este Dockerfile para compilar sus imágenes para una depuración más rápida.

# Esta fase se usa cuando se ejecuta desde VS en modo rápido (valor predeterminado para la configuración de depuración)
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081


# Esta fase se usa para compilar el proyecto de servicio
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["API/API.csproj", "API/"]
RUN dotnet restore "./API/API.csproj"
COPY . .
WORKDIR "/src/API"
RUN dotnet build "./API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Esta fase se usa para publicar el proyecto de servicio que se copiará en la fase final.
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Esta fase genera el ejecutable de migraciones de EF Core (efbundle). La usa docker-compose.yml
# para crear o actualizar la base de datos antes de arrancar la API. Va antes de "final" para que
# "final" siga siendo la fase por defecto.
FROM build AS migraciones-build
ARG BUILD_CONFIGURATION=Release
ENV PATH="${PATH}:/root/.dotnet/tools"
RUN dotnet tool install --global dotnet-ef --version 10.0.12
WORKDIR /src
RUN dotnet ef migrations bundle --project Biblioteca/Biblioteca.csproj --startup-project API/API.csproj \
    --configuration $BUILD_CONFIGURATION --output /app/efbundle --force

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS migraciones
WORKDIR /app
COPY --from=migraciones-build /app/efbundle .
ENTRYPOINT ["./efbundle"]

# Esta fase se usa en producción o cuando se ejecuta desde VS en modo normal (valor predeterminado cuando no se usa la configuración de depuración)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "API.dll"]