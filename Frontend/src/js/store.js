// Sistema de Store Vanilla JS (Observable Pattern)
class Store {
  constructor(initialState = {}) {
    this.state = initialState
    this.subscribers = {}
  }

  getState() {
    return { ...this.state }
  }

  setState(newState) {
    this.state = { ...this.state, ...newState }
    // Notificar cambios en cada propiedad actualizada
    Object.keys(newState).forEach(key => this.notify(key))
  }

  subscribe(key, callback) {
    if (!this.subscribers[key]) {
      this.subscribers[key] = []
    }
    this.subscribers[key].push(callback)
  }

  notify(key) {
    if (key && this.subscribers[key]) {
      this.subscribers[key].forEach(cb => cb(this.state[key]))
    }
  }
}

// Inicializar store global
export const store = new Store({
  currentUser: null,
  currentPage: 'dashboard',
  clientes: [],
  productos: [],
  usuarios: [],
  isLoading: false,
  error: null
})
