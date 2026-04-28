<script>
  import { onMount } from 'svelte'
  import { authStore } from '../stores/store.js'
  import DataTable from '../components/DataTable.svelte'
  import Alert from '../components/Alert.svelte'

  let productosEliminados = []
  let loading = false
  let error = ''
  let authState = {}

  onMount(async () => {
    const unsubAuth = authStore.subscribe(value => {
      authState = value
    })

    // Verificar permisos
    if (!authState.user || (authState.user.rol !== 'Admin' && authState.user.roleId !== 1)) {
      error = 'Acceso Denegado - No tienes permisos para acceder a esta página'
      return
    }

    cargarDatos()

    return unsubAuth
  })

  const cargarDatos = async () => {
    loading = true
    try {
      const response = await fetch('http://localhost:5000/api/productos/eliminados', {
        headers: {
          'Authorization': `Bearer ${authState.token}`
        }
      })

      if (!response.ok) {
        throw new Error('Error al cargar productos eliminados')
      }

      productosEliminados = await response.json()
    } catch (err) {
      error = err.message
      console.error(err)
    } finally {
      loading = false
    }
  }

  const restaurar = async (productoId) => {
    if (!window.confirm('¿Deseas restaurar este producto?')) return

    try {
      const response = await fetch(`http://localhost:5000/api/productos/${productoId}/restaurar`, {
        method: 'PUT',
        headers: {
          'Authorization': `Bearer ${authState.token}`,
          'Content-Type': 'application/json'
        }
      })

      if (!response.ok) {
        throw new Error('Error al restaurar producto')
      }

      productosEliminados = productosEliminados.filter(p => p.id !== productoId)
      alert('Producto restaurado exitosamente')
    } catch (err) {
      error = err.message
      console.error(err)
    }
  }

  const eliminarPermanente = async (productoId) => {
    if (!window.confirm('¿Estás seguro de que deseas ELIMINAR PERMANENTEMENTE este producto? Esta acción no se puede deshacer')) return

    try {
      const response = await fetch(`http://localhost:5000/api/productos/${productoId}/eliminar-permanente`, {
        method: 'DELETE',
        headers: {
          'Authorization': `Bearer ${authState.token}`
        }
      })

      if (!response.ok) {
        throw new Error('Error al eliminar producto permanentemente')
      }

      productosEliminados = productosEliminados.filter(p => p.id !== productoId)
      alert('Producto eliminado permanentemente')
    } catch (err) {
      error = err.message
      console.error(err)
    }
  }
</script>

<div class="container">
  <div class="card">
    <div class="card-header">
      <h2>Eliminación de Productos</h2>
      <p>Gestiona productos eliminados - Restaurar o eliminar permanentemente</p>
    </div>

    {#if error}
      <Alert type="error" message={error} />
    {/if}

    {#if loading}
      <div class="flex-center" style="padding: 2rem;">
        <div class="spinner"></div>
      </div>
    {:else if productosEliminados.length === 0}
      <div class="empty-state">
        <i class="fas fa-inbox"></i>
        <p>No hay productos eliminados</p>
      </div>
    {:else}
      <div class="table-container">
        <table class="table">
          <thead>
            <tr>
              <th>Nombre</th>
              <th>Código</th>
              <th>Precio Venta</th>
              <th>Stock</th>
              <th>Acciones</th>
            </tr>
          </thead>
          <tbody>
            {#each productosEliminados as producto (producto.id)}
              <tr>
                <td>{producto.nombre}</td>
                <td>{producto.codigo}</td>
                <td>${producto.precioVenta.toFixed(2)}</td>
                <td>{producto.stockInicial}</td>
                <td>
                  <button
                    class="btn btn-sm btn-success"
                    on:click={() => restaurar(producto.id)}
                    title="Restaurar producto"
                  >
                    <i class="fas fa-undo"></i> Restaurar
                  </button>
                  <button
                    class="btn btn-sm btn-danger"
                    on:click={() => eliminarPermanente(producto.id)}
                    title="Eliminar permanentemente"
                  >
                    <i class="fas fa-trash"></i> Eliminar
                  </button>
                </td>
              </tr>
            {/each}
          </tbody>
        </table>
      </div>
    {/if}
  </div>
</div>

<style>
  .container {
    padding: 2rem;
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

  .card-header h2 {
    margin: 0;
    font-size: 1.5rem;
    color: #1f2937;
  }

  .card-header p {
    margin: 0.5rem 0 0 0;
    color: #6b7280;
    font-size: 0.875rem;
  }

  .table-container {
    overflow-x: auto;
  }

  .table {
    width: 100%;
    border-collapse: collapse;
  }

  .table thead {
    background: #f9fafb;
  }

  .table th {
    padding: 1rem;
    text-align: left;
    font-weight: 600;
    color: #4b5563;
    border-bottom: 1px solid #e5e7eb;
  }

  .table td {
    padding: 1rem;
    border-bottom: 1px solid #e5e7eb;
    color: #1f2937;
  }

  .table tbody tr:hover {
    background: #f9fafb;
  }

  .empty-state {
    text-align: center;
    padding: 3rem 1.5rem;
    color: #6b7280;
  }

  .empty-state i {
    font-size: 3rem;
    color: #d1d5db;
    display: block;
    margin-bottom: 1rem;
  }

  .flex-center {
    display: flex;
    justify-content: center;
    align-items: center;
  }

  .btn {
    display: inline-flex;
    align-items: center;
    gap: 0.5rem;
    padding: 0.5rem 1rem;
    border: none;
    border-radius: 4px;
    cursor: pointer;
    font-size: 0.875rem;
    transition: all 200ms ease;
  }

  .btn-sm {
    padding: 0.25rem 0.75rem;
    font-size: 0.8rem;
  }

  .btn-success {
    background: #10b981;
    color: white;
  }

  .btn-success:hover {
    background: #059669;
  }

  .btn-danger {
    background: #ef4444;
    color: white;
  }

  .btn-danger:hover {
    background: #dc2626;
  }
</style>
