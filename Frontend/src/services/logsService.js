// Servicio de Logs y Auditoría
import { httpClient } from './http-client.js'

const ENDPOINT_LOGS = '/logs/intentos-login'
const ENDPOINT_ERROR_LOGS = '/error-logs'

export const logService = {
  // Logs de Auditoría
  async getAll(filtros = {}) {
    try {
      const params = new URLSearchParams()
      if (filtros.fechaInicio) params.append('fechaInicio', filtros.fechaInicio)
      if (filtros.fechaFin) params.append('fechaFin', filtros.fechaFin)
      if (filtros.exitoso !== undefined) params.append('exitoso', filtros.exitoso)
      if (filtros.usuario) params.append('usuario', filtros.usuario)

      const query = params.toString()
      const endpoint = `${ENDPOINT_LOGS}${query ? '?' + query : ''}`
      const response = await httpClient.get(endpoint)
      return response?.data || response || []
    } catch (error) {
      console.error('[logService] Error en getAll:', error)
      return []
    }
  },

  async search(term, filtros = {}) {
    try {
      const params = new URLSearchParams({ term, ...filtros })
      const response = await httpClient.get(`${ENDPOINT_LOGS}/search?${params}`)
      return response?.data || response || []
    } catch (error) {
      console.error('[logService] Error en search:', error)
      return []
    }
  },

  async getByType(tipo) {
    try {
      const response = await httpClient.get(`${ENDPOINT_LOGS}?tipo=${tipo}`)
      return response?.data || response || []
    } catch (error) {
      console.error('[logService] Error en getByType:', error)
      return []
    }
  },

  // Error Logs
  async getErrores(filtros = {}) {
    try {
      const params = new URLSearchParams()
      if (filtros.fechaInicio) params.append('fechaInicio', filtros.fechaInicio)
      if (filtros.fechaFin) params.append('fechaFin', filtros.fechaFin)
      if (filtros.severidad) params.append('severidad', filtros.severidad)
      if (filtros.resuelto !== undefined) params.append('resuelto', filtros.resuelto)

      const query = params.toString()
      const endpoint = `${ENDPOINT_ERROR_LOGS}${query ? '?' + query : ''}`
      const response = await httpClient.get(endpoint)
      return response?.data || response || []
    } catch (error) {
      console.error('[logService] Error en getErrores:', error)
      return []
    }
  },

  async getErrorById(id) {
    try {
      const response = await httpClient.get(`${ENDPOINT_ERROR_LOGS}/${id}`)
      return response?.data || response || null
    } catch (error) {
      console.error('[logService] Error en getErrorById:', error)
      return null
    }
  },

  async marcarErrorResuelto(id, notas = null) {
    try {
      const response = await httpClient.put(`${ENDPOINT_ERROR_LOGS}/${id}/resolver`, { notas })
      return response?.data || response || null
    } catch (error) {
      console.error('[logService] Error en marcarErrorResuelto:', error)
      return null
    }
  },

  async deleteLog(id) {
    try {
      const response = await httpClient.delete(`${ENDPOINT_LOGS}/${id}`)
      return response?.data || response || null
    } catch (error) {
      console.error('[logService] Error en deleteLog:', error)
      return null
    }
  }
}
