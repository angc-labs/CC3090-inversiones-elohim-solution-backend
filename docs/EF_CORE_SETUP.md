# Entity Framework Core - Configuración PostgreSQL

## Resumen de Implementación

Se han configurado completamente las entidades del dominio con Entity Framework Core para mapear a PostgreSQL.

### Estructura Creada

#### 1. **DbContext** ([ElohimShopDbContext.cs](src/ElohimShop.Infrastructure/Persistence/ElohimShopDbContext.cs))
- Contexto central que gestiona todas las entidades del dominio
- Registra 12 DbSets para todas las tablas
- Aplica configuraciones desde el assembly de Infrastructure (`ApplyConfigurationsFromAssembly`)

#### 2. **Configuraciones Fluent API** (`src/ElohimShop.Infrastructure/Persistence/Configurations/`)
Se crearon 12 archivos de configuración, uno por cada entidad:

| Entidad | Archivo | Características |
|---------|---------|-----------------|
| **Rol** | `RolConfiguration.cs` | Tablas maestras, relación 1:N con Administrador |
| **Marca** | `MarcaConfiguration.cs` | Tablas maestras, relación 1:N con Producto |
| **Categoria** | `CategoriaConfiguration.cs` | Tablas maestras, relación 1:N con Producto |
| **MetodoPago** | `MetodoPagoConfiguration.cs` | Tablas maestras, relación 1:N con Reservacion |
| **TipoCliente** | `TipoClienteConfiguration.cs` | Tablas maestras, relación 1:N con Cliente |
| **Cliente** | `ClienteConfiguration.cs` | Usuario final, índice único en Correo, FK con TipoCliente |
| **Administrador** | `AdministradorConfiguration.cs` | Personal interno, índice único en Correo, FK con Rol, enum Estado |
| **Consulta** | `ConsultaConfiguration.cs` | Relación M2M entre Cliente y Administrador, cascada en delete |
| **Producto** | `ProductoConfiguration.cs` | Catálogo, índice único en CodigoProducto, FK con Marca y Categoria |
| **Reservacion** | `ReservacionConfiguration.cs` | Transacción, enum EstadoRenovacion, relación 1:1 con Venta |
| **DetalleReservacion** | `DetalleReservacionConfiguration.cs` | Línea de detalle, columna computada Subtotal, cascada en delete |
| **Venta** | `VentaConfiguration.cs` | Transacción finalizada, enum EstadoVenta, índice único en ReservacionId |

#### 3. **Enums** (`src/ElohimShop.Domain/Enums/`)
- **EstadoAdministrador**: Activo, Inactivo, Bloqueado
- **EstadoRenovacion**: Pendiente, Renovada, Vencida, Cancelada
- **EstadoVenta**: Pendiente, Pagada, Anulada, Completada

Los enums se almacenan como strings en PostgreSQL mediante conversión en Fluent API.

### Convenciones Aplicadas

#### Naming de Columnas
- Se respetan los nombres del esquema PostgreSQL original
- Uso explícito de `HasColumnName()` para columnas con snake_case
- Propiedades de C# en PascalCase, columnas en snake_case en BD

#### Tipos de Datos
- Strings: `VARCHAR(n)` con máximas longitudes configuradas
- Numéricos: `INTEGER` para precios con unidad mínima, `NUMERIC` para derivados
- Fechas: `TIMESTAMP` para todas las fechas (UTC)
- Booleanos: `BOOLEAN`
- Campos de texto largo: `TEXT`
- Enums: `VARCHAR(n)` con conversión automática

#### Restricciones de Clave Foránea
```
DeleteBehavior.Cascade   → Si se elimina el padre, se eliminan los hijos
DeleteBehavior.SetNull   → Si se elimina el padre, la FK del hijo se vuelve NULL
```

Aplicadas según reglas de negocio:
- Consultas y DetalleReservacion: CASCADE (lógicamente dependientes)
- Demás relaciones opcionales: SET_NULL (independencia de datos)

#### Índices
- Únicos: `Correo` (Cliente, Administrador), `CodigoProducto` (Producto), `CodigoReservacion` (Reservacion), `ReservacionId` (Venta)
- Normales: Todas las claves foráneas

### Columna Computada
```sql
DetalleReservacion.Subtotal = Cantidad * PrecioUnitario (STORED)
```

### Configuración de Conexión

#### appsettings.json
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=elohim_db;Username=postgres;Password=postgres"
  }
}
```

#### Program.cs
```csharp
builder.Services.AddDbContext<ElohimShopDbContext>(options =>
    options.UseNpgsql(connectionString));
```

### Migración Inicial

Se generó la migración inicial con:
```bash
dotnet ef migrations add InitialCreate --project src/ElohimShop.Infrastructure --startup-project src/ElohimShop.API
```

Archivo generado: `Migrations/20260402143754_InitialCreate.cs`

Para aplicar la migración:
```bash
dotnet ef database update --project src/ElohimShop.Infrastructure --startup-project src/ElohimShop.API
```

### Dependencias Agregadas

- `Microsoft.EntityFrameworkCore` (v10.0.5)
- `Microsoft.EntityFrameworkCore.Design` (v10.0.5)
- `Npgsql.EntityFrameworkCore.PostgreSQL` (v10.0.1)

### Validación

✅ Compilación exitosa: `dotnet build ElohimShop.slnx`  
✅ Migración generada correctamente  
✅ Todas las relaciones mapeadas  
✅ Enums configurados para persistencia  
✅ Índices y restricciones configurados  

### Próximos Pasos

1. Ejecutar migración en base de datos de desarrollo
2. Crear repositorios en `Infrastructure/Repositories/`
3. Implementar casos de uso en `Application/`
4. Exponer endpoints en `API/Controllers/`
