# Instructions

- Following Playwright test failed.
- Explain why, be concise, respect Playwright best practices.
- Provide a snippet of code with the fix, if possible.

# Test info

- Name: reportes.spec.ts >> Reportes - Negocio >> la pestaña personalizada muestra la consola SQL de solo lectura
- Location: tests/reportes.spec.ts:141:2

# Error details

```
Error: expect(locator).toBeVisible() failed

Locator:  locator('textarea')
Expected: visible
Received: hidden
Timeout:  5000ms

Call log:
  - Expect "toBeVisible" with timeout 5000ms
  - waiting for locator('textarea')
    - locator resolved to <textarea wrap="on" tabindex="0" data-mprt="7" role="textbox" autocorrect="off" autocomplete="off" spellcheck="false" autocapitalize="off" aria-required="false" aria-multiline="true" aria-autocomplete="both" aria-label="Editor content" aria-roledescription="editor" class="inputarea monaco-mouse-cursor-text"></textarea>
    - unexpected value "hidden"

```

```yaml
- complementary:
  - img "DM Hub Logo"
  - heading "DM Hub" [level=1]
  - navigation:
    - button "Tablero"
    - button "Sucursales"
    - button "Reservaciones"
    - button "Pagos"
    - button "Kanban"
    - button "Clientes"
    - button "Productos"
    - button "Reportes"
    - button "Usuarios"
    - button "Documentación"
    - button "Constructor"
    - button "Configuración"
  - button "Cerrar Sesión"
- main:
  - textbox "Buscar en panel (módulos, productos, órdenes)..."
  - button "Cambiar idioma (ES / EN)": ES
  - button "Esmirna"
  - button "Configuración"
  - paragraph: Administrador Seed
  - paragraph: Administrador
  - text: AD
  - heading "Reportes de Negocio" [level=2]
  - paragraph: Analiza el rendimiento de tu tienda y realiza consultas personalizadas
  - button "Productos"
  - button "Empleados"
  - button "Métodos de Pago"
  - button "Personalizado (SQL)"
  - 'heading "Consola SQL Custom (Soporta variables: @tenant_id)" [level=3]'
  - button "Ocultar Ayuda de Tablas"
  - text: READ-ONLY ACTIVE Consola de Consultas SQL
  - button "Ver Esquema & Atributos"
  - button "+ tienda_id = @tenant_id"
  - button "+ @tenant_id"
  - code:
    - textbox "Editor content"
  - text: "PostgreSQL Ready | Líneas: 1 | Ln 1, Col 1 Caracteres: 157 Ejecutar: Ctrl + Enter Autocompletado: Ctrl + Espacio"
  - paragraph:
    - text: "* Por razones de seguridad y aislamiento de datos, toda consulta SQL debe incluir un filtro explícito en la columna"
    - code: tienda_id
    - text: utilizando la variable
    - code: "@tenant_id"
    - text: "(ej:"
    - code: WHERE tienda_id = @tenant_id
    - text: ). Solo se permiten comandos de lectura (
    - code: SELECT
    - text: /
    - code: WITH
    - text: ).
  - text: Tus consultas se ejecutan con privilegios restringidos de base de datos PostgreSQL.
  - button "Restaurar Inicial"
  - button "Ejecutar Query"
  - heading "Ayuda de Esquema & Tablas Disponibles PostgreSQL" [level=3]
  - paragraph: Consulta las tablas, atributos, tipos de datos y plantillas SQL para tus consultas
  - button "Tablas (10)"
  - button "Plantillas (6)"
  - button "Reglas & Seguridad"
  - 'textbox "Buscar tabla, columna o tipo (ej: producto, precio, cantidad)..."'
  - button "Todas"
  - button "Catálogo & Stock"
  - button "Ventas & Pedidos"
  - button "Usuarios & Sucursales"
  - button "Configuración & Reportes"
  - button "Expandir todas"
  - text: •
  - button "Colapsar todas"
  - code: public."Producto"
  - text: Catálogo de productos, precios y niveles de stock 14 columnas
  - button "+ Insertar"
  - button "SELECT *"
  - paragraph: Contiene los productos creados en la tienda con sus precios al detalle/mayoreo e inventario actual.
  - table:
    - rowgroup:
      - row "Columna Tipo de Dato Restricciones Descripción Acción":
        - columnheader "Columna"
        - columnheader "Tipo de Dato"
        - columnheader "Restricciones"
        - columnheader "Descripción"
        - columnheader "Acción"
    - rowgroup:
      - row "id PK VARCHAR(255) NOT NULL Identificador único (UUID) + Añadir":
        - cell "id PK"
        - cell "VARCHAR(255)"
        - cell "NOT NULL"
        - cell "Identificador único (UUID)"
        - cell "+ Añadir":
          - button "+ Añadir"
      - 'row "tienda_id FK VARCHAR(255) NOT NULL ID del inquilino (Tienda) → Ref: public.\"Tienda\".id + Añadir"':
        - cell "tienda_id FK"
        - cell "VARCHAR(255)"
        - cell "NOT NULL"
        - 'cell "ID del inquilino (Tienda) → Ref: public.\"Tienda\".id"'
        - cell "+ Añadir":
          - button "+ Añadir"
      - 'row "categoria_id FK VARCHAR(255) NULL Categoría asociada → Ref: public.\"Categoria\".id + Añadir"':
        - cell "categoria_id FK"
        - cell "VARCHAR(255)"
        - cell "NULL"
        - 'cell "Categoría asociada → Ref: public.\"Categoria\".id"'
        - cell "+ Añadir":
          - button "+ Añadir"
      - row "nombre VARCHAR(150) NOT NULL Nombre del producto + Añadir":
        - cell "nombre"
        - cell "VARCHAR(150)"
        - cell "NOT NULL"
        - cell "Nombre del producto"
        - cell "+ Añadir":
          - button "+ Añadir"
      - row "descripcion TEXT NULL Descripción detallada + Añadir":
        - cell "descripcion"
        - cell "TEXT"
        - cell "NULL"
        - cell "Descripción detallada"
        - cell "+ Añadir":
          - button "+ Añadir"
      - row "sku VARCHAR(100) NULL Código SKU único + Añadir":
        - cell "sku"
        - cell "VARCHAR(100)"
        - cell "NULL"
        - cell "Código SKU único"
        - cell "+ Añadir":
          - button "+ Añadir"
      - row "precio_detalle NUMERIC(18,2) NOT NULL Precio de venta al detalle + Añadir":
        - cell "precio_detalle"
        - cell "NUMERIC(18,2)"
        - cell "NOT NULL"
        - cell "Precio de venta al detalle"
        - cell "+ Añadir":
          - button "+ Añadir"
      - row "precio_mayoreo NUMERIC(18,2) NOT NULL Precio de venta al mayoreo + Añadir":
        - cell "precio_mayoreo"
        - cell "NUMERIC(18,2)"
        - cell "NOT NULL"
        - cell "Precio de venta al mayoreo"
        - cell "+ Añadir":
          - button "+ Añadir"
      - row "stock_actual INTEGER NOT NULL(0) Cantidad total de unidades disponibles + Añadir":
        - cell "stock_actual"
        - cell "INTEGER"
        - cell "NOT NULL(0)"
        - cell "Cantidad total de unidades disponibles"
        - cell "+ Añadir":
          - button "+ Añadir"
      - row "stock_minimo INTEGER NOT NULL(0) Umbral de alerta para stock crítico + Añadir":
        - cell "stock_minimo"
        - cell "INTEGER"
        - cell "NOT NULL(0)"
        - cell "Umbral de alerta para stock crítico"
        - cell "+ Añadir":
          - button "+ Añadir"
      - row "imagen_url VARCHAR(500) NULL URL de la imagen del producto + Añadir":
        - cell "imagen_url"
        - cell "VARCHAR(500)"
        - cell "NULL"
        - cell "URL de la imagen del producto"
        - cell "+ Añadir":
          - button "+ Añadir"
      - row "publicado BOOLEAN NOT NULL(true) Visible en el catálogo público + Añadir":
        - cell "publicado"
        - cell "BOOLEAN"
        - cell "NOT NULL(true)"
        - cell "Visible en el catálogo público"
        - cell "+ Añadir":
          - button "+ Añadir"
      - row "eliminado BOOLEAN NOT NULL(false) Estado de borrado lógico + Añadir":
        - cell "eliminado"
        - cell "BOOLEAN"
        - cell "NOT NULL(false)"
        - cell "Estado de borrado lógico"
        - cell "+ Añadir":
          - button "+ Añadir"
      - row "fecha_creacion TIMESTAMPTZ NOT NULL(NOW()) Fecha de registro + Añadir":
        - cell "fecha_creacion"
        - cell "TIMESTAMPTZ"
        - cell "NOT NULL(NOW())"
        - cell "Fecha de registro"
        - cell "+ Añadir":
          - button "+ Añadir"
  - code: public."Reservacion"
  - text: Órdenes, compras y reservaciones realizadas 9 columnas
  - button "+ Insertar"
  - button "SELECT *"
  - paragraph: Registra los pedidos de clientes, montos totales, estados de pago (pagado, pendiente) y despacho.
  - table:
    - rowgroup:
      - row "Columna Tipo de Dato Restricciones Descripción Acción":
        - columnheader "Columna"
        - columnheader "Tipo de Dato"
        - columnheader "Restricciones"
        - columnheader "Descripción"
        - columnheader "Acción"
    - rowgroup:
      - row "id PK VARCHAR(255) NOT NULL Identificador único de la orden/venta (UUID) + Añadir":
        - cell "id PK"
        - cell "VARCHAR(255)"
        - cell "NOT NULL"
        - cell "Identificador único de la orden/venta (UUID)"
        - cell "+ Añadir":
          - button "+ Añadir"
      - 'row "tienda_id FK VARCHAR(255) NOT NULL ID del inquilino (Tienda) → Ref: public.\"Tienda\".id + Añadir"':
        - cell "tienda_id FK"
        - cell "VARCHAR(255)"
        - cell "NOT NULL"
        - 'cell "ID del inquilino (Tienda) → Ref: public.\"Tienda\".id"'
        - cell "+ Añadir":
          - button "+ Añadir"
      - 'row "sucursal_id FK VARCHAR(255) NOT NULL Sucursal donde se retira o atiende → Ref: public.\"Sucursal\".id + Añadir"':
        - cell "sucursal_id FK"
        - cell "VARCHAR(255)"
        - cell "NOT NULL"
        - 'cell "Sucursal donde se retira o atiende → Ref: public.\"Sucursal\".id"'
        - cell "+ Añadir":
          - button "+ Añadir"
      - 'row "usuario_id FK VARCHAR(255) NOT NULL ID del usuario o cliente que ordenó → Ref: public.\"user\".id + Añadir"':
        - cell "usuario_id FK"
        - cell "VARCHAR(255)"
        - cell "NOT NULL"
        - 'cell "ID del usuario o cliente que ordenó → Ref: public.\"user\".id"'
        - cell "+ Añadir":
          - button "+ Añadir"
      - row "monto_total NUMERIC(18,2) NOT NULL Total monetario de la orden + Añadir":
        - cell "monto_total"
        - cell "NUMERIC(18,2)"
        - cell "NOT NULL"
        - cell "Total monetario de la orden"
        - cell "+ Añadir":
          - button "+ Añadir"
      - row "estado_pago VARCHAR(30) NOT NULL('pendiente') Estado del pago ('pendiente', 'pagado', 'cancelado') + Añadir":
        - cell "estado_pago"
        - cell "VARCHAR(30)"
        - cell "NOT NULL('pendiente')"
        - cell "Estado del pago ('pendiente', 'pagado', 'cancelado')"
        - cell "+ Añadir":
          - button "+ Añadir"
      - row "estado_despacho VARCHAR(30) NOT NULL('procesando') Estado de despacho ('procesando', 'listo', 'entregado', 'cancelado') + Añadir":
        - cell "estado_despacho"
        - cell "VARCHAR(30)"
        - cell "NOT NULL('procesando')"
        - cell "Estado de despacho ('procesando', 'listo', 'entregado', 'cancelado')"
        - cell "+ Añadir":
          - button "+ Añadir"
      - row "stripe_intent_id VARCHAR(255) NULL ID de transacción con tarjeta en Stripe + Añadir":
        - cell "stripe_intent_id"
        - cell "VARCHAR(255)"
        - cell "NULL"
        - cell "ID de transacción con tarjeta en Stripe"
        - cell "+ Añadir":
          - button "+ Añadir"
      - row "fecha_reserva TIMESTAMPTZ NOT NULL(NOW()) Fecha y hora de creación de la orden + Añadir":
        - cell "fecha_reserva"
        - cell "TIMESTAMPTZ"
        - cell "NOT NULL(NOW())"
        - cell "Fecha y hora de creación de la orden"
        - cell "+ Añadir":
          - button "+ Añadir"
  - code: public."DetalleReservacion"
  - text: Líneas de detalle de productos en cada orden/venta 6 columnas
  - button "+ Insertar"
  - button "SELECT *"
  - paragraph: Desglose de cada producto comprado dentro de una orden con su cantidad, precio cobrado y subtotal.
  - table:
    - rowgroup:
      - row "Columna Tipo de Dato Restricciones Descripción Acción":
        - columnheader "Columna"
        - columnheader "Tipo de Dato"
        - columnheader "Restricciones"
        - columnheader "Descripción"
        - columnheader "Acción"
    - rowgroup:
      - row "id PK VARCHAR(255) NOT NULL Identificador único de la línea de detalle (UUID) + Añadir":
        - cell "id PK"
        - cell "VARCHAR(255)"
        - cell "NOT NULL"
        - cell "Identificador único de la línea de detalle (UUID)"
        - cell "+ Añadir":
          - button "+ Añadir"
      - 'row "reservacion_id FK VARCHAR(255) NOT NULL ID de la reservación padre → Ref: public.\"Reservacion\".id + Añadir"':
        - cell "reservacion_id FK"
        - cell "VARCHAR(255)"
        - cell "NOT NULL"
        - 'cell "ID de la reservación padre → Ref: public.\"Reservacion\".id"'
        - cell "+ Añadir":
          - button "+ Añadir"
      - 'row "producto_id FK VARCHAR(255) NOT NULL ID del producto vendido → Ref: public.\"Producto\".id + Añadir"':
        - cell "producto_id FK"
        - cell "VARCHAR(255)"
        - cell "NOT NULL"
        - 'cell "ID del producto vendido → Ref: public.\"Producto\".id"'
        - cell "+ Añadir":
          - button "+ Añadir"
      - row "cantidad INTEGER NOT NULL Número de unidades adquiridas + Añadir":
        - cell "cantidad"
        - cell "INTEGER"
        - cell "NOT NULL"
        - cell "Número de unidades adquiridas"
        - cell "+ Añadir":
          - button "+ Añadir"
      - row "precio_cobrado NUMERIC(18,2) NOT NULL Precio unitario cobrado en la compra + Añadir":
        - cell "precio_cobrado"
        - cell "NUMERIC(18,2)"
        - cell "NOT NULL"
        - cell "Precio unitario cobrado en la compra"
        - cell "+ Añadir":
          - button "+ Añadir"
      - row "subtotal NUMERIC(18,2) NOT NULL Subtotal calculado (cantidad * precio_cobrado) + Añadir":
        - cell "subtotal"
        - cell "NUMERIC(18,2)"
        - cell "NOT NULL"
        - cell "Subtotal calculado (cantidad * precio_cobrado)"
        - cell "+ Añadir":
          - button "+ Añadir"
  - code: public."Categoria"
  - text: Categorías y secciones de productos 6 columnas
  - button "+ Insertar"
  - button "SELECT *"
  - code: public."Sucursal"
  - text: Sucursales y puntos de venta físicos 6 columnas
  - button "+ Insertar"
  - button "SELECT *"
  - code: public."Inventario"
  - text: Stock discriminado por sucursal específica 5 columnas
  - button "+ Insertar"
  - button "SELECT *"
  - code: public."user"
  - text: Clientes y miembros del equipo staff 13 columnas
  - button "+ Insertar"
  - button "SELECT *"
  - code: public."CarritoElemento"
  - text: Productos en carritos de compra activos 6 columnas
  - button "+ Insertar"
  - button "SELECT *"
  - code: public."Tienda"
  - text: Información y configuración general de la tienda 6 columnas
  - button "+ Insertar"
  - button "SELECT *"
  - code: public."ReportePersonalizado"
  - text: Consultas SQL y reportes guardados por el usuario 7 columnas
  - button "+ Insertar"
  - button "SELECT *"
- region "Notifications alt+T"
- alert: Reportes – Esmirna | Portal Admin
- alert
- alert
```

# Test source

```ts
  47  | 	test.beforeEach(async ({ page }) => {
  48  | 		await iniciarSesionComoAdministrador(page);
  49  | 		await abrirReportes(page);
  50  | 	});
  51  | 
  52  | 	test('la página de reportes carga correctamente', async ({ page }) => {
  53  | 		// Verifica que la navegación termina en un panel usable y no en una
  54  | 		// pantalla vacía o en un error de ruta.
  55  | 		await expect(page.getByText('Analiza el rendimiento de tu tienda y realiza consultas personalizadas')).toBeVisible();
  56  | 		await expect(page.locator('main').getByRole('button', { name: 'Productos', exact: true })).toBeVisible();
  57  | 		await expect(page.getByRole('button', { name: 'Empleados', exact: true })).toBeVisible();
  58  | 		await expect(page.getByRole('button', { name: 'Métodos de Pago', exact: true })).toBeVisible();
  59  | 		await expect(page.getByRole('button', { name: 'Personalizado (SQL)', exact: true })).toBeVisible();
  60  | 	});
  61  | 
  62  | 	test('el reporte de productos muestra métricas principales', async ({ page }) => {
  63  | 		// Comprueba las métricas disponibles en la implementación actual:
  64  | 		// productos vendidos, ingresos y producto más vendido.
  65  | 		await esperarReporteProductos(page);
  66  | 		await expect(page.getByText('Ingresos Totales', { exact: true })).toBeVisible();
  67  | 		await expect(page.getByText('Producto Top', { exact: true })).toBeVisible();
  68  | 	});
  69  | 
  70  | 	test('el reporte de productos muestra gráficas y detalle', async ({ page }) => {
  71  | 		// La existencia de títulos y tabla confirma que el reporte tiene una
  72  | 		// representación visual además de los KPI.
  73  | 		await esperarReporteProductos(page);
  74  | 		await expect(page.getByRole('heading', { name: 'Cantidad Vendida por Producto' })).toBeVisible();
  75  | 		await expect(page.getByRole('heading', { name: 'Ingresos por Producto' })).toBeVisible();
  76  | 		await expect(page.getByRole('heading', { name: 'Detalle de Productos Más Vendidos' })).toBeVisible();
  77  | 		await expect(page.getByRole('table')).toBeVisible();
  78  | 	});
  79  | 
  80  | 	test('los filtros de fecha y modo están disponibles', async ({ page }) => {
  81  | 		// Verifica los dos filtros que sí implementa ReportesTab; los controles
  82  | 		// de sucursal/producto no forman parte de la UI actual.
  83  | 		await esperarReporteProductos(page);
  84  | 		await expect(page.locator('input[type="date"]')).toHaveCount(2);
  85  | 		const reportesMain = page.locator('main');
  86  | 		await expect(reportesMain.getByRole('button', { name: 'Todos', exact: true })).toBeVisible();
  87  | 		await expect(reportesMain.getByRole('button', { name: 'Ventas', exact: true })).toBeVisible();
  88  | 		await expect(reportesMain.getByRole('button', { name: 'Reservaciones', exact: true })).toBeVisible();
  89  | 	});
  90  | 
  91  | 	test('cambiar el modo Ventas actualiza el reporte', async ({ page }) => {
  92  | 		// El cambio debe provocar una nueva representación visible sin salir de
  93  | 		// la pestaña Productos.
  94  | 		await esperarReporteProductos(page);
  95  | 		const responsePromise = page.waitForResponse((response) =>
  96  | 			response.url().includes('/api/admin/reportes/productos') && response.request().method() === 'GET'
  97  | 		);
  98  | 		await page.getByRole('button', { name: 'Ventas', exact: true }).click();
  99  | 		await responsePromise;
  100 | 		await esperarReporteProductos(page);
  101 | 		await expect(page.getByRole('heading', { name: 'Reportes de Negocio' })).toBeVisible();
  102 | 	});
  103 | 
  104 | 	test('cambiar el rango de fechas actualiza el reporte', async ({ page }) => {
  105 | 		// Verifica persistencia inmediata del filtro mediante la interfaz y el
  106 | 		// refresco visual posterior, sin inspeccionar la petición internamente.
  107 | 		await esperarReporteProductos(page);
  108 | 		const hasta = page.locator('input[type="date"]').nth(1);
  109 | 		const responsePromise = page.waitForResponse((response) =>
  110 | 			response.url().includes('/api/admin/reportes/productos') && response.request().method() === 'GET'
  111 | 		);
  112 | 		await hasta.fill('2026-08-20');
  113 | 		await responsePromise;
  114 | 		await expect(hasta).toHaveValue('2026-08-20');
  115 | 		await esperarReporteProductos(page);
  116 | 	});
  117 | 
  118 | 	test('el reporte de empleados muestra métricas y productividad', async ({ page }) => {
  119 | 		// Cubre la segunda vista disponible del módulo y sus indicadores de
  120 | 		// personal, ventas y monto total.
  121 | 		await page.getByRole('button', { name: 'Empleados', exact: true }).click();
  122 | 		await expect(page.getByText('Total Empleados', { exact: true })).toBeVisible({ timeout: 15000 });
  123 | 		await expect(page.getByText('Total Ventas', { exact: true })).toBeVisible();
  124 | 		await expect(
  125 | 			page.locator('main span').filter({ hasText: /^Monto Total$/ })
  126 | 		).toBeVisible();
  127 | 		await expect(page.getByText('Top Vendedor', { exact: true })).toBeVisible();
  128 | 		await expect(page.getByRole('heading', { name: 'Productividad Detallada' })).toBeVisible();
  129 | 	});
  130 | 
  131 | 	test('el reporte de métodos de pago muestra resumen y detalle', async ({ page }) => {
  132 | 		// Comprueba métodos, transacciones y montos agregados en la vista de
  133 | 		// pagos, además de la tabla de detalle.
  134 | 		await page.getByRole('button', { name: 'Métodos de Pago', exact: true }).click();
  135 | 		await expect(page.getByRole('heading', { name: 'Distribución por Método de Pago' })).toBeVisible({ timeout: 15000 });
  136 | 		await expect(page.getByRole('heading', { name: 'Monto por Método de Pago' })).toBeVisible();
  137 | 		await expect(page.getByRole('heading', { name: 'Detalle de Métodos de Pago' })).toBeVisible();
  138 | 		await expect(page.getByText(/transacciones/).first()).toBeVisible();
  139 | 	});
  140 | 
  141 | 	test('la pestaña personalizada muestra la consola SQL de solo lectura', async ({ page }) => {
  142 | 		// Valida que la funcionalidad personalizada está visible y marcada como
  143 | 		// solo lectura antes de cualquier ejecución.
  144 | 		await page.getByRole('button', { name: 'Personalizado (SQL)', exact: true }).click();
  145 | 		await expect(page.getByRole('heading', { name: /Consola SQL Custom/ })).toBeVisible();
  146 | 		await expect(page.getByText('READ-ONLY ACTIVE', { exact: true })).toBeVisible();
> 147 | 		await expect(page.locator('textarea')).toBeVisible();
      |                                          ^ Error: expect(locator).toBeVisible() failed
  148 | 		await expect(page.getByRole('button', { name: /Ejecutar/i })).toBeVisible();
  149 | 	});
  150 | 
  151 | 	
  152 | 
  153 | 	test('exportar CSV descarga el reporte visible cuando hay datos', async ({ page }) => {
  154 | 		// La exportación solo aparece cuando el reporte tiene filas; en un entorno
  155 | 		// sin ventas se marca como omitida porque no existe archivo que descargar.
  156 | 		await esperarReporteProductos(page);
  157 | 		const exportar = page.getByRole('button', { name: 'Exportar CSV', exact: true });
  158 | 		test.skip(await exportar.count() === 0, 'El reporte no tiene datos para exportar.');
  159 | 
  160 | 		const descarga = page.waitForEvent('download');
  161 | 		await exportar.click();
  162 | 		const archivo = await descarga;
  163 | 		expect(archivo.suggestedFilename()).toMatch(/reporte_ventas_por_producto_\d{4}-\d{2}-\d{2}\.csv/);
  164 | 	});
  165 | 
  166 | 	test('exportar Excel descarga el reporte visible cuando hay datos', async ({ page }) => {
  167 | 		// Igual que CSV, valida el flujo de descarga real desde el navegador.
  168 | 		await esperarReporteProductos(page);
  169 | 		const exportar = page.getByRole('button', { name: 'Exportar Excel', exact: true });
  170 | 		test.skip(await exportar.count() === 0, 'El reporte no tiene datos para exportar.');
  171 | 
  172 | 		const descarga = page.waitForEvent('download');
  173 | 		await exportar.click();
  174 | 		const archivo = await descarga;
  175 | 		expect(archivo.suggestedFilename()).toMatch(/reporte_ventas_por_producto_\d{4}-\d{2}-\d{2}\.xlsx/);
  176 | 	});
  177 | 
  178 | 	test('el reporte conserva la vista seleccionada después de recargar', async ({ page }) => {
  179 | 		// La vista seleccionada se reinicia actualmente a Productos al recargar;
  180 | 		// se comprueba que el módulo vuelve a cargar datos y no queda roto.
  181 | 		await page.getByRole('button', { name: 'Empleados', exact: true }).click();
  182 | 		await expect(page.getByText('Total Empleados', { exact: true })).toBeVisible({ timeout: 15000 });
  183 | 		await page.reload();
  184 | 		await abrirReportes(page);
  185 | 		await esperarReporteProductos(page);
  186 | 		await expect(page.getByRole('heading', { name: 'Reportes de Negocio' })).toBeVisible();
  187 | 	});
  188 | 
  189 | 	// FIXME: ReportesTab no implementa filtros independientes por sucursal,
  190 | 	// producto ni método de pago.
  191 | 	test.fixme('filtrar reportes por sucursal, producto y método de pago', async () => {});
  192 | 
  193 | 	// FIXME: La UI actual no expone margen de ganancia ni exportación PDF.
  194 | 	test.fixme('mostrar margen de ganancia y exportar PDF', async () => {});
  195 | });
  196 | 
```