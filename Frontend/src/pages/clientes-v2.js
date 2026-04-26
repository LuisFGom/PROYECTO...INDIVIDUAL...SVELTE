import { clienteService } from '../services/clienteService.js'
import { auth } from '../js/auth.js'
import Swal from 'sweetalert2'

export class Page {
  constructor() {
    this.clientes = []
    this.filteredClientes = []
    this.currentPage = 1
    this.itemsPerPage = 10
    this.searchTerm = ''
    this.isAdmin = auth.isAdmin()
    this.editingId = null
  }

  render() {
    return `
      <div class="clientes-page">
        <div class="page-header" style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px;">
          <h1 class="page-title" style="margin: 0;"><i class="fas fa-users"></i> Clientes</h1>
          ${this.isAdmin ? '<button class="btn btn-primary" id="newClienteBtn" style="padding: 10px 20px; background: #3B82F6; color: white; border: none; border-radius: 6px; cursor: pointer;">Nuevo Cliente</button>' : ''}
        </div>

        <div class="card" style="background: white; padding: 20px; border-radius: 8px; box-shadow: 0 2px 8px rgba(0,0,0,0.1);">
          <div class="card-header" style="margin-bottom: 15px;">
            <input 
              type="text" 
              id="searchInput" 
              class="form-control search-input" 
              placeholder="Buscar por nombre, documento, email..." 
              style="padding: 10px; border: 1px solid #E2E8F0; border-radius: 6px; width: 100%; font-size: 14px;"
            />
          </div>

          <div class="table-container" style="overflow-x: auto;">
            <table class="table" style="width: 100%; border-collapse: collapse;">
              <thead style="background: #F8FAFC;">
                <tr>
                  <th style="padding: 12px; text-align: left; border-bottom: 2px solid #E2E8F0; font-weight: 600;">Nombre</th>
                  <th style="padding: 12px; text-align: left; border-bottom: 2px solid #E2E8F0; font-weight: 600;">Documento</th>
                  <th style="padding: 12px; text-align: left; border-bottom: 2px solid #E2E8F0; font-weight: 600;">Email</th>
                  <th style="padding: 12px; text-align: left; border-bottom: 2px solid #E2E8F0; font-weight: 600;">Teléfono</th>
                  <th style="padding: 12px; text-align: left; border-bottom: 2px solid #E2E8F0; font-weight: 600;">Estado</th>
                  ${this.isAdmin ? '<th style="padding: 12px; text-align: left; border-bottom: 2px solid #E2E8F0; font-weight: 600;">Acciones</th>' : ''}
                </tr>
              </thead>
              <tbody id="clientesTableBody">
                <tr><td colspan="${this.isAdmin ? 6 : 5}" style="padding: 12px; text-align: center; color: #64748B;">Cargando...</td></tr>
              </tbody>
            </table>
          </div>

          <div class="pagination" id="pagination" style="margin-top: 15px; text-align: center;"></div>
        </div>
      </div>

      <!-- Modal -->
      <div id="clienteModal" style="display:none; position: fixed; top: 0; left: 0; right: 0; bottom: 0; background: rgba(0,0,0,0.5); z-index: 1000; display: none;">
        <div style="position: absolute; top: 50%; left: 50%; transform: translate(-50%, -50%); background: white; padding: 30px; border-radius: 8px; width: 90%; max-width: 500px; box-shadow: 0 20px 25px rgba(0,0,0,0.15);">
          <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px;">
            <h2 id="modalTitle" style="margin: 0; color: #1E293B;">Nuevo Cliente</h2>
            <button id="modalClose" style="background: none; border: none; font-size: 24px; cursor: pointer; color: #64748B;">&times;</button>
          </div>
          <form id="clienteForm" style="display: grid; gap: 15px;">
            <div>
              <label style="display: block; margin-bottom: 5px; font-weight: 600; color: #0F172A;">Nombre</label>
              <input type="text" id="nombre" required style="width: 100%; padding: 10px; border: 1px solid #E2E8F0; border-radius: 6px; font-size: 14px;" />
            </div>
            <div>
              <label style="display: block; margin-bottom: 5px; font-weight: 600; color: #0F172A;">Documento</label>
              <input type="text" id="documento" required style="width: 100%; padding: 10px; border: 1px solid #E2E8F0; border-radius: 6px; font-size: 14px;" />
            </div>
            <div>
              <label style="display: block; margin-bottom: 5px; font-weight: 600; color: #0F172A;">Email</label>
              <input type="email" id="email" style="width: 100%; padding: 10px; border: 1px solid #E2E8F0; border-radius: 6px; font-size: 14px;" />
            </div>
            <div>
              <label style="display: block; margin-bottom: 5px; font-weight: 600; color: #0F172A;">Teléfono</label>
              <input type="text" id="telefono" style="width: 100%; padding: 10px; border: 1px solid #E2E8F0; border-radius: 6px; font-size: 14px;" />
            </div>
            <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 10px;  margin-top: 10px;">
              <button type="submit" style="padding: 10px; background: #3B82F6; color: white; border: none; border-radius: 6px; cursor: pointer; font-weight: 600;">Guardar</button>
              <button type="button" id="formCancel" style="padding: 10px; background: #E2E8F0; color: #0F172A; border: none; border-radius: 6px; cursor: pointer; font-weight: 600;">Cancelar</button>
            </div>
          </form>
        </div>
      </div>
    `
  }

  init() {
    console.log('[CLIENTES] Inicializando página...')
    
    this.loadClientes()
    
    // Eventos
    const searchInput = document.getElementById('searchInput')
    if (searchInput) {
      searchInput.addEventListener('input', (e) => {
        this.searchTerm = e.target.value.toLowerCase()
        this.filterAndRender()
      })
    }

    const newBtn = document.getElementById('newClienteBtn')
    if (newBtn) {
      newBtn.addEventListener('click', () => this.openCreateModal())
    }

    const formCancel = document.getElementById('formCancel')
    if (formCancel) {
      formCancel.addEventListener('click', () => this.closeModal())
    }

    const modalClose = document.getElementById('modalClose')
    if (modalClose) {
      modalClose.addEventListener('click', () => this.closeModal())
    }

    const form = document.getElementById('clienteForm')
    if (form) {
      form.addEventListener('submit', (e) => this.saveCliente(e))
    }
  }

  async loadClientes() {
    try {
      const response = await clienteService.getAll()
      this.clientes = response.data || []
      this.filterAndRender()
    } catch (error) {
      console.error('[CLIENTES] Error cargando:', error)
      Swal.fire('Error', 'No se pudieron cargar los clientes', 'error')
    }
  }

  filterAndRender() {
    if (!this.searchTerm) {
      this.filteredClientes = [...this.clientes]
    } else {
      this.filteredClientes = this.clientes.filter(c =>
        (c.nombre?.toLowerCase().includes(this.searchTerm)) ||
        (c.documento?.includes(this.searchTerm)) ||
        (c.email?.toLowerCase().includes(this.searchTerm))
      )
    }
    this.renderTable()
    this.renderPagination()
  }

  renderTable() {
    const start = (this.currentPage - 1) * this.itemsPerPage
    const paginated = this.filteredClientes.slice(start, start + this.itemsPerPage)
    
    const tbody = document.getElementById('clientesTableBody')
    if (!tbody) return

    if (paginated.length === 0) {
      tbody.innerHTML = `<tr><td colspan="${this.isAdmin ? 6 : 5}" style="padding: 12px; text-align: center; color: #64748B;">No se encontraron clientes</td></tr>`
      return
    }

    tbody.innerHTML = paginated.map(c => `
      <tr style="border-bottom: 1px solid #E2E8F0; hover: background #F8FAFC;">
        <td style="padding: 12px;">${c.nombre || ''}</td>
        <td style="padding: 12px;">${c.documento || ''}</td>
        <td style="padding: 12px;">${c.email || 'N/A'}</td>
        <td style="padding: 12px;">${c.telefono || 'N/A'}</td>
        <td style="padding: 12px;"><span style="background: ${c.activo ? '#DEF7EC' : '#FEE2E2'}; color: ${c.activo ? '#047857' : '#991B1B'}; padding: 4px 8px; border-radius: 4px; font-size: 12px; font-weight: 600;">${c.activo ? 'Activo' : 'Inactivo'}</span></td>
        ${this.isAdmin ? `
          <td style="padding: 12px;">
            <button class="edit-btn" data-id="${c.id}" style="padding: 6px 12px; background: #FCD34D; color: #00; border: none; border-radius: 4px; cursor: pointer; font-size: 12px; margin-right: 5px;">Editar</button>
            <button class="delete-btn" data-id="${c.id}" style="padding: 6px 12px; background: #FCA5A5; color: #7F1D1D; border: none; border-radius: 4px; cursor: pointer; font-size: 12px;">Eliminar</button>
          </td>
        ` : ''}
      </tr>
    `).join('')

    // Eventos de botones
    document.querySelectorAll('.edit-btn').forEach(btn => {
      btn.addEventListener('click', () => this.openEditModal(btn.dataset.id))
    })
    document.querySelectorAll('.delete-btn').forEach(btn => {
      btn.addEventListener('click', () => this.deleteCliente(btn.dataset.id))
    })
  }

  renderPagination() {
    const totalPages = Math.ceil(this.filteredClientes.length / this.itemsPerPage)
    const pagination = document.getElementById('pagination')
    if (!pagination) return

    let html = ''
    for (let i = 1; i <= totalPages; i++) {
      html += `<button style="padding: 6px 12px; margin: 0 2px; background: ${i === this.currentPage ? '#3B82F6' : '#E2E8F0'}; color: ${i === this.currentPage ? 'white' : '#0F172A'}; border: none; border-radius: 4px; cursor: pointer;">${i}</button>`
    }
    pagination.innerHTML = html

    pagination.querySelectorAll('button').forEach((btn, i) => {
      btn.addEventListener('click', () => {
        this.currentPage = i + 1
        this.renderTable()
      })
    })
  }

  openCreateModal() {
    this.editingId = null
    document.getElementById('modalTitle').textContent = 'Nuevo Cliente'
    document.getElementById('clienteForm').reset()
    document.getElementById('clienteModal').style.display = 'block'
  }

  openEditModal(id) {
    const cliente = this.clientes.find(c => c.id == id)
    if (!cliente) return

    this.editingId = id
    document.getElementById('modalTitle').textContent = 'Editar Cliente'
    document.getElementById('nombre').value = cliente.nombre || ''
    document.getElementById('documento').value = cliente.documento || ''
    document.getElementById('email').value = cliente.email || ''
    document.getElementById('telefono').value = cliente.telefono || ''
    document.getElementById('clienteModal').style.display = 'block'
  }

  closeModal() {
    document.getElementById('clienteModal').style.display = 'none'
  }

  async saveCliente(e) {
    e.preventDefault()
    
    const data = {
      nombre: document.getElementById('nombre').value,
      documento: document.getElementById('documento').value,
      email: document.getElementById('email').value,
      telefono: document.getElementById('telefono').value
    }

    try {
      if (this.editingId) {
        await clienteService.update(this.editingId, data)
        Swal.fire('Éxito', 'Cliente actualizado', 'success')
      } else {
        await clienteService.create(data)
        Swal.fire('Éxito', 'Cliente creado', 'success')
      }
      this.closeModal()
      await this.loadClientes()
    } catch (error) {
      Swal.fire('Error', error.message, 'error')
    }
  }

  async deleteCliente(id) {
    const result = await Swal.fire({
      title: '¿Está seguro?',
      text: 'Esta acción no se puede deshacer',
      icon: 'warning',
      showCancelButton: true,
      confirmButtonText: 'Eliminar'
    })

    if (result.isConfirmed) {
      try {
        await clienteService.delete(id)
        Swal.fire('Éxito', 'Cliente eliminado', 'success')
        await this.loadClientes()
      } catch (error) {
        Swal.fire('Error', error.message, 'error')
      }
    }
  }
}
