# Guía de uso del frontend de autenticación

Este proyecto contiene dos versiones del frontend de autenticación:

## 1. Versión HTML puro (login.html)

### Cómo usarla:
- Abre directamente el archivo `login.html` en tu navegador
- No requiere servidor ni instalación de dependencias
- Totalmente funcional con JavaScript vanilla
- Perfecta para pruebas rápidas o integración directa

### Ventajas:
- No requiere Node.js ni dependencias
- Funciona localmente sin servidor
- Código más simple de entender

## 2. Versión React (App.jsx)

### Cómo usarla:
1. Asegúrate de tener Node.js instalado en tu sistema
2. Abre una terminal en el directorio `/workspace/frontend-login`
3. Ejecuta `npm install` para instalar las dependencias
4. Ejecuta `npm run dev` para iniciar el servidor de desarrollo
5. Abre `http://localhost:5173` en tu navegador

### Solución de problemas comunes:

#### Problema: "Pantalla blanca"
- Asegúrate de que el servidor esté corriendo
- Revisa la consola del navegador (F12) para ver errores
- Verifica que no haya errores de CORS si estás conectando con un backend externo

#### Problema: "Error de memoria al instalar dependencias"
- Prueba con `npm ci` en lugar de `npm install`
- Si aún tienes problemas, usa la versión HTML puro (login.html)

#### Problema: "No se conecta con el backend"
- Asegúrate de que tu backend esté corriendo
- Configura el proxy en `vite.config.js` si usas la versión de React
- Verifica que las rutas API coincidan con las que espera el frontend

## Conexión con tu backend

Ambas versiones del frontend están preparadas para conectarse con tu backend que implementa:

- Inicio de sesión con verificación de código
- Registro con validación de código
- Recuperación de contraseña con código
- Autenticación con Google con verificación

### Rutas API esperadas:

- `POST /api/auth/login` - Iniciar sesión
- `POST /api/auth/register` - Registrar usuario
- `POST /api/auth/forgot-password` - Recuperar contraseña
- `POST /api/auth/verify-code` - Verificar código
- `POST /api/auth/google-login` - Iniciar sesión con Google
- `POST /api/auth/verify-google-code` - Verificar código de Google

### Personalización:

Para adaptar el frontend a tu backend específico:

1. Cambia las URLs de las rutas API en los archivos correspondientes
2. Ajusta el formato de los datos que envías y recibes
3. Modifica la lógica de manejo de respuestas según tu backend

## Personalización del estilo

Puedes personalizar el aspecto del frontend modificando:

- En la versión HTML: El CSS dentro de las etiquetas `<style>` en `login.html`
- En la versión React: El archivo `src/App.css`

## Notas importantes

- La lógica de verificación de códigos de 6 dígitos está completamente implementada
- Los formularios incluyen validación básica
- La navegación entre vistas (login, registro, recuperación, verificación) está completamente funcional
- El flujo de autenticación con Google incluye la verificación adicional por código

## Próximos pasos

Para integrar completamente con tu backend:

1. Implementa las rutas API mencionadas
2. Configura la respuesta del backend para que coincida con las expectativas del frontend
3. Ajusta los mensajes y flujos según tus necesidades específicas
4. Prueba cada funcionalidad para asegurar la correcta integración