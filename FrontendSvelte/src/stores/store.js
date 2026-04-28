import { writable } from 'svelte/store'

// Store de autenticación
export const authStore = writable({
  isAuthenticated: false,
  isLoading: false,
  user: null,
  token: null,
  error: null
})

// Store de datos globales
export const dataStore = writable({
  clientes: [],
  productos: [],
  ventas: [],
  usuarios: [],
  roles: [],
  auditorias: [],
  errorLogs: [],
  loading: false,
  error: null,
  success: null
})

// Store de UI
export const uiStore = writable({
  currentPage: 'login',
  sidebarOpen: true,
  modal: {
    isOpen: false,
    title: '',
    content: null,
    onConfirm: null,
    isLoading: false
  }
})

// Acciones de autenticación
export const setAuthState = (state) => {
  authStore.update(auth => ({ ...auth, ...state }))
}

export const logout = () => {
  authStore.set({
    isAuthenticated: false,
    isLoading: false,
    user: null,
    token: null,
    error: null
  })
}

// Acciones de datos
export const setData = (key, value) => {
  dataStore.update(data => ({ ...data, [key]: value }))
}

export const setLoading = (loading) => {
  dataStore.update(data => ({ ...data, loading }))
}

export const setError = (error) => {
  dataStore.update(data => ({ ...data, error }))
}

export const setSuccess = (success) => {
  dataStore.update(data => ({ ...data, success }))
}

// Acciones de UI
export const setCurrentPage = (page) => {
  uiStore.update(ui => ({ ...ui, currentPage: page }))
}

export const toggleSidebar = () => {
  uiStore.update(ui => ({ ...ui, sidebarOpen: !ui.sidebarOpen }))
}

export const openModal = (title, content, onConfirm) => {
  uiStore.update(ui => ({
    ...ui,
    modal: {
      isOpen: true,
      title,
      content,
      onConfirm,
      isLoading: false
    }
  }))
}

export const closeModal = () => {
  uiStore.update(ui => ({
    ...ui,
    modal: {
      ...ui.modal,
      isOpen: false
    }
  }))
}

export const setModalLoading = (loading) => {
  uiStore.update(ui => ({
    ...ui,
    modal: {
      ...ui.modal,
      isLoading: loading
    }
  }))
}
