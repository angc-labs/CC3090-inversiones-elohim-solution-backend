# Documentación de la Base de Datos

El sistema utiliza **PostgreSQL** (versión 16-alpine) como motor de base de datos relacional. La persistencia y el mapeo objeto-relacional (ORM) se gestionan en el backend C# con **Entity Framework Core (EF Core)**.

La arquitectura de datos está dividida en dos contextos separados con propósitos diferentes: **ElohimShopDbContext** (datos del eCommerce original) y **PlatformDbContext** (núcleo del DMV Hub con soporte multi-inquilino/multi-tenant).

---

## 1. Arquitectura de Contextos (DbContexts)

### A. PlatformDbContext (DM Hub Multi-Tenant)
Es el contexto principal de la aplicación actual. Implementa soporte nativo para múltiples tiendas (tenants) y sucursales compartiendo la misma base de datos de forma segura.

* **Filtros de Consulta Automáticos (Query Filters)**: 
  Todas las entidades ligadas a una tienda específica (`Sucursal`, `PlatformUser`, `Categoria`, `Producto`, `Inventario`, `CarritoElemento`, `Reservacion`, `ReportePersonalizado` y `CredencialesIntegracion`) configuran un filtro automático en `OnModelCreating`:
  ```csharp
  builder.HasQueryFilter(x => x.TiendaId == _tenantProvider.GetTenantId());
  ```
  Esto garantiza que cualquier consulta a la base de datos filtre de forma transparente los registros que pertenecen únicamente al tenant activo resuelto en la petición HTTP actual, previniendo fugas de información.

* **Deducción Automática de Stock (`SaveChangesAsync`)**:
  El contexto intercepta la persistencia de cambios en la entidad `Reservacion`. Cuando el estado de pago de una reservación cambia a `"pagado"`, el método sobreescrito `SaveChangesAsync` realiza de manera automática la deducción de inventario:
  1. Reduce la cantidad comprada en el `Inventario` específico de la `Sucursal` correspondiente.
  2. Reduce el stock en la tabla global de `Producto` (`StockActual`), previniendo sobreventas.

### B. ElohimShopDbContext (Módulo eCommerce Histórico)
Mapea las tablas originales del eCommerce de Esmira. Se mantiene por motivos de compatibilidad y operaciones históricas heredadas.

---

## 2. Diagrama de Relaciones (MER - Platform)

A continuación se muestra el modelo de datos del contexto unificado multi-tenant:

### Tabla de Relaciones

| Entidad Principal | Relación | Entidad Secundaria | Cardinalidad | Significado |
|---|---|---|---|---|
| **Tienda** | posee | Sucursal | 1 : N | Una tienda tiene múltiples sucursales |
| **Tienda** | registra | PlatformUser | 1 : N | Una tienda puede tener múltiples usuarios |
| **Tienda** | organiza | Categoria | 1 : N | Una tienda organiza sus productos en categorías |
| **Tienda** | vende | Producto | 1 : N | Una tienda vende múltiples productos |
| **Tienda** | distribuye | Inventario | 1 : N | Una tienda distribuye inventario a sucursales |
| **Tienda** | almacena | CarritoElemento | 1 : N | Una tienda almacena carritos de compra de usuarios |
| **Tienda** | recibe | Reservacion | 1 : N | Una tienda recibe múltiples reservaciones |
| **Tienda** | ejecuta | ReportePersonalizado | 1 : N | Una tienda genera múltiples reportes |
| **Tienda** | configura | CredencialesIntegracion | 1 : 1 | Una tienda tiene exactamente una configuración de integraciones |
| **PlatformUser** | inicia | Session | 1 : N | Un usuario puede tener múltiples sesiones activas |
| **PlatformUser** | enlaza | Account | 1 : N | Un usuario puede enlazar múltiples cuentas (OAuth) |
| **PlatformUser** | agrega | CarritoElemento | 1 : N | Un usuario agrega múltiples productos a su carrito |
| **PlatformUser** | realiza | Reservacion | 1 : N | Un usuario realiza múltiples reservaciones |
| **Sucursal** | asigna_a | PlatformUser | 1 : N | Una sucursal asigna múltiples usuarios |
| **Sucursal** | almacena_en | Inventario | 1 : N | Una sucursal almacena inventario de múltiples productos |
| **Sucursal** | despacha_en | Reservacion | 1 : N | Una sucursal despacha múltiples reservaciones |
| **Producto** | tiene | Inventario | 1 : N | Un producto tiene múltiples registros de inventario por sucursal |
| **Producto** | esta_en | CarritoElemento | 1 : N | Un producto puede estar en múltiples carritos |
| **Producto** | contiene_en | DetalleReservacion | 1 : N | Un producto se detalla en múltiples reservaciones |
| **Producto** | pertenece | Categoria | M : N | Un producto puede pertenecer a múltiples categorías |
| **Reservacion** | desglosa_en | DetalleReservacion | 1 : N | Una reservación se desglosa en múltiples detalles de productos |

### Definición de Entidades Principales

```
Tienda
├── id (PK): Identificador único
├── nombre: Nombre del negocio
├── slug: Identificador URL amigable
├── estado: Activa/Inactiva
├── configuracion_visual: Datos JSON de personalización
└── fecha_creacion: Timestamp de registro

Sucursal
├── id (PK)
├── tienda_id (FK): Referencia a Tienda
├── nombre: Nombre de la sucursal
├── direccion: Ubicación física
├── telefono: Contacto
└── fecha_creacion

PlatformUser
├── id (PK)
├── name: Nombre completo
├── email: Correo único por tienda
├── tienda_id (FK)
├── rol_staff: Admin/Vendor/Cashier
├── sucursal_id (FK): Asignación a sucursal (opcional)
└── stripe_customer_id: ID de cliente en Stripe

Producto
├── id (PK)
├── tienda_id (FK)
├── categoria_id (FK)
├── nombre
├── sku: Identificador de stock
├── precio_mayoreo / precio_detalle
├── stock_actual: Disponible globalmente
├── stock_minimo: Alerta de bajo stock
└── imagen_url

Inventario
├── id (PK)
├── tienda_id (FK)
├── sucursal_id (FK): Stock específico por sucursal
├── producto_id (FK)
└── stock: Cantidad disponible en esta sucursal

Reservacion
├── id (PK)
├── tienda_id (FK)
├── sucursal_id (FK): Despacho en sucursal
├── usuario_id (FK): Quien reservó
├── monto_total: Precio final
├── estado_pago: Pendiente/Pagado/Refundado
├── estado_despacho: Pendiente/Despachado/Entregado
└── stripe_intent_id: Referencia de Stripe

CredencialesIntegracion
├── tienda_id (PK, FK)
├── stripe_secret_key
├── stripe_public_key
├── cloudinary_cloud_name
└── smtp_email / smtp_password
```

---

## 3. Lógica de Inicialización y Seeders

El backend contiene un flujo de arranque automático que se ejecuta al iniciar la API (en `Program.cs`):

1. **Database Schema Bootstrapper**:
   Ejecuta las migraciones pendientes en PostgreSQL para asegurar que las tablas y columnas existan y estén en la última versión.
2. **PlatformDatabaseBootstrapper & Schema Bootstrapper**:
   Crea la estructura física de la base de datos si no existe.
3. **SuperAdminSeeder**:
   Si no existe ningún superadministrador en la base de datos, lee las credenciales del archivo de configuración (o variables de entorno `SUPER_ADMIN_EMAIL` y `SUPER_ADMIN_PASSWORD`) y crea el usuario inicial con rol `superadmin`.
4. **Demo Data Seeders (PlatformDemoDataSeeder & DemoDataSeeder)**:
   Si la variable de entorno `SEED_DATA=true` está configurada, el sistema inyectará registros de prueba automatizados para verificar el funcionamiento de la plataforma:
   * Instancias de tienda demo (`dmhub`, `logistics`, etc.).
   * Sucursales por tienda.
   * Categorías y productos representativos con stock e imágenes.
   * Cuentas de usuarios de staff y clientes demo (`carlos.demo@dmhub.gt`, `cliente.demo@dmhub.gt`, contraseña común `Demo123!`).
   * Reservaciones, ventas e inventarios iniciales para poblar los gráficos del dashboard.
