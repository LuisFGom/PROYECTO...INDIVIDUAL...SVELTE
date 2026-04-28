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

  async getEliminados() {
    // Trae todos los clientes y filtra los eliminados (activo: false o fechaEliminacion)
    const clientes = await httpClient.get('/clientes')
    return (clientes || []).filter(c => c.activo === false || c.fechaEliminacion)
  },

  restaurar(clienteId) {
    // Reactiva el cliente y borra la fecha de eliminación
    return httpClient.put(`/clientes/${clienteId}`, { activo: true, fechaEliminacion: null })
  }
}

export default clienteService
