import { store } from '../store.js'
import { clienteService } from '../services/clienteService.js'
import { productoService } from '../services/productoService.js'
import { ventaService } from '../services/ventaService.js'

export class Dashboard {
  constructor() {
    this.stats = {
      totalClientes: 0,
      totalProductos: 0,
      totalVentas: 0,
      productosStockBajo: 0
    }
    this.chartInstance = null
    this.dateRangeStart = new Date(Date.now() - 30 * 24 * 60 * 60 * 1000)
    this.dateRangeEnd = new Date()
  }

  render() {
    const userData = JSON.parse(localStorage.getItem('currentUser') || '{}')
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
        <p style="margin: 0; color: #64748B;"><i class="fas fa-user" style="color: #3B82F6; margin-right: 8px;"></i> <strong>Usuario:</strong> ${userData.nombreUsuario || userData.nombreCompleto || 'Usuario'}</p>
        <p style="margin: 0; color: #64748B;"><i class="fas fa-user-shield" style="color: #3B82F6; margin-right: 8px;"></i> <strong>Rol:</strong> ${userData.rol || 'Usuario'}</p>
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

  <div class="sales-chart-section" style="background: white; padding: 30px; border-radius: 12px; box-shadow: 0 2px 8px rgba(0,0,0,0.05); margin-top: 30px;">
    <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 25px;">
      <h3 style="margin: 0; color: #1E293B; font-size: 20px;"><i class="fas fa-chart-bar"></i> Ventas por Día</h3>
      <div style="display: flex; gap: 10px; align-items: center;">
        <div style="display: flex; gap: 5px; align-items: center;">
          <label style="font-size: 13px; color: #64748B; font-weight: 600;">Desde:</label>
          <input type="date" id="chartDateFrom" style="padding: 8px; border: 1px solid #E2E8F0; border-radius: 6px; font-size: 13px;"/>
        </div>
        <div style="display: flex; gap: 5px; align-items: center;">
          <label style="font-size: 13px; color: #64748B; font-weight: 600;">Hasta:</label>
          <input type="date" id="chartDateTo" style="padding: 8px; border: 1px solid #E2E8F0; border-radius: 6px; font-size: 13px;"/>
        </div>
        <button id="btnApplyDateRange" style="padding: 8px 16px; background: #3B82F6; color: white; border: none; border-radius: 6px; cursor: pointer; font-weight: 600; font-size: 13px;">Aplicar</button>
      </div>
    </div>
    <div style="position: relative; height: 300px;">
      <canvas id="salesChart"></canvas>
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

      // Cargar gráfico de ventas
      this.initializeSalesChart()
      
      // Event listeners para los date pickers
      const btnApply = document.getElementById('btnApplyDateRange')
      if (btnApply) {
        btnApply.addEventListener('click', () => this.updateSalesChart())
      }
    }, 100)
  }

  async loadStats() {
    try {
      console.log('[DASHBOARD] Cargando estadísticas desde backend...')
      
      // Cargar datos en paralelo
      const [clientes, productos, ventas] = await Promise.all([
        clienteService.getAll(),
        productoService.getAll(),
        ventaService.getAll()
      ])

      // Calcular estadísticas
      const totalVentas = Array.isArray(ventas) ? ventas.reduce((sum, v) => sum + (v.montoTotal || v.monto || 0), 0) : 0
      const stockBajo = Array.isArray(productos) ? productos.filter(p => (p.stockActual || p.stock || 0) <= (p.stockMinimo || 10)).length : 0

      this.stats = {
        totalClientes: Array.isArray(clientes) ? clientes.length : 0,
        totalProductos: Array.isArray(productos) ? productos.length : 0,
        totalVentas: totalVentas,
        productosStockBajo: stockBajo
      }

      console.log('[DASHBOARD] Estadísticas cargadas:', this.stats)
      
      // Actualizar UI
      this.updateStatsDisplay()
    } catch (error) {
      console.error('[DASHBOARD] Error cargando estadísticas:', error)
      this.stats = {
        totalClientes: 0,
        totalProductos: 0,
        totalVentas: 0,
        productosStockBajo: 0
      }
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

  async initializeSalesChart() {
    try {
      // Obtener ventas del período
      const ventas = await ventaService.getAll()
      console.log('[DASHBOARD] Ventas para gráfico:', ventas.length)
      
      // Setear fechas por defecto (últimos 30 días)
      const dateFromInput = document.getElementById('chartDateFrom')
      const dateToInput = document.getElementById('chartDateTo')
      
      if (dateFromInput && dateToInput) {
        const today = new Date()
        const thirtyDaysAgo = new Date(today.getTime() - 30 * 24 * 60 * 60 * 1000)
        
        dateFromInput.valueAsDate = thirtyDaysAgo
        dateToInput.valueAsDate = today
        
        this.dateRangeStart = thirtyDaysAgo
        this.dateRangeEnd = today
        
        console.log('[DASHBOARD] Rango por defecto:', thirtyDaysAgo.toLocaleDateString(), 'a', today.toLocaleDateString())
      }

      this.renderSalesChart(ventas)
    } catch (error) {
      console.error('[DASHBOARD] Error inicializando gráfico de ventas:', error)
    }
  }

  async updateSalesChart() {
    try {
      const dateFromInput = document.getElementById('chartDateFrom')
      const dateToInput = document.getElementById('chartDateTo')
      
      if (dateFromInput && dateToInput) {
        this.dateRangeStart = new Date(dateFromInput.value)
        this.dateRangeEnd = new Date(dateToInput.value)
        
        // Validar que la fecha inicial sea menor que la final
        if (this.dateRangeStart > this.dateRangeEnd) {
          Swal.fire({
            icon: 'warning',
            title: 'Validación',
            text: 'La fecha inicial debe ser menor que la fecha final',
            confirmButtonColor: '#3B82F6'
          })
          return
        }
      }
      
      const ventas = await ventaService.getAll()
      this.renderSalesChart(ventas)
    } catch (error) {
      console.error('[DASHBOARD] Error actualizando gráfico:', error)
    }
  }

  renderSalesChart(ventas) {
    // Agrupar ventas por día
    const salesByDay = {}
    
    // Asegurar que el rango de fechas incluye el día completo
    const rangeStart = new Date(this.dateRangeStart)
    rangeStart.setHours(0, 0, 0, 0)
    
    const rangeEnd = new Date(this.dateRangeEnd)
    rangeEnd.setHours(23, 59, 59, 999)
    
    console.log('[DASHBOARD] Filtrando ventas por rango:', rangeStart.toLocaleString('es-EC'), 'hasta', rangeEnd.toLocaleString('es-EC'))
    console.log('[DASHBOARD] Total de ventas a procesar:', ventas.length)
    
    let ventasEnRango = 0
    Array.isArray(ventas) && ventas.forEach((venta, index) => {
      const ventaDate = new Date(venta.fechaVenta)
      
      // Validar que la venta esté en el rango de fechas
      if (ventaDate >= rangeStart && ventaDate <= rangeEnd) {
        ventasEnRango++
        const dayKey = ventaDate.toLocaleDateString('es-EC')
        
        // Usar totalVenta como es el campo correcto según la estructura
        const monto = venta.totalVenta || 0
        
        if (salesByDay[dayKey]) {
          salesByDay[dayKey] += monto
        } else {
          salesByDay[dayKey] = monto
        }
      }
    })
    
    console.log('[DASHBOARD] Ventas en rango:', ventasEnRango)
    console.log('[DASHBOARD] Días con ventas:', Object.keys(salesByDay).length)

    // Crear array con todos los días en el rango (incluso si no hay ventas)
    const labels = []
    const data = []
    
    let currentDate = new Date(rangeStart)
    while (currentDate <= rangeEnd) {
      const dayKey = currentDate.toLocaleDateString('es-EC')
      labels.push(dayKey)
      const amount = salesByDay[dayKey] || 0
      data.push(amount)
      currentDate.setDate(currentDate.getDate() + 1)
    }
    
    // Calcular máximo valor para ajustar eje Y
    const maxValue = Math.max(...data, 0)
    const yAxisMax = maxValue > 0 ? maxValue * 1.1 : 1000 // 10% más que el máximo o 1000 si no hay datos
    
    console.log('[DASHBOARD] Máximo valor de ventas:', maxValue)
    console.log('[DASHBOARD] Escala Y ajustada a:', yAxisMax)

    // Configurar Chart.js si está disponible
    const canvas = document.getElementById('salesChart')
    if (!canvas) {
      console.warn('[DASHBOARD] Canvas #salesChart no encontrado')
      return
    }

    // Destruir gráfico anterior si existe
    if (this.chartInstance) {
      this.chartInstance.destroy()
    }

    // Verificar si Chart está disponible
    if (typeof Chart === 'undefined') {
      console.warn('[DASHBOARD] Chart.js no está disponible. Asegúrate que esté cargado en index.html')
      return
    }

    const ctx = canvas.getContext('2d')
    this.chartInstance = new Chart(ctx, {
      type: 'bar',
      data: {
        labels: labels,
        datasets: [{
          label: 'Ventas ($)',
          data: data,
          backgroundColor: 'rgba(59, 130, 246, 0.7)',
          borderColor: 'rgba(59, 130, 246, 1)',
          borderWidth: 1,
          borderRadius: 6,
          hoverBackgroundColor: 'rgba(59, 130, 246, 0.9)',
          tension: 0.1
        }]
      },
      options: {
        responsive: true,
        maintainAspectRatio: false,
        plugins: {
          legend: {
            position: 'top',
            labels: {
              font: { size: 13, weight: '600' },
              color: '#64748B',
              padding: 15
            }
          }
        },
        scales: {
          y: {
            beginAtZero: true,
            max: yAxisMax,
            ticks: {
              color: '#64748B',
              font: { size: 12 },
              callback: function(value) {
                return '$' + value.toFixed(2)
              }
            },
            grid: {
              color: '#E2E8F0'
            }
          },
          x: {
            ticks: {
              color: '#64748B',
              font: { size: 11 },
              maxRotation: 45,
              minRotation: 0
            },
            grid: {
              color: '#E2E8F0'
            }
          }
        }
      }
    })
  }
}
