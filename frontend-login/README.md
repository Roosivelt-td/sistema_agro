# Frontend de Login con Autenticación de Google

Este es un frontend de login desarrollado con React y Vite que implementa un sistema de autenticación con Google mediante código de verificación.

## Características

- Inicio de sesión con Google
- Verificación por código de 6 dígitos
- Flujo de dos pasos: correo electrónico y verificación
- Componente modular reutilizable

## Requisitos

- Node.js v16 o superior
- npm o yarn

## Instalación

1. Clona o copia este repositorio
2. Navega al directorio del proyecto
3. Instala las dependencias:

```bash
npm install
```

## Ejecución en modo desarrollo

Para ejecutar el proyecto en modo desarrollo:

```bash
npm run dev
```

El servidor se iniciará en `http://localhost:5173`

## Compilación para producción

Para compilar el proyecto para producción:

```bash
npm run build
```

## Tecnologías utilizadas

- React
- Vite
- JavaScript
- CSS

## Notas sobre la integración con backend

Este frontend está diseñado para trabajar con un backend que proporcione los siguientes endpoints:

- `POST /api/auth/google`: Inicia el proceso de autenticación con Google y envía un código de verificación al correo electrónico proporcionado
- `POST /api/auth/verify`: Verifica el código de 6 dígitos enviado al usuario

## Autor

Sistema de Gestión Agrícola