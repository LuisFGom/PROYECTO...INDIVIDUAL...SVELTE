<script>
  import { onMount } from 'svelte'
  import { authStore } from '../stores/store.js'
  import DataTable from '../components/DataTable.svelte'
  import Alert from '../components/Alert.svelte'

  let facturasEliminadas = []
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
      const response = await fetch('http://localhost:5000/api/ventas/eliminadas', {
        headers: {
          'Authorization': `Bearer ${authState.token}`
        }
      })

      if (!response.ok) {
        throw new Error('Error al cargar facturas eliminadas')
      }

      facturasEliminadas = await response.json()
    } catch (err) {
      error = err.message
      console.error(err)
    } finally {
      loading = false
    }
  }

  const restaurar = async (facturaId) => {
    if (!window.confirm('¿Deseas restaurar esta factura?')) return

    try {
      const response = await fetch(`http://localhost:5000/api/ventas/${facturaId}/restaurar`, {
        method: 'PUT',
        headers: {
          'Authorization': `Bearer ${authState.token}`,
          'Content-Type': 'application/json'
        }
      })

      if (!response.ok) {
        throw new Error('Error al restaurar factura')
      }

      facturasEliminadas = facturasEliminadas.filter(f => f.id !== facturaId)
      alert('Factura restaurada exitosamente')
    } catch (err) {
      error = err.message
      console.error(err)
    }
  }

  const eliminarPermanente = async (facturaId) => {
    if (!window.confirm('¿Estás seguro de que deseas ELIMINAR PERMANENTEMENTE esta factura? Esta acción no se puede deshacer')) return

    try {
      const response = await fetch(`http://localhost:5000/api/ventas/${facturaId}/eliminar-permanente`, {
        method: 'DELETE',
        headers: {
          'Authorization': `Bearer ${authState.token}`
        }
      })

      if (!response.ok) {
        throw new Error('Error al eliminar factura permanentemente')
      }

      facturasEliminadas = facturasEliminadas.filter(f => f.id !== facturaId)
      alert('Factura eliminada permanentemente')
    } catch (err) {
      error = err.message
      console.error(err)
    }
  }
</script>

<div class="container">
  <div class="card">
    <div class="card-header">
      <h2>Eliminación de Facturas</h2>
      <p>Gestiona facturas eliminadas - Restaurar o eliminar permanentemente</p>
    </div>

    {#if error}
      <Alert type="error" message={error} />
    {/if}

    {#if loading}
      <div class="flex-center" style="padding: 2rem;">
        <div class="spinner"></div>
      </div>
    {:else if facturasEliminadas.length === 0}
      <div class="empty-state">
        <i class="fas fa-inbox"></i>
        <p>No hay facturas eliminadas</p>
      </div>
    {:else}
      <div class="table-container">
        <table class="table">
          <thead>
            <tr>
              <th>Número de Factura</th>
              <th>Cliente</th>
              <th>Fecha</th>
              <th>Total</th>
              <th>Estado</th>
              <th>Acciones</th>
            </tr>
          </thead>
          <tbody>
            {#each facturasEliminadas as factura (factura.id)}
              <tr>
                <td>#{factura.numero}</td>
                <td>{factura.clienteNombre}</td>
                <td>{new Date(factura.fecha).toLocaleDateString('es-EC')}</td>
                <td>${factura.total.toFixed(2)}</td>
                <td>
                  <span class="badge {factura.estado === 'Completada' ? 'badge-success' : factura.estado === 'Cancelada' ? 'badge-danger' : 'badge-warning'}">
                    {factura.estado}
                  </span>
                </td>
                <td>
                  <button
                    class="btn btn-sm btn-success"
                    on:click={() => restaurar(factura.id)}
                    title="Restaurar factura"
                  >
                    <i class="fas fa-undo"></i> Restaurar
                  </button>
                  <button
                    class="btn btn-sm btn-danger"
                    on:click={() => eliminarPermanente(factura.id)}
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

  .badge {
    display: inline-block;
    padding: 0.25rem 0.75rem;
    border-radius: 999px;
    font-size: 0.75rem;
    font-weight: 600;
  }

  .badge-success {
    background: #d1fae5;
    color: #065f46;
  }

  .badge-danger {
    background: #fee2e2;
    color: #7f1d1d;
  }

  .badge-warning {
    background: #fef3c7;
    color: #78350f;
  }
</style>
