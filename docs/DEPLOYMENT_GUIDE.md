# Gourmet Express — Production Deployment Guide

This guide provides end-to-end instructions for deploying both the ASP.NET Core Web API backend and the React TypeScript frontend to production hosting environments.

---

## 1. Architecture Overview for Deployment

```
                    +-----------------------------+
                    |  DNS / Custom Domain        |
                    +--------------+--------------+
                                   |
            +----------------------+----------------------+
            | HTTPS                                       | HTTPS / WSS
            v                                             v
+-----------------------+                     +-----------------------+
| Frontend: Vercel /    |                     | Backend: Render /     |
| Netlify Static Host   |  REST APIs / WS     | Azure App Service /   |
| (React + Vite SPA)    | ------------------> | Container App (.NET 8)|
+-----------------------+                     +-----------+-----------+
                                                          |
                                                          v
                                              +-----------------------+
                                              | Relational Database:  |
                                              | Managed PostgreSQL or |
                                              | Azure SQL / SQLite    |
                                              +-----------------------+
```

---

## 2. Option A: Containerized Deployment via Docker & Docker Compose

### Prerequisites
- Docker Engine 24+ and Docker Compose v2+

### Running Locally with Docker Compose
```bash
docker compose up --build -d
```
The backend API becomes accessible at `http://localhost:5000` (Swagger at `http://localhost:5000/swagger`), and the frontend SPA becomes accessible at `http://localhost:3000`.

---

## 3. Option B: Cloud Hosting (Vercel + Render / Azure)

### Part 1: Deploying the .NET 8 Backend (Render or Azure App Service)

#### Deploying on Render (Web Service):
1. Create a new **Web Service** on [Render](https://render.com).
2. Connect your Git repository.
3. Configure settings:
   - **Environment:** Docker
   - **Root Directory:** `.`
   - **Dockerfile Path:** `./Dockerfile.backend`
   - **Plan:** Free or Starter
4. Environment Variables:
   - `ASPNETCORE_ENVIRONMENT`: `Production`
   - `DatabaseProvider`: `Sqlite` (or `SqlServer` / `Postgres`)
   - `Cors__AllowedOrigins__0`: `https://your-frontend.vercel.app`
5. Note your backend public URL (e.g., `https://gourmet-express-api.onrender.com`).

#### Deploying on Azure App Service:
1. Publish the .NET application:
   ```bash
   dotnet publish src/FoodDelivery.Api/FoodDelivery.Api.csproj -c Release -o ./publish/api
   ```
2. Deploy via Azure CLI or Azure Developer CLI:
   ```bash
   az webapp up --name gourmet-express-api --resource-group rg-fooddelivery --runtime "DOTNETCORE:8.0"
   ```
3. Set Application Settings in Azure Portal:
   - `Cors:AllowedOrigins:0` = `https://your-frontend.vercel.app`
   - `OrderSettings:DeliveryFee` = `3.99`

---

### Part 2: Deploying the React Frontend (Vercel or Netlify)

#### Deploying on Vercel:
1. Install Vercel CLI or import the repository in the Vercel Dashboard.
2. Select the `frontend` subfolder as the Project Root Directory.
3. Configure Build Settings:
   - **Framework Preset:** Vite
   - **Build Command:** `npm run build`
   - **Output Directory:** `dist`
   - **Install Command:** `npm install`
4. Set Environment Variables:
   - `VITE_API_BASE_URL`: `https://gourmet-express-api.onrender.com`
   - `VITE_HUB_URL`: `https://gourmet-express-api.onrender.com/hubs/orders`
5. Deploy:
   ```bash
   cd frontend
   vercel --prod
   ```

#### Deploying on Netlify:
1. In Netlify, link your repository.
2. Base directory: `frontend`
3. Build command: `npm run build`
4. Publish directory: `frontend/dist`
5. Add `frontend/public/_redirects` for client-side routing:
   ```
   /*    /index.html   200
   ```
6. Add environment variables `VITE_API_BASE_URL` and `VITE_HUB_URL`.

---

## 4. Production Security Checklist

- [x] All CORS headers restrict access strictly to configured frontend domains.
- [x] WebSockets / SignalR transport allows credentials with explicit origins.
- [x] Production database credentials injected via cloud secrets / environment variables.
- [x] Global exception middleware returns RFC 7807 ProblemDetails without stack trace leakage.
- [x] Client input thoroughly sanitized and validated on the backend.
