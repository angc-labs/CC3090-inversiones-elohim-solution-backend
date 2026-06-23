# Documentación de la API

La API del sistema está construida sobre ASP.NET Core utilizando arquitectura REST. Los endpoints están organizados bajo prefijos de ruta de versión (`/api/v1`) y de administración (`/api/admin`).

Todos los controladores (excepto los de catálogo público) requieren el paso del identificador del tenant para resolver el contexto de la tienda (mediante cookies o las cabeceras `X-Tenant-ID` o `X-Tenant-Slug`).

---

## 1. Resumen de Controladores y Rutas

| Controlador | Prefijo de Ruta | Acceso | Propósito |
| :--- | :--- | :--- | :--- |
| **AuthController** | `/api/v1/auth` | Público / Autenticado | Gestión de sesiones, login, registro, recuperación de contraseña. |
| **AdminUsuariosController** | `/api/admin/usuarios` | Staff / Admin | Administración de usuarios y staff dentro de una tienda. |
| **ProductosV1Controller** | `/api/v1/productos` | Staff / Admin | ABM (Alta, Baja, Modificación) de productos y categorías de la tienda. |
| **SucursalesV1Controller** | `/api/v1/sucursales` | Staff / Admin | Gestión de sucursales asociadas a la tienda. |
| **InventariosController** | `/api/v1/inventarios` | Staff / Admin | Control y actualización de existencias de productos por sucursal. |
| **ReservacionesV1Controller** | `/api/v1/reservaciones` | Staff / Admin / Cliente | Creación y despacho de reservaciones / compras. |
| **ReportesV1Controller** | `/api/v1/reportes` | Staff / Admin | Obtención de estadísticas, reportes personalizados y exportación a Excel. |
| **TiendasController** | `/api/v1/tiendas` | Staff / Admin | Configuración de información básica y configuración visual (JSONB). |
| **MetodoPagoController** | `/api/metodoPago` | Cliente | Gestión de tarjetas y tokens guardados en la pasarela de pagos. |
| **PagosController** | `/api/pagos` | Cliente | Intenciones de pago de Stripe y Webhooks para confirmación automática. |
| **MediaController** | `/api/v1/media` | Staff / Admin | Carga de archivos multimedia directamente a Cloudinary. |
| **CarritoV1Controller** | `/api/v1/carrito` | Cliente | Gestión del carrito de compras persistente. |
| **CatalogController** | `/api` | Público | Consulta del catálogo de productos y categorías para el Storefront. |

---

## 2. Detalle de Endpoints y Operaciones

### Auth (Sesiones y Registro) - `AuthController`
Prefijo: `/api/v1/auth`

* **`POST /login`** (Público)
  * **Payload**: `{ "correo": "string", "contrasena": "string" }`
  * **Respuesta (200 OK)**:
    ```json
    {
      "usuarioId": "string",
      "correo": "string",
      "nombre": "string",
      "tipoUsuario": "cliente|staff",
      "rol": "cajero|administrador|superadmin|null",
      "token": "jwt_token_string",
      "expiraEn": "ISO-8601-DateTime"
    }
    ```
* **`POST /register`** (Público / Admin para Staff)
  * **Payload**: `{ "correo": "string", "nombre": "string", "contrasena": "string", "tipoUsuario": "cliente|administrador", "rol": "cajero|administrador|null", "tipoCliente": "particular|mayorista|minorista" }`
  * **Respuesta (200 OK)**: Mismo formato de `/login`.
* **`POST /logout`** (Autenticado)
  * **Headers**: `Authorization: Bearer <token>`
  * **Respuesta (200 OK)**: `{ "mensaje": "Sesión cerrada correctamente" }`
* **`POST /forgot-password`** (Público)
  * **Payload**: `{ "correo": "string" }`
  * **Respuesta (200 OK)**: Envia el código OTP SMTP configurado de forma autónoma.
* **`POST /change-password`** (Autenticado)
  * **Payload**: `{ "contrasenaActual": "string", "nuevaContrasena": "string" }`
  * **Respuesta (200 OK)**: `{ "mensaje": "Contraseña cambiada con éxito" }`

---

### Administración de Usuarios - `AdminUsuariosController`
Prefijo: `/api/admin/usuarios` (Solo accesible por Staff con rol `administrador` o `superadmin`)

* **`GET /`**
  * **Respuesta (200 OK)**: Retorna lista de usuarios pertenecientes al tenant.
* **`POST /`**
  * **Payload**: `{ "nombre": "string", "email": "string", "tipoUsuario": "staff|cliente", "rolStaff": "cajero|admin", "sucursalId": "string|null" }`
  * **Respuesta (201 Created)**: Retorna el objeto del usuario creado.
* **`PUT /{id}`**
  * **Payload**: `{ "nombre": "string", "rolStaff": "string", "sucursalId": "string|null", "estado": boolean }`
  * **Respuesta (200 OK)**: Usuario actualizado.
* **`POST /{id}/reset-password`**
  * **Respuesta (200 OK)**: Genera 8 códigos de recuperación alternativos para restablecer la contraseña offline.
    ```json
    {
      "usuarioId": "string",
      "correo": "string",
      "nombre": "string",
      "codigos": ["code1", "code2", "code3", "..."]
    }
    ```

---

### Productos y Categorías - `ProductosV1Controller`
Prefijo: `/api/v1/productos` (Operaciones de escritura reservadas para Staff)

* **`GET /`** (Público)
  * **Parámetros**: `page` (int), `limit` (int), `categoriaId` (string), `buscar` (string)
  * **Respuesta (200 OK)**: Lista paginada de productos filtrados por el tenant actual.
* **`POST /`** (Admin)
  * **Payload**: `{ "nombre": "string", "descripcion": "string", "sku": "string", "precioMayoreo": decimal, "precioDetalle": decimal, "imagenUrl": "string", "categoriaId": "string" }`
  * **Respuesta (201 Created)**: Producto creado.
* **`PUT /{id}`** (Admin)
  * **Payload**: Estructura de actualización del producto.
  * **Respuesta (200 OK)**: Producto modificado.
* **`DELETE /{id}`** (Admin)
  * **Respuesta (200 OK)**: Soft-delete (marca columna `eliminado = true`).

---

### Reservaciones y Compras - `ReservacionesV1Controller`
Prefijo: `/api/v1/reservaciones`

* **`GET /`** (Staff)
  * **Respuesta (200 OK)**: Lista completa de reservaciones de la tienda.
* **`GET /mis-reservaciones`** (Cliente)
  * **Respuesta (200 OK)**: Lista de reservaciones asociadas al cliente logueado.
* **`POST /`** (Cliente)
  * **Payload**:
    ```json
    {
      "sucursalId": "string",
      "detalles": [
        { "productoId": "string", "cantidad": 5 }
      ]
    }
    ```
  * **Respuesta (201 Created)**: Reservación creada con estado de pago `"pendiente"`.
* **`PUT /{id}/despacho`** (Staff)
  * **Payload**: `{ "estadoDespacho": "procesando|completado|cancelado" }`
  * **Respuesta (200 OK)**: Estado actualizado.

---

### Inventarios - `InventariosController`
Prefijo: `/api/v1/inventarios` (Solo accesible por Staff)

* **`GET /sucursal/{sucursalId}`**
  * **Respuesta (200 OK)**: Existencias de todos los productos en la sucursal indicada.
* **`PUT /`**
  * **Payload**: `{ "sucursalId": "string", "productoId": "string", "stock": 50 }`
  * **Respuesta (200 OK)**: Existencia de stock actualizada en el inventario físico de la sucursal.

---

### Dashboard y Reportes - `ReportesV1Controller`
Prefijo: `/api/v1/reportes` (Solo accesible por Staff con rol `administrador` o superior)

* **`GET /dashboard`**
  * **Respuesta (200 OK)**: Métricas consolidadas (Ventas totales, reservaciones pendientes, stock bajo por producto, facturación del mes).
* **`POST /personalizados`**
  * **Payload**: `{ "nombre": "string", "querySql": "SELECT ... FROM ... WHERE tienda_id = @TiendaId" }`
  * **Respuesta (201 Created)**: Guarda una plantilla de consulta SQL personalizada para el tenant.
* **`GET /personalizados/{id}/ejecutar`**
  * **Respuesta (200 OK)**: Ejecuta de manera segura la consulta SQL personalizada del tenant y retorna la rejilla de datos en formato JSON.
* **`GET /ventas/exportar`**
  * **Respuesta (200 OK)**: Genera y descarga un archivo `.xlsx` (Excel) con el desglose de ventas del mes en curso utilizando `ClosedXML` u otra librería de hojas de cálculo.

---

### Pasarela de Pagos (Stripe) - `PagosController`
Prefijo: `/api/pagos`

* **`POST /crear-intento`** (Cliente Autenticado)
  * **Payload**: `{ "reservacionId": "string" }`
  * **Respuesta (200 OK)**: Retorna el `clientSecret` e `intentId` de Stripe para inicializar el SDK del lado del cliente.
* **`POST /webhook`** (Público)
  * **Payload**: Eventos asíncronos provenientes de los servidores de Stripe.
  * **Acción**: Intercepta eventos de tipo `payment_intent.succeeded`. Busca la reservación asociada mediante `stripe_intent_id`, cambia su estado a `"pagado"` en la base de datos, lo que dispara automáticamente la deducción de inventario (`SaveChangesAsync`) en la sucursal.

---

### Gestión Visual del Constructor de Tiendas - `TiendasController`
Prefijo: `/api/v1/tiendas`

* **`GET /configuracion-visual`** (Público)
  * **Respuesta (200 OK)**: JSON completo del constructor visual de la tienda asociada.
* **`PUT /configuracion-visual`** (Staff con rol `administrador`)
  * **Payload**: Objeto JSON serializado con el esquema de secciones (`sections`).
  * **Respuesta (200 OK)**: Configuración guardada en la base de datos de manera persistente.
