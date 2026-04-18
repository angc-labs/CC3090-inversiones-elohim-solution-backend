# Guía de Testing - API ElohimShop

> **Base URL:** `http://localhost:5000` (ajustar según tu configuración)

> **Nota:** Todos los endpoints que requieren autenticación deben incluir el header `Authorization: Bearer <token>`.

---

## Autenticación

### Registrar cliente
```bash
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "correo": "cliente@test.com",
    "nombre": "Juan",
    "contrasena": "Password123!",
    "tipoUsuario": "cliente",
    "tipoCliente": "minorista"
  }'
```

### Login
```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "correo": "cliente@test.com",
    "contrasena": "Password123!"
  }'
```

> **Response:** Devuelve JWT token. Usar en todos los requests siguientes.

---

## Carrito

> Reemplazar `<TOKEN>` con el JWT recibido del login.

### GET /api/carrito
Obtiene el carrito activo del usuario.

```bash
curl -X GET http://localhost:5000/api/carrito \
  -H "Authorization: Bearer <TOKEN>"
```

**Response (200):**
```json
{
  "carritoId": "abc-123",
  "items": [
    {
      "articuloId": "item-1",
      "productoId": "prod-001",
      "nombreProducto": "Producto A",
      "cantidad": 2,
      "precioUnitario": 100,
      "subtotal": 200
    }
  ],
  "total": 200
}
```

### POST /api/carrito/articulos
Agrega un producto al carrito.

```bash
curl -X POST http://localhost:5000/api/carrito/articulos \
  -H "Authorization: Bearer <TOKEN>" \
  -H "Content-Type: application/json" \
  -d '{
    "productoId": "prod-001",
    "cantidad": 2
  }'
```

**Response (200):**
```json
{
  "articuloId": "item-1",
  "productoId": "prod-001",
  "nombreProducto": "Producto A",
  "cantidad": 2,
  "subtotal": 200
}
```

**Response (400) - Stock insuficiente:**
```json
{
  "error": "Stock insuficiente. Disponible: 3."
}
```

### PUT /api/carrito/articulos/:articulo_id
Actualiza la cantidad de un artículo.

```bash
curl -X PUT http://localhost:5000/api/carrito/articulos/item-1 \
  -H "Authorization: Bearer <TOKEN>" \
  -H "Content-Type: application/json" \
  -d '{
    "cantidad": 5
  }'
```

### DELETE /api/carrito/articulos/:articulo_id
Elimina un artículo del carrito.

```bash
curl -X DELETE http://localhost:5000/api/carrito/articulos/item-1 \
  -H "Authorization: Bearer <TOKEN>"
```

**Response (204):** No Content

---

## Reservaciones

### POST /api/reservacion
Crea una reservación desde el carrito.

```bash
curl -X POST http://localhost:5000/api/reservacion \
  -H "Authorization: Bearer <TOKEN>" \
  -H "Content-Type: application/json" \
  -d '{
    "metodoPagoId": "metodo-123"
  }'
```

**Response (201):**
```json
{
  "idReservacion": "res-123",
  "codigoReservacion": "RES-20260418-ABC123",
  "clienteId": "user-123",
  "estado": "pendiente",
  "totalReservacion": 200,
  "metodoPagoId": "metodo-123",
  "pagado": false,
  "fechaLimiteRetiro": "2026-04-21T00:00:00Z",
  "items": [
    {
      "productoId": "prod-001",
      "nombreProducto": "Producto A",
      "cantidad": 2,
      "precioUnitario": 100,
      "subtotal": 200
    }
  ]
}
```

**Response (400) - Carrito vacío:**
```json
{
  "error": "El carrito está vacío."
}
```

### GET /api/reservacion
Lista las reservaciones del usuario.

```bash
curl -X GET http://localhost:5000/api/reservacion \
  -H "Authorization: Bearer <TOKEN>"
```

**Response (200):**
```json
[
  {
    "idReservacion": "res-123",
    "codigoReservacion": "RES-20260418-ABC123",
    "clienteId": "user-123",
    "estado": "pendiente",
    "totalReservacion": 200,
    "pagado": false,
    "fechaLimiteRetiro": "2026-04-21T00:00:00Z"
  }
]
```

### GET /api/reservacion/:id
Obtiene detalle de una reservación.

```bash
curl -X GET http://localhost:5000/api/reservacion/res-123 \
  -H "Authorization: Bearer <TOKEN>"
```

**Response (404):**
```json
{
  "error": "Reservación no encontrada."
}
```

---

## Catálogo

### GET /api/productos
Lista productos con paginación.

```bash
curl -X GET "http://localhost:5000/api/productos?page=1&limit=20"
```

### GET /api/productos/:id
Detalle de un producto.

```bash
curl -X GET http://localhost:5000/api/productos/prod-001
```

### GET /api/marcas
Lista todas las marcas.

```bash
curl -X GET http://localhost:5000/api/marcas
```

### GET /api/categorias
Lista todas las categorías.

```bash
curl -X GET http://localhost:5000/api/categorias
```

---

## Errores Comunes

| Código | Descripción |
|--------|-------------|
| 400 | Bad Request - datos inválidos |
| 401 | Unauthorized - token ausente o inválido |
| 403 | Forbidden - sin permisos |
| 404 | Not Found - recurso no existe |
| 409 | Conflict - recurso duplicado |

---

## Scripts de Test con jq

### Extraer token del login:
```bash
TOKEN=$(curl -s -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"correo":"cliente@test.com","contrasena":"Password123!"}' \
  | jq -r '.token')
```

### Usar token en requests:
```bash
curl -X GET http://localhost:5000/api/carrito \
  -H "Authorization: Bearer $TOKEN"
```