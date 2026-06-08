# GLMS Setup & Deployment Guide

## Quick Start (Docker Compose - Recommended)

### Step 1: Prerequisites
- Install [Docker Desktop](https://www.docker.com/products/docker-desktop/)
- Ensure Docker Compose is enabled
- Allocate at least 4GB RAM to Docker

### Step 2: Build and Run
```bash
# Navigate to docker directory
cd docker

# Build and start all services
docker-compose up --build -d

# Wait for SQL Server to initialize (60-90 seconds)
# Check service status:
docker-compose ps

# View logs:
docker-compose logs -f glms-backend-api
```

### Step 3: Access the System
| Service | URL | Description |
|---------|-----|-------------|
| Web Application | http://localhost:8081 | MVC Frontend |
| API Documentation | http://localhost:8080/swagger | Swagger UI |
| API Health | http://localhost:8080/health | Health Check |
| SQL Server | localhost:1433 | Database (sa/YourStrong@Passw0rd) |
| Redis | localhost:6379 | Cache |

### Step 4: Login with Demo Credentials
- **Admin**: admin / Admin123!
- **Manager**: manager / Manager123!
- **User**: user / User123!

### Step 5: Stop Services
```bash
docker-compose down          # Stop and remove containers
docker-compose down -v       # Stop and remove volumes (data)
```

---

## Development Setup (Local)

### Step 1: Install Prerequisites
- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server Express](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) or LocalDB
- [Redis](https://redis.io/download) (optional, fallback to in-memory)

### Step 2: Configure Database
```bash
# Create database in SQL Server
sqlcmd -S localhost -U sa -P YourStrong@Passw0rd -Q "CREATE DATABASE GLMS"
```

### Step 3: Run API
```bash
cd src/GLMS.API
dotnet restore
dotnet run
# API: https://localhost:7080 / http://localhost:5080
# Swagger: https://localhost:7080/swagger
```

### Step 4: Run Web App (New Terminal)
```bash
cd src/GLMS.Web
dotnet restore
dotnet run
# Web: https://localhost:7081 / http://localhost:5081
```

### Step 5: Run Tests
```bash
dotnet test tests/GLMS.IntegrationTests/GLMS.IntegrationTests.csproj
```

---

## GitHub Repository Setup

### Step 1: Initialize Repository
```bash
cd GLMS
git init
git add .
git commit -m "Initial commit: GLMS complete solution"
```

### Step 2: Create GitHub Repository
1. Go to [GitHub](https://github.com) and create new repository
2. Name it `GLMS-TechMove-Logistics`
3. Do NOT initialize with README (we have one)

### Step 3: Push to GitHub
```bash
git remote add origin https://github.com/YOUR_USERNAME/GLMS-TechMove-Logistics.git
git branch -M main
git push -u origin main
```

### Step 4: Verify Structure on GitHub
Your repository should contain:
```
GLMS/
├── src/
│   ├── GLMS.API/          # Backend API
│   └── GLMS.Web/          # Frontend MVC
├── tests/
│   └── GLMS.IntegrationTests/  # Tests
├── docker/
│   ├── docker-compose.yml      # Orchestration
│   ├── Dockerfile.api          # API container
│   ├── Dockerfile.web          # Web container
│   └── nginx.conf              # Proxy config
├── docs/
│   └── Technical_Reflection_Report.md
├── GLMS.sln
└── README.md
```

---

## Docker Desktop Screenshots Checklist

For your submission, capture these screenshots:

1. **Docker Desktop Dashboard**: Shows all 5 containers running
   - `glms-sql-server` (green)
   - `glms-redis` (green)
   - `glms-backend-api` (green)
   - `glms-frontend-web` (green)
   - `glms-nginx` (green)

2. **Web Application**: http://localhost:8081
   - Login page
   - Dashboard with statistics
   - Contracts list with filters
   - Service requests page

3. **API Swagger**: http://localhost:8080/swagger
   - Swagger UI with all endpoints
   - Try out a GET request
   - Authorization with Bearer token

4. **Database**: SQL Server Management Studio or Azure Data Studio
   - Connected to localhost:1433
   - GLMS database with tables
   - Sample data in Contracts table

5. **Terminal**: `docker-compose ps` output
   - All services showing "Up (healthy)"

---

## Troubleshooting

### SQL Server Fails to Start
```bash
# Check logs
docker-compose logs sql-server-db

# Common fix: Increase Docker memory to 4GB+
# Docker Desktop → Settings → Resources → Memory
```

### API Cannot Connect to Database
```bash
# Ensure SQL Server is healthy
docker-compose ps

# Restart API after DB is ready
docker-compose restart glms-backend-api
```

### Web App Cannot Connect to API
```bash
# Check API health
curl http://localhost:8080/health

# Verify Web app settings
cat src/GLMS.Web/appsettings.Docker.json
# Should point to: http://glms-backend-api:8080/
```

### Port Already in Use
```bash
# Find and kill process using port 8080
lsof -i :8080
kill -9 <PID>

# Or use different ports in docker-compose.yml
```

---

## Submission Checklist

### Source Code (GitHub)
- [ ] Repository created and pushed
- [ ] All source code included
- [ ] Dockerfile for API
- [ ] Dockerfile for Web App
- [ ] docker-compose.yml included
- [ ] README.md with instructions

### Screenshots
- [ ] Docker Desktop with all containers running
- [ ] Web application pages (Login, Dashboard, Contracts, Service Requests)
- [ ] API Swagger documentation
- [ ] Database connection showing tables
- [ ] Terminal showing `docker-compose ps`

### Documentation
- [ ] Technical Reflection Report (PDF)
  - [ ] DevOps & Testing explanation
  - [ ] Containerization benefits
  - [ ] CI/CD pipeline discussion
  - [ ] "It works on my machine" problem analysis

### Testing
- [ ] Integration tests run successfully
- [ ] All 29 tests passing
- [ ] Screenshot of test results

---

## Architecture Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                        NGINX (Port 80)                        │
│                    Reverse Proxy / Load Balancer             │
└─────────────────────────────────────────────────────────────┘
                              │
            ┌─────────────────┴─────────────────┐
            ▼                                   ▼
┌──────────────────────┐            ┌──────────────────────┐
│   GLMS.Web           │            │   GLMS.API           │
│   (Port 8081)        │            │   (Port 8080)        │
│   MVC Frontend       │◄─────────│   Web API Backend    │
│   - Razor Views      │  HttpClient│   - Controllers      │
│   - Bootstrap 5      │            │   - Services         │
│   - Cookie Auth      │            │   - JWT Auth         │
└──────────────────────┘            └──────────────────────┘
                                              │
                              ┌───────────────┴───────────────┐
                              ▼                               ▼
                    ┌─────────────────┐          ┌─────────────────┐
                    │   SQL Server     │          │   Redis Cache   │
                    │   (Port 1433)    │          │   (Port 6379)   │
                    │   - Contracts    │          │   - Session     │
                    │   - ServiceReqs  │          │   - API Rates   │
                    │   - Users        │          │   - Contracts   │
                    └─────────────────┘          └─────────────────┘
```

---

**Good luck with your submission!** 🚀
