<script>
  import rolService from '../services/rolService.js'
  import FormInput from '../components/FormInput.svelte'
  import DataTable from '../components/DataTable.svelte'
  import Alert from '../components/Alert.svelte'
  import Swal from 'sweetalert2'

  let roles = []
  let loading = true
  let searchTerm = ''
  let filteredRoles = []
  let isAdmin = false

  const loadRoles = async () => {
    loading = true
    try {
      const user = JSON.parse(localStorage.getItem('currentUser') || '{}')
      isAdmin = user.rol === 'Admin' || user.roleId === 1
      
      const data = await rolService.getAll()
      roles = Array.isArray(data) ? data : []
      filterRoles()
    } catch (error) {
      await Swal.fire('Error', error.message, 'error')
    } finally {
      loading = false
    }
  }

  const filterRoles = () => {
    if (!searchTerm.trim()) {
      filteredRoles = [...roles]
    } else {
      const term = searchTerm.toLowerCase()
      filteredRoles = roles.filter(r =>
        r.nombre?.toLowerCase().includes(term) ||
        r.descripcion?.toLowerCase().includes(term)
      )
    }
  }

  loadRoles()
</script>

<div class="roles-page">
  <div class="page-header">
    <h1><i class="fas fa-lock"></i> Roles</h1>
  </div>

  <div class="card">
    <div class="card-header">
      <input
        class="input"
        type="text"
        placeholder="Buscar por nombre..."
        bind:value={searchTerm}
        on:input={() => filterRoles()}
      />
    </div>

    <div class="card-body">
      <DataTable
        columns={[
          { key: 'nombre', label: 'Nombre' },
          { key: 'descripcion', label: 'Descripción' }
        ]}
        rows={filteredRoles}
        {loading}
        {isAdmin}
      />
    </div>
  </div>
</div>

<style>
  .roles-page {
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
