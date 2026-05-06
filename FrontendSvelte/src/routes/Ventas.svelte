<script>
  import { onMount } from 'svelte'
  import ventaService from '../services/ventaService.js'
  import clienteService from '../services/clienteService.js'
  import productoService from '../services/productoService.js'
  import { formatters } from '../utils/validators.js'
  import DataTable from '../components/DataTable.svelte'
  import Alert from '../components/Alert.svelte'
  import Swal from 'sweetalert2'

  // Variables de estado
  let view = 'list'
  let ventas = []
  let loading = true
  let searchTerm = ''
  let filteredVentas = []
  let isAdmin = false
  let successMessage = ''
  
  // Paginación
  let currentPage = 1
  let itemsPerPage = 10
  let paginatedVentas = []

  // Variables para nueva factura
  let newFactura = {
    numeroFactura: '',
    fecha: new Date().toISOString().split('T')[0],
    cliente: null,
    productosAgregados: [],
    observaciones: ''
  }

  // Búsqueda de clientes
  let showClienteModal = false
  let clienteSearchTerm = ''
  let clientesDisponibles = []
  let filteredClientes = []

  // Búsqueda de productos
  let showProductoModal = false
  let productoSearchTerm = ''
  let productosDisponibles = []
  let filteredProductos = []

  // Modal de detalles
  let showDetallesModal = false
  let detallesFactura = null

  // Modal de edición
  let showEditModal = false
  let facturaEnEdicion = null

  // Modal de descarga PDF
  let showPdfModal = false
  let nombrePdf = ''
  let ventaParaPdf = null

  // Modal de cantidad a agregar
  let showCantidadModal = false
  let productoSeleccionado = null
  let cantidadAAgregar = 1

  // Hora actual en tiempo real
  let horaActual = new Date().toLocaleTimeString('es-ES', { hour: '2-digit', minute: '2-digit', second: '2-digit' })

  // Cálculos reactivos
  $: subtotal = newFactura.productosAgregados.reduce((sum, p) => sum + (p.precioUnitario * p.cantidad), 0)
  $: ivaCalculado = (subtotal * 0.19)
  $: total = subtotal + ivaCalculado
  
  // Paginación reactiva
  $: totalPages = Math.ceil(filteredVentas.length / itemsPerPage)
  $: {
    const start = (currentPage - 1) * itemsPerPage
    const end = start + itemsPerPage
    paginatedVentas = filteredVentas.slice(start, end)
  }

  const normalizeVenta = (venta) => {
    // Mapear todos los campos del backend al frontend
    const numeroFactura = venta.numeroFactura || `${venta.ventaId || venta.id}`
    const numeroConPrefijo = numeroFactura.startsWith('FAC-') ? numeroFactura : `FAC-${numeroFactura}`
    
    // Normalizar detalles
    const detallesNormalizados = (venta.detalles || []).map(d => ({
      id: d.id || d.productoId,
      ventaId: d.ventaId,
      productoId: d.productoId,
      nombre: d.productoNombre || d.nombre,
      cantidad: d.cantidad,
      precioUnitario: d.precioUnitario,
      descuento: d.descuento || 0,
      total: d.total
    }))
    
    return {
      id: venta.ventaId || venta.id || 'SIN ID',
      numeroFactura: numeroConPrefijo,
      cliente: {
        nombre: venta.clienteNombre || 'N/A',
        id: venta.clienteId || 0
      },
      total: venta.totalVenta || venta.total || 0,
      fechaVenta: venta.fechaVenta || new Date().toISOString(),
      detalles: detallesNormalizados,
      observaciones: venta.observaciones || '',
      subtotal: venta.subtotal || 0,
      tasaIva: venta.porcentajeIVA ? (venta.porcentajeIVA / 100) : 0.19,
      usuarioNombre: venta.usuarioNombre || 'Administrator',
      estado: venta.estado || 'Completada'
    }
  }

  const loadVentas = async () => {
    loading = true
    try {
      const user = JSON.parse(localStorage.getItem('currentUser') || '{}')
      const rolNormalizado = (user.rol || '').toLowerCase()
      isAdmin = rolNormalizado === 'admin' || user.roleId === 1
      
      console.log('Cargando ventas...')
      const data = await ventaService.getAll()
      console.log('Ventas obtenidas (raw):', data)
      
      if (Array.isArray(data) && data.length > 0) {
        console.log('Primera venta sin mapear COMPLETA:', JSON.stringify(data[0], null, 2))
      }
      
      ventas = Array.isArray(data) ? data.map(normalizeVenta) : []
      ventas.sort((a, b) => new Date(b.fechaVenta) - new Date(a.fechaVenta))
      filterVentas()
      console.log('Ventas mapeadas primera:', ventas.length > 0 ? JSON.stringify(ventas[0], null, 2) : 'Sin ventas')
    } catch (error) {
      console.error('Error cargando ventas:', error)
      ventas = []
      filteredVentas = []
    } finally {
      loading = false
    }
  }

  const filterVentas = () => {
    console.log('Filtrando ventas, searchTerm:', searchTerm)
    if (!searchTerm || searchTerm.trim() === '') {
      filteredVentas = [...ventas]
    } else {
      const term = searchTerm.toLowerCase()
      filteredVentas = ventas.filter(v => {
        const clienteMatch = v.cliente?.nombre?.toLowerCase().includes(term)
        const idMatch = v.id?.toString().toLowerCase().includes(term)
        const numeroFacturaMatch = v.numeroFactura?.toLowerCase().includes(term)
        return clienteMatch || idMatch || numeroFacturaMatch
      })
    }
    currentPage = 1 // Resetear a primera página
    console.log('Ventas filtradas:', filteredVentas.length)
  }

  const loadClientesParaBusqueda = async () => {
    try {
      const data = await clienteService.getAll()
      clientesDisponibles = Array.isArray(data) ? data : []
      filterClientes()
    } catch (error) {
      console.error('Error cargando clientes:', error)
    }
  }

  const filterClientes = () => {
    if (!clienteSearchTerm.trim()) {
      filteredClientes = [...clientesDisponibles].slice(0, 20)
    } else {
      const term = clienteSearchTerm.toLowerCase()
      filteredClientes = clientesDisponibles.filter(c =>
        String(c.id).includes(term) ||
        c.nombre?.toLowerCase().includes(term) ||
        c.apellido?.toLowerCase().includes(term) ||
        c.email?.toLowerCase().includes(term)
      ).slice(0, 20)
    }
  }

  const seleccionarCliente = (cliente) => {
    newFactura.cliente = cliente
    showClienteModal = false
    clienteSearchTerm = ''
  }

  const loadProductosParaBusqueda = async () => {
    try {
      const data = await productoService.getAll()
      productosDisponibles = Array.isArray(data) ? data.filter(p => p.activo !== false) : []
      filterProductos()
    } catch (error) {
      console.error('Error cargando productos:', error)
    }
  }

  const filterProductos = () => {
    if (!productoSearchTerm.trim()) {
      filteredProductos = [...productosDisponibles].slice(0, 20)
    } else {
      const term = productoSearchTerm.toLowerCase()
      filteredProductos = productosDisponibles.filter(p =>
        p.nombre?.toLowerCase().includes(term) ||
        p.codigo?.toLowerCase().includes(term)
      ).slice(0, 20)
    }
  }

  const agregarProducto = (producto) => {
    const stock = producto.stock || 0
    if (stock <= 0) {
      Swal.fire('Advertencia', 'Este producto no tiene stock disponible', 'warning')
      return
    }

    const existe = newFactura.productosAgregados.find(p => p.id === producto.id)
    if (existe) {
      Swal.fire('Aviso', 'Este producto ya fue agregado', 'info')
      return
    }

    // Abre el modal de cantidad en vez de agregar directamente
    productoSeleccionado = producto
    cantidadAAgregar = 1
    showCantidadModal = true
  }

  const confirmarAgregarProducto = () => {
    if (!productoSeleccionado) return
    
    const cantidad = parseInt(cantidadAAgregar) || 0
    if (cantidad < 1) {
      Swal.fire('Error', 'La cantidad mínima es 1', 'error')
      return
    }
    if (cantidad > productoSeleccionado.stock) {
      Swal.fire('Error', `Stock máximo disponible: ${productoSeleccionado.stock}`, 'error')
      return
    }

    newFactura.productosAgregados = [...newFactura.productosAgregados, {
      id: productoSeleccionado.id,
      nombre: productoSeleccionado.nombre,
      cantidad: cantidad,
      precioUnitario: productoSeleccionado.precio || 0,
      stock: productoSeleccionado.stock
    }]

    // Cierra el modal de cantidad pero MANTIENE abierto el modal de productos
    showCantidadModal = false
    productoSeleccionado = null
    cantidadAAgregar = 1
  }

  const eliminarProducto = (productoId) => {
    newFactura.productosAgregados = newFactura.productosAgregados.filter(p => p.id !== productoId)
  }

  const actualizarCantidad = (productoId, nuevaCantidad) => {
    const producto = newFactura.productosAgregados.find(p => p.id === productoId)
    if (producto) {
      const cantidad = parseInt(nuevaCantidad) || 0
      if (cantidad > producto.stock) {
        Swal.fire('Error', `Stock máximo disponible: ${producto.stock}`, 'error')
        return
      }
      if (cantidad < 1) {
        Swal.fire('Error', 'La cantidad debe ser al menos 1', 'error')
        return
      }
      producto.cantidad = cantidad
      newFactura.productosAgregados = [...newFactura.productosAgregados]
    }
  }

  const crearFactura = async () => {
    if (!newFactura.cliente) {
      Swal.fire('Error', 'Debes seleccionar un cliente', 'error')
      return
    }

    if (newFactura.productosAgregados.length === 0) {
      Swal.fire('Error', 'Debes agregar al menos un producto', 'error')
      return
    }

    try {
      // Extraer solo los dígitos del número de factura (ej: "FAC-1010029" → "1010029")
      const numeroFacturaLimpio = newFactura.numeroFactura.replace('FAC-', '')
      
      // Combinar fecha con hora actual para tener timestamp completo
      const ahora = new Date()
      const fechaConHora = new Date(newFactura.fecha)
      fechaConHora.setHours(ahora.getHours())
      fechaConHora.setMinutes(ahora.getMinutes())
      fechaConHora.setSeconds(ahora.getSeconds())
      
      const dataToSend = {
        numeroFactura: numeroFacturaLimpio,
        clienteId: newFactura.cliente.id,
        fechaVenta: fechaConHora.toISOString(),
        detalles: newFactura.productosAgregados.map(p => ({
          productoId: p.id,
          cantidad: p.cantidad,
          precioUnitario: p.precioUnitario
        })),
        observaciones: newFactura.observaciones,
        total: total,
        tasaIva: 0.19
      }

      console.log('Creando factura con número:', numeroFacturaLimpio)
      console.log('Datos completos:', JSON.stringify(dataToSend, null, 2))
      await ventaService.create(dataToSend)
      console.log('Factura creada exitosamente')
      
      Swal.fire('Éxito', `Factura ${newFactura.numeroFactura} creada correctamente`, 'success')
      
      successMessage = 'Factura creada correctamente'
      setTimeout(() => { successMessage = '' }, 3000)
      
      resetearFactura()
      view = 'list'
      await loadVentas()
    } catch (error) {
      console.error('Error creando factura:', error)
      
      // ✅ MOSTRAR ERROR SIN CERRAR LA VENTANA - PERMITIR HACER CAMBIOS
      // El usuario permanece en la pantalla de factura para corregir
      Swal.fire({
        icon: 'error',
        title: 'No se pudo guardar la factura',
        html: `
          <div style="text-align: left; color: #d32f2f; font-weight: 600; padding: 1rem; background: #ffebee; border-radius: 4px;">
            ${error.message || 'Error desconocido al crear factura'}
          </div>
          <p style="margin-top: 1rem; color: #666;">
            Por favor verifica:
          </p>
          <ul style="text-align: left; color: #666;">
            <li>El cliente no haya sido eliminado</li>
            <li>Los productos sigan disponibles</li>
            <li>El stock sea suficiente</li>
          </ul>
        `,
        confirmButtonText: 'Entendido, permanecer editando',
        confirmButtonColor: '#3B82F6',
        allowOutsideClick: false
      })
    }
  }

  const resetearFactura = () => {
    newFactura = {
      numeroFactura: '',
      fecha: new Date().toISOString().split('T')[0],
      cliente: null,
      productosAgregados: [],
      observaciones: ''
    }
  }

  const handleDownloadPdf = async (venta) => {
    try {
      await ventaService.downloadPdf(venta.id)
      successMessage = 'PDF descargado correctamente'
      setTimeout(() => { successMessage = '' }, 3000)
    } catch (error) {
      await Swal.fire('Error', 'Error al descargar PDF: ' + error.message, 'error')
    }
  }

  const abrirDetalles = async (venta) => {
    console.log('Abriendo resumen de:', venta.id)
    try {
      // Cargar detalles completos del backend
      const ventaCompleta = await ventaService.getById(venta.id)
      console.log('Venta completa obtenida del backend:', JSON.stringify(ventaCompleta, null, 2))
      detallesFactura = normalizeVenta(ventaCompleta)
      console.log('Detalles normalizados:', detallesFactura)
    } catch (error) {
      console.error('Error cargando detalles:', error)
      detallesFactura = normalizeVenta(venta)
    }
    showDetallesModal = true
  }

  const guardarEdicion = async () => {
    try {
      // Aquí iría la llamada al backend para actualizar
      Swal.fire('Éxito', 'Factura actualizada correctamente', 'success')
      showEditModal = false
      await loadVentas()
    } catch (error) {
      console.error('Error guardando edición:', error)
    }
  }

  const eliminarFactura = async (venta) => {
    const result = await Swal.fire({
      title: '¿Anular Factura?',
      text: `¿Seguro que deseas anular la factura ${venta.numeroFactura}?`,
      icon: 'warning',
      showCancelButton: true,
      confirmButtonText: 'Sí, anular',
      cancelButtonText: 'Cancelar',
      confirmButtonColor: '#d32f2f'
    })

    if (result.isConfirmed) {
      try {
        // ✅ LLAMAR AL BACKEND PARA ANULAR (soft delete) Y RESTAURAR STOCK
        await ventaService.anularVenta(venta.id)
        
        Swal.fire('Éxito', 'Factura anulada correctamente', 'success')
        await loadVentas()
      } catch (error) {
        console.error('Error eliminando factura:', error)
        
        // ✅ MOSTRAR ERROR ESPECÍFICO
        Swal.fire({
          icon: 'error',
          title: 'No se pudo eliminar la factura',
          html: `
            <div style="text-align: left; color: #d32f2f; font-weight: 600; padding: 1rem; background: #ffebee; border-radius: 4px;">
              ${error.message || 'Error desconocido'}
            </div>
          `,
          confirmButtonText: 'Entendido',
          confirmButtonColor: '#d32f2f'
        })
      }
    }
  }

  const abrirPdfModal = (venta) => {
    ventaParaPdf = venta
    const hoy = new Date(venta.fechaVenta)
    const fecha = hoy.toLocaleDateString('es-ES')
    const hora = hoy.toLocaleTimeString('es-ES', { hour: '2-digit', minute: '2-digit', second: '2-digit' })
    nombrePdf = `Factura ${venta.numeroFactura} ${fecha} ${hora}`
    showPdfModal = true
  }

  const descargarPdf = async () => {
    try {
      // Aquí se descargaría con el nombre personalizado
      await ventaService.downloadPdf(ventaParaPdf.id, nombrePdf)
      successMessage = 'PDF descargado correctamente'
      setTimeout(() => { successMessage = '' }, 3000)
      showPdfModal = false
    } catch (error) {
      console.error('Error al descargar PDF:', error)
    }
  }

  // Cargar facturas al iniciar
  onMount(() => {
    console.log('Ventas.svelte montado')
    loadVentas()
    
    // Intervalo para actualizar la hora cada segundo
    const intervaloHora = setInterval(() => {
      horaActual = new Date().toLocaleTimeString('es-ES', { hour: '2-digit', minute: '2-digit', second: '2-digit' })
    }, 1000)
    
    // Limpiar intervalo cuando se desmonta el componente
    return () => clearInterval(intervaloHora)
  })

  const abrirNuevaFactura = () => {
    // Calcular el próximo número basándose en las ventas existentes
    if (ventas.length > 0) {
      // Encontrar el ventaId más alto
      const maxVentaId = Math.max(...ventas.map(v => parseInt(v.id) || 0))
      const proximoVentaId = maxVentaId + 1
      // Fórmula: 1010000 + (ventaId - 20000)
      const numeroFactura = 1010000 + (proximoVentaId - 20000)
      newFactura.numeroFactura = `FAC-${numeroFactura}`
      console.log('Próximo número calculado:', newFactura.numeroFactura, '(ventaId:', proximoVentaId, ')')
    } else {
      newFactura.numeroFactura = `FAC-1010001`
    }
    
    view = 'new'
    loadClientesParaBusqueda()
    loadProductosParaBusqueda()
  }
</script>

<div class="ventas-page">
  {#if successMessage}
    <Alert type="success" message={successMessage} />
  {/if}

  {#if view === 'list'}
    <div class="page-header">
      <h1><i class="fas fa-shopping-cart"></i> Ventas/Facturas</h1>
      {#if isAdmin}
        <button class="btn btn-primary" on:click={abrirNuevaFactura}>
          <i class="fas fa-plus"></i> Nueva Venta
        </button>
      {/if}
    </div>

    <div class="card">
      <div class="card-header">
        <input
          class="input"
          type="text"
          placeholder="Buscar por cliente o ID..."
          bind:value={searchTerm}
          on:input={() => filterVentas()}
        />
      </div>

      <div class="card-body">
        <div style="overflow-x: auto;">
          <table class="tabla-ventas">
            <thead>
              <tr>
                <th>NÚMERO FACTURA</th>
                <th>CLIENTE</th>
                <th>FECHA</th>
                <th>TOTAL</th>
                <th>ESTADO</th>
                <th>ACCIONES</th>
              </tr>
            </thead>
            <tbody>
              {#each paginatedVentas as venta, index (index)}
                <tr>
                  <td>{venta.numeroFactura}</td>
                  <td>{venta.cliente?.nombre || 'N/A'}</td>
                  <td>{formatters.formatDate(venta.fechaVenta)}</td>
                  <td style="font-weight: 600;">{formatters.formatCurrency(venta.total)}</td>
                  <td>
                    <span class="estado-badge">{venta.estado}</span>
                  </td>
                  <td>
                    <div style="display: flex; gap: 0.5rem; flex-wrap: wrap;">
                      <button class="btn-action btn-action-ver" on:click={() => abrirDetalles(venta)} title="Ver resumen">
                        <i class="fas fa-file-alt"></i> Resumen
                      </button>
                      <button class="btn-action btn-action-eliminar" on:click={() => eliminarFactura(venta)} title="Anular" disabled={!isAdmin}>
                        <i class="fas fa-ban"></i> Anular
                      </button>
                      <button class="btn-action btn-action-pdf" on:click={() => abrirPdfModal(venta)} title="Descargar PDF">
                        <i class="fas fa-file-pdf"></i> PDF
                      </button>
                    </div>
                  </td>
                </tr>
              {/each}
            </tbody>
          </table>
        </div>

        {#if filteredVentas.length === 0}
          <div style="text-align: center; padding: 2rem; color: #9CA3AF;">
            <i class="fas fa-inbox" style="font-size: 32px; margin-bottom: 1rem;"></i>
            <p>No hay facturas</p>
          </div>
        {:else}
          <!-- Controles de Paginación -->
          <div style="display: flex; justify-content: center; align-items: center; padding: 1.5rem; border-top: 1px solid #E5E7EB; gap: 0.5rem; flex-wrap: wrap;">
            <button class="btn btn-sm btn-secondary" on:click={() => currentPage = 1} disabled={currentPage === 1}>
              <i class="fas fa-step-backward"></i> Primera
            </button>
            <button class="btn btn-sm btn-secondary" on:click={() => currentPage = Math.max(1, currentPage - 10)} disabled={currentPage - 10 < 1}>
              -1000
            </button>
            <button class="btn btn-sm btn-secondary" on:click={() => currentPage = Math.max(1, currentPage - 100)} disabled={currentPage - 100 < 1}>
              -100
            </button>
            <button class="btn btn-sm btn-secondary" on:click={() => currentPage = Math.max(1, currentPage - 10)} disabled={currentPage - 10 < 1}>
              -10
            </button>
            <button class="btn btn-sm btn-secondary" on:click={() => currentPage = Math.max(1, currentPage - 1)} disabled={currentPage === 1}>
              <i class="fas fa-chevron-left"></i> Anterior
            </button>
            
            {#each Array.from({length: Math.min(5, totalPages)}, (_, i) => currentPage + i) as pageNum}
              {#if pageNum <= totalPages}
                <button 
                  class="btn btn-sm"
                  class:btn-primary={pageNum === currentPage}
                  class:btn-secondary={pageNum !== currentPage}
                  on:click={() => currentPage = pageNum}
                >
                  {pageNum}
                </button>
              {/if}
            {/each}
            
            <button class="btn btn-sm btn-secondary" on:click={() => currentPage = Math.min(totalPages, currentPage + 1)} disabled={currentPage === totalPages}>
              Siguiente <i class="fas fa-chevron-right"></i>
            </button>
            <button class="btn btn-sm btn-secondary" on:click={() => currentPage = Math.min(totalPages, currentPage + 10)} disabled={currentPage + 10 > totalPages}>
              +10
            </button>
            <button class="btn btn-sm btn-secondary" on:click={() => currentPage = Math.min(totalPages, currentPage + 100)} disabled={currentPage + 100 > totalPages}>
              +100
            </button>
            <button class="btn btn-sm btn-secondary" on:click={() => currentPage = Math.min(totalPages, currentPage + 10)} disabled={currentPage + 10 > totalPages}>
              +1000
            </button>
            <button class="btn btn-sm btn-secondary" on:click={() => currentPage = totalPages} disabled={currentPage === totalPages}>
              Última <i class="fas fa-step-forward"></i>
            </button>
            
            <div style="font-size: 12px; color: #6B7280; width: 100%; text-align: center;">
              Página <strong>{currentPage}</strong> de <strong>{totalPages}</strong> • Total: <strong>{filteredVentas.length}</strong> registros ({itemsPerPage} por página)
            </div>
          </div>
        {/if}
      </div>
    </div>

  {:else if view === 'new'}
    <div class="page-header">
      <h1><i class="fas fa-file-invoice"></i> Nueva Venta/Factura</h1>
      <button class="btn btn-secondary" on:click={() => { view = 'list'; resetearFactura() }}>
        <i class="fas fa-arrow-left"></i> Volver
      </button>
    </div>

    <div class="card">
      <div class="card-body">
        <!-- Datos de Factura -->
        <div style="display: grid; grid-template-columns: 1fr 1fr 1fr; gap: 2rem; margin-bottom: 2rem;">
          <div class="form-group">
            <label style="font-size: 16px; font-weight: 700;">Número de Factura:</label>
            <input type="text" disabled value={newFactura.numeroFactura} style="font-size: 24px; font-weight: 700; padding: 1rem; background: #F3F4F6;" />
          </div>
          <div class="form-group">
            <label style="font-size: 16px; font-weight: 700;">Fecha:</label>
            <input type="date" disabled value={newFactura.fecha} style="font-size: 24px; font-weight: 700; padding: 1rem; background: #F3F4F6;" />
          </div>
          <div class="form-group">
            <label style="font-size: 16px; font-weight: 700;">Hora:</label>
            <input type="text" disabled value={horaActual} style="font-size: 24px; font-weight: 700; padding: 1rem; background: #F3F4F6; text-align: center;" />
          </div>
        </div>

        <!-- Datos del Cliente -->
        <div class="form-section">
          <h3><i class="fas fa-user"></i> Datos del Cliente <span style="color: red;">*</span></h3>
          {#if newFactura.cliente}
            <div style="background: #F0F9FF; border-left: 4px solid #3B82F6; padding: 1rem; border-radius: 4px; margin-bottom: 1rem;">
              <div style="display: grid; grid-template-columns: repeat(2, 1fr); gap: 1rem;">
                <div>
                  <strong>CI:</strong> {formatters.formatCedula(newFactura.cliente.cedula || newFactura.cliente.documento || '')}
                </div>
                <div>
                  <strong>Nombre:</strong> {newFactura.cliente.nombre}
                </div>
                <div>
                  <strong>Apellido:</strong> {newFactura.cliente.apellido || '-'}
                </div>
                <div>
                  <strong>Email:</strong> {newFactura.cliente.email || '-'}
                </div>
              </div>
            </div>
            <button class="btn btn-sm btn-secondary" on:click={() => { showClienteModal = true; loadClientesParaBusqueda() }}>
              Cambiar Cliente
            </button>
          {:else}
            <button class="btn-buscar" on:click={() => { showClienteModal = true; loadClientesParaBusqueda() }}>
              <i class="fas fa-search"></i> BUSCAR CLIENTE
            </button>
          {/if}
        </div>

        <!-- Agregar Productos -->
        <div class="form-section" style="margin-top: 2rem;">
          <h3><i class="fas fa-box"></i> Información del Producto <span style="color: red;">*</span></h3>
          <button class="btn-buscar btn-buscar-verde" on:click={() => { showProductoModal = true; loadProductosParaBusqueda() }}>
            <i class="fas fa-search"></i> BUSCAR PRODUCTO
          </button>
        </div>

        <!-- Tabla de Productos Agregados -->
        {#if newFactura.productosAgregados.length > 0}
          <div class="form-section" style="margin-top: 1rem;">
            <table class="tabla-productos">
              <thead>
                <tr>
                  <th style="text-align: left;">PRODUCTO</th>
                  <th style="text-align: center;">CANTIDAD</th>
                  <th style="text-align: right;">PRECIO UNIT.</th>
                  <th style="text-align: right;">TOTAL</th>
                  <th style="text-align: center;">ACCIÓN</th>
                </tr>
              </thead>
              <tbody>
                {#each newFactura.productosAgregados as producto (producto.id)}
                  <tr>
                    <td>{producto.nombre}</td>
                    <td style="text-align: center;">
                      <input
                        type="number"
                        min="1"
                        max={producto.stock}
                        value={producto.cantidad}
                        on:change={(e) => actualizarCantidad(producto.id, e.target.value)}
                        style="width: 70px; padding: 0.5rem; text-align: center;"
                      />
                    </td>
                    <td style="text-align: right;">{formatters.formatCurrency(producto.precioUnitario)}</td>
                    <td style="text-align: right; font-weight: 600;">{formatters.formatCurrency(producto.cantidad * producto.precioUnitario)}</td>
                    <td style="text-align: center;">
                      <button class="btn btn-sm btn-danger" on:click={() => eliminarProducto(producto.id)}>
                        <i class="fas fa-trash"></i>
                      </button>
                    </td>
                  </tr>
                {/each}
              </tbody>
            </table>
          </div>
        {:else}
          <div style="background: #F3F4F6; padding: 2rem; text-align: center; margin-top: 1rem; border-radius: 4px;">
            <p style="color: #6B7280;"><i class="fas fa-inbox"></i> No hay productos agregados</p>
          </div>
        {/if}

        <!-- Observaciones -->
        <div class="form-section" style="margin-top: 2rem;">
          <h3>Observaciones</h3>
          <textarea
            bind:value={newFactura.observaciones}
            placeholder="Notas adicionales..."
            style="min-height: 100px; width: 100%; padding: 0.75rem; border: 1px solid #D1D5DB; border-radius: 4px; font-family: inherit;"
          ></textarea>
        </div>

        <!-- Resumen de Factura -->
        <div style="background: #EBF8FF; border: 2px solid #3B82F6; border-radius: 8px; padding: 2rem; margin-top: 2rem;">
          <div style="display: flex; justify-content: flex-end;">
            <div style="width: 50%;">
              <div style="display: flex; justify-content: space-between; margin-bottom: 1rem; font-size: 16px;">
                <span>Subtotal:</span>
                <strong>{formatters.formatCurrency(subtotal)}</strong>
              </div>
              <div style="display: flex; justify-content: space-between; margin-bottom: 1.5rem; font-size: 16px;">
                <span>IVA (19%):</span>
                <strong>{formatters.formatCurrency(ivaCalculado)}</strong>
              </div>
              <div style="border-top: 2px solid #3B82F6; padding-top: 1rem; display: flex; justify-content: space-between;">
                <span style="font-weight: 700; font-size: 18px;">TOTAL:</span>
                <span style="color: #10B981; font-weight: 700; font-size: 20px;">{formatters.formatCurrency(total)}</span>
              </div>
            </div>
          </div>
        </div>

        <!-- Botones -->
        <div style="display: flex; gap: 1rem; margin-top: 2rem; justify-content: flex-end;">
          <button class="btn btn-secondary" on:click={() => { view = 'list'; resetearFactura() }}>
            Cancelar
          </button>
          <button class="btn btn-primary" on:click={crearFactura}>
            <i class="fas fa-save"></i> Guardar Venta
          </button>
        </div>
      </div>
    </div>

  {/if}

  <!-- Modal: Buscar Cliente -->
  {#if showClienteModal}
    <div class="modal-overlay" on:click={() => { showClienteModal = false }}>
      <div class="modal" on:click|stopPropagation>
        <div class="modal-header">
          <h3>Buscar Cliente</h3>
          <button class="modal-close" on:click={() => { showClienteModal = false }}>
            <i class="fas fa-times"></i>
          </button>
        </div>
        <div class="modal-body">
          <input
            type="text"
            placeholder="Buscar por ID, Nombre, Apellido o Email..."
            bind:value={clienteSearchTerm}
            on:input={() => filterClientes()}
            class="input"
            style="margin-bottom: 1rem;"
          />
          <div style="max-height: 400px; overflow-y: auto;">
            {#each filteredClientes as cliente (cliente.id)}
              <div class="cliente-item" on:click={() => seleccionarCliente(cliente)} style="padding: 1rem 1.5rem; cursor: pointer;">
                <div style="display: flex; justify-content: space-between; align-items: center;">
                  <strong style="font-size: 15px;">CI: {(cliente.cedula || cliente.documento || '').replace(/[^0-9]/g, '')}</strong>
                  <strong style="font-size: 15px; text-align: right; flex: 1; margin-left: 3rem;">Nombre: {cliente.nombre} {cliente.apellido || ''}</strong>
                </div>
                <div style="font-size: 12px; color: #6B7280; margin-top: 0.5rem; text-align: right;">
                  {cliente.email}
                </div>
              </div>
            {/each}
            {#if filteredClientes.length === 0}
              <div style="text-align: center; color: #9CA3AF; padding: 2rem;">
                No se encontraron clientes
              </div>
            {/if}
          </div>
        </div>
      </div>
    </div>
  {/if}

  <!-- Modal: Buscar Producto -->
  {#if showProductoModal}
    <div class="modal-overlay" on:click={() => { showProductoModal = false }}>
      <div class="modal" on:click|stopPropagation>
        <div class="modal-header">
          <h3>Buscar Producto</h3>
          <button class="modal-close" on:click={() => { showProductoModal = false }}>
            <i class="fas fa-times"></i>
          </button>
        </div>
        <div class="modal-body">
          <input
            type="text"
            placeholder="Buscar por nombre o código..."
            bind:value={productoSearchTerm}
            on:input={() => filterProductos()}
            class="input"
            style="margin-bottom: 1rem;"
          />
          <div style="max-height: 400px; overflow-y: auto;">
            {#each filteredProductos as producto (producto.id)}
              <div
                class="producto-item"
                class:sin-stock={producto.stock <= 0}
                on:click={() => agregarProducto(producto)}
              >
                <div>
                  <strong>{producto.nombre}</strong>
                  <div style="font-size: 12px; color: #6B7280;">
                    Código: {producto.codigo} | Precio: {formatters.formatCurrency(producto.precio)}
                  </div>
                </div>
                {#if producto.stock <= 0}
                  <div style="color: #EF4444; font-weight: 700;">
                    <i class="fas fa-ban"></i> Sin Stock
                  </div>
                {:else}
                  <div style="color: #10B981;">
                    Stock: {producto.stock}
                  </div>
                {/if}
              </div>
            {/each}
            {#if filteredProductos.length === 0}
              <div style="text-align: center; color: #9CA3AF; padding: 2rem;">
                No se encontraron productos
              </div>
            {/if}
          </div>
        </div>
      </div>
    </div>
  {/if}

  <!-- Modal: Cantidad a Agregar -->
  {#if showCantidadModal && productoSeleccionado}
    <div class="modal-overlay" on:click={() => { showCantidadModal = false }}>
      <div class="modal" on:click|stopPropagation style="max-width: 500px;">
        <div class="modal-header">
          <h3>Cantidad a Agregar</h3>
          <button class="modal-close" on:click={() => { showCantidadModal = false }}>
            <i class="fas fa-times"></i>
          </button>
        </div>
        <div class="modal-body">
          <div style="margin-bottom: 1.5rem;">
            <div style="margin-bottom: 0.5rem;">
              <strong>Producto:</strong> {productoSeleccionado.codigo} - {productoSeleccionado.nombre}
            </div>
            <div style="margin-bottom: 1rem;">
              <strong>Stock Disponible:</strong> <span style="color: #10B981; font-weight: 700;">{productoSeleccionado.stock}</span>
            </div>
            <div style="border-top: 1px solid #E5E7EB; padding-top: 1rem;">
              <label style="display: block; font-weight: 600; margin-bottom: 0.5rem;">Cantidad:</label>
              <input
                type="number"
                min="1"
                max={productoSeleccionado.stock}
                bind:value={cantidadAAgregar}
                style="width: 100%; padding: 0.75rem; border: 1px solid #D1D5DB; border-radius: 4px; font-size: 16px;"
              />
            </div>
          </div>

          <div style="display: flex; gap: 1rem; justify-content: flex-end;">
            <button class="btn btn-secondary" on:click={() => { showCantidadModal = false }}>
              Cancelar
            </button>
            <button class="btn btn-primary" on:click={confirmarAgregarProducto}>
              <i class="fas fa-plus"></i> Agregar
            </button>
          </div>
        </div>
      </div>
    </div>
  {/if}

  <!-- Modal: Detalles de Factura -->
  {#if showDetallesModal && detallesFactura}
    <div class="modal-overlay" on:click={() => { showDetallesModal = false }}>
      <div class="modal" on:click|stopPropagation style="max-width: 700px;">
        <div class="modal-header">
          <h3>Factura: {detallesFactura.numeroFactura}</h3>
          <button class="modal-close" on:click={() => { showDetallesModal = false }}>
            <i class="fas fa-times"></i>
          </button>
        </div>
        <div class="modal-body">
          <div style="margin-bottom: 1rem; padding-bottom: 1rem; border-bottom: 1px solid #E5E7EB;">
            <div style="margin-bottom: 0.5rem;">
              <strong>Fecha y Hora:</strong> {formatters.formatDateTime(detallesFactura.fechaVenta)}
            </div>
            <div style="margin-bottom: 0.5rem;">
              <strong>Cliente:</strong> {detallesFactura.cliente?.nombre || 'N/A'}
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
              <strong>IVA ({(detallesFactura.tasaIva * 100).toFixed(0)}%):</strong> {formatters.formatCurrency(detallesFactura.total - detallesFactura.subtotal)}
            </div>
            <div style="border-top: 2px solid #3B82F6; padding-top: 0.75rem; font-size: 18px;">
              <strong>TOTAL:</strong> {formatters.formatCurrency(detallesFactura.total)}
            </div>
          </div>

          <div style="display: flex; gap: 1rem; justify-content: flex-end; margin-top: 2rem;">
            <button class="btn btn-secondary" on:click={() => { showDetallesModal = false }}>
              Cerrar
            </button>
            <button class="btn btn-primary" on:click={() => { window.print() }}>
              <i class="fas fa-print"></i> Imprimir
            </button>
          </div>
        </div>
      </div>
    </div>
  {/if}

  <!-- Modal: Editar Factura -->
  {#if showEditModal && facturaEnEdicion}
    <div class="modal-overlay" on:click={() => { showEditModal = false }}>
      <div class="modal" on:click|stopPropagation style="max-width: 700px;">
        <div class="modal-header">
          <h3>Editar Factura: {facturaEnEdicion.numeroFactura}</h3>
          <button class="modal-close" on:click={() => { showEditModal = false }}>
            <i class="fas fa-times"></i>
          </button>
        </div>
        <div class="modal-body">
          <div style="background: #FFFBEB; border: 1px solid #FCD34D; padding: 1rem; border-radius: 4px; margin-bottom: 1.5rem;">
            <strong><i class="fas fa-info-circle"></i> Solo puedes modificar las cantidades. Los otros campos están bloqueados.</strong>
          </div>

          {#each facturaEnEdicion.detalles || [] as detalle (detalle.id)}
            <div style="background: #F0F9FF; border-left: 4px solid #3B82F6; padding: 1.5rem; margin-bottom: 1.5rem; border-radius: 4px;">
              <h4 style="margin: 0 0 1rem 0;">{detalle.producto?.nombre || 'Producto'}</h4>
              
              <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 1.5rem; margin-bottom: 1rem;">
                <div>
                  <label style="display: block; font-weight: 600; margin-bottom: 0.5rem;">Cantidad:</label>
                  <input type="number" bind:value={detalle.cantidad} style="width: 100%; padding: 0.75rem; border: 1px solid #D1D5DB; border-radius: 4px;" />
                </div>
                <div>
                  <label style="display: block; font-weight: 600; margin-bottom: 0.5rem;">Precio Unit.:</label>
                  <input type="text" value={formatters.formatCurrency(detalle.precioUnitario)} disabled style="width: 100%; padding: 0.75rem; border: 1px solid #D1D5DB; border-radius: 4px; background: #F3F4F6;" />
                </div>
              </div>

              <div style="text-align: right; padding-top: 1rem; border-top: 1px solid #BFDBFE;">
                <strong>Total:</strong> {formatters.formatCurrency(detalle.cantidad * detalle.precioUnitario)}
              </div>
            </div>
          {/each}

          <div style="display: flex; gap: 1rem; justify-content: flex-end;">
            <button class="btn btn-secondary" on:click={() => { showEditModal = false }}>
              Cancelar
            </button>
            <button class="btn btn-primary" on:click={guardarEdicion}>
              <i class="fas fa-save"></i> Guardar
            </button>
          </div>
        </div>
      </div>
    </div>
  {/if}

  <!-- Modal: Descargar PDF -->
  {#if showPdfModal && ventaParaPdf}
    <div class="modal-overlay" on:click={() => { showPdfModal = false }}>
      <div class="modal" on:click|stopPropagation style="max-width: 500px;">
        <div class="modal-header">
          <h3>Descargar PDF</h3>
          <button class="modal-close" on:click={() => { showPdfModal = false }}>
            <i class="fas fa-times"></i>
          </button>
        </div>
        <div class="modal-body">
          <div style="margin-bottom: 1rem;">
            <label style="display: block; font-weight: 600; margin-bottom: 0.5rem;">Nombre del archivo:</label>
            <input
              type="text"
              bind:value={nombrePdf}
              style="width: 100%; padding: 0.75rem; border: 1px solid #D1D5DB; border-radius: 4px; font-size: 14px;"
            />
            <small style="color: #6B7280; margin-top: 0.5rem; display: block;">Se agregará automáticamente la extensión .pdf</small>
          </div>

          <div style="display: flex; gap: 1rem; justify-content: flex-end;">
            <button class="btn btn-secondary" on:click={() => { showPdfModal = false }}>
              Cancelar
            </button>
            <button class="btn btn-primary" on:click={descargarPdf}>
              <i class="fas fa-download"></i> Descargar
            </button>
          </div>
        </div>
      </div>
    </div>
  {/if}
</div>

<style>
  .ventas-page {
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
  }

  .card {
    background: white;
    border-radius: 8px;
    box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
    overflow: hidden;
  }

  .card-header {
    padding: 1.5rem;
    border-bottom: 1px solid #E5E7EB;
  }

  .card-body {
    padding: 1.5rem;
  }

  .input {
    width: 100%;
    padding: 0.75rem;
    border: 1px solid #D1D5DB;
    border-radius: 4px;
    font-size: 14px;
  }

  .form-group {
    margin-bottom: 1rem;
  }

  .form-group label {
    display: block;
    font-weight: 600;
    margin-bottom: 0.4rem;
    color: #1F2937;
  }

  .form-group input {
    width: 100%;
    padding: 0.75rem;
    border: 1px solid #D1D5DB;
    border-radius: 4px;
    font-size: 14px;
    font-family: inherit;
  }

  .form-section {
    background: #F9FAFB;
    padding: 1.5rem;
    border-radius: 6px;
    border-left: 4px solid #3B82F6;
  }

  .form-section h3 {
    margin: 0 0 1rem 0;
    font-size: 16px;
    color: #1F2937;
  }

  .tabla-productos {
    width: 100%;
    border-collapse: collapse;
    font-size: 14px;
  }

  .tabla-productos th {
    background: #2D3748;
    padding: 1rem;
    text-align: left;
    font-weight: 700;
    color: white;
    border-bottom: 2px solid #1A202C;
  }

  .tabla-productos td {
    padding: 1rem;
    border-bottom: 1px solid #E5E7EB;
  }

  .tabla-productos tbody tr:hover {
    background: #F9FAFB;
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
    padding-left: 10px;
  }

  .modal {
    background: white;
    border-radius: 8px;
    box-shadow: 0 4px 6px rgba(0, 0, 0, 0.15);
    width: 90%;
    max-width: 900px;
    max-height: 90vh;
    display: flex;
    flex-direction: column;
  }

  .modal-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    padding: 1.5rem;
    border-bottom: 1px solid #E5E7EB;
  }

  .modal-header h3 {
    margin: 0;
    font-size: 18px;
  }

  .modal-close {
    background: none;
    border: none;
    font-size: 20px;
    cursor: pointer;
    color: #6B7280;
  }

  .modal-body {
    padding: 1.5rem;
    overflow-y: auto;
  }

  .cliente-item,
  .producto-item {
    padding: 1rem;
    border: 1px solid #E5E7EB;
    border-radius: 4px;
    margin-bottom: 0.5rem;
    cursor: pointer;
    transition: all 0.2s;
    display: flex;
    justify-content: space-between;
    align-items: center;
  }

  .cliente-item:hover,
  .producto-item:hover:not(.sin-stock) {
    background: #F3F4F6;
    border-color: #3B82F6;
  }

  .producto-item.sin-stock {
    background: #FEF2F2;
    cursor: not-allowed;
    opacity: 0.6;
  }

  .btn {
    padding: 0.75rem 1.5rem;
    border: none;
    border-radius: 4px;
    font-size: 14px;
    font-weight: 600;
    cursor: pointer;
    display: inline-flex;
    align-items: center;
    gap: 0.5rem;
    transition: all 0.2s;
  }

  .btn-primary {
    background: #3B82F6;
    color: white;
  }

  .btn-primary:hover {
    background: #2563EB;
  }

  .btn-secondary {
    background: #E5E7EB;
    color: #1F2937;
  }

  .btn-secondary:hover {
    background: #D1D5DB;
  }

  .btn-danger {
    background: #EF4444;
    color: white;
    padding: 0.4rem 0.75rem;
    font-size: 12px;
  }

  .btn-danger:hover {
    background: #DC2626;
  }

  .btn-sm {
    padding: 0.4rem 0.75rem;
    font-size: 12px;
  }

  .btn-buscar {
    width: 100%;
    padding: 1rem 1.5rem;
    border: none;
    border-radius: 4px;
    font-size: 18px;
    font-weight: 700;
    cursor: pointer;
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 0.75rem;
    transition: all 0.2s;
    background: #3B82F6;
    color: white;
  }

  .btn-buscar:hover {
    background: #2563EB;
  }

  .btn-buscar-verde {
    background: #10B981;
  }

  .btn-buscar-verde:hover {
    background: #059669;
  }

  .tabla-ventas {
    width: 100%;
    border-collapse: collapse;
    font-size: 14px;
  }

  .tabla-ventas th {
    background: #F3F4F6;
    padding: 1rem;
    text-align: left;
    font-weight: 700;
    color: #1F2937;
    border-bottom: 2px solid #D1D5DB;
    font-size: 12px;
    text-transform: uppercase;
  }

  .tabla-ventas td {
    padding: 1rem;
    border-bottom: 1px solid #E5E7EB;
  }

  .tabla-ventas tbody tr:hover {
    background: #F9FAFB;
  }

  .estado-badge {
    display: inline-block;
    padding: 0.4rem 0.8rem;
    background: #D1FAE5;
    color: #065F46;
    border-radius: 4px;
    font-weight: 600;
    font-size: 12px;
  }

  .btn-action {
    padding: 0.4rem 0.8rem;
    border: none;
    border-radius: 4px;
    font-size: 12px;
    font-weight: 600;
    cursor: pointer;
    display: inline-flex;
    align-items: center;
    gap: 0.3rem;
    transition: all 0.2s;
  }

  .btn-action:disabled {
    opacity: 0.5;
    cursor: not-allowed;
  }

  .btn-action-ver {
    background: #3B82F6;
    color: white;
  }

  .btn-action-ver:hover:not(:disabled) {
    background: #2563EB;
  }

  .btn-action-eliminar {
    background: #EF4444;
    color: white;
  }

  .btn-action-eliminar:hover:not(:disabled) {
    background: #DC2626;
  }

  .btn-action-pdf {
    background: #EC4899;
    color: white;
  }

  .btn-action-pdf:hover:not(:disabled) {
    background: #DB2777;
  }

  .tabla-detalles {
    width: 100%;
    border-collapse: collapse;
    font-size: 13px;
  }

  .tabla-detalles th {
    background: #F3F4F6;
    padding: 0.75rem;
    text-align: left;
    font-weight: 600;
    color: #374151;
    border-bottom: 2px solid #D1D5DB;
  }

  .tabla-detalles td {
    padding: 0.75rem;
    border-bottom: 1px solid #E5E7EB;
  }
</style>
