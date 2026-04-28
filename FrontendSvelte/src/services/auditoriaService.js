import httpClient from './httpClient.js'

const auditoriaService = {
  getAll() {
    return httpClient.get('/auditorias/acciones')
  },

  getById(id) {
    return httpClient.get(`/auditorias/${id}`)
  }
}

export default auditoriaService
