/**
 * GlobalModal - Componente modal reutilizable global
 * Similar al componente Modal.vue de Vue
 */
export class GlobalModal {
  static instance = null

  static getInstance() {
    if (!GlobalModal.instance) {
      GlobalModal.instance = new GlobalModal()
    }
    return GlobalModal.instance
  }

  constructor() {
    this.modal = null
    this.isOpen = false
    this._init()
  }

  _init() {
    // Crear el modal si no existe
    if (!document.getElementById('globalModal')) {
      const modalHtml = `
        <div id="globalModal" style="display: none; position: fixed; top: 0; left: 0; width: 100%; height: 100%; background: rgba(0,0,0,0.6); z-index: 9999; align-items: center; justify-content: center;">
          <div style="background: white; border-radius: 12px; box-shadow: 0 20px 60px rgba(0, 0, 0, 0.3); max-height: 90vh; width: 90%; max-width: 500px; display: flex; flex-direction: column;">
            <div style="display: flex; justify-content: space-between; align-items: center; padding: 20px; border-bottom: 1px solid #E2E8F0;">
              <h3 id="globalModalTitle" style="margin: 0; color: #1E293B; font-size: 20px;">Modal</h3>
              <button id="globalModalClose" style="background: none; border: none; font-size: 28px; cursor: pointer; color: #64748B; padding: 0; width: 30px; height: 30px; line-height: 1;">&times;</button>
            </div>
            <div id="globalModalBody" style="flex: 1; overflow-y: auto; padding: 20px;">
              <!-- Contenido dinámico aquí -->
            </div>
          </div>
        </div>
      `
      document.body.insertAdjacentHTML('beforeend', modalHtml)
    }

    this.modal = document.getElementById('globalModal')

    // Event listeners - Cerrar con botón X
    const closeBtn = document.getElementById('globalModalClose')
    if (closeBtn) {
      closeBtn.addEventListener('click', () => this.close())
    }

    // Event listener - Cerrar clickeando fuera del modal
    this.modal.addEventListener('click', (e) => {
      if (e.target === this.modal) {
        this.close()
      }
    })
  }

  /**
   * Abre el modal con contenido específico
   * @param {string} title - Título del modal
   * @param {string} content - Contenido HTML del modal
   * @param {function} onClose - Callback al cerrar
   */
  open(title, content, onClose = null) {
    const titleEl = document.getElementById('globalModalTitle')
    const bodyEl = document.getElementById('globalModalBody')

    if (titleEl) {
      titleEl.textContent = title
    }

    if (bodyEl) {
      bodyEl.innerHTML = content
    }

    this.modal.style.display = 'flex'
    this.isOpen = true
    this.onClose = onClose

    console.log(`[MODAL GLOBAL] Abierto: ${title}`)
  }

  /**
   * Cierra el modal
   */
  close() {
    this.modal.style.display = 'none'
    this.isOpen = false

    if (this.onClose && typeof this.onClose === 'function') {
      this.onClose()
    }

    console.log('[MODAL GLOBAL] Cerrado')
  }

  /**
   * Obtiene el elemento body del modal para manipularlo
   */
  getBodyElement() {
    return document.getElementById('globalModalBody')
  }
}
