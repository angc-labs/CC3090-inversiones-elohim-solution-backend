# DM Hub - API REST

**Base URL (Docker):** `http://localhost:5000`  
**Swagger (Development):** `http://localhost:5000/swagger`

## Contrato actual y Autenticación

- **Aislamiento Multi-Tenant:** Los endpoints operativos de tienda requieren la cabecera HTTP `X-Tenant-ID` para aislar lógicamente las peticiones.
- **Resource Server & Better Auth:** El backend funciona como un Resource Server. La autenticación y las sesiones son administradas por Better Auth en el frontend Next.js.
- **Validación de Sesiones:** El backend valida las solicitudes interceptando el token de sesión propagado de las siguientes formas:
  - Cabecera: `Authorization: Bearer <sessionToken>`
  - Cookie: `better-auth.session_token` o `__secure-better-auth.session_token`
- **Inyección de Contexto:** Al validar el token con las tablas `session` y `user` en la base de datos centralizada, se inyectan las siguientes claims en `HttpContext.User`:
  - `sub` / `NameIdentifier`: ID único del usuario
  - `email`: Correo electrónico
  - `tienda_id`: Tenant (ID de la tienda activa del usuario)
  - `tipo_usuario`: Rol general (`cliente` o `administrador` para staff)
  - `rol` / `rol_staff`: Sub-rol específico para personal (`administrador`, `cajero`, `logistica`)
- **Compatibilidad:** Para compatibilidad con los controladores heredados, el claim `tipo_usuario` se mapea a `"administrador"` para el personal del staff, manteniendo el acceso a las funciones administrativas.
- **Endpoints Nuevos:** Los endpoints migrados viven bajo `/api/v1/...`.


## Endpoints v1 principales

| Método | Ruta | Acceso | Uso |
|---|---|---|---|
| `POST` | `/api/v1/tiendas` | Público | Crear tienda |
| `GET` | `/api/v1/tiendas/valida-slug/{slug}` | Público | Validar slug |
| `PUT` | `/api/v1/tiendas/configuracion-visual` | Tenant | Guardar JSON visual |
| `POST` | `/api/v1/tiendas/integraciones` | Tenant | Guardar credenciales |
| `GET` | `/api/v1/media/cloudinary-signature` | Tenant | Firmar subida a Cloudinary |
| `DELETE` | `/api/v1/media` | Tenant | Eliminar asset por `publicId` |
| `GET` | `/api/v1/productos` | Tenant | Listar catálogo |
| `GET` | `/api/v1/productos/{id}` | Tenant | Ver producto |
| `POST` | `/api/v1/productos` | Tenant | Crear producto |
| `PUT` | `/api/v1/productos/{id}` | Tenant | Actualizar producto |
| `DELETE` | `/api/v1/productos/{id}` | Tenant | Eliminar producto |
| `GET` | `/api/v1/inventarios/sucursal/{sucursalId}` | Tenant | Ver inventario por sucursal |
| `PUT` | `/api/v1/inventarios/ajuste` | Tenant | Ajustar stock |
| `GET` | `/api/v1/carrito` | Auth + Tenant | Ver carrito |
| `POST` | `/api/v1/carrito/articulos` | Auth + Tenant | Agregar al carrito |
| `PUT` | `/api/v1/carrito/articulos/{id}` | Auth + Tenant | Actualizar cantidad |
| `DELETE` | `/api/v1/carrito/articulos/{id}` | Auth + Tenant | Quitar artículo |
| `POST` | `/api/v1/checkout/crear-intento` | Auth + Tenant | Generar intento de pago |
| `POST` | `/api/v1/reservaciones` | Auth + Tenant | Crear reservación |
| `GET` | `/api/v1/reservaciones/mis-compras` | Auth + Tenant | Historial del cliente |
| `GET` | `/api/v1/reservaciones/control-staff` | Staff + Tenant | Panel operativo |
| `PATCH` | `/api/v1/reservaciones/{id}/estado` | Staff + Tenant | Cambiar estado |
| `POST` | `/api/v1/reportes/ejecutar-raw` | Staff + Tenant | Ejecutar SELECT seguro |
| `POST` | `/api/v1/reportes/guardar` | Staff + Tenant | Guardar reporte |
| `GET` | `/api/v1/reportes` | Staff + Tenant | Listar reportes |
| `GET` | `/api/v1/reportes/{id}/correr` | Staff + Tenant | Correr reporte guardado |

## Reglas de payload

- Moneda: GTQ.
- Fechas: ISO-8601.
- Reportes SQL: solo `SELECT` o `WITH`; se bloquean verbos mutantes.
- La API nueva responde con DTOs simples; el frontend debe leer `ConfiguracionVisual` como JSON serializado.

## Nota de compatibilidad

Las rutas antiguas siguen presentes en el backend mientras se termina la migración del frontend. La documentación y el trabajo nuevo deben apuntar al prefijo `/api/v1`.

**Response `409 Conflict`:**
```json
{
  "error": "El correo ya está en uso por otra cuenta."
}
```

---

## 3. Catálogo

### GET `/api/marcas`

Lista todas las marcas disponibles.

**Query params:** ninguno

**Response `200 OK`:**
```json
[
  {
    "id": "string",
    "nombreMarca": "string",
    "descripcion": "string"
  }
]
```

---

### GET `/api/categorias`

Lista todas las categorías disponibles.

**Query params:** ninguno

**Response `200 OK`:**
```json
[
  {
    "id": "string",
    "nombreCategoria": "string",
    "descripcion": "string",
    "fechaCreacion": "2026-01-01T00:00:00Z"
  }
]
```

---

### GET `/api/productos`

Lista productos con filtros y paginación.

**Query params:**
```
?category=string   (opcional) ID de categoría
?brand=string      (opcional) ID de marca
?page=int          (opcional, default: 1)
?limit=int         (opcional, default: 20)
```

**Response `200 OK`:**
```json
{
  "total": 100,
  "pagina": 1,
  "limite": 20,
  "productos": [
    {
      "idProducto": "string",
      "codigoProducto": "string",
      "nombreProducto": "string",
      "descripcion": "string",
      "precio": 0,
      "stockActual": 0,
      "idMarca": "string",
      "categoriaId": "string",
      "imagenPrincipal": "string (URL)",
      "fechaVencimiento": "2026-12-31T00:00:00Z"
    }
  ]
}
```

---

### GET `/api/productos/buscar`

Busca productos por nombre desde la barra de búsqueda.

**Query params:**
```
?q=string   (requerido) Texto a buscar
```

**Response `200 OK`:**
```json
{
  "query": "string",
  "resultados": [
    {
      "idProducto": "string",
      "nombreProducto": "string",
      "precio": 0,
      "imagenPrincipal": "string (URL)"
    }
  ]
}
```

---

### GET `/api/productos/:id`

Obtiene el detalle completo de un producto.

**Path params:**
```
:id   ID del producto
```

**Response `200 OK`:**
```json
{
  "idProducto": "string",
  "codigoProducto": "string",
  "nombreProducto": "string",
  "descripcion": "string",
  "precio": 0,
  "stockActual": 0,
  "idMarca": "string",
  "categoriaId": "string",
  "imagenPrincipal": "string (URL)",
  "fechaVencimiento": "2026-12-31T00:00:00Z",
  "fechaCreacion": "2026-01-01T00:00:00Z",
  "fechaActualizacion": "2026-01-01T00:00:00Z"
}
```

**Response `404 Not Found`:**
```json
{
  "error": "Producto no encontrado."
}
```

---

### POST `/api/productos`

Crea un producto nuevo.

> 🔒 Requiere rol `administrador`.

**Headers:**
```
Authorization: Bearer <token>
```

**Request body:**
```json
{
  "codigoProducto": "string (requerido)",
  "nombreProducto": "string (requerido)",
  "precio": "int (requerido)",
  "stockActual": "int (requerido)",
  "descripcion": "string (opcional)",
  "idMarca": "string (opcional)",
  "categoriaId": "string (opcional)",
  "fechaVencimiento": "datetime (opcional)",
  "imagenPrincipal": "string URL (opcional)"
}
```

**Response `201 Created`:**
```json
{
  "idProducto": "string",
  "codigoProducto": "string",
  "nombreProducto": "string",
  "precio": 0,
  "stockActual": 0,
  "descripcion": "string",
  "idMarca": "string",
  "categoriaId": "string",
  "fechaVencimiento": "2026-12-31T00:00:00Z",
  "imagenPrincipal": "string (URL)",
  "fechaCreacion": "2026-01-15T00:00:00Z",
  "fechaActualizacion": "2026-01-15T00:00:00Z"
}
```

**Response `400 Bad Request`:**
```json
{
  "error": "Datos inválidos.",
  "detalles": ["El precio debe ser mayor a 0."]
}
```

**Response `409 Conflict`:**
```json
{
  "error": "El código de producto ya existe."
}
```

---

### POST `/api/productos/bulk`

Crea múltiples productos en una sola solicitud.

> 🔒 Requiere rol `administrador`.

**Headers:**
```
Authorization: Bearer <token>
```

**Request body:**
```json
[
  {
    "codigoProducto": "string (requerido)",
    "nombreProducto": "string (requerido)",
    "precio": "int (requerido)",
    "stockActual": "int (requerido)",
    "descripcion": "string (opcional)",
    "idMarca": "string (opcional)",
    "categoriaId": "string (opcional)",
    "fechaVencimiento": "datetime (opcional)",
    "imagenPrincipal": "string URL (opcional)"
  }
]
```

**Response `200 OK`:**
```json
{
  "totalRecibidos": 10,
  "totalCreados": 8,
  "totalFallidos": 2,
  "creados": [
    {
      "idProducto": "string",
      "codigoProducto": "string",
      "nombreProducto": "string"
    }
  ],
  "errores": [
    {
      "codigoProducto": "string",
      "error": "El código de producto ya existe."
    }
  ]
}
```

>  Si un registro falla, los demás válidos se insertan de todos modos.

---

### PUT `/api/productos/:id`

Actualiza un producto existente.

> 🔒 Requiere rol `administrador`.

**Headers:**
```
Authorization: Bearer <token>
```

**Path params:** `:id` — ID del producto

**Request body:** mismos campos que POST (parcial o completo según implementación).

**Response `200 OK`:** objeto producto actualizado.

**Response `404 Not Found`:** producto no encontrado.

---

### DELETE `/api/productos/:id`

Elimina un producto del catálogo.

> 🔒 Requiere rol `administrador`.

**Headers:**
```
Authorization: Bearer <token>
```

**Path params:** `:id` — ID del producto

**Response `204 No Content`**

**Response `404 Not Found`:** producto no encontrado.

---

## 4. Carrito

### GET `/api/carrito`

Obtiene el carrito activo del usuario autenticado.

> 🔒 Requiere `tipoUsuario: cliente`.

**Headers:**
```
Authorization: Bearer <token>
```

**Response `200 OK`:**
```json
{
  "carritoId": "string",
  "items": [
    {
      "articuloId": "string",
      "productoId": "string",
      "nombreProducto": "string",
      "cantidad": 0,
      "precioUnitario": 0,
      "subtotal": 0
    }
  ],
  "total": 0
}
```

---

### POST `/api/carrito/articulos`

Agrega un producto al carrito del usuario autenticado.

> 🔒 Requiere `tipoUsuario: cliente`.

**Headers:**
```
Authorization: Bearer <token>
```

**Request body:**
```json
{
  "productoId": "string (requerido)",
  "cantidad": "int (requerido)"
}
```

**Response `200 OK`:**
```json
{
  "articuloId": "string",
  "productoId": "string",
  "nombreProducto": "string",
  "cantidad": 0,
  "subtotal": 0
}
```

**Response `400 Bad Request`:**
```json
{
  "error": "Stock insuficiente. Disponible: 3."
}
```

---

### PUT `/api/carrito/articulos/:articulo_id`

Actualiza la cantidad de un ítem del carrito.

> 🔒 Requiere `tipoUsuario: cliente`.

**Headers:**
```
Authorization: Bearer <token>
```

**Path params:**
```
:articulo_id   ID del artículo en el carrito
```

**Request body:**
```json
{
  "cantidad": "int (requerido)"
}
```

**Response `200 OK`:**
```json
{
  "articuloId": "string",
  "productoId": "string",
  "cantidad": 0,
  "subtotal": 0
}
```

---

### DELETE `/api/carrito/articulos/:articulo_id`

Elimina un artículo del carrito.

> 🔒 Requiere `tipoUsuario: cliente`.

**Headers:**
```
Authorization: Bearer <token>
```

**Path params:**
```
:articulo_id   ID del artículo en el carrito
```

**Response `204 No Content`**

**Response `404 Not Found`:**
```json
{
  "error": "Artículo no encontrado en el carrito."
}
```

---

## 5. Reservaciones

### POST `/api/reservacion`

Crea una reservación al finalizar la compra.

> 🔒 Requiere `tipoUsuario: cliente`.

**Headers:**
```
Authorization: Bearer <token>
```

**Request body:**
```json
{
  "metodoPagoId": "string (requerido)"
}
```

>  El carrito activo del usuario se convierte automáticamente en la reservación. No es necesario enviar los ítems.

**Response `201 Created`:**
```json
{
  "idReservacion": "string",
  "codigoReservacion": "string",
  "clienteId": "string",
  "estado": "pendiente",
  "totalReservacion": 0,
  "metodoPagoId": "string",
  "pagado": false,
  "fechaLimiteRetiro": "2026-04-20T00:00:00Z",
  "items": [
    {
      "productoId": "string",
      "nombreProducto": "string",
      "cantidad": 0,
      "precioUnitario": 0,
      "subtotal": 0
    }
  ]
}
```

**Response `400 Bad Request`:**
```json
{
  "error": "El carrito está vacío."
}
```

---

### GET `/api/reservacion`

Obtiene el historial de reservaciones del usuario autenticado.

>  Si el token pertenece a un `administrador`, devuelve todas las reservaciones del sistema. Si pertenece a un `cliente`, devuelve solo las suyas.

**Headers:**
```
Authorization: Bearer <token>
```

**Response `200 OK`:**
```json
[
  {
    "idReservacion": "string",
    "codigoReservacion": "string",
    "clienteId": "string",
    "estado": "string",
    "totalReservacion": 0,
    "pagado": false,
    "fechaLimiteRetiro": "2026-04-20T00:00:00Z"
  }
]
```

---

### GET `/api/reservacion/:id`

Obtiene el detalle de una reservación específica.

**Headers:**
```
Authorization: Bearer <token>
```

**Path params:**
```
:id   ID de la reservación
```

**Response `200 OK`:**
```json
{
  "idReservacion": "string",
  "codigoReservacion": "string",
  "clienteId": "string",
  "estado": "string",
  "totalReservacion": 0,
  "metodoPagoId": "string",
  "pagado": false,
  "observaciones": "string",
  "fechaLimiteRetiro": "2026-04-20T00:00:00Z",
  "items": [
    {
      "productoId": "string",
      "nombreProducto": "string",
      "cantidad": 0,
      "precioUnitario": 0,
      "subtotal": 0
    }
  ]
}
```

**Response `403 Forbidden`:**
```json
{
  "error": "No tenés permisos para ver esta reservación."
}
```

**Response `404 Not Found`:**
```json
{
  "error": "Reservación no encontrada."
}
```

---

## 6. Pagos (Stripe)

>  **Variables de entorno requeridas:**
> - `STRIPE_SECRET_KEY` — solo en el backend, nunca exponer
> - `STRIPE_PUBLISHABLE_KEY` — se entrega al frontend
> - `STRIPE_WEBHOOK_SECRET` — solo en el backend, para validar webhooks (o `Stripe:WebhookSecret` en configuración)
>
>  **Sincronización `pagado`:** Stripe notifica pagos exitosos mediante el webhook `POST /api/pagos/webhook`. Sin ese endpoint configurado en el Dashboard de Stripe (o con `stripe listen` en local), el campo `pagado` en la base puede quedar desactualizado hasta que el cliente llame a `GET /api/pagos/:id/status`, que también reconcilia el estado si Stripe ya reporta `succeeded`.

---

### POST `/api/pagos/create-intent`

Crea un `PaymentIntent` en Stripe para iniciar el flujo de pago. El monto se calcula desde la reservación en la base de datos; **nunca se acepta el monto desde el cliente.**

**Headers:**
```
Authorization: Bearer <token>
```

**Request body:**
```json
{
  "reservacionId": "string (requerido)"
}
```

**Response `200 OK`:**
```json
{
  "clientSecret": "pi_xxx_secret_xxx",
  "reservacionId": "string",
  "montoCentavos": 15000,
  "moneda": "usd"
}
```

>  El frontend usa el `clientSecret` con Stripe.js para capturar los datos de tarjeta directamente en Stripe. Los datos de tarjeta **nunca pasan por tu servidor.**

**Response `400 Bad Request`:**
```json
{
  "error": "La reservación ya fue pagada o no existe."
}
```

---

### POST `/api/pagos/webhook`

 **Endpoint crítico.** Stripe llama automáticamente este endpoint cuando ocurre un evento de pago. Aquí se confirma la orden en la base de datos (`pagado = true` cuando el `PaymentIntent` coincide con la reserva).

> Este endpoint **no requiere JWT**. La autenticación se hace validando la firma del header `Stripe-Signature` con el `STRIPE_WEBHOOK_SECRET`.

> En **Stripe Dashboard → Developers → Webhooks**, la URL debe ser `https://<tu-api>/api/pagos/webhook`. En local, usar la CLI: `stripe listen --forward-to http://localhost:<puerto>/api/pagos/webhook` y copiar el *signing secret* como `STRIPE_WEBHOOK_SECRET`.

**Headers:**
```
Stripe-Signature: t=xxx,v1=xxx
Content-Type: application/json
```

**Request body:** Payload raw del evento Stripe (no modificar).

**Eventos manejados:**

| Evento | Acción |
|--------|--------|
| `payment_intent.succeeded` | Marcar reservación como pagada en DB |
| `payment_intent.payment_failed` | Registrar fallo, notificar al usuario |
| `charge.refunded` | Procesar reembolso en el sistema |

**Response `200 OK`:**
```json
{
  "recibido": true
}
```

**Response `400 Bad Request`:**
```json
{
  "error": "Firma de webhook inválida."
}
```

**Response `500 Internal Server Error`** (p. ej. `STRIPE_WEBHOOK_SECRET` no configurado en el servidor):
```json
{
  "error": "STRIPE_WEBHOOK_SECRET no configurado."
}
```

>  Si este endpoint responde con cualquier código que no sea 2xx, Stripe reintentará el evento hasta 3 días.

---

### GET `/api/pagos/:paymentIntentId/status`

Consulta el estado actual de un pago. **Además**, si Stripe devuelve `succeeded` y la reservación asociada aún tiene `pagado = false`, el backend actualiza la base en la misma petición (útil cuando el webhook no está disponible o llega tarde).

**Headers:**
```
Authorization: Bearer <token>
```

**Path params:**
```
:paymentIntentId   ID del PaymentIntent de Stripe (formato: pi_xxx)
```

**Response `200 OK`:**
```json
{
  "paymentIntentId": "string",
  "status": "succeeded | processing | requires_payment_method | canceled",
  "reservacionId": "string",
  "montoCentavos": 15000,
  "moneda": "usd"
}
```

---

### POST `/api/pagos/:paymentIntentId/refund`

Inicia un reembolso parcial o total a través de Stripe.

> 🔒 Requiere rol `administrador`.

**Headers:**
```
Authorization: Bearer <token>
```

**Path params:**
```
:paymentIntentId   ID del PaymentIntent de Stripe
```

**Request body:**
```json
{
  "montoCentavos": "int (opcional — si se omite, reembolso total)",
  "razon": "duplicate | fraudulent | requested_by_customer (opcional)"
}
```

**Response `200 OK`:**
```json
{
  "reembolsoId": "re_xxx",
  "estado": "succeeded",
  "montoCentavos": 5000,
  "moneda": "usd"
}
```

**Response `400 Bad Request`:**
```json
{
  "error": "El monto de reembolso supera el total del pago."
}
```

---

### GET `/api/metodoPago`

Lista los métodos de pago guardados del usuario autenticado en Stripe.

**Headers:**
```
Authorization: Bearer <token>
```

**Response `200 OK`:**
```json
[
  {
    "id": "string",
    "stripePaymentMethodId": "pm_xxx",
    "alias": "string",
    "marca": "visa | mastercard | amex",
    "ultimosDigitos": "4242",
    "expiraMes": 12,
    "expiraAnio": 2028
  }
]
```

---

### POST `/api/metodoPago`

Guarda un método de pago de Stripe asociado al usuario.

**Headers:**
```
Authorization: Bearer <token>
```

**Request body:**
```json
{
  "stripePaymentMethodId": "pm_xxx (requerido)",
  "alias": "string (opcional, ej: 'Mi Visa personal')"
}
```

**Response `201 Created`:**
```json
{
  "id": "string",
  "stripePaymentMethodId": "pm_xxx",
  "alias": "string",
  "marca": "visa",
  "ultimosDigitos": "4242",
  "expiraMes": 12,
  "expiraAnio": 2028
}
```

---

### DELETE `/api/metodoPago/:id`

Desvincula un método de pago guardado del usuario.

**Headers:**
```
Authorization: Bearer <token>
```

**Path params:**
```
:id   ID interno del método de pago
```

**Response `204 No Content`**

**Response `404 Not Found`:**
```json
{
  "error": "Método de pago no encontrado."
}
```

---

## 7. Admin — usuarios

Prefijo: `/api/admin/usuarios`

> 🔒 Requiere JWT con `tipo_usuario: administrador` y `rol: administrador` (no cajero), salvo donde se indique super admin.

### GET `/api/admin/usuarios`

Lista usuarios del sistema.

**Query params:** `busqueda`, `tipoUsuario`, `estado` (opcionales)

**Response `200 OK`:** array de usuarios con `id`, `nombre`, `apellido`, `correo`, `telefono`, `tipoUsuario`, `rol`, `estado`, `fechaCreacion`.

---

### POST `/api/admin/usuarios`

Crea un usuario (cliente, cajero o administrador).

**Request body:**
```json
{
  "correo": "string",
  "nombre": "string",
  "contrasena": "string (mín. 8 caracteres)",
  "tipoUsuario": "cliente | administrador",
  "rol": "cajero | administrador (si tipoUsuario es administrador)",
  "tipoCliente": "particular | minorista | mayorista (si cliente)",
  "apellido": "string (opcional)",
  "telefono": "string (opcional)",
  "direccion": "string (opcional, cliente)"
}
```

**Response `201 Created`:** usuario creado.

---

### PUT `/api/admin/usuarios/:id/estado`

Activa o desactiva un usuario.

**Request body:**
```json
{
  "estado": true
}
```

**Response `200 OK`:** usuario actualizado.

---

### PUT `/api/admin/usuarios/:id/rol`

Cambia el rol de un usuario. **Solo super administrador** (`es_super_admin` o correo `SUPER_ADMIN_EMAIL`).

**Request body:**
```json
{
  "rol": "cliente | cajero | administrador",
  "tipoCliente": "particular (opcional, al pasar a cliente)"
}
```

**Response `200 OK`:** usuario actualizado.

**Response `403 Forbidden`:** no es super admin.

**Response `400 Bad Request`:** no se puede modificar el rol del super administrador.

---

## 8. Admin — ventas

Prefijo: `/api/admin/ventas`

> 🔒 Admin o cajero (`tipo_usuario: administrador`).

### GET `/api/admin/ventas`

Historial de ventas con resumen del día.

**Query params:** `busqueda`, `fecha`, `filtroPrecio`, `filtroMetodoPago`

**Response `200 OK`:**
```json
{
  "resumen": {
    "ventasHoy": 0,
    "ingresosHoy": 0,
    "ticketPromedio": 0,
    "productosVendidos": 0
  },
  "ventas": []
}
```

---

## 9. Admin — reportes

Prefijo: `/api/admin/reportes`

> 🔒 Admin o cajero.

**Query común:** `desde`, `hasta` (ISO), `modo` (`todos` | `ventas` | `reservaciones`) en reportes que lo soportan.

| Método | Ruta | Descripción |
|--------|------|-------------|
| GET | `/productos` | Productos más vendidos, KPIs y gráficos |
| GET | `/empleados` | Rendimiento por cajero |
| GET | `/demanda` | Demanda por horario |
| GET | `/metodos-pago` | Distribución por método de pago |
| GET | `/stock-critico` | Productos bajo stock mínimo |

---

## Códigos de error comunes

| Código | Significado |
|--------|-------------|
| `400` | Bad Request — datos inválidos o faltantes |
| `401` | Unauthorized — token ausente, expirado o inválido |
| `403` | Forbidden — sin permisos para este recurso |
| `404` | Not Found — recurso no encontrado |
| `409` | Conflict — recurso duplicado (correo, código de producto) |
| `500` | Internal Server Error — error inesperado del servidor |

---

## Operación y herramientas

### Base de datos (Docker)

- Esquema: `db/elohim_db.sql` (init Postgres)
- **No** se usa `dotnet ef database update` en Docker
- Seed demo: `SEED_DATA=true` en `backend/.env` (alias `SEED_DEMO_DATA`). Ver `backend/.env.example`.

### Colección Bruno

Pruebas HTTP en `backend/bruno/` con entorno `local` (`baseUrl`, tokens).

---

## Referencia rápida

| Método | Ruta | Auth | Descripción |
|--------|------|------|-------------|
| POST | `/api/auth/register` | No* | Registro (*admin requiere JWT admin) |
| POST | `/api/auth/login` | No | Login (JWT) |
| POST | `/api/auth/logout` | Bearer | Revoca token |
| POST | `/api/auth/forgot-password` | No | Recuperación (stub) |
| GET | `/api/marcas` | No | Listar marcas |
| GET | `/api/categorias` | No | Listar categorías |
| GET | `/api/productos` | No | Listado paginado |
| GET | `/api/productos/buscar` | No | Búsqueda |
| GET | `/api/productos/{id}` | No | Detalle |
| POST | `/api/productos` | Admin | Crear |
| PUT | `/api/productos/{id}` | Admin | Actualizar |
| DELETE | `/api/productos/{id}` | Admin | Eliminar |
| GET | `/api/carrito` | Cliente | Ver carrito |
| POST | `/api/carrito/articulos` | Cliente | Agregar ítem |
| POST | `/api/reservacion` | Cliente | Crear reserva |
| GET | `/api/admin/usuarios` | Admin | Listar usuarios |
| PUT | `/api/admin/usuarios/{id}/rol` | Super admin | Cambiar rol |
| GET | `/api/admin/ventas` | Staff | Ventas |
| GET | `/api/admin/reportes/*` | Staff | Reportes |

Controladores adicionales: `CarritoController`, `ReservacionController`, `PagosController`, `MetodoPagoController`, `UsuarioController` en `src/ElohimShop.API/Controllers/`.