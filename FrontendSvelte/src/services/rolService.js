import httpClient from './httpClient.js'

const rolService = {
  getAll() {
    return httpClient.get('/roles')
  },

  getById(id) {
    return httpClient.get(`/roles/${id}`)
  },

  create(data) {
    return httpClient.post('/roles', data)
  },

  update(id, data) {
    return httpClient.put(`/roles/${id}`, data)
  },

  delete(id) {
    return httpClient.delete(`/roles/${id}`)
  }
}

export default rolService
