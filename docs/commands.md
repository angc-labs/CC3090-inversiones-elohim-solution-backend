# Manual de comandos utiles para la API .NET

Este manual esta pensado para ejecutarse desde la carpeta `backend/` del proyecto.

## 1. Diagnostico inicial

- Ver informacion del entorno .NET:

```powershell
dotnet --info
```

- Ver version activa del SDK:

```powershell
dotnet --version
```

- Listar SDKs instalados:

```powershell
dotnet --list-sdks
```

- Listar runtimes instalados:

```powershell
dotnet --list-runtimes
```

## 2. Restaurar y compilar

- Restaurar paquetes NuGet de la solucion:

```powershell
dotnet restore ElohimShop.slnx
```

- Compilar toda la solucion:

```powershell
dotnet build ElohimShop.slnx
```

- Compilar en modo Release:

```powershell
dotnet build ElohimShop.slnx -c Release
```

- Limpiar artefactos de compilacion:

```powershell
dotnet clean ElohimShop.slnx
```

## 3. Ejecutar la API

- Correr la API:

```powershell
dotnet run --project src/ElohimShop.API/ElohimShop.API.csproj
```

- Correr API en entorno Development:

```powershell
$env:ASPNETCORE_ENVIRONMENT="Development"
dotnet run --project src/ElohimShop.API/ElohimShop.API.csproj
```

- Hot reload en desarrollo:

```powershell
dotnet watch --project src/ElohimShop.API/ElohimShop.API.csproj run
```

## 4. Pruebas

- Ejecutar pruebas de toda la solucion:

```powershell
dotnet test ElohimShop.slnx
```

- Ejecutar pruebas con salida detallada:

```powershell
dotnet test ElohimShop.slnx -v normal
```

- Ejecutar pruebas filtrando por nombre:

```powershell
dotnet test ElohimShop.slnx --filter "FullyQualifiedName~NombreDelTest"
```

## 5. Entity Framework Core (migraciones)

- Crear una migracion nueva:

```powershell
dotnet ef migrations add NombreDescriptivo --project src/ElohimShop.Infrastructure --startup-project src/ElohimShop.API
```

- Aplicar migraciones a la base de datos:

```powershell
dotnet ef database update --project src/ElohimShop.Infrastructure --startup-project src/ElohimShop.API
```

- Listar migraciones:

```powershell
dotnet ef migrations list --project src/ElohimShop.Infrastructure --startup-project src/ElohimShop.API
```

- Eliminar la ultima migracion (si aun no fue aplicada):

```powershell
dotnet ef migrations remove --project src/ElohimShop.Infrastructure --startup-project src/ElohimShop.API
```

- Generar script SQL de migraciones:

```powershell
dotnet ef migrations script --project src/ElohimShop.Infrastructure --startup-project src/ElohimShop.API
```

## 6. Gestion de paquetes NuGet

- Agregar un paquete a un proyecto:

```powershell
dotnet add src/ElohimShop.API/ElohimShop.API.csproj package Nombre.Paquete
```

- Ver paquetes instalados:

```powershell
dotnet list src/ElohimShop.API/ElohimShop.API.csproj package
```

- Ver paquetes desactualizados:

```powershell
dotnet list src/ElohimShop.API/ElohimShop.API.csproj package --outdated
```

## 7. Calidad y formato

- Formatear codigo:

```powershell
dotnet format
```

- Tratar warnings como errores durante build:

```powershell
dotnet build ElohimShop.slnx -warnaserror
```

## 8. Publicacion

- Publicar la API para despliegue:

```powershell
dotnet publish src/ElohimShop.API/ElohimShop.API.csproj -c Release -o publish
```

## 9. Variables de entorno utiles (PowerShell)

- Definir entorno de ejecucion:

```powershell
$env:ASPNETCORE_ENVIRONMENT="Development"
```

- Definir connection string para la sesion actual:

```powershell
$env:ConnectionStrings__DefaultConnection="Host=...;Port=5432;Database=...;Username=...;Password=..."
```

## 10. Flujo recomendado diario

1. Restaurar dependencias.
2. Compilar solucion.
3. Ejecutar API con hot reload.
4. Si hubo cambios de modelo EF, crear y aplicar migracion.
5. Ejecutar pruebas.

Comandos:

```powershell
dotnet restore ElohimShop.slnx
dotnet build ElohimShop.slnx
dotnet watch --project src/ElohimShop.API/ElohimShop.API.csproj run
# Si aplica:
dotnet ef migrations add NombreDescriptivo --project src/ElohimShop.Infrastructure --startup-project src/ElohimShop.API
dotnet ef database update --project src/ElohimShop.Infrastructure --startup-project src/ElohimShop.API
dotnet test ElohimShop.slnx
```

## 11. Proyectos de Test

El proyecto de tests se encuentra en `tests/ElohimShop.Tests/`.

- Ejecutar todos los tests:

```powershell
dotnet test ElohimShop.slnx
```

- Ejecutar tests de un proyecto especifico:

```powershell
dotnet test tests/ElohimShop.Tests/ElohimShop.Tests.csproj
```

- Ejecutar tests con coverage:

```powershell
dotnet test ElohimShop.slnx --collect:"XPlat Code Coverage"
```

- Ejecutar tests por nombre:

```powershell
dotnet test ElohimShop.slnx --filter "FullyQualifiedName~CarritoServiceTests"
```

## 12. Docker y Contenedores

- Ver contenedores en ejecucion:

```powershell
docker ps
```

- Ver logs de un contenedor:

```powershell
docker logs <container_id>
```

- Reconstruir y levantar servicios:

```powershell
docker-compose up --build
```

- Detener servicios:

```powershell
docker-compose down
```
