# Setup Backend - Guía de Instalación

## Requisitos del Sistema

### Mínimo
- **.NET SDK 10** ([descargar](https://dotnet.microsoft.com/download))
- **Git**
- **PostgreSQL 15+** (si no usas Docker)

### Recomendado
- **Docker Desktop** (Windows/Mac) o **Docker Engine** (Linux)
- **Visual Studio Code** o **Visual Studio 2022+**
- **Postman** o **REST Client** para testing de APIs
- **pgAdmin** para gestión de base de datos

---

## Instalación con Docker (RECOMENDADO)

### Paso 1: Clonar y Posicionarse
```bash
cd CC3090-inversiones-elohim-solution
```

### Paso 2: Levantar Servicios
```bash
docker compose up -d --build
```

### Paso 3: Verificar Servicios
```bash
docker compose ps
```

**Salida esperada:**
```
NAME                              STATUS
backend                           Up
postgres                          Up
```

### Paso 4: Acceder a Servicios

| Servicio | URL | Descripción |
|----------|-----|-------------|
| **API Backend** | http://localhost:5000 | Servidor principal |
| **Swagger UI** | http://localhost:5000/swagger | Documentación interactiva |
| **PostgreSQL** | localhost:5433 | Base de datos |

### Paso 5: Cargar Datos de Prueba (Opcional)

Si `SEED_DATA=true` en `backend/.env`, los datos se cargan automáticamente.

Verificar en Swagger:
1. Login con: `admin@elohim.com` / contraseña en `.env`
2. Navegar a `GET /api/v1/productos` para ver datos

---

## Instalación Local (sin Docker)

### Paso 1: Requisitos

```bash
# Verificar .NET
dotnet --version

# Verificar PostgreSQL está corriendo
psql --version
```

### Paso 2: Restaurar Dependencias

```bash
cd backend
dotnet restore ElohimShop.slnx
```

### Paso 3: Configurar Base de Datos

**Opción A: Usar script SQL**
```bash
# En PostgreSQL
psql -U postgres -d postgres -f ../db/elohim_db.sql
```

**Opción B: Entity Framework Migrations**
```bash
cd src/ElohimShop.API
dotnet ef database update
```

### Paso 4: Configurar Variables de Entorno

Crear `backend/.env` (consulta `.env.example` en el proyecto):
```env
ASPNETCORE_ENVIRONMENT=Development
DATABASE_URL=Host=localhost;Port=5432;Database=elohim_shop;Username=postgres;Password=your_password
JWT_SECRET=your-super-secret-key-min-32-chars-long
SEED_DATA=true
SUPER_ADMIN_EMAIL=admin@elohim.com
SUPER_ADMIN_PASSWORD=SecurePassword123!
STRIPE_SECRET_KEY=sk_test_your_key_here
CLOUDINARY_URL=cloudinary://key:secret@cloud_name
```
**NOTA**: Estas credenciales solo son de ejemplo, obten tus credenciales reales de Stripe ([Stripe](https://dashboard.stripe.com)) y Cloudinary ([Cloudinary](https://cloudinary.com/console))
### Paso 5: Build y Run

```bash
cd backend
dotnet build ElohimShop.slnx
dotnet run --project src/ElohimShop.API
```

**Salida esperada:**
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: http://localhost:5000
```

---

## Configuración de Credenciales

### Super Admin Inicial

Estas credenciales se crean automáticamente en la primer ejecución:

```
Email: admin@elohim.com
Contraseña: (definida en SUPER_ADMIN_PASSWORD)
Tipo: Super Admin
```

### Usuarios de Prueba (si SEED_DATA=true)

El seeder crea automáticamente:
- **Super Admin**: admin@elohim.com
- **Admin de Tienda**: store-admin@elohim.com
- **Vendedor**: seller@elohim.com
- **Cliente**: customer@elohim.com

Todas con contraseña: `Temporal123!` (modificable en `Configuration/SeedDataOptions.cs`)

---

## Base de Datos

### Esquema

```sql
tenants
├── id, nombre, slug, activo

usuarios
├── id, correo, nombre, tipo_usuario, rol, hash_contraseña

productos
├── id, tenant_id, nombre, descripcion, precio, categorias[]

inventarios
├── id, producto_id, sucursal_id, cantidad_disponible

reservaciones
├── id, usuario_id, estado, total, fecha_creacion

pagos
├── id, reservacion_id, stripe_payment_id, estado
```

### Conexión desde Cliente

**PostgreSQL nativo:**
```bash
psql -h localhost -p 5433 -U postgres -d elohim_shop
```

**pgAdmin (si está disponible):**
```
Host: localhost:5050
Usuario: admin@admin.com
Contraseña: admin
```

---

## Verificación de Instalación

### 1. Backend corriendo
```bash
curl http://localhost:5000/health
```
**Respuesta esperada:** 
```json
{"status":"Healthy"}
```

### 2. Base de datos conectada
```bash
curl http://localhost:5000/swagger
```
Debería cargar la página Swagger sin errores.

### 3. Autenticación funcionando
```bash
curl -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"correo":"admin@elohim.com","contrasena":"SecurePassword123!"}'
```

---

## Comandos Útiles

### Docker

```bash
# Ver logs
docker compose logs -f backend

# Reiniciar servicio
docker compose restart backend

# Detener
docker compose down

# Detener y limpiar volúmenes
docker compose down -v

# Rebuild
docker compose build --no-cache backend
docker compose up -d
```

### Dotnet

```bash
# Build
dotnet build ElohimShop.slnx

# Run
dotnet run --project src/ElohimShop.API

# Tests
dotnet test tests/ElohimShop.Tests.csproj

# Migrations (si usas EF)
dotnet ef migrations add MigrationName
dotnet ef database update
```

---

## Troubleshooting

### "Port 5000 is already in use"

```bash
# Opción 1: Usar otro puerto
# En docker-compose.yml cambiar: "5001:8080"

# Opción 2: Matar proceso
# Windows:
netstat -ano | findstr :5000
taskkill /PID <PID> /F

# Linux/Mac:
lsof -i :5000
kill -9 <PID>
```

### "Cannot connect to database"

```bash
# Verificar que PostgreSQL está corriendo
docker compose ps postgres

# Ver logs
docker compose logs postgres

# Recrear volumen
docker compose down -v
docker compose up -d --build
```

### "Seed data not loading"

1. Verificar `SEED_DATA=true` en `.env`
2. Revisar logs: `docker compose logs backend | grep -i seed`
3. Forzar recreación: `docker compose down -v && docker compose up -d --build`

### Error en JWT

```bash
# Verificar que JWT_SECRET está configurado
# Debe tener al menos 32 caracteres
echo ${JWT_SECRET} | wc -c
```

---

## Próximos Pasos

1. Backend levantado
2. **Lee**: [ARCHITECTURE.md](ARCHITECTURE.md) - Entender la estructura
3. **Lee**: [API.md](API.md) - Explorar endpoints
4. **Lee**: [DATABASE.md](DATABASE.md) - Entender el modelo de datos
5. **Accede**: Swagger en http://localhost:5000/swagger

---

**Última actualización**: 2026-07-25
