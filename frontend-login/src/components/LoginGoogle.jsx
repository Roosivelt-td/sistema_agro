import { useState } from 'react';

const LoginGoogle = ({ onVerificationSuccess }) => {
  const [email, setEmail] = useState('');
  const [isLoading, setIsLoading] = useState(false);
  const [showVerification, setShowVerification] = useState(false);
  const [verificationCode, setVerificationCode] = useState(['', '', '', '', '', '']);

  // Simular inicio de sesión con Google
  const handleGoogleLogin = async () => {
    setIsLoading(true);
    
    // Simulamos una llamada al backend para iniciar sesión con Google
    setTimeout(() => {
      setIsLoading(false);
      setShowVerification(true); // Mostrar campo de verificación después del inicio de sesión
      console.log('Inicio de sesión con Google solicitado para:', email);
    }, 1500);
  };

  const handleCodeChange = (index, value) => {
    if (/^[0-9]$/.test(value) || value === '') {
      const newCode = [...verificationCode];
      newCode[index] = value;
      setVerificationCode(newCode);

      // Mover al siguiente campo si se ingresó un dígito
      if (value && index < 5) {
        const nextInput = document.getElementById(`code-${index + 1}`);
        if (nextInput) nextInput.focus();
      }
    }
  };

  const handleVerifyCode = (e) => {
    e.preventDefault();
    const code = verificationCode.join('');
    
    // Simulamos verificación del código con el backend
    if (code.length === 6) {
      console.log('Código de verificación recibido:', code);
      onVerificationSuccess && onVerificationSuccess(code);
    } else {
      alert('Por favor, ingrese el código completo de 6 dígitos');
    }
  };

  return (
    <div className="google-auth-container">
      {!showVerification ? (
        <div className="google-login-form">
          <h3>Iniciar sesión con Google</h3>
          <div className="input-group">
            <label htmlFor="google-email">Email:</label>
            <input
              type="email"
              id="google-email"
              value={email}
              onChange={(e) => setEmail(e.target.value)}
              placeholder="Ingresa tu email de Google"
              required
            />
          </div>
          <button 
            type="button" 
            onClick={handleGoogleLogin}
            disabled={isLoading || !email}
            className="google-login-btn"
          >
            {isLoading ? 'Procesando...' : 'Iniciar sesión con Google'}
          </button>
        </div>
      ) : (
        <div className="verification-form">
          <h3>Verificación de Seguridad</h3>
          <p>Se ha enviado un código de verificación a {email}</p>
          
          <form onSubmit={handleVerifyCode}>
            <div className="verification-code-inputs">
              {verificationCode.map((digit, index) => (
                <input
                  key={index}
                  id={`code-${index}`}
                  type="text"
                  maxLength="1"
                  value={digit}
                  onChange={(e) => handleCodeChange(index, e.target.value)}
                  className="code-digit"
                />
              ))}
            </div>
            <button type="submit">Verificar Código</button>
          </form>
          
          <p className="resend-code">
            ¿No recibiste el código? <button type="button" className="link-button">Reenviar</button>
          </p>
        </div>
      )}
    </div>
  );
};

export default LoginGoogle;