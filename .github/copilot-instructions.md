<!-- Use this file to provide workspace-specific custom instructions to Copilot. For more details, visit https://code.visualstudio.com/docs/copilot/copilot-customization#_use-a-githubcopilotinstructionsmd-file -->

# Demo de Autenticación - .NET 8 Web API + React

Este proyecto es una demo completa de autenticación que incluye:

## Backend (.NET 8 Web API)
- Arquitectura limpia (Domain, Application, Infrastructure, API)
- Autenticación JWT
- OAuth social login (Google, Instagram, GitHub)
- MFA con TOTP (Google Authenticator/Authy)
- PostgreSQL con Entity Framework Core

## Frontend (React)
- Pantallas de login y registro
- Integración con OAuth providers
- Configuración y validación de MFA
- Manejo de tokens JWT y refresh tokens

## Estructura del Proyecto
```
demo-autenticacion/
├── backend/
│   ├── src/
│   │   ├── Domain/
│   │   ├── Application/
│   │   ├── Infrastructure/
│   │   └── API/
│   └── tests/
└── frontend/
    ├── src/
    │   ├── components/
    │   ├── pages/
    │   ├── services/
    │   └── contexts/
    └── public/
```

## Funcionalidades Implementadas
- ✅ Registro y login con usuario/contraseña
- ✅ Autenticación JWT con refresh tokens  
- ✅ OAuth con Google, Instagram y GitHub
- ✅ MFA con TOTP (QR, setup, verificación)
- ✅ Base de datos PostgreSQL
- ✅ Frontend React completo
- ✅ CORS y Swagger configurados