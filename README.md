# Demo de Autenticación - .NET 9 Web API + React

Sistema completo de autenticación con soporte para login tradicional, OAuth social y MFA (Multi-Factor Authentication).

## 🚀 Características

- ✅ Registro y login con usuario/contraseña
- ✅ Autenticación JWT con refresh tokens
- ✅ OAuth 2.0 con Google, GitHub y Discord
- ✅ MFA con TOTP (Google Authenticator/Authy)
- ✅ Base de datos MySQL 8.0
- ✅ Arquitectura limpia (Clean Architecture)
- ✅ Despliegue completo con Docker
- ✅ Frontend React con TypeScript y Tailwind CSS

## 📋 Requisitos

- Docker y Docker Compose
- .NET 9 SDK (opcional, para desarrollo local)
- Node.js 20+ (opcional, para desarrollo local)

## 🔧 Configuración Inicial

### 1. Clonar el repositorio

```bash
git clone https://github.com/kamaro600/demo-autenticacion.git
cd demo-autenticacion
```

### 2. Configurar Backend (appsettings.json)

Copia el archivo de ejemplo y configura tus credenciales OAuth:

```bash
cd backend/src/API
cp appsettings.example.json appsettings.json
cp appsettings.example.json appsettings.Development.json
```

Edita `appsettings.json` y `appsettings.Development.json` con tus credenciales:

```json
{
  "OAuth": {
    "Google": {
      "ClientId": "tu-google-client-id.apps.googleusercontent.com",
      "ClientSecret": "tu-google-client-secret",
      "RedirectUri": "http://localhost:3000/auth/google/callback"
    },
    "GitHub": {
      "ClientId": "tu-github-client-id",
      "ClientSecret": "tu-github-client-secret",
      "RedirectUri": "http://localhost:3000/auth/github/callback"
    },
    "Discord": {
      "ClientId": "tu-discord-client-id",
      "ClientSecret": "tu-discord-client-secret",
      "RedirectUri": "http://localhost:3000/auth/discord/callback"
    }
  }
}
```

### 3. Configurar Frontend (.env)

Copia el archivo de ejemplo:

```bash
cd frontend
cp .env.example .env
```

Edita `frontend/.env` con tus Client IDs:

```env
REACT_APP_API_URL=http://localhost:5000/api

REACT_APP_GOOGLE_CLIENT_ID=tu-google-client-id.apps.googleusercontent.com
REACT_APP_GITHUB_CLIENT_ID=tu-github-client-id
REACT_APP_DISCORD_CLIENT_ID=tu-discord-client-id

REACT_APP_GITHUB_REDIRECT_URI=http://localhost:3000/auth/github/callback
REACT_APP_DISCORD_REDIRECT_URI=http://localhost:3000/auth/discord/callback
```

### 4. Obtener credenciales OAuth

#### Google OAuth
1. Ve a [Google Cloud Console](https://console.cloud.google.com/)
2. Crea un nuevo proyecto
3. Habilita Google+ API
4. Crea credenciales OAuth 2.0
5. Agrega URIs de redirección autorizadas:
   - `http://localhost:3000`
   - `http://localhost:3000/auth/google/callback`

#### GitHub OAuth
1. Ve a [GitHub Developer Settings](https://github.com/settings/developers)
2. Crea una nueva OAuth App
3. Homepage URL: `http://localhost:3000`
4. Authorization callback URL: `http://localhost:3000/auth/github/callback`

#### Discord OAuth
1. Ve a [Discord Developer Portal](https://discord.com/developers/applications)
2. Crea una nueva aplicación
3. En OAuth2, agrega redirect: `http://localhost:3000/auth/discord/callback`
4. Scopes necesarios: `identify`, `email`

Ver [DISCORD_OAUTH_SETUP.md](./DISCORD_OAUTH_SETUP.md) para más detalles.

## 🐳 Ejecución con Docker (Recomendado)

### Modo Producción (Todo en Docker)

```bash
# Iniciar todos los servicios
docker-compose up --build

# Detener servicios
docker-compose down
```

**Servicios disponibles:**
- 🌐 **Frontend (React)**: http://localhost:3000
- 🚀 **Backend API**: http://localhost:5000/api
- 📚 **Swagger UI**: http://localhost:5000/swagger
- 🗄️ **PHPMyAdmin**: http://localhost:8081 (usuario: `root`, contraseña: `rootpassword`)
- 💾 **MySQL**: `localhost:3306`

### Modo Desarrollo (Solo Base de Datos en Docker)

Útil para desarrollo con hot-reload:

```bash
# Iniciar solo MySQL y PHPMyAdmin
docker-compose -f docker-compose.dev.yml up -d

# En otra terminal, ejecutar backend
cd backend
dotnet run --project src/API

# En otra terminal, ejecutar frontend
cd frontend
npm install
npm start
```

## 💻 Desarrollo Local (Sin Docker)

### Backend

```bash
cd backend

# Restaurar dependencias
dotnet restore

# Configurar appsettings (ver paso 2 de configuración inicial)
# Asegúrate de tener MySQL corriendo localmente en puerto 3306

# Ejecutar API
dotnet run --project src/API
```

✅ Backend disponible en: http://localhost:5000  
📚 Swagger UI: http://localhost:5000/swagger

### Frontend

```bash
cd frontend

# Instalar dependencias
npm install

# Configurar .env (ver paso 3 de configuración inicial)

# Ejecutar en modo desarrollo
npm start
```

✅ Frontend disponible en: http://localhost:3000

## 📁 Estructura del Proyecto

```
demo-autenticacion/
├── backend/                          # API .NET 9
│   ├── src/
│   │   ├── Domain/                  # Entidades y lógica de negocio
│   │   ├── Application/             # DTOs, interfaces y servicios de aplicación
│   │   ├── Infrastructure/          # DbContext, repositorios, servicios externos
│   │   └── API/                     # Controllers, Program.cs, configuración
│   │       ├── appsettings.example.json    # ⚠️ Template con placeholders
│   │       ├── appsettings.json            # ⚠️ Credenciales reales (ignorado por git)
│   │       └── appsettings.Development.json # ⚠️ Credenciales dev (ignorado por git)
│   ├── Dockerfile
│   └── .gitignore
├── frontend/                         # React 18 + TypeScript + Tailwind
│   ├── src/
│   │   ├── components/              # Componentes de OAuth (GoogleLoginButton, etc.)
│   │   ├── pages/                   # Login, Register, Dashboard, MfaSetup
│   │   ├── contexts/                # AuthContext (gestión de autenticación)
│   │   ├── services/                # api.ts (Axios client)
│   │   └── config/                  # google.ts, github.ts, discord.ts
│   ├── public/
│   ├── Dockerfile
│   ├── .env.example                 # ⚠️ Template de variables de entorno
│   ├── .env                         # ⚠️ Variables reales (ignorado por git)
│   ├── package.json
│   └── .gitignore
├── database/
│   └── init.sql                     # Script de inicialización MySQL
├── docker-compose.yml               # Producción (mysql + backend + frontend + phpmyadmin)
├── docker-compose.dev.yml           # Desarrollo (solo mysql + phpmyadmin)
├── .gitignore
└── README.md
```

## 🔐 Seguridad

### ⚠️ Archivos con credenciales (NO están en git)

Estos archivos contienen secretos y están excluidos del repositorio:
- ✅ `backend/src/API/appsettings.json`
- ✅ `backend/src/API/appsettings.Development.json`
- ✅ `frontend/.env`

### 📄 Archivos de ejemplo (SÍ están en git)

Usa estos templates para crear tus archivos de configuración:
- `backend/src/API/appsettings.example.json`
- `frontend/.env.example`

### Configuración de producción

Para producción, asegúrate de:
1. Cambiar todas las contraseñas y secrets (especialmente JWT SecretKey)
2. Usar HTTPS
3. Configurar CORS apropiadamente
4. Usar secretos de al menos 256 bits para JWT
5. Habilitar rate limiting
6. Actualizar las URIs de redirección OAuth

## 📚 Endpoints Principales

### Autenticación
- `POST /api/auth/register` - Registro de usuario
- `POST /api/auth/login` - Login con credenciales
- `POST /api/auth/refresh` - Renovar access token
- `POST /api/auth/logout` - Cerrar sesión

### OAuth
- `POST /api/auth/external/google` - Login con Google
- `POST /api/auth/external/github` - Login con GitHub
- `POST /api/auth/external/discord` - Login con Discord

### MFA
- `POST /api/mfa/setup` - Configurar MFA
- `POST /api/mfa/verify-setup` - Verificar configuración MFA
- `POST /api/mfa/verify` - Verificar código MFA
- `POST /api/mfa/disable` - Deshabilitar MFA

## 🛠️ Stack Tecnológico

### Backend
- .NET 9 Web API
- Entity Framework Core 9
- MySQL 8.0 (Pomelo.EntityFrameworkCore.MySql)
- JWT Authentication
- Swagger/OpenAPI

### Frontend
- React 18
- TypeScript
- Tailwind CSS
- Axios
- React Router

### DevOps
- Docker & Docker Compose
- Multi-stage builds
- PHPMyAdmin para gestión de BD

## 📝 Notas Importantes

- ⚙️ El backend usa `Environment=Development` en Docker para exponer Swagger
- 🔑 Los tokens JWT expiran en 60 minutos
- 🔄 Los refresh tokens duran 7 días
- 🔐 MFA usa algoritmo TOTP (RFC 6238) con 6 dígitos
- 🌐 CORS configurado para `localhost:3000` y `frontend:3000`
- 📦 Frontend usa `serve` en Docker (puerto 3000) en lugar de nginx

## 🤝 Contribuir

Este es un proyecto de demostración educativa. Siéntete libre de:
- Hacer fork del proyecto
- Crear issues para sugerencias
- Enviar pull requests con mejoras

## 📄 Licencia

Este proyecto es de código abierto para fines educativos.

## 📧 Contacto

Si tienes preguntas sobre la implementación, revisa la documentación adicional:
- [Configuración Discord OAuth](./DISCORD_OAUTH_SETUP.md)

---

**Happy Coding!** 🚀
