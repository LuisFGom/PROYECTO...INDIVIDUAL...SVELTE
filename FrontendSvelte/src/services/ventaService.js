import httpClient from './httpClient.js'

const ventaService = {
  getAll() {
    return httpClient.get('/ventas')
  },

  // Obtener facturas eliminadas
  getEliminadas(params = {}) {
    // params: { fechaInicio, fechaFin, clienteId, numeroFactura }
    const query = []
    if (params.fechaInicio) query.push(`fechaInicio=${encodeURIComponent(params.fechaInicio)}`)
    if (params.fechaFin) query.push(`fechaFin=${encodeURIComponent(params.fechaFin)}`)
    if (params.clienteId) query.push(`clienteId=${params.clienteId}`)
    if (params.numeroFactura) query.push(`numeroFactura=${encodeURIComponent(params.numeroFactura)}`)
    const qs = query.length ? `?${query.join('&')}` : ''
    return httpClient.get(`/ventas/eliminadas/lista${qs}`)
  },

  // Restaurar factura eliminada
  restaurar(id) {
    return httpClient.put(`/ventas/${id}/reinsertar`)
  },

  getById(id) {
    return httpClient.get(`/ventas/${id}`)
  },

  create(data) {
    return httpClient.post('/ventas', data)
  },

  update(id, data) {
    return httpClient.put(`/ventas/${id}`, data)
  },

  delete(id) {
    return httpClient.delete(`/ventas/${id}`)
  },

  // Anular una factura (soft delete) - restaura stock
  anularVenta(id) {
    return httpClient.put(`/ventas/${id}/eliminar`)
  },

  async downloadPdf(id, nombreArchivo = null) {
    try {
      const response = await fetch(`http://localhost:5000/api/ventas/${id}/pdf`, {
        method: 'GET',
        headers: {
          'Authorization': `Bearer ${localStorage.getItem('authToken')}`
        }
      })

      if (!response.ok) {
        throw new Error('Error descargando PDF')
      }

      const blob = await response.blob()
      const url = window.URL.createObjectURL(blob)
      const link = document.createElement('a')
      link.href = url
      link.download = nombreArchivo ? `${nombreArchivo}.pdf` : `venta-${id}.pdf`
      link.click()
      window.URL.revokeObjectURL(url)
    } catch (error) {
      throw error
    }
  }
}

export default ventaService
