import httpClient from './httpClient.js'

const auditoriaService = {
  getAll() {
    return httpClient.get('/auditorias/acciones')
  },

  getById(id) {
    return httpClient.get(`/auditorias/${id}`)
  },

  // Obtiene acciones de auditoría para clientes eliminados
  getEliminacionesClientes: async () => {
    // Trae hasta 500 eliminaciones de clientes
    return httpClient.get('/auditorias/acciones?modulo=Clientes&tipoAccion=Eliminar&take=500')
  }
}

export default auditoriaService
