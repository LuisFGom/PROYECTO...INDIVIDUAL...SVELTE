<script>
  import { onMount } from 'svelte'
  import { formatters } from '../utils/validators.js'
  import usuarioService from '../services/usuarioService.js'
  import Alert from '../components/Alert.svelte'
  import Swal from 'sweetalert2'

  const ITEMS_PER_PAGE = 10

  let usuariosEliminados = []
  let filteredUsuarios = []
  let paginatedUsuarios = []
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
    paginatedUsuarios = [...filteredUsuarios.slice(start, end)]
    totalPages = Math.ceil(filteredUsuarios.length / ITEMS_PER_PAGE) || 1
  }

  const normalizeEliminado = (eliminado) => {
    return {
      id: eliminado.id,
      usuarioEliminadoId: eliminado.usuarioEliminadoId,
      nombreUsuario: eliminado.nombreUsuarioEliminado || eliminado.nombreUsuario || 'N/A',
      nombre: eliminado.nombreUsuarioEliminado || '',
      apellido: '',
      email: eliminado.emailUsuarioEliminado || eliminado.email || '-',
      rolNombre: eliminado.rolUsuarioEliminado || eliminado.rolNombre || 'N/A',
      rol: eliminado.rolUsuarioEliminado || eliminado.rol || 'N/A',
      eliminadoPor: eliminado.nombreAdministrador || eliminado.eliminadoPor || 'Admin',
      tipoEliminacion: eliminado.tipoEliminacion || 'Desactivación',
      fechaEliminacion: eliminado.fechaEliminacion
    }
  }

  const cargarDatos = async () => {
    loading = true
    try {
      let data = await usuarioService.getEliminados()
      data = Array.isArray(data) ? data : []
      usuariosEliminados = data.map(normalizeEliminado)
      usuariosEliminados.sort((a, b) => new Date(b.fechaEliminacion) - new Date(a.fechaEliminacion))
      searchTerm = ''
      currentPage = 1
      filterUsuarios()
    } catch (err) {
      Swal.fire('Error', err.message || 'Error al cargar usuarios eliminados', 'error')
      console.error('Error cargando eliminados:', err)
    } finally {
      loading = false
    }
  }

  const filterUsuarios = () => {
    if (!searchTerm.trim()) {
      filteredUsuarios = [...usuariosEliminados]
    } else {
      const term = searchTerm.toLowerCase()
      filteredUsuarios = usuariosEliminados.filter(u =>
        u.nombreUsuario?.toLowerCase().includes(term) ||
        u.nombre?.toLowerCase().includes(term) ||
        u.email?.toLowerCase().includes(term)
      )
    }
    currentPage = 1
  }

  const goToPage = (page) => {
    if (page >= 1 && page <= totalPages) {
      currentPage = page
    }
  }

  const restaurar = async (usuario) => {
    const result = await Swal.fire({
      title: '¿Restaurar usuario?',
      html: `Se restaurará el usuario <strong>${usuario.nombreUsuario}</strong>`,
      icon: 'question',
      showCancelButton: true,
      confirmButtonColor: '#10b981',
      cancelButtonColor: '#6b7280',
      confirmButtonText: 'Sí, restaurar',
      cancelButtonText: 'Cancelar'
    })

    if (!result.isConfirmed) return

    try {
      await usuarioService.restaurar(usuario.usuarioEliminadoId)
      successMessage = `Usuario ${usuario.nombreUsuario} restaurado correctamente`
      setTimeout(() => { successMessage = '' }, 3000)
      usuariosEliminados = usuariosEliminados.filter(u => u.id !== usuario.id)
      filterUsuarios()
    } catch (err) {
      Swal.fire('Error', err.message || 'Error al restaurar usuario', 'error')
      console.error('Error restaurando:', err)
    }
  }
</script>

<div class="elimusuarios-page">
  <div class="page-header">
    <h1><i class="fas fa-trash-restore"></i> Historial de Eliminaciones de Usuarios</h1>
  </div>

  {#if successMessage}
    <Alert type="success" message={successMessage} />
  {/if}

  <div class="card">
    <div class="card-header">
      <input
        class="input search-input"
        type="text"
        placeholder="Buscar por nombre usuario, nombre o email..."
        bind:value={searchTerm}
        on:input={() => filterUsuarios()}
      />
    </div>

    <div class="card-body">
      {#if loading}
        <div class="loading">Cargando usuarios eliminados...</div>
      {:else if usuariosEliminados.length === 0}
        <div class="empty-state">
          <i class="fas fa-inbox"></i>
          <p>No hay usuarios eliminados</p>
        </div>
      {:else if filteredUsuarios.length === 0}
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
                <th>NOMBRE USUARIO</th>
                <th>EMAIL</th>
                <th>ROL</th>
                <th>ELIMINADO POR</th>
                <th>TIPO</th>
                <th>ACCIONES</th>
              </tr>
            </thead>
            <tbody>
              {#each paginatedUsuarios as usuario (usuario.id)}
                <tr>
                  <td>{formatters.formatDate(usuario.fechaEliminacion)}</td>
                  <td>
                    <strong>{usuario.nombreUsuario}</strong>
                    <div style="font-size: 0.75rem; color: #6b7280;">{usuario.nombre} {usuario.apellido}</div>
                  </td>
                  <td>{usuario.email || usuario.correo || '-'}</td>
                  <td>
                    <span class="badge badge-info">
                      {usuario.rolNombre || usuario.rol || 'N/A'}
                    </span>
                  </td>
                  <td>{usuario.eliminadoPor || 'Admin'}</td>
                  <td>
                    <span class="badge badge-warning">
                      {usuario.tipoEliminacion || 'Desactivación'}
                    </span>
                  </td>
                  <td>
                    <div class="action-buttons">
                      <button
                        class="btn btn-sm btn-success"
                        on:click={() => restaurar(usuario)}
                        title="Restaurar usuario"
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
          <p>Página {currentPage} de {totalPages} • Total: {filteredUsuarios.length} registros (10 por página)</p>
        </div>
      {/if}
    </div>
  </div>
</div>

<style>
  .elimusuarios-page {
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

  .badge-info {
    background: #dbeafe;
    color: #1e40af;
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
