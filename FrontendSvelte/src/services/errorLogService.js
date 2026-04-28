import httpClient from './httpClient.js'

const errorLogService = {
  getAll() {
    // El backend puede filtrar por estos parámetros opcionales
    return httpClient.get('/errorlogs')
  },

  getById(id) {
    return httpClient.get(`/errorlogs/${id}`)
  },

  create(data) {
    return httpClient.post('/errorlogs', data)
  }
}

export default errorLogService
