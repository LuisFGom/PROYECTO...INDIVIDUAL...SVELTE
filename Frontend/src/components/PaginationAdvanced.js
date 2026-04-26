// Componente de Paginación Avanzada para Vanilla JS
export class PaginationAdvanced {
  constructor(options = {}) {
    this.currentPage = options.currentPage || 1
    this.totalPages = options.totalPages || 1
    this.totalItems = options.totalItems || 0
    this.itemsPerPage = options.itemsPerPage || 10
    this.onChange = options.onChange || (() => {})
  }

  update(totalItems) {
    this.totalItems = totalItems
    this.totalPages = Math.ceil(totalItems / this.itemsPerPage)
    if (this.currentPage > this.totalPages && this.totalPages > 0) {
      this.currentPage = this.totalPages
    }
  }

  goToFirst() {
    if (this.currentPage !== 1) {
      this.currentPage = 1
      this.onChange(this.currentPage)
    }
  }

  goToLast() {
    if (this.currentPage !== this.totalPages) {
      this.currentPage = this.totalPages
      this.onChange(this.currentPage)
    }
  }

  goToPrevious() {
    if (this.currentPage > 1) {
      this.currentPage--
      this.onChange(this.currentPage)
    }
  }

  goToNext() {
    if (this.currentPage < this.totalPages) {
      this.currentPage++
      this.onChange(this.currentPage)
    }
  }

  movePages(delta) {
    const newPage = this.currentPage + delta
    if (newPage >= 1 && newPage <= this.totalPages) {
      this.currentPage = newPage
      this.onChange(this.currentPage)
    }
  }

  goToPage(page) {
    if (page >= 1 && page <= this.totalPages && page !== this.currentPage) {
      this.currentPage = page
      this.onChange(this.currentPage)
    }
  }

  getPageNumbers() {
    const pages = []
    const start = Math.max(1, this.currentPage - 5)
    const end = Math.min(this.currentPage - 1, this.totalPages)
    
    for (let i = start; i <= end && pages.length < 5; i++) {
      pages.push(i)
    }
    
    return pages
  }

  getRightPageNumbers() {
    const pages = []
    const start = Math.min(this.currentPage + 1, this.totalPages)
    const end = Math.min(this.currentPage + 5, this.totalPages)
    
    for (let i = start; i <= end && pages.length < 5; i++) {
      pages.push(i)
    }
    
    return pages
  }

  hasLeftGap() {
    const leftPages = this.getPageNumbers()
    return leftPages.length > 0 && leftPages[leftPages.length - 1] < this.currentPage - 1
  }

  hasRightGap() {
    const rightPages = this.getRightPageNumbers()
    return rightPages.length > 0 && rightPages[0] > this.currentPage + 1
  }

  render() {
    const leftPages = this.getPageNumbers()
    const rightPages = this.getRightPageNumbers()
    const hasLeftGap = this.hasLeftGap()
    const hasRightGap = this.hasRightGap()

    return `
    <div class="pagination-advanced" style="margin-top: 20px; padding: 20px; background: white; border-radius: 8px; box-shadow: 0 2px 8px rgba(0,0,0,0.05);">
      <!-- Controles de navegación -->
      <div class="pagination-controls" style="display: flex; gap: 8px; margin-bottom: 15px; flex-wrap: wrap; align-items: center;">
        <!-- Primera -->
        <button 
          class="pagination-btn ${this.currentPage === 1 ? 'disabled' : ''}"
          onclick="window.pagination.goToFirst()"
          ${this.currentPage === 1 ? 'disabled' : ''}
          style="padding: 8px 12px; border: 1px solid #3B82F6; background: white; color: #3B82F6; border-radius: 6px; cursor: ${this.currentPage === 1 ? 'not-allowed' : 'pointer'}; font-weight: 600; opacity: ${this.currentPage === 1 ? '0.5' : '1'};"
          title="Ir a la primera página"
        >
          ⏮ Primera
        </button>

        <!-- Retroceder 1000 -->
        <button 
          class="pagination-btn ${this.currentPage <= 1000 ? 'disabled' : ''}"
          onclick="window.pagination.movePages(-1000)"
          ${this.currentPage <= 1000 ? 'disabled' : ''}
          style="padding: 8px 12px; border: 1px solid #E2E8F0; background: white; color: #64748B; border-radius: 6px; cursor: ${this.currentPage <= 1000 ? 'not-allowed' : 'pointer'}; opacity: ${this.currentPage <= 1000 ? '0.5' : '1'};"
          title="Retroceder 1000 páginas"
        >
          ⬅ -1000
        </button>

        <!-- Retroceder 100 -->
        <button 
          class="pagination-btn ${this.currentPage <= 100 ? 'disabled' : ''}"
          onclick="window.pagination.movePages(-100)"
          ${this.currentPage <= 100 ? 'disabled' : ''}
          style="padding: 8px 12px; border: 1px solid #E2E8F0; background: white; color: #64748B; border-radius: 6px; cursor: ${this.currentPage <= 100 ? 'not-allowed' : 'pointer'}; opacity: ${this.currentPage <= 100 ? '0.5' : '1'};"
          title="Retroceder 100 páginas"
        >
          ⬅ -100
        </button>

        <!-- Retroceder 10 -->
        <button 
          class="pagination-btn ${this.currentPage <= 10 ? 'disabled' : ''}"
          onclick="window.pagination.movePages(-10)"
          ${this.currentPage <= 10 ? 'disabled' : ''}
          style="padding: 8px 12px; border: 1px solid #E2E8F0; background: white; color: #64748B; border-radius: 6px; cursor: ${this.currentPage <= 10 ? 'not-allowed' : 'pointer'}; opacity: ${this.currentPage <= 10 ? '0.5' : '1'};"
          title="Retroceder 10 páginas"
        >
          ⬅ -10
        </button>

        <!-- Anterior -->
        <button 
          class="pagination-btn ${this.currentPage === 1 ? 'disabled' : ''}"
          onclick="window.pagination.goToPrevious()"
          ${this.currentPage === 1 ? 'disabled' : ''}
          style="padding: 8px 12px; border: 1px solid #3B82F6; background: white; color: #3B82F6; border-radius: 6px; cursor: ${this.currentPage === 1 ? 'not-allowed' : 'pointer'}; font-weight: 600; opacity: ${this.currentPage === 1 ? '0.5' : '1'};"
          title="Página anterior"
        >
          ◀ Anterior
        </button>

        <!-- Números de página -->
        <div class="pagination-numbers" style="display: flex; gap: 4px; align-items: center;">
          ${leftPages.map(page => `
            <button 
              class="pagination-number"
              onclick="window.pagination.goToPage(${page})"
              style="padding: 8px 10px; border: 1px solid #E2E8F0; background: white; color: #64748B; border-radius: 6px; cursor: pointer; font-weight: 500;"
            >
              ${page}
            </button>
          `).join('')}

          ${hasLeftGap ? '<span style="padding: 0 4px; color: #64748B;">...</span>' : ''}

          <button 
            class="pagination-number active"
            style="padding: 8px 10px; border: 2px solid #3B82F6; background: #3B82F6; color: white; border-radius: 6px; font-weight: 600;"
            disabled
          >
            ${this.currentPage}
          </button>

          ${hasRightGap ? '<span style="padding: 0 4px; color: #64748B;">...</span>' : ''}

          ${rightPages.map(page => `
            <button 
              class="pagination-number"
              onclick="window.pagination.goToPage(${page})"
              style="padding: 8px 10px; border: 1px solid #E2E8F0; background: white; color: #64748B; border-radius: 6px; cursor: pointer; font-weight: 500;"
            >
              ${page}
            </button>
          `).join('')}
        </div>

        <!-- Siguiente -->
        <button 
          class="pagination-btn ${this.currentPage === this.totalPages ? 'disabled' : ''}"
          onclick="window.pagination.goToNext()"
          ${this.currentPage === this.totalPages ? 'disabled' : ''}
          style="padding: 8px 12px; border: 1px solid #3B82F6; background: white; color: #3B82F6; border-radius: 6px; cursor: ${this.currentPage === this.totalPages ? 'not-allowed' : 'pointer'}; font-weight: 600; opacity: ${this.currentPage === this.totalPages ? '0.5' : '1'};"
          title="Página siguiente"
        >
          Siguiente ▶
        </button>

        <!-- Avanzar 10 -->
        <button 
          class="pagination-btn ${this.currentPage + 10 > this.totalPages ? 'disabled' : ''}"
          onclick="window.pagination.movePages(10)"
          ${this.currentPage + 10 > this.totalPages ? 'disabled' : ''}
          style="padding: 8px 12px; border: 1px solid #E2E8F0; background: white; color: #64748B; border-radius: 6px; cursor: ${this.currentPage + 10 > this.totalPages ? 'not-allowed' : 'pointer'}; opacity: ${this.currentPage + 10 > this.totalPages ? '0.5' : '1'};"
          title="Avanzar 10 páginas"
        >
          +10 ➡
        </button>

        <!-- Avanzar 100 -->
        <button 
          class="pagination-btn ${this.currentPage + 100 > this.totalPages ? 'disabled' : ''}"
          onclick="window.pagination.movePages(100)"
          ${this.currentPage + 100 > this.totalPages ? 'disabled' : ''}
          style="padding: 8px 12px; border: 1px solid #E2E8F0; background: white; color: #64748B; border-radius: 6px; cursor: ${this.currentPage + 100 > this.totalPages ? 'not-allowed' : 'pointer'}; opacity: ${this.currentPage + 100 > this.totalPages ? '0.5' : '1'};"
          title="Avanzar 100 páginas"
        >
          +100 ➡
        </button>

        <!-- Avanzar 1000 -->
        <button 
          class="pagination-btn ${this.currentPage + 1000 > this.totalPages ? 'disabled' : ''}"
          onclick="window.pagination.movePages(1000)"
          ${this.currentPage + 1000 > this.totalPages ? 'disabled' : ''}
          style="padding: 8px 12px; border: 1px solid #E2E8F0; background: white; color: #64748B; border-radius: 6px; cursor: ${this.currentPage + 1000 > this.totalPages ? 'not-allowed' : 'pointer'}; opacity: ${this.currentPage + 1000 > this.totalPages ? '0.5' : '1'};"
          title="Avanzar 1000 páginas"
        >
          +1000 ➡
        </button>

        <!-- Última -->
        <button 
          class="pagination-btn ${this.currentPage === this.totalPages ? 'disabled' : ''}"
          onclick="window.pagination.goToLast()"
          ${this.currentPage === this.totalPages ? 'disabled' : ''}
          style="padding: 8px 12px; border: 1px solid #3B82F6; background: white; color: #3B82F6; border-radius: 6px; cursor: ${this.currentPage === this.totalPages ? 'not-allowed' : 'pointer'}; font-weight: 600; opacity: ${this.currentPage === this.totalPages ? '0.5' : '1'};"
          title="Ir a la última página"
        >
          Última ⏭
        </button>
      </div>

      <!-- Información de página -->
      <div class="pagination-info" style="text-align: center; color: #64748B; font-size: 14px;">
        <span>Página <strong style="color: #1E293B;">${this.currentPage}</strong> de <strong style="color: #1E293B;">${this.totalPages}</strong></span>
        <span style="margin: 0 8px;">•</span>
        <span>Total: <strong style="color: #1E293B;">${this.totalItems}</strong> registros (${this.itemsPerPage} por página)</span>
      </div>
    </div>
    `
  }
}
