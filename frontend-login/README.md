# Sistema de Autenticación con Verificación por Código

Este es un frontend de login desarrollado con React y Vite que implementa un sistema de autenticación con verificación por código, similar al que tiene tu backend. La aplicación incluye las siguientes funcionalidades:

## Características

- **Inicio de sesión tradicional**: Con email y contraseña
- **Registro de usuarios**: Con validación de contraseña
- **Recuperación de contraseña**: Con envío de código de verificación
- **Inicio de sesión con Google**: Con verificación por código de 6 dígitos
- **Verificación por código**: Interfaz intuitiva para ingresar códigos de 6 dígitos

## Estructura del Proyecto

```
frontend-login/
├── src/
│   ├── components/
│   │   └── LoginGoogle.jsx      # Componente de autenticación con Google
│   ├── App.jsx                  # Componente principal con vistas de login, registro, etc.
│   ├── App.css                  # Estilos generales
│   └── main.jsx                 # Punto de entrada de la aplicación
├── public/
├── index.html                   # Página HTML principal
├── vite.config.js               # Configuración de Vite
└── package.json                 # Dependencias y scripts
```

## Instalación

1. Asegúrate de tener Node.js instalado en tu sistema
2. Clona o descarga este repositorio
3. Navega al directorio del proyecto
4. Ejecuta el siguiente comando para instalar las dependencias:

```bash
npm install
```

## Ejecución

Para iniciar la aplicación en modo desarrollo:

```bash
npm run dev
```

Luego abre tu navegador en `http://localhost:5173` (o el puerto que indique la consola).

## Funcionalidades Implementadas

### 1. Inicio de Sesión
- Formulario con campos de email y contraseña
- Opción para iniciar sesión con Google
- Enlaces para registro y recuperación de contraseña

### 2. Registro de Usuario
- Formulario con campos de email, contraseña y confirmación
- Validación de coincidencia de contraseñas

### 3. Recuperación de Contraseña
- Formulario para ingresar email
- Flujo que lleva a la pantalla de verificación

### 4. Verificación por Código
- Interfaz intuitiva para ingresar códigos de 6 dígitos
- Campo por cada dígito del código
- Auto-focus al siguiente campo al ingresar un dígito

### 5. Autenticación con Google
- Flujo especializado para autenticación con Google
- Pantalla de verificación específica para códigos de Google
- Integración con el sistema de verificación general

## Personalización

Para integrar con tu backend real:

1. Modifica las funciones de submit en cada formulario para llamar a tus endpoints API
2. Actualiza las URLs y parámetros según la configuración de tu backend
3. Implementa la lógica de manejo de tokens y almacenamiento de sesión

## Conexión con el backend

Para conectar este frontend con tu backend, necesitas implementar las siguientes rutas API:

- `POST /api/auth/login` - Para iniciar sesión con email y contraseña
- `POST /api/auth/register` - Para registrar nuevos usuarios
- `POST /api/auth/forgot-password` - Para solicitar recuperación de contraseña
- `POST /api/auth/verify-code` - Para verificar códigos de verificación
- `POST /api/auth/google-login` - Para iniciar sesión con Google
- `POST /api/auth/verify-google-code` - Para verificar códigos de Google

Los endpoints deben responder con:
- Código 200 para operaciones exitosas
- Código de error para operaciones fallidas
- JSON con mensajes de error si es necesario

## Configuración de proxy para desarrollo

Durante el desarrollo, puedes configurar un proxy para redirigir las solicitudes API a tu backend.
Actualiza el archivo `vite.config.js` agregando la configuración de proxy:

```javascript
import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

// https://vitejs.dev/config/
export default defineConfig({
  plugins: [react()],
  server: {
    proxy: {
      '/api': {
        target: 'http://localhost:tu_puerto_backend', // Reemplaza con la URL de tu backend
        changeOrigin: true,
        secure: false,
      }
    }
  }
});
```

## Tecnologías Utilizadas

- React 18
- Vite como build tool
- JavaScript ES6+
- CSS para estilos

## Notas

Este frontend simula la funcionalidad de autenticación con verificación por código. Para conectarlo con tu backend real, deberás implementar las llamadas a tus APIs correspondientes en cada acción del usuario.