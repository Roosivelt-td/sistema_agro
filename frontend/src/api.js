// src/api.js
const API_BASE_URL = import.meta.env.VITE_API_BASE_URL;

export const authService = {
  // Iniciar sesión
  login: async (email, password) => {
    const response = await fetch(`${API_BASE_URL}/api/auth/login`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({ email, password }),
    });

    if (!response.ok) {
      const errorData = await response.json();
      throw new Error(errorData.message || 'Error en el inicio de sesión');
    }

    return response.json();
  },

  // Verificar código de Google
  verifyGoogleCode: async (email, googleCode) => {
    const response = await fetch(`${API_BASE_URL}/api/auth/verify-google-code`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({ email, googleCode }),
    });

    if (!response.ok) {
      const errorData = await response.json();
      throw new Error(errorData.message || 'Error en la verificación de Google');
    }

    return response.json();
  },

  // Verificar si el correo existe
  checkEmail: async (email) => {
    const response = await fetch(`${API_BASE_URL}/api/auth/check-email`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({ email }),
    });

    if (!response.ok) {
      const errorData = await response.json();
      throw new Error(errorData.message || 'Error al verificar el correo electrónico');
    }

    return response.json();
  },

  // Enviar código de verificación
  sendVerificationCode: async (email, type) => {
    const response = await fetch(`${API_BASE_URL}/api/auth/send-verification-code`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({ email, type }),
    });

    if (!response.ok) {
      const errorData = await response.json();
      throw new Error(errorData.message || 'Error al enviar el código de verificación');
    }

    return response.json();
  },

  // Verificar código
  verifyCode: async (email, code) => {
    const response = await fetch(`${API_BASE_URL}/api/auth/verify-code`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({ email, code }),
    });

    if (!response.ok) {
      const errorData = await response.json();
      throw new Error(errorData.message || 'Error al verificar el código');
    }

    return response.json();
  },

  // Registrar usuario completo
  register: async (userData) => {
    const response = await fetch(`${API_BASE_URL}/api/auth/register`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify(userData),
    });

    if (!response.ok) {
      const errorData = await response.json();
      throw new Error(errorData.message || 'Error en el registro');
    }

    return response.json();
  },

  // Recuperar contraseña (iniciar proceso)
  forgotPassword: async (email) => {
    const response = await fetch(`${API_BASE_URL}/api/auth/forgot-password`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({ email }),
    });

    if (!response.ok) {
      const errorData = await response.json();
      throw new Error(errorData.message || 'Error al iniciar proceso de recuperación de contraseña');
    }

    return response.json();
  },

  // Restablecer contraseña
  resetPassword: async (email, password, verificationCode) => {
    const response = await fetch(`${API_BASE_URL}/api/auth/reset-password`, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
      },
      body: JSON.stringify({ email, password, verificationCode }),
    });

    if (!response.ok) {
      const errorData = await response.json();
      throw new Error(errorData.message || 'Error al restablecer la contraseña');
    }

    return response.json();
  },
};