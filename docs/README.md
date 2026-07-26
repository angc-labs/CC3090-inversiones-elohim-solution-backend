# Backend Documentation Index

Bienvenido a la documentación técnica del backend de **Esmira Shop**.

## Estructura de Documentación

```
backend/docs/
├── README.md           ← Estás aquí
├── SETUP.md            → Guía de instalación y configuración
├── ARCHITECTURE.md     → Diseño de la aplicación
├── API.md              → Referencia completa de endpoints
├── DATABASE.md         → Esquema de base de datos
├── NETWORK.md          → Configuración de red y Docker
└── TESTING.md          → Estrategia de testing (en desarrollo)
```

## Inicio Rápido

### Desarrollo con Docker (recomendado)
```bash
docker compose up -d --build
# API: http://localhost:5000
# Swagger: http://localhost:5000/swagger
```

### Desarrollo Local
```bash
cd backend
dotnet restore ElohimShop.slnx
dotnet build ElohimShop.slnx
dotnet run --project src/ElohimShop.API
```

---

## Documentación Disponible

### 1️⃣ [SETUP.md](SETUP.md)
- Requisitos del sistema
- Instalación paso a paso
- Variables de entorno
- Seed de datos

### 2️⃣ [ARCHITECTURE.md](ARCHITECTURE.md)
- Estructura de capas
- Patrones de diseño
- Estructura de proyectos
- Flujo de datos

### 3️⃣ [API.md](API.md)
- Referencia de controladores
- Endpoints por módulo
- Ejemplos de request/response
- Códigos de error

### 4️⃣ [DATABASE.md](DATABASE.md)
- Modelo de datos
- Tablas principales
- Relaciones
- Migraciones

### 5️⃣ [NETWORK.md](NETWORK.md)
- Configuración Docker Compose
- Puertos y servicios
- Variables de entorno
- Troubleshooting

---

## Arquitectura en Capas

```
ElohimShop.API           ← Controllers, Middleware
    ↓
ElohimShop.Application   ← DTOs, Services
    ↓
ElohimShop.Domain        ← Entities, Interfaces
    ↓
ElohimShop.Infrastructure ← EF Core, Repositories
    ↓
PostgreSQL DB
```

---

## Stack Tecnológico

- **Framework**: ASP.NET Core 10
- **Base de datos**: PostgreSQL
- **ORM**: Entity Framework Core
- **Autenticación**: JWT
- **Pagos**: Stripe
- **Almacenamiento**: Cloudinary
- **Contenedorización**: Docker

---

## Variables de Entorno Principales

### 🗄️ Base de Datos
- **`DATABASE_URL`**: Conexión a PostgreSQL
  - Ejemplo: `Host=postgres;Port=5432;Database=dmhub;Username=postgres;Password=postgres`
- **`ASPNETCORE_ENVIRONMENT`**: Ambiente de ejecución
  - Ejemplo: `Development` o `Production`

### 🔐 Autenticación y Seguridad
- **`JWT_SECRET`**: Secreto para firmar JWT (mínimo 32 caracteres)
  - Ejemplo: `your-super-secret-key-min-32-chars-long`
- **`SUPER_ADMIN_EMAIL`**: Email del superadmin inicial
  - Ejemplo: `admin@elohim.com`
- **`SUPER_ADMIN_PASSWORD`**: Contraseña del superadmin inicial
  - Ejemplo: `SecurePassword123!`

### 🌱 Inicialización de Datos
- **`SEED_DATA`**: Cargar datos de prueba automáticamente
  - Valores: `true` o `false`
- **`SEED_USER_EMAIL`**: Email de usuario de prueba (opcional)
  - Ejemplo: `demo@elohim.com`
- **`SEED_USER_PASSWORD`**: Contraseña de usuario de prueba (opcional)
  - Ejemplo: `Demo123!`

### 💳 Integraciones Externas
- **`STRIPE_SECRET_KEY`**: API Key privada de Stripe
  - Ejemplo: `sk_test_xxxxxxxxxxxxx`
- **`CLOUDINARY_URL`**: Credenciales completas de Cloudinary
  - Ejemplo: `cloudinary://key:secret@cloud_name`
- **`CLOUDINARY_CLOUD_NAME`**: Nombre de la nube en Cloudinary
  - Ejemplo: `tu-cloud-name`

### 📧 Email (SMTP)
- **`SMTP_EMAIL`**: Email para envíos (remitente)
  - Ejemplo: `noreply@tudominio.com`
- **`SMTP_PASSWORD`**: Contraseña de aplicación SMTP
  - Ejemplo: `tu-contraseña-app`
- **`SMTP_HOST`**: Servidor SMTP
  - Ejemplo: `smtp.gmail.com`
- **`SMTP_PORT`**: Puerto SMTP
  - Ejemplo: `587`

---

## Testing

*Documentación en desarrollo. Ver `tests/ElohimShop.Tests/` para ejemplos actuales.*

---

## Troubleshooting

### Puerto 5000 en uso
```bash
docker compose down
# O cambiar puerto en docker-compose.yml
```

### Problemas de base de datos
```bash
docker compose down -v
docker compose up -d --build
```

### Rebuild de servicios
```bash
docker compose build --no-cache backend
docker compose up -d
```

---

## Soporte Técnico

Para reportar problemas:
1. Verificar logs: `docker logs backend-service-name`
2. Consultar Swagger: http://localhost:5000/swagger
3. Revisar sección de Troubleshooting en cada documento

---

**Última actualización**: 2026-07-25
