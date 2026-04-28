import httpClient from './httpClient.js'

const authService = {
  async login(email, contrasena) {
    const response = await httpClient.post('/auth/login', { email, contrasena })
    if (response && response.token) {
      localStorage.setItem('authToken', response.token)
      // El servidor retorna usuarioId, nombreUsuario, nombreCompleto, correo, rol
      const user = {
        id: response.usuarioId,
        nombre: response.nombreCompleto,
        email: response.correo,
        rol: response.rol,
        nombreUsuario: response.nombreUsuario
      }
      localStorage.setItem('currentUser', JSON.stringify(user))
    }
    return response
  },

  async registro(userData) {
    const response = await httpClient.post('/auth/registro', userData)
    if (response && response.token) {
      localStorage.setItem('authToken', response.token)
      const user = {
        id: response.usuarioId,
        nombre: response.nombreCompleto,
        email: response.correo,
        rol: response.rol,
        nombreUsuario: response.nombreUsuario
      }
      localStorage.setItem('currentUser', JSON.stringify(user))
    }
    return response
  },

  logout() {
    localStorage.removeItem('authToken')
    localStorage.removeItem('currentUser')
  },

  getToken() {
    return localStorage.getItem('authToken')
  },

  getUser() {
    const user = localStorage.getItem('currentUser')
    return user ? JSON.parse(user) : null
  },

  setToken(token) {
    localStorage.setItem('authToken', token)
  },

  setUser(user) {
    localStorage.setItem('currentUser', JSON.stringify(user))
  },

  isAuthenticated() {
    return !!this.getToken()
  },

  isAdmin() {
    const user = this.getUser()
    return user && (user.rol === 'Admin' || user.roleId === 1)
  }
}

export default authService
