import { useState } from 'react'
import LoginGoogle from './components/LoginGoogle'
import './App.css'

function App() {
  const [currentView, setCurrentView] = useState('login') // 'login', 'register', 'forgot-password', 'verification'

  return (
    <div className="app">
      <div className="container">
        <h1>Sistema de Autenticación</h1>
        
        {currentView === 'login' && <LoginView setCurrentView={setCurrentView} />}
        {currentView === 'register' && <RegisterView setCurrentView={setCurrentView} />}
        {currentView === 'forgot-password' && <ForgotPasswordView setCurrentView={setCurrentView} />}
        {currentView === 'verification' && <VerificationView setCurrentView={setCurrentView} />}
      </div>
    </div>
  )
}

// Vista de inicio de sesión
const LoginView = ({ setCurrentView }) => {
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [showGoogleAuth, setShowGoogleAuth] = useState(false)

  const handleSubmit = (e) => {
    e.preventDefault()
    // Lógica de inicio de sesión aquí
    console.log('Iniciar sesión con:', email, password)
  }

  const handleGoogleAuthSuccess = (code) => {
    console.log('Autenticación con Google completada con código:', code);
    // Aquí iría la lógica para manejar el éxito de la autenticación
    // Por ejemplo, redirigir al usuario o mostrar un mensaje
  }

  return (
    <div className="form-container">
      <h2>Iniciar Sesión</h2>
      
      {!showGoogleAuth ? (
        <>
          <form onSubmit={handleSubmit}>
            <div className="input-group">
              <label htmlFor="email">Email:</label>
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
          
          <div className="divider">O</div>
          
          <div className="google-auth-option">
            <button 
              type="button" 
              onClick={() => setShowGoogleAuth(true)}
              className="google-auth-btn"
            >
              Iniciar sesión con Google
            </button>
          </div>
          
          <div className="links">
            <button onClick={() => setCurrentView('register')} className="link-button">
              ¿No tienes cuenta? Regístrate aquí
            </button>
            <button onClick={() => setCurrentView('forgot-password')} className="link-button">
              ¿Olvidaste tu contraseña?
            </button>
          </div>
        </>
      ) : (
        <LoginGoogle onVerificationSuccess={handleGoogleAuthSuccess} />
      )}
    </div>
  )
}

// Vista de registro
const RegisterView = ({ setCurrentView }) => {
  const [email, setEmail] = useState('')
  const [password, setPassword] = useState('')
  const [confirmPassword, setConfirmPassword] = useState('')

  const handleSubmit = (e) => {
    e.preventDefault()
    if (password !== confirmPassword) {
      alert('Las contraseñas no coinciden')
      return
    }
    // Lógica de registro aquí
    console.log('Registrar usuario con:', email, password)
  }

  return (
    <div className="form-container">
      <h2>Registro de Usuario</h2>
      <form onSubmit={handleSubmit}>
        <div className="input-group">
          <label htmlFor="reg-email">Email:</label>
          <input
            type="email"
            id="reg-email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required
          />
        </div>
        <div className="input-group">
          <label htmlFor="reg-password">Contraseña:</label>
          <input
            type="password"
            id="reg-password"
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            required
          />
        </div>
        <div className="input-group">
          <label htmlFor="confirm-password">Confirmar Contraseña:</label>
          <input
            type="password"
            id="confirm-password"
            value={confirmPassword}
            onChange={(e) => setConfirmPassword(e.target.value)}
            required
          />
        </div>
        <button type="submit">Registrarse</button>
      </form>
      
      <div className="links">
        <button onClick={() => setCurrentView('login')} className="link-button">
          ¿Ya tienes cuenta? Inicia sesión aquí
        </button>
      </div>
    </div>
  )
}

// Vista de recuperación de contraseña
const ForgotPasswordView = ({ setCurrentView }) => {
  const [email, setEmail] = useState('')

  const handleSubmit = (e) => {
    e.preventDefault()
    // Lógica para recuperar contraseña aquí
    console.log('Recuperar contraseña para:', email)
    setCurrentView('verification') // Pasar a la vista de verificación
  }

  return (
    <div className="form-container">
      <h2>Recuperar Contraseña</h2>
      <p>Ingresa tu email para recibir un código de verificación</p>
      <form onSubmit={handleSubmit}>
        <div className="input-group">
          <label htmlFor="forgot-email">Email:</label>
          <input
            type="email"
            id="forgot-email"
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            required
          />
        </div>
        <button type="submit">Enviar Código</button>
      </form>
      
      <div className="links">
        <button onClick={() => setCurrentView('login')} className="link-button">
          Volver al inicio de sesión
        </button>
      </div>
    </div>
  )
}

// Vista de verificación de código
const VerificationView = ({ setCurrentView }) => {
  const [code, setCode] = useState(['', '', '', '', '', ''])

  const handleCodeChange = (index, value) => {
    if (/^[0-9]$/.test(value) || value === '') {
      const newCode = [...code]
      newCode[index] = value
      setCode(newCode)

      // Mover al siguiente campo si se ingresó un dígito
      if (value && index < 5) {
        document.getElementById(`code-${index + 1}`).focus()
      }
    }
  }

  const handleSubmit = (e) => {
    e.preventDefault()
    const verificationCode = code.join('')
    // Lógica de verificación aquí
    console.log('Código de verificación:', verificationCode)
  }

  return (
    <div className="form-container">
      <h2>Verificación de Código</h2>
      <p>Ingresa el código de verificación enviado a tu correo electrónico</p>
      <form onSubmit={handleSubmit}>
        <div className="verification-code-inputs">
          {code.map((digit, index) => (
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
      
      <div className="links">
        <button onClick={() => setCurrentView('login')} className="link-button">
          Volver al inicio de sesión
        </button>
      </div>
    </div>
  )
}

export default App