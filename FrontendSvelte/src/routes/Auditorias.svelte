<script>
  import { onMount } from 'svelte'
  import { formatters } from '../utils/validators.js'
  import auditoriasService from '../services/auditoriasService.js'
  import Swal from 'sweetalert2'

  const ITEMS_PER_PAGE = 15

  let auditorias = []
  let filteredAuditorias = []
  let paginatedAuditorias = []
  let loading = false
  let searchTerm = ''
  let filterModulo = ''
  let filterTipoAccion = ''
  let filterFechaDesde = ''
  let filterFechaHasta = ''
  let currentPage = 1
  let totalPages = 1
  let modalAbierto = false
  let auditoriaSeleccionada = null

  // Colores para diferentes módulos
  const colorModulo = {
    'Usuarios': '#3B82F6',
    'Clientes': '#10b981',
    'Productos': '#F59E0B',
    'Ventas': '#EF4444',
    'Reportes': '#8B5CF6',
    'Seguridad': '#EC4899',
    'Auditorias': '#6B7280'
  }

  onMount(async () => {
    cargarDatos()
  })

  // Reactividad para paginación
  $: {
    const start = (currentPage - 1) * ITEMS_PER_PAGE
    const end = start + ITEMS_PER_PAGE
    paginatedAuditorias = [...filteredAuditorias.slice(start, end)]
    totalPages = Math.ceil(filteredAuditorias.length / ITEMS_PER_PAGE) || 1
  }

  const cargarDatos = async () => {
    loading = true
    try {
      let data = await auditoriasService.getAll({ take: 500 })
      data = Array.isArray(data) ? data : []
      auditorias = data.sort((a, b) => new Date(b.fechaAccion) - new Date(a.fechaAccion))
      currentPage = 1
      filtrarAuditorias()
    } catch (err) {
      Swal.fire('Error', err.message || 'Error al cargar auditorías', 'error')
      console.error('Error cargando auditorías:', err)
    } finally {
      loading = false
    }
  }

  const filtrarAuditorias = () => {
    let resultado = [...auditorias]

    // Filtro por búsqueda (usuario, descripción)
    if (searchTerm.trim()) {
      const term = searchTerm.toLowerCase()
      resultado = resultado.filter(a =>
        a.nombreUsuario?.toLowerCase().includes(term) ||
        a.descripcion?.toLowerCase().includes(term) ||
        a.registroAfectadoDescripcion?.toLowerCase().includes(term)
      )
    }

    // Filtro por módulo
    if (filterModulo) {
      resultado = resultado.filter(a => a.modulo === filterModulo)
    }

    // Filtro por tipo de acción
    if (filterTipoAccion) {
      resultado = resultado.filter(a => a.tipoAccion === filterTipoAccion)
    }

    // Filtro por fecha desde
    if (filterFechaDesde) {
      const fechaDesde = new Date(filterFechaDesde)
      resultado = resultado.filter(a => new Date(a.fechaAccion) >= fechaDesde)
    }

    // Filtro por fecha hasta
    if (filterFechaHasta) {
      const fechaHasta = new Date(filterFechaHasta)
      fechaHasta.setHours(23, 59, 59, 999)
      resultado = resultado.filter(a => new Date(a.fechaAccion) <= fechaHasta)
    }

    filteredAuditorias = resultado
    currentPage = 1
  }

  const limpiarFiltros = () => {
    searchTerm = ''
    filterModulo = ''
    filterTipoAccion = ''
    filterFechaDesde = ''
    filterFechaHasta = ''
    filtrarAuditorias()
  }

  const goToPage = (page) => {
    if (page >= 1 && page <= totalPages) {
      currentPage = page
    }
  }

  const abrirModalDetalles = (auditoria) => {
    auditoriaSeleccionada = auditoria
    modalAbierto = true
  }

  const cerrarModal = () => {
    modalAbierto = false
    auditoriaSeleccionada = null
  }

  const getColorModulo = (modulo) => {
    return colorModulo[modulo] || '#6B7280'
  }

  const getTipoAccionBadge = (tipoAccion) => {
    const estilos = {
      'Crear': { bg: '#D1FAE5', text: '#047857', icon: 'fa-plus-circle' },
      'Editar': { bg: '#DBEAFE', text: '#1e40af', icon: 'fa-edit' },
      'Eliminar': { bg: '#FEE2E2', text: '#DC2626', icon: 'fa-trash' },
      'Desactivar': { bg: '#FEE2E2', text: '#DC2626', icon: 'fa-ban' },
      'Anular': { bg: '#FEE2E2', text: '#DC2626', icon: 'fa-times-circle' },
      'Editar': { bg: '#DBEAFE', text: '#1e40af', icon: 'fa-edit' },
      'Ver': { bg: '#F3E8FF', text: '#6D28D9', icon: 'fa-eye' },
      'Descargar': { bg: '#E0E7FF', text: '#3730A3', icon: 'fa-download' },
      'Login': { bg: '#D1FAE5', text: '#047857', icon: 'fa-sign-in-alt' },
      'Logout': { bg: '#DBEAFE', text: '#1e40af', icon: 'fa-sign-out-alt' },
      'Cambiar Contraseña': { bg: '#FEF3C7', text: '#92400E', icon: 'fa-key' }
    }
    return estilos[tipoAccion] || { bg: '#F3F4F6', text: '#374151', icon: 'fa-circle' }
  }

  // Obtener módulos únicos para el filtro - REACTIVO
  $: modulosUnicos = [...new Set(auditorias.map(a => a.modulo))].sort()

  // Obtener tipos de acción únicos para el filtro - REACTIVO (sin Reinsertar ni Desactivar)
  $: tiposAccionUnicos = [...new Set(auditorias.map(a => a.tipoAccion).filter(t => t !== 'Reinsertar' && t !== 'Desactivar'))].sort()
</script>

<div class="auditorias-page">
  <div class="page-header">
    <h1><i class="fas fa-shield-alt"></i> Auditorías del Sistema</h1>
    <p class="subtitle">Registro completo de todas las acciones realizadas en el sistema</p>
  </div>

  <div class="card">
    <div class="card-header">
      <h3 class="filters-title">Filtros</h3>
      <div class="filters-container">
        <div class="filter-group">
          <label>Usuario</label>
          <input
            class="input"
            type="text"
            placeholder="ID o nombre del usuario"
            bind:value={searchTerm}
          />
        </div>

        <div class="filter-group">
          <label>Módulo</label>
          <select class="input" bind:value={filterModulo}>
            <option value="">-- Todos --</option>
            {#each modulosUnicos as modulo}
              <option value={modulo}>{modulo}</option>
            {/each}
          </select>
        </div>

        <div class="filter-group">
          <label>Tipo Acción</label>
          <select class="input" bind:value={filterTipoAccion}>
            <option value="">-- Todos --</option>
            {#each tiposAccionUnicos as tipo}
              <option value={tipo}>{tipo}</option>
            {/each}
          </select>
        </div>

        <div class="filter-group">
          <label>Desde</label>
          <div class="date-input-wrapper">
            <input
              class="input"
              type="date"
              bind:value={filterFechaDesde}
            />
            <i class="fas fa-calendar"></i>
          </div>
        </div>

        <div class="filter-group">
          <label>Hasta</label>
          <div class="date-input-wrapper">
            <input
              class="input"
              type="date"
              bind:value={filterFechaHasta}
            />
            <i class="fas fa-calendar"></i>
          </div>
        </div>

        <div class="filter-buttons">
          <button class="btn btn-primary" on:click={filtrarAuditorias} title="Filtrar">
            <i class="fas fa-filter"></i> Filtrar
          </button>
          <button class="btn btn-secondary" on:click={limpiarFiltros} title="Limpiar filtros">
            <i class="fas fa-times"></i> Limpiar
          </button>
        </div>
      </div>
    </div>

    <div class="card-body">
      {#if loading}
        <div class="loading">
          <i class="fas fa-spinner fa-spin"></i> Cargando auditorías...
        </div>
      {:else if auditorias.length === 0}
        <div class="empty-state">
          <i class="fas fa-inbox"></i>
          <p>No hay auditorías disponibles</p>
        </div>
      {:else if filteredAuditorias.length === 0}
        <div class="empty-state">
          <i class="fas fa-search"></i>
          <p>No se encontraron resultados con los filtros aplicados</p>
        </div>
      {:else}
        <div class="table-wrapper">
          <table class="table">
            <thead>
              <tr>
                <th>FECHA Y HORA</th>
                <th>USUARIO</th>
                <th>ACCIÓN</th>
                <th>MÓDULO</th>
                <th>REGISTRO AFECTADO</th>
                <th>ACCIONES</th>
              </tr>
            </thead>
            <tbody>
              {#each paginatedAuditorias as auditoria (auditoria.id)}
                {@const estilo = getTipoAccionBadge(auditoria.tipoAccion)}
                {@const colorMod = getColorModulo(auditoria.modulo)}
                <tr>
                  <td class="fecha-cell">
                    <strong>{new Date(auditoria.fechaAccion).toLocaleString('es-EC')}</strong>
                  </td>
                  <td class="usuario-cell">
                    <strong>{auditoria.nombreUsuario}</strong>
                  </td>
                  <td>
                    <span class="badge" style="background: {estilo.bg}; color: {estilo.text};">
                      <i class="fas {estilo.icon}"></i> {auditoria.tipoAccion}
                    </span>
                  </td>
                  <td>
                    <span class="badge-modulo" style="border-left: 4px solid {colorMod};">
                      {auditoria.modulo}
                    </span>
                  </td>
                  <td class="registro-cell">
                    {#if auditoria.registroAfectadoDescripcion}
                      <small title={auditoria.registroAfectadoDescripcion}>
                        {auditoria.registroAfectadoDescripcion}
                      </small>
                    {:else}
                      <small>-</small>
                    {/if}
                  </td>
                  <td class="acciones-cell">
                    <button class="btn-investigar" on:click={() => abrirModalDetalles(auditoria)} title="Ver detalles">
                      <i class="fas fa-search"></i> Investigar
                    </button>
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
            <i class="fas fa-chevron-left"></i>
          </button>
          <span class="pagination-info">Página {currentPage} de {totalPages}</span>
          <button class="btn-page" on:click={() => goToPage(currentPage + 1)} disabled={currentPage === totalPages}>
            <i class="fas fa-chevron-right"></i>
          </button>
          <button class="btn-page" on:click={() => goToPage(totalPages)} disabled={currentPage === totalPages}>
            <i class="fas fa-step-forward"></i> Última
          </button>
        </div>

        <div class="stats">
          <strong>Total de auditorías:</strong> {auditorias.length} | <strong>Mostradas:</strong> {filteredAuditorias.length}
        </div>
      {/if}
    </div>
  </div>
</div>

<!-- Modal de Detalles -->
{#if modalAbierto && auditoriaSeleccionada}
  <div class="modal-overlay" on:click={cerrarModal}>
    <div class="modal-content" on:click|stopPropagation>
      <div class="modal-header">
        <h2>Detalles de Auditoría</h2>
        <button class="modal-close" on:click={cerrarModal}>
          <i class="fas fa-times"></i>
        </button>
      </div>

      <div class="modal-body">
        <div class="detail-row">
          <label>Fecha y Hora:</label>
          <span>{new Date(auditoriaSeleccionada.fechaAccion).toLocaleString('es-EC')}</span>
        </div>

        <div class="detail-row">
          <label>Usuario:</label>
          <span><strong>{auditoriaSeleccionada.nombreUsuario}</strong></span>
        </div>

        <div class="detail-row">
          <label>Acción:</label>
          {#if auditoriaSeleccionada}
            {@const estilo = getTipoAccionBadge(auditoriaSeleccionada.tipoAccion)}
            <span class="badge" style="background: {estilo.bg}; color: {estilo.text};">
              <i class="fas {estilo.icon}"></i> {auditoriaSeleccionada.tipoAccion}
            </span>
          {/if}
        </div>

        <div class="detail-row">
          <label>Módulo:</label>
          <span>{auditoriaSeleccionada.modulo}</span>
        </div>

        <div class="detail-row">
          <label>Registro Afectado:</label>
          <span><strong>{auditoriaSeleccionada.registroAfectadoDescripcion || '-'}</strong></span>
        </div>

        <div class="detail-row full-width">
          <label>Descripción:</label>
          <p class="description-text">{auditoriaSeleccionada.descripcion || '-'}</p>
        </div>

        {#if auditoriaSeleccionada.direccionIP}
          <div class="detail-row">
            <label>Dirección IP:</label>
            <span>{auditoriaSeleccionada.direccionIP}</span>
          </div>
        {/if}
      </div>

      <div class="modal-footer">
        <button class="btn btn-secondary" on:click={cerrarModal}>
          <i class="fas fa-times"></i> Cerrar
        </button>
      </div>
    </div>
  </div>
{/if}

<style>
  .auditorias-page {
    padding: 2rem;
  }

  .page-header {
    margin-bottom: 2rem;
  }

  .page-header h1 {
    font-size: 28px;
    color: #1F2937;
    margin: 0 0 0.5rem 0;
  }

  .subtitle {
    color: #6B7280;
    font-size: 14px;
    margin: 0;
  }

  .card {
    background: white;
    border-radius: 8px;
    box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
    overflow: hidden;
  }

  .card-header {
    padding: 1.5rem;
    background: #F9FAFB;
    border-bottom: 1px solid #E5E7EB;
  }

  .filters-container {
    display: flex;
    gap: 1rem;
    flex-wrap: wrap;
  }

  .input {
    padding: 0.75rem;
    border: 1px solid #D1D5DB;
    border-radius: 4px;
    font-size: 14px;
    background: white;
  }

  .input:focus {
    outline: none;
    border-color: #3B82F6;
    box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
  }

  .search-input {
    flex: 1;
    min-width: 250px;
  }

  .filter-select {
    min-width: 150px;
  }

  .btn {
    padding: 0.75rem 1rem;
    border: none;
    border-radius: 4px;
    cursor: pointer;
    font-size: 14px;
    font-weight: 500;
    transition: all 0.2s;
    display: inline-flex;
    align-items: center;
    gap: 0.5rem;
  }

  .btn-primary {
    background: #3B82F6;
    color: white;
  }

  .btn-primary:hover {
    background: #2563EB;
  }

  .card-body {
    padding: 1.5rem;
  }

  .table-wrapper {
    overflow-x: auto;
    margin-bottom: 2rem;
  }

  .table {
    width: 100%;
    border-collapse: collapse;
    font-size: 13px;
  }

  .table thead {
    background: #F3F4F6;
  }

  .table th {
    padding: 12px;
    text-align: left;
    font-weight: 600;
    color: #374151;
    font-size: 12px;
    text-transform: uppercase;
    border-bottom: 2px solid #E5E7EB;
  }

  .table td {
    padding: 12px;
    border-bottom: 1px solid #E5E7EB;
    color: #111827;
  }

  .table tbody tr:hover {
    background: #F9FAFB;
  }

  .fecha-cell {
    font-family: 'Courier New', monospace;
    font-size: 12px;
    white-space: nowrap;
  }

  .usuario-cell {
    color: #1F2937;
    font-size: 13px;
  }

  .descripcion-cell {
    color: #6B7280;
    max-width: 300px;
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
  }

  .registro-cell {
    color: #6B7280;
    max-width: 200px;
  }

  .badge {
    display: inline-block;
    padding: 4px 10px;
    border-radius: 12px;
    font-size: 11px;
    font-weight: 600;
    white-space: nowrap;
  }

  .badge-modulo {
    display: inline-block;
    padding: 4px 10px;
    border-radius: 4px;
    font-size: 12px;
    font-weight: 600;
    background: #F3F4F6;
    color: #374151;
  }

  .pagination-controls {
    display: flex;
    gap: 0.5rem;
    justify-content: center;
    align-items: center;
    margin-top: 2rem;
    padding-top: 1rem;
    border-top: 1px solid #E5E7EB;
  }

  .btn-page {
    padding: 0.5rem 0.75rem;
    border: 1px solid #D1D5DB;
    background: white;
    border-radius: 4px;
    cursor: pointer;
    font-size: 12px;
    transition: all 0.2s;
  }

  .btn-page:hover:not(:disabled) {
    background: #3B82F6;
    color: white;
    border-color: #3B82F6;
  }

  .btn-page:disabled {
    opacity: 0.5;
    cursor: not-allowed;
  }

  .pagination-info {
    margin: 0 1rem;
    font-size: 14px;
    color: #6B7280;
  }

  .loading {
    text-align: center;
    padding: 3rem 2rem;
    color: #6B7280;
    font-size: 16px;
  }

  .loading i {
    margin-right: 0.5rem;
    font-size: 20px;
  }

  .empty-state {
    text-align: center;
    padding: 3rem 2rem;
  }

  .empty-state i {
    font-size: 48px;
    color: #D1D5DB;
    margin-bottom: 1rem;
    display: block;
  }

  .empty-state p {
    color: #6B7280;
    font-size: 16px;
  }

  .stats {
    text-align: right;
    margin-top: 1rem;
    padding-top: 1rem;
    border-top: 1px solid #E5E7EB;
    font-size: 13px;
    color: #6B7280;
  }

  .acciones-cell {
    text-align: center;
  }

  .btn-investigar {
    padding: 0.4rem 0.8rem;
    background: #10b981;
    color: white;
    border: none;
    border-radius: 4px;
    cursor: pointer;
    font-size: 12px;
    font-weight: 600;
    transition: all 0.2s;
    display: inline-flex;
    align-items: center;
    gap: 0.4rem;
    white-space: nowrap;
  }

  .btn-investigar:hover {
    background: #059669;
    transform: scale(1.05);
  }

  /* Modal Styles */
  .modal-overlay {
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background: rgba(0, 0, 0, 0.5);
    display: flex;
    justify-content: center;
    align-items: center;
    z-index: 1000;
  }

  .modal-content {
    background: white;
    border-radius: 8px;
    box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.1);
    max-width: 600px;
    width: 90%;
    max-height: 80vh;
    overflow-y: auto;
    animation: slideIn 0.3s ease-out;
  }

  @keyframes slideIn {
    from {
      transform: translateY(-50px);
      opacity: 0;
    }
    to {
      transform: translateY(0);
      opacity: 1;
    }
  }

  .modal-header {
    padding: 1.5rem;
    border-bottom: 1px solid #E5E7EB;
    display: flex;
    justify-content: space-between;
    align-items: center;
  }

  .modal-header h2 {
    margin: 0;
    font-size: 20px;
    color: #1F2937;
  }

  .modal-close {
    background: none;
    border: none;
    font-size: 24px;
    color: #6B7280;
    cursor: pointer;
    transition: color 0.2s;
  }

  .modal-close:hover {
    color: #1F2937;
  }

  .modal-body {
    padding: 2rem;
  }

  .detail-row {
    margin-bottom: 1.5rem;
    display: flex;
    flex-direction: column;
  }

  .detail-row.full-width {
    flex-direction: column;
  }

  .detail-row label {
    font-weight: 600;
    color: #374151;
    font-size: 13px;
    margin-bottom: 0.5rem;
    text-transform: uppercase;
    letter-spacing: 0.5px;
  }

  .detail-row span {
    color: #1F2937;
    font-size: 14px;
  }

  .description-text {
    color: #4B5563;
    font-size: 14px;
    line-height: 1.6;
    background: #F9FAFB;
    padding: 1rem;
    border-radius: 4px;
    border-left: 4px solid #3B82F6;
    margin: 0;
  }

  .modal-footer {
    padding: 1.5rem;
    border-top: 1px solid #E5E7EB;
    display: flex;
    justify-content: flex-end;
    gap: 0.5rem;
  }

  .btn-secondary {
    background: #6B7280;
    color: white;
  }

  .btn-secondary:hover {
    background: #4B5563;
  }

  /* Horizontal Filters Styles */
  .filters-title {
    margin: 0 0 1rem 0;
    font-size: 16px;
    font-weight: 600;
    color: #1F2937;
  }

  .filters-container {
    display: flex;
    gap: 1rem;
    flex-wrap: wrap;
    align-items: flex-end;
  }

  .filter-group {
    display: flex;
    flex-direction: column;
    gap: 0.5rem;
    flex: 1;
    min-width: 150px;
  }

  .filter-group label {
    font-size: 13px;
    font-weight: 600;
    color: #374151;
    text-transform: uppercase;
    letter-spacing: 0.5px;
  }

  .filter-group .input {
    width: 100%;
    padding: 0.75rem;
    border: 1px solid #D1D5DB;
    border-radius: 4px;
    font-size: 14px;
    background: white;
  }

  .filter-group .input:focus {
    outline: none;
    border-color: #3B82F6;
    box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
  }

  .date-input-wrapper {
    position: relative;
  }

  .date-input-wrapper .input {
    width: 100%;
    padding: 0.75rem;
    border: 1px solid #D1D5DB;
    border-radius: 4px;
    font-size: 14px;
    background: white;
    padding-right: 35px;
  }

  .date-input-wrapper i {
    position: absolute;
    right: 10px;
    top: 50%;
    transform: translateY(-50%);
    color: #9CA3AF;
    pointer-events: none;
    font-size: 14px;
  }

  .filter-buttons {
    display: flex;
    gap: 0.5rem;
    align-items: center;
    height: 38px;
  }

  .filter-buttons .btn {
    white-space: nowrap;
    padding: 0.65rem 1rem;
    font-size: 13px;
  }
</style>
