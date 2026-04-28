<script>
  import { dataStore } from '../stores/store.js'
  import productoService from '../services/productoService.js'
  import { validators, formatters } from '../utils/validators.js'
  import FormInput from '../components/FormInput.svelte'
  import Alert from '../components/Alert.svelte'
  import Swal from 'sweetalert2'

  const ITEMS_PER_PAGE = 10

  let productos = []
  let loading = true
  let searchTerm = ''
  let filteredProductos = []
  let paginatedProductos = []
  let isAdmin = false
  let editingProductoId = null
  let showFormModal = false
  let successMessage = ''
  let currentPage = 1
  let totalPages = 1

  let formData = {
    nombre: '',
    codigo: '',
    descripcion: '',
    precioCosto: '',
    precio: '',
    cantidad: '',
    stockMinimo: 0,
    iva: 0
  }

  let errors = {}

  // Cálculos reactivos para el resumen
  $: precioCosto = parseFloat(formData.precioCosto) || 0
  $: precioVenta = parseFloat(formData.precio) || 0
  $: ivaPercent = parseFloat(formData.iva) || 0
  $: margen = precioVenta - precioCosto
  $: porcentajeMargen = precioCosto > 0 ? ((margen / precioCosto) * 100).toFixed(2) : 0
  $: montoIva = (precioVenta * ivaPercent) / 100
  $: precioFinal = precioVenta + montoIva

  // Reactividad para paginación: recalcular cuando cambie filteredProductos o currentPage
  $: {
    const start = (currentPage - 1) * ITEMS_PER_PAGE
    const end = start + ITEMS_PER_PAGE
    paginatedProductos = [...filteredProductos.slice(start, end)]
    totalPages = Math.ceil(filteredProductos.length / ITEMS_PER_PAGE) || 1
  }

  const normalizeProducto = (producto) => {
    // Mapea los nombres del backend al formato del frontend
    const normalizado = {
      ...producto,
      // Mapear precios
      precio: producto.precio || producto.precioVenta || 0,
      precioCosto: producto.precioCompra || producto.precioCosto || 0,
      // Mapear stock - toma el valor aunque sea 0
      cantidad: producto.stockInicial !== undefined ? producto.stockInicial : (producto.stock !== undefined ? producto.stock : (producto.cantidad || 0)),
      stock: producto.stockInicial !== undefined ? producto.stockInicial : (producto.stock !== undefined ? producto.stock : (producto.cantidad || 0)),
      // Mapear IVA - importante: conservar 0 si es 0, no confundir con undefined
      // NOTA: El backend devuelve "porcentajeIVA" con mayúsculas
      iva: producto.porcentajeIVA !== undefined ? producto.porcentajeIVA : (producto.iva !== undefined ? producto.iva : 0)
    }
    return normalizado
  }

  const loadProductos = async () => {
    loading = true
    try {
      const user = JSON.parse(localStorage.getItem('currentUser') || '{}')
      const rolNormalizado = (user.rol || user.role || '').toLowerCase()
      isAdmin = rolNormalizado === 'admin' || user.roleId === 1
      
      const data = await productoService.getAll()
      // Cargar TODOS los productos (no limitados a 500)
      productos = Array.isArray(data) ? data.filter(p => p.activo !== false).map(normalizeProducto) : []
      
      console.log('=== PRODUCTOS CARGADOS ===')
      console.log('Total cargados:', productos.length)
      if (productos.length > 0) {
        console.log('Primero: Nombre:', productos[0].nombre, 'ID:', productos[0].id, 'Cantidad:', productos[0].cantidad)
        console.log('Último: Nombre:', productos[productos.length - 1].nombre, 'ID:', productos[productos.length - 1].id)
      }
      
      // Ordenar por fecha DESC (más nuevos primero)
      productos.sort((a, b) => new Date(b.fechaCreacion) - new Date(a.fechaCreacion))
      console.log('Después de ordenar - Primero: Nombre:', productos[0]?.nombre, 'ID:', productos[0]?.id)
      
      searchTerm = ''
      currentPage = 1
      filterProductos()
    } catch (error) {
      await Swal.fire('Error', error.message, 'error')
    } finally {
      loading = false
    }
  }

  const filterProductos = () => {
    if (!searchTerm.trim()) {
      filteredProductos = [...productos]
    } else {
      const term = searchTerm.toLowerCase()
      filteredProductos = productos.filter(p =>
        String(p.nombre || '').toLowerCase().includes(term) ||
        String(p.descripcion || '').toLowerCase().includes(term)
      )
    }
    currentPage = 1
  }

  const goToPage = (page) => {
    if (page >= 1 && page <= totalPages) {
      currentPage = page
    }
  }

  const validateForm = () => {
    errors = {}
    if (!formData.nombre?.trim()) errors.nombre = 'El nombre es requerido'
    if (!formData.codigo?.trim()) errors.codigo = 'El código es requerido'
    if (!formData.precioCosto || parseFloat(formData.precioCosto) <= 0) errors.precioCosto = 'El precio de compra debe ser mayor a 0'
    if (!formData.precio || parseFloat(formData.precio) <= 0) errors.precio = 'El precio de venta debe ser mayor a 0'
    if (!formData.cantidad || parseInt(formData.cantidad) < 0) errors.cantidad = 'La cantidad debe ser un número positivo'
    return Object.keys(errors).length === 0
  }

  const handleEdit = (producto) => {
    editingProductoId = producto.id
    const normalized = normalizeProducto(producto)
    
    formData = {
      nombre: normalized.nombre || '',
      codigo: normalized.codigo || normalized.codigoBarras || '',
      descripcion: normalized.descripcion || '',
      precioCosto: normalized.precioCosto ? parseFloat(normalized.precioCosto) : '',
      precio: normalized.precio ? parseFloat(normalized.precio) : '',
      cantidad: normalized.cantidad ? parseInt(normalized.cantidad) : '',
      stockMinimo: normalized.stockMinimo ? parseInt(normalized.stockMinimo) : 0,
      iva: normalized.iva ? parseFloat(normalized.iva) : 0
    }
    console.log('=== EDITAR PRODUCTO ===')
    console.log('Campos del producto del backend:')
    console.log('  - porcentajeIVA:', producto.porcentajeIVA, '(usado en normalización)')
    console.log('  - iva:', producto.iva, '(alternativa)')
    console.log('FormData final - iva:', formData.iva)
    showFormModal = true
  }

  const handleDelete = async (producto) => {
    const result = await Swal.fire({
      title: '¿Eliminar producto?',
      text: `Se eliminará ${producto.nombre}`,
      icon: 'warning',
      showCancelButton: true,
      confirmButtonText: 'Sí, eliminar'
    })

    if (result.isConfirmed) {
      try {
        await productoService.delete(producto.id)
        successMessage = 'Producto eliminado correctamente'
        setTimeout(() => { successMessage = '' }, 3000)
        await loadProductos()
      } catch (error) {
        await Swal.fire('Error', error.message, 'error')
      }
    }
  }

  const handleSubmit = async () => {
    if (!validateForm()) return

    try {
      let dataToSend
      
      if (editingProductoId) {
        // UPDATE: espera precioVenta, precioCompra, stock
        dataToSend = {
          id: editingProductoId,
          nombre: formData.nombre.trim(),
          descripcion: formData.descripcion?.trim() || '',
          precioVenta: parseFloat(formData.precio) || 0,
          precioCompra: parseFloat(formData.precioCosto) || 0,
          stock: parseInt(formData.cantidad) || 0,
          stockMinimo: parseInt(formData.stockMinimo) || 0,
          PorcentajeIVA: parseFloat(formData.iva) || 0
        }
        console.log('=== UPDATE PRODUCTO ===')
        console.log('Enviando UPDATE con precioVenta, precioCompra, stock, PorcentajeIVA')
      } else {
        // CREATE: espera precio, precioCompra, stockInicial
        dataToSend = {
          nombre: formData.nombre.trim(),
          codigo: formData.codigo.trim(),
          descripcion: formData.descripcion?.trim() || '',
          precio: parseFloat(formData.precio) || 0,
          precioCompra: parseFloat(formData.precioCosto) || 0,
          stockInicial: parseInt(formData.cantidad) || 0,
          stockMinimo: parseInt(formData.stockMinimo) || 0,
          PorcentajeIVA: parseFloat(formData.iva) || 0
        }
        console.log('=== CREATE PRODUCTO ===')
        console.log('Enviando CREATE con precio, precioCompra, stockInicial, PorcentajeIVA')
      }

      console.log('DataToSend:', JSON.stringify(dataToSend))

      let respuesta
      let productoNuevoId = null
      
      if (editingProductoId) {
        respuesta = await productoService.update(editingProductoId, dataToSend)
        successMessage = 'Producto actualizado correctamente'
      } else {
        respuesta = await productoService.create(dataToSend)
        successMessage = 'Producto creado correctamente'
        
        // Extraer ID del mensaje devuelto por el backend
        const idMatch = respuesta?.message?.match(/ID:\s*(\d+)/)
        if (idMatch && idMatch[1]) {
          productoNuevoId = parseInt(idMatch[1])
          console.log('ID del nuevo producto extraído:', productoNuevoId)
        }
      }
      
      console.log('=== RESPUESTA DEL BACKEND ===')
      console.log('Mensaje:', respuesta?.message)
      
      setTimeout(() => { successMessage = '' }, 3000)
      showFormModal = false
      resetForm()
      await loadProductos()
    } catch (error) {
      await Swal.fire('Error', error.message, 'error')
    }
  }

  const resetForm = () => {
    formData = { nombre: '', codigo: '', descripcion: '', precioCosto: '', precio: '', cantidad: '', stockMinimo: 0, iva: 0 }
    errors = {}
    editingProductoId = null
  }

  const handleNewProducto = () => {
    resetForm()
    showFormModal = true
  }

  loadProductos()
</script>

<div class="productos-page">
  <div class="page-header">
    <h1><i class="fas fa-box"></i> Productos</h1>
    {#if isAdmin}
      <button class="btn btn-primary" on:click={handleNewProducto}>
        <i class="fas fa-plus"></i> Nuevo Producto
      </button>
    {/if}
  </div>

  {#if successMessage}
    <Alert type="success" message={successMessage} />
  {/if}

  <div class="card">
    <div class="card-header">
      <input
        class="input search-input"
        type="text"
        placeholder="Buscar por nombre, descripción o categoría..."
        bind:value={searchTerm}
        on:input={() => filterProductos()}
      />
    </div>

    <div class="card-body">
      {#if loading}
        <div class="loading">Cargando productos...</div>
      {:else if productos.length === 0}
        <div class="empty-state">
          <i class="fas fa-box"></i>
          <p>No hay productos registrados</p>
        </div>
      {:else if filteredProductos.length === 0}
        <div class="empty-state">
          <i class="fas fa-search"></i>
          <p>No se encontraron resultados</p>
        </div>
      {:else}
        <div class="table-wrapper">
          <table class="table">
            <thead>
              <tr>
                <th>NOMBRE</th>
                <th>DESCRIPCIÓN</th>
                <th>PRECIO</th>
                <th>STOCK</th>
                <th>ESTADO</th>
                <th>CREADO</th>
                <th>MODIFICADO</th>
                {#if isAdmin}
                  <th>ACCIONES</th>
                {/if}
              </tr>
            </thead>
            <tbody>
              {#each paginatedProductos as producto (producto.id)}
                <tr>
                  <td><strong>{producto.nombre}</strong></td>
                  <td>{producto.descripcion || '-'}</td>
                  <td>{formatters.formatCurrency(producto.precio)}</td>
                  <td>{producto.stock !== undefined ? producto.stock : (producto.cantidad || 0)}</td>
                  <td>
                    <span class="badge badge-success">
                      {producto.activo ? 'Activo' : 'Inactivo'}
                    </span>
                  </td>
                  <td>{formatters.formatDate(producto.fechaCreacion)}</td>
                  <td>{formatters.formatDate(producto.fechaModificacion)}</td>
                  {#if isAdmin}
                    <td>
                      <div class="action-buttons">
                        <button
                          class="btn btn-sm btn-warning"
                          on:click={() => handleEdit(producto)}
                          title="Editar"
                        >
                          <i class="fas fa-edit"></i> Editar
                        </button>
                        <button
                          class="btn btn-sm btn-danger"
                          on:click={() => handleDelete(producto)}
                          title="Eliminar"
                        >
                          <i class="fas fa-trash"></i> Eliminar
                        </button>
                      </div>
                    </td>
                  {/if}
                </tr>
              {/each}
            </tbody>
          </table>
        </div>

        <!-- PAGINACIÓN -->
        <div class="pagination">
          <button class="btn-page" on:click={() => goToPage(1)} disabled={currentPage === 1} title="Primera página">
            <i class="fas fa-step-backward"></i> Primera
          </button>
          <button class="btn-page" on:click={() => goToPage(currentPage - 1)} disabled={currentPage === 1} title="Página anterior">
            <i class="fas fa-chevron-left"></i> Anterior
          </button>

          {#each Array.from({ length: Math.min(5, totalPages) }, (_, i) => i + 1) as page}
            {#if page <= totalPages}
              <button
                class="btn-page"
                class:active={currentPage === page}
                on:click={() => goToPage(page)}
              >
                {page}
              </button>
            {/if}
          {/each}

          <button class="btn-page" on:click={() => goToPage(currentPage + 1)} disabled={currentPage === totalPages} title="Siguiente página">
            <i class="fas fa-chevron-right"></i> Siguiente
          </button>
          <button class="btn-page" on:click={() => goToPage(totalPages)} disabled={currentPage === totalPages} title="Última página">
            <i class="fas fa-step-forward"></i> Última
          </button>
        </div>

        <div class="pagination-info">
          Página <strong>{currentPage}</strong> de <strong>{totalPages}</strong> • Total: <strong>{filteredProductos.length}</strong> registros ({ITEMS_PER_PAGE} por página)
        </div>
      {/if}
    </div>
  </div>

  {#if showFormModal}
    <div class="modal-overlay" role="button" tabindex="0" on:click={() => { showFormModal = false }} on:keydown={(e) => { if (e.key === 'Escape') showFormModal = false; }}>
      <div class="modal-content" role="presentation" on:click|stopPropagation>
        <div class="modal-header">
          <h2 class="modal-title">
            {editingProductoId ? 'Editar Producto' : 'Nuevo Producto'}
          </h2>
          <button class="modal-close" on:click={() => { showFormModal = false }}>
            <i class="fas fa-times"></i>
          </button>
        </div>
        <div class="modal-body">
          <!-- Nombre -->
          <div class="form-group">
            <label for="nombre">Nombre <span style="color: red;">*</span></label>
            <input
              id="nombre"
              type="text"
              placeholder="Nombre del producto"
              bind:value={formData.nombre}
            />
            {#if errors.nombre}
              <div class="error-text">{errors.nombre}</div>
            {/if}
          </div>

          <!-- Código -->
          <div class="form-group">
            <label for="codigo">Código <span style="color: red;">*</span></label>
            <input
              id="codigo"
              type="text"
              placeholder="Código único"
              bind:value={formData.codigo}
              disabled={editingProductoId ? true : false}
            />
            {#if errors.codigo}
              <div class="error-text">{errors.codigo}</div>
            {/if}
          </div>

          <!-- Descripción -->
          <div class="form-group">
            <label for="descripcion">Descripción:</label>
            <textarea
              id="descripcion"
              placeholder="Descripción del producto"
              bind:value={formData.descripcion}
              style="min-height: 60px;"
            ></textarea>
          </div>

          <!-- Precios lado a lado -->
          <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 1rem; margin-bottom: 1rem;">
            <div class="form-group">
              <label for="precioCosto">Precio Compra <span style="color: red;">*</span></label>
              <input
                id="precioCosto"
                type="number"
                placeholder="Ej: 10.99 (máx 2 decimales)"
                step="0.01"
                min="0.01"
                bind:value={formData.precioCosto}
              />
              <small style="color: #64748B; font-size: 12px; display: block; margin-top: 3px;">Decimal positivo, máximo 2 decimales</small>
              {#if errors.precioCosto}
                <div class="error-text">{errors.precioCosto}</div>
              {/if}
            </div>
            <div class="form-group">
              <label for="precio">Precio Venta <span style="color: red;">*</span></label>
              <input
                id="precio"
                type="number"
                placeholder="Ej: 19.99 (máx 2 decimales)"
                step="0.01"
                min="0.01"
                bind:value={formData.precio}
              />
              <small style="color: #64748B; font-size: 12px; display: block; margin-top: 3px;">Decimal positivo, máximo 2 decimales</small>
              {#if errors.precio}
                <div class="error-text">{errors.precio}</div>
              {/if}
            </div>
          </div>

          <!-- Stock lado a lado -->
          <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 1rem; margin-bottom: 1rem;">
            <div class="form-group">
              <label for="cantidad">Stock Inicial <span style="color: red;">*</span></label>
              <input
                id="cantidad"
                type="number"
                placeholder="Ej: 50 (solo números enteros)"
                min="0"
                step="1"
                bind:value={formData.cantidad}
              />
              <small style="color: #64748B; font-size: 12px; display: block; margin-top: 3px;">Número entero positivo</small>
              {#if errors.cantidad}
                <div class="error-text">{errors.cantidad}</div>
              {/if}
            </div>
            <div class="form-group">
              <label for="stockMinimo">Stock Mínimo:</label>
              <input
                id="stockMinimo"
                type="number"
                placeholder="0"
                min="0"
                step="1"
                bind:value={formData.stockMinimo}
              />
              <small style="color: #64748B; display: block; margin-top: 3px;">Debe ser &lt;= al Stock Inicial</small>
            </div>
          </div>

          <!-- IVA -->
          <div class="form-group">
            <label for="iva">IVA (%):</label>
            <input
              id="iva"
              type="number"
              placeholder="0"
              step="0.01"
              min="0"
              max="100"
              bind:value={formData.iva}
            />
          </div>

          <!-- RESUMEN DE CÁLCULOS -->
          <div style="background: #F0F9FF; border: 1px solid #BFDBFE; border-radius: 8px; padding: 1rem; margin: 1rem 0; text-align: center;">
            <label style="color: #1E40AF; font-weight: 700; display: block; margin-bottom: 0.5rem;">PRECIO FINAL (con IVA):</label>
            <div style="color: #1E40AF; font-weight: 700; font-size: 24px;">{formatters.formatCurrency(precioFinal)}</div>
          </div>
        </div>
        <div class="modal-footer">
          <button class="btn btn-secondary" on:click={() => { showFormModal = false }}>
            Cancelar
          </button>
          <button class="btn btn-primary" on:click={handleSubmit}>
            {editingProductoId ? 'Actualizar' : 'Crear'}
          </button>
        </div>
      </div>
    </div>
  {/if}
</div>

<style>
  .productos-page {
    padding: 2rem;
  }

  .page-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 2rem;
  }

  .page-header h1 {
    margin: 0;
    font-size: 2rem;
    display: flex;
    align-items: center;
    gap: 0.5rem;
    color: #1f2937;
  }

  .card {
    background: white;
    border-radius: 8px;
    box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
    overflow: hidden;
  }

  .card-header {
    padding: 1.5rem;
    border-bottom: 1px solid #e5e7eb;
  }

  .search-input {
    width: 100%;
    max-width: 400px;
  }

  .card-body {
    padding: 0;
  }

  .loading, .empty-state {
    padding: 2rem;
    text-align: center;
    color: #6b7280;
  }

  .empty-state i {
    font-size: 2rem;
    margin-bottom: 0.5rem;
    display: block;
  }

  .table-wrapper {
    overflow-x: auto;
  }

  .table {
    width: 100%;
    border-collapse: collapse;
  }

  .table thead {
    background: #f9fafb;
    border-bottom: 1px solid #e5e7eb;
  }

  .table th {
    padding: 1rem;
    text-align: left;
    font-weight: 600;
    font-size: 0.875rem;
    color: #374151;
    text-transform: uppercase;
  }

  .table td {
    padding: 1rem;
    border-bottom: 1px solid #e5e7eb;
  }

  .table tbody tr:hover {
    background: #f9fafb;
  }

  .badge {
    display: inline-block;
    padding: 0.25rem 0.75rem;
    border-radius: 9999px;
    font-size: 0.75rem;
    font-weight: 600;
  }

  .badge-success {
    background: #d1fae5;
    color: #065f46;
  }

  .action-buttons {
    display: flex;
    gap: 0.5rem;
    flex-wrap: wrap;
  }

  .btn {
    padding: 0.5rem 1rem;
    border: none;
    border-radius: 6px;
    cursor: pointer;
    font-size: 0.875rem;
    font-weight: 500;
    display: inline-flex;
    align-items: center;
    gap: 0.5rem;
    transition: all 0.2s;
  }

  .btn-primary {
    background: #3b82f6;
    color: white;
  }

  .btn-primary:hover {
    background: #2563eb;
  }

  .btn-secondary {
    background: #e5e7eb;
    color: #1f2937;
  }

  .btn-secondary:hover {
    background: #d1d5db;
  }

  .btn-warning {
    background: #f59e0b;
    color: white;
  }

  .btn-warning:hover {
    background: #d97706;
  }

  .btn-danger {
    background: #ef4444;
    color: white;
  }

  .btn-danger:hover {
    background: #dc2626;
  }

  .btn-sm {
    padding: 0.375rem 0.75rem;
    font-size: 0.75rem;
  }

  .pagination {
    display: flex;
    justify-content: center;
    align-items: center;
    gap: 0.5rem;
    padding: 1.5rem;
    border-top: 1px solid #e5e7eb;
    flex-wrap: wrap;
  }

  .btn-page {
    padding: 0.5rem 0.75rem;
    border: 1px solid #d1d5db;
    background: white;
    color: #374151;
    border-radius: 4px;
    cursor: pointer;
    font-size: 0.875rem;
    transition: all 0.2s;
  }

  .btn-page:hover:not(:disabled) {
    background: #f3f4f6;
    border-color: #9ca3af;
  }

  .btn-page:disabled {
    opacity: 0.5;
    cursor: not-allowed;
  }

  .btn-page.active {
    background: #3b82f6;
    color: white;
    border-color: #3b82f6;
  }

  .pagination-info {
    text-align: center;
    padding: 1rem;
    color: #6b7280;
    font-size: 0.875rem;
    border-top: 1px solid #e5e7eb;
  }

  .modal-overlay {
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background: rgba(0, 0, 0, 0.5);
    display: flex;
    align-items: center;
    justify-content: center;
    z-index: 1000;
  }

  .modal-content {
    background: white;
    border-radius: 8px;
    box-shadow: 0 10px 25px rgba(0, 0, 0, 0.2);
    max-width: 500px;
    width: 90%;
    max-height: 90vh;
    overflow-y: auto;
  }

  .modal-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 1.5rem;
    border-bottom: 1px solid #e5e7eb;
  }

  .modal-title {
    margin: 0;
    font-size: 1.25rem;
  }

  .modal-close {
    background: none;
    border: none;
    font-size: 1.5rem;
    cursor: pointer;
    color: #6b7280;
  }

  .modal-body {
    padding: 1.5rem;
  }

  .modal-footer {
    display: flex;
    justify-content: flex-end;
    gap: 0.75rem;
    padding: 1.5rem;
    border-top: 1px solid #e5e7eb;
  }

  .form-group {
    margin-bottom: 1rem;
  }

  .form-group label {
    display: block;
    margin-bottom: 0.5rem;
    font-weight: 600;
    color: #374151;
  }

  .form-group input,
  .form-group textarea {
    width: 100%;
    padding: 0.75rem;
    border: 1px solid #d1d5db;
    border-radius: 6px;
    font-size: 0.875rem;
  }

  .form-group input:focus,
  .form-group textarea:focus {
    outline: none;
    border-color: #3b82f6;
    box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
  }

  .error-text {
    color: #ef4444;
    font-size: 0.75rem;
    margin-top: 0.25rem;
  }
</style>
