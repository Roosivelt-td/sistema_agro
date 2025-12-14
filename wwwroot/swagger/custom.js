(function() {
    console.log('🔧 Swagger Custom JS cargado');

    // Función para mostrar notificaciones
    function showNotification(type, message, duration = 5000) {
        const colors = {
            success: '#28a745',
            error: '#dc3545',
            warning: '#ffc107',
            info: '#17a2b8'
        };

        // Remover notificación anterior
        const old = document.getElementById('custom-notification');
        if (old) old.remove();

        // Crear nueva
        const notification = document.createElement('div');
        notification.id = 'custom-notification';
        notification.style.cssText = `
            position: fixed;
            top: 80px;
            right: 20px;
            padding: 15px 20px;
            border-radius: 5px;
            z-index: 99999;
            font-weight: bold;
            background: ${colors[type] || colors.info};
            color: white;
            box-shadow: 0 4px 12px rgba(0,0,0,0.15);
            animation: slideIn 0.3s ease;
            max-width: 500px;
            word-break: break-word;
            border-left: 5px solid ${type === 'success' ? '#1e7e34' : type === 'error' ? '#bd2130' : type === 'warning' ? '#d39e00' : '#138496'};
        `;

        notification.innerHTML = `
            <div style="display: flex; align-items: center; justify-content: space-between;">
                <span>${message}</span>
                <button onclick="this.parentElement.parentElement.remove()" 
                        style="background: rgba(255,255,255,0.2); border: none; color: white; cursor: pointer; padding: 2px 8px; margin-left: 15px; border-radius: 3px;">
                    ✕
                </button>
            </div>
        `;

        document.body.appendChild(notification);

        // Auto-remover
        if (duration > 0) {
            setTimeout(() => {
                if (notification.parentElement) {
                    notification.style.opacity = '0';
                    notification.style.transform = 'translateX(100px)';
                    setTimeout(() => notification.remove(), 300);
                }
            }, duration);
        }

        // Agregar estilos de animación
        if (!document.querySelector('#notification-styles')) {
            const style = document.createElement('style');
            style.id = 'notification-styles';
            style.textContent = `
                @keyframes slideIn {
                    from { transform: translateX(100px); opacity: 0; }
                    to { transform: translateX(0); opacity: 1; }
                }
            `;
            document.head.appendChild(style);
        }
    }

    // Decodificar token JWT
    function decodeToken(token) {
        try {
            // Limpiar "Bearer " si existe
            token = token.replace(/^Bearer\s+/i, '').trim();
            const parts = token.split('.');
            if (parts.length !== 3) return null;

            const payload = JSON.parse(atob(parts[1]));
            return {
                email: payload.email || payload.unique_name,
                role: payload.role,
                userId: payload.userId,
                exp: payload.exp ? new Date(payload.exp * 1000) : null
            };
        } catch (e) {
            console.error('Error decodificando token:', e);
            return null;
        }
    }

    // Función para probar token
    window.testToken = function() {
        // Buscar token en el modal
        const modal = document.querySelector('.dialog-ux');
        let token = '';

        if (modal) {
            const input = modal.querySelector('input[type="text"]');
            token = input ? input.value : '';
        }

        if (!token || token.length < 10) {
            showNotification('warning', '⚠️ Ingresa un token primero', 4000);
            return;
        }

        showNotification('info', '🧪 Probando token...', 3000);

        // Decodificar para mostrar info
        const decoded = decodeToken(token);
        if (decoded) {
            console.log('Token decodificado:', decoded);
        }

        // Limpiar token (remover Bearer si está)
        const cleanToken = token.replace(/^Bearer\s+/i, '').trim();

        // Hacer petición de prueba
        fetch('/api/Usuarios/profile', {
            headers: {
                'Authorization': 'Bearer ' + cleanToken,
                'Content-Type': 'application/json'
            }
        })
            .then(response => {
                if (response.ok) {
                    return response.json().then(data => {
                        showNotification('success',
                            `✅ AUTENTICACIÓN EXITOSA\n👤 ${data.nombre}\n📧 ${data.email}\n🎯 ${data.rol}`,
                            6000
                        );
                    });
                } else {
                    return response.text().then(text => {
                        showNotification('error',
                            `❌ ERROR ${response.status}\n${text.substring(0, 100)}`,
                            6000
                        );
                    });
                }
            })
            .catch(error => {
                showNotification('error', `❌ Error de conexión:\n${error.message}`, 6000);
            });
    };

    // Observar cuando se abre el modal de autorización
    const observer = new MutationObserver(function(mutations) {
        mutations.forEach(function(mutation) {
            if (mutation.type === 'childList') {
                const modal = document.querySelector('.dialog-ux .modal-ux');
                if (modal) {
                    // Agregar botón de prueba
                    setTimeout(() => {
                        const authorizeBtn = modal.querySelector('.btn.authorize');
                        if (authorizeBtn && !modal.querySelector('#test-btn')) {
                            const testBtn = authorizeBtn.cloneNode(true);
                            testBtn.id = 'test-btn';
                            testBtn.textContent = '🧪 Probar';
                            testBtn.style.background = '#ff9800';
                            testBtn.style.marginLeft = '10px';
                            testBtn.onclick = window.testToken;
                            authorizeBtn.parentNode.appendChild(testBtn);

                            // Agregar instrucciones
                            const modalContent = modal.querySelector('.modal-ux-content');
                            if (modalContent) {
                                const instructions = document.createElement('div');
                                instructions.style.cssText = `
                                    background: #e8f4fd;
                                    border-left: 4px solid #2196F3;
                                    padding: 10px 15px;
                                    margin-bottom: 15px;
                                    border-radius: 4px;
                                    font-size: 14px;
                                `;
                                instructions.innerHTML = `
                                    <strong>💡 Instrucciones:</strong><br>
                                    • Pega el token completo <strong>con "Bearer"</strong><br>
                                    • Ejemplo: <code>Bearer eyJhbGciOiJIUz...</code><br>
                                    • Usa "🧪 Probar" para verificar
                                `;
                                modalContent.insertBefore(instructions, modalContent.firstChild);
                            }
                        }
                    }, 100);
                }
            }
        });
    });

    observer.observe(document.body, { childList: true, subtree: true });

    // Mensaje inicial
    setTimeout(() => {
        showNotification('info', '🔧 Swagger personalizado listo. Usa "Authorize" para configurar token.', 6000);
    }, 2000);
    function addPasswordRequirements() {
        const modal = document.querySelector('.dialog-ux .modal-ux');
        if (modal) {
            const passwordInfo = document.createElement('div');
            passwordInfo.innerHTML = `
            <div style="
                background: #fff3cd;
                border-left: 4px solid #ffc107;
                padding: 10px 15px;
                margin: 10px 0;
                border-radius: 4px;
                font-size: 12px;
                color: #856404;
            ">
                <strong>🔒 Requisitos de contraseña:</strong>
                <ul style="margin: 5px 0 0 15px; padding: 0;">
                    <li>Mínimo 8 caracteres</li>
                    <li>Al menos una mayúscula (A-Z)</li>
                    <li>Al menos una minúscula (a-z)</li>
                    <li>Al menos un número (0-9)</li>
                    <li>Al menos un carácter especial (!@#$%^&*)</li>
                    <li>Sin espacios</li>
                </ul>
            </div>
        `;

            const modalContent = modal.querySelector('.modal-ux-content');
            if (modalContent) {
                modalContent.appendChild(passwordInfo);
            }
        }
    }
})();