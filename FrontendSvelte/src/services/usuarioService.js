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
    console.log('[DEBUG usuarioService] Llamando a GET /eliminacionesusuarios')
    return httpClient.get('/eliminacionesusuarios').then(result => {
      console.log('[DEBUG usuarioService] Respuesta de /eliminacionesusuarios:', result)
      return result
    }).catch(err => {
      console.error('[ERROR usuarioService] Error en GET /eliminacionesusuarios:', err)
      throw err
    })
  },

  restaurar(usuarioEliminadoId) {
    return httpClient.put(`/usuarios/${usuarioEliminadoId}/restaurar`)
  },

}

export default usuarioService
