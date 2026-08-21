# Instructions

- Following Playwright test failed.
- Explain why, be concise, respect Playwright best practices.
- Provide a snippet of code with the fix, if possible.

# Test info

- Name: clientes.spec.ts >> Clientes - CRM >> el modal de rol muestra opciones de staff al seleccionarlas
- Location: tests/clientes.spec.ts:133:2

# Error details

```
Error: expect(locator).toBeVisible() failed

Locator: getByRole('dialog', { name: 'Cambiar tipo y rol' }).getByText('Sucursal')
Expected: visible
Error: strict mode violation: getByRole('dialog', { name: 'Cambiar tipo y rol' }).getByText('Sucursal') resolved to 3 elements:
    1) <label class="text-[10px] font-bold text-slate-400 uppercase tracking-wider">Sucursal</label> aka getByText('Sucursal', { exact: true })
    2) <option value="">Ninguna sucursal (Sin asignar)</option> aka getByRole('combobox').nth(2)
    ...

Call log:
  - Expect "toBeVisible" with timeout 5000ms
  - waiting for getByRole('dialog', { name: 'Cambiar tipo y rol' }).getByText('Sucursal')

```

# Page snapshot

```yaml
- generic [ref=e1]:
  - generic [ref=e2]:
    - complementary [ref=e3]:
      - generic [ref=e4]:
        - generic [ref=e5]:
          - img "DM Hub Logo" [ref=e6]
          - heading "DM Hub" [level=1] [ref=e8]
        - navigation [ref=e9]:
          - button "Tablero" [ref=e10] [cursor=pointer]
          - button "Sucursales" [ref=e17] [cursor=pointer]
          - button "Reservaciones" [ref=e23] [cursor=pointer]
          - button "Pagos" [ref=e30] [cursor=pointer]
          - button "Kanban" [ref=e35] [cursor=pointer]
          - button "Clientes" [ref=e41] [cursor=pointer]
          - button "Productos" [ref=e48] [cursor=pointer]
          - button "Reportes" [ref=e55] [cursor=pointer]
          - button "Usuarios" [ref=e62] [cursor=pointer]
          - button "Documentación" [ref=e67] [cursor=pointer]
          - button "Constructor" [ref=e72] [cursor=pointer]
          - button "Configuración" [ref=e79] [cursor=pointer]
      - button "Cerrar Sesión" [ref=e85] [cursor=pointer]
    - main [ref=e91]:
      - generic [ref=e92]:
        - textbox "Buscar en panel (módulos, productos, órdenes)..." [ref=e98]
        - generic [ref=e99]:
          - button "Cambiar idioma (ES / EN)" [ref=e100] [cursor=pointer]:
            - generic [ref=e105]: ES
          - button "Esmirna" [ref=e107] [cursor=pointer]
          - button "Configuración" [ref=e115] [cursor=pointer]
          - generic [ref=e119]:
            - generic [ref=e120]:
              - paragraph [ref=e121]: Administrador Seed
              - paragraph [ref=e122]: Administrador
            - generic [ref=e123]: AD
      - generic [ref=e126]:
        - generic [ref=e127]:
          - heading "Clientes (CRM)" [level=2] [ref=e128]
          - paragraph [ref=e129]: Consulta la base de datos de clientes registrados en tu tienda
        - table [ref=e132]:
          - rowgroup [ref=e133]:
            - row [ref=e134]:
              - columnheader "Cliente" [ref=e135]
              - columnheader "Correo" [ref=e136]
              - columnheader "Estado" [ref=e137]
              - columnheader "Acciones" [ref=e138]
          - rowgroup [ref=e139]:
            - row [ref=e140]:
              - cell "CL Cliente Demo" [ref=e141]:
                - generic [ref=e142]: CL
                - generic [ref=e143]: Cliente Demo
              - cell "cliente.demo@dmhub.gt" [ref=e144]
              - cell "Suspendido" [ref=e145]
              - cell [ref=e146]:
                - button "Activar" [ref=e147] [cursor=pointer]
                - button "Cambiar Rol" [active] [ref=e148] [cursor=pointer]
  - region "Notifications alt+T"
  - alert [ref=e149]: Clientes – Esmirna | Portal Admin
  - generic [ref=e150]: "180"
  - dialog "Cambiar tipo y rol" [ref=e151]:
    - generic [ref=e152]:
      - button [ref=e153] [cursor=pointer]
      - generic [ref=e157]:
        - heading "Cambiar Tipo & Rol" [level=3] [ref=e158]
        - paragraph [ref=e159]: Modifica los permisos de Cliente Demo
      - generic [ref=e160]:
        - generic [ref=e161]:
          - generic [ref=e162]: Tipo de Usuario
          - combobox [ref=e163]:
            - option "Cliente"
            - option "Personal (Staff)" [selected]
        - generic [ref=e164]:
          - generic [ref=e165]: Rol de Staff
          - combobox [ref=e166]:
            - option "Cajero" [selected]
            - option "Administrador"
            - option "Super Administrador"
        - generic [ref=e167]:
          - generic [ref=e168]: Sucursal
          - combobox [ref=e169]:
            - option "Ninguna sucursal (Sin asignar)" [selected]
            - option "Sucursal Principal"
        - button "Guardar Permisos" [ref=e170] [cursor=pointer]
```

# Test source

```ts
  43  | 		await iniciarSesionComoAdministrador(page);
  44  | 		await abrirClientes(page);
  45  | 	});
  46  | 
  47  | 	test('la pestaña Clientes carga desde el portal', async ({ page }) => {
  48  | 		// Verifica navegación visible y evita considerar exitosa una pantalla vacía.
  49  | 		await expect(page.getByText('Consulta la base de datos de clientes registrados en tu tienda')).toBeVisible();
  50  | 		await expect(page.getByRole('heading', { name: 'Clientes (CRM)' })).toBeVisible();
  51  | 	});
  52  | 
  53  | 	test('muestra tabla de clientes o estado vacío', async ({ page }) => {
  54  | 		// La respuesta puede no tener clientes en un entorno recién instalado;
  55  | 		// ambos estados son válidos y deben renderizarse sin errores.
  56  | 		const tabla = page.getByRole('table');
  57  | 		const estadoVacio = page.getByText('No se encontraron clientes registrados.');
  58  | 
  59  | 		await expect(tabla.or(estadoVacio)).toBeVisible();
  60  | 	});
  61  | 
  62  | 	test('la tabla expone las columnas del cliente', async ({ page }) => {
  63  | 		// Verifica el contrato visual del listado: identidad, contacto y estado.
  64  | 		const tabla = page.getByRole('table');
  65  | 		test.skip(await tabla.count() === 0, 'El entorno no tiene clientes para renderizar la tabla.');
  66  | 
  67  | 		await expect(tabla.getByRole('columnheader', { name: 'Cliente' })).toBeVisible();
  68  | 		await expect(tabla.getByRole('columnheader', { name: 'Correo' })).toBeVisible();
  69  | 		await expect(tabla.getByRole('columnheader', { name: 'Estado' })).toBeVisible();
  70  | 		await expect(tabla.getByRole('columnheader', { name: 'Acciones' })).toBeVisible();
  71  | 	});
  72  | 
  73  | 	test('cada fila muestra nombre, correo y estado', async ({ page }) => {
  74  | 		// Comprueba que una fila no quede parcialmente renderizada o sin datos.
  75  | 		const filas = page.getByRole('table').getByRole('row').filter({ has: page.locator('td') });
  76  | 		test.skip(await filas.count() === 0, 'El entorno no tiene clientes registrados.');
  77  | 
  78  | 		for (const fila of await filas.all()) {
  79  | 			await expect(fila.locator('td').nth(0)).not.toBeEmpty();
  80  | 			await expect(fila.locator('td').nth(1)).toContainText('@');
  81  | 			await expect(fila.locator('td').nth(2)).toContainText(/Activo|Suspendido/);
  82  | 		}
  83  | 	});
  84  | 
  85  | 	test('el estado del cliente se presenta como Activo o Suspendido', async ({ page }) => {
  86  | 		// Evita estados ambiguos: son los únicos dos valores que ClientesTab expone.
  87  | 		const estados = page.getByRole('table').locator('tbody td').filter({ hasText: /Activo|Suspendido/ });
  88  | 		test.skip(await estados.count() === 0, 'El entorno no tiene clientes registrados.');
  89  | 
  90  | 		await expect(estados.first()).toContainText(/Activo|Suspendido/);
  91  | 	});
  92  | 
  93  | 	test('un administrador ve las acciones CRM por cliente', async ({ page }) => {
  94  | 		// Las acciones deben estar disponibles para el rol administrador.
  95  | 		const filas = page.getByRole('table').locator('tbody tr');
  96  | 		test.skip(await filas.count() === 0, 'El entorno no tiene clientes registrados.');
  97  | 
  98  | 		await expect(filas.first().getByRole('button', { name: /Suspender|Activar/ })).toBeVisible();
  99  | 		await expect(filas.first().getByRole('button', { name: 'Cambiar Rol' })).toBeVisible();
  100 | 	});
  101 | 
  102 | 	test('suspender y activar un cliente actualiza su estado', async ({ page }) => {
  103 | 		// Caso de mutación reversible: valida la acción y luego deja el entorno
  104 | 		// exactamente como estaba para no contaminar ejecuciones posteriores.
  105 | 		const filaActiva = page.getByRole('table').locator('tbody tr').filter({ hasText: 'Activo' }).first();
  106 | 		test.skip(await filaActiva.count() === 0, 'No hay un cliente activo para probar el ciclo de estado.');
  107 | 
  108 | 		await Promise.all([
  109 | 			page.waitForResponse((response) => response.url().includes('/api/v1/usuarios/') && response.request().method() === 'PUT'),
  110 | 			filaActiva.getByRole('button', { name: 'Suspender' }).click(),
  111 | 		]);
  112 | 		await expect(filaActiva).toContainText('Suspendido');
  113 | 
  114 | 		await Promise.all([
  115 | 			page.waitForResponse((response) => response.url().includes('/api/v1/usuarios/') && response.request().method() === 'PUT'),
  116 | 			filaActiva.getByRole('button', { name: 'Activar' }).click(),
  117 | 		]);
  118 | 		await expect(filaActiva).toContainText('Activo');
  119 | 	});
  120 | 
  121 | 	test('abrir Cambiar Rol muestra el modal de permisos', async ({ page }) => {
  122 | 		// El modal permite inspeccionar el tipo de usuario sin guardar cambios.
  123 | 		const filas = page.getByRole('table').locator('tbody tr');
  124 | 		test.skip(await filas.count() === 0, 'El entorno no tiene clientes registrados.');
  125 | 
  126 | 		await filas.first().getByRole('button', { name: 'Cambiar Rol' }).click();
  127 | 		const modal = page.getByRole('dialog', { name: 'Cambiar tipo y rol' });
  128 | 		await expect(modal).toBeVisible();
  129 | 		await expect(modal.getByText('Tipo de Usuario')).toBeVisible();
  130 | 		await expect(modal.getByRole('combobox')).toHaveValue('cliente');
  131 | 	});
  132 | 
  133 | 	test('el modal de rol muestra opciones de staff al seleccionarlas', async ({ page }) => {
  134 | 		// Comprueba que el formulario revela sus campos dependientes de forma
  135 | 		// visible, sin enviar una modificación destructiva.
  136 | 		const filas = page.getByRole('table').locator('tbody tr');
  137 | 		test.skip(await filas.count() === 0, 'El entorno no tiene clientes registrados.');
  138 | 
  139 | 		await filas.first().getByRole('button', { name: 'Cambiar Rol' }).click();
  140 | 		const modal = page.getByRole('dialog', { name: 'Cambiar tipo y rol' });
  141 | 		await modal.getByRole('combobox').selectOption('staff');
  142 | 		await expect(modal.getByText('Rol de Staff')).toBeVisible();
> 143 | 		await expect(modal.getByText('Sucursal')).toBeVisible();
      |                                             ^ Error: expect(locator).toBeVisible() failed
  144 | 		await page.keyboard.press('Escape');
  145 | 		await expect(modal).toBeHidden();
  146 | 	});
  147 | 
  148 | 	test('cerrar el modal de rol cancela la operación', async ({ page }) => {
  149 | 		// Escape debe cerrar el modal y no cambiar la vista del listado.
  150 | 		const filas = page.getByRole('table').locator('tbody tr');
  151 | 		test.skip(await filas.count() === 0, 'El entorno no tiene clientes registrados.');
  152 | 
  153 | 		await filas.first().getByRole('button', { name: 'Cambiar Rol' }).click();
  154 | 		const modal = page.getByRole('dialog', { name: 'Cambiar tipo y rol' });
  155 | 		await expect(modal).toBeVisible();
  156 | 		await page.keyboard.press('Escape');
  157 | 		await expect(modal).toBeHidden();
  158 | 		await expect(page.getByRole('heading', { name: 'Clientes (CRM)' })).toBeVisible();
  159 | 	});
  160 | 
  161 | 	test('el listado conserva sus datos después de recargar', async ({ page }) => {
  162 | 		// La persistencia se verifica únicamente desde navegador, como exige la
  163 | 		// regla de caja negra del plan.
  164 | 		const estadoInicial = await page.getByRole('table').or(page.getByText('No se encontraron clientes registrados.')).innerText();
  165 | 		await page.reload();
  166 | 		await abrirClientes(page);
  167 | 		await expect(page.getByRole('table').or(page.getByText('No se encontraron clientes registrados.'))).toBeVisible();
  168 | 		const estadoRecargado = await page.getByRole('table').or(page.getByText('No se encontraron clientes registrados.')).innerText();
  169 | 		expect(estadoRecargado).toBe(estadoInicial);
  170 | 	});
  171 | 
  172 | 	// FIXME: ClientesTab no implementa búsqueda por nombre/correo propia.
  173 | 	test.fixme('buscar cliente por nombre o correo', async () => {});
  174 | 
  175 | 	// FIXME: ClientesTab no implementa detalle, historial de compras ni reservaciones.
  176 | 	test.fixme('abrir detalle e historial de un cliente', async () => {});
  177 | 
  178 | 	// FIXME: falta un flujo UI para crear un cliente desde el portal administrativo.
  179 | 	test.fixme('crear cliente desde Clientes y verificar su aparición', async () => {});
  180 | });
  181 | 
```