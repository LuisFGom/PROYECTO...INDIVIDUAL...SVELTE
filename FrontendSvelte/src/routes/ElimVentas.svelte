<script>
  import { onMount } from 'svelte'
  import { formatters } from '../utils/validators.js'
  import ventaService from '../services/ventaService.js'
  import Swal from 'sweetalert2'

  const ITEMS_PER_PAGE = 10

  let ventasEliminadas = []
  let filteredVentas = []
  let paginatedVentas = []
  let loading = false
  let searchTerm = ''
  let currentPage = 1
  let totalPages = 1

  // Modal de detalles
  let showDetallesModal = false
  let detallesFactura = null

  onMount(async () => {
    cargarDatos()
  })

  // Reactividad para paginación
  $: {
    const start = (currentPage - 1) * ITEMS_PER_PAGE
    const end = start + ITEMS_PER_PAGE
    paginatedVentas = [...filteredVentas.slice(start, end)]
    totalPages = Math.ceil(filteredVentas.length / ITEMS_PER_PAGE) || 1
  }

  const normalizeEliminado = (venta) => {
    return {
      id: venta.ventaId,
      ventaId: venta.ventaId,
      numeroFactura: venta.numeroFactura || '-',
      totalVenta: venta.totalVenta ?? 0,
      estado: 'Anulado',
      clienteNombre: venta.clienteNombre || '-',
      eliminadoPor: venta.eliminadoPor || '-',
      fechaVenta: venta.fechaVenta || null,
      ventaCompleta: venta
    }
  }

  const cargarDatos = async () => {
    loading = true
    try {
      let data = await ventaService.getEliminadas()
      data = Array.isArray(data) ? data : []
      ventasEliminadas = data.map(normalizeEliminado)
      ventasEliminadas.sort((a, b) => new Date(b.fechaVenta) - new Date(a.fechaVenta))
      searchTerm = ''
      currentPage = 1
      filterVentas()
    } catch (err) {
      Swal.fire('Error', err.message || 'Error al cargar facturas anuladas', 'error')
      console.error('Error cargando anuladas:', err)
    } finally {
      loading = false
    }
  }

  const filterVentas = () => {
    if (!searchTerm.trim()) {
      filteredVentas = [...ventasEliminadas]
    } else {
      const term = searchTerm.toLowerCase()
      filteredVentas = ventasEliminadas.filter(v =>
        v.numeroFactura?.toLowerCase().includes(term) ||
        v.clienteNombre?.toLowerCase().includes(term)
      )
    }
    currentPage = 1
  }

  const goToPage = (page) => {
    if (page >= 1 && page <= totalPages) {
      currentPage = page
    }
  }

  const normalizeVenta = (venta) => {
    return {
      id: venta.ventaId,
      numeroFactura: venta.numeroFactura || '-',
      fechaVenta: venta.fechaVenta,
      clienteNombre: venta.clienteNombre || 'N/A',
      usuarioNombre: venta.usuarioNombre || 'Administrator',
      detalles: (venta.detalles || []).map(d => ({
        nombre: d.producto?.nombre || d.nombre || 'N/A',
        cantidad: d.cantidad,
        precioUnitario: d.precioUnitario || d.precio,
        descuento: d.descuento || 0,
        total: d.total || (d.cantidad * (d.precioUnitario || d.precio) * (1 - (d.descuento || 0) / 100))
      })),
      subtotal: venta.subtotal || 0,
      totalImpuesto: venta.totalImpuesto || 0,
      totalVenta: venta.totalVenta || 0,
      porcentajeIVA: venta.porcentajeIVA || 0.12
    }
  }

  const abrirDetalles = async (venta) => {
    try {
      const ventaCompleta = await ventaService.getById(venta.ventaId)
      detallesFactura = normalizeVenta(ventaCompleta)
    } catch (error) {
      console.error('Error cargando detalles:', error)
      detallesFactura = normalizeVenta(venta.ventaCompleta)
    }
    showDetallesModal = true
  }
</script>

<div class="elimventas-page">
  <div class="page-header">
    <h1><i class="fas fa-file-invoice"></i> Historial de Facturas Anuladas</h1>
  </div>

  <div class="card">
    <div class="card-header">
      <input
        class="input search-input"
        type="text"
        placeholder="Buscar por número de factura o cliente..."
        bind:value={searchTerm}
        on:input={() => filterVentas()}
      />
    </div>

    <div class="card-body">
      {#if loading}
        <div class="loading">Cargando facturas anuladas...</div>
      {:else if ventasEliminadas.length === 0}
        <div class="empty-state">
          <i class="fas fa-inbox"></i>
          <p>No hay facturas anuladas</p>
        </div>
      {:else if filteredVentas.length === 0}
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
                <th>NÚMERO FACTURA</th>
                <th>TOTAL</th>
                <th>ESTADO</th>
                <th>ELIMINADO POR</th>
                <th>ACCIONES</th>
              </tr>
            </thead>
            <tbody>
              {#each paginatedVentas as venta (venta.id)}
                <tr>
                  <td>{venta.fechaVenta ? new Date(venta.fechaVenta).toLocaleString('es-EC') : '-'}</td>
                  <td>
                    <strong>{venta.numeroFactura}</strong>
                    <div style="font-size: 0.75rem; color: #6b7280;">{venta.clienteNombre}</div>
                  </td>
                  <td>{formatters.formatCurrency(venta.totalVenta)}</td>
                  <td>
                    <span class="badge badge-error">
                      {venta.estado}
                    </span>
                  </td>
                  <td>{venta.eliminadoPor}</td>
                  <td>
                    <div class="action-buttons">
                      <button
                        class="btn btn-sm btn-success"
                        on:click={() => abrirDetalles(venta)}
                        title="Observar"
                      >
                        <i class="fas fa-eye"></i> Observar
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
      {/if}
    </div>
  </div>

  <!-- Modal: Detalles de Factura -->
  {#if showDetallesModal && detallesFactura}
    <div class="modal-overlay" on:click={() => { showDetallesModal = false }}>
      <div class="modal" on:click|stopPropagation>
        <div class="modal-header">
          <h3>Factura: {detallesFactura.numeroFactura}</h3>
          <button class="modal-close" on:click={() => { showDetallesModal = false }}>
            <i class="fas fa-times"></i>
          </button>
        </div>
        <div class="modal-body">
          <div style="margin-bottom: 1rem; padding-bottom: 1rem; border-bottom: 1px solid #E5E7EB;">
            <div style="margin-bottom: 0.75rem; padding: 0.75rem; background: #FEE2E2; border-radius: 4px; color: #DC2626; text-align: center; font-weight: 600;">
              <i class="fas fa-exclamation-circle"></i> FACTURA ANULADA
            </div>
            <div style="margin-bottom: 0.5rem; margin-top: 1rem;">
              <strong>Fecha y Hora:</strong> {formatters.formatDateTime(detallesFactura.fechaVenta)}
            </div>
            <div style="margin-bottom: 0.5rem;">
              <strong>Cliente:</strong> {detallesFactura.clienteNombre || 'N/A'}
            </div>
            <div>
              <strong>Usuario:</strong> {detallesFactura.usuarioNombre || 'Administrator'}
            </div>
          </div>

          <h4 style="margin-top: 1.5rem; margin-bottom: 1rem;">Productos:</h4>
          <table class="tabla-detalles">
            <thead>
              <tr>
                <th>Producto</th>
                <th style="text-align: center;">Cantidad</th>
                <th style="text-align: right;">Precio Unit.</th>
                <th style="text-align: right;">Descuento</th>
                <th style="text-align: right;">Total</th>
              </tr>
            </thead>
            <tbody>
              {#each detallesFactura.detalles as detalle, index (index)}
                <tr>
                  <td>{detalle.nombre || 'N/A'}</td>
                  <td style="text-align: center;">{detalle.cantidad}</td>
                  <td style="text-align: right;">{formatters.formatCurrency(detalle.precioUnitario)}</td>
                  <td style="text-align: right;">{detalle.descuento}%</td>
                  <td style="text-align: right; font-weight: 600;">{formatters.formatCurrency(detalle.total)}</td>
                </tr>
              {/each}
            </tbody>
          </table>

          <div style="margin-top: 2rem; text-align: right;">
            <div style="margin-bottom: 0.75rem;">
              <strong>Subtotal:</strong> {formatters.formatCurrency(detallesFactura.subtotal)}
            </div>
            <div style="margin-bottom: 0.75rem;">
              <strong>IVA ({(detallesFactura.porcentajeIVA * 100).toFixed(0)}%):</strong> {formatters.formatCurrency(detallesFactura.totalImpuesto)}
            </div>
            <div style="border-top: 2px solid #3B82F6; padding-top: 0.75rem; font-size: 18px;">
              <strong>TOTAL:</strong> {formatters.formatCurrency(detallesFactura.totalVenta)}
            </div>
          </div>

          <div style="display: flex; gap: 1rem; justify-content: flex-end; margin-top: 2rem;">
            <button class="btn btn-secondary" on:click={() => { showDetallesModal = false }}>
              Cerrar
            </button>
          </div>
        </div>
      </div>
    </div>
  {/if}
</div>

<style>
  .elimventas-page {
    padding: 2rem;
  }

  .page-header {
    margin-bottom: 2rem;
  }

  .page-header h1 {
    font-size: 28px;
    color: #1F2937;
    margin: 0;
  }

  .card {
    background: #fff;
    border-radius: 8px;
    box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
    overflow: hidden;
  }

  .card-header {
    padding: 1.5rem;
    background: #f9fafb;
    border-bottom: 1px solid #E5E7EB;
  }

  .card-body {
    padding: 1.5rem;
  }

  .search-input {
    width: 100%;
    padding: 0.75rem;
    border: 1px solid #D1D5DB;
    border-radius: 4px;
    font-size: 14px;
    background: #fff;
  }

  .search-input:focus {
    outline: none;
    border-color: #3B82F6;
    box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
  }

  .input {
    padding: 0.75rem;
    border: 1px solid #D1D5DB;
    border-radius: 4px;
    font-size: 14px;
    background: #fff;
  }

  .input:focus {
    outline: none;
    border-color: #3B82F6;
  }

  .table-wrapper {
    overflow-x: auto;
    margin-bottom: 2rem;
  }

  .table {
    width: 100%;
    border-collapse: collapse;
    font-size: 14px;
  }

  .table thead {
    background: #f3f4f6;
  }

  .table th {
    padding: 12px;
    text-align: left;
    font-weight: 600;
    color: #374151;
    border-bottom: 2px solid #E5E7EB;
  }

  .table td {
    padding: 12px;
    border-bottom: 1px solid #E5E7EB;
  }

  .table tbody tr:hover {
    background: #f9fafb;
  }

  .badge {
    display: inline-block;
    padding: 4px 12px;
    border-radius: 12px;
    font-size: 12px;
    font-weight: 600;
  }

  .badge-error {
    background: #FEE2E2;
    color: #DC2626;
  }

  .action-buttons {
    display: flex;
    gap: 0.5rem;
  }

  .btn {
    padding: 0.5rem 1rem;
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

  .btn:hover:not(:disabled) {
    transform: translateY(-1px);
    box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
  }

  .btn:disabled {
    opacity: 0.5;
    cursor: not-allowed;
  }

  .btn-sm {
    padding: 0.375rem 0.75rem;
    font-size: 12px;
  }

  .btn-success {
    background: #10b981;
    color: white;
  }

  .btn-success:hover {
    background: #059669;
  }

  .btn-secondary {
    background: #6B7280;
    color: white;
  }

  .btn-secondary:hover {
    background: #4B5563;
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
    background: #fff;
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
    padding: 2rem;
    color: #6B7280;
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

  .modal {
    background: white;
    border-radius: 8px;
    box-shadow: 0 10px 40px rgba(0, 0, 0, 0.2);
    max-width: 700px;
    max-height: 90vh;
    overflow-y: auto;
  }

  .modal-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 1.5rem;
    border-bottom: 1px solid #E5E7EB;
    background: #f9fafb;
  }

  .modal-header h3 {
    margin: 0;
    font-size: 18px;
    color: #1F2937;
  }

  .modal-close {
    background: none;
    border: none;
    font-size: 20px;
    color: #6B7280;
    cursor: pointer;
    padding: 0;
  }

  .modal-close:hover {
    color: #1F2937;
  }

  .modal-body {
    padding: 2rem;
  }

  .tabla-detalles {
    width: 100%;
    border-collapse: collapse;
    margin-bottom: 1rem;
  }

  .tabla-detalles thead {
    background: #f3f4f6;
  }

  .tabla-detalles th,
  .tabla-detalles td {
    padding: 8px;
    text-align: left;
    font-size: 13px;
    border-bottom: 1px solid #E5E7EB;
  }

  .tabla-detalles th {
    font-weight: 600;
    color: #374151;
  }

  .tabla-detalles tbody tr:hover {
    background: #f9fafb;
  }
</style>
