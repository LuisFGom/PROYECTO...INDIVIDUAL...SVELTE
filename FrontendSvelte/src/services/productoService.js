import httpClient from './httpClient.js'

const productoService = {
  getAll() {
    return httpClient.get('/Product')
  },

  // Obtiene productos eliminados (inactivos o con fechaEliminacion)
  async getEliminados() {
    return await httpClient.get('/Product/eliminados')
  },

  // Restaura producto usando el endpoint específico
  restaurar(productoId) {
    return httpClient.put(`/Product/${productoId}/restaurar`)
  },

  getById(id) {
    return httpClient.get(`/Product/${id}`)
  },

  create(data) {
    return httpClient.post('/Product', data)
  },

  update(id, data) {
    return httpClient.put(`/Product/${id}`, data)
  },

  delete(id) {
    return httpClient.delete(`/Product/${id}`)
  }
}

export default productoService
