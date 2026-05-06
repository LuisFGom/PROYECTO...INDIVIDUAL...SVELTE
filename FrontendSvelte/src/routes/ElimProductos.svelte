<script>
  import { onMount } from 'svelte'
  import productoService from '../services/productoService.js'
  import auditoriaService from '../services/auditoriaService.js'
  import Alert from '../components/Alert.svelte'
  import Swal from 'sweetalert2'

  const ITEMS_PER_PAGE = 10

  let productosEliminados = []
  let filteredProductos = []
  let paginatedProductos = []
  let loading = false
  let searchTerm = ''
  let currentPage = 1
  let totalPages = 1
  let successMessage = ''
  let auditoriaMap = {} // idProducto -> {nombreUsuario, fechaAccion}

  onMount(async () => {
    await cargarDatos()
  })

  $: {
    const start = (currentPage - 1) * ITEMS_PER_PAGE
    const end = start + ITEMS_PER_PAGE
    paginatedProductos = [...filteredProductos.slice(start, end)]
    totalPages = Math.ceil(filteredProductos.length / ITEMS_PER_PAGE) || 1
  }

  const normalizeEliminado = (p) => {
    const aud = auditoriaMap[p.id]
    return {
      id: p.id,
      productoEliminadoId: p.id,
      codigo: p.codigo || '-',
      nombre: p.nombre || '-',
      descripcion: p.descripcion || '-',
      precioCosto: p.precioCompra ?? 0,
      precioVenta: p.precio ?? 0,
      stock: p.stock ?? 0,
      eliminadoPor: p.nombreAdministrador || aud?.nombreUsuario || '-',
      tipoEliminacion: 'Desactivación',
      fechaEliminacion: aud?.fechaAccion || p.fechaEliminacion || null
    }
  }

  const cargarDatos = async () => {
    loading = true
    try {
      // 1. Cargar auditoría de eliminaciones
      const auditoria = await auditoriaService.getEliminacionesProductos()
      auditoriaMap = {}
      if (Array.isArray(auditoria)) {
        for (const a of auditoria) {
          if (a.registroAfectadoId) {
            auditoriaMap[a.registroAfectadoId] = {
              nombreUsuario: a.nombreUsuario,
              fechaAccion: a.fechaAccion
            }
          }
        }
      }
      // 2. Cargar productos eliminados y cruzar con auditoría
      let data = await productoService.getEliminados()
      data = Array.isArray(data) ? data : []
      productosEliminados = data.map(normalizeEliminado)
      productosEliminados.sort((a, b) => new Date(b.fechaEliminacion) - new Date(a.fechaEliminacion))
      searchTerm = ''
      currentPage = 1
      filterProductos()
    } catch (err) {
      Swal.fire('Error', err.message || 'Error al cargar productos eliminados', 'error')
      console.error('Error cargando eliminados:', err)
    } finally {
      loading = false
    }
  }

  const filterProductos = () => {
    if (!searchTerm.trim()) {
      filteredProductos = [...productosEliminados]
    } else {
      const term = searchTerm.toLowerCase()
      filteredProductos = productosEliminados.filter(p =>
        p.nombre?.toLowerCase().includes(term) ||
        p.codigo?.toLowerCase().includes(term) ||
        p.eliminadoPor?.toLowerCase().includes(term)
      )
    }
    currentPage = 1
  }

  const goToPage = (page) => {
    if (page >= 1 && page <= totalPages) {
      currentPage = page
    }
  }

  const restaurar = async (producto) => {
    const result = await Swal.fire({
      title: '¿Restaurar producto?',
      html: `Se restaurará el producto <strong>${producto.nombre}</strong>`,
      icon: 'question',
      showCancelButton: true,
      confirmButtonColor: '#10b981',
      cancelButtonColor: '#6b7280',
      confirmButtonText: 'Sí, restaurar',
      cancelButtonText: 'Cancelar'
    })

    if (!result.isConfirmed) return

    try {
      await productoService.restaurar(producto.productoEliminadoId)
      successMessage = `Producto ${producto.nombre} restaurado correctamente`
      setTimeout(() => { successMessage = '' }, 3000)
      productosEliminados = productosEliminados.filter(p => p.id !== producto.id)
      filterProductos()
    } catch (err) {
      Swal.fire('Error', err.message || 'Error al restaurar producto', 'error')
      console.error('Error restaurando:', err)
    }
  }
</script>

<div class="elimproductos-page">
  <div class="page-header">
    <h1><i class="fas fa-trash-restore"></i> Historial de Desactivaciones de Productos</h1>
  </div>

  {#if successMessage}
    <Alert type="success" message={successMessage} />
  {/if}

  <div class="card">
    <div class="card-header">
      <input
        class="input search-input"
        type="text"
        placeholder="Buscar por nombre, código, administrador..."
        bind:value={searchTerm}
        on:input={() => filterProductos()}
      />
    </div>

    <div class="card-body">
      {#if loading}
        <div class="loading">Cargando productos eliminados...</div>
      {:else if productosEliminados.length === 0}
        <div class="empty-state">
          <i class="fas fa-inbox"></i>
          <p>No hay productos eliminados</p>
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
                <th>FECHA</th>
                <th>CÓDIGO</th>
                <th>PRODUCTO</th>
                <th>COSTO</th>
                <th>VENTA</th>
                <th>STOCK</th>
                <th>ELIMINADO POR</th>
                <th>TIPO</th>
                <th>ACCIONES</th>
              </tr>
            </thead>
            <tbody>
              {#each paginatedProductos as producto (producto.id)}
                <tr>
                  <td>{producto.fechaEliminacion ? new Date(producto.fechaEliminacion).toLocaleString('es-EC') : '-'}</td>
                  <td><span class="badge badge-secondary">{producto.codigo}</span></td>
                  <td>{producto.nombre}</td>
                  <td>${producto.precioCosto.toFixed(2)}</td>
                  <td>${producto.precioVenta.toFixed(2)}</td>
                  <td>{producto.stock}</td>
                  <td>{producto.eliminadoPor}</td>
                  <td>
                    <span class="badge badge-warning">
                      {producto.tipoEliminacion}
                    </span>
                  </td>
                  <td>
                    <div class="action-buttons">
                      <button
                        class="btn btn-sm btn-success"
                        on:click={() => restaurar(producto)}
                        title="Restaurar producto"
                      >
                        <i class="fas fa-undo"></i> Restaurar
                      </button>
                    </div>
                  </td>
                </tr>
              {/each}
            </tbody>
          </table>
        </div>
        <div class="pagination">
          <button class="btn-page" on:click={() => goToPage(1)} disabled={currentPage === 1}>Primera</button>
          <button class="btn-page" on:click={() => goToPage(currentPage - 1)} disabled={currentPage === 1}>Anterior</button>
          <span>Página {currentPage} de {totalPages}</span>
          <button class="btn-page" on:click={() => goToPage(currentPage + 1)} disabled={currentPage === totalPages}>Siguiente</button>
          <button class="btn-page" on:click={() => goToPage(totalPages)} disabled={currentPage === totalPages}>Última</button>
        </div>
      {/if}
    </div>
  </div>
</div>

<style>
  .elimproductos-page {
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
    border-radius: 12px;
    box-shadow: 0 2px 8px rgba(0,0,0,0.05);
    margin-bottom: 2rem;
  }
  .card-header {
    padding: 1rem 1.5rem;
    border-bottom: 1px solid #f1f5f9;
    background: #f9fafb;
  }
  .input.search-input {
    width: 100%;
    padding: 0.75rem 1rem;
    border-radius: 8px;
    border: 1px solid #e5e7eb;
    font-size: 1rem;
    outline: none;
    margin-bottom: 0;
  }
  .card-body {
    padding: 1.5rem;
  }
  .table-wrapper {
    overflow-x: auto;
  }
  .table {
    width: 100%;
    border-collapse: collapse;
    margin-bottom: 1rem;
  }
  .table th, .table td {
    padding: 0.75rem 1rem;
    border-bottom: 1px solid #f1f5f9;
    text-align: left;
    font-size: 0.95rem;
  }
  .table th {
    background: #f3f4f6;
    color: #64748b;
    font-weight: 600;
    text-transform: uppercase;
    font-size: 0.85rem;
  }
  .badge {
    display: inline-block;
    padding: 0.25em 0.6em;
    font-size: 0.85em;
    font-weight: 600;
    border-radius: 0.5em;
    background: #f3f4f6;
    color: #6366f1;
  }
  .badge-warning {
    background: #fef3c7;
    color: #b45309;
  }
  .badge-secondary {
    background: #e0e7ff;
    color: #3730a3;
  }
  .action-buttons {
    display: flex;
    gap: 0.5rem;
  }
  .btn-page {
    background: #f3f4f6;
    color: #1f2937;
    border: none;
    border-radius: 6px;
    padding: 0.4rem 0.9rem;
    margin: 0 0.2rem;
    font-size: 0.95rem;
    cursor: pointer;
    font-weight: 500;
    transition: background 0.2s;
  }
  .btn-page:disabled {
    opacity: 0.5;
    cursor: not-allowed;
  }
  .btn {
    border: none;
    border-radius: 6px;
    padding: 0.4rem 0.9rem;
    font-size: 0.95rem;
    cursor: pointer;
    font-weight: 500;
    transition: background 0.2s;
  }
  .btn-success {
    background: #10b981;
    color: white;
  }
  .loading, .empty-state {
    text-align: center;
    color: #64748b;
    padding: 2rem 0;
  }
  .empty-state i {
    font-size: 3rem;
    color: #d1d5db;
    display: block;
    margin-bottom: 1rem;
  }

  .flex-center {
    display: flex;
    justify-content: center;
    align-items: center;
  }

  .btn {
    display: inline-flex;
    align-items: center;
    gap: 0.5rem;
    padding: 0.5rem 1rem;
    border: none;
    border-radius: 4px;
    cursor: pointer;
    font-size: 0.875rem;
    transition: all 200ms ease;
  }

  .btn-sm {
    padding: 0.25rem 0.75rem;
    font-size: 0.8rem;
  }

  .btn-success {
    background: #10b981;
    color: white;
  }

  .btn-success:hover {
    background: #059669;
  }

  .btn-danger {
    background: #ef4444;
    color: white;
  }

  .btn-danger:hover {
    background: #dc2626;
  }
</style>
