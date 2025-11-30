# Demo de Autenticación - .NET 8 Web API + React

Sistema completo de autenticación con soporte para login tradicional, OAuth social y MFA (Multi-Factor Authentication).

## 🚀 Características

- ✅ Registro y login con usuario/contraseña
- ✅ Autenticación JWT con refresh tokens
- ✅ OAuth 2.0 con Google, GitHub y Discord
- ✅ MFA con TOTP (Google Authenticator/Authy)
- ✅ Base de datos MySQL
- ✅ Arquitectura limpia (Clean Architecture)
- ✅ Docker support

## 📋 Requisitos

- Docker y Docker Compose
- .NET 9 SDK (para desarrollo local)
- Node.js 18+ (para desarrollo local)
- MySQL 8.0 (para desarrollo local)

## 🔧 Configuración Inicial

### 1. Clonar el repositorio

```bash
git clone <tu-repositorio>
cd demo-autenticacion
```

### 2. Configurar variables de entorno

Copia el archivo `.env.example` y renómbralo a `.env`:

```bash
cp .env.example .env
```

Edita el archivo `.env` y configura tus credenciales OAuth:

```env
# OAuth - Google
GOOGLE_CLIENT_ID=tu-google-client-id
GOOGLE_CLIENT_SECRET=tu-google-client-secret

# OAuth - GitHub
GITHUB_CLIENT_ID=tu-github-client-id
GITHUB_CLIENT_SECRET=tu-github-client-secret

# OAuth - Discord
DISCORD_CLIENT_ID=tu-discord-client-id
DISCORD_CLIENT_SECRET=tu-discord-client-secret
```

### 3. Obtener credenciales OAuth

#### Google OAuth
1. Ve a [Google Cloud Console](https://console.cloud.google.com/)
2. Crea un nuevo proyecto
3. Habilita Google+ API
4. Crea credenciales OAuth 2.0
5. Agrega URI de redirección: `http://localhost:3000`

#### GitHub OAuth
1. Ve a [GitHub Developer Settings](https://github.com/settings/developers)
2. Crea una nueva OAuth App
3. Authorization callback URL: `http://localhost:3000/auth/github/callback`

#### Discord OAuth
1. Ve a [Discord Developer Portal](https://discord.com/developers/applications)
2. Crea una nueva aplicación
3. En OAuth2, agrega redirect: `http://localhost:3000/auth/discord/callback`
4. Scopes necesarios: `identify`, `email`

Ver [DISCORD_OAUTH_SETUP.md](./DISCORD_OAUTH_SETUP.md) para más detalles.

## 🐳 Ejecución con Docker

### Producción (Todo en Docker)

```bash
# Iniciar todos los servicios
docker-compose up --build

# Detener servicios
docker-compose down
```

Accede a:
- **Frontend**: http://localhost:3000
- **Backend API**: http://localhost:5000
- **Swagger**: http://localhost:5000/swagger
- **PHPMyAdmin**: http://localhost:8081

### Desarrollo (Solo Base de Datos)

```bash
# Iniciar solo MySQL y PHPMyAdmin
docker-compose -f docker-compose.dev.yml up

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

# Configurar base de datos
# Editar src/API/appsettings.Development.json con tu connection string

# Ejecutar migraciones (si existen)
dotnet ef database update --project src/Infrastructure --startup-project src/API

# Ejecutar API
dotnet run --project src/API
```

Backend disponible en: http://localhost:5000

### Frontend

```bash
cd frontend

# Instalar dependencias
npm install

# Copiar archivo de variables de entorno
cp .env.example .env

# Editar .env con tus Client IDs
# REACT_APP_GOOGLE_CLIENT_ID=...
# REACT_APP_GITHUB_CLIENT_ID=...
# REACT_APP_DISCORD_CLIENT_ID=...

# Ejecutar en modo desarrollo
npm start
```

Frontend disponible en: http://localhost:3000

## 📁 Estructura del Proyecto

```
demo-autenticacion/
├── backend/                    # API .NET 9
│   ├── src/
│   │   ├── Domain/            # Entidades y lógica de negocio
│   │   ├── Application/       # DTOs, interfaces y servicios
│   │   ├── Infrastructure/    # Implementaciones y DbContext
│   │   └── API/              # Controllers y configuración
│   ├── Dockerfile
│   └── .env.example
├── frontend/                   # React + TypeScript
│   ├── src/
│   │   ├── components/       # Componentes reutilizables
│   │   ├── pages/           # Páginas principales
│   │   ├── contexts/        # Context API (Auth)
│   │   ├── services/        # API client (Axios)
│   │   └── config/          # Configuración OAuth
│   ├── Dockerfile
│   ├── nginx.conf
│   └── .env.example
├── database/
│   └── init.sql             # Script inicial de BD
├── docker-compose.yml        # Producción
├── docker-compose.dev.yml    # Desarrollo
├── .env.example
├── .gitignore
└── README.md
```

## 🔐 Seguridad

### Archivos sensibles (NO commitear)

Los siguientes archivos están en `.gitignore`:
- `.env`
- `backend/appsettings.Development.json`
- `frontend/.env.local`
- `backend/src/API/appsettings.Development.json`

### Configuración de producción

Para producción, asegúrate de:
1. Cambiar todas las contraseñas y secrets
2. Usar HTTPS
3. Configurar CORS apropiadamente
4. Usar secretos de al menos 256 bits para JWT
5. Habilitar rate limiting
6. Actualizar las URIs de redirección OAuth

## 📚 Endpoints Principales

### Autenticación
- `POST /auth/register` - Registro de usuario
- `POST /auth/login` - Login con credenciales
- `POST /auth/refresh` - Renovar access token
- `POST /auth/logout` - Cerrar sesión

### OAuth
- `POST /auth/external/google` - Login con Google
- `POST /auth/external/github` - Login con GitHub
- `POST /auth/external/discord` - Login con Discord

### MFA
- `POST /mfa/setup` - Configurar MFA
- `POST /mfa/verify-setup` - Verificar configuración MFA
- `POST /mfa/verify` - Verificar código MFA
- `POST /mfa/disable` - Deshabilitar MFA

## 🧪 Testing

```bash
# Backend tests
cd backend
dotnet test

# Frontend tests
cd frontend
npm test
```

## 📝 Notas

- El modo demo está habilitado por defecto en desarrollo
- Los tokens JWT expiran en 15 minutos
- Los refresh tokens duran 7 días
- MFA usa algoritmo TOTP con 6 dígitos

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
