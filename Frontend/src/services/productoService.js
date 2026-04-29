// Servicio de Productos
import { httpClient } from './http-client.js'

const ENDPOINT_PRODUCTOS = '/Product'

// Función auxiliar para normalizar datos del backend
const normalizeProducto = (p) => ({
  id: p.id || p.Id,
  nombre: p.nombre || p.Nombre,
  codigoBarra: p.codigo || p.codigoBarra || p.CodigoBarra || p.Codigo,
  descripcion: p.descripcion || p.Descripcion || '',
  precioCosto: p.precioCompra || p.PrecioCompra || 0,
  precioVenta: p.precio || p.precioVenta || p.Precio || 0,
  stockActual: p.stock || p.Stock || 0,
  stockMinimo: p.stockMinimo || p.StockMinimo || 0,
  porcentajeIVA: p.porcentajeIVA || p.PorcentajeIVA || 0,
  activo: p.activo !== false && p.Activo !== false,
  fechaCreacion: p.fechaCreacion || p.FechaCreacion,
  fechaActualizacion: p.fechaActualizacion || p.FechaActualizacion
})

export const productoService = {
  async getAll() {
    try {
      const response = await httpClient.get('/Product')
      const productos = response?.data || response || []
      return Array.isArray(productos) ? productos.map(normalizeProducto) : []
    } catch (error) {
      console.error('[productoService] Error en getAll:', error)
      return []
    }
  },

  async getById(id) {
    try {
      const response = await httpClient.get(`/Product/${id}`)
      return response?.data ? normalizeProducto(response.data) : response?.data || null
    } catch (error) {
      console.error('[productoService] Error en getById:', error)
      return null
    }
  },

  async create(productoData) {
    const payload = {
      nombre: productoData.nombre,
      codigo: productoData.codigoBarra,
      descripcion: productoData.descripcion || '',
      precio: productoData.precioVenta,
      precioCompra: productoData.precioCosto || 0,
      stock: productoData.stockActual || 0,
      stockMinimo: productoData.stockMinimo || 10,
      porcentajeIVA: productoData.porcentajeIVA || 0
    }
    const response = await httpClient.post('/Product', payload)
    return response?.data || response || null
  },

  async update(id, productoData) {
    const payload = {
      id: id,
      nombre: productoData.nombre,
      descripcion: productoData.descripcion,
      precioVenta: productoData.precioVenta,
      precioCompra: productoData.precioCosto,
      stock: productoData.stockActual,
      stockMinimo: productoData.stockMinimo,
      porcentajeIVA: productoData.porcentajeIVA || 0
    }
    const response = await httpClient.put(`/Product/${id}`, payload)
    return response?.data || response || null
  },

  async delete(id) {
    const response = await httpClient.delete(`/Product/${id}`)
    return response?.data || response || true
  },

  async getDisponibles() {
    try {
      const response = await httpClient.get(`/Product/disponibles`)
      const productos = response?.data || response || []
      return Array.isArray(productos) ? productos.map(normalizeProducto) : []
    } catch (error) {
      console.error('[productoService] Error en getDisponibles:', error)
      return []
    }
  },

  async search(term, limit = 20) {
    try {
      const response = await httpClient.get(`/Product/search?term=${encodeURIComponent(term)}&limit=${limit}`)
      const productos = response?.data || response || []
      return Array.isArray(productos) ? productos.map(normalizeProducto) : []
    } catch (error) {
      console.error('[productoService] Error en search:', error)
      return []
    }
  }
}
