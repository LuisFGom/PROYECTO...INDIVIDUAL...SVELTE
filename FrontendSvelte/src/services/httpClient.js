// HTTP Client - Fetch API con JWT y manejo de errores
const API_BASE_URL = 'http://localhost:5000/api'

class HttpClient {
  constructor() {
    this.baseURL = API_BASE_URL
    this.timeout = 10000
  }

  getHeaders() {
    const headers = {
      'Content-Type': 'application/json'
    }
    const token = localStorage.getItem('authToken')
    if (token) {
      headers['Authorization'] = `Bearer ${token}`
    }
    return headers
  }

  async request(method, endpoint, data = null) {
    try {
      const url = `${this.baseURL}${endpoint}`
      const config = {
        method,
        headers: this.getHeaders(),
        signal: AbortSignal.timeout(this.timeout)
      }

      if (data && (method === 'POST' || method === 'PUT' || method === 'PATCH')) {
        config.body = JSON.stringify(data)
      }

      console.log(`[HTTP] ${method} ${endpoint}`)
      const response = await fetch(url, config)

      // Sesión expirada
      if (response.status === 401) {
        localStorage.removeItem('authToken')
        localStorage.removeItem('currentUser')
        window.location.href = '/'
        return null
      }

      // No autorizado
      if (response.status === 403) {
        throw new Error('No tienes permisos para realizar esta acción.')
      }

      // Error del servidor
      if (response.status >= 500) {
        throw new Error('Error del servidor. Inténtalo más tarde.')
      }

      if (!response.ok) {
        let errorMessage = `Error ${response.status}`
        let errorDetails = {}
        try {
          const contentType = response.headers.get('content-type')
          if (contentType && contentType.includes('application/json')) {
            const errorData = await response.json()
            errorMessage = errorData.message || errorData.title || errorData.errors || JSON.stringify(errorData) || errorMessage
            errorDetails = errorData
          } else {
            errorMessage = await response.text()
          }
        } catch {
          errorMessage = `Error ${response.status}`
        }
        console.error('[HTTP ERROR]', response.status, errorDetails)
        throw new Error(errorMessage)
      }

      // Parsear respuesta
      const contentType = response.headers.get('content-type')
      if (contentType && contentType.includes('application/json')) {
        return await response.json()
      }

      if (response.status === 201 || response.status === 204) {
        return { success: true }
      }

      return null
    } catch (error) {
      if (error.name === 'AbortError') {
        throw new Error('La solicitud tardó demasiado tiempo. Verifica tu conexión.')
      }
      throw error
    }
  }

  get(endpoint) {
    return this.request('GET', endpoint)
  }

  post(endpoint, data) {
    return this.request('POST', endpoint, data)
  }

  put(endpoint, data) {
    return this.request('PUT', endpoint, data)
  }

  patch(endpoint, data) {
    return this.request('PATCH', endpoint, data)
  }

  delete(endpoint) {
    return this.request('DELETE', endpoint)
  }
}

export default new HttpClient()
