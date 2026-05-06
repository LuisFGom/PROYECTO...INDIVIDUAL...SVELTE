<script>
  import { dataStore, setData, setLoading, setSuccess } from '../stores/store.js'
  import clienteService from '../services/clienteService.js'
  import { validators, formatters } from '../utils/validators.js'
  import FormInput from '../components/FormInput.svelte'
  import Alert from '../components/Alert.svelte'
  import Swal from 'sweetalert2'

  const ITEMS_PER_PAGE = 10

  let clientes = []
  let loading = true
  let searchTerm = ''
  let filteredClientes = []
  let isAdmin = false
  let editingClienteId = null
  let showFormModal = false
  let successMessage = ''
  let currentPage = 1
  let totalPages = 1
  let paginatedClientes = []

  let formData = {
    nombre: '',
    apellido: '',
    cedula: '',
    email: '',
    telefono: '',
    direccion: ''
  }

  let errors = {}

  // Validaciones reactivas en tiempo real (solo cuando el modal está abierto)
  $: if (showFormModal) {
    // Filtrar nombre: solo letras, espacios y acentos
    const nombreFiltrado = formData.nombre.replace(/[^a-zA-ZáéíóúÁÉÍÓÚñÑ\s]/g, '')
    if (nombreFiltrado !== formData.nombre) {
      formData.nombre = nombreFiltrado
    }

    // Filtrar apellido: solo letras, espacios y acentos
    const apellidoFiltrado = formData.apellido.replace(/[^a-zA-ZáéíóúÁÉÍÓÚñÑ\s]/g, '')
    if (apellidoFiltrado !== formData.apellido) {
      formData.apellido = apellidoFiltrado
    }

    // La cédula se formatea en el evento on:input del input

    // El teléfono se formatea en el evento on:input del input

    // Limitar dirección a 100 caracteres
    if (formData.direccion.length > 100) {
      formData.direccion = formData.direccion.substring(0, 100)
    }
  }

  // Reactividad para filtrado: cuando searchTerm cambia desde el input, recalcular y volver a página 1
  $: searchTerm && (() => {
    console.log('[REACTIVIDAD SEARCH] searchTerm cambió a:', searchTerm, 'ejecutando filterClientes(true)')
    filterClientes(true)
  })()

  // Reactividad para paginación: recalcular cuando cambie filteredClientes o currentPage
  $: {
    console.log('[REACTIVIDAD PAGINACION] Disparada - filteredClientes:', filteredClientes.length, 'currentPage:', currentPage, 'searchTerm:', searchTerm)
    const start = (currentPage - 1) * ITEMS_PER_PAGE
    const end = start + ITEMS_PER_PAGE
    paginatedClientes = [...filteredClientes.slice(start, end)]
    totalPages = Math.ceil(filteredClientes.length / ITEMS_PER_PAGE) || 1
    console.log('[REACTIVIDAD PAGINACION] paginatedClientes:', paginatedClientes.length, 'totalPages:', totalPages)
  }

  const loadClientes = async () => {
    loading = true
    try {
      const user = JSON.parse(localStorage.getItem('currentUser') || '{}')
      // Verificar si es admin de diferentes formas (case-insensitive)
      const rolNormalizado = (user.rol || user.role || '').toLowerCase()
      isAdmin = rolNormalizado === 'admin' || user.roleId === 1
      console.log('Usuario actual:', user)
      console.log('¿Es admin?', isAdmin)
      
      const data = await clienteService.getAll()
      console.log('[loadClientes] Data recibida:', data)
      
      // Cargar solo clientes activos
      clientes = Array.isArray(data) ? data.filter(c => c.activo !== false && c.Activo !== false) : []
      console.log('[loadClientes] Clientes activos después de filtrar:', clientes.length)
      
      // Ordenar por FechaCreacion descendente (más nuevo primero)
      clientes.sort((a, b) => {
        const fechaA = new Date(a.fechaCreacion || a.FechaCreacion || 0)
        const fechaB = new Date(b.fechaCreacion || b.FechaCreacion || 0)
        return fechaB - fechaA
      })
      
      // Resetear búsqueda y página
      searchTerm = ''
      currentPage = 1
      filterClientes(true)
    } catch (error) {
      console.error('[loadClientes] Error:', error)
      await Swal.fire('Error', error.message, 'error')
    } finally {
      loading = false
    }
  }

  const filterClientes = (resetPage = true) => {
    console.log('[BUSQUEDA] searchTerm:', searchTerm)
    console.log('[BUSQUEDA] clientes disponibles:', clientes.length)
    const term = searchTerm.trim().toLowerCase()
    const termSinFormato = searchTerm.replace(/[^0-9]/g, '')
    if (!term) {
      filteredClientes = [...clientes]
    } else {
      const contieneArroba = term.includes('@')
      const palabras = term.split(/\s+/).filter(Boolean)
      filteredClientes = clientes.filter(c => {
        const nombre = (c.nombre || '').toLowerCase()
        const apellido = (c.apellido || '').toLowerCase()
        const email = (c.email || '').toLowerCase()
        const correo = (c.correo || '').toLowerCase()
        const cedula = (c.cedula || '').replace(/[^0-9]/g, '')
        const documento = (c.documento || '').replace(/[^0-9]/g, '')
        const telefono = (c.telefono || '').replace(/[^0-9]/g, '')
        const buscaNumeros = !!termSinFormato
        // Si el término tiene números, buscar en campos numéricos
        if (buscaNumeros && (
          cedula.includes(termSinFormato) ||
          documento.includes(termSinFormato) ||
          telefono.includes(termSinFormato)
        )) {
          return true
        }
        // Si el término parece un correo, buscar solo en email/correo
        if (contieneArroba) {
          return email.includes(term) || correo.includes(term)
        }
        // Si el término tiene palabras, todas deben estar en algún campo de texto
        return palabras.every(palabra =>
          nombre.includes(palabra) ||
          apellido.includes(palabra) ||
          email.includes(palabra) ||
          correo.includes(palabra)
        )
      })
    }
    console.log('[BUSQUEDA] resultados filtrados:', filteredClientes.length)
    if (resetPage) {
      currentPage = 1
    }
  }

  const goToPage = (page) => {
    if (page >= 1 && page <= totalPages) {
      currentPage = page
    }
  }

  const validateForm = () => {
    errors = {}

    // Validar nombre
    if (!formData.nombre?.trim()) {
      errors.nombre = 'El nombre es requerido'
    } else if (!validators.isOnlyLetters(formData.nombre)) {
      errors.nombre = 'El nombre solo puede contener letras'
    }

    // Validar apellido
    if (!formData.apellido?.trim()) {
      errors.apellido = 'El apellido es requerido'
    } else if (!validators.isOnlyLetters(formData.apellido)) {
      errors.apellido = 'El apellido solo puede contener letras'
    }

    // Validar cédula (solo en creación)
    if (!editingClienteId) {
      if (!formData.cedula?.trim()) {
        errors.cedula = 'La cédula es requerida'
      } else if (!validators.isCedula(formData.cedula)) {
        errors.cedula = 'La cédula debe tener 10 dígitos (ej: 185104679-8)'
      }
    }

    // Validar email (opcional, pero si se ingresa debe ser válido)
    if (formData.email?.trim() && !validators.isEmail(formData.email)) {
      errors.email = 'El email no es válido'
    }

    // Validar teléfono (opcional, pero si se ingresa debe tener exactamente 10 dígitos)
    if (formData.telefono?.trim() && formData.telefono.replace(/[^0-9]/g, '').length !== 10) {
      errors.telefono = 'El teléfono debe tener exactamente 10 dígitos'
    }

    // Validar dirección (máximo 100 caracteres)
    if (formData.direccion && !validators.isAddressLength(formData.direccion)) {
      errors.direccion = 'La dirección no puede exceder 100 caracteres'
    }

    return Object.keys(errors).length === 0
  }

  const handleEdit = (cliente) => {
    editingClienteId = cliente.id
    formData = {
      nombre: cliente.nombre || '',
      apellido: cliente.apellido || '',
      cedula: cliente.cedula || '',
      email: cliente.email || '',
      telefono: cliente.telefono || '',
      direccion: cliente.direccion || ''
    }
    showFormModal = true
  }

  const filterClientesSilentMode = () => {
    // Filtrar sin resetear página (modo silencioso)
    console.log('[filterClientesSilentMode] Iniciando con searchTerm:', searchTerm, 'currentPage:', currentPage)
    const term = searchTerm.trim().toLowerCase()
    const termSinFormato = searchTerm.replace(/[^0-9]/g, '')
    if (!term) {
      filteredClientes = [...clientes]
    } else {
      const contieneArroba = term.includes('@')
      const palabras = term.split(/\s+/).filter(Boolean)
      filteredClientes = clientes.filter(c => {
        const nombre = (c.nombre || '').toLowerCase()
        const apellido = (c.apellido || '').toLowerCase()
        const email = (c.email || '').toLowerCase()
        const correo = (c.correo || '').toLowerCase()
        const cedula = (c.cedula || '').replace(/[^0-9]/g, '')
        const documento = (c.documento || '').replace(/[^0-9]/g, '')
        const telefono = (c.telefono || '').replace(/[^0-9]/g, '')
        const buscaNumeros = !!termSinFormato
        if (buscaNumeros && (
          cedula.includes(termSinFormato) ||
          documento.includes(termSinFormato) ||
          telefono.includes(termSinFormato)
        )) {
          return true
        }
        if (contieneArroba) {
          return email.includes(term) || correo.includes(term)
        }
        return palabras.every(palabra =>
          nombre.includes(palabra) ||
          apellido.includes(palabra) ||
          email.includes(palabra) ||
          correo.includes(palabra)
        )
      })
    }
    console.log('[filterClientesSilentMode] Resultados filtrados:', filteredClientes.length, 'currentPage después:', currentPage)
  }

  const handleDelete = async (cliente) => {
    const result = await Swal.fire({
      title: '¿Desactivar cliente?',
      text: `Se desactivará el cliente ${cliente.nombre}.`,
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#ef4444',
      cancelButtonColor: '#6b7280',
      confirmButtonText: 'Sí, desactivar',
      cancelButtonText: 'Cancelar'
    })

    if (result.isConfirmed) {
      try {
        const paginaActual = currentPage
        console.log('[handleDelete] Página actual antes de eliminar:', paginaActual)
        await clienteService.delete(cliente.id)
        clientes = clientes.filter(c => c.id !== cliente.id)
        filterClientesSilentMode() // Recalcular sin ir a página 1
        console.log('[handleDelete] filteredClientes después de eliminar:', filteredClientes.length)
        // Calcular cuántas páginas hay después de la eliminación
        const totalPaginasAhora = Math.ceil(filteredClientes.length / ITEMS_PER_PAGE) || 1
        console.log('[handleDelete] Total páginas ahora:', totalPaginasAhora, 'página actual:', paginaActual)
        // Si la página actual es > total de páginas, ir a la última página
        if (paginaActual > totalPaginasAhora) {
          currentPage = totalPaginasAhora
          console.log('[handleDelete] Página ajustada a:', totalPaginasAhora)
        } else {
          currentPage = paginaActual
          console.log('[handleDelete] Mantener página:', paginaActual)
        }
        successMessage = 'Cliente desactivado correctamente'
        setTimeout(() => { successMessage = '' }, 3000)
      } catch (error) {
        await Swal.fire('Error', error.message, 'error')
      }
    }
  }

  const handleSubmit = async () => {
    if (!validateForm()) return

    try {
      const dataToSend = {
        nombre: formData.nombre.trim(),
        apellido: formData.apellido.trim(),
        email: formData.email.trim() || null,
        telefono: formData.telefono.replace(/[^0-9]/g, '') || null,
        direccion: formData.direccion.trim() || null
      }

      // Incluir documento (cédula) solo cuando es creación (no en edición)
      if (!editingClienteId) {
        dataToSend.documento = formData.cedula.replace(/[^0-9]/g, '')
      }

      if (editingClienteId) {
        // Actualización: actualizar en la lista sin recargarse, mantener página actual
        console.log('[handleSubmit] Editando cliente:', editingClienteId, 'página actual:', currentPage)
        await clienteService.update(editingClienteId, dataToSend)
        const index = clientes.findIndex(c => c.id === editingClienteId)
        if (index !== -1) {
          clientes[index] = { ...clientes[index], ...dataToSend }
          clientes = clientes // Trigger reactivity
          const paginaAntes = currentPage
          filterClientesSilentMode() // Recalcular filtrado sin ir a página 1
          // Asegurar que la página se mantiene igual
          currentPage = paginaAntes
          console.log('[handleSubmit] Página después de editar:', currentPage)
        }
        successMessage = 'Cliente actualizado correctamente'
      } else {
        // Creación: agregar a la lista sin recargarse, ir a página 1
        const response = await clienteService.create(dataToSend)
        console.log('[handleSubmit] Response de creación:', response)
        
        const clienteId = response?.id || response?.Id
        if (clienteId) {
          // Usar la respuesta del servidor con el ID asignado
          const nuevoCliente = {
            ...response,
            id: response?.id || response?.Id,
            documento: response?.documento || response?.Documento,
            correo: response?.email || response?.Email || response?.correo || response?.Correo
          }
          clientes = [nuevoCliente, ...clientes] // Esto actualizará filteredClientes en el bloque reactivo
          // Limpiar búsqueda y mostrar en página 1
          searchTerm = ''
          currentPage = 1
          filterClientes(true) // Asegurar que filteredClientes se actualiza con el nuevo cliente
          successMessage = 'Cliente creado correctamente'
        } else {
          throw new Error('No se pudo obtener el ID del cliente creado. Response: ' + JSON.stringify(response))
        }
      }
      
      setTimeout(() => { successMessage = '' }, 3000)
      showFormModal = false
      resetForm()
    } catch (error) {
      console.error('[handleSubmit] Error:', error)
      await Swal.fire('Error', error.message, 'error')
    }
  }

  const resetForm = () => {
    formData = {
      nombre: '',
      apellido: '',
      cedula: '',
      email: '',
      telefono: '',
      direccion: ''
    }
    errors = {}
    editingClienteId = null
  }

  const handleNewCliente = () => {
    resetForm()
    showFormModal = true
  }

  const handleNameInput = () => {
    // Este handler ya no es necesario, la reactividad lo maneja
  }

  const handleLastNameInput = () => {
    // Este handler ya no es necesario, la reactividad lo maneja
  }

  const handleCedulaInput = () => {
    // Este handler ya no es necesario, la reactividad lo maneja
  }

  const handlePhoneInput = () => {
    // Este handler ya no es necesario, la reactividad lo maneja
  }

  loadClientes()
</script>

<div class="clientes-page">
  <div class="page-header">
    <h1><i class="fas fa-users"></i> Clientes</h1>
    {#if isAdmin}
      <button class="btn btn-primary" on:click={handleNewCliente}>
        <i class="fas fa-plus"></i> Nuevo Cliente
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
        placeholder="Buscar por nombre, documento, teléfono o correo..."
        bind:value={searchTerm}
      />
    </div>

    <div class="card-body">
      {#if loading}
        <div class="loading">Cargando clientes...</div>
      {:else if clientes.length === 0}
        <div class="empty-state">
          <i class="fas fa-users"></i>
          <p>No hay clientes registrados</p>
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
                <th>NOMBRE</th>
                <th>DOCUMENTO</th>
                <th>EMAIL</th>
                <th>TELÉFONO</th>
                <th>ESTADO</th>
                <th>CREACIÓN</th>
                <th>ACCIONES</th>
              </tr>
            </thead>
            <tbody>
              {#each paginatedClientes as cliente (cliente.id)}
                <tr>
                  <td>
                    <strong>{cliente.nombre} {cliente.apellido || ''}</strong>
                  </td>
                  <td>{formatters.formatCedula(cliente.cedula || cliente.documento || '')}</td>
                  <td>{cliente.email || cliente.correo || '-'}</td>
                  <td>{cliente.telefono || '-'}</td>
                  <td>
                    <span class="badge badge-success">
                      {cliente.activo ? 'Activo' : 'Inactivo'}
                    </span>
                  </td>
                  <td>
                    {formatters.formatDate(cliente.fechaCreacion)}
                  </td>
                  <td>
                    <div class="action-buttons">
                      {#if isAdmin}
                        <button
                          class="btn btn-sm btn-warning"
                          on:click={() => handleEdit(cliente)}
                          title="Editar"
                        >
                          <i class="fas fa-edit"></i> Editar
                        </button>
                        <button
                          class="btn btn-sm btn-danger"
                          on:click={() => handleDelete(cliente)}
                          title="Desactivar"
                        >
                          <i class="fas fa-ban"></i> Desactivar
                        </button>
                      {:else}
                        <span style="color: #6b7280; font-size: 0.75rem;">-</span>
                      {/if}
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

  {#if showFormModal}
    <div class="modal-overlay" role="button" tabindex="0" on:click={() => { showFormModal = false }} on:keydown={(e) => { if (e.key === 'Escape') showFormModal = false; }}>
      <div class="modal-content" role="presentation" on:click|stopPropagation>
        <div class="modal-header">
          <h2 class="modal-title">
            {#if editingClienteId}
              Editar Cliente
            {:else}
              Nuevo Cliente
            {/if}
          </h2>
          <button class="modal-close" on:click={() => { showFormModal = false }}>
            <i class="fas fa-times"></i>
          </button>
        </div>

        <div class="modal-body">
          <div class="form-row">
            <FormInput
              label="Nombre"
              name="nombre"
              bind:value={formData.nombre}
              placeholder="Nombre"
              error={errors.nombre}
              required
              maxlength={35}
            />
            <FormInput
              label="Apellido"
              name="apellido"
              bind:value={formData.apellido}
              placeholder="Apellido"
              error={errors.apellido}
              required
              maxlength={35}
            />
          </div>

          <div style="display: grid; grid-template-columns: 1fr; gap: 0.5rem; margin-bottom: 1rem;">
            <div>
              <label for="cedula" style="display: block; margin-bottom: 0.5rem; font-weight: 600; color: #0F172A;">
                Cédula/Documento
                {#if !editingClienteId}
                  <span style="color: #ef4444;">*</span>
                {/if}
              </label>
              <input
                id="cedula"
                type="text"
                inputmode="numeric"
                value={formData.cedula}
                on:input={(e) => { 
                  let numeros = e.target.value.replace(/[^0-9]/g, '').substring(0, 10)
                  formData.cedula = numeros.length < 10 ? numeros : numeros.substring(0, 9) + '-' + numeros.substring(9, 10)
                }}
                on:keypress={(e) => {
                  if (!/[0-9]/.test(e.key) && e.key !== 'Backspace' && e.key !== 'Enter') {
                    e.preventDefault()
                  }
                }}
                placeholder="185104679-8 (10 dígitos)"
                disabled={editingClienteId ? true : false}
                maxlength={11}
                style="width: 100%; padding: 0.625rem; border: 1px solid {errors.cedula ? '#ef4444' : '#E2E8F0'}; border-radius: 0.375rem; box-sizing: border-box;"
              />
              {#if errors.cedula}
                <div style="color: #ef4444; font-size: 0.875rem; margin-top: 0.25rem;">{errors.cedula}</div>
              {:else if !editingClienteId}
                <div style="color: #64748B; font-size: 0.75rem; margin-top: 0.25rem;">Únicamente 10 dígitos - Formato automático XXXXXXXXX-X</div>
              {/if}
            </div>
          </div>

          <div class="form-row">
            <div>
              <label for="email" style="display: block; margin-bottom: 0.5rem; font-weight: 600; color: #0F172A;">
                Email
              </label>
              <input
                id="email"
                type="email"
                value={formData.email}
                on:keypress={(e) => {
                  // Solo permitir: letras, números, punto, guión, guión bajo, @, Backspace, Enter
                  const charPermitido = /[a-zA-Z0-9._\-@]/.test(e.key)
                  if (!charPermitido && e.key !== 'Backspace' && e.key !== 'Enter' && e.key !== 'ArrowLeft' && e.key !== 'ArrowRight' && e.key !== 'Delete') {
                    e.preventDefault()
                  }
                }}
                on:input={(e) => {
                  // Limpieza adicional por si acaso (para paste)
                  let valor = e.target.value
                  const partes = valor.split('@')
                  if (partes.length > 1) {
                    let antes = partes[0].replace(/[^a-zA-Z0-9._\-]/g, '')
                    let despues = partes.slice(1).join('@')
                    valor = antes + '@' + despues
                  } else {
                    valor = valor.replace(/[^a-zA-Z0-9._\-@]/g, '')
                  }
                  formData.email = valor
                }}
                placeholder="correo@ejemplo.com"
                style="width: 100%; padding: 0.625rem; border: 1px solid {errors.email ? '#ef4444' : '#E2E8F0'}; border-radius: 0.375rem; box-sizing: border-box;"
              />
              {#if errors.email}
                <div style="color: #ef4444; font-size: 0.875rem; margin-top: 0.25rem;">{errors.email}</div>
              {:else}
                <div style="color: #64748B; font-size: 0.75rem; margin-top: 0.25rem;">Símbolos permitidos: letras, números, punto (.), guión (-), guión bajo (_)</div>
              {/if}
            </div>
          </div>

          <div class="form-row">
            <div>
              <label for="telefono" style="display: block; margin-bottom: 0.5rem; font-weight: 600; color: #0F172A;">
                Teléfono
              </label>
              <input
                id="telefono"
                type="text"
                inputmode="numeric"
                value={formData.telefono}
                on:input={(e) => { 
                  let numeros = e.target.value.replace(/[^0-9]/g, '').substring(0, 10)
                  formData.telefono = numeros
                }}
                on:keypress={(e) => {
                  if (!/[0-9]/.test(e.key) && e.key !== 'Backspace' && e.key !== 'Enter') {
                    e.preventDefault()
                  }
                }}
                placeholder="0963018765"
                maxlength={10}
                style="width: 100%; padding: 0.625rem; border: 1px solid {errors.telefono ? '#ef4444' : '#E2E8F0'}; border-radius: 0.375rem; box-sizing: border-box;"
              />
              {#if errors.telefono}
                <div style="color: #ef4444; font-size: 0.875rem; margin-top: 0.25rem;">{errors.telefono}</div>
              {:else}
                <div style="color: #64748B; font-size: 0.75rem; margin-top: 0.25rem;">Unicamente 10 dígitos</div>
              {/if}
            </div>
          </div>

          <div class="form-row full-width">
            <FormInput
              label="Dirección"
              name="direccion"
              bind:value={formData.direccion}
              placeholder="Ej: Calle Principal 123 (máximo 100 caracteres)"
              error={errors.direccion}
              maxlength={100}
            />
            <div class="help-text">Máximo 100 caracteres ({formData.direccion?.length || 0}/100)</div>
          </div>
        </div>

        <div class="modal-footer">
          <button class="btn btn-secondary" on:click={() => { showFormModal = false }}>
            Cancelar
          </button>
          <button class="btn btn-primary" on:click={handleSubmit}>
            {editingClienteId ? 'Actualizar' : 'Guardar'}
          </button>
        </div>
      </div>
    </div>
  {/if}
</div>

<style>
  .clientes-page {
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

  .table-footer {
    padding: 1rem;
    border-top: 1px solid #e5e7eb;
    font-size: 0.875rem;
    color: #6b7280;
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
    border-radius: 12px;
    box-shadow: 0 20px 25px rgba(0, 0, 0, 0.15);
    max-width: 600px;
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
    font-weight: 600;
    color: #1f2937;
  }

  .modal-close {
    background: none;
    border: none;
    font-size: 1.5rem;
    color: #6b7280;
    cursor: pointer;
    padding: 0;
    width: 2rem;
    height: 2rem;
    display: flex;
    align-items: center;
    justify-content: center;
  }

  .modal-close:hover {
    color: #1f2937;
  }

  .modal-body {
    padding: 1.5rem;
  }

  .form-row {
    margin-bottom: 1.5rem;
  }

  .form-row.full-width {
    display: block;
  }

  .help-text {
    font-size: 0.75rem;
    color: #6b7280;
    margin-top: 0.25rem;
  }

  .modal-footer {
    display: flex;
    justify-content: flex-end;
    gap: 1rem;
    padding: 1.5rem;
    border-top: 1px solid #e5e7eb;
  }

  :global(.form-group) {
    display: flex;
    flex-direction: column;
    gap: 0.5rem;
    margin-bottom: 1rem;
  }

  :global(.input) {
    padding: 0.75rem;
    border: 1px solid #d1d5db;
    border-radius: 6px;
    font-size: 0.875rem;
  }

  :global(.input:focus) {
    outline: none;
    border-color: #3b82f6;
    box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
  }

  :global(.input.error) {
    border-color: #ef4444;
  }

  :global(.error-message) {
    color: #ef4444;
    font-size: 0.75rem;
  }

  .pagination-controls {
    padding: 1.5rem;
    border-top: 1px solid #e5e7eb;
    display: flex;
    gap: 0.5rem;
    flex-wrap: wrap;
    align-items: center;
    justify-content: center;
  }

  .btn-page {
    padding: 0.5rem 0.75rem;
    border: 1px solid #d1d5db;
    background: white;
    color: #1f2937;
    border-radius: 6px;
    cursor: pointer;
    font-size: 0.875rem;
    font-weight: 500;
    transition: all 0.2s;
    display: inline-flex;
    align-items: center;
    gap: 0.25rem;
  }

  .btn-page:hover:not(:disabled) {
    background: #3b82f6;
    color: white;
    border-color: #3b82f6;
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

  .btn-page span {
    white-space: nowrap;
  }
</style>
