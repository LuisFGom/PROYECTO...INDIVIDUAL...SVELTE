<script>
  import { setAuthState, setCurrentPage } from '../stores/store.js'
  import authService from '../services/authService.js'
  import { validators } from '../utils/validators.js'
  import FormInput from '../components/FormInput.svelte'
  import Swal from 'sweetalert2'

  // LOGIN
  let email = ''
  let contrasena = ''
  let showPassword = false
  let loading = false
  let loginErrors = {}

  // REGISTRO
  let showRegisterModal = false
  let regEmail = ''
  let regNombreUsuario = ''
  let regNombre = ''
  let regApellido = ''
  let regContrasena = ''
  let regConfirmContrasena = ''
  let regShowPassword = false
  let regShowConfirmPassword = false
  let regLoading = false
  let regErrors = {}

  const validateLoginForm = () => {
    loginErrors = {}
    if (!email) {
      loginErrors.email = 'El email es requerido'
    } else if (!validators.isEmail(email)) {
      loginErrors.email = 'Ingresa un email válido'
    }
    if (!contrasena) {
      loginErrors.contrasena = 'La contraseña es requerida'
    }
    return Object.keys(loginErrors).length === 0
  }

  const validateRegisterForm = () => {
    regErrors = {}
    
    // Email
    const emailRegex = /^[a-zA-Z0-9._%-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/
    if (!regEmail) {
      regErrors.email = 'El email es requerido'
    } else if (!emailRegex.test(regEmail)) {
      regErrors.email = 'Por favor ingresa un email válido (ej: juan@ejemplo.com)'
    }

    // Nombre de Usuario
    const soloLetrasRegex = /^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$/
    if (!regNombreUsuario) {
      regErrors.nombreUsuario = 'El nombre de usuario es requerido'
    } else if (!soloLetrasRegex.test(regNombreUsuario)) {
      regErrors.nombreUsuario = 'El nombre de usuario solo puede contener letras'
    } else if (regNombreUsuario.length < 3) {
      regErrors.nombreUsuario = 'Debe tener al menos 3 caracteres'
    } else if (regNombreUsuario.length > 20) {
      regErrors.nombreUsuario = 'No puede exceder 20 caracteres'
    }

    // Nombre
    if (!regNombre) {
      regErrors.nombre = 'El nombre es requerido'
    } else if (!soloLetrasRegex.test(regNombre)) {
      regErrors.nombre = 'El nombre solo puede contener letras'
    } else if (regNombre.length < 2) {
      regErrors.nombre = 'Debe tener al menos 2 caracteres'
    } else if (regNombre.length > 20) {
      regErrors.nombre = 'No puede exceder 20 caracteres'
    }

    // Apellido
    if (!regApellido) {
      regErrors.apellido = 'El apellido es requerido'
    } else if (!soloLetrasRegex.test(regApellido)) {
      regErrors.apellido = 'El apellido solo puede contener letras'
    } else if (regApellido.length < 2) {
      regErrors.apellido = 'Debe tener al menos 2 caracteres'
    } else if (regApellido.length > 20) {
      regErrors.apellido = 'No puede exceder 20 caracteres'
    }

    // Contraseña
    const contraseniaRegex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]+$/
    if (!regContrasena) {
      regErrors.contrasena = 'La contraseña es requerida'
    } else if (regContrasena.length < 8) {
      regErrors.contrasena = 'La contraseña debe tener al menos 8 caracteres'
    } else if (!contraseniaRegex.test(regContrasena)) {
      regErrors.contrasena = 'Debe contener: mayúscula, minúscula, número y carácter especial (@$!%*?&)'
    }

    // Confirmar Contraseña
    if (!regConfirmContrasena) {
      regErrors.confirmContrasena = 'Debe confirmar la contraseña'
    } else if (regContrasena !== regConfirmContrasena) {
      regErrors.confirmContrasena = 'Las contraseñas no coinciden'
    }

    return Object.keys(regErrors).length === 0
  }

  const handleLogin = async (e) => {
    e.preventDefault()
    if (!validateLoginForm()) return

    loading = true
    try {
      const response = await authService.login(email, contrasena)
      
      if (response && response.token) {
        setAuthState({
          isAuthenticated: true,
          user: response.usuario,
          token: response.token,
          error: null
        })
        setCurrentPage('dashboard')
      }
    } catch (error) {
      await Swal.fire({
        icon: 'error',
        title: 'Error en el login',
        text: error.message
      })
    } finally {
      loading = false
    }
  }

  const handleRegister = async (e) => {
    e.preventDefault()
    if (!validateRegisterForm()) return

    regLoading = true
    try {
      const response = await authService.registro({
        Email: regEmail,
        NombreUsuario: regNombreUsuario,
        Nombre: regNombre,
        Apellido: regApellido,
        Contrasena: regContrasena
      })

      await Swal.fire({
        icon: 'success',
        title: '¡Cuenta Creada!',
        text: 'Tu cuenta ha sido creada exitosamente. Redirigiendo a login en 2 segundos...',
        confirmButtonColor: '#10B981',
        allowOutsideClick: false,
        didOpen: () => {
          setTimeout(() => {
            Swal.close()
            closeRegisterModal()
          }, 2000)
        }
      })
    } catch (error) {
      await Swal.fire({
        icon: 'error',
        title: 'Error al Crear Cuenta',
        text: error.message || 'Error al crear la cuenta. Por favor intenta nuevamente.',
        confirmButtonColor: '#EF4444'
      })
    } finally {
      regLoading = false
    }
  }

  const openRegisterModal = () => {
    showRegisterModal = true
    regErrors = {}
  }

  const closeRegisterModal = () => {
    showRegisterModal = false
    regEmail = ''
    regNombreUsuario = ''
    regNombre = ''
    regApellido = ''
    regContrasena = ''
    regConfirmContrasena = ''
    regShowPassword = false
    regShowConfirmPassword = false
    regErrors = {}
  }

  const handleNombreUsuarioInput = (e) => {
    let valor = e.target.value
    // Solo letras (incluyendo acentos)
    valor = valor.replace(/[^a-zA-ZáéíóúÁÉÍÓÚñÑ\s]/g, '')
    // Limitar a 20 caracteres
    valor = valor.substring(0, 20)
    regNombreUsuario = valor
  }

  const handleNombreInput = (e) => {
    let valor = e.target.value
    valor = valor.replace(/[^a-zA-ZáéíóúÁÉÍÓÚñÑ\s]/g, '')
    valor = valor.substring(0, 20)
    regNombre = valor
  }

  const handleApellidoInput = (e) => {
    let valor = e.target.value
    valor = valor.replace(/[^a-zA-ZáéíóúÁÉÍÓÚñÑ\s]/g, '')
    valor = valor.substring(0, 20)
    regApellido = valor
  }
</script>

<div class="login-container">
  <div class="login-card">
    <div class="login-header">
      <div class="login-logo">💼</div>
      <h1>PuntoVenta</h1>
      <p>Sistema de Gestión Comercial</p>
    </div>

    <form on:submit={handleLogin}>
      <div class="form-group">
        <label for="email">Email</label>
        <input 
          type="email" 
          id="email" 
          name="email" 
          placeholder="Ingrese su email"
          bind:value={email}
          required
          autocomplete="email"
        />
        {#if loginErrors.email}
          <div class="error-text">{loginErrors.email}</div>
        {/if}
      </div>

      <div class="form-group">
        <label for="contrasena">Contraseña</label>
        <div class="password-container">
          {#if showPassword}
            <input 
              type="text"
              id="contrasena" 
              name="contrasena" 
              placeholder="Ingrese su contraseña"
              bind:value={contrasena}
              required
              autocomplete="current-password"
            />
          {:else}
            <input 
              type="password"
              id="contrasena" 
              name="contrasena" 
              placeholder="Ingrese su contraseña"
              bind:value={contrasena}
              required
              autocomplete="current-password"
            />
          {/if}
          <button 
            type="button" 
            class="toggle-password" 
            on:click={() => showPassword = !showPassword}
            aria-label="Mostrar/ocultar contraseña"
          >
            <i class="fas {showPassword ? 'fa-eye-slash' : 'fa-eye'}"></i>
          </button>
        </div>
        {#if loginErrors.contrasena}
          <div class="error-text">{loginErrors.contrasena}</div>
        {/if}
      </div>

      <button type="submit" class="login-btn" disabled={loading}>
        {#if loading}
          <i class="fas fa-spinner fa-spin"></i> Iniciando sesión...
        {:else}
          <i class="fas fa-sign-in-alt"></i> Iniciar Sesión
        {/if}
      </button>
    </form>

    <div class="demo-credentials">
      <p><strong>Credenciales de Demostración:</strong></p>
      <p>Email: <strong>admin@test.com</strong></p>
      <p>Contraseña: <strong>Password123!</strong></p>
    </div>

    <div class="login-footer">
      <p>¿No tienes cuenta? <button type="button" class="register-link" on:click={openRegisterModal}>Registrate aquí</button></p>
    </div>
  </div>
</div>

<!-- Modal de Registro -->
{#if showRegisterModal}
<div class="modal-overlay" role="button" tabindex="0" on:click={closeRegisterModal} on:keydown={(e) => { if (e.key === 'Escape') closeRegisterModal(); }}>
  <div class="modal-content" role="presentation" on:click|stopPropagation>
    <div class="modal-header">
      <h2>Crear Cuenta</h2>
      <button type="button" class="modal-close" on:click={closeRegisterModal} aria-label="Cerrar modal">
        <i class="fas fa-times"></i>
      </button>
    </div>

    <form on:submit={handleRegister}>
      <div class="form-group">
        <label for="regEmail">Email</label>
        <input 
          type="email" 
          id="regEmail" 
          name="email" 
          placeholder="Ejemplo: usuario@ejemplo.com"
          bind:value={regEmail}
          required
          autocomplete="email"
        />
        {#if regErrors.email}
          <div class="error-text">{regErrors.email}</div>
        {/if}
      </div>

      <div class="form-group">
        <label for="regNombreUsuario">Nombre de Usuario</label>
        <input 
          type="text" 
          id="regNombreUsuario" 
          name="nombreUsuario" 
          placeholder="Ejemplo: Juan (3-20 letras, sin números)"
          bind:value={regNombreUsuario}
          on:input={handleNombreUsuarioInput}
          maxlength="20"
          required
        />
        {#if regErrors.nombreUsuario}
          <div class="error-text">{regErrors.nombreUsuario}</div>
        {/if}
      </div>

      <div class="form-group">
        <label for="regNombre">Nombre</label>
        <input 
          type="text" 
          id="regNombre" 
          name="nombre" 
          placeholder="Ejemplo: Juan Carlos (2-20 letras)"
          bind:value={regNombre}
          on:input={handleNombreInput}
          maxlength="20"
          required
        />
        {#if regErrors.nombre}
          <div class="error-text">{regErrors.nombre}</div>
        {/if}
      </div>

      <div class="form-group">
        <label for="regApellido">Apellido (Requerido)</label>
        <input 
          type="text" 
          id="regApellido" 
          name="apellido" 
          placeholder="Ejemplo: Pérez (2-20 letras)"
          bind:value={regApellido}
          on:input={handleApellidoInput}
          maxlength="20"
          required
        />
        {#if regErrors.apellido}
          <div class="error-text">{regErrors.apellido}</div>
        {/if}
      </div>

      <div class="form-group">
        <label for="regContrasena">Contraseña</label>
        <div class="password-container">
          {#if regShowPassword}
            <input 
              type="text"
              id="regContrasena" 
              name="contrasena" 
              placeholder="Ejemplo: Pass123@ (mayús, minús, número, símbolo)"
              bind:value={regContrasena}
              required
            />
          {:else}
            <input 
              type="password"
              id="regContrasena" 
              name="contrasena" 
              placeholder="Ejemplo: Pass123@ (mayús, minús, número, símbolo)"
              bind:value={regContrasena}
              required
            />
          {/if}
          <button 
            type="button" 
            class="toggle-password" 
            on:click={() => regShowPassword = !regShowPassword}
            aria-label="Mostrar/ocultar contraseña"
          >
            <i class="fas {regShowPassword ? 'fa-eye-slash' : 'fa-eye'}"></i>
          </button>
        </div>
        {#if regErrors.contrasena}
          <div class="error-text">{regErrors.contrasena}</div>
        {/if}
      </div>

      <div class="form-group">
        <label for="regConfirmContrasena">Confirmar Contraseña</label>
        <div class="password-container">
          {#if regShowConfirmPassword}
            <input 
              type="text"
              id="regConfirmContrasena" 
              name="confirmContrasena" 
              placeholder="Ejemplo: Pass123@ (debe ser igual a la anterior)"
              bind:value={regConfirmContrasena}
              required
            />
          {:else}
            <input 
              type="password"
              id="regConfirmContrasena" 
              name="confirmContrasena" 
              placeholder="Ejemplo: Pass123@ (debe ser igual a la anterior)"
              bind:value={regConfirmContrasena}
              required
            />
          {/if}
          <button 
            type="button" 
            class="toggle-password" 
            on:click={() => regShowConfirmPassword = !regShowConfirmPassword}
            aria-label="Mostrar/ocultar contraseña"
          >
            <i class="fas {regShowConfirmPassword ? 'fa-eye-slash' : 'fa-eye'}"></i>
          </button>
        </div>
        {#if regErrors.confirmContrasena}
          <div class="error-text">{regErrors.confirmContrasena}</div>
        {/if}
      </div>

      <button type="submit" class="register-btn" disabled={regLoading}>
        {#if regLoading}
          <i class="fas fa-spinner fa-spin"></i> Creando cuenta...
        {:else}
          <i class="fas fa-user-plus"></i> Crear Cuenta
        {/if}
      </button>
    </form>
  </div>
</div>
{/if}

<style>
  .login-container {
    display: flex;
    align-items: center;
    justify-content: center;
    min-height: 100vh;
    background: linear-gradient(135deg, #1E293B 0%, #3B82F6 100%);
    padding: 1rem;
  }

  .login-card {
    background-color: white;
    border-radius: 12px;
    box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.3);
    padding: 2rem;
    width: 100%;
    max-width: 400px;
  }

  .login-header {
    text-align: center;
    margin-bottom: 2rem;
  }

  .login-logo {
    font-size: 3rem;
    margin-bottom: 1rem;
  }

  .login-header h1 {
    font-size: 1.75rem;
    color: #1E293B;
    margin: 0;
    margin-bottom: 0.5rem;
  }

  .login-header p {
    color: #64748B;
    margin: 0;
    font-size: 0.875rem;
  }

  .form-group {
    margin-bottom: 1.25rem;
  }

  .form-group label {
    display: block;
    margin-bottom: 0.5rem;
    font-weight: 600;
    color: #1E293B;
    font-size: 0.875rem;
  }

  .form-group input {
    width: 100%;
    padding: 0.75rem 1rem;
    border: 1px solid #E2E8F0;
    border-radius: 8px;
    font-size: 0.875rem;
    transition: all 250ms ease-in-out;
    box-sizing: border-box;
  }

  .form-group input:focus {
    outline: none;
    border-color: #3B82F6;
    box-shadow: 0 0 0 3px rgba(59, 130, 246, 0.1);
  }

  .password-container {
    position: relative;
    display: flex;
    align-items: center;
    width: 100%;
  }

  .password-container input {
    width: 100%;
    margin: 0;
    padding-right: 2.75rem !important;
  }

  .toggle-password {
    position: absolute;
    right: 0.5rem;
    top: 50%;
    transform: translateY(-50%);
    background: linear-gradient(135deg, #E0F2FE 0%, #F0F9FF 100%);
    border: 1px solid #BAE6FD;
    color: #0284C7;
    cursor: pointer;
    font-size: 1rem;
    padding: 0.6rem 0.8rem;
    transition: all 250ms ease-in-out;
    display: flex;
    align-items: center;
    justify-content: center;
    z-index: 10;
    border-radius: 6px;
    font-weight: 500;
  }

  .toggle-password:hover {
    background: linear-gradient(135deg, #BAE6FD 0%, #7DD3FC 100%);
    color: #0C4A6E;
    box-shadow: 0 2px 8px rgba(2, 132, 199, 0.2);
    transform: translateY(-50%) scale(1.05);
  }

  .toggle-password:active {
    transform: translateY(-50%) scale(0.95);
  }

  .login-btn {
    width: 100%;
    padding: 0.75rem;
    background-color: #3B82F6;
    color: white;
    border: none;
    border-radius: 8px;
    font-size: 0.875rem;
    font-weight: 600;
    cursor: pointer;
    transition: all 250ms ease-in-out;
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 0.5rem;
  }

  .login-btn:hover:not(:disabled) {
    background-color: #2563EB;
    transform: translateY(-1px);
    box-shadow: 0 4px 6px -1px rgba(59, 130, 246, 0.3);
  }

  .login-btn:disabled {
    opacity: 0.5;
    cursor: not-allowed;
  }

  .error-text {
    padding: 0.5rem 0.75rem;
    background-color: rgba(239, 68, 68, 0.1);
    border: 1px solid rgba(239, 68, 68, 0.3);
    border-radius: 6px;
    color: #DC2626;
    font-size: 0.75rem;
    margin-top: 0.25rem;
  }

  .demo-credentials {
    background-color: #F0F9FF;
    border: 1px solid #BAE6FD;
    border-radius: 8px;
    padding: 1rem;
    margin-top: 1.5rem;
    font-size: 0.75rem;
    color: #0369A1;
  }

  .demo-credentials p {
    margin: 0.25rem 0;
  }

  .demo-credentials strong {
    color: #0C4A6E;
  }

  .login-footer {
    margin-top: 1rem;
    text-align: center;
  }

  .register-link {
    background: none;
    border: none;
    color: #3B82F6;
    cursor: pointer;
    font-weight: 600;
    font-size: 0.875rem;
    transition: color 250ms ease-in-out;
    padding: 0;
  }

  .register-link:hover {
    color: #2563EB;
    text-decoration: underline;
  }

  /* Modal Styles */
  .modal-overlay {
    display: flex;
    position: fixed;
    top: 0;
    left: 0;
    right: 0;
    bottom: 0;
    background-color: rgba(0, 0, 0, 0.5);
    z-index: 1000;
    justify-content: center;
    align-items: center;
    padding: 1rem;
  }

  .modal-content {
    background-color: white;
    border-radius: 12px;
    box-shadow: 0 20px 25px -5px rgba(0, 0, 0, 0.3);
    width: 100%;
    max-width: 500px;
    max-height: 90vh;
    overflow-y: auto;
    padding: 2rem;
    position: relative;
  }

  .modal-header {
    display: flex;
    justify-content: space-between;
    align-items: center;
    margin-bottom: 1.5rem;
  }

  .modal-header h2 {
    margin: 0;
    color: #1E293B;
    font-size: 1.5rem;
  }

  .modal-close {
    background: none;
    border: none;
    font-size: 1.5rem;
    color: #64748B;
    cursor: pointer;
    padding: 0;
    display: flex;
    align-items: center;
    justify-content: center;
    transition: color 250ms ease-in-out;
  }

  .modal-close:hover {
    color: #1E293B;
  }

  .register-btn {
    width: 100%;
    padding: 0.875rem;
    background: linear-gradient(135deg, #10B981 0%, #059669 100%);
    color: white;
    border: none;
    border-radius: 8px;
    font-size: 0.875rem;
    font-weight: 600;
    cursor: pointer;
    transition: all 250ms ease-in-out;
    display: flex;
    align-items: center;
    justify-content: center;
    gap: 0.5rem;
    box-shadow: 0 2px 8px rgba(16, 185, 129, 0.2);
  }

  .register-btn:hover:not(:disabled) {
    background: linear-gradient(135deg, #059669 0%, #047857 100%);
    transform: translateY(-2px);
    box-shadow: 0 6px 16px rgba(16, 185, 129, 0.3);
  }

  .register-btn:active:not(:disabled) {
    transform: translateY(0);
  }

  .register-btn:disabled {
    opacity: 0.6;
    cursor: not-allowed;
    transform: none;
  }
</style>
