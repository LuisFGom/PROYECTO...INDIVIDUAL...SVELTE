# 🚀 Guía de Despliegue en Azure - PuntoVenta

Esta guía te explica cómo desplegar tu aplicación (Vue.js + .NET Core + PostgreSQL) en Azure.

## 📋 Arquitectura de Despliegue

| Componente | Servicio Azure | Costo Estimado |
|------------|----------------|----------------|
| Frontend (Vue.js) | Azure Static Web Apps | **Gratis** (tier Free) |
| Backend (.NET Core) | Azure App Service | ~$13/mes (tier B1) |
| Base de Datos | Azure Database for PostgreSQL | ~$15/mes (tier Burstable) |

---

## 🔧 Prerrequisitos

1. **Cuenta de Azure** - [Crear cuenta gratuita](https://azure.microsoft.com/free/)
2. **Azure CLI** - [Instalar Azure CLI](https://docs.microsoft.com/cli/azure/install-azure-cli)
3. **Node.js** (para el frontend)
4. **.NET 8 SDK** (para el backend)

---

## 📦 Paso 1: Preparar el Backend

### 1.1 Crear archivo de configuración para producción

Crea el archivo `appsettings.Production.json` en `VersionM/Backend/PuntoVenta.Api/`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=TU_SERVIDOR.postgres.database.azure.com;Port=5432;Database=puntoventa;Username=TU_USUARIO;Password=TU_PASSWORD;SSL Mode=Require;Trust Server Certificate=true"
  },
  "JwtSettings": {
    "SecretKey": "UNA_CLAVE_MUY_SEGURA_DE_AL_MENOS_32_CARACTERES_PARA_PRODUCCION",
    "ExpirationHours": 24
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### 1.2 Modificar Program.cs para CORS en producción

Asegúrate de que el CORS permita tu dominio de Static Web Apps:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder
            .WithOrigins(
                "http://localhost:5173",  // Desarrollo
                "https://TU_APP.azurestaticapps.net"  // Producción
            )
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});
```

---

## ☁️ Paso 2: Crear Recursos en Azure (Azure CLI)

### 2.1 Iniciar sesión en Azure

```powershell
# Iniciar sesión
az login

# Ver suscripciones disponibles
az account list --output table

# Seleccionar suscripción (si tienes varias)
az account set --subscription "TU_SUSCRIPCION"
```

### 2.2 Crear Grupo de Recursos

```powershell
# Variables (personaliza estos valores)
$RESOURCE_GROUP = "rg-puntoventa"
$LOCATION = "eastus"  # o "westus", "centralus", etc.

# Crear grupo de recursos
az group create --name $RESOURCE_GROUP --location $LOCATION
```

### 2.3 Crear Base de Datos PostgreSQL

```powershell
# Variables para PostgreSQL
$PG_SERVER_NAME = "puntoventa-db-server"  # Debe ser único globalmente
$PG_ADMIN_USER = "pvadmin"
$PG_ADMIN_PASSWORD = "TuPassword123!"  # Mínimo 8 caracteres, mayúsculas, minúsculas, números
$PG_DATABASE = "puntoventa"

# Crear servidor PostgreSQL Flexible
az postgres flexible-server create `
    --resource-group $RESOURCE_GROUP `
    --name $PG_SERVER_NAME `
    --location $LOCATION `
    --admin-user $PG_ADMIN_USER `
    --admin-password $PG_ADMIN_PASSWORD `
    --sku-name Standard_B1ms `
    --tier Burstable `
    --storage-size 32 `
    --version 15 `
    --public-access 0.0.0.0

# Crear la base de datos
az postgres flexible-server db create `
    --resource-group $RESOURCE_GROUP `
    --server-name $PG_SERVER_NAME `
    --database-name $PG_DATABASE

# Obtener el string de conexión
Write-Host "Connection String:"
Write-Host "Host=$PG_SERVER_NAME.postgres.database.azure.com;Port=5432;Database=$PG_DATABASE;Username=$PG_ADMIN_USER;Password=$PG_ADMIN_PASSWORD;SSL Mode=Require"
```

### 2.4 Crear App Service para el Backend

```powershell
# Variables para App Service
$APP_SERVICE_PLAN = "asp-puntoventa"
$APP_SERVICE_NAME = "puntoventa-api"  # Debe ser único globalmente

# Crear App Service Plan (Linux)
az appservice plan create `
    --resource-group $RESOURCE_GROUP `
    --name $APP_SERVICE_PLAN `
    --location $LOCATION `
    --sku B1 `
    --is-linux

# Crear Web App
az webapp create `
    --resource-group $RESOURCE_GROUP `
    --plan $APP_SERVICE_PLAN `
    --name $APP_SERVICE_NAME `
    --runtime "DOTNETCORE:8.0"

# Configurar variables de entorno
az webapp config appsettings set `
    --resource-group $RESOURCE_GROUP `
    --name $APP_SERVICE_NAME `
    --settings `
    "ConnectionStrings__DefaultConnection=Host=$PG_SERVER_NAME.postgres.database.azure.com;Port=5432;Database=$PG_DATABASE;Username=$PG_ADMIN_USER;Password=$PG_ADMIN_PASSWORD;SSL Mode=Require;Trust Server Certificate=true" `
    "JwtSettings__SecretKey=TuClaveSuperSecretaDeMasDeReiintaDosCaracteresAquiMismoOk123456789" `
    "JwtSettings__ExpirationHours=24" `
    "ASPNETCORE_ENVIRONMENT=Production"
```

---

## 🔨 Paso 3: Desplegar el Backend

### Opción A: Despliegue con Azure CLI (Recomendado)

```powershell
# Navegar al directorio del backend
cd "C:\Users\Usuario\Desktop\Bryan_el_vitamina\VersionM\Backend\PuntoVenta.Api"

# Publicar la aplicación
dotnet publish -c Release -o ./publish

# Comprimir para despliegue
Compress-Archive -Path ./publish/* -DestinationPath ./deploy.zip -Force

# Desplegar a Azure
az webapp deploy `
    --resource-group $RESOURCE_GROUP `
    --name $APP_SERVICE_NAME `
    --src-path ./deploy.zip `
    --type zip

# Limpiar archivos temporales
Remove-Item -Path ./publish -Recurse -Force
Remove-Item -Path ./deploy.zip -Force
```

### Opción B: Despliegue con Visual Studio

1. Abre la solución `ProyectoWEB.sln` en Visual Studio
2. Click derecho en `PuntoVenta.Api` → **Publicar**
3. Selecciona **Azure** → **Azure App Service (Linux)**
4. Selecciona tu App Service creado
5. Click en **Publicar**

---

## 🌐 Paso 4: Desplegar el Frontend (Static Web Apps)

### 4.1 Preparar el Frontend

```powershell
# Navegar al frontend
cd "C:\Users\Usuario\Desktop\Bryan_el_vitamina\VersionM\Frontend"

# Crear archivo de configuración de producción
# Crea el archivo .env.production con:
```

Crea el archivo `.env.production` en `VersionM/Frontend/`:

```env
VITE_API_BASE_URL=https://puntoventa-api.azurewebsites.net/api
```

### 4.2 Actualizar la configuración de API

Modifica `src/config/api.js` para usar variables de entorno:

```javascript
// Usar variable de entorno o localhost como fallback
export const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5000/api'
```

### 4.3 Build del Frontend

```powershell
# Instalar dependencias
npm install

# Build para producción
npm run build
```

### 4.4 Crear Static Web App

```powershell
# Variables
$SWA_NAME = "puntoventa-frontend"

# Crear Static Web App
az staticwebapp create `
    --resource-group $RESOURCE_GROUP `
    --name $SWA_NAME `
    --location $LOCATION `
    --sku Free

# Obtener el token de despliegue
$DEPLOYMENT_TOKEN = az staticwebapp secrets list `
    --resource-group $RESOURCE_GROUP `
    --name $SWA_NAME `
    --query "properties.apiKey" -o tsv
```

### 4.5 Desplegar con SWA CLI

```powershell
# Instalar SWA CLI globalmente
npm install -g @azure/static-web-apps-cli

# Desplegar
swa deploy ./dist `
    --deployment-token $DEPLOYMENT_TOKEN `
    --env production
```

---

## 🔄 Paso 5: Configurar GitHub Actions (CI/CD Automático)

### 5.1 Para el Backend

Crea el archivo `.github/workflows/backend-deploy.yml`:

```yaml
name: Deploy Backend to Azure

on:
  push:
    branches:
      - main
    paths:
      - 'VersionM/Backend/**'

jobs:
  build-and-deploy:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '8.0.x'
    
    - name: Build
      run: dotnet build VersionM/Backend/PuntoVenta.Api -c Release
    
    - name: Publish
      run: dotnet publish VersionM/Backend/PuntoVenta.Api -c Release -o ./publish
    
    - name: Deploy to Azure
      uses: azure/webapps-deploy@v3
      with:
        app-name: 'puntoventa-api'
        publish-profile: ${{ secrets.AZURE_WEBAPP_PUBLISH_PROFILE }}
        package: ./publish
```

### 5.2 Para el Frontend

Crea el archivo `.github/workflows/frontend-deploy.yml`:

```yaml
name: Deploy Frontend to Azure Static Web Apps

on:
  push:
    branches:
      - main
    paths:
      - 'VersionM/Frontend/**'

jobs:
  build_and_deploy:
    runs-on: ubuntu-latest
    
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup Node.js
      uses: actions/setup-node@v4
      with:
        node-version: '20'
    
    - name: Install and Build
      run: |
        cd VersionM/Frontend
        npm install
        npm run build
    
    - name: Deploy to Azure Static Web Apps
      uses: Azure/static-web-apps-deploy@v1
      with:
        azure_static_web_apps_api_token: ${{ secrets.AZURE_STATIC_WEB_APPS_API_TOKEN }}
        repo_token: ${{ secrets.GITHUB_TOKEN }}
        action: "upload"
        app_location: "VersionM/Frontend"
        output_location: "dist"
```

---

## ✅ Paso 6: Verificar el Despliegue

### 6.1 Verificar Backend

```powershell
# Obtener URL del backend
$BACKEND_URL = az webapp show `
    --resource-group $RESOURCE_GROUP `
    --name $APP_SERVICE_NAME `
    --query "defaultHostName" -o tsv

Write-Host "Backend URL: https://$BACKEND_URL"

# Probar el endpoint de Swagger
Start-Process "https://$BACKEND_URL/swagger"
```

### 6.2 Verificar Frontend

```powershell
# Obtener URL del frontend
$FRONTEND_URL = az staticwebapp show `
    --resource-group $RESOURCE_GROUP `
    --name $SWA_NAME `
    --query "defaultHostname" -o tsv

Write-Host "Frontend URL: https://$FRONTEND_URL"
Start-Process "https://$FRONTEND_URL"
```

---

## 🔒 Paso 7: Configuraciones de Seguridad

### 7.1 Actualizar CORS en el Backend

Una vez desplegado, actualiza el CORS para incluir el dominio del frontend:

```powershell
az webapp cors add `
    --resource-group $RESOURCE_GROUP `
    --name $APP_SERVICE_NAME `
    --allowed-origins "https://TU_FRONTEND.azurestaticapps.net"
```

### 7.2 Habilitar HTTPS

Azure App Service y Static Web Apps tienen HTTPS habilitado por defecto.

---

## 💰 Estimación de Costos Mensuales

| Servicio | Tier | Costo Aprox. |
|----------|------|--------------|
| Static Web Apps | Free | $0 |
| App Service | B1 | ~$13 |
| PostgreSQL Flexible | Burstable B1ms | ~$15 |
| **Total** | | **~$28/mes** |

### Opción más económica:
- Usar **Azure Cosmos DB for PostgreSQL** tier serverless
- Usar **App Service** tier F1 (gratis, con limitaciones)

---

## 🆘 Solución de Problemas

### Error de conexión a la base de datos
```powershell
# Verificar reglas de firewall
az postgres flexible-server firewall-rule create `
    --resource-group $RESOURCE_GROUP `
    --name $PG_SERVER_NAME `
    --rule-name AllowAzureServices `
    --start-ip-address 0.0.0.0 `
    --end-ip-address 0.0.0.0
```

### Ver logs del backend
```powershell
az webapp log tail `
    --resource-group $RESOURCE_GROUP `
    --name $APP_SERVICE_NAME
```

### Reiniciar el App Service
```powershell
az webapp restart `
    --resource-group $RESOURCE_GROUP `
    --name $APP_SERVICE_NAME
```

---

## 📚 Recursos Adicionales

- [Documentación Azure App Service](https://docs.microsoft.com/azure/app-service/)
- [Azure Static Web Apps](https://docs.microsoft.com/azure/static-web-apps/)
- [Azure Database for PostgreSQL](https://docs.microsoft.com/azure/postgresql/)
- [Tutorial REST API con CORS](https://learn.microsoft.com/azure/app-service/app-service-web-tutorial-rest-api)
