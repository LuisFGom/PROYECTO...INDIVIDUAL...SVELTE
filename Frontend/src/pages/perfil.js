// Modal de Perfil de Usuario
import Swal from 'sweetalert2'

export const perfilModal = {
  init() {
    this.attachUserInfoListener()
  },

  attachUserInfoListener() {
    const userInfoBtn = document.getElementById('userInfoBtn')
    if (userInfoBtn) {
      userInfoBtn.addEventListener('click', () => this.showUserProfile())
    }
  },

  getUserData() {
    try {
      const user = JSON.parse(localStorage.getItem('currentUser') || '{}')
      return {
        nombre: user.nombre || user.name || 'Usuario',
        email: user.email || 'No disponible',
        cedula: user.cedula || user.ci || 'No disponible',
        rol: user.rol || 'Usuario',
        telefono: user.telefono || 'No disponible',
        direccion: user.direccion || 'No disponible'
      }
    } catch (error) {
      console.error('[PERFIL] Error obteniendo datos:', error)
      return {
        nombre: 'Usuario',
        email: 'No disponible',
        cedula: 'No disponible',
        rol: 'Usuario',
        telefono: 'No disponible',
        direccion: 'No disponible'
      }
    }
  },

  showUserProfile() {
    const user = this.getUserData()

    const profileHTML = `
      <div style="text-align: left; padding: 20px;">
        <div style="display: flex; align-items: center; gap: 20px; margin-bottom: 25px;">
          <div style="font-size: 60px; color: #3B82F6;">
            <i class="fas fa-user-circle"></i>
          </div>
          <div>
            <h3 style="margin: 0; color: #1E293B; font-size: 20px;">${user.nombre}</h3>
            <p style="margin: 5px 0; color: #64748B; font-size: 14px;">${user.rol}</p>
          </div>
        </div>

        <div style="background: #F1F5F9; padding: 15px; border-radius: 8px; margin-bottom: 15px;">
          <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 15px;">
            <div>
              <label style="display: block; font-size: 12px; font-weight: 600; color: #64748B; margin-bottom: 5px;">Email</label>
              <p style="margin: 0; color: #1E293B; word-break: break-all;">${user.email}</p>
            </div>
            <div>
              <label style="display: block; font-size: 12px; font-weight: 600; color: #64748B; margin-bottom: 5px;">Cédula</label>
              <p style="margin: 0; color: #1E293B;">${user.cedula}</p>
            </div>
            <div>
              <label style="display: block; font-size: 12px; font-weight: 600; color: #64748B; margin-bottom: 5px;">Teléfono</label>
              <p style="margin: 0; color: #1E293B;">${user.telefono}</p>
            </div>
            <div>
              <label style="display: block; font-size: 12px; font-weight: 600; color: #64748B; margin-bottom: 5px;">Rol</label>
              <p style="margin: 0; color: #1E293B;">${user.rol}</p>
            </div>
          </div>
        </div>

        <div>
          <label style="display: block; font-size: 12px; font-weight: 600; color: #64748B; margin-bottom: 5px;">Dirección</label>
          <p style="margin: 0; color: #1E293B;">${user.direccion}</p>
        </div>
      </div>
    `

    Swal.fire({
      title: 'Mi Perfil',
      html: profileHTML,
      icon: 'info',
      confirmButtonColor: '#3B82F6',
      confirmButtonText: 'Cerrar',
      customClass: {
        popup: 'swal2-custom-profile'
      }
    })
  }
}
