import httpClient from './httpClient.js'

const productoService = {
  getAll() {
    return httpClient.get('/productos')
  },

  // Obtiene productos eliminados (inactivos o con fechaEliminacion)
  async getEliminados() {
    const productos = await httpClient.get('/productos')
    return (productos || []).filter(p => p.activo === false || p.fechaEliminacion)
  },

  // Restaura producto (activo: true, fechaEliminacion: null)
  restaurar(productoId) {
    return httpClient.put(`/productos/${productoId}`, { activo: true, fechaEliminacion: null })
  },

  getById(id) {
    return httpClient.get(`/productos/${id}`)
  },

  create(data) {
    return httpClient.post('/productos', data)
  },

  update(id, data) {
    return httpClient.put(`/productos/${id}`, data)
  },

  delete(id) {
    return httpClient.delete(`/productos/${id}`)
  }
}

export default productoService
