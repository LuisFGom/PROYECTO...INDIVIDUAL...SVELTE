import httpClient from './httpClient.js'

const productoService = {
  getAll() {
    return httpClient.get('/productos')
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
