// Router Vanilla JS simple
import { store } from './store.js'

class Router {
  constructor() {
    this.currentPage = 'dashboard'
  }

  navigateTo(page) {
    this.currentPage = page
    store.setState({ currentPage: page })
    
    // Actualizar active link en menú
    document.querySelectorAll('[data-page]').forEach(link => {
      link.parentElement.classList.remove('active')
      if (link.getAttribute('data-page') === page) {
        link.parentElement.classList.add('active')
      }
    })
  }

  getCurrentPage() {
    return this.currentPage
  }
}

export const router = new Router()
