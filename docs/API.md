# Documentación de la API

La API del sistema está construida sobre ASP.NET Core utilizando arquitectura REST. Los endpoints están organizados bajo prefijos de ruta de versión (`/api/v1`) y de administración (`/api/admin`).

Todos los controladores (excepto los de catálogo público) requieren el paso del identificador del tenant para resolver el contexto de la tienda (mediante cookies o las cabeceras `X-Tenant-ID` o `X-Tenant-Slug`).

---

## 1. Controladores por Módulo

### Autenticación (`AuthController`)
Prefijo: `/api/v1/auth` | Acceso: Público / Autenticado

- `POST /login` - Iniciar sesión con credenciales
- `POST /register` - Registrar nuevo usuario o staff
- `POST /logout` - Cerrar sesión
- `POST /forgot-password` - Solicitar recuperación de contraseña
- `POST /change-password` - Cambiar contraseña existente

### Administración de Usuarios (`AdminUsuariosController`)
Prefijo: `/api/admin/usuarios` | Acceso: Admin / SuperAdmin

- `GET /` - Listar usuarios de la tienda
- `POST /` - Crear nuevo usuario o staff
- `PUT /{id}` - Editar información de usuario
- `POST /{id}/reset-password` - Generar códigos de recuperación

### Productos y Catálogo (`ProductosV1Controller`)
Prefijo: `/api/v1/productos` | Acceso: Público (lectura), Admin (escritura)

- `GET /` - Listar productos con paginación y filtros
- `POST /` - Crear producto (Admin)
- `PUT /{id}` - Editar producto (Admin)
- `DELETE /{id}` - Eliminar producto soft-delete (Admin)

### Sucursales (`SucursalesV1Controller`)
Prefijo: `/api/v1/sucursales` | Acceso: Admin

- `GET /` - Listar sucursales de la tienda
- `POST /` - Crear sucursal
- `PUT /{id}` - Editar información de sucursal
- `DELETE /{id}` - Eliminar sucursal

### Inventarios (`InventariosController`)
Prefijo: `/api/v1/inventarios` | Acceso: Admin

- `GET /sucursal/{sucursalId}` - Ver stock por sucursal
- `PUT /` - Actualizar cantidad de stock
- `GET /bajo-stock` - Productos con stock bajo

### Carrito (`CarritoV1Controller`)
Prefijo: `/api/v1/carrito` | Acceso: Cliente Autenticado

- `GET /` - Ver carrito del cliente
- `POST /` - Agregar producto al carrito
- `PUT /{itemId}` - Actualizar cantidad de item
- `DELETE /{itemId}` - Remover producto del carrito

### Reservaciones y Compras (`ReservacionesV1Controller`)
Prefijo: `/api/v1/reservaciones` | Acceso: Admin / Cliente

- `GET /` - Listar todas las reservaciones (Admin)
- `GET /mis-reservaciones` - Historial de compras del cliente
- `POST /` - Crear nueva reservación (Cliente)
- `PUT /{id}/despacho` - Cambiar estado de despacho (Admin)

### Pagos (`PagosController`)
Prefijo: `/api/pagos` | Acceso: Cliente / Público (webhooks)

- `POST /crear-intento` - Crear intent de pago Stripe (Cliente)
- `POST /webhook` - Recibir eventos de Stripe (Público - asíncrono)

### Métodos de Pago (`MetodoPagoController`)
Prefijo: `/api/metodoPago` | Acceso: Cliente Autenticado

- `GET /` - Listar tarjetas guardadas
- `POST /` - Guardar nueva tarjeta (tokenizada en Stripe)
- `DELETE /{id}` - Eliminar tarjeta guardada

### Reportes y Dashboard (`ReportesV1Controller`)
Prefijo: `/api/v1/reportes` | Acceso: Admin

- `GET /dashboard` - Métricas consolidadas de la tienda
- `POST /personalizados` - Guardar plantilla de reporte SQL
- `GET /personalizados/{id}/ejecutar` - Ejecutar reporte personalizado
- `GET /ventas/exportar` - Descargar reporte en Excel

### Configuración Visual (`TiendasController`)
Prefijo: `/api/v1/tiendas` | Acceso: Público (lectura), Admin (escritura)

- `GET /configuracion-visual` - Obtener configuración del constructor
- `PUT /configuracion-visual` - Guardar cambios de diseño (Admin)
- `GET /informacion` - Datos básicos de la tienda

### Media y Archivos (`MediaController`)
Prefijo: `/api/v1/media` | Acceso: Admin

- `POST /upload` - Subir archivo a Cloudinary
- `DELETE /{id}` - Eliminar archivo (Cloudinary)

### Catálogo Público (`CatalogController`)
Prefijo: `/api/` | Acceso: Público

- `GET /categorias` - Listar categorías disponibles
- `GET /productos` - Consultar catálogo (sin autenticación)

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

**Nota**: Para mayor informacion se sugiere consultar el Swagger del proyecto, este incluye el listado completo de todos los metodos http creados y el formato de las solicitudes y respuestas (en formato Json) de dichos metodos. 