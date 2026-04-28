import httpClient from './httpClient.js'

const usuarioService = {
  getAll() {
    return httpClient.get('/usuarios')
  },

  getById(id) {
    return httpClient.get(`/usuarios/${id}`)
  },

  create(data) {
    return httpClient.post('/usuarios', data)
  },

  update(id, data) {
    return httpClient.put(`/usuarios/${id}`, data)
  },

  delete(id) {
    return httpClient.delete(`/usuarios/${id}`)
  },

  getEliminados() {
    return httpClient.get('/eliminacionesusuarios')
  },

  restaurar(usuarioEliminadoId) {
    return httpClient.put(`/usuarios/${usuarioEliminadoId}`, { activo: true })
  },

}

export default usuarioService
