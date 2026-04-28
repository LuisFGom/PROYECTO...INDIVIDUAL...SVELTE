import httpClient from './httpClient.js'

const clienteService = {
  getAll() {
    return httpClient.get('/clientes')
  },

  getById(id) {
    return httpClient.get(`/clientes/${id}`)
  },

  create(data) {
    return httpClient.post('/clientes', data)
  },

  update(id, data) {
    return httpClient.put(`/clientes/${id}`, data)
  },

  delete(id) {
    return httpClient.delete(`/clientes/${id}`)
  },

  getEliminados() {
    return httpClient.get('/eliminacionesclientes')
  },

  restaurar(clienteEliminadoId) {
    return httpClient.put(`/clientes/${clienteEliminadoId}`, { activo: true })
  }
}

export default clienteService
