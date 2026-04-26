#!/usr/bin/env node

// Generador de cédulas ecuatorianas válidas (Algoritmo Módulo 10)

function generarCedulaValida(provincia = 17) {
  // Validar provincia
  if (provincia < 1 || provincia > 24) {
    throw new Error('Provincia debe estar entre 01 y 24')
  }

  // Crear provincia con 2 dígitos
  const provinciaStr = String(provincia).padStart(2, '0')
  
  // Tercer dígito: 0-5 (personas naturales)
  const tercerDigito = Math.floor(Math.random() * 6)
  
  // Dígitos 4-9: números secuenciales aleatorios
  const digitos4a9 = String(Math.floor(Math.random() * 1000000)).padStart(6, '0')
  
  // Primeros 9 dígitos
  const primeros9 = provinciaStr + tercerDigito + digitos4a9
  
  // Calcular dígito verificador (Módulo 10)
  const factores = [2, 1, 2, 1, 2, 1, 2, 1, 2]
  let suma = 0
  
  for (let i = 0; i < 9; i++) {
    let digito = parseInt(primeros9[i])
    let producto = digito * factores[i]
    
    if (producto >= 10) {
      producto -= 9
    }
    
    suma += producto
  }
  
  const residuo = suma % 10
  let digitoVerificador = 10 - residuo
  
  if (residuo === 0) {
    digitoVerificador = 0
  }
  
  return primeros9 + digitoVerificador
}

// Generar 10 cédulas válidas
console.log('Cédulas ecuatorianas VÁLIDAS (Algoritmo Módulo 10)\n')
console.log('=' .repeat(50))

const cedulas = new Set()
while (cedulas.size < 10) {
  const cedula = generarCedulaValida(Math.floor(Math.random() * 24) + 1)
  cedulas.add(cedula)
}

cedulas.forEach((cedula, index) => {
  console.log(`${index + 1}. ${cedula}`)
})

console.log('\n' + '='.repeat(50))
console.log('Todas estas cédulas son matemáticamente válidas')
