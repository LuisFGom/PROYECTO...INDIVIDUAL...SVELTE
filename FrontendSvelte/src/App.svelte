<script>
  import { onMount } from 'svelte'
  import { authStore, setCurrentPage, setAuthState, logout } from './stores/store.js'
  import authService from './services/authService.js'
  import './app.css'

  import Login from './routes/Login.svelte'
  import Dashboard from './routes/Dashboard.svelte'
  import Clientes from './routes/Clientes.svelte'
  import Productos from './routes/Productos.svelte'
  import Ventas from './routes/Ventas.svelte'
  import Usuarios from './routes/Usuarios.svelte'
  import ElimUsuarios from './routes/ElimUsuarios.svelte'
  import ElimClientes from './routes/ElimClientes.svelte'
  import ElimProductos from './routes/ElimProductos.svelte'
  import ElimVentas from './routes/ElimVentas.svelte'

  import Logs from './routes/Logs.svelte'

  let authState = {}
  let currentPage = 'login'
  let sidebarOpen = true

  onMount(() => {
    // Suscribirse a cambios de autenticación
    const unsubAuth = authStore.subscribe(value => {
      authState = value
    })

    // Verificar si hay sesión activa
    const token = authService.getToken()
    const user = authService.getUser()

    if (token && user) {
      setAuthState({
        isAuthenticated: true,
        user,
        token,
        isLoading: false
      })
      currentPage = 'dashboard'
    } else {
      currentPage = 'login'
    }

    return unsubAuth
  })

  const handleNavigate = (page) => {
    currentPage = page
  }

  const handleLogout = () => {
    authService.logout()
    logout()
    currentPage = 'login'
  }

  const toggleSidebar = () => {
    sidebarOpen = !sidebarOpen
  }
</script>

<main>
  {#if !authState.isAuthenticated}
    <Login />

  {:else if authState.isLoading}
    <div class="flex-center" style="height: 100vh;">
      <div class="spinner spinner-lg"></div>
    </div>

  {:else}
    <!-- Layout autenticado -->
    <div style="display: flex; height: 100vh; background: #f3f4f6;">
      
      <!-- Sidebar -->
      <nav class="sidebar {sidebarOpen ? 'open' : 'closed'}">
        <div class="sidebar-header">
          <h1>PuntoVenta</h1>
          <p>Sistema de Ventas</p>
        </div>

        <div class="sidebar-user">
          <p style="margin: 0; font-size: 0.875rem; color: rgba(255, 255, 255, 0.6);">Usuario</p>
          <p style="margin: 0.5rem 0 0 0; font-weight: 600; color: white; overflow: hidden; text-overflow: ellipsis;">
            {authState.user?.nombre || ''} {authState.user?.apellido || ''}
          </p>
        </div>

        <ul class="sidebar-menu">
          <li>
            <button
              on:click={() => handleNavigate('dashboard')}
              class="menu-item {currentPage === 'dashboard' ? 'active' : ''}"
            >
              <i class="fas fa-chart-line"></i>
              <span>Dashboard</span>
            </button>
          </li>

          <li>
            <button
              on:click={() => handleNavigate('clientes')}
              class="menu-item {currentPage === 'clientes' ? 'active' : ''}"
            >
              <i class="fas fa-users"></i>
              <span>Clientes</span>
            </button>
          </li>

          <li>
            <button
              on:click={() => handleNavigate('productos')}
              class="menu-item {currentPage === 'productos' ? 'active' : ''}"
            >
              <i class="fas fa-box"></i>
              <span>Productos</span>
            </button>
          </li>

          <li>
            <button
              on:click={() => handleNavigate('ventas')}
              class="menu-item {currentPage === 'ventas' ? 'active' : ''}"
            >
              <i class="fas fa-shopping-cart"></i>
              <span>Facturas</span>
            </button>
          </li>

          <!-- Separador -->
          <li class="menu-separator"></li>

          <li>
            <button
              on:click={() => handleNavigate('usuarios')}
              class="menu-item {currentPage === 'usuarios' ? 'active' : ''}"
            >
              <i class="fas fa-user"></i>
              <span>Usuarios</span>
            </button>
          </li>

          <li>
            <button
              on:click={() => handleNavigate('elim-usuarios')}
              class="menu-item {currentPage === 'elim-usuarios' ? 'active' : ''}"
            >
              <i class="fas fa-user-slash"></i>
              <span>Usuarios Desactivados</span>
            </button>
          </li>

          <li>
            <button
              on:click={() => handleNavigate('elim-clientes')}
              class="menu-item {currentPage === 'elim-clientes' ? 'active' : ''}"
            >
              <i class="fas fa-user-slash"></i>
              <span>Clientes Desactivados</span>
            </button>
          </li>

          <li>
            <button
              on:click={() => handleNavigate('elim-productos')}
              class="menu-item {currentPage === 'elim-productos' ? 'active' : ''}"
            >
              <i class="fas fa-box"></i>
              <span>Productos Desactivados</span>
            </button>
          </li>

          <li>
            <button
              on:click={() => handleNavigate('elim-ventas')}
              class="menu-item {currentPage === 'elim-ventas' ? 'active' : ''}"
            >
              <i class="fas fa-file-invoice"></i>
              <span>Facturas Anuladas</span>
            </button>
          </li>



          <!-- Separador -->
          <li class="menu-separator"></li>

          <li>
            <button
              on:click={() => handleNavigate('logs')}
              class="menu-item {currentPage === 'logs' ? 'active' : ''}"
            >
              <i class="fas fa-exclamation-triangle"></i>
              <span>Logs</span>
            </button>
          </li>
        </ul>

        <div class="sidebar-footer">
          <button class="btn btn-danger" style="width: 100%;" on:click={handleLogout}>
            <i class="fas fa-sign-out-alt"></i>
            <span>Cerrar sesión</span>
          </button>
        </div>
      </nav>

      <!-- Contenido -->
      <div style="flex: 1; display: flex; flex-direction: column; overflow: hidden;">
        <header class="app-header">
          <button class="sidebar-toggle" on:click={toggleSidebar}>
            <i class="fas fa-bars"></i>
          </button>
          <div style="flex: 1;"></div>
        </header>

        <div style="flex: 1; overflow-y: auto;">
          {#if currentPage === 'dashboard'}
            <Dashboard />
          {:else if currentPage === 'clientes'}
            <Clientes />
          {:else if currentPage === 'productos'}
            <Productos />
          {:else if currentPage === 'ventas'}
            <Ventas />
          {:else if currentPage === 'usuarios'}
            <Usuarios />
          {:else if currentPage === 'elim-usuarios'}
            <ElimUsuarios />
          {:else if currentPage === 'elim-clientes'}
            <ElimClientes />
          {:else if currentPage === 'elim-productos'}
            <ElimProductos />
          {:else if currentPage === 'elim-ventas'}
            <ElimVentas />

          {:else if currentPage === 'logs'}
            <Logs />
          {/if}
        </div>
      </div>
    </div>
  {/if}
</main>

<style>
  main {
    width: 100%;
    height: 100%;
  }

  .sidebar {
    width: 256px;
    background: linear-gradient(180deg, #3b82f6 0%, #2563eb 100%);
    color: white;
    display: flex;
    flex-direction: column;
    transition: transform 200ms ease;
    overflow-y: auto;
    box-shadow: 2px 0 8px rgba(0, 0, 0, 0.1);
  }

  .sidebar.closed {
    transform: translateX(-100%);
  }

  .sidebar-header {
    padding: 1.5rem;
    border-bottom: 1px solid rgba(255, 255, 255, 0.1);
  }

  .sidebar-header h1 {
    margin: 0;
    font-size: 1.5rem;
    font-weight: 700;
  }

  .sidebar-header p {
    margin: 0.5rem 0 0 0;
    font-size: 0.75rem;
    color: rgba(255, 255, 255, 0.7);
  }

  .sidebar-user {
    padding: 1rem 1.5rem;
    border-bottom: 1px solid rgba(255, 255, 255, 0.1);
  }

  .sidebar-menu {
    flex: 1;
    list-style: none;
    margin: 0;
    padding: 1rem 0;
    overflow-y: auto;
  }

  .sidebar-menu li {
    margin: 0;
  }

  .menu-separator {
    height: 1px;
    background: rgba(255, 255, 255, 0.2);
    margin: 0.5rem 0;
    list-style: none;
  }

  .menu-item {
    display: flex;
    align-items: center;
    gap: 0.75rem;
    width: 100%;
    padding: 0.75rem 1.5rem;
    background: none;
    border: none;
    color: rgba(255, 255, 255, 0.8);
    cursor: pointer;
    transition: all 200ms ease;
    text-align: left;
    font-size: 0.95rem;
  }

  .menu-item:hover {
    background: rgba(255, 255, 255, 0.1);
    color: white;
  }

  .menu-item.active {
    background: rgba(255, 255, 255, 0.2);
    color: white;
    border-left: 3px solid white;
  }

  .menu-item i {
    width: 20px;
    text-align: center;
  }

  .sidebar-footer {
    padding: 1.5rem;
    border-top: 1px solid rgba(255, 255, 255, 0.1);
  }

  .app-header {
    display: flex;
    align-items: center;
    gap: 1rem;
    padding: 1rem;
    background: white;
    border-bottom: 1px solid #e5e7eb;
    box-shadow: 0 1px 3px rgba(0, 0, 0, 0.05);
  }

  .sidebar-toggle {
    display: none;
    background: none;
    border: none;
    font-size: 1.25rem;
    cursor: pointer;
    color: #6b7280;
  }

  @media (max-width: 768px) {
    .sidebar {
      position: fixed;
      height: 100vh;
      z-index: 100;
    }

    .sidebar-toggle {
      display: block;
    }
  }
</style>
