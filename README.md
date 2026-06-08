# Global Logistics Management System (GLMS)

## Enterprise-Grade Service-Oriented Architecture Solution

### Overview
GLMS is a cloud-native, containerized enterprise application built for TechMove Logistics. It demonstrates modern software architecture principles including:

- **Service-Oriented Architecture (SOA)**: Decoupled API and Web layers
- **Containerization**: Full Docker support with Docker Compose orchestration
- **JWT Authentication**: Secure token-based authentication
- **Automated Testing**: Comprehensive integration test suite
- **API Documentation**: Self-documenting Swagger/OpenAPI endpoints

### Architecture

```
┌─────────────────┐     ┌──────────────────┐     ┌─────────────────┐
│   GLMS.Web      │────▶│   GLMS.API       │────▶│   SQL Server    │
│  (MVC Frontend) │     │  (Web API)       │     │   (Database)    │
│   Port 8081     │     │   Port 8080      │     │   Port 1433     │
└─────────────────┘     └──────────────────┘     └─────────────────┘
                               │
                               ▼
                        ┌──────────────────┐
                        │   Redis Cache    │
                        │   Port 6379      │
                        └──────────────────┘
```

### Project Structure

```
GLMS/
├── src/
│   ├── GLMS.API/              # Backend Web API
│   │   ├── Controllers/         # API Controllers
│   │   ├── Models/              # Domain Models & DTOs
│   │   ├── Services/            # Business Logic Layer
│   │   ├── Data/                # DbContext & Migrations
│   │   ├── Middleware/          # Custom Middleware
│   │   ├── Dockerfile           # API Container Definition
│   │   └── Program.cs           # API Entry Point
│   └── GLMS.Web/              # Frontend MVC Application
│       ├── Controllers/         # MVC Controllers
│       ├── Models/              # ViewModels
│       ├── Views/               # Razor Views
│       ├── Services/            # API Client Service
│       ├── Dockerfile           # Web Container Definition
│       └── Program.cs           # Web Entry Point
├── tests/
│   └── GLMS.IntegrationTests/   # Automated Integration Tests
│       ├── Controllers/         # API Endpoint Tests
│       └── TestProgram.cs       # Test Fixture
├── docker/
│   ├── docker-compose.yml       # Full Stack Orchestration
│   ├── nginx.conf               # Reverse Proxy Config
│   └── init-db.sql              # Database Initialization
└── GLMS.sln                     # Solution File
```

### Quick Start

#### Prerequisites
- Docker Desktop
- .NET 8.0 SDK (for local development)
- Visual Studio 2022 or VS Code

#### Running with Docker Compose

```bash
# Clone the repository
git clone <repository-url>
cd GLMS

# Start all services
cd docker
docker-compose up -d

# Wait for services to initialize (30-60 seconds)
# Then access:
# - Web Application: http://localhost:8081
# - API Swagger Docs: http://localhost:8080/swagger
# - API Health Check: http://localhost:8080/health

# View logs
docker-compose logs -f

# Stop all services
docker-compose down
```

#### Running Locally (Development)

```bash
# Restore packages
dotnet restore GLMS.sln

# Run API
cd src/GLMS.API
dotnet run
# API runs at: https://localhost:7080 / http://localhost:5080

# Run Web (in new terminal)
cd src/GLMS.Web
dotnet run
# Web runs at: https://localhost:7081 / http://localhost:5081
```

### API Endpoints

| Method | Endpoint | Description | Auth Required |
|--------|----------|-------------|---------------|
| POST | `/api/auth/login` | Authenticate user | No |
| POST | `/api/auth/register` | Register new user | No |
| GET | `/api/auth/me` | Get current user | Yes |
| GET | `/api/contracts` | List all contracts (with filters) | No |
| GET | `/api/contracts/{id}` | Get contract by ID | No |
| POST | `/api/contracts` | Create new contract | Yes (Admin/Manager) |
| PUT | `/api/contracts/{id}` | Update contract | Yes (Admin/Manager) |
| PATCH | `/api/contracts/{id}/status` | Update contract status | Yes (Admin/Manager) |
| POST | `/api/contracts/{id}/activate` | Activate contract | Yes (Admin/Manager) |
| POST | `/api/contracts/{id}/expire` | Expire contract | Yes (Admin/Manager) |
| DELETE | `/api/contracts/{id}` | Delete contract | Yes (Admin) |
| GET | `/api/contracts/expiring` | Get expiring contracts | Yes (Admin/Manager) |
| GET | `/api/servicerequests` | List service requests | No |
| POST | `/api/servicerequests` | Create service request | Yes |
| PATCH | `/api/servicerequests/{id}/status` | Update request status | Yes (Admin/Manager) |
| GET | `/api/currency/rates` | Get exchange rates | No |
| POST | `/api/currency/convert` | Convert currency | No |
| GET | `/health` | Health check | No |

### Demo Credentials

| Username | Password | Role |
|----------|----------|------|
| admin | Admin123! | Admin |
| manager | Manager123! | Manager |
| user | User123! | User |

### Running Tests

```bash
# Run all integration tests
dotnet test tests/GLMS.IntegrationTests/GLMS.IntegrationTests.csproj

# Run with verbose output
dotnet test --logger "console;verbosity=detailed"

# Run specific test class
dotnet test --filter "FullyQualifiedName~ContractsControllerTests"
```

### Technology Stack

- **Backend**: ASP.NET Core 8.0 Web API
- **Frontend**: ASP.NET Core 8.0 MVC with Razor Views
- **Database**: SQL Server 2022
- **Cache**: Redis 7
- **Authentication**: JWT Bearer Tokens + Cookie Authentication
- **Documentation**: Swagger / OpenAPI 3.0
- **Testing**: xUnit, FluentAssertions, Moq
- **Containerization**: Docker, Docker Compose
- **Reverse Proxy**: NGINX

### Key Features

1. **Contract Management Hub**: Centralized repository with automated status tracking
2. **Service Request Processing**: Linked to active contracts with validation
3. **Financial Integration**: Currency conversion with external API fallback
4. **Role-Based Access Control**: Admin, Manager, and User roles
5. **Responsive UI**: Bootstrap 5 with modern design
6. **Health Monitoring**: Built-in health checks for all services
7. **Caching**: Redis-based distributed caching for performance


### Author

Mihla Jack Nxumalo
