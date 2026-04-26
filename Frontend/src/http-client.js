// HTTP Client con Fetch API
// Maneja token JWT, errores globales y reintentos
// Compatible con Vanilla JS

const API_BASE_URL = 'http://localhost:5000/api'

class HttpClient {
  constructor() {
    this.baseURL = API_BASE_URL
    this.defaultHeaders = {
      'Content-Type': 'application/json'
    }
    this.timeout = 10000 // 10 segundos
  }

  getHeaders() {
    const headers = { ...this.defaultHeaders }
    const token = localStorage.getItem('authToken')
    if (token) {
      headers['Authorization'] = `Bearer ${token}`
    }
    return headers
  }

  async request(method, endpoint, data = null, options = {}) {
    try {
      const url = `${this.baseURL}${endpoint}`
      const config = {
        method,
        headers: this.getHeaders(),
        signal: AbortSignal.timeout(this.timeout),
        ...(data && { body: JSON.stringify(data) }),
        ...options
      }

      console.log(`[HTTP] ${method.toUpperCase()} ${endpoint}`)
      const response = await fetch(url, config)

      // Manejar respuesta según status
      if (response.status === 401) {
        console.warn('[HTTP] Sesión expirada (401)')
        localStorage.removeItem('authToken')
        localStorage.removeItem('currentUser')
        // Redirigir a login
        if (typeof window !== 'undefined') {
          window.location.href = '/login.html'
        }
        return null
      }

      if (response.status === 403) {
        throw new Error('No tienes permisos para realizar esta acción.')
      }

      if (response.status >= 500) {
        throw new Error('Error del servidor. Inténtalo nuevamente.')
      }

      if (!response.ok) {
        let errorMessage = `Error ${response.status}`
        try {
          const errorData = await response.json()
          // Priorizar errores de validación específicos de campo sobre el título genérico
          const fieldErrors = errorData.errors
            ? Object.entries(errorData.errors).map(([, msgs]) => msgs.join(', ')).join('. ')
            : null
          errorMessage = errorData.message || fieldErrors || errorData.title || errorMessage
        } catch {
          errorMessage = (await response.text()) || errorMessage
        }
        throw new Error(errorMessage)
      }

      // Intentar parsear JSON
      const contentType = response.headers.get('content-type')
      if (contentType && contentType.includes('application/json')) {
        return await response.json()
      }

      return null
    } catch (error) {
      if (error.name === 'AbortError') {
        throw new Error('La solicitud tardó demasiado tiempo. Verifica tu conexión.')
      }
      throw error
    }
  }

  get(endpoint, options = {}) {
    return this.request('GET', endpoint, null, options)
  }

  post(endpoint, data, options = {}) {
    return this.request('POST', endpoint, data, options)
  }

  put(endpoint, data, options = {}) {
    return this.request('PUT', endpoint, data, options)
  }

  patch(endpoint, data, options = {}) {
    return this.request('PATCH', endpoint, data, options)
  }

  delete(endpoint, options = {}) {
    return this.request('DELETE', endpoint, null, options)
  }

  async getBlob(endpoint, options = {}) {
    try {
      const url = `${this.baseURL}${endpoint}`
      const config = {
        method: 'GET',
        headers: this.getHeaders(),
        signal: AbortSignal.timeout(this.timeout),
        ...options
      }

      console.log(`[HTTP] GET ${endpoint}`)
      const response = await fetch(url, config)

      if (response.status === 401) {
        console.warn('[HTTP] Sesión expirada (401)')
        localStorage.removeItem('authToken')
        localStorage.removeItem('currentUser')
        return null
      }

      if (!response.ok) {
        throw new Error(`Error ${response.status}`)
      }

      return await response.blob()
    } catch (error) {
      console.error('[HTTP] Error en getBlob:', error)
      throw error
    }
  }
}

export const httpClient = new HttpClient()
