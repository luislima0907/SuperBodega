# Etapa de construcción
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar el archivo de proyecto y restaurar dependencias
COPY ["SuperBodega.API/SuperBodega.API.csproj", "SuperBodega.API/"]
RUN dotnet restore "SuperBodega.API/SuperBodega.API.csproj"

# Copiar el resto de archivos y compilar
COPY . .
WORKDIR "/src/SuperBodega.API"
RUN dotnet build "SuperBodega.API.csproj" -c Release -o /app/build
RUN dotnet publish "SuperBodega.API.csproj" -c Release -o /app/publish

# Imagen final
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Variable para forzar actualización de caché
ENV CACHE_BUST=timestamp_value

# Copiar los archivos publicados
COPY --from=build /app/publish .

# Copiar las vistas y archivos estáticos explícitamente
COPY ["SuperBodega.API/Views", "/app/Views/"]
COPY ["SuperBodega.API/wwwroot", "/app/wwwroot/"]

#Crear la carpeta para las imágenes si no existe
RUN mkdir -p /app/wwwroot/images/productos
#Dar permisos de escritura a la carpeta
RUN chmod -R 777 /app/wwwroot/images

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT [ "dotnet", "SuperBodega.API.dll" ]