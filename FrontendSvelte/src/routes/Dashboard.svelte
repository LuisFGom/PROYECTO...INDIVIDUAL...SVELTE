<script>
  import { dataStore } from '../stores/store.js'
  import clienteService from '../services/clienteService.js'
  import productoService from '../services/productoService.js'
  import ventaService from '../services/ventaService.js'
  import { formatters } from '../utils/validators.js'
  import Swal from 'sweetalert2'

  let stats = {
    clientes: 0,
    productos: 0,
    ventas: 0,
    totalVentas: 0
  }
  let loading = true

  const loadStats = async () => {
    loading = true
    try {
      const [clientes, productos, ventas] = await Promise.all([
        clienteService.getAll(),
        productoService.getAll(),
        ventaService.getAll()
      ])

      stats = {
        clientes: (clientes || []).length,
        productos: (productos || []).length,
        ventas: (ventas || []).length,
        totalVentas: (ventas || []).reduce((sum, v) => sum + (v.total || 0), 0)
      }
    } catch (error) {
      await Swal.fire('Error', 'Error al cargar estadísticas: ' + error.message, 'error')
    } finally {
      loading = false
    }
  }

  loadStats()
</script>

<div class="dashboard">
  <div class="dashboard-header">
    <h1><i class="fas fa-chart-line"></i> Dashboard</h1>
    <p>Bienvenido al sistema de ventas NorthWind</p>
  </div>

  {#if loading}
    <div class="flex-center" style="height: 200px;">
      <div class="spinner spinner-lg"></div>
    </div>
  {:else}
    <div class="stats-grid">
      <div class="stat-card">
        <div class="stat-icon" style="background: #dbeafe;">
          <i class="fas fa-users" style="color: #3b82f6;"></i>
        </div>
        <div class="stat-content">
          <p class="stat-label">Clientes</p>
          <p class="stat-value">{stats.clientes}</p>
        </div>
      </div>

      <div class="stat-card">
        <div class="stat-icon" style="background: #d1fae5;">
          <i class="fas fa-box" style="color: #10b981;"></i>
        </div>
        <div class="stat-content">
          <p class="stat-label">Productos</p>
          <p class="stat-value">{stats.productos}</p>
        </div>
      </div>

      <div class="stat-card">
        <div class="stat-icon" style="background: #fef3c7;">
          <i class="fas fa-shopping-cart" style="color: #f59e0b;"></i>
        </div>
        <div class="stat-content">
          <p class="stat-label">Ventas</p>
          <p class="stat-value">{stats.ventas}</p>
        </div>
      </div>

      <div class="stat-card">
        <div class="stat-icon" style="background: #fee2e2;">
          <i class="fas fa-dollar-sign" style="color: #ef4444;"></i>
        </div>
        <div class="stat-content">
          <p class="stat-label">Total Ventas</p>
          <p class="stat-value">{formatters.formatCurrency(stats.totalVentas)}</p>
        </div>
      </div>
    </div>
  {/if}
</div>

<style>
  .dashboard {
    padding: 2rem;
  }

  .dashboard-header {
    margin-bottom: 2rem;
  }

  .dashboard-header h1 {
    margin: 0;
    font-size: 2rem;
    color: #1f2937;
    display: flex;
    align-items: center;
    gap: 0.5rem;
  }

  .dashboard-header p {
    margin: 0.5rem 0 0 0;
    color: #6b7280;
  }

  .stats-grid {
    display: grid;
    grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
    gap: 1.5rem;
  }

  .stat-card {
    background: white;
    border-radius: 0.75rem;
    padding: 1.5rem;
    display: flex;
    align-items: center;
    gap: 1.5rem;
    box-shadow: 0 1px 3px rgba(0, 0, 0, 0.1);
    transition: transform 0.2s, box-shadow 0.2s;
  }

  .stat-card:hover {
    transform: translateY(-2px);
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.1);
  }

  .stat-icon {
    width: 60px;
    height: 60px;
    border-radius: 0.5rem;
    display: flex;
    align-items: center;
    justify-content: center;
    font-size: 1.5rem;
    flex-shrink: 0;
  }

  .stat-label {
    margin: 0;
    font-size: 0.875rem;
    color: #6b7280;
    text-transform: uppercase;
    letter-spacing: 0.5px;
  }

  .stat-value {
    margin: 0.5rem 0 0 0;
    font-size: 1.5rem;
    font-weight: 700;
    color: #1f2937;
  }
</style>
