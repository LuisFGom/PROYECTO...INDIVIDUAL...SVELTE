import httpClient from './httpClient.js'

const auditoriasService = {
  // Obtiene todas las acciones de auditoría con filtros opcionales
  getAll: async (params = {}) => {
    const query = new URLSearchParams()
    if (params.usuarioId) query.append('usuarioId', params.usuarioId)
    if (params.modulo) query.append('modulo', params.modulo)
    if (params.tipoAccion) query.append('tipoAccion', params.tipoAccion)
    if (params.fechaInicio) query.append('fechaInicio', params.fechaInicio)
    if (params.fechaFin) query.append('fechaFin', params.fechaFin)
    query.append('skip', params.skip || 0)
    query.append('take', params.take || 100)

    const qs = query.toString() ? `?${query.toString()}` : ''
    return await httpClient.get(`/auditorias/acciones${qs}`)
  },

  // Obtiene acciones de un usuario específico
  getByUsuario: async (usuarioId) => {
    return await httpClient.get(`/auditorias/usuario/${usuarioId}`)
  },

  // Obtiene acciones de un módulo específico
  getByModulo: async (modulo) => {
    return await httpClient.get(`/auditorias/modulo/${modulo}`)
  }
}

export default auditoriasService
