// Configuración de la API
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api';

// Funciones para el manejo de autenticación
export const authService = {
  // Iniciar sesión
  login: async (email, password) => {
    const response = await fetch(`${API_BASE_URL}/auth/login`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({ email, password }),
    });

    if (!response.ok) {
      throw new Error(`Error en el inicio de sesión: ${response.status}`);
    }

    return await response.json();
  },

  // Verificar código de Google
  verifyGoogleCode: async (email, verificationCode) => {
    const response = await fetch(`${API_BASE_URL}/auth/verify-google-code`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({ email, verificationCode }),
    });

    if (!response.ok) {
      throw new Error(`Error en la verificación: ${response.status}`);
    }

    return await response.json();
  },
};