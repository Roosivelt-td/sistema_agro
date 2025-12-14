import React, { useState } from 'react';
import { authService } from '../api';
import './LoginRegisterForm.css';

const LoginRegisterForm = () => {
  const [currentView, setCurrentView] = useState('login'); // 'login', 'register', 'forgot-password', 'verification'
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [confirmPassword, setConfirmPassword] = useState('');
  const [name, setName] = useState('');
  const [verificationCode, setVerificationCode] = useState('');
  const [message, setMessage] = useState('');
  const [loading, setLoading] = useState(false);

  const handleEmailCheck = async () => {
    setLoading(true);
    try {
      const data = await authService.checkEmail(email);
      
      if (data.exists) {
        // Si el correo existe, ir a la pantalla de recuperación de contraseña
        setCurrentView('forgot-password');
        setMessage('Se ha enviado un código de verificación a tu correo electrónico');
        
        // Enviar código de verificación para recuperación
        await authService.sendVerificationCode(email, 'recovery');
      } else {
        // Si el correo no existe, ir a la pantalla de registro
        setCurrentView('verification');
        setMessage('Por favor, verifica tu correo electrónico para registrarte');
        
        // Enviar código de verificación para registro
        await authService.sendVerificationCode(email, 'registration');
      }
    } catch (error) {
      setMessage(error.message);
    } finally {
      setLoading(false);
    }
  };

  const handleVerification = async () => {
    setLoading(true);
    try {
      const data = await authService.verifyCode(email, verificationCode);
      
      if (currentView === 'verification') {
        // Código correcto para registro - mostrar formulario completo de registro
        setCurrentView('register-complete');
      } else if (currentView === 'forgot-password') {
        // Código correcto para recuperación - permitir cambio de contraseña
        setCurrentView('reset-password');
      }
    } catch (error) {
      setMessage(error.message);
    } finally {
      setLoading(false);
    }
  };

  const handleRegisterComplete = async () => {
    if (password !== confirmPassword) {
      setMessage('Las contraseñas no coinciden');
      return;
    }

    setLoading(true);
    try {
      await authService.register({ 
        email, 
        password, 
        name,
        verificationCode 
      });
      
      setMessage('Registro completado exitosamente');
      setCurrentView('login');
    } catch (error) {
      setMessage(error.message);
    } finally {
      setLoading(false);
    }
  };

  const handleForgotPassword = async () => {
    setLoading(true);
    try {
      await authService.forgotPassword(email);
      setCurrentView('forgot-password');
      setMessage('Se ha enviado un código de verificación a tu correo electrónico');
    } catch (error) {
      setMessage(error.message);
    } finally {
      setLoading(false);
    }
  };

  const handleResetPassword = async () => {
    if (password !== confirmPassword) {
      setMessage('Las contraseñas no coinciden');
      return;
    }

    setLoading(true);
    try {
      await authService.resetPassword(email, password, verificationCode);
      setMessage('Contraseña restablecida exitosamente');
      setCurrentView('login');
    } catch (error) {
      setMessage(error.message);
    } finally {
      setLoading(false);
    }
  };

  const handleLogin = async (e) => {
    e.preventDefault();
    setLoading(true);
    
    try {
      const data = await authService.login(email, password);
      
      if (data.requiresGoogleVerification) {
        setCurrentView('google-verification');
        setMessage('Por favor, ingresa el código de verificación de Google');
      } else {
        setMessage('Inicio de sesión exitoso');
        // Aquí puedes redirigir al usuario o guardar el token
      }
    } catch (error) {
      setMessage(error.message);
    } finally {
      setLoading(false);
    }
  };

  const handleGoogleVerification = async (e) => {
    e.preventDefault();
    setLoading(true);
    
    try {
      await authService.verifyGoogleCode(email, verificationCode);
      setMessage('Verificación exitosa. Bienvenido!');
      // Aquí puedes redirigir al usuario o guardar el token
      setCurrentView('login');
    } catch (error) {
      setMessage(error.message);
    } finally {
      setLoading(false);
    }
  };

  const renderCurrentView = () => {
    switch (currentView) {
      case 'login':
        return (
          <div className="form-container">
            <h2>Iniciar Sesión</h2>
            <form onSubmit={handleLogin}>
              <input
                type="email"
                placeholder="Correo electrónico"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
              />
              <input
                type="password"
                placeholder="Contraseña"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                required
              />
              <button type="submit" disabled={loading}>
                {loading ? 'Iniciando sesión...' : 'Iniciar Sesión'}
              </button>
            </form>
            <div className="links">
              <button onClick={() => setCurrentView('register')}>¿No tienes cuenta? Regístrate aquí</button>
              <button onClick={() => setCurrentView('forgot-password-email')}>¿Olvidaste tu contraseña?</button>
            </div>
          </div>
        );
      
      case 'register':
        return (
          <div className="form-container">
            <h2>Registrar Usuario</h2>
            <p>Ingresa tu correo electrónico para comenzar el proceso de registro</p>
            <form>
              <input
                type="email"
                placeholder="Correo electrónico"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
              />
              <button type="button" onClick={handleEmailCheck} disabled={loading}>
                {loading ? 'Procesando...' : 'Continuar'}
              </button>
            </form>
            <div className="links">
              <button onClick={() => setCurrentView('login')}>¿Ya tienes cuenta? Inicia sesión</button>
            </div>
          </div>
        );
      
      case 'forgot-password-email':
        return (
          <div className="form-container">
            <h2>Recuperar Contraseña</h2>
            <p>Ingresa tu correo electrónico para recibir un código de verificación</p>
            <form>
              <input
                type="email"
                placeholder="Correo electrónico"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
              />
              <button type="button" onClick={handleForgotPassword} disabled={loading}>
                {loading ? 'Enviando...' : 'Enviar Código'}
              </button>
            </form>
            <div className="links">
              <button onClick={() => setCurrentView('login')}>Volver al inicio de sesión</button>
            </div>
          </div>
        );
      
      case 'verification':
        return (
          <div className="form-container">
            <h2>Verificación de Registro</h2>
            <p>Se ha enviado un código de verificación a {email}. El código expira en 10 minutos.</p>
            <form>
              <input
                type="text"
                placeholder="Código de verificación"
                value={verificationCode}
                onChange={(e) => setVerificationCode(e.target.value)}
                required
              />
              <button type="button" onClick={handleVerification} disabled={loading}>
                {loading ? 'Verificando...' : 'Verificar'}
              </button>
            </form>
            <div className="links">
              <button onClick={() => setCurrentView('register')}>Cambiar correo electrónico</button>
            </div>
          </div>
        );
      
      case 'register-complete':
        return (
          <div className="form-container">
            <h2>Completa tu Registro</h2>
            <form>
              <input
                type="text"
                placeholder="Nombre completo"
                value={name}
                onChange={(e) => setName(e.target.value)}
                required
              />
              <input
                type="password"
                placeholder="Contraseña (mínimo 8 caracteres, mayúsculas, minúsculas y números)"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                required
              />
              <input
                type="password"
                placeholder="Confirmar contraseña"
                value={confirmPassword}
                onChange={(e) => setConfirmPassword(e.target.value)}
                required
              />
              <button type="button" onClick={handleRegisterComplete} disabled={loading}>
                {loading ? 'Registrando...' : 'Completar Registro'}
              </button>
            </form>
            <div className="links">
              <button onClick={() => setCurrentView('login')}>Cancelar y volver al inicio</button>
            </div>
          </div>
        );
      
      case 'forgot-password':
        return (
          <div className="form-container">
            <h2>Verificación para Recuperación</h2>
            <p>Se ha enviado un código de verificación a {email}. El código expira en 10 minutos.</p>
            <form>
              <input
                type="text"
                placeholder="Código de verificación"
                value={verificationCode}
                onChange={(e) => setVerificationCode(e.target.value)}
                required
              />
              <button type="button" onClick={handleVerification} disabled={loading}>
                {loading ? 'Verificando...' : 'Verificar'}
              </button>
            </form>
            <div className="links">
              <button onClick={() => setCurrentView('forgot-password-email')}>Cambiar correo electrónico</button>
            </div>
          </div>
        );
      
      case 'reset-password':
        return (
          <div className="form-container">
            <h2>Restablecer Contraseña</h2>
            <form>
              <input
                type="password"
                placeholder="Nueva contraseña"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                required
              />
              <input
                type="password"
                placeholder="Confirmar nueva contraseña"
                value={confirmPassword}
                onChange={(e) => setConfirmPassword(e.target.value)}
                required
              />
              <button type="button" onClick={handleResetPassword} disabled={loading}>
                {loading ? 'Restableciendo...' : 'Restablecer Contraseña'}
              </button>
            </form>
            <div className="links">
              <button onClick={() => setCurrentView('login')}>Cancelar y volver al inicio</button>
            </div>
          </div>
        );
      
      case 'google-verification':
        return (
          <div className="form-container">
            <h2>Verificación de Google</h2>
            <p>Ingresa el código de verificación de Google</p>
            <form onSubmit={handleGoogleVerification}>
              <input
                type="text"
                placeholder="Código de verificación de Google"
                value={verificationCode}
                onChange={(e) => setVerificationCode(e.target.value)}
                required
              />
              <button type="submit" disabled={loading}>
                {loading ? 'Verificando...' : 'Verificar'}
              </button>
            </form>
            <div className="links">
              <button onClick={() => setCurrentView('login')}>Volver al inicio de sesión</button>
            </div>
          </div>
        );
      
      default:
        return null;
    }
  };

  return (
    <div className="login-register-form">
      {renderCurrentView()}
      {message && <div className="message">{message}</div>}
    </div>
  );
};

export default LoginRegisterForm;