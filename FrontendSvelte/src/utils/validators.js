// Validadores
export const validators = {
  isEmail(email) {
    if (!email || email.trim() === '') return true // Email es opcional
    // Antes del @: letras, números, punto, guión, guión bajo
    // Después del @: formato estándar
    const re = /^[a-zA-Z0-9._-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/
    return re.test(email)
  },

  isCedula(cedula) {
    // Formato: XXXXXXXXXX-X (10 dígitos totales)
    const numeros = cedula.replace(/[^0-9]/g, '')
    return numeros.length === 10
  },

  isPhone(phone) {
    // Teléfono: 7-10 dígitos
    const numeros = phone.replace(/[^\d]/g, '')
    return numeros.length >= 7 && numeros.length <= 10
  },

  isPasswordStrong(password) {
    // Mínimo 8 caracteres, mayúscula, minúscula, número, símbolo
    return (
      password.length >= 8 &&
      /[A-Z]/.test(password) &&
      /[a-z]/.test(password) &&
      /[0-9]/.test(password) &&
      /[@$!%*?&]/.test(password)
    )
  },

  isRequired(value) {
    return value && value.toString().trim().length > 0
  },

  minLength(value, min) {
    return value && value.toString().length >= min
  },

  maxLength(value, max) {
    return value && value.toString().length <= max
  },

  isOnlyLetters(value) {
    if (!value) return true
    const re = /^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$/
    return re.test(value)
  },

  isAddressLength(value) {
    return !value || value.length <= 100
  }
}

// Formateadores
export const formatters = {
  formatCedula(cedula) {
    // Formato: XXXXXXXXXX-X (10 dígitos + guión al final)
    if (!cedula) return ''
    const numeros = cedula.replace(/[^0-9]/g, '').substring(0, 10)
    if (numeros.length === 0) return ''
    if (numeros.length < 10) return numeros
    return numeros.substring(0, 9) + '-' + numeros.substring(9, 10)
  },

  formatPhone(phone) {
    // Formato: 09-XXXX-XXXX (7-10 dígitos)
    if (!phone) return ''
    const numeros = phone.replace(/[^0-9]/g, '').substring(0, 10)
    if (numeros.length === 0) return ''
    if (numeros.length <= 2) return numeros
    if (numeros.length <= 6) return numeros.substring(0, 2) + '-' + numeros.substring(2)
    return numeros.substring(0, 2) + '-' + numeros.substring(2, 6) + '-' + numeros.substring(6)
  },

  removeOnlyLetters(text) {
    // Acepta solo letras, espacios y acentos
    if (!text) return ''
    return text.replace(/[^a-zA-ZáéíóúÁÉÍÓÚñÑ\s]/g, '')
  },

  formatDate(date) {
    if (!date) return ''
    const d = new Date(date)
    return d.toLocaleDateString('es-ES')
  },

  formatDateTime(date) {
    if (!date) return ''
    const d = new Date(date)
    const fechaFormato = d.toLocaleDateString('es-ES')
    const horaFormato = d.toLocaleTimeString('es-ES', { hour: '2-digit', minute: '2-digit', second: '2-digit' })
    return `${fechaFormato} ${horaFormato}`
  },

  formatCurrency(amount) {
    if (!amount) return '$0.00'
    const numero = parseFloat(amount).toFixed(2)
    return `$${numero.replace(/\B(?=(\d{3})+(?!\d))/g, ',')}`
  }
}
