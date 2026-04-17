# Elohim Tienda Online — Documentación de Endpoints API

> **Autenticación:** Bearer Token (JWT) en el header `Authorization: Bearer <token>`

> **Formato:** JSON en todos los requests y responses

## Índice

1. [Autenticación](#1-autenticación)
2. [Perfil de usuario](#2-perfil-de-usuario)
3. [Catálogo](#3-catálogo)
4. [Carrito](#4-carrito)
5. [Reservaciones](#5-reservaciones)
6. [Pagos (Stripe)](#6-pagos-stripe)

---

## 1. Autenticación

### POST `/api/auth/register`

Registra un nuevo usuario (cliente o administrador) y devuelve un JWT.

>  El registro de administradores requiere un JWT con rol `administrador` en el header. El registro de clientes es público.

**Request body — cliente:**
```json
{
  "correo": "string (requerido)",
  "nombre": "string (requerido)",
  "contrasena": "string (requerido)",
  "tipoUsuario": "cliente",
  "tipoCliente": "mayorista | minorista | particular (requerido para clientes)",
  "apellido": "string (opcional)",
  "telefono": "string (opcional)",
  "direccion": "string (opcional)"
}
```

**Request body — administrador:**
```json
{
  "correo": "string (requerido)",
  "nombre": "string (requerido)",
  "contrasena": "string (requerido)",
  "tipoUsuario": "administrador",
  "rol": "cajero | administrador (requerido para administradores)",
  "apellido": "string (opcional)",
  "telefono": "string (opcional)"
}
```

**Response `201 Created`:**
```json
{
  "usuarioId": "string",
  "correo": "string",
  "nombre": "string",
  "tipoUsuario": "cliente | administrador",
  "token": "string",
  "expiraEn": "2026-05-15T00:00:00Z"
}
```

**Response `400 Bad Request`:**
```json
{
  "error": "Datos inválidos.",
  "detalles": ["El campo tipoCliente es requerido para usuarios de tipo cliente."]
}
```

**Response `403 Forbidden`:**
```json
{
  "error": "No tenés permisos para registrar administradores."
}
```

**Response `409 Conflict`:**
```json
{
  "error": "El correo ya está registrado."
}
```

---

### POST `/api/auth/login`

Inicia sesión con cualquier tipo de usuario y devuelve un JWT.

**Request body:**
```json
{
  "correo": "string (requerido)",
  "contrasena": "string (requerido)"
}
```

**Response `200 OK`:**
```json
{
  "usuarioId": "string",
  "correo": "string",
  "nombre": "string",
  "tipoUsuario": "cliente | administrador",
  "rol": "cajero | administrador | null",
  "tipoCliente": "mayorista | minorista | particular | null",
  "token": "string",
  "expiraEn": "2026-05-15T00:00:00Z"
}
```

>  `rol` solo viene poblado si `tipoUsuario` es `administrador`. `tipoCliente` solo viene poblado si `tipoUsuario` es `cliente`.

**Response `401 Unauthorized`:**
```json
{
  "error": "Credenciales inválidas o cuenta inactiva."
}
```

---

### POST `/api/auth/logout`

Revoca el JWT actual del usuario autenticado.

**Headers:**
```
Authorization: Bearer <token>
```

**Response `204 No Content`**

**Response `401 Unauthorized`:**
```json
{
  "error": "Token inválido, expirado o ya revocado."
}
```

---

### POST `/api/auth/forgot-password`

Envía un correo con enlace de recuperación de contraseña.

**Request body:**
```json
{
  "correo": "string (requerido)"
}
```

**Response `200 OK`:**
```json
{
  "mensaje": "Si el correo existe, recibirás un enlace de recuperación."
}
```

>  La respuesta es genérica para no revelar si el correo está registrado.

---

## 2. Perfil de usuario

### GET `/api/usuario/me`

Obtiene el perfil del usuario autenticado. La respuesta varía según el tipo de usuario.

**Headers:**
```
Authorization: Bearer <token>
```

**Response `200 OK` — cliente:**
```json
{
  "usuarioId": "string",
  "nombre": "string",
  "apellido": "string",
  "correo": "string",
  "telefono": "string",
  "tipoUsuario": "cliente",
  "tipoCliente": "mayorista | minorista | particular",
  "direccion": "string",
  "fechaRegistro": "2026-01-01T00:00:00Z"
}
```

**Response `200 OK` — administrador:**
```json
{
  "usuarioId": "string",
  "nombre": "string",
  "apellido": "string",
  "correo": "string",
  "telefono": "string",
  "tipoUsuario": "administrador",
  "rol": "cajero | administrador",
  "fechaCreacion": "2026-01-01T00:00:00Z"
}
```

---

### PUT `/api/usuario/me`

Actualiza la información del perfil del usuario autenticado.

**Headers:**
```
Authorization: Bearer <token>
```

**Request body** (todos los campos son opcionales):
```json
{
  "nombre": "string",
  "apellido": "string",
  "correo": "string",
  "contrasena": "string",
  "telefono": "string",
  "direccion": "string (solo clientes)"
}
```

**Response `200 OK`:**
```json
{
  "usuarioId": "string",
  "nombre": "string",
  "apellido": "string",
  "correo": "string",
  "telefono": "string"
}
```

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
> - `STRIPE_WEBHOOK_SECRET` — solo en el backend, para validar webhooks

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

 **Endpoint crítico.** Stripe llama automáticamente este endpoint cuando ocurre un evento de pago. Aquí se confirma la orden en la base de datos.

> Este endpoint **no requiere JWT**. La autenticación se hace validando la firma del header `Stripe-Signature` con el `STRIPE_WEBHOOK_SECRET`.

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

>  Si este endpoint responde con cualquier código que no sea 2xx, Stripe reintentará el evento hasta 3 días.

---

### GET `/api/pagos/:paymentIntentId/status`

Consulta el estado actual de un pago. Útil como fallback si el webhook tarda en llegar o falla.

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

## Códigos de error comunes

| Código | Significado |
|--------|-------------|
| `400` | Bad Request — datos inválidos o faltantes |
| `401` | Unauthorized — token ausente, expirado o inválido |
| `403` | Forbidden — sin permisos para este recurso |
| `404` | Not Found — recurso no encontrado |
| `409` | Conflict — recurso duplicado (correo, código de producto) |
| `500` | Internal Server Error — error inesperado del servidor |