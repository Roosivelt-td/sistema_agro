import './App.css';
import GoogleAuth from './components/GoogleAuth';

function App() {
  const handleLoginSuccess = (email) => {
    console.log('Inicio de sesión con Google exitoso para:', email);
  };

  const handleVerificationSuccess = (data) => {
    console.log('Verificación exitosa:', data);
    // Redirigir al dashboard o página principal
    window.location.href = '/dashboard';
  };

  return (
    <div className="app">
      <div className="login-container">
        <h1>Sistema de Gestión Agrícola</h1>
        <GoogleAuth 
          onLoginSuccess={handleLoginSuccess}
          onVerificationSuccess={handleVerificationSuccess}
        />
      </div>
    </div>
  );
}

export default App;