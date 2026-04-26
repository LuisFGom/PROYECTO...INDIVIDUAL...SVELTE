export const auth = {
  checkAuth() {
    const token = localStorage.getItem('authToken')
    const user = localStorage.getItem('currentUser')
    return !!(token && user)
  },

  logout() {
    localStorage.removeItem('authToken')
    localStorage.removeItem('currentUser')
    window.location.href = '/login.html'
  },

  getToken() {
    return localStorage.getItem('authToken')
  },

  getUser() {
    const user = localStorage.getItem('currentUser')
    return user ? JSON.parse(user) : null
  }
}
