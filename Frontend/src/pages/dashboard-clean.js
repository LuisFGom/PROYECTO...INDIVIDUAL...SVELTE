import { store } from '../js/store.js'
import { clienteService } from '../services/clienteService.js'
import { productoService } from '../services/productoService.js'
import { ventaService } from '../services/ventaService.js'

export class Page {
  constructor() {
    this.stats = {
      totalClientes: 0,
      totalProductos: 0,
      totalVentas: 0,
      productosStockBajo: 0
    }
  }

  render() {
    const user = store.getState().currentUser
    return `
<div class="dashboard-container" style="padding: 20px;">
  <div class="page-header">
    <h1 class="page-title" style="margin-bottom: 30px; font-size: 28px; color: #1E293B;"><i class="fas fa-chart-line"></i> Dashboard</h1>
  </div>

  <div class="stats-grid" style="display: grid; grid-template-columns: repeat(auto-fit, minmax(250px, 1fr)); gap: 20px; margin-bottom: 30px;">
    <div class="stat-card stat-clientes" style="background: linear-gradient(135deg, #667eea 0%, #764ba2 100%); color: white; padding: 25px; border-radius: 12px; box-shadow: 0 8px 16px rgba(0,0,0,0.1);">
      <div style="display: flex; justify-content: space-between; align-items: flex-start;">
        <div>
          <h3 style="font-size: 36px; margin: 0; font-weight: bold;">${this.stats.totalClientes}</h3>
          <p style="margin: 8px 0 0 0; font-size: 14px; opacity: 0.9;">Total Clientes</p>
        </div>
        <i class="fas fa-users" style="font-size: 32px; opacity: 0.2;"></i>
      </div>
    </div>

    <div class="stat-card stat-productos" style="background: linear-gradient(135deg, #f093fb 0%, #f5576c 100%); color: white; padding: 25px; border-radius: 12px; box-shadow: 0 8px 16px rgba(0,0,0,0.1);">
      <div style="display: flex; justify-content: space-between; align-items: flex-start;">
        <div>
          <h3 style="font-size: 36px; margin: 0; font-weight: bold;">${this.stats.totalProductos}</h3>
          <p style="margin: 8px 0 0 0; font-size: 14px; opacity: 0.9;">Total Productos</p>
        </div>
        <i class="fas fa-box" style="font-size: 32px; opacity: 0.2;"></i>
      </div>
    </div>

    <div class="stat-card stat-ventas" style="background: linear-gradient(135deg, #4facfe 0%, #00f2fe 100%); color: white; padding: 25px; border-radius: 12px; box-shadow: 0 8px 16px rgba(0,0,0,0.1);">
      <div style="display: flex; justify-content: space-between; align-items: flex-start;">
        <div>
          <h3 style="font-size: 36px; margin: 0; font-weight: bold;">$${this.stats.totalVentas.toFixed(2)}</h3>
          <p style="margin: 8px 0 0 0; font-size: 14px; opacity: 0.9;">Ventas del Mes</p>
        </div>
        <i class="fas fa-receipt" style="font-size: 32px; opacity: 0.2;"></i>
      </div>
    </div>

    <div class="stat-card stat-stock" style="background: linear-gradient(135deg, #fa709a 0%, #fee140 100%); color: #333; padding: 25px; border-radius: 12px; box-shadow: 0 8px 16px rgba(0,0,0,0.1);">
      <div style="display: flex; justify-content: space-between; align-items: flex-start;">
        <div>
          <h3 style="font-size: 36px; margin: 0; font-weight: bold;">${this.stats.productosStockBajo}</h3>
          <p style="margin: 8px 0 0 0; font-size: 14px; opacity: 0.9;">Stock Bajo</p>
        </div>
        <i class="fas fa-triangle-exclamation" style="font-size: 32px; opacity: 0.2;"></i>
      </div>
    </div>
  </div>

  <div class="welcome-section" style="background: white; padding: 30px; border-radius: 12px; box-shadow: 0 2px 8px rgba(0,0,0,0.05);">
    <div style="margin-bottom: 25px;">
      <h3 style="margin: 0 0 15px 0; color: #1E293B; font-size: 20px;"><i class="fas fa-hand-wave"></i> Bienvenido al Sistema</h3>
      <div style="display: grid; grid-template-columns: repeat(3, 1fr); gap: 15px;">
        <p style="margin: 0; color: #64748B;"><i class="fas fa-user" style="color: #3B82F6; margin-right: 8px;"></i> <strong>Usuario:</strong> ${user?.nombreUsuario || 'Usuario'}</p>
        <p style="margin: 0; color: #64748B;"><i class="fas fa-user-shield" style="color: #3B82F6; margin-right: 8px;"></i> <strong>Rol:</strong> ${user?.rolNombre || 'Usuario'}</p>
        <p style="margin: 0; color: #64748B;"><i class="fas fa-calendar-day" style="color: #3B82F6; margin-right: 8px;"></i> <strong>Fecha:</strong> ${new Date().toLocaleDateString('es-EC')}</p>
      </div>
    </div>

    <div class="quick-actions-section">
      <h4 style="margin: 20px 0 15px 0; color: #1E293B;"><i class="fas fa-bolt" style="color: #F59E0B; margin-right: 8px;"></i> Accesos Rápidos</h4>
      <div class="quick-actions" style="display: grid; grid-template-columns: repeat(auto-fit, minmax(150px, 1fr)); gap: 12px;">
        <button class="btn-quick-action" data-page="ventas" style="padding: 12px 16px; border: none; border-radius: 8px; background: #3B82F6; color: white; cursor: pointer; font-weight: 600; transition: all 0.3s; display: flex; align-items: center; gap: 8px; justify-content: center;">
          <i class="fas fa-plus"></i> Nueva Venta
        </button>
        <button class="btn-quick-action" data-page="clientes" style="padding: 12px 16px; border: none; border-radius: 8px; background: #10B981; color: white; cursor: pointer; font-weight: 600; transition: all 0.3s; display: flex; align-items: center; gap: 8px; justify-content: center;">
          <i class="fas fa-users"></i> Ver Clientes
        </button>
        <button class="btn-quick-action" data-page="productos" style="padding: 12px 16px; border: none; border-radius: 8px; background: #F59E0B; color: white; cursor: pointer; font-weight: 600; transition: all 0.3s; display: flex; align-items: center; gap: 8px; justify-content: center;">
          <i class="fas fa-box"></i> Ver Productos
        </button>
        <button class="btn-quick-action" data-page="ventas" style="padding: 12px 16px; border: none; border-radius: 8px; background: #64748B; color: white; cursor: pointer; font-weight: 600; transition: all 0.3s; display: flex; align-items: center; gap: 8px; justify-content: center;">
          <i class="fas fa-file-invoice-dollar"></i> Ver Facturas
        </button>
      </div>
    </div>
  </div>
</div>
    `
  }

  init() {
    console.log('[DASHBOARD] Inicializando...')
    this.loadStats()
    
    setTimeout(() => {
      document.querySelectorAll('.btn-quick-action').forEach(btn => {
        btn.addEventListener('click', (e) => {
          e.preventDefault()
          const page = btn.getAttribute('data-page')
          if (window.router) window.router.navigateTo(page)
        })
      })
    }, 100)
  }

  async loadStats() {
    try {
      console.log('[DASHBOARD] Cargando estadísticas...')
      const [clientesResp, productosResp, ventasResp] = await Promise.all([
        clienteService.getAll().catch(() => ({ data: [] })),
        productoService.getAll().catch(() => ({ data: [] })),
        ventaService.getAll().catch(() => ({ data: [] }))
      ])

      const clientes = clientesResp.data || []
      const productos = productosResp.data || []
      const ventas = ventasResp.data || []

      this.stats.totalClientes = clientes.length
      this.stats.totalProductos = productos.length
      this.stats.productosStockBajo = productos.filter(p => (p.stock || 0) < 10).length
      this.stats.totalVentas = ventas.reduce((sum, v) => sum + (v.totalVenta || v.total || 0), 0)
      
      console.log('[DASHBOARD] Stats:', this.stats)
      this.updateStatsDisplay()
    } catch (error) {
      console.error('[DASHBOARD] Error:', error)
    }
  }

  updateStatsDisplay() {
    const clientesCard = document.querySelector('.stat-clientes h3')
    const productosCard = document.querySelector('.stat-productos h3')
    const ventasCard = document.querySelector('.stat-ventas h3')
    const stockCard = document.querySelector('.stat-stock h3')
    
    if (clientesCard) clientesCard.textContent = this.stats.totalClientes
    if (productosCard) productosCard.textContent = this.stats.totalProductos
    if (ventasCard) ventasCard.textContent = '$' + this.stats.totalVentas.toFixed(2)
    if (stockCard) stockCard.textContent = this.stats.productosStockBajo
  }
}
