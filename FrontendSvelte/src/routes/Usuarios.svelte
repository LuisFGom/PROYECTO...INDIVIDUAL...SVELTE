<script>
  import usuarioService from '../services/usuarioService.js'
  import rolService from '../services/rolService.js'
  import { validators, formatters } from '../utils/validators.js'
  import FormInput from '../components/FormInput.svelte'
  import Alert from '../components/Alert.svelte'
  import Swal from 'sweetalert2'

  const ITEMS_PER_PAGE = 10

  let usuarios = []
  let roles = []
  let loading = true
  let searchTerm = ''
  let filteredUsuarios = []
  let isAdmin = false
  let editingUsuarioId = null
  let showFormModal = false
  let successMessage = ''
  let currentPage = 1
  let totalPages = 1
  let paginatedUsuarios = []

  let formData = {
    nombreUsuario: '',
    nombre: '',
    apellido: '',
    email: '',
    contrasena: '',
    confirmarContrasena: '',
    rol: '',
    estado: 'Activo'
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

    // Convertir nombreUsuario a minúsculas automáticamente
    const nombreUsuarioLowercase = formData.nombreUsuario.toLowerCase()
    if (nombreUsuarioLowercase !== formData.nombreUsuario && !editingUsuarioId) {
      formData.nombreUsuario = nombreUsuarioLowercase
    }

    // Limitar nombre a 50 caracteres
    if (formData.nombre.length > 50) {
      formData.nombre = formData.nombre.substring(0, 50)
    }
  }

  // Reactividad para paginación: recalcular cuando cambie filteredUsuarios o currentPage
  $: {
    const start = (currentPage - 1) * ITEMS_PER_PAGE
    const end = start + ITEMS_PER_PAGE
    paginatedUsuarios = [...filteredUsuarios.slice(start, end)]
    totalPages = Math.ceil(filteredUsuarios.length / ITEMS_PER_PAGE) || 1
  }

  const loadUsuarios = async () => {
    loading = true
    try {
      const user = JSON.parse(localStorage.getItem('currentUser') || '{}')
      const rolNormalizado = (user.rol || user.role || '').toLowerCase()
      isAdmin = rolNormalizado === 'admin' || user.roleId === 1
      
      const [usuariosData, rolesData] = await Promise.all([
        usuarioService.getAll(),
        rolService.getAll()
      ])
      
      usuarios = Array.isArray(usuariosData) ? usuariosData : []
      roles = Array.isArray(rolesData) ? rolesData : []
      
      usuarios.sort((a, b) => new Date(b.fechaCreacion) - new Date(a.fechaCreacion))
      searchTerm = ''
      currentPage = 1
      filterUsuarios()
    } catch (error) {
      await Swal.fire('Error', error.message, 'error')
    } finally {
      loading = false
    }
  }

  const filterUsuarios = () => {
    if (!searchTerm.trim()) {
      filteredUsuarios = [...usuarios]
    } else {
      const term = searchTerm.toLowerCase()
      filteredUsuarios = usuarios.filter(u =>
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

  const validateForm = () => {
    errors = {}

    // Validar nombre de usuario (solo en creación)
    if (!editingUsuarioId) {
      if (!formData.nombreUsuario?.trim()) {
        errors.nombreUsuario = 'El nombre de usuario es requerido'
      } else if (formData.nombreUsuario.length < 3) {
        errors.nombreUsuario = 'El nombre de usuario debe tener mínimo 3 caracteres'
      } else if (!/^[a-zA-Z\-_]+$/.test(formData.nombreUsuario)) {
        errors.nombreUsuario = 'Solo se permiten letras, guiones (-) y guiones bajos (_)'
      }
    }

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

    // Validar email
    if (!formData.email?.trim()) {
      errors.email = 'El email es requerido'
    } else if (!validators.isEmail(formData.email)) {
      errors.email = 'El email no es válido'
    }

    // Validar contraseña (solo en creación)
    if (!editingUsuarioId) {
      if (!formData.contrasena?.trim()) {
        errors.contrasena = 'La contraseña es requerida'
      } else if (!validators.isPasswordStrong(formData.contrasena)) {
        errors.contrasena = 'Mín. 8 caracteres, mayúscula, minúscula, número y símbolo (@$!%*?&)'
      }

      if (!formData.confirmarContrasena?.trim()) {
        errors.confirmarContrasena = 'Debe confirmar la contraseña'
      } else if (formData.contrasena !== formData.confirmarContrasena) {
        errors.confirmarContrasena = 'Las contraseñas no coinciden'
      }
    }

    // Validar rol
    if (!formData.rol) {
      errors.rol = 'Debe seleccionar un rol'
    }

    return Object.keys(errors).length === 0
  }

  const handleEdit = (usuario) => {
    editingUsuarioId = usuario.id
    formData = {
      nombreUsuario: usuario.nombreUsuario || '',
      nombre: usuario.nombre || '',
      apellido: usuario.apellido || '',
      email: usuario.email || usuario.correo || '',
      contrasena: '',
      confirmarContrasena: '',
      rol: (usuario.rolId?.toString() || ''),
      estado: usuario.estado || 'Activo'
    }
    showFormModal = true
  }

  const handleDelete = async (usuario) => {
    const result = await Swal.fire({
      title: '¿Eliminar usuario?',
      text: `Se eliminará el usuario "${usuario.nombreUsuario}". Esta acción no se puede deshacer.`,
      icon: 'warning',
      showCancelButton: true,
      confirmButtonColor: '#ef4444',
      cancelButtonColor: '#6b7280',
      confirmButtonText: 'Sí, eliminar',
      cancelButtonText: 'Cancelar'
    })

    if (result.isConfirmed) {
      try {
        await usuarioService.delete(usuario.id)
        successMessage = 'Usuario eliminado correctamente'
        setTimeout(() => { successMessage = '' }, 3000)
        await loadUsuarios()
      } catch (error) {
        await Swal.fire('Error', error.message, 'error')
      }
    }
  }

  const handleSubmit = async () => {
    if (!validateForm()) return

    try {
      const dataToSend = {
        nombreUsuario: formData.nombreUsuario.trim().toLowerCase(),
        nombre: formData.nombre.trim(),
        apellido: formData.apellido.trim(),
        email: formData.email.trim(),
        rolId: parseInt(formData.rol) // Cambiar 'rol' a 'rolId' y convertir a número
      }

      console.log('[SUBMIT] ¿Es edición?', !!editingUsuarioId)
      console.log('[SUBMIT] formData completo:', formData)
      console.log('[SUBMIT] Datos a enviar ANTES de contrasena:', JSON.stringify(dataToSend))

      // Incluir contraseña solo en creación
      if (!editingUsuarioId) {
        dataToSend.contrasena = formData.contrasena
        console.log('[SUBMIT] CREACION - Contraseña agregada')
      }

      console.log('[SUBMIT] Datos FINALES a enviar:', JSON.stringify(dataToSend, null, 2))

      if (editingUsuarioId) {
        await usuarioService.update(editingUsuarioId, dataToSend)
        successMessage = 'Usuario actualizado correctamente'
      } else {
        await usuarioService.create(dataToSend)
        successMessage = 'Usuario creado correctamente'
      }
      
      setTimeout(() => { successMessage = '' }, 3000)
      showFormModal = false
      resetForm()
      await loadUsuarios()
    } catch (error) {
      let mensajeError = error.message
      
      // Limpiar mensajes de error del backend
      if (mensajeError.includes('Ya existe un usuario')) {
        mensajeError = 'El nombre de usuario ya se encuentra en uso'
      }
      
      await Swal.fire('Error', mensajeError, 'error')
    }
  }

  const resetForm = () => {
    formData = {
      nombreUsuario: '',
      nombre: '',
      apellido: '',
      email: '',
      contrasena: '',
      confirmarContrasena: '',
      rol: '',
      estado: 'Activo'
    }
    errors = {}
    editingUsuarioId = null
  }

  const handleNewUsuario = () => {
    resetForm()
    showFormModal = true
  }

  loadUsuarios()
</script>

<div class="usuarios-page">
  <div class="page-header">
    <h1><i class="fas fa-users"></i> Gestión de Usuarios</h1>
    {#if isAdmin}
      <button class="btn btn-primary" on:click={handleNewUsuario}>
        <i class="fas fa-plus"></i> Nuevo Usuario
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
        placeholder="Buscar por nombre usuario, nombre o email..."
        bind:value={searchTerm}
        on:input={() => filterUsuarios()}
      />
    </div>

    <div class="card-body">
      {#if loading}
        <div class="loading">Cargando usuarios...</div>
      {:else if usuarios.length === 0}
        <div class="empty-state">
          <i class="fas fa-users"></i>
          <p>No hay usuarios registrados</p>
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
                <th>NOMBRE USUARIO</th>
                <th>NOMBRE</th>
                <th>APELLIDO</th>
                <th>EMAIL</th>
                <th>ROL</th>
                <th>CREACIÓN</th>
                <th>ACCIONES</th>
              </tr>
            </thead>
            <tbody>
              {#each paginatedUsuarios as usuario (usuario.id)}
                <tr>
                  <td>
                    <strong>{usuario.nombreUsuario}</strong>
                  </td>
                  <td>{usuario.nombre || '-'}</td>
                  <td>{usuario.apellido || '-'}</td>
                  <td>{usuario.email || usuario.correo || '-'}</td>
                  <td>
                    <span class="badge badge-info">
                      {usuario.rolNombre || 'N/A'}
                    </span>
                  </td>
                  <td>
                    {formatters.formatDate(usuario.fechaCreacion)}
                  </td>
                  <td>
                    <div class="action-buttons">
                      {#if isAdmin}
                        <button
                          class="btn btn-sm btn-warning"
                          on:click={() => handleEdit(usuario)}
                          title="Editar"
                        >
                          <i class="fas fa-edit"></i> Editar
                        </button>
                        <button
                          class="btn btn-sm btn-danger"
                          on:click={() => handleDelete(usuario)}
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
          <p>Página {currentPage} de {totalPages} • Total: {filteredUsuarios.length} registros (10 por página)</p>
        </div>
      {/if}
    </div>
  </div>

  {#if showFormModal}
    <div class="modal-overlay" role="button" tabindex="0" on:click={() => { showFormModal = false }} on:keydown={(e) => { if (e.key === 'Escape') showFormModal = false; }}>
      <div class="modal-content" role="presentation" on:click|stopPropagation>
        <div class="modal-header">
          <h2 class="modal-title">
            {#if editingUsuarioId}
              Editar Usuario
            {:else}
              Nuevo Usuario
            {/if}
          </h2>
          <button class="modal-close" on:click={() => { showFormModal = false }}>
            <i class="fas fa-times"></i>
          </button>
        </div>

        <div class="modal-body">
          <div class="form-row">
            <FormInput
              label="Nombre de Usuario"
              name="nombreUsuario"
              bind:value={formData.nombreUsuario}
              placeholder="usuario_ejemplo"
              error={errors.nombreUsuario}
              required
              maxlength={20}
              disabled={editingUsuarioId ? true : false}
              on:keypress={(e) => {
                if (!/[a-zA-Z_-]/.test(e.key)) {
                  e.preventDefault()
                }
              }}
              on:input={(e) => {
                formData.nombreUsuario = formData.nombreUsuario.replace(/[^a-zA-Z_-]/g, '')
              }}
              on:paste={(e) => {
                e.preventDefault()
                const paste = (e.clipboardData || window.clipboardData).getData('text')
                formData.nombreUsuario = paste.replace(/[^a-zA-Z_-]/g, '')
              }}
            />
          </div>

          <div class="form-row">
            <FormInput
              label="Nombre"
              name="nombre"
              bind:value={formData.nombre}
              placeholder="Juan"
              error={errors.nombre}
              required
              maxlength={50}
            />
            <FormInput
              label="Apellido"
              name="apellido"
              bind:value={formData.apellido}
              placeholder="Pérez"
              error={errors.apellido}
              required
              maxlength={50}
            />
          </div>

          <div class="form-row">
            <FormInput
              label="Email"
              name="email"
              type="email"
              bind:value={formData.email}
              placeholder="usuario@ejemplo.com"
              error={errors.email}
              required
              on:keypress={(e) => {
                if (!/[a-zA-Z0-9.@]/.test(e.key)) {
                  e.preventDefault()
                }
              }}
              on:input={(e) => {
                formData.email = formData.email.replace(/[^a-zA-Z0-9.@]/g, '')
              }}
              on:paste={(e) => {
                e.preventDefault()
                const paste = (e.clipboardData || window.clipboardData).getData('text')
                formData.email = paste.replace(/[^a-zA-Z0-9.@]/g, '')
              }}
            />
          </div>

          {#if !editingUsuarioId}
            <div class="form-row">
              <FormInput
                label="Contraseña"
                name="contrasena"
                type="password"
                bind:value={formData.contrasena}
                placeholder="Pass123@"
                error={errors.contrasena}
                required
              />
              <FormInput
                label="Confirmar Contraseña"
                name="confirmarContrasena"
                type="password"
                bind:value={formData.confirmarContrasena}
                placeholder="Pass123@"
                error={errors.confirmarContrasena}
                required
              />
            </div>

            <div style="background: #f0f9ff; padding: 0.75rem; border-radius: 0.375rem; margin-bottom: 1rem; border-left: 4px solid #0284c7;">
              <p style="margin: 0; font-size: 0.875rem; color: #0c4a6e;">
                <i class="fas fa-info-circle"></i> La contraseña debe contener: mínimo 8 caracteres, mayúscula, minúscula, número y símbolo (@$!%*?&)
              </p>
            </div>
          {/if}

          <div class="form-row full-width">
            <div>
              <label for="rol" style="display: block; margin-bottom: 0.5rem; font-weight: 600; color: #0F172A;">
                Rol
                <span style="color: #ef4444;">*</span>
              </label>
              <select
                id="rol"
                bind:value={formData.rol}
                style="width: 100%; padding: 0.625rem; border: 1px solid {errors.rol ? '#ef4444' : '#E2E8F0'}; border-radius: 0.375rem; box-sizing: border-box;"
              >
                <option value="">Selecciona un rol...</option>
                {#each roles as rol}
                  <option value={rol.id}>{rol.nombre}</option>
                {/each}
              </select>
              {#if errors.rol}
                <div style="color: #ef4444; font-size: 0.875rem; margin-top: 0.25rem;">{errors.rol}</div>
              {/if}
            </div>
          </div>
        </div>

        <div class="modal-footer">
          <button class="btn btn-secondary" on:click={() => { showFormModal = false }}>
            Cancelar
          </button>
          <button class="btn btn-primary" on:click={handleSubmit}>
            {editingUsuarioId ? 'Actualizar' : 'Guardar'}
          </button>
        </div>
      </div>
    </div>
  {/if}
</div>

<style>
  .usuarios-page {
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

  .btn-primary {
    background: #3b82f6;
    color: white;
  }

  .btn-primary:hover {
    background: #2563eb;
  }

  .btn-warning {
    background: #fbbf24;
    color: white;
  }

  .btn-warning:hover {
    background: #f59e0b;
  }

  .btn-danger {
    background: #ef4444;
    color: white;
  }

  .btn-danger:hover {
    background: #dc2626;
  }

  .btn-secondary {
    background: #6b7280;
    color: white;
  }

  .btn-secondary:hover {
    background: #4b5563;
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
    border-radius: 8px;
    box-shadow: 0 10px 40px rgba(0, 0, 0, 0.2);
    max-width: 600px;
    width: 90%;
    max-height: 90vh;
    display: flex;
    flex-direction: column;
    overflow: hidden;
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
    font-size: 1.5rem;
    color: #1f2937;
    font-weight: 700;
  }

  .modal-close {
    background: none;
    border: none;
    font-size: 1.5rem;
    cursor: pointer;
    color: #6b7280;
    padding: 0;
    width: 2rem;
    height: 2rem;
    display: flex;
    align-items: center;
    justify-content: center;
    border-radius: 4px;
    transition: all 0.2s;
  }

  .modal-close:hover {
    background: #f3f4f6;
    color: #1f2937;
  }

  .modal-body {
    padding: 1.5rem;
    overflow-y: auto;
    flex: 1;
  }

  .form-row {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 1rem;
    margin-bottom: 1rem;
  }

  .form-row.full-width {
    grid-template-columns: 1fr;
  }

  .modal-footer {
    display: flex;
    justify-content: flex-end;
    gap: 0.5rem;
    padding: 1.5rem;
    border-top: 1px solid #e5e7eb;
  }

  input, select {
    width: 100%;
    padding: 0.625rem;
    border: 1px solid #e2e8f0;
    border-radius: 0.375rem;
    font-size: 0.875rem;
    box-sizing: border-box;
  }

  input:focus, select:focus {
    outline: none;
    border-color: #3b82f6;
    box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
  }

  input:disabled {
    background: #f3f4f6;
    cursor: not-allowed;
  }
</style>
