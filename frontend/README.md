# Frontend de Autenticación con Verificación de Google

Este proyecto es un frontend de autenticación completo que incluye funcionalidades de login, registro y recuperación de contraseña con verificación de Google.

## Características

- Inicio de sesión con verificación de Google
- Registro de nuevos usuarios con verificación por código
- Recuperación de contraseña con verificación por código
- Validación de correo electrónico existente
- Interfaz de usuario responsive y amigable

## Estructura del Proyecto

```
frontend/
├── public/
├── src/
│   ├── components/
│   │   └── LoginRegisterForm.jsx
│   │   └── LoginRegisterForm.css
│   ├── api.js
│   ├── App.jsx
│   └── main.jsx
├── .env.example
├── package.json
└── README.md
```

## Instalación

1. Asegúrate de tener Node.js instalado en tu sistema
2. Instala las dependencias del proyecto:
```bash
npm install
```

## Configuración

1. Crea un archivo `.env` basado en `.env.example`
2. Configura la URL de tu backend:
```env
VITE_API_BASE_URL=http://localhost:5000
```

## Variables de Entorno

- `VITE_API_BASE_URL`: URL base de la API del backend

## Endpoints Requeridos del Backend

Para que este frontend funcione correctamente, tu backend debe proporcionar los siguientes endpoints:

- `POST /api/auth/login` - Iniciar sesión
- `POST /api/auth/verify-google-code` - Verificar código de Google
- `POST /api/auth/check-email` - Verificar si un correo existe
- `POST /api/auth/send-verification-code` - Enviar código de verificación
- `POST /api/auth/verify-code` - Verificar código de verificación
- `POST /api/auth/register` - Registrar nuevo usuario
- `POST /api/auth/forgot-password` - Iniciar proceso de recuperación de contraseña
- `POST /api/auth/reset-password` - Restablecer contraseña

## Funcionalidades

### Inicio de Sesión
1. El usuario ingresa su correo y contraseña
2. Si el backend requiere verificación de Google, se solicita el código
3. Se completa el proceso de autenticación

### Registro
1. El usuario ingresa su correo electrónico
2. Se verifica si el correo ya existe
3. Si no existe, se envía un código de verificación
4. Con el código correcto, se completa el registro con nombre y contraseña

### Recuperación de Contraseña
1. El usuario ingresa su correo electrónico
2. Se envía un código de verificación
3. Con el código correcto, se permite cambiar la contraseña

## Desarrollo

Para iniciar el servidor de desarrollo:
```bash
npm run dev
```

## Tecnologías Utilizadas

- React
- JavaScript
- CSS
- Vite
