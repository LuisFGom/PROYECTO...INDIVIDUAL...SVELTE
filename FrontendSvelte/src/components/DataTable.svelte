<script>
  export let columns = []
  export let rows = []
  export let onEdit = null
  export let onDelete = null
  export let loading = false
  export let isAdmin = false

  let sortKey = null
  let sortDirection = 'asc'

  const handleSort = (key) => {
    if (sortKey === key) {
      sortDirection = sortDirection === 'asc' ? 'desc' : 'asc'
    } else {
      sortKey = key
      sortDirection = 'asc'
    }
  }

  $: sortedRows = [...rows].sort((a, b) => {
    if (!sortKey) return 0
    const aVal = a[sortKey]
    const bVal = b[sortKey]
    if (aVal < bVal) return sortDirection === 'asc' ? -1 : 1
    if (aVal > bVal) return sortDirection === 'asc' ? 1 : -1
    return 0
  })
</script>

<div class="table-container">
  <table class="table">
    <thead>
      <tr>
        {#each columns as col}
          <th
            on:click={() => handleSort(col.key)}
            style="cursor: {col.sortable !== false ? 'pointer' : 'default'}"
          >
            {col.label}
            {#if sortKey === col.key}
              <i class="fas fa-{sortDirection === 'asc' ? 'arrow-up' : 'arrow-down'}"></i>
            {/if}
          </th>
        {/each}
        {#if (onEdit || onDelete) && isAdmin}
          <th style="text-align: center;">Acciones</th>
        {/if}
      </tr>
    </thead>
    <tbody>
      {#if loading}
        <tr>
          <td colspan={columns.length + (onEdit || onDelete ? 1 : 0)} style="text-align: center; padding: 2rem;">
            <i class="fas fa-spinner fa-spin"></i> Cargando...
          </td>
        </tr>
      {:else if sortedRows.length === 0}
        <tr>
          <td colspan={columns.length + (onEdit || onDelete ? 1 : 0)} style="text-align: center; color: #9ca3af; padding: 2rem;">
            No hay datos
          </td>
        </tr>
      {:else}
        {#each sortedRows as row}
          <tr>
            {#each columns as col}
              <td>
                {#if col.format}
                  {col.format(row[col.key])}
                {:else}
                  {row[col.key]}
                {/if}
              </td>
            {/each}
            {#if (onEdit || onDelete) && isAdmin}
              <td style="text-align: center;">
                <div class="flex gap-1 flex-center">
                  {#if onEdit}
                    <button
                      class="btn btn-sm btn-primary"
                      on:click={() => onEdit(row)}
                      title="Editar"
                    >
                      <i class="fas fa-edit"></i>
                    </button>
                  {/if}
                  {#if onDelete}
                    <button
                      class="btn btn-sm btn-danger"
                      on:click={() => onDelete(row)}
                      title="Eliminar"
                    >
                      <i class="fas fa-trash"></i>
                    </button>
                  {/if}
                </div>
              </td>
            {/if}
          </tr>
        {/each}
      {/if}
    </tbody>
  </table>
</div>

<style>
  th {
    user-select: none;
  }

  i {
    margin-left: 0.5rem;
    font-size: 0.75rem;
  }
</style>
