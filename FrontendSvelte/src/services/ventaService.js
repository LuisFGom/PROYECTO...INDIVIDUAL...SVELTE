import httpClient from './httpClient.js'

const ventaService = {
  getAll() {
    return httpClient.get('/ventas')
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

  async downloadPdf(id) {
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
      link.download = `venta-${id}.pdf`
      link.click()
      window.URL.revokeObjectURL(url)
    } catch (error) {
      throw error
    }
  }
}

export default ventaService
