# --- Etapa 1: Construcción (Build) ---
# CAMBIO: Usamos el SDK de .NET 9.0 (Preview)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

# Copia los archivos de solución (.sln) y de proyecto (.csproj)
COPY *.sln .
COPY *.csproj .

# Restaura los paquetes NuGet
RUN dotnet restore

# Copia todo el resto del código (incluyendo la carpeta 'src')
COPY . .

# --- ARREGLO AQUÍ ---
# 1. NO nos movemos a /app/src
# 2. Le decimos a 'publish' DÓNDE está el proyecto
RUN dotnet publish "backend.csproj" -c Release -o /app/publish

# --- Etapa 2: Imagen Final (Runtime) ---
# CAMBIO: Usamos el runtime de ASP.NET 9.0 (Preview)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Expone el puerto 8080
EXPOSE 8080

# Comando para iniciar tu app
ENTRYPOINT ["dotnet", "backend.dll"]