### GET `/api/admin/inventario`

Reporte de inventario para administradores. Devuelve resumen agregados y lista de productos con filtros, ordenamiento y paginación.

> 🔒 Requiere rol `administrador`.

**Headers:**
```
Authorization: Bearer <token>
```

**Query params:**
```
?q=string                     (opcional) Búsqueda por nombre o código
?categoriaId=string           (opcional) ID de categoría
?estado=critico|normal|agotado (opcional) Filtrar por estado de stock
?orderBy=nombre|stockActual|precio|fechaVencimiento (opcional)
?order=asc|desc               (opcional, default: desc)
?page=int                     (opcional, default: 1)
?limit=int                    (opcional, default: 20)
```

**Response `200 OK`:**
```json
{
  "resumen": {
    "totalProductos": 10,
    "stockNormal": 7,
    "stockCritico": 3,
    "valorInventario": 42350
  },
  "productos": [
    {
      "idProducto": "string",
      "codigoProducto": "string",
      "nombreProducto": "string",
      "categoria": { "id": "string", "nombre": "string" },
      "marca": { "id": "string", "nombre": "string" },
      "precio": 0,
      "stockActual": 0,
      "stockMinimo": 0,
      "estado": "critico | normal | agotado",
      "valorStock": 0,
      "fechaVencimiento": "2026-12-31T00:00:00Z",
      "imagenPrincipal": "string"
    }
  ],
  "total": 100,
  "pagina": 1,
  "limite": 20
}
```

**Response `401 Unauthorized`:**
```json
{
  "error": "Token inválido, expirado o ausente."
}
```

**Response `403 Forbidden`:**
```json
{
  "error": "No tenés permisos (rol administrador requerido)."
}
```

### GET `/api/admin/inventario/exportar`

Exporta el inventario filtrado a CSV. Usa los mismos filtros que GET `/api/admin/inventario` y devuelve el archivo directamente con `Content-Type: text/csv` y `Content-Disposition: attachment; filename=inventario.csv`.

**Query params:**
```
?q=string                     (opcional) Búsqueda por nombre o código
?categoriaId=string           (opcional) ID de categoría
?estado=critico|normal|agotado (opcional) Filtrar por estado de stock
?orderBy=nombre|stockActual|precio|fechaVencimiento (opcional)
?order=asc|desc               (opcional, default: desc)
```

**CSV columns:**
`código, nombre, categoría, marca, precio, stock actual, stock mínimo, estado, valor stock, fecha vencimiento, descuento activo, fecha fin oferta`
