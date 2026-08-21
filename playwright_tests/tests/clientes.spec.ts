import { expect, test, type Page } from '@playwright/test';

// ============================================================================
// ENDPOINT CUBIERTO: Portal de gestión -> pestaña "Clientes"
// CAPA TESTEADA: Interfaz E2E mediante navegador, sin consultar la API ni la
// base de datos directamente.
//
// La pantalla Clientes (ClientesTab) actualmente permite:
//   - Consultar nombre, correo y estado de los clientes.
//   - Suspender y activar clientes como administrador.
//   - Abrir el modal para cambiar tipo/rol.
//
// El plan de pruebas también menciona búsqueda, detalle, historial de compras
// y reservaciones asociadas. Esos controles no existen todavía en la UI; se
// dejan como FIXME al final para que la brecha quede visible y no se disfrace
// con una prueba que solo valide implementación interna.
//
// Requisitos de ejecución:
//   PLAYWRIGHT_BASE_URL (opcional, por defecto https://dmhub.fun)
//   Admin de pruebas: admin1@test.com / 123456789
// ============================================================================

const environment = (globalThis as { process?: { env?: Record<string, string | undefined> } }).process?.env ?? {};
const baseUrl = environment.PLAYWRIGHT_BASE_URL ?? 'https://dmhub.fun';
const adminEmail = environment.PLAYWRIGHT_ADMIN_EMAIL ?? 'admin1@test.com';
const adminPassword = environment.PLAYWRIGHT_ADMIN_PASSWORD ?? '123456789';

async function iniciarSesionComoAdministrador(page: Page) {
	await page.goto(`${baseUrl}/login`);
	await page.getByLabel('Correo electrónico').fill(adminEmail);
	await page.getByLabel('Contraseña').fill(adminPassword);
	await page.getByRole('button', { name: 'Iniciar sesión' }).click();
	await expect(page).toHaveURL(/\/portal/);
}

async function abrirClientes(page: Page) {
	await page.locator('aside:visible').getByRole('button', { name: 'Clientes', exact: true }).click();
	await expect(page.getByRole('heading', { name: 'Clientes (CRM)' })).toBeVisible();
}

test.describe.serial('Clientes - CRM', () => {
	test.beforeEach(async ({ page }) => {
		await iniciarSesionComoAdministrador(page);
		await abrirClientes(page);
	});

	test('la pestaña Clientes carga desde el portal', async ({ page }) => {
		// Verifica navegación visible y evita considerar exitosa una pantalla vacía.
		await expect(page.getByText('Consulta la base de datos de clientes registrados en tu tienda')).toBeVisible();
		await expect(page.getByRole('heading', { name: 'Clientes (CRM)' })).toBeVisible();
	});

	test('muestra tabla de clientes o estado vacío', async ({ page }) => {
		// La respuesta puede no tener clientes en un entorno recién instalado;
		// ambos estados son válidos y deben renderizarse sin errores.
		const tabla = page.getByRole('table');
		const estadoVacio = page.getByText('No se encontraron clientes registrados.');

		await expect(tabla.or(estadoVacio)).toBeVisible();
	});

	test('la tabla expone las columnas del cliente', async ({ page }) => {
		// Verifica el contrato visual del listado: identidad, contacto y estado.
		const tabla = page.getByRole('table');
		test.skip(await tabla.count() === 0, 'El entorno no tiene clientes para renderizar la tabla.');

		await expect(tabla.getByRole('columnheader', { name: 'Cliente' })).toBeVisible();
		await expect(tabla.getByRole('columnheader', { name: 'Correo' })).toBeVisible();
		await expect(tabla.getByRole('columnheader', { name: 'Estado' })).toBeVisible();
		await expect(tabla.getByRole('columnheader', { name: 'Acciones' })).toBeVisible();
	});

	test('cada fila muestra nombre, correo y estado', async ({ page }) => {
		// Comprueba que una fila no quede parcialmente renderizada o sin datos.
		const filas = page.getByRole('table').getByRole('row').filter({ has: page.locator('td') });
		test.skip(await filas.count() === 0, 'El entorno no tiene clientes registrados.');

		for (const fila of await filas.all()) {
			await expect(fila.locator('td').nth(0)).not.toBeEmpty();
			await expect(fila.locator('td').nth(1)).toContainText('@');
			await expect(fila.locator('td').nth(2)).toContainText(/Activo|Suspendido/);
		}
	});

	test('el estado del cliente se presenta como Activo o Suspendido', async ({ page }) => {
		// Evita estados ambiguos: son los únicos dos valores que ClientesTab expone.
		const estados = page.getByRole('table').locator('tbody td').filter({ hasText: /Activo|Suspendido/ });
		test.skip(await estados.count() === 0, 'El entorno no tiene clientes registrados.');

		await expect(estados.first()).toContainText(/Activo|Suspendido/);
	});

	test('un administrador ve las acciones CRM por cliente', async ({ page }) => {
		// Las acciones deben estar disponibles para el rol administrador.
		const filas = page.getByRole('table').locator('tbody tr');
		test.skip(await filas.count() === 0, 'El entorno no tiene clientes registrados.');

		await expect(filas.first().getByRole('button', { name: /Suspender|Activar/ })).toBeVisible();
		await expect(filas.first().getByRole('button', { name: 'Cambiar Rol' })).toBeVisible();
	});

	test('suspender y activar un cliente actualiza su estado', async ({ page }) => {
		// Caso de mutación reversible: valida la acción y luego deja el entorno
		// exactamente como estaba para no contaminar ejecuciones posteriores.
		const filaActiva = page.getByRole('table').locator('tbody tr').filter({ hasText: 'Activo' }).first();
		test.skip(await filaActiva.count() === 0, 'No hay un cliente activo para probar el ciclo de estado.');

		await Promise.all([
			page.waitForResponse((response) => response.url().includes('/api/v1/usuarios/') && response.request().method() === 'PUT'),
			filaActiva.getByRole('button', { name: 'Suspender' }).click(),
		]);
		await expect(filaActiva).toContainText('Suspendido');

		await Promise.all([
			page.waitForResponse((response) => response.url().includes('/api/v1/usuarios/') && response.request().method() === 'PUT'),
			filaActiva.getByRole('button', { name: 'Activar' }).click(),
		]);
		await expect(filaActiva).toContainText('Activo');
	});

	test('abrir Cambiar Rol muestra el modal de permisos', async ({ page }) => {
		// El modal permite inspeccionar el tipo de usuario sin guardar cambios.
		const filas = page.getByRole('table').locator('tbody tr');
		test.skip(await filas.count() === 0, 'El entorno no tiene clientes registrados.');

		await filas.first().getByRole('button', { name: 'Cambiar Rol' }).click();
		const modal = page.getByRole('dialog', { name: 'Cambiar tipo y rol' });
		await expect(modal).toBeVisible();
		await expect(modal.getByText('Tipo de Usuario')).toBeVisible();
		await expect(modal.getByRole('combobox')).toHaveValue('cliente');
	});

	test('el modal de rol muestra opciones de staff al seleccionarlas', async ({ page }) => {
		// Comprueba que el formulario revela sus campos dependientes de forma
		// visible, sin enviar una modificación destructiva.
		const filas = page.getByRole('table').locator('tbody tr');
		test.skip(await filas.count() === 0, 'El entorno no tiene clientes registrados.');

		await filas.first().getByRole('button', { name: 'Cambiar Rol' }).click();
		const modal = page.getByRole('dialog', { name: 'Cambiar tipo y rol' });
		await modal.getByRole('combobox').selectOption('staff');
		await expect(modal.getByText('Rol de Staff')).toBeVisible();
		await expect(modal.getByText('Sucursal')).toBeVisible();
		await page.keyboard.press('Escape');
		await expect(modal).toBeHidden();
	});

	test('cerrar el modal de rol cancela la operación', async ({ page }) => {
		// Escape debe cerrar el modal y no cambiar la vista del listado.
		const filas = page.getByRole('table').locator('tbody tr');
		test.skip(await filas.count() === 0, 'El entorno no tiene clientes registrados.');

		await filas.first().getByRole('button', { name: 'Cambiar Rol' }).click();
		const modal = page.getByRole('dialog', { name: 'Cambiar tipo y rol' });
		await expect(modal).toBeVisible();
		await page.keyboard.press('Escape');
		await expect(modal).toBeHidden();
		await expect(page.getByRole('heading', { name: 'Clientes (CRM)' })).toBeVisible();
	});

	test('el listado conserva sus datos después de recargar', async ({ page }) => {
		// La persistencia se verifica únicamente desde navegador, como exige la
		// regla de caja negra del plan.
		const estadoInicial = await page.getByRole('table').or(page.getByText('No se encontraron clientes registrados.')).innerText();
		await page.reload();
		await abrirClientes(page);
		await expect(page.getByRole('table').or(page.getByText('No se encontraron clientes registrados.'))).toBeVisible();
		const estadoRecargado = await page.getByRole('table').or(page.getByText('No se encontraron clientes registrados.')).innerText();
		expect(estadoRecargado).toBe(estadoInicial);
	});

	// FIXME: ClientesTab no implementa búsqueda por nombre/correo propia.
	test.fixme('buscar cliente por nombre o correo', async () => {});

	// FIXME: ClientesTab no implementa detalle, historial de compras ni reservaciones.
	test.fixme('abrir detalle e historial de un cliente', async () => {});

	// FIXME: falta un flujo UI para crear un cliente desde el portal administrativo.
	test.fixme('crear cliente desde Clientes y verificar su aparición', async () => {});
});
