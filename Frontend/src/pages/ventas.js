// Página de Ventas con Paginación Avanzada
import { ventaService } from '../services/ventaService.js'
import { PaginationAdvanced } from '../components/PaginationAdvanced.js'
import Swal from 'sweetalert2'

const ITEMS_PER_PAGE = 10

export class Ventas {
  constructor() {
    this.ventas = []
    this.filteredVentas = []
    this.searchTerm = ''
    this.currentPage = 1
    this.itemsPerPage = ITEMS_PER_PAGE
    this.pagination = null
    this.loading = false
    this.selectedVenta = null
  }

  render() {
    return `
<div class="ventas-page" style="padding: 20px;">
  <div class="page-header" style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 30px;">
    <h1 class="page-title" style="font-size: 28px; color: #1E293B; margin: 0;"><i class="fas fa-receipt"></i> Ventas/Facturas</h1>
    <button class="btn-add-venta" style="padding: 10px 20px; background: #3B82F6; color: white; border: none; border-radius: 8px; cursor: pointer; font-weight: 600;">
      <i class="fas fa-plus"></i> Nueva Venta
    </button>
  </div>

  <div class="card" style="background: white; border-radius: 12px; box-shadow: 0 2px 8px rgba(0,0,0,0.05); overflow: hidden;">
    <div class="card-header" style="padding: 20px; border-bottom: 1px solid #E2E8F0;">
      <input 
        type="text" 
        class="search-box" 
        placeholder="Buscar por número de factura o cliente..."
        style="width: 100%; padding: 10px 15px; border: 1px solid #E2E8F0; border-radius: 8px; font-size: 14px; box-sizing: border-box;"
      />
    </div>

    ${this.loading ? `
      <div style="padding: 40px; text-align: center; color: #64748B;">
        <i class="fas fa-spinner fa-spin" style="font-size: 24px; margin-bottom: 10px;"></i>
        <p>Cargando ventas...</p>
      </div>
    ` : `
      <div class="table-container" style="overflow-x: auto;">
        <table class="table" style="width: 100%; border-collapse: collapse;">
          <thead style="background: #F8FAFC; border-bottom: 2px solid #E2E8F0;">
            <tr>
              <th style="padding: 15px; text-align: left; font-weight: 600; color: #475569; font-size: 14px;">Número Factura</th>
              <th style="padding: 15px; text-align: left; font-weight: 600; color: #475569; font-size: 14px;">Cliente</th>
              <th style="padding: 15px; text-align: center; font-weight: 600; color: #475569; font-size: 14px;">Fecha</th>
              <th style="padding: 15px; text-align: right; font-weight: 600; color: #475569; font-size: 14px;">Total</th>
              <th style="padding: 15px; text-align: center; font-weight: 600; color: #475569; font-size: 14px;">Estado</th>
              <th style="padding: 15px; text-align: center; font-weight: 600; color: #475569; font-size: 14px;">Acciones</th>
            </tr>
          </thead>
          <tbody id="ventas-tbody">
            <tr><td colspan="6" style="padding: 40px; text-align: center; color: #64748B;">Cargando...</td></tr>
          </tbody>
        </table>
      </div>
      <div id="pagination-container"></div>
    `}
  </div>

  <!-- Modal de Detalles -->
  <div id="detalles-modal" style="display: none; position: fixed; top: 0; left: 0; right: 0; bottom: 0; background: rgba(0,0,0,0.5); z-index: 1000; align-items: center; justify-content: center;">
    <div style="background: white; border-radius: 12px; max-width: 700px; width: 90%; max-height: 80vh; overflow-y: auto; box-shadow: 0 20px 25px rgba(0,0,0,0.15);">
      <div style="padding: 24px; border-bottom: 1px solid #E2E8F0; display: flex; justify-content: space-between; align-items: center;">
        <h2 id="modal-title" style="margin: 0; color: #1E293B; font-size: 20px;"></h2>
        <button id="close-modal" style="background: none; border: none; font-size: 24px; cursor: pointer; color: #64748B;">×</button>
      </div>
      <div id="modal-content" style="padding: 24px;"></div>
      <div style="padding: 16px 24px; border-top: 1px solid #E2E8F0; display: flex; gap: 10px; justify-content: flex-end;">
        <button id="modal-close-btn" style="padding: 8px 16px; background: #E2E8F0; color: #1E293B; border: none; border-radius: 6px; cursor: pointer; font-weight: 600;">Cerrar</button>
        <button id="modal-pdf-btn" style="padding: 8px 16px; background: #EC4899; color: white; border: none; border-radius: 6px; cursor: pointer; font-weight: 600;"><i class="fas fa-file-pdf" style="margin-right: 5px;"></i>Descargar PDF</button>
      </div>
    </div>
  </div>
</div>
    `
  }

  init() {
    console.log('[VENTAS] Inicializando...')
    this.loadVentas()
    
    setTimeout(() => {
      const searchBox = document.querySelector('.search-box')
      if (searchBox) {
        searchBox.addEventListener('input', (e) => {
          this.searchTerm = e.target.value
          this.currentPage = 1
          this.filterVentas()
          this.updateTableAndPagination()
        })
      }

      const btnAdd = document.querySelector('.btn-add-venta')
      if (btnAdd) {
        btnAdd.addEventListener('click', () => {
          if (window.router) {
            window.router.navigateTo('nuevaventa')
          }
        })
      }

      // Event listeners para Modal
      const closeBtn = document.getElementById('close-modal')
      const closeBtnFooter = document.getElementById('modal-close-btn')
      const modal = document.getElementById('detalles-modal')
      
      if (closeBtn) {
        closeBtn.addEventListener('click', () => this.closeDetailsModal())
      }
      if (closeBtnFooter) {
        closeBtnFooter.addEventListener('click', () => this.closeDetailsModal())
      }
      if (modal) {
        modal.addEventListener('click', (e) => {
          if (e.target === modal) this.closeDetailsModal()
        })
      }

      // Event listeners para botones de Acciones
      this.attachButtonListeners()
    }, 100)
  }

  attachButtonListeners() {
    // Botones Ver
    document.querySelectorAll('.btn-view-venta').forEach(btn => {
      btn.addEventListener('click', async (e) => {
        e.preventDefault()
        const numeroFactura = btn.getAttribute('data-number')
        console.log('[VENTAS] Botón Ver clickeado, numeroFactura:', numeroFactura)
        if (numeroFactura && numeroFactura !== 'undefined' && numeroFactura !== 'N/A') {
          await this.showVentaDetails(numeroFactura)
        } else {
          Swal.fire({ icon: 'error', title: 'Error', text: 'Número de factura no disponible' })
        }
      })
    })

    // Botones Editar
    document.querySelectorAll('.btn-edit-venta').forEach(btn => {
      btn.addEventListener('click', async (e) => {
        e.preventDefault()
        const ventaId = btn.getAttribute('data-id')
        console.log('[VENTAS] Botón Editar clickeado, ventaId:', ventaId)
        if (ventaId && ventaId !== 'undefined' && ventaId !== 'N/A') {
          await this.showEditModal(ventaId)
        } else {
          Swal.fire({ icon: 'error', title: 'Error', text: 'ID de venta no disponible' })
        }
      })
    })

    // Botones Eliminar
    document.querySelectorAll('.btn-delete-venta').forEach(btn => {
      btn.addEventListener('click', async (e) => {
        e.preventDefault()
        const ventaId = btn.getAttribute('data-id')
        console.log('[VENTAS] Botón Eliminar clickeado, ventaId:', ventaId)
        if (ventaId && ventaId !== 'undefined' && ventaId !== 'N/A') {
          await this.deleteVenta(ventaId)
        } else {
          Swal.fire({ icon: 'error', title: 'Error', text: 'ID de venta no disponible' })
        }
      })
    })

    // Botones PDF
    document.querySelectorAll('.btn-pdf-venta').forEach(btn => {
      btn.addEventListener('click', async (e) => {
        e.preventDefault()
        const ventaId = btn.getAttribute('data-id')
        const numeroFactura = btn.getAttribute('data-number')
        console.log('[VENTAS] Botón PDF clickeado, ventaId:', ventaId, 'numeroFactura:', numeroFactura)
        if (ventaId && ventaId !== 'undefined' && ventaId !== 'N/A') {
          await this.downloadPDF(ventaId, numeroFactura)
        } else {
          Swal.fire({ icon: 'error', title: 'Error', text: 'ID de venta no disponible' })
        }
      })
    })
  }

  async showVentaDetails(numeroFactura) {
    try {
      console.log('[VENTAS] Obteniendo detalles de venta:', numeroFactura)
      const venta = await ventaService.getByNumeroFactura(numeroFactura)
      
      if (!venta) {
        Swal.fire({ icon: 'error', title: 'Error', text: 'No se pudo cargar los detalles de la venta' })
        return
      }

      this.selectedVenta = venta
      this.showDetailsModal(venta)
    } catch (error) {
      console.error('[VENTAS] Error al obtener detalles:', error)
      Swal.fire({ icon: 'error', title: 'Error', text: 'Error al cargar los detalles' })
    }
  }

  showDetailsModal(venta) {
    const modal = document.getElementById('detalles-modal')
    const title = document.getElementById('modal-title')
    const content = document.getElementById('modal-content')
    const pdfBtn = document.getElementById('modal-pdf-btn')

    title.textContent = `Factura: ${venta.numeroFactura || venta.number || 'N/A'}`

    // Formatear fechas
    const fecha = venta.fechaVenta || venta.fecha
    const fechaFormato = fecha ? new Date(fecha).toLocaleDateString('es-EC', { 
      year: 'numeric', 
      month: 'long', 
      day: 'numeric',
      hour: '2-digit',
      minute: '2-digit'
    }) : 'N/A'

    // Generar tabla de detalles
    const detallesHTML = venta.detalles && venta.detalles.length > 0 
      ? `<table style="width: 100%; border-collapse: collapse; margin: 16px 0;">
          <thead style="background: #F8FAFC; border-bottom: 2px solid #E2E8F0;">
            <tr>
              <th style="padding: 10px; text-align: left; font-weight: 600;">Producto</th>
              <th style="padding: 10px; text-align: center; font-weight: 600;">Cantidad</th>
              <th style="padding: 10px; text-align: right; font-weight: 600;">Precio Unit.</th>
              <th style="padding: 10px; text-align: center; font-weight: 600;">Descuento</th>
              <th style="padding: 10px; text-align: right; font-weight: 600;">Total</th>
            </tr>
          </thead>
          <tbody>
            ${venta.detalles.map(d => `
              <tr style="border-bottom: 1px solid #E2E8F0;">
                <td style="padding: 10px;">${d.productoNombre || d.productName || 'N/A'}</td>
                <td style="padding: 10px; text-align: center;">${d.cantidad || d.quantity || 0}</td>
                <td style="padding: 10px; text-align: right;">$${(d.precioUnitario || d.unitPrice || 0).toFixed(2)}</td>
                <td style="padding: 10px; text-align: center;">${d.descuento || d.discount || 0}%</td>
                <td style="padding: 10px; text-align: right; font-weight: 600;">$${(d.total || 0).toFixed(2)}</td>
              </tr>
            `).join('')}
          </tbody>
        </table>`
      : '<p style="color: #64748B;">Sin detalles de productos</p>'

    content.innerHTML = `
      <div style="margin-bottom: 20px;">
        <p style="margin: 8px 0;"><strong>Fecha:</strong> ${fechaFormato}</p>
        <p style="margin: 8px 0;"><strong>Cliente:</strong> ${venta.clienteNombre || venta.clientName || 'Consumidor Final'}</p>
        <p style="margin: 8px 0;"><strong>Usuario:</strong> ${venta.usuarioNombre || venta.userName || 'N/A'}</p>
      </div>

      <h3 style="color: #1E293B; margin: 16px 0 8px 0;">Productos:</h3>
      ${detallesHTML}

      <div style="background: #F8FAFC; padding: 16px; border-radius: 8px; margin-top: 20px;">
        <div style="display: flex; justify-content: space-between; margin: 8px 0;">
          <span>Subtotal:</span>
          <span>$${(venta.subtotal || 0).toFixed(2)}</span>
        </div>
        <div style="display: flex; justify-content: space-between; margin: 8px 0;">
          <span>IVA (${venta.porcentajeIVA || 0}%):</span>
          <span>$${(venta.totalImpuesto || 0).toFixed(2)}</span>
        </div>
        <div style="display: flex; justify-content: space-between; margin: 12px 0; font-size: 18px; font-weight: 700; color: #1E293B; border-top: 2px solid #E2E8F0; padding-top: 12px;">
          <span>TOTAL:</span>
          <span>$${(venta.totalVenta || venta.total || 0).toFixed(2)}</span>
        </div>
      </div>

      ${venta.observaciones ? `
        <div style="margin-top: 16px; background: #FFF8DC; padding: 12px; border-radius: 8px; border-left: 4px solid #F59E0B;">
          <p style="margin: 0; color: #92400E;"><strong>Observaciones:</strong></p>
          <p style="margin: 8px 0 0 0; color: #92400E;">${venta.observaciones}</p>
        </div>
      ` : ''}
    `

    if (pdfBtn) {
      const ventaId = venta.ventaId || venta.id || venta.VentaId
      const numeroFactura = venta.numeroFactura || venta.number || 'Factura'
      pdfBtn.onclick = () => this.downloadPDF(ventaId, numeroFactura)
    }

    modal.style.display = 'flex'
  }

  closeDetailsModal() {
    const modal = document.getElementById('detalles-modal')
    if (modal) {
      modal.style.display = 'none'
    }
    this.selectedVenta = null
  }

  async downloadPDF(ventaId, numeroFactura = 'Factura') {
    try {
      console.log('[VENTAS] Descargando PDF - ventaId:', ventaId, 'numeroFactura:', numeroFactura)
      Swal.fire({ title: 'Descargando...', allowOutsideClick: false, didOpen: () => Swal.showLoading() })
      
      const success = await ventaService.downloadPDFById(ventaId, numeroFactura)
      
      Swal.close()
      if (success) {
        Swal.fire({ icon: 'success', title: 'Éxito', text: 'PDF descargado correctamente' })
      } else {
        Swal.fire({ icon: 'error', title: 'Error', text: 'No se pudo descargar el PDF' })
      }
    } catch (error) {
      console.error('[VENTAS] Error descargando PDF:', error)
      Swal.close()
      Swal.fire({ icon: 'error', title: 'Error', text: 'Error al descargar el PDF' })
    }
  }

  async loadVentas() {
    this.loading = true
    try {
      console.log('[VENTAS] Cargando desde backend...')
      const ventas = await ventaService.getAll()
      if (Array.isArray(ventas)) {
        // Filtrar SOLO facturas que NO están eliminadas
        const ventasActivas = ventas.filter(v => v.estado !== 'Eliminada')
        
        // Ordenar de más reciente a más viejo
        this.ventas = ventasActivas.sort((a, b) => {
          // Si tienen fechaVenta, usar eso
          if (a.fechaVenta && b.fechaVenta) {
            return new Date(b.fechaVenta) - new Date(a.fechaVenta)
          }
          // Si no, usar el ID (asumiendo que IDs más altos = más recientes)
          return (b.id || 0) - (a.id || 0)
        })
      } else {
        this.ventas = []
      }
      console.log('[VENTAS] Cargadas:', this.ventas.length)
      this.filterVentas()
      this.setupPagination()
      this.updateTableAndPagination()
    } catch (error) {
      console.error('[VENTAS] Error cargando:', error)
      this.ventas = []
    } finally {
      this.loading = false
    }
  }

  filterVentas() {
    if (!this.searchTerm.trim()) {
      this.filteredVentas = [...this.ventas]
    } else {
      const term = this.searchTerm.toLowerCase()
      this.filteredVentas = this.ventas.filter(v =>
        v.numeroFactura?.includes(term) ||
        v.clienteNombre?.toLowerCase().includes(term)
      )
    }
  }

  getPaginatedVentas() {
    const start = (this.currentPage - 1) * this.itemsPerPage
    const end = start + this.itemsPerPage
    return this.filteredVentas.slice(start, end)
  }

  setupPagination() {
    this.pagination = new PaginationAdvanced({
      currentPage: 1,
      totalPages: Math.ceil(this.filteredVentas.length / this.itemsPerPage) || 1,
      totalItems: this.filteredVentas.length,
      itemsPerPage: this.itemsPerPage,
      onChange: (page) => {
        this.currentPage = page
        this.updateTableAndPagination()
      }
    })
    
    window.pagination = this.pagination
  }

  renderPagination() {
    const container = document.getElementById('pagination-container')
    if (container && this.pagination) {
      this.pagination.update(this.filteredVentas.length)
      container.innerHTML = this.pagination.render()
    }
  }

  updateTableAndPagination() {
    this.renderTable()
    this.renderPagination()
    // Reasignar event listeners después de renderizar
    setTimeout(() => this.attachButtonListeners(), 50)
  }

  renderTable() {
    const tbody = document.getElementById('ventas-tbody')
    if (!tbody) return

    const paginatedVentas = this.getPaginatedVentas()
    
    if (paginatedVentas.length === 0) {
      tbody.innerHTML = `<tr><td colspan="6" style="padding: 40px; text-align: center; color: #64748B;">${this.ventas.length === 0 ? 'No hay ventas registradas' : 'No se encontraron resultados'}</td></tr>`
      return
    }

    tbody.innerHTML = paginatedVentas.map(venta => {
      // Obtener datos con múltiples opciones de nombres
      const ventaId = venta.ventaId || venta.id || venta.VentaId || 'N/A'
      const numeroFactura = venta.numeroFactura || venta.number || venta.NumeroFactura || 'N/A'
      const fecha = venta.fechaVenta || venta.fecha || venta.FechaVenta
      const total = venta.totalVenta || venta.total || venta.montoTotal || venta.TotalVenta || 0
      const clienteNombre = venta.clienteNombre || venta.cliente || venta.ClienteNombre || 'Consumidor Final'
      
      console.log('[RENDERIZADO] Venta:', { ventaId, numeroFactura, clienteNombre, total })
      
      return `
      <tr style="border-bottom: 1px solid #E2E8F0;">
        <td style="padding: 15px; color: #1E293B; font-weight: 600;">${numeroFactura}</td>
        <td style="padding: 15px; color: #1E293B;">${clienteNombre}</td>
        <td style="padding: 15px; text-align: center; color: #64748B; font-size: 14px;">${fecha ? new Date(fecha).toLocaleDateString('es-EC') : 'N/A'}</td>
        <td style="padding: 15px; text-align: right; color: #1E293B; font-weight: 600;">$${total.toFixed(2)}</td>
        <td style="padding: 15px; text-align: center;">
          <span style="display: inline-block; padding: 4px 12px; border-radius: 20px; font-size: 12px; font-weight: 600; background: #D1FAE5; color: #065F46;">
            Completada
          </span>
        </td>
        <td style="padding: 15px; text-align: center;">
          <button class="btn-view-venta" data-number="${numeroFactura}" title="Ver detalles de la venta" style="padding: 6px 12px; background: #3B82F6; color: white; border: none; border-radius: 6px; cursor: pointer; margin-right: 5px; font-size: 12px; min-width: 90px; display: inline-flex; align-items: center; justify-content: center;">
            <i class="fas fa-eye" style="margin-right: 5px;"></i> Ver
          </button>
          <button class="btn-edit-venta" data-id="${ventaId}" title="Editar cantidad" style="padding: 6px 12px; background: #F59E0B; color: white; border: none; border-radius: 6px; cursor: pointer; margin-right: 5px; font-size: 12px; min-width: 90px; display: inline-flex; align-items: center; justify-content: center;">
            <i class="fas fa-edit" style="margin-right: 5px;"></i> Editar
          </button>
          <button class="btn-delete-venta" data-id="${ventaId}" title="Eliminar factura" style="padding: 6px 12px; background: #EF4444; color: white; border: none; border-radius: 6px; cursor: pointer; margin-right: 5px; font-size: 12px; min-width: 90px; display: inline-flex; align-items: center; justify-content: center;">
            <i class="fas fa-trash" style="margin-right: 5px;"></i> Eliminar
          </button>
          <button class="btn-pdf-venta" data-id="${ventaId}" data-number="${numeroFactura}" title="Descargar PDF" style="padding: 6px 12px; background: #EC4899; color: white; border: none; border-radius: 6px; cursor: pointer; font-size: 12px; min-width: 90px; display: inline-flex; align-items: center; justify-content: center;">
            <i class="fas fa-file-pdf" style="margin-right: 5px;"></i> PDF
          </button>
        </td>
      </tr>
    `}).join('')
  }

  async showEditModal(ventaId) {
    try {
      console.log('[VENTAS] Obteniendo venta para editar:', ventaId)
      const venta = await ventaService.getById(ventaId)
      
      if (!venta || !venta.detalles) {
        Swal.fire({ icon: 'error', title: 'Error', text: 'No se pudo cargar los detalles de la venta' })
        return
      }

      // Crear modal de edición
      const { value: editedDetalles, dismiss } = await Swal.fire({
        title: `Editar Factura: ${venta.numeroFactura}`,
        html: this.createEditForm(venta),
        didOpen: () => {
          // Agregar event listeners a los inputs
          const inputs = document.querySelectorAll('.edit-cantidad-input')
          inputs.forEach(input => {
            input.addEventListener('change', () => {
              this.recalculateTotals(venta)
            })
          })
        },
        confirmButtonText: 'Guardar',
        cancelButtonText: 'Cancelar',
        showCancelButton: true,
        preConfirm: () => {
          const detallesActualizados = []
          const inputs = document.querySelectorAll('.edit-cantidad-input')
          
          inputs.forEach((input, index) => {
            const nuevaCantidad = parseInt(input.value)
            const detalleOriginal = venta.detalles[index]
            
            if (nuevaCantidad !== detalleOriginal.cantidad) {
              detallesActualizados.push({
                detalleId: detalleOriginal.id || detalleOriginal.ventaId,
                nuevaCantidad: nuevaCantidad
              })
            }
          })

          if (detallesActualizados.length === 0) {
            Swal.fire({ icon: 'info', title: 'Sin cambios', text: 'No se realizaron cambios en las cantidades' })
            return false
          }

          return detallesActualizados
        }
      })

      if (editedDetalles && editedDetalles !== false) {
        await this.editVenta(ventaId, editedDetalles)
      }
    } catch (error) {
      console.error('[VENTAS] Error al editar:', error)
      Swal.fire({ icon: 'error', title: 'Error', text: 'Error al cargar la venta para editar' })
    }
  }

  createEditForm(venta) {
    const detallesHTML = venta.detalles.map((d, idx) => `
      <div style="margin: 12px 0; padding: 12px; background: #F8FAFC; border-radius: 6px; border-left: 4px solid #3B82F6;">
        <div style="margin-bottom: 8px; font-weight: 600; color: #1E293B;">
          ${d.productoNombre || d.productName || 'N/A'}
        </div>
        <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 10px; font-size: 14px;">
          <div>
            <label style="display: block; font-size: 12px; color: #64748B; margin-bottom: 4px;">Cantidad:</label>
            <input 
              type="number" 
              class="edit-cantidad-input" 
              value="${d.cantidad || d.quantity || 0}" 
              min="1" 
              style="width: 100%; padding: 6px; border: 1px solid #E2E8F0; border-radius: 4px;"
            />
          </div>
          <div>
            <label style="display: block; font-size: 12px; color: #64748B; margin-bottom: 4px;">Precio Unit.:</label>
            <input 
              type="number" 
              value="${d.precioUnitario || d.unitPrice || 0}" 
              disabled
              style="width: 100%; padding: 6px; border: 1px solid #E2E8F0; border-radius: 4px; background: #E2E8F0; color: #64748B; cursor: not-allowed;"
            />
          </div>
        </div>
        <div style="margin-top: 8px; padding-top: 8px; border-top: 1px solid #E2E8F0; display: flex; justify-content: space-between; font-size: 13px;">
          <span>Total: </span>
          <span style="font-weight: 600; color: #1E293B;">$${(d.total || 0).toFixed(2)}</span>
        </div>
      </div>
    `).join('')

    return `
      <div style="max-height: 400px; overflow-y: auto; text-align: left;">
        <div style="background: #FEF3C7; padding: 12px; border-radius: 6px; margin-bottom: 16px; border-left: 4px solid #F59E0B;">
          <p style="margin: 0; color: #92400E; font-size: 13px;">
            <i class="fas fa-info-circle" style="margin-right: 6px;"></i>
            Solo puedes modificar las cantidades. Los otros campos están bloqueados.
          </p>
        </div>
        ${detallesHTML}
      </div>
    `
  }

  recalculateTotals(venta) {
    // Esta función se puede usar para recalcular totales en tiempo real
    // Por ahora es un placeholder
  }

  async editVenta(ventaId, detalles) {
    try {
      Swal.fire({ title: 'Guardando...', allowOutsideClick: false, didOpen: () => Swal.showLoading() })
      
      const result = await ventaService.updateCantidad(ventaId, detalles)
      
      Swal.close()
      
      if (result) {
        Swal.fire({ icon: 'success', title: 'Éxito', text: 'Factura actualizada correctamente' })
        this.loadVentas()
      } else {
        Swal.fire({ icon: 'error', title: 'Error', text: 'No se pudo actualizar la factura' })
      }
    } catch (error) {
      console.error('[VENTAS] Error al guardar:', error)
      Swal.close()
      Swal.fire({ icon: 'error', title: 'Error', text: error.message || 'Error al actualizar la factura' })
    }
  }

  async deleteVenta(ventaId) {
    try {
      const { isConfirmed } = await Swal.fire({
        icon: 'warning',
        title: '¿Eliminar factura?',
        text: 'Esta factura se marcará como eliminada y el stock se restaurará. ¿Deseas continuar?',
        confirmButtonText: 'Sí, eliminar',
        cancelButtonText: 'Cancelar',
        showCancelButton: true,
        confirmButtonColor: '#EF4444'
      })

      if (!isConfirmed) return

      Swal.fire({ title: 'Eliminando...', allowOutsideClick: false, didOpen: () => Swal.showLoading() })
      
      const result = await ventaService.deleteSoft(ventaId)
      
      Swal.close()
      
      if (result) {
        Swal.fire({ icon: 'success', title: 'Éxito', text: 'Factura eliminada correctamente' })
        this.loadVentas()
      } else {
        Swal.fire({ icon: 'error', title: 'Error', text: 'No se pudo eliminar la factura' })
      }
    } catch (error) {
      console.error('[VENTAS] Error al eliminar:', error)
      Swal.close()
      Swal.fire({ icon: 'error', title: 'Error', text: error.message || 'Error al eliminar la factura' })
    }
  }
}
