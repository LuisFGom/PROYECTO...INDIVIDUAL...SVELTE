<script>
  import { onMount } from 'svelte'
  import { formatters } from '../utils/validators.js'
  import clienteService from '../services/clienteService.js'
  import Alert from '../components/Alert.svelte'
  import Swal from 'sweetalert2'

  const ITEMS_PER_PAGE = 10

  let clientesEliminados = []
  let filteredClientes = []
  let paginatedClientes = []
  let loading = false
  let searchTerm = ''
  let currentPage = 1
  let totalPages = 1
  let successMessage = ''

  onMount(async () => {
    cargarDatos()
  })

  // Reactividad para paginación
  $: {
    const start = (currentPage - 1) * ITEMS_PER_PAGE
    const end = start + ITEMS_PER_PAGE
    paginatedClientes = [...filteredClientes.slice(start, end)]
    totalPages = Math.ceil(filteredClientes.length / ITEMS_PER_PAGE) || 1
  }

  const normalizeEliminado = (eliminado) => {
    return {
      id: eliminado.id,
      clienteEliminadoId: eliminado.clienteEliminadoId,
      nombre: eliminado.nombreClienteEliminado || eliminado.nombreCompleto || 'N/A',
      documento: eliminado.documentoClienteEliminado || eliminado.documento || '-',
      email: eliminado.emailClienteEliminado || eliminado.email || '-',
      telefono: eliminado.telefonoClienteEliminado || eliminado.telefono || '-',
      eliminadoPor: eliminado.nombreAdministrador || eliminado.eliminadoPor || 'Admin',
      tipoEliminacion: eliminado.tipoEliminacion || 'Desactivación',
      fechaEliminacion: eliminado.fechaEliminacion
    }
  }

  const cargarDatos = async () => {
    loading = true
    try {
      let data = await clienteService.getEliminados()
      data = Array.isArray(data) ? data : []
      clientesEliminados = data.map(normalizeEliminado)
      clientesEliminados.sort((a, b) => new Date(b.fechaEliminacion) - new Date(a.fechaEliminacion))
      searchTerm = ''
      currentPage = 1
      filterClientes()
    } catch (err) {
      Swal.fire('Error', err.message || 'Error al cargar clientes eliminados', 'error')
      console.error('Error cargando eliminados:', err)
    } finally {
      loading = false
    }
  }

  const filterClientes = () => {
    if (!searchTerm.trim()) {
      filteredClientes = [...clientesEliminados]
    } else {
      const term = searchTerm.toLowerCase()
      filteredClientes = clientesEliminados.filter(c =>
        c.nombre?.toLowerCase().includes(term) ||
        c.documento?.toLowerCase().includes(term) ||
        c.email?.toLowerCase().includes(term)
      )
    }
    currentPage = 1
  }

  const goToPage = (page) => {
    if (page >= 1 && page <= totalPages) {
      currentPage = page
    }
  }

  const restaurar = async (cliente) => {
    const result = await Swal.fire({
      title: '¿Restaurar cliente?',
      html: `Se restaurará el cliente <strong>${cliente.nombre}</strong>`,
      icon: 'question',
      showCancelButton: true,
      confirmButtonColor: '#10b981',
      cancelButtonColor: '#6b7280',
      confirmButtonText: 'Sí, restaurar',
      cancelButtonText: 'Cancelar'
    })

    if (!result.isConfirmed) return

    try {
      await clienteService.restaurar(cliente.clienteEliminadoId)
      successMessage = `Cliente ${cliente.nombre} restaurado correctamente`
      setTimeout(() => { successMessage = '' }, 3000)
      clientesEliminados = clientesEliminados.filter(c => c.id !== cliente.id)
      filterClientes()
    } catch (err) {
      Swal.fire('Error', err.message || 'Error al restaurar cliente', 'error')
      console.error('Error restaurando:', err)
    }
  }
</script>

<div class="elimclientes-page">
  <div class="page-header">
    <h1><i class="fas fa-trash-restore"></i> Historial de Eliminaciones de Clientes</h1>
  </div>

  {#if successMessage}
    <Alert type="success" message={successMessage} />
  {/if}

  <div class="card">
    <div class="card-header">
      <input
        class="input search-input"
        type="text"
        placeholder="Buscar por nombre, documento o email..."
        bind:value={searchTerm}
        on:input={() => filterClientes()}
      />
    </div>

    <div class="card-body">
      {#if loading}
        <div class="loading">Cargando clientes eliminados...</div>
      {:else if clientesEliminados.length === 0}
        <div class="empty-state">
          <i class="fas fa-inbox"></i>
          <p>No hay clientes eliminados</p>
        </div>
      {:else if filteredClientes.length === 0}
        <div class="empty-state">
          <i class="fas fa-search"></i>
          <p>No se encontraron resultados</p>
        </div>
      {:else}
        <div class="table-wrapper">
          <table class="table">
            <thead>
              <tr>
                <th>FECHA ELIMINACIÓN</th>
                <th>NOMBRE CLIENTE</th>
                <th>DOCUMENTO</th>
                <th>EMAIL</th>
                <th>TELÉFONO</th>
                <th>ELIMINADO POR</th>
                <th>TIPO</th>
                <th>ACCIONES</th>
              </tr>
            </thead>
            <tbody>
              {#each paginatedClientes as cliente (cliente.id)}
                <tr>
                  <td>{formatters.formatDate(cliente.fechaEliminacion)}</td>
                  <td>
                    <strong>{cliente.nombre}</strong>
                  </td>
                  <td>{cliente.documento}</td>
                  <td>{cliente.email}</td>
                  <td>{cliente.telefono}</td>
                  <td>{cliente.eliminadoPor}</td>
                  <td>
                    <span class="badge badge-warning">
                      {cliente.tipoEliminacion}
                    </span>
                  </td>
                  <td>
                    <div class="action-buttons">
                      <button
                        class="btn btn-sm btn-success"
                        on:click={() => restaurar(cliente)}
                        title="Restaurar cliente"
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

        <!-- Pagination Controls -->
        <div class="pagination-controls">
          <button class="btn-page" on:click={() => goToPage(1)} disabled={currentPage === 1}>
            <i class="fas fa-step-backward"></i> Primera
          </button>
          <button class="btn-page" on:click={() => goToPage(currentPage - 1)} disabled={currentPage === 1}>
            <i class="fas fa-chevron-left"></i> Anterior
          </button>

          <button class="btn-page" on:click={() => goToPage(currentPage - 1000)} disabled={currentPage <= 1000}>
            <span>-1000</span>
          </button>
          <button class="btn-page" on:click={() => goToPage(currentPage - 100)} disabled={currentPage <= 100}>
            <span>-100</span>
          </button>
          <button class="btn-page" on:click={() => goToPage(currentPage - 10)} disabled={currentPage <= 10}>
            <span>-10</span>
          </button>

          {#each Array.from({ length: Math.min(6, totalPages) }, (_, i) => {
            const pageNum = Math.max(1, Math.min(currentPage - 2, totalPages - 5)) + i
            return pageNum
          }) as pageNum}
            <button
              class="btn-page {pageNum === currentPage ? 'active' : ''}"
              on:click={() => goToPage(pageNum)}
            >
              {pageNum}
            </button>
          {/each}

          <button class="btn-page" on:click={() => goToPage(currentPage + 10)} disabled={currentPage >= totalPages - 9}>
            <span>+10</span>
          </button>
          <button class="btn-page" on:click={() => goToPage(currentPage + 100)} disabled={currentPage >= totalPages - 99}>
            <span>+100</span>
          </button>
          <button class="btn-page" on:click={() => goToPage(currentPage + 1000)} disabled={currentPage >= totalPages - 999}>
            <span>+1000</span>
          </button>

          <button class="btn-page" on:click={() => goToPage(currentPage + 1)} disabled={currentPage === totalPages}>
            Siguiente <i class="fas fa-chevron-right"></i>
          </button>
          <button class="btn-page" on:click={() => goToPage(totalPages)} disabled={currentPage === totalPages}>
            Última <i class="fas fa-step-forward"></i>
          </button>
        </div>

        <div class="table-footer">
          <p>Página {currentPage} de {totalPages} • Total: {filteredClientes.length} registros (10 por página)</p>
        </div>
      {/if}
    </div>
  </div>
</div>

<style>
  .elimclientes-page {
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
    letter-spacing: 0.05em;
  }

  .table td {
    padding: 1rem;
    border-bottom: 1px solid #e5e7eb;
    color: #1f2937;
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

  .badge-warning {
    background: #fef3c7;
    color: #92400e;
  }

  .action-buttons {
    display: flex;
    gap: 0.5rem;
    flex-wrap: wrap;
  }

  .btn, .btn-sm {
    padding: 0.5rem 1rem;
    border: none;
    border-radius: 0.375rem;
    font-size: 0.875rem;
    font-weight: 600;
    cursor: pointer;
    display: inline-flex;
    align-items: center;
    gap: 0.5rem;
    transition: all 0.2s;
  }

  .btn-sm {
    padding: 0.375rem 0.75rem;
    font-size: 0.75rem;
  }

  .btn-success {
    background: #10b981;
    color: white;
  }

  .btn-success:hover {
    background: #059669;
  }

  .pagination-controls {
    display: flex;
    justify-content: center;
    align-items: center;
    gap: 0.5rem;
    padding: 1.5rem;
    flex-wrap: wrap;
    border-top: 1px solid #e5e7eb;
  }

  .btn-page {
    padding: 0.5rem 0.75rem;
    border: 1px solid #d1d5db;
    background: white;
    color: #374151;
    border-radius: 0.375rem;
    cursor: pointer;
    font-size: 0.875rem;
    font-weight: 500;
    transition: all 0.2s;
    display: flex;
    align-items: center;
    gap: 0.25rem;
  }

  .btn-page:hover:not(:disabled) {
    background: #f3f4f6;
    border-color: #9ca3af;
  }

  .btn-page.active {
    background: #3b82f6;
    color: white;
    border-color: #3b82f6;
  }

  .btn-page:disabled {
    opacity: 0.5;
    cursor: not-allowed;
  }

  .table-footer {
    padding: 1rem 1.5rem;
    background: #f9fafb;
    border-top: 1px solid #e5e7eb;
    text-align: center;
    font-size: 0.875rem;
    color: #6b7280;
  }

  .table-footer p {
    margin: 0;
  }

  input {
    width: 100%;
    padding: 0.625rem;
    border: 1px solid #e2e8f0;
    border-radius: 0.375rem;
    font-size: 0.875rem;
    box-sizing: border-box;
  }

  input:focus {
    outline: none;
    border-color: #3b82f6;
    box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
  }
</style>
