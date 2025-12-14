import { useState } from 'react';
import { authService } from './api';
import './App.css';

function App() {
  const [email, setEmail] = useState('');
  const [password, setPassword] = useState('');
  const [verificationCode, setVerificationCode] = useState('');
  const [showVerification, setShowVerification] = useState(false);
  const [message, setMessage] = useState('');

  // Función para manejar el inicio de sesión
  const handleLogin = async (e) => {
    e.preventDefault();
    
    try {
      // Llamar al servicio de autenticación para iniciar sesión
      const response = await authService.login(email, password);
      
      // Si el backend requiere verificación de Google
      setShowVerification(true);
      setMessage('Se ha enviado un código de verificación a tu cuenta de Google');
    } catch (error) {
      setMessage(error.message || 'Error en el inicio de sesión');
    }
  };

  // Función para manejar la verificación de Google
  const handleVerification = async (e) => {
    e.preventDefault();
    
    try {
      // Enviar el código de verificación al backend
      const response = await authService.verifyGoogleCode(email, verificationCode);
      
      setMessage('Inicio de sesión exitoso');
      // Aquí redirigirías al usuario a la página principal
      console.log('Token de autenticación:', response.token); // Ejemplo de uso del token
      
    } catch (error) {
      setMessage(error.message || 'Código de verificación incorrecto');
    }
  };

  return (
    <div className="login-container">
      <div className="login-form">
        <h2>{showVerification ? 'Verificación de Google' : 'Inicio de Sesión'}</h2>
        
        {message && <div className="message">{message}</div>}
        
        {!showVerification ? (
          <form onSubmit={handleLogin}>
            <div className="input-group">
              <label htmlFor="email">Correo Electrónico:</label>
              <input
                type="email"
                id="email"
                value={email}
                onChange={(e) => setEmail(e.target.value)}
                required
              />
            </div>
            
            <div className="input-group">
              <label htmlFor="password">Contraseña:</label>
              <input
                type="password"
                id="password"
                value={password}
                onChange={(e) => setPassword(e.target.value)}
                required
              />
            </div>
            
            <button type="submit">Iniciar Sesión</button>
          </form>
        ) : (
          <form onSubmit={handleVerification}>
            <div className="input-group">
              <label htmlFor="verification">Código de Verificación:</label>
              <input
                type="text"
                id="verification"
                value={verificationCode}
                onChange={(e) => setVerificationCode(e.target.value)}
                placeholder="Ingresa el código de Google"
                required
              />
            </div>
            
            <button type="submit">Verificar</button>
            <button 
              type="button" 
              onClick={() => setShowVerification(false)}
              className="secondary-button"
            >
              Volver
            </button>
          </form>
        )}
      </div>
    </div>
  );
}

export default App;
