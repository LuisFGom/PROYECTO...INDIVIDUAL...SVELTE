export class ErrorLogs {
  render() {
    return `
      <div style="padding: 2rem;">
        <div class="page-header">
          <h1 class="page-title">
            <i class="fas fa-triangle-exclamation"></i>
            Registro de Errores
          </h1>
        </div>

        <div class="card">
          <div class="card-body">
            <div style="margin-bottom: 1.5rem; display: flex; gap: 1rem;">
              <input type="text" id="searchErrors" placeholder="Buscar errores..." 
                     style="flex: 1; padding: 0.75rem; border: 1px solid #E2E8F0; border-radius: 6px;">
              <select id="severityFilter" style="padding: 0.75rem; border: 1px solid #E2E8F0; border-radius: 6px;">
                <option value="">Todas las severidades</option>
                <option value="BAJA">Baja</option>
                <option value="MEDIA">Media</option>
                <option value="ALTA">Alta</option>
              </select>
            </div>

            <table style="width: 100%; border-collapse: collapse;">
              <thead>
                <tr style="background-color: #F8FAFC; border-bottom: 2px solid #E2E8F0;">
                  <th style="padding: 0.75rem; text-align: left;">Fecha</th>
                  <th style="padding: 0.75rem; text-align: left;">Severidad</th>
                  <th style="padding: 0.75rem; text-align: left;">Mensaje</th>
                  <th style="padding: 0.75rem; text-align: left;">Stack Trace</th>
                  <th style="padding: 0.75rem; text-align: center;">Acción</th>
                </tr>
              </thead>
              <tbody id="errorsTable">
                <tr>
                  <td colspan="5" style="padding: 2rem; text-align: center; color: #64748B;">
                    <i class="fas fa-check-circle"></i> Sin errores registrados
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>
      </div>
    `
  }

  async init() {
    this.loadErrors()
    
    document.getElementById('searchErrors')?.addEventListener('input', () => {
      this.filterErrors()
    })

    document.getElementById('severityFilter')?.addEventListener('change', () => {
      this.filterErrors()
    })
  }

  async loadErrors() {
    const table = document.getElementById('errorsTable')
    if (!table) return

    try {
      // Datos de ejemplo
      const mockErrors = []
      table.innerHTML = mockErrors.length > 0 ? mockErrors.map(error => `
        <tr style="border-bottom: 1px solid #E2E8F0;">
          <td style="padding: 0.75rem;">${error.fecha}</td>
          <td style="padding: 0.75rem;">
            <span style="padding: 0.25rem 0.75rem; border-radius: 4px; font-weight: 600;">
              ${error.severidad}
            </span>
          </td>
          <td style="padding: 0.75rem;">${error.mensaje}</td>
          <td style="padding: 0.75rem; font-size: 0.875rem; color: #64748B;">Ver detalles</td>
          <td style="padding: 0.75rem; text-align: center;">
            <button class="btn btn-sm btn-outline">Resolver</button>
          </td>
        </tr>
      `).join('') : '<tr><td colspan="5" style="padding: 2rem; text-align: center; color: #64748B;"><i class="fas fa-check-circle"></i> Sin errores registrados</td></tr>'
    } catch (error) {
      console.error('Error cargando errores:', error)
    }
  }

  filterErrors() {
    // Implementar filtrado
  }
}
