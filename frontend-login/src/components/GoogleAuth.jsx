import { useState } from 'react';

const GoogleAuth = ({ onLoginSuccess, onVerificationSuccess }) => {
  const [email, setEmail] = useState('');
  const [step, setStep] = useState('login'); // 'login' o 'verification'
  const [verificationCode, setVerificationCode] = useState('');
  const [loading, setLoading] = useState(false);

  const handleGoogleLogin = async (e) => {
    e.preventDefault();
    setLoading(true);
    
    try {
      // Llamada al backend para iniciar el proceso de autenticación de Google
      const response = await fetch('/api/auth/google', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ email }),
      });

      if (response.ok) {
        setStep('verification');
        onLoginSuccess && onLoginSuccess(email); // Callback opcional
      } else {
        const errorData = await response.json();
        alert(errorData.message || 'Error al iniciar sesión con Google');
      }
    } catch (error) {
      console.error('Error:', error);
      alert('Error de conexión con el servidor');
    } finally {
      setLoading(false);
    }
  };

  const handleVerification = async (e) => {
    e.preventDefault();
    setLoading(true);
    
    try {
      // Llamada al backend para verificar el código
      const response = await fetch('/api/auth/verify', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({ email, verificationCode }),
      });

      if (response.ok) {
        const data = await response.json();
        localStorage.setItem('token', data.token);
        onVerificationSuccess && onVerificationSuccess(data); // Callback opcional
      } else {
        const errorData = await response.json();
        alert(errorData.message || 'Código de verificación incorrecto');
      }
    } catch (error) {
      console.error('Error:', error);
      alert('Error de conexión con el servidor');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="google-auth-container">
      {step === 'login' ? (
        <form onSubmit={handleGoogleLogin} className="login-form">
          <h2>Iniciar Sesión con Google</h2>
          <div className="input-group">
            <input
              type="email"
              placeholder="Ingresa tu correo electrónico"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              required
              disabled={loading}
            />
          </div>
          <button type="submit" className="google-btn" disabled={loading}>
            {loading ? 'Procesando...' : (
              <>
                <span className="google-icon">G</span>
                Continuar con Google
              </>
            )}
          </button>
        </form>
      ) : (
        <form onSubmit={handleVerification} className="verification-form">
          <h2>Verificación de Código</h2>
          <p>Se ha enviado un código de verificación a <strong>{email}</strong></p>
          
          <div className="input-group">
            <input
              type="text"
              placeholder="Código de 6 dígitos"
              value={verificationCode}
              onChange={(e) => setVerificationCode(e.target.value)}
              maxLength="6"
              required
              disabled={loading}
            />
          </div>
          
          <div className="button-group">
            <button type="submit" disabled={loading}>
              {loading ? 'Verificando...' : 'Verificar Código'}
            </button>
            
            <button 
              type="button" 
              className="secondary-btn"
              onClick={() => setStep('login')}
              disabled={loading}
            >
              Cambiar Correo
            </button>
          </div>
        </form>
      )}
    </div>
  );
};

export default GoogleAuth;