<script>
  import auditoriaService from '../services/auditoriaService.js'
  import { formatters } from '../utils/validators.js'
  import DataTable from '../components/DataTable.svelte'
  import Swal from 'sweetalert2'

  let auditorias = []
  let loading = true
  let searchTerm = ''
  let filteredAuditorias = []

  const loadAuditorias = async () => {
    loading = true
    try {
      const data = await auditoriaService.getAll()
      auditorias = Array.isArray(data) ? data : []
      auditorias.sort((a, b) => new Date(b.fecha) - new Date(a.fecha))
      filterAuditorias()
    } catch (error) {
      await Swal.fire('Error', error.message, 'error')
    } finally {
      loading = false
    }
  }

  const filterAuditorias = () => {
    if (!searchTerm.trim()) {
      filteredAuditorias = [...auditorias]
    } else {
      const term = searchTerm.toLowerCase()
      filteredAuditorias = auditorias.filter(a =>
        a.nombreUsuario?.toLowerCase().includes(term) ||
        a.tipoAccion?.toLowerCase().includes(term) ||
        a.modulo?.toLowerCase().includes(term)
      )
    }
  }

  loadAuditorias()
</script>

<div class="auditorias-page">
  <div class="page-header">
    <h1><i class="fas fa-list"></i> Auditorías</h1>
  </div>

  <div class="card">
    <div class="card-header">
      <input
        class="input"
        type="text"
        placeholder="Buscar por usuario, acción o tabla..."
        bind:value={searchTerm}
        on:input={() => filterAuditorias()}
      />
    </div>

    <div class="card-body">
      <DataTable
        columns={[
          { key: 'nombreUsuario', label: 'Usuario' },
          { key: 'tipoAccion', label: 'Acción' },
          { key: 'modulo', label: 'Tabla' },
          { key: 'fechaAccion', label: 'Fecha', format: (d) => formatters.formatDate(d) }
        ]}
        rows={filteredAuditorias}
        {loading}
      />
    </div>
  </div>
</div>

<style>
  .auditorias-page {
    padding: 2rem;
  }

  .page-header {
    margin-bottom: 2rem;
  }

  .page-header h1 {
    margin: 0;
    font-size: 2rem;
    display: flex;
    align-items: center;
    gap: 0.5rem;
  }
</style>
