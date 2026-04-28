<script>
  import errorLogService from '../services/errorLogService.js'
  import { formatters } from '../utils/validators.js'
  import DataTable from '../components/DataTable.svelte'
  import Swal from 'sweetalert2'

  let errorLogs = []
  let loading = true
  let searchTerm = ''
  let filteredLogs = []

  const loadLogs = async () => {
    loading = true
    try {
      const data = await errorLogService.getAll()
      errorLogs = Array.isArray(data) ? data : []
      errorLogs.sort((a, b) => new Date(b.fecha) - new Date(a.fecha))
      filterLogs()
    } catch (error) {
      await Swal.fire('Error', error.message, 'error')
    } finally {
      loading = false
    }
  }

  const filterLogs = () => {
    if (!searchTerm.trim()) {
      filteredLogs = [...errorLogs]
    } else {
      const term = searchTerm.toLowerCase()
      filteredLogs = errorLogs.filter(log =>
        log.mensaje?.toLowerCase().includes(term) ||
        log.stackTrace?.toLowerCase().includes(term)
      )
    }
  }

  loadLogs()
</script>

<div class="logs-page">
  <div class="page-header">
    <h1><i class="fas fa-exclamation-triangle"></i> Logs de Errores</h1>
  </div>

  <div class="card">
    <div class="card-header">
      <input
        class="input"
        type="text"
        placeholder="Buscar por mensaje..."
        bind:value={searchTerm}
        on:input={() => filterLogs()}
      />
    </div>

    <div class="card-body">
      <DataTable
        columns={[
          { key: 'mensaje', label: 'Mensaje' },
          { key: 'stackTrace', label: 'Stack Trace' },
          { key: 'fecha', label: 'Fecha', format: (d) => formatters.formatDate(d) }
        ]}
        rows={filteredLogs}
        {loading}
      />
    </div>
  </div>
</div>

<style>
  .logs-page {
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
