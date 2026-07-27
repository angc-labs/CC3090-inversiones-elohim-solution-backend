# Seguridad Backend

## Alcance
Auditoría estrictamente de backend: `backend/src` y documentación en `backend/docs`.

Se revisaron:
- `Program.cs`
- `Middleware/TenantResolverMiddleware.cs`
- `Auth/BetterAuthSessionMiddleware.cs`
- Controllers en `backend/src/ElohimShop.API/Controllers`
- Servicios de auth, pagos y persistencia relevantes
- Configuración de appsettings y manejo de secretos

## 1. Hallazgos principales

### 1.1 Manejo de secretos
- `Program.cs` depende de variables de entorno y configuración para `JWT_KEY`, `JWT_ISSUER`, `JWT_AUDIENCE`, `STRIPE_SECRET_KEY`, `STRIPE_WEBHOOK_SECRET`, `STRIPE_PUBLISHABLE_KEY`.
- `backend/src/ElohimShop.API/appsettings.json` incluye la clave JWT en texto plano. Aunque puede ser de ejemplo, esto no debe dejarse en un repositorio activo.
- `backend/src/ElohimShop.API/appsettings.Development.json` contiene credenciales de `SuperAdmin` y contraseñas de base de datos de desarrollo.
- `PlatformDatabaseBootstrapper` inicializa un usuario de base de datos de demo con contraseña estática `ReadOnlyPassword123!`.
- Las credenciales de Stripe y SMTP se almacenan en `CredencialesIntegraciones` sin cifrado.
- `SuperAdminSeeder` usa `SuperAdmin123!` si no se configura `SUPER_ADMIN_PASSWORD`.

### 1.2 Validación de inputs
- Muchos controllers usan `[ApiController]`, lo que activa validación automática de modelo cuando los DTOs tienen `DataAnnotations`.
- Varios endpoints realizan validación manual útil, por ejemplo en `CatalogController` y `AuthController.RecoverWithCode`.
- Sin embargo, hay endpoints que no verifican `ModelState.IsValid` ni validan payloads con rigurosidad en el controller.
- Existen rutas que aceptan datos de escritura sin validación explícita de campos críticos, especialmente en `ProductosV1Controller`, `SucursalesV1Controller`, `TiendasController` y `CarritoV1Controller`.
- El uso de excepciones genéricas (`ArgumentException`, `InvalidOperationException`) para controlar la lógica de validación es consistente, pero hace menos clara la separación entre errores de negocio y errores de entrada.

### 1.3 TenantResolverMiddleware
- Resuelve tenant en este orden:
  1. `X-Tenant-ID`
  2. `X-Tenant-Slug`
  3. subdominio (`*.localhost`, `*.lvh.me`, o `*.{MAIN_DOMAIN}`)
- No valida que `X-Tenant-ID` exista en la base de datos; lo almacena directamente en `HttpContext.Items["ResolvedTenantId"]`.
- Esto permite que un cliente envíe un `X-Tenant-ID` inválido o arbitrario y cambie el tenant activo del request.
- También existe riesgo de host spoofing en subdominios si no se controlan correctamente los dominios permitidos.
- Recomendación: validar la existencia del tenant antes de exponerlo como tenant resuelto.

### 1.4 BetterAuthSessionMiddleware
- Acepta sesiones por `Authorization: Bearer` o cookies `better-auth.session_token` / `__secure-better-auth.session_token`.
- Consulta `Sessions.IgnoreQueryFilters().Include(s => s.User)` y valida expiración con `ExpiresAt > DateTime.UtcNow`.
- Si el usuario pertenece a otro tenant:
  - `cliente`: la petición continúa como anónima.
  - `staff`: busca el mismo email en el tenant resuelto y reasigna la identidad a ese usuario.
  - `superadmin`: permite acceso a cualquier tenant.
- Vulnerabilidad de aislamiento: la comparación de tenant cruzado depende solo de `X-Tenant-ID`, no del tenant resuelto por subdominio/slug.
- Un staff autenticado en tenant A podría acceder a tenant B mediante subdominio/slug si comparte email con un usuario en tenant B.
- El token de sesión se almacena como texto plano y no hay firma/hashing adicional sobre el valor del token.
- Recomendación: comparar siempre el tenant resuelto por `TenantResolverMiddleware` con el tenant de la sesión y restringir reasignación de usuario.

### 1.5 Control de acceso y roles
- El backend utiliza dos modelos de autorización:
  - `[Authorize]` en controllers específicos.
  - verificaciones manuales con `EsStaff()`, `EsAdministrador()` y `GetUserId()` en controllers de `V1ControllerBase`.
- Algunos controllers sensibles no tienen `[Authorize]`:
  - `ProductosV1Controller`
  - `ReportesV1Controller`
  - `InventariosController`
  - `MediaController`
  - `SucursalesV1Controller`
  - `TiendasController`
  - `UsuariosV1Controller`
  - `ReservacionesV1Controller`
  - `CarritoV1Controller`
- Esto hace que la protección dependa del desarrollador, en lugar de definirse declarativamente.
- En varios casos, la autenticación es implícita en `GetUserId()` o `GetTenantId()`, lo que puede ocultar rutas no protegidas en el futuro.
- Recomendación: usar `[Authorize]` para endpoints que requieran usuario autenticado y mantener las comprobaciones de rol para autorización granular.

### 1.6 Endpoints de riesgo
- `ProductosV1Controller`: CRUD de productos sin `[Authorize]`. Si está destinado a uso administrativo, es un riesgo alto.
- `ReportesV1Controller.EjecutarRaw`: ejecuta SQL raw con validación basada en regex. Aunque hay restricciones, los queries dinámicos son siempre un riesgo.
- `PagosController.WebhookStripe`: público por diseño, pero depende totalmente de `Stripe-Signature` y `STRIPE_WEBHOOK_SECRET`.
- `AuthController.ForgotPassword`: requiere `X-Tenant-ID` y usa credenciales SMTP de tienda. No hay rate limiting ni anti-abuso evidente.
- `TenantResolverMiddleware` y `BetterAuthSessionMiddleware`: fallos de tenant/usuario cruzado son los hallazgos más críticos.

## 2. Mapeo de endpoints y roles de backend

| Ruta | Auth | Rol esperado | Tenant requerido | Riesgo / comentario |
|---|---|---|---|---|
| `POST /api/v1/auth/register` | No | Público / admin interno | No | Registro de cliente y admin. |
| `POST /api/v1/auth/login` | No | Público | No | Login general. |
| `POST /api/v1/auth/logout` | Sí | Autenticado | No | Revoca JWT. |
| `POST /api/v1/auth/forgot-password` | No | Público | Sí | Requiere `X-Tenant-ID`. |
| `POST /api/v1/auth/recover-with-code` | No | Público | No | Recovery con código. |
| `GET /api/admin/usuarios` | Sí | `administrador` / `superadmin` | Implícito | Protegido con `[Authorize]`. |
| `POST /api/admin/usuarios` | Sí | `administrador` / `superadmin` | Implícito | Protegido. |
| `POST /api/admin/usuarios/{id}/reset-password` | Sí | `administrador` / `superadmin` | Implícito | Protegido. |
| `GET /api/admin/reportes/*` | Sí | `administrador` / panel admin | Implícito | Protegido con `[Authorize]`. |
| `POST /api/pagos/webhook` | No | Stripe | No | Público con firma Stripe. |
| `POST /api/pagos/create-intent` | Sí | `cliente` | Implícito | Requiere JWT cliente. |
| `GET /api/pagos/{id}/status` | Sí | `cliente` o `administrador` | Implícito | Auth y rol. |
| `GET /api/productos/{id}` | No | Público | No | Consulta pública. |
| `GET /api/productos` | No | Público | No | Consulta pública. |
| `POST /api/productos` | Sí | `administrador` | Implícito | Protegido. |
| `PUT /api/productos/{id}` | Sí | `administrador` | Implícito | Protegido. |
| `DELETE /api/productos/{id}` | Sí | `administrador` | Implícito | Protegido. |
| `GET /api/v1/productos` | No | No explícito | Sí | CRUD sin `[Authorize]` si necesita auth. |
| `POST /api/v1/productos` | No | No explícito | Sí | Riesgo si debe ser admin-only. |
| `PUT /api/v1/productos/{id}` | No | No explícito | Sí | Riesgo. |
| `DELETE /api/v1/productos/{id}` | No | No explícito | Sí | Riesgo. |
| `GET /api/v1/inventarios/*` | No | No explícito | Sí | Tenant requerido, no auth. |
| `PUT /api/v1/inventarios/ajuste` | No | No explícito | Sí | Tenant requerido, no auth. |
| `GET /api/v1/media/*` | No | No explícito | Sí | Tenant requerido, no auth. |
| `DELETE /api/v1/media` | No | No explícito | Sí | Tenant requerido, no auth. |
| `GET /api/v1/reportes/ejecutar-raw` | No | `staff` | Sí | Auth implícito con `EsStaff()`. |
| `POST /api/v1/reportes/guardar` | No | `staff` | Sí | Auth implícito. |
| `GET /api/v1/reportes` | No | `staff` | Sí | Auth implícito. |
| `GET /api/v1/reportes/{id}/correr` | No | `staff` | Sí | Auth implícito. |
| `GET /api/v1/reservaciones/mis-compras` | No | Autenticado | Sí | Auth implícito por `GetUserId()`. |
| `GET /api/v1/reservaciones/control-staff` | No | `staff` | Sí | Auth implícito. |
| `PATCH /api/v1/reservaciones/{id}/estado` | No | `staff` | Sí | Auth implícito. |
| `GET /api/v1/sucursales` | No | Público/tenant | Sí | Lectura pública con tenant. |
| `POST /api/v1/sucursales` | No | `administrador` | Sí | Requiere `EsAdministrador()`. |
| `PUT /api/v1/sucursales/{id}` | No | `administrador` | Sí | Requiere `EsAdministrador()`. |
| `DELETE /api/v1/sucursales/{id}` | No | `administrador` | Sí | Requiere `EsAdministrador()`. |
| `GET /api/v1/tiendas` | No | Público | No | Listado público. |
| `POST /api/v1/tiendas` | No | Público | No | Crea tienda. |
| `PUT /api/v1/tiendas/actualizar` | No | No explícito | Sí | Tenant requerido, no auth. |
| `PUT /api/v1/tiendas/configuracion-visual` | No | No explícito | Sí | Tenant requerido, no auth. |
| `POST /api/v1/tiendas/integraciones` | No | No explícito | Sí | Tenant requerido, no auth. |
| `GET /api/v1/tiendas/integraciones` | No | No explícito | Sí | Tenant requerido, no auth. |
| `GET /api/v1/usuarios` | No | `administrador` | Sí | Auth implícito. |
| `GET /api/v1/usuarios/{id}` | No | `administrador` | Sí | Auth implícito. |
| `POST /api/v1/usuarios/invitar` | No | `administrador` | Sí | Auth implícito. |
| `PUT /api/v1/usuarios/{id}/rol` | No | `administrador` | Sí | Auth implícito. |
| `PUT /api/v1/usuarios/{id}/estado` | No | `administrador` | Sí | Auth implícito. |
| `DELETE /api/v1/usuarios/{id}` | No | `administrador` | Sí | Auth implícito. |
| `GET /api/v1/carrito` | No | Autenticado | Sí | Auth implícito. |
| `POST /api/v1/carrito/articulos` | No | Autenticado | Sí | Auth implícito. |
| `PUT /api/v1/carrito/articulos/{id}` | No | Autenticado | Sí | Auth implícito. |
| `DELETE /api/v1/carrito/articulos/{id}` | No | Autenticado | Sí | Auth implícito. |

## 3. Riesgos destacados
- `X-Tenant-ID` se confía sin validar su existencia.
- `BetterAuthSessionMiddleware` no compara siempre el tenant resuelto contra el tenant del usuario.
- Controllers sin `[Authorize]` dependen de verificaciones manuales y pueden dejar rutas poco seguras.
- `ProductosV1Controller` expone operaciones de escritura sin autorización explícita.
- `ReportesV1Controller.EjecutarRaw` ejecuta SQL dinámico mediante validación regex.
- Secretos sensibles aparecen en `appsettings.*.json`.
- `AuthController.ForgotPassword` carece de protección anti-abuso evidente.

## 4. Recomendaciones inmediatas
1. Eliminar secretos de `appsettings.json` y `appsettings.Development.json` del repositorio.
2. Requerir `[Authorize]` en todos los controllers/endpoints que manipulan datos de usuario, pagos, productos, inventarios, reservas o reportes.
3. Validar `X-Tenant-ID` en `TenantResolverMiddleware` contra la base de datos.
4. Corregir `BetterAuthSessionMiddleware` para validar el tenant resuelto por subdominio/slug y evitar reasignación de usuario sin la tenant match.
5. Cifrar o proteger claves Stripe/SMTP en la base de datos.
6. Añadir validación de modelo consistente (`ModelState.IsValid`) o DTOs más estrictos en endpoints de escritura.
7. Agregar rate limiting y protección contra abuse en recuperación de contraseña y registro.
8. Revisar `PagosController.WebhookStripe` y asegurar que `STRIPE_WEBHOOK_SECRET` está protegido.

## 5. Conclusión
Este documento cubre únicamente el backend de la solución. La arquitectura muestra esfuerzos de aislamiento multi-tenant y roles, pero presenta riesgos significativos en:
- aislamiento de tenant/session
- autorización declarativa
- validación de inputs
- manejo de secretos

Corregir los puntos listados mejora la seguridad backend de forma inmediata.
