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

    // Formatear cédula: solo números con máscara XX-XXX-XXX-X (SOLO SI NO ESTAMOS EDITANDO)
    if (!editingClienteId) {
      let numeros = formData.cedula.replace(/[^0-9]/g, '').substring(0, 10)
      let cedulaFormateada = ''
      if (numeros.length > 0) {
        if (numeros.length <= 2) {
          cedulaFormateada = numeros
        } else if (numeros.length <= 5) {
          cedulaFormateada = numeros.substring(0, 2) + '-' + numeros.substring(2)
        } else if (numeros.length <= 8) {
          cedulaFormateada = numeros.substring(0, 2) + '-' + numeros.substring(2, 5) + '-' + numeros.substring(5)
        } else {
          cedulaFormateada = numeros.substring(0, 2) + '-' + numeros.substring(2, 5) + '-' + numeros.substring(5, 8) + '-' + numeros.substring(8)
        }
      }
      if (cedulaFormateada !== formData.cedula && (cedulaFormateada !== '' || formData.cedula !== '')) {
        formData.cedula = cedulaFormateada
      }
    }

    // Formatear teléfono: solo números con máscara 09-XXXX-XXXX
    let telefonoNumeros = formData.telefono.replace(/[^0-9]/g, '').substring(0, 10)
    let telefonoFormateado = ''
    if (telefonoNumeros.length > 0) {
      if (telefonoNumeros.length <= 2) {
        telefonoFormateado = telefonoNumeros
      } else if (telefonoNumeros.length <= 6) {
        telefonoFormateado = telefonoNumeros.substring(0, 2) + '-' + telefonoNumeros.substring(2)
      } else {
        telefonoFormateado = telefonoNumeros.substring(0, 2) + '-' + telefonoNumeros.substring(2, 6) + '-' + telefonoNumeros.substring(6)
      }
    }
    if (telefonoFormateado !== formData.telefono && (telefonoFormateado !== '' || formData.telefono !== '')) {
      formData.telefono = telefonoFormateado
    }

    // Limitar dirección a 100 caracteres
    if (formData.direccion.length > 100) {
      formData.direccion = formData.direccion.substring(0, 100)
    }
  }

  // Reactividad para búsqueda: filtrar clientes cuando cambie searchTerm
  $: if (clientes.length > 0) searchTerm, filterClientes()

  // Reactividad para paginación: recalcular cuando cambie filteredClientes o currentPage
  $: {
    console.log('[REACTIVIDAD] paginación actualizada - filteredClientes:', filteredClientes.length, 'currentPage:', currentPage)
    const start = (currentPage - 1) * ITEMS_PER_PAGE
    const end = start + ITEMS_PER_PAGE
    paginatedClientes = [...filteredClientes.slice(start, end)]
    totalPages = Math.ceil(filteredClientes.length / ITEMS_PER_PAGE) || 1
    console.log('[REACTIVIDAD] paginatedClientes:', paginatedClientes.length, 'totalPages:', totalPages)
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
      
      // Para no sobrecargar el navegador, limitar a 500 clientes
      const data = await clienteService.getAll()
      clientes = Array.isArray(data) ? data.filter(c => c.activo !== false).slice(0, 500) : []
      clientes.sort((a, b) => new Date(b.fechaCreacion) - new Date(a.fechaCreacion))
      // Resetear búsqueda y página
      searchTerm = ''
      currentPage = 1
      filterClientes()
    } catch (error) {
      await Swal.fire('Error', error.message, 'error')
    } finally {
      loading = false
    }
  }

  const filterClientes = () => {
    console.log('[BUSQUEDA] searchTerm:', searchTerm)
    console.log('[BUSQUEDA] clientes disponibles:', clientes.length)
    const term = searchTerm.trim().toLowerCase()
    const termSinFormato = searchTerm.replace(/[^0-9]/g, '')
    if (!term) {
      filteredClientes = [...clientes]
    } else {
      filteredClientes = clientes.filter(c => {
        const nombre = (c.nombre || '').toLowerCase()
        const apellido = (c.apellido || '').toLowerCase()
        const email = (c.email || '').toLowerCase()
        const correo = (c.correo || '').toLowerCase()
        const cedula = (c.cedula || '').replace(/[^0-9]/g, '')
        const documento = (c.documento || '').replace(/[^0-9]/g, '')
        const telefono = (c.telefono || '').replace(/[^0-9]/g, '')
        const buscaNumeros = !!termSinFormato
        return (
          nombre.includes(term) ||
          apellido.includes(term) ||
          email.includes(term) ||
          correo.includes(term) ||
          (buscaNumeros && (
            cedula.includes(termSinFormato) ||
            documento.includes(termSinFormato) ||
            telefono.includes(termSinFormato)
          ))
        )
      })
    }
    console.log('[BUSQUEDA] resultados filtrados:', filteredClientes.length)
    currentPage = 1
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
        errors.cedula = 'La cédula debe tener 10 dígitos (formato: XX-XXX-XXX-X)'
      }
    }

    // Validar email (opcional, pero si se ingresa debe ser válido)
    if (formData.email?.trim() && !validators.isEmail(formData.email)) {
      errors.email = 'El email no es válido'
    }

    // Validar teléfono (opcional, pero si se ingresa debe tener 7-10 dígitos)
    if (formData.telefono?.trim() && !validators.isPhone(formData.telefono)) {
      errors.telefono = 'El teléfono debe tener 7-10 dígitos'
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

  const handleDelete = async (cliente) => {
    const result = await Swal.fire({
      title: '¿Eliminar cliente?',
      text: 'Esta acción no se puede deshacer.',
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#ef4444',
      cancelButtonColor: '#6b7280',
      confirmButtonText: 'Sí, eliminar',
      cancelButtonText: 'Cancelar'
    })

    if (result.isConfirmed) {
      try {
        await clienteService.delete(cliente.id)
        successMessage = 'Cliente eliminado correctamente'
        setTimeout(() => { successMessage = '' }, 3000)
        await loadClientes()
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
        await clienteService.update(editingClienteId, dataToSend)
        successMessage = 'Cliente actualizado correctamente'
      } else {
        await clienteService.create(dataToSend)
        successMessage = 'Cliente creado correctamente'
      }
      
      setTimeout(() => { successMessage = '' }, 3000)
      showFormModal = false
      resetForm()
      await loadClientes()
    } catch (error) {
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
        on:input={() => filterClientes()}
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
                  <td>{formatters.formatPhone(cliente.telefono || '-')}</td>
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
                          title="Eliminar"
                        >
                          <i class="fas fa-trash"></i> Eliminar
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

          <div style="display: grid; grid-template-columns: 1fr; gap: 0.5rem;">
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
                value={formData.cedula}
                on:input={(e) => { formData.cedula = e.target.value }}
                placeholder="17-056-789-01 (formato: XX-XXX-XXX-X)"
                disabled={editingClienteId ? true : false}
                maxlength={13}
                style="width: 100%; padding: 0.625rem; border: 1px solid {errors.cedula ? '#ef4444' : '#E2E8F0'}; border-radius: 0.375rem; box-sizing: border-box;"
              />
              {#if errors.cedula}
                <div style="color: #ef4444; font-size: 0.875rem; margin-top: 0.25rem;">{errors.cedula}</div>
              {:else if !editingClienteId}
                <div style="color: #64748B; font-size: 0.75rem; margin-top: 0.25rem;">10 dígitos, máscara automática XX-XXX-XXX-X</div>
              {/if}
            </div>
          </div>

          <div class="form-row">
            <FormInput
              label="Email"
              name="email"
              type="email"
              bind:value={formData.email}
              placeholder="correo@ejemplo.com"
              error={errors.email}
            />
          </div>

          <div class="form-row">
            <FormInput
              label="Teléfono"
              name="telefono"
              bind:value={formData.telefono}
              placeholder="09-8765-4321 (formato: 09-XXXX-XXXX)"
              error={errors.telefono}
              maxlength={12}
            />
            <div class="help-text">7-10 dígitos, máscara automática 09-XXXX-XXXX</div>
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
