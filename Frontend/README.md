# Sistema de Facturación - Frontend Vue.js

Frontend desarrollado en Vue.js 3 para el sistema de facturación y punto de venta.

## 🚀 Tecnologías Utilizadas

- **Vue.js 3** - Framework JavaScript progresivo
- **Vue Router** - Enrutamiento
- **Pinia** - State Management
- **Axios** - Cliente HTTP
- **Vee-Validate & Yup** - Validación de formularios
- **SweetAlert2** - Alertas elegantes
- **jsPDF** - Generación de PDFs
- **Vite** - Build tool

## 📦 Instalación

```bash
# Instalar dependencias
npm install

# Ejecutar en modo desarrollo
npm run dev

# Compilar para producción
npm run build

# Previsualizar build de producción
npm run preview
```

## 🔧 Configuración

El proyecto está configurado para conectarse al backend en `http://localhost:5000`

Puedes modificar la URL del API en `src/config/api.js`

## 📁 Estructura del Proyecto

```
src/
├── assets/          # Estilos y recursos estáticos
├── components/      # Componentes reutilizables
├── config/          # Configuración de la aplicación
├── router/          # Configuración de rutas
├── services/        # Servicios API
├── stores/          # Stores de Pinia
├── utils/           # Utilidades
├── views/           # Vistas/Páginas
├── App.vue          # Componente raíz
└── main.js          # Punto de entrada
```

## 👤 Usuario por Defecto

Para iniciar sesión, usa las credenciales que hayas configurado en el backend.

## 🔐 Características de Seguridad

- Autenticación JWT
- Protección de rutas
- Control de roles (Administrador/Usuario)
- Validación de contraseñas (4-10 caracteres con mayúscula, minúscula, número y símbolo)
- Bloqueo de cuenta tras 3 intentos fallidos

## 📋 Funcionalidades

### Administrador
- ✅ Login con JWT
- ✅ Gestión de Clientes (CRUD)
- ✅ Gestión de Productos (CRUD)
- ✅ Gestión de Ventas/Facturas (Crear, Ver, Imprimir PDF)
- ✅ Gestión de Usuarios (CRUD)
- ✅ Gestión de Roles
- ✅ Desbloqueo de cuentas
- ✅ Reporte de errores

### Usuario
- ✅ Login
- ✅ Visualización de Clientes
- ✅ Visualización de Productos
- ✅ Visualización de Ventas

## 🎨 Diseño

Interfaz moderna y responsiva con diseño limpio y profesional.
# frontend_Factu
