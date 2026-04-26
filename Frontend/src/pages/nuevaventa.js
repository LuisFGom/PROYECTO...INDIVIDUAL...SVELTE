import { ventaService } from '../services/ventaService.js'
import { clienteService } from '../services/clienteService.js'
import { productoService } from '../services/productoService.js'
import { showErrorAlert, showSuccessAlert } from '../utils/errorHandler.js'
import Swal from 'sweetalert2'

const IVA_PERCENTAGE = 0.19

export class NuevaVenta {
  constructor() {
    this.clientes = []
    this.productos = []
    this.detalles = []
    this.clienteSeleccionado = null
    this.filteredClientes = []
    this.filteredProductos = []
  }

  render() {
    return `
      <div style="padding: 2rem;">
        <div class="page-header" style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 2rem;">
          <h1 class="page-title" style="font-size: 28px; color: #1E293B; margin: 0;">
            <i class="fas fa-file-invoice"></i> Nueva Venta/Factura
          </h1>
        </div>

        <div class="card" style="background: white; border-radius: 12px; box-shadow: 0 2px 8px rgba(0,0,0,0.05); overflow: hidden;">
          <div style="padding: 20px; border-bottom: 1px solid #E2E8F0;">
            <h2 style="margin: 0; color: #1E293B; font-size: 20px;">Crear Nueva Venta</h2>
          </div>
          
          <form id="ventaForm" style="display: grid; gap: 1.5rem; padding: 2rem;">
            
            <!-- Datos del Cliente -->
            <div style="border: 1px solid #E2E8F0; border-radius: 8px; padding: 1.5rem;">
              <h3 style="margin: 0 0 1rem 0; font-size: 1.125rem; color: #1E293B;">Datos del Cliente <span style="color: red;">*</span></h3>
              <div style="display: grid; grid-template-columns: 2fr 1fr; gap: 1rem;">
                <div style="position: relative;">
                  <label style="display: block; margin-bottom: 0.5rem; font-weight: 600; color: #0F172A;">Seleccionar Cliente</label>
                  <input type="text" id="clienteInput" placeholder="Buscar cliente por nombre o cédula..." 
                         style="width: 100%; padding: 0.75rem; border: 1px solid #E2E8F0; border-radius: 6px; box-sizing: border-box; font-size: 14px;" autocomplete="off">
                  <div id="clientesDropdown" style="display: none; position: absolute; top: 100%; left: 0; right: 0; background: white; border: 1px solid #E2E8F0; border-top: none; border-radius: 0 0 6px 6px; max-height: 200px; overflow-y: auto; z-index: 100;">
                  </div>
                </div>
                <div>
                  <label style="display: block; margin-bottom: 0.5rem; font-weight: 600; color: #0F172A;">Fecha</label>
                  <input type="date" id="fechaVenta" 
                         style="width: 100%; padding: 0.75rem; border: 1px solid #E2E8F0; border-radius: 6px; box-sizing: border-box;">
                </div>
              </div>
              <div id="clienteSeleccionado" style="margin-top: 1rem; padding: 1rem; background: #F0F9FF; border-radius: 6px; border-left: 4px solid #3B82F6; display: none;">
                <p style="margin: 0; color: #1E293B; font-weight: 600;"><strong>Seleccionado:</strong> <span id="clienteNombre"></span></p>
              </div>
            </div>

            <!-- Agregar Productos -->
            <div style="border: 1px solid #E2E8F0; border-radius: 8px; padding: 1.5rem;">
              <h3 style="margin: 0 0 1rem 0; font-size: 1.125rem; color: #1E293B;">Agregar Productos <span style="color: red;">*</span></h3>
              <div style="display: grid; grid-template-columns: 2fr 1fr 1fr auto; gap: 0.75rem; margin-bottom: 1rem;">
                <div style="position: relative;">
                  <input type="text" id="productoInput" placeholder="Buscar producto..." 
                         style="width: 100%; padding: 0.75rem; border: 1px solid #E2E8F0; border-radius: 6px; font-size: 14px; box-sizing: border-box;" autocomplete="off">
                  <div id="productosDropdown" style="display: none; position: absolute; top: 100%; left: 0; right: 0; background: white; border: 1px solid #E2E8F0; border-top: none; border-radius: 0 0 6px 6px; max-height: 200px; overflow-y: auto; z-index: 100;">
                  </div>
                </div>
                <div>
                  <input type="number" id="cantidadInput" placeholder="Cantidad" min="1" value="1"
                         style="width: 100%; padding: 0.75rem; border: 1px solid #E2E8F0; border-radius: 6px; font-size: 14px; box-sizing: border-box;">
                </div>
                <div>
                  <input type="number" id="precioInput" placeholder="Precio" min="0" step="0.01" value="0"
                         style="width: 100%; padding: 0.75rem; border: 1px solid #E2E8F0; border-radius: 6px; font-size: 14px; box-sizing: border-box;">
                </div>
                <button type="button" id="addProductBtn" style="padding: 0.75rem 1.5rem; background: #3B82F6; color: white; border: none; border-radius: 6px; cursor: pointer; font-weight: 600;">
                  <i class="fas fa-plus"></i>
                </button>
              </div>
            </div>

            <!-- Detalle de Productos -->
            <div style="border: 1px solid #E2E8F0; border-radius: 8px; padding: 1.5rem; overflow-x: auto;">
              <h3 style="margin: 0 0 1rem 0; font-size: 1.125rem; color: #1E293B;">Detalle de Productos</h3>
              <table style="width: 100%; border-collapse: collapse;">
                <thead>
                  <tr style="background-color: #F8FAFC; border-bottom: 2px solid #E2E8F0;">
                    <th style="padding: 0.75rem; text-align: left; font-weight: 600; color: #475569;">Producto</th>
                    <th style="padding: 0.75rem; text-align: center; font-weight: 600; color: #475569;">Cantidad</th>
                    <th style="padding: 0.75rem; text-align: right; font-weight: 600; color: #475569;">Precio Unit.</th>
                    <th style="padding: 0.75rem; text-align: right; font-weight: 600; color: #475569;">Subtotal</th>
                    <th style="padding: 0.75rem; text-align: center; font-weight: 600; color: #475569;">Acción</th>
                  </tr>
                </thead>
                <tbody id="productosTable">
                  <tr>
                    <td colspan="5" style="padding: 1rem; text-align: center; color: #64748B;">
                      Sin productos agregados
                    </td>
                  </tr>
                </tbody>
              </table>
            </div>

            <!-- Totales -->
            <div style="background-color: #F8FAFC; border-radius: 8px; padding: 1.5rem; display: grid; grid-template-columns: repeat(3, 1fr); gap: 1rem;">
              <div style="text-align: right;">
                <p style="color: #64748B; margin: 0 0 0.25rem 0; font-size: 14px;">Subtotal:</p>
                <p style="font-size: 1.5rem; font-weight: 700; margin: 0; color: #1E293B;">$<span id="subtotal">0.00</span></p>
              </div>
              <div style="text-align: right;">
                <p style="color: #64748B; margin: 0 0 0.25rem 0; font-size: 14px;">IVA (19%):</p>
                <p style="font-size: 1.5rem; font-weight: 700; margin: 0; color: #1E293B;">$<span id="iva">0.00</span></p>
              </div>
              <div style="text-align: right;">
                <p style="color: #64748B; margin: 0 0 0.25rem 0; font-size: 14px;">Total:</p>
                <p style="font-size: 1.5rem; font-weight: 700; margin: 0; color: #10B981;">$<span id="total">0.00</span></p>
              </div>
            </div>

            <!-- Observaciones -->
            <div>
              <label style="display: block; margin-bottom: 0.5rem; font-weight: 600; color: #0F172A;">Observaciones</label>
              <textarea id="observaciones" placeholder="Notas adicionales..." rows="3"
                        style="width: 100%; padding: 0.75rem; border: 1px solid #E2E8F0; border-radius: 6px; box-sizing: border-box; font-family: inherit; resize: vertical;"></textarea>
            </div>

            <!-- Botones -->
            <div style="display: flex; gap: 1rem; justify-content: flex-end;">
              <button type="button" id="cancelBtn" style="padding: 0.75rem 1.5rem; background: #E2E8F0; color: #1E293B; border: none; border-radius: 6px; cursor: pointer; font-weight: 600;">
                Cancelar
              </button>
              <button type="submit" style="padding: 0.75rem 1.5rem; background: #10B981; color: white; border: none; border-radius: 6px; cursor: pointer; font-weight: 600;">
                <i class="fas fa-save"></i> Guardar Venta
              </button>
            </div>
          </form>
        </div>
      </div>
    `
  }

  async loadClientes() {
    try {
      this.clientes = await clienteService.getAll()
      console.log('[NuevaVenta] Clientes cargados:', this.clientes.length)
    } catch (error) {
      console.error('[NuevaVenta] Error cargando clientes:', error)
      this.clientes = []
    }
  }

  async loadProductos() {
    try {
      this.productos = await productoService.getAll()
      console.log('[NuevaVenta] Productos cargados:', this.productos.length)
    } catch (error) {
      console.error('[NuevaVenta] Error cargando productos:', error)
      this.productos = []
    }
  }

  filterClientes(term) {
    if (!term.trim()) {
      this.filteredClientes = []
      return
    }
    
    const search = term.toLowerCase()
    this.filteredClientes = this.clientes.filter(c =>
      c.nombre?.toLowerCase().includes(search) ||
      c.apellido?.toLowerCase().includes(search) ||
      c.documento?.includes(term) ||
      c.cedula?.includes(term)
    ).slice(0, 10) // Máximo 10 resultados
  }

  filterProductos(term) {
    if (!term.trim()) {
      this.filteredProductos = []
      return
    }

    const search = term.toLowerCase()
    this.filteredProductos = this.productos.filter(p =>
      p.nombre?.toLowerCase().includes(search) ||
      p.codigoBarra?.includes(term)
    ).slice(0, 10)
  }

  renderClientesDropdown() {
    const dropdown = document.getElementById('clientesDropdown')
    if (!dropdown) return

    if (this.filteredClientes.length === 0) {
      dropdown.innerHTML = '<div style="padding: 0.75rem; color: #64748B; text-align: center;">No se encontraron clientes</div>'
      dropdown.style.display = 'block'
      return
    }

    dropdown.innerHTML = this.filteredClientes.map(c => `
      <div data-cliente-id="${c.id}" style="padding: 0.75rem 1rem; border-bottom: 1px solid #F1F5F9; cursor: pointer; hover:background: #F1F5F9;">
        <strong>${c.nombre} ${c.apellido || ''}</strong><br>
        <small style="color: #64748B;">${c.documento || c.cedula || ''}</small>
      </div>
    `).join('')
    
    dropdown.style.display = 'block'
    
    dropdown.querySelectorAll('div[data-cliente-id]').forEach(item => {
      item.addEventListener('click', () => {
        const clienteId = item.getAttribute('data-cliente-id')
        const cliente = this.clientes.find(c => c.id == clienteId)
        if (cliente) {
          this.clienteSeleccionado = cliente
          document.getElementById('clienteInput').value = `${cliente.nombre} ${cliente.apellido || ''}`
          document.getElementById('clienteNombre').textContent = `${cliente.nombre} ${cliente.apellido || ''} - ${cliente.documento || cliente.cedula || ''}`
          document.getElementById('clienteSeleccionado').style.display = 'block'
          dropdown.style.display = 'none'
        }
      })
    })
  }

  renderProductosDropdown() {
    const dropdown = document.getElementById('productosDropdown')
    if (!dropdown) return

    if (this.filteredProductos.length === 0) {
      dropdown.innerHTML = '<div style="padding: 0.75rem; color: #64748B; text-align: center;">No se encontraron productos</div>'
      dropdown.style.display = 'block'
      return
    }

    dropdown.innerHTML = this.filteredProductos.map(p => `
      <div data-producto-id="${p.id}" style="padding: 0.75rem 1rem; border-bottom: 1px solid #F1F5F9; cursor: pointer;">
        <strong>${p.nombre}</strong><br>
        <small style="color: #64748B;">Stock: ${p.stockActual || 0} | Precio: $${parseFloat(p.precioVenta || 0).toFixed(2)}</small>
      </div>
    `).join('')

    dropdown.style.display = 'block'

    dropdown.querySelectorAll('div[data-producto-id]').forEach(item => {
      item.addEventListener('click', () => {
        const productoId = item.getAttribute('data-producto-id')
        const producto = this.productos.find(p => p.id == productoId)
        if (producto) {
          document.getElementById('productoInput').value = producto.nombre
          document.getElementById('precioInput').value = parseFloat(producto.precioVenta || 0).toFixed(2)
          item.closest('[id="productosDropdown"]').style.display = 'none'
        }
      })
    })
  }

  addDetalle() {
    const productoInput = document.getElementById('productoInput').value.trim()
    const cantidad = parseInt(document.getElementById('cantidadInput').value) || 1
    const precio = parseFloat(document.getElementById('precioInput').value) || 0

    // Validaciones
    if (!productoInput) {
      showErrorAlert(new Error('Debe seleccionar un producto'), 'Error', 'Por favor busque y seleccione un producto')
      return
    }

    if (cantidad <= 0) {
      showErrorAlert(new Error('Cantidad inválida'), 'Error', 'La cantidad debe ser mayor a 0')
      return
    }

    if (precio <= 0) {
      showErrorAlert(new Error('Precio inválido'), 'Error', 'El precio debe ser mayor a 0')
      return
    }

    // Buscar producto
    const producto = this.filteredProductos.find(p => p.nombre === productoInput)
    if (!producto) {
      showErrorAlert(new Error('Producto no encontrado'), 'Error', 'No se encontró el producto')
      return
    }

    // Verificar si el producto ya existe en los detalles
    const detalleExistente = this.detalles.find(d => d.productoId === producto.id)
    
    if (detalleExistente) {
      // Si existe, incrementar la cantidad
      detalleExistente.cantidad += cantidad
      detalleExistente.subtotal = detalleExistente.cantidad * detalleExistente.precioUnitario
    } else {
      // Si no existe, agregarlo como nuevo detalle
      const detalle = {
        productoId: producto.id,
        productoNombre: producto.nombre,
        cantidad: cantidad,
        precioUnitario: precio,
        subtotal: cantidad * precio
      }
      this.detalles.push(detalle)
    }

    this.updateDetallesTable()
    this.calcularTotales()

    // Limpiar inputs
    document.getElementById('productoInput').value = ''
    document.getElementById('cantidadInput').value = '1'
    document.getElementById('precioInput').value = '0'
    document.getElementById('productosDropdown').style.display = 'none'
  }

  removeDetalle(index) {
    this.detalles.splice(index, 1)
    this.updateDetallesTable()
    this.calcularTotales()
  }

  async editDetalle(index) {
    const detalle = this.detalles[index]
    if (!detalle) return

    const { value: nuevaCantidad } = await Swal.fire({
      title: `Editar Cantidad - ${detalle.productoNombre}`,
      input: 'number',
      inputValue: detalle.cantidad,
      inputAttributes: {
        min: 1,
        step: 1,
        placeholder: 'Ingrese la nueva cantidad'
      },
      showCancelButton: true,
      confirmButtonText: 'Actualizar',
      cancelButtonText: 'Cancelar',
      confirmButtonColor: '#3B82F6',
      cancelButtonColor: '#94A3B8',
      inputValidator: (value) => {
        if (!value || parseInt(value) < 1) {
          return 'La cantidad debe ser mayor a 0'
        }
      }
    })

    if (nuevaCantidad) {
      detalle.cantidad = parseInt(nuevaCantidad)
      detalle.subtotal = detalle.cantidad * detalle.precioUnitario
      this.updateDetallesTable()
      this.calcularTotales()
    }
  }

  updateDetallesTable() {
    const tbody = document.getElementById('productosTable')
    if (this.detalles.length === 0) {
      tbody.innerHTML = '<tr><td colspan="5" style="padding: 1rem; text-align: center; color: #64748B;">Sin productos agregados</td></tr>'
      return
    }

    tbody.innerHTML = this.detalles.map((detalle, index) => `
      <tr style="border-bottom: 1px solid #E2E8F0;">
        <td style="padding: 0.75rem; color: #1E293B;">${detalle.productoNombre}</td>
        <td style="padding: 0.75rem; text-align: center; color: #1E293B;">${detalle.cantidad}</td>
        <td style="padding: 0.75rem; text-align: right; color: #1E293B;">$${detalle.precioUnitario.toFixed(2)}</td>
        <td style="padding: 0.75rem; text-align: right; color: #1E293B; font-weight: 600;">$${detalle.subtotal.toFixed(2)}</td>
        <td style="padding: 0.75rem; text-align: center;">
          <button type="button" class="btn-edit" data-index="${index}" style="background: #3B82F6; color: white; border: none; padding: 0.4rem 0.8rem; border-radius: 4px; cursor: pointer; font-size: 12px; margin-right: 0.5rem;">
            <i class="fas fa-edit"></i> Editar
          </button>
          <button type="button" class="btn-remove" data-index="${index}" style="background: #EF4444; color: white; border: none; padding: 0.4rem 0.8rem; border-radius: 4px; cursor: pointer; font-size: 12px;">
            <i class="fas fa-trash"></i> Quitar
          </button>
        </td>
      </tr>
    `).join('')

    // Agregar event listeners a botones de quitar
    tbody.querySelectorAll('.btn-remove').forEach(btn => {
      btn.addEventListener('click', (e) => {
        e.preventDefault()
        const index = parseInt(btn.getAttribute('data-index'))
        this.removeDetalle(index)
      })
    })

    // Agregar event listeners a botones de editar
    tbody.querySelectorAll('.btn-edit').forEach(btn => {
      btn.addEventListener('click', (e) => {
        e.preventDefault()
        const index = parseInt(btn.getAttribute('data-index'))
        this.editDetalle(index)
      })
    })
  }

  calcularTotales() {
    const subtotal = this.detalles.reduce((sum, d) => sum + d.subtotal, 0)
    const iva = subtotal * IVA_PERCENTAGE
    const total = subtotal + iva

    document.getElementById('subtotal').textContent = subtotal.toFixed(2)
    document.getElementById('iva').textContent = iva.toFixed(2)
    document.getElementById('total').textContent = total.toFixed(2)
  }

  async saveVenta() {
    try {
      // Validaciones
      if (!this.clienteSeleccionado) {
        const result = await Swal.fire({
          icon: 'warning',
          title: 'Cliente no seleccionado',
          html: '<p>No has seleccionado un cliente. ¿Deseas registrar un nuevo cliente?</p>',
          showCancelButton: true,
          confirmButtonText: 'Registrar nuevo cliente',
          cancelButtonText: 'Cancelar',
          confirmButtonColor: '#3B82F6',
          cancelButtonColor: '#94A3B8'
        })
        
        if (result.isConfirmed) {
          // Abrir modal de creación de cliente
          await this.openClienteModal()
        }
        return
      }

      if (this.detalles.length === 0) {
        showErrorAlert(new Error('Sin productos'), 'Error', 'Debe agregar al least un producto a la venta')
        return
      }

      // Crear DTO de venta
      const ventaData = {
        clienteId: this.clienteSeleccionado.id,
        detalles: this.detalles.map(d => ({
          productoId: d.productoId,
          cantidad: d.cantidad,
          precioUnitario: d.precioUnitario,
          descuento: 0
        })),
        observaciones: document.getElementById('observaciones').value.trim() || null
      }

      console.log('[NuevaVenta] Guardando venta:', ventaData)

      const result = await ventaService.create(ventaData)
      console.log('[NuevaVenta] Resultado de crear venta:', result)
      
      // Verificar si la venta se creó (result puede ser 0, un objeto, o truthy)
      if (result !== null && result !== undefined) {
        const confirmResult = await Swal.fire({
          icon: 'success',
          title: '¡Éxito!',
          text: `Venta creada${result.numeroFactura ? ': ' + result.numeroFactura : ''}`,
          confirmButtonText: 'Ir a Ventas',
          confirmButtonColor: '#10B981',
          allowOutsideClick: false,
          allowEscapeKey: false
        })
        
        // Redirigir después de que el usuario haga clic
        if (confirmResult.isConfirmed) {
          console.log('[NuevaVenta] Redirigiendo a ventas...')
          if (window.router) {
            window.router.navigateTo('ventas')
          } else {
            window.location.href = '#/ventas'
          }
        }
      } else {
        showErrorAlert(new Error('Error desconocido'), 'Error', 'No se pudo crear la venta')
      }
    } catch (error) {
      console.error('[NuevaVenta] Error completo guardando venta:', error)
      console.error('[NuevaVenta] error.message:', error?.message)
      console.error('[NuevaVenta] error.response:', error?.response)
      console.error('[NuevaVenta] error.response.data:', error?.response?.data)
      showErrorAlert(error, 'Error al guardar venta', 'No se pudo crear la venta')
    }
  }

  async openClienteModal() {
    const formHTML = `
      <form id="clienteFormModal" style="width: 100%;">
        <div style="margin-bottom: 15px;">
          <label style="display: block; margin-bottom: 5px; font-weight: 600; color: #0F172A;">Nombre: <span style="color:red">*</span></label>
          <input type="text" id="nuevoClienteNombre" placeholder="Nombre del cliente" required style="width: 100%; padding: 10px; border: 1px solid #E2E8F0; border-radius: 6px; box-sizing: border-box;"/>
        </div>
        <div style="margin-bottom: 15px;">
          <label style="display: block; margin-bottom: 5px; font-weight: 600; color: #0F172A;">Apellido: <span style="color:red">*</span></label>
          <input type="text" id="nuevoClienteApellido" placeholder="Apellido del cliente" required style="width: 100%; padding: 10px; border: 1px solid #E2E8F0; border-radius: 6px; box-sizing: border-box;"/>
        </div>
        <div style="margin-bottom: 15px;">
          <label style="display: block; margin-bottom: 5px; font-weight: 600; color: #0F172A;">Cédula: <span style="color:red">*</span></label>
          <input type="text" id="nuevoClienteCedula" placeholder="Cédula de identidad" required style="width: 100%; padding: 10px; border: 1px solid #E2E8F0; border-radius: 6px; box-sizing: border-box;"/>
          <div id="cedulaError" style="color: #EF4444; font-size: 13px; margin-top: 5px; display: none;"></div>
        </div>
        <div style="margin-bottom: 15px;">
          <label style="display: block; margin-bottom: 5px; font-weight: 600; color: #0F172A;">Email:</label>
          <input type="email" id="nuevoClienteEmail" placeholder="email@ejemplo.com" style="width: 100%; padding: 10px; border: 1px solid #E2E8F0; border-radius: 6px; box-sizing: border-box;"/>
          <div id="emailError" style="color: #EF4444; font-size: 13px; margin-top: 5px; display: none;"></div>
        </div>
        <div style="margin-bottom: 15px;">
          <label style="display: block; margin-bottom: 5px; font-weight: 600; color: #0F172A;">Teléfono:</label>
          <input type="text" id="nuevoClienteTelefono" placeholder="Número de teléfono" style="width: 100%; padding: 10px; border: 1px solid #E2E8F0; border-radius: 6px; box-sizing: border-box;"/>
        </div>
        <div style="display: flex; gap: 10px; justify-content: flex-end; margin-top: 25px;">
          <button type="button" id="btnCancelarCliente" style="padding: 10px 20px; background: #E2E8F0; color: #1E293B; border: none; border-radius: 6px; cursor: pointer; font-weight: 600;">Cancelar</button>
          <button type="submit" style="padding: 10px 20px; background: #3B82F6; color: white; border: none; border-radius: 6px; cursor: pointer; font-weight: 600;">Guardar Cliente</button>
        </div>
      </form>
    `

    const { value: confirmed } = await Swal.fire({
      title: 'Registrar Nuevo Cliente',
      html: formHTML,
      icon: 'info',
      didOpen: (modal) => {
        const form = document.getElementById('clienteFormModal')
        const btnCancel = document.getElementById('btnCancelarCliente')
        const cedulaErrorDiv = document.getElementById('cedulaError')
        const emailErrorDiv = document.getElementById('emailError')
        const emailInput = document.getElementById('nuevoClienteEmail')
        
        if (btnCancel) {
          btnCancel.addEventListener('click', () => {
            Swal.close()
          })
        }

        // Validación de email en tiempo real
        if (emailInput) {
          emailInput.addEventListener('blur', () => {
            const email = emailInput.value.trim()
            if (email && !this.isValidEmail(email)) {
              emailErrorDiv.textContent = 'Formato de email inválido'
              emailErrorDiv.style.display = 'block'
            } else {
              emailErrorDiv.style.display = 'none'
              emailErrorDiv.textContent = ''
            }
          })
        }

        if (form) {
          form.addEventListener('submit', async (e) => {
            e.preventDefault()
            
            const nombre = document.getElementById('nuevoClienteNombre').value.trim()
            const apellido = document.getElementById('nuevoClienteApellido').value.trim()
            const cedula = document.getElementById('nuevoClienteCedula').value.trim()
            const email = document.getElementById('nuevoClienteEmail').value.trim()
            const telefono = document.getElementById('nuevoClienteTelefono').value.trim()

            // Limpiar errores anteriores
            cedulaErrorDiv.style.display = 'none'
            cedulaErrorDiv.textContent = ''
            emailErrorDiv.style.display = 'none'
            emailErrorDiv.textContent = ''

            if (!nombre || !apellido || !cedula) {
              Swal.fire({ icon: 'warning', title: 'Campos requeridos', text: 'Por favor completa Nombre, Apellido y Cédula.', confirmButtonColor: '#3B82F6' })
              return
            }

            // Validar email si está presente
            if (email && !this.isValidEmail(email)) {
              emailErrorDiv.textContent = 'Formato de email inválido'
              emailErrorDiv.style.display = 'block'
              return
            }

            try {
              const nuevoCliente = await clienteService.create({
                nombre,
                apellido,
                documento: cedula,
                email: email || null,
                telefono: telefono || null
              })

              if (nuevoCliente) {
                // Agregar el nuevo cliente a la lista
                this.clientes.push(nuevoCliente)
                this.clienteSeleccionado = nuevoCliente
                document.getElementById('clienteInput').value = nuevoCliente.nombre + ' ' + (nuevoCliente.apellido || '')
                document.getElementById('clienteNombre').textContent = nuevoCliente.nombre + ' ' + (nuevoCliente.apellido || '') + ' - ' + (nuevoCliente.documento || '')
                document.getElementById('clienteSeleccionado').style.display = 'block'
                
                Swal.close()
                Swal.fire({
                  icon: 'success',
                  title: '¡Éxito!',
                  text: 'Cliente registrado correctamente',
                  confirmButtonColor: '#10B981'
                })
              }
            } catch (error) {
              console.error('[NuevaVenta] Error al crear cliente:', error)
              // Mostrar error debajo del campo correspondiente
              if (error.message.includes('email')) {
                emailErrorDiv.textContent = error.message || 'Error con el email'
                emailErrorDiv.style.display = 'block'
              } else if (error.message.includes('cedula') || error.message.includes('documento')) {
                cedulaErrorDiv.textContent = error.message || 'Error con la cédula'
                cedulaErrorDiv.style.display = 'block'
              } else {
                Swal.fire({
                  icon: 'error',
                  title: 'Error',
                  text: error.message || 'No se pudo crear el cliente',
                  confirmButtonColor: '#EF4444'
                })
              }
            }
          })
        }
      },
      allowOutsideClick: false,
      allowEscapeKey: false,
      showConfirmButton: false
    })
  }

  isValidEmail(email) {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/
    return emailRegex.test(email)
  }

  init() {
    console.log('[NuevaVenta] Inicializando...')
    
    this.loadClientes()
    this.loadProductos()
    
    // Establecer fecha actual
    const today = new Date().toISOString().split('T')[0]
    const fechaInput = document.getElementById('fechaVenta')
    if (fechaInput) fechaInput.value = today

    // Event listeners para el formulario
    const form = document.getElementById('ventaForm')
    const cancelBtn = document.getElementById('cancelBtn')
    const addProductBtn = document.getElementById('addProductBtn')
    const clienteInput = document.getElementById('clienteInput')
    const productoInput = document.getElementById('productoInput')

    if (form) {
      form.addEventListener('submit', (e) => {
        e.preventDefault()
        this.saveVenta()
      })
    }

    if (cancelBtn) {
      cancelBtn.addEventListener('click', () => {
        window.location.href = '#/ventas'
      })
    }

    if (clienteInput) {
      clienteInput.addEventListener('input', (e) => {
        this.filterClientes(e.target.value)
        this.renderClientesDropdown()
      })
      
      clienteInput.addEventListener('blur', () => {
        setTimeout(() => {
          document.getElementById('clientesDropdown').style.display = 'none'
        }, 200)
      })
    }

    if (productoInput) {
      productoInput.addEventListener('input', (e) => {
        this.filterProductos(e.target.value)
        this.renderProductosDropdown()
      })

      productoInput.addEventListener('blur', () => {
        setTimeout(() => {
          document.getElementById('productosDropdown').style.display = 'none'
        }, 200)
      })
    }

    if (addProductBtn) {
      addProductBtn.addEventListener('click', (e) => {
        e.preventDefault()
        this.addDetalle()
      })
    }

    // Cerrar dropdowns al hacer click afuera
    document.addEventListener('click', (e) => {
      const clientesDropdown = document.getElementById('clientesDropdown')
      const productosDropdown = document.getElementById('productosDropdown')
      
      if (clientesDropdown && !e.target.closest('#clienteInput') && !e.target.closest('#clientesDropdown')) {
        clientesDropdown.style.display = 'none'
      }
      if (productosDropdown && !e.target.closest('#productoInput') && !e.target.closest('#productosDropdown')) {
        productosDropdown.style.display = 'none'
      }
    })
  }
}
