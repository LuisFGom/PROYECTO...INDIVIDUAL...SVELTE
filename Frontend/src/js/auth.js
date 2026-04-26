// Sistema de Autenticación Vanilla JS
import { store } from './store.js'
import { authService } from '../services/authService.js'
import Swal from 'sweetalert2'

class Auth {
  constructor() {
    this.token = localStorage.getItem('authToken')
  }

  async login(usuario, contrasena) {
    try {
      store.setState({ isLoading: true })
      
      const response = await authService.login(usuario, contrasena)
      
      if (response && response.token) {
        // Guardar token
        this.token = response.token
        localStorage.setItem('authToken', this.token)
        
        // Guardar usuario
        const userData = {
          id: response.usuarioId,
          email: response.correo,
          nombre: response.nombreCompleto,
          nombreUsuario: response.nombreUsuario,
          rolNombre: response.rol
        }
        store.setState({ currentUser: userData })
        localStorage.setItem('currentUser', JSON.stringify(userData))
        
        return true
      }
    } catch (error) {
      console.error('[AUTH] Login error:', error)
      Swal.fire('Error', error.message || 'Error al iniciar sesión', 'error')
      return false
    } finally {
      store.setState({ isLoading: false })
    }
  }

  logout() {
    this.token = null
    localStorage.removeItem('authToken')
    localStorage.removeItem('currentUser')
    store.setState({ currentUser: null })
    window.location.href = '/login.html'
  }

  checkAuth() {
    const token = localStorage.getItem('authToken')
    const user = localStorage.getItem('currentUser')
    
    if (!token || !user) {
      window.location.href = '/login.html'
      return false
    }
    
    try {
      const userData = JSON.parse(user)
      store.setState({ currentUser: userData })
      return true
    } catch (err) {
      this.logout()
      return false
    }
  }

  isAdmin() {
    const user = store.getState().currentUser
    return user && user.rolNombre === 'Admin'
  }

  getToken() {
    return this.token || localStorage.getItem('authToken')
  }
}

export const auth = new Auth()
