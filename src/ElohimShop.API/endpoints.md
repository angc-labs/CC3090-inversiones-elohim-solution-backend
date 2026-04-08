# Endpoints API

Base path: `/api/client`

Base path productos: `/api/product`

## POST `/api/client/register`
Registra un cliente nuevo y devuelve JWT.

Request body:
- `correo` (string, requerido)
- `nombre` (string, requerido)
- `contrasena` (string, requerido)
- `apellido` (string, opcional)
- `telefono` (string, opcional)
- `direccion` (string, opcional)

Response `200 OK`:
- `clienteId`
- `correo`
- `nombre`
- `token`
- `expiraEn` (vigencia: 1 mes)

Response `409 Conflict`:
- correo ya registrado

## POST `/api/client/login`
Inicia sesión con un cliente existente y devuelve JWT.

Request body:
- `correo` (string, requerido)
- `contrasena` (string, requerido)

Response `200 OK`:
- `clienteId`
- `correo`
- `nombre`
- `token`
- `expiraEn` (vigencia: 1 mes)

Response `401 Unauthorized`:
- credenciales inválidas o cuenta inactiva

## POST `/api/client/logout`
Revoca el JWT actual.

Headers:
- `Authorization: Bearer <token>`

Response `204 No Content`

Response `401 Unauthorized`:
- token inválido, expirado o revocado

## POST `/api/product`
Crea un producto nuevo.

Request body:
- `codigoProducto` (string, requerido)
- `nombreProducto` (string, requerido)
- `precio` (int, requerido)
- `stockActual` (int, requerido)
- `descripcion` (string, opcional)
- `idMarca` (string, opcional)
- `categoriaId` (string, opcional)
- `fechaVencimiento` (datetime, opcional)
- `imagenPrincipal` (string, opcional)

Response `200 OK`:
- `idProducto`
- `codigoProducto`
- `nombreProducto`
- `precio`
- `stockActual`
- `descripcion`
- `idMarca`
- `categoriaId`
- `fechaVencimiento`
- `imagenPrincipal`
- `fechaCreacion`
- `fechaActualizacion`

Response `400 Bad Request`:
- datos invalidos

Response `409 Conflict`:
- codigo de producto ya registrado

## GET `/api/product`
Consulta todos los productos registrados.

Response `200 OK`:
- arreglo de productos con los mismos campos de la creacion

## Documentación
- Swagger UI: `/swagger`
- OpenAPI JSON: `/swagger/v1/swagger.json`
