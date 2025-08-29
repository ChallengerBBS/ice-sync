# 🍦 IceSync - The Ice Cream Company Workflow Management System

IceSync is a comprehensive workflow management application that synchronizes data with the Universal Loader API and provides a modern web interface for managing workflows.

## 🚀 Features

- **Workflow Management**: View all workflows from the Universal Loader API
- **Real-time Synchronization**: Automatic sync every 30 minutes with manual sync option
- **Workflow Execution**: Run workflows directly from the web interface with toastr notifications
- **Modern UI**: Beautiful React-based user interface with TypeScript
- **Database Storage**: SQL Server database for local workflow storage
- **JWT Authentication**: Secure communication with Universal Loader API
- **Comprehensive Testing**: Full test coverage for both frontend and backend

## 🏗️ Architecture

The application consists of two main components:

1. **IceSync.Api** - .NET 9 Web API backend
2. **IceSync.Web** - React TypeScript frontend

```
┌─────────────────┐    HTTP/REST     ┌─────────────────┐    HTTP/JWT     ┌─────────────────┐
│                 │ ◄──────────────► │                 │ ◄─────────────► │                 │
│  React Frontend │                  │  .NET 9 Web API │                 │ Universal Loader│
│   (TypeScript)  │                  │                 │                 │      API        │
│                 │                  │                 │                 │                 │
└─────────────────┘                  └─────────────────┘                 └─────────────────┘
                                             │
                                             │ Entity Framework
                                             ▼
                                    ┌─────────────────┐
                                    │                 │
                                    │  SQL Server     │
                                    │   Database      │
                                    │                 │
                                    └─────────────────┘
```

## 📋 Prerequisites

Before you begin, ensure you have the following installed:

- **.NET 9 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/9.0)
- **SQL Server LocalDB** (comes with Visual Studio) or SQL Server instance
- **Node.js 18+** and **npm** - [Download here](https://nodejs.org/)
- **Visual Studio 2022** or **VS Code** (recommended)
- **Git** (for cloning the repository)

## ⚙️ Configuration

### Universal Loader API Configuration

The API credentials are configured in `IceSync.Api/appsettings.json`. For security reasons, the actual credentials are not included in the repository. You'll need to update these values:

```json
{
  "UniversalLoader": {
    "BaseUrl": "https://api-test.universal-loader.com",
    "CompanyId": "your-company-id",
    "UserId": "your-user-id",
    "UserSecret": "your-secret-key"
  }
}
```

## 🛠️ Quick Start

Add your credentials for accessing the Universal Loader API in appsettings.json

There's provided convenient startup scripts to run both projects simultaneously:

**For Windows PowerShell:**
```powershell
.\start-icesync.ps1
```

**For Windows Command Prompt:**
```cmd
start-icesync.bat
```

These scripts will:
1. Start the .NET API on `https://localhost:7041`
2. Start the React frontend on `http://localhost:3000`
3. Open your browser automatically

## 🔌 API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET`  | `/api/workflows`                  | Get all workflows from database  |
| `POST` | `/api/workflows/{workflowId}/run` | Run a specific workflow          |
| `POST` | `/api/workflows/sync`             | Manually trigger synchronization |

## 🧪 Testing

### Backend Tests (.NET)

The backend uses **NUnit**, **FluentAssertions**, and **Moq** for comprehensive testing:

```bash
cd IceSync.Api.Tests
dotnet test
```

**Run tests with coverage:**
```bash
dotnet test --collect:"XPlat Code Coverage"
```

**Generate coverage report:**
```bash
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:"TestResults\*\coverage.cobertura.xml" -targetdir:"TestResults\CoverageReport" -reporttypes:Html
```

### Frontend Tests (React)

The frontend uses **Jest** and **React Testing Library** for testing:

```bash
cd IceSync.Web
npm test
```

**Run tests with coverage:**
```bash
npm test -- --coverage
```

**Run tests in watch mode:**
```bash
npm test -- --watch
```

### Test Coverage

Both projects have comprehensive test suites with high coverage:

- **Backend**: Unit tests for controllers, services, and data layer
- **Frontend**: Component tests, API service tests, and user interaction tests

## 🔄 Features in Detail

### Automatic Synchronization

The application includes a background service that automatically synchronizes workflows every 30 minutes. The synchronization process:

1. Fetches all workflows from the Universal Loader API
2. Compares with local database
3. Inserts new workflows
4. Updates existing workflows
5. Deletes workflows that no longer exist in the API

### Manual Synchronization

Users can manually trigger synchronization using the "🔄 Sync Workflows" button in the web interface.

### Workflow Execution

Users can run workflows directly from the web interface. The system will:

1. Call the Universal Loader API to execute the workflow
2. Display success/failure messages using **toastr notifications**
3. Handle errors gracefully with user-friendly messages

## 🔒 Security

- **JWT Authentication**: Tokens are cached for 55 minutes to optimize API calls
- **CORS Configuration**: Properly configured to allow frontend-backend communication
- **Input Validation**: Comprehensive validation on all API endpoints
- **Error Handling**: Graceful error handling without exposing sensitive information

### Code Quality

The project follows these standards:
- **C#**: Clean Architecture principles, SOLID principles
- **TypeScript/React**: Functional components, hooks, proper typing
- **Testing**: High test coverage with meaningful test cases


### Port Configuration

Default ports:
- **API**: `https://localhost:7041` (HTTPS), `http://localhost:5041` (HTTP)
- **Frontend**: `http://localhost:3000`

To change ports:
- **API**: Modify `IceSync.Api/Properties/launchSettings.json`
- **Frontend**: Set `PORT=3001` in environment variables

## 📁 Project Structure

```
The Ice Cream Company/
├── IceSync.Api/                 # .NET 9 Web API
│   ├── Controllers/             # API Controllers
│   ├── Services/               # Business Logic Services
│   ├── Data/                   # Entity Framework DbContext
│   ├── Domain/                 # Entities and DTOs
│   └── Infrastructure/         # Configuration and External Services
├── IceSync.Api.Tests/          # Backend Unit Tests
│   ├── Controllers/            # Controller Tests
│   └── Services/              # Service Tests
├── IceSync.Web/               # React TypeScript Frontend
│   ├── src/
│   │   ├── components/        # React Components
│   │   ├── services/          # API Service Layer
│   │   ├── types/             # TypeScript Type Definitions
│   │   └── __tests__/         # Frontend Tests
│   └── public/                # Static Assets
├── start-icesync.bat          # Windows Batch Startup Script
├── start-icesync.ps1          # PowerShell Startup Script
└── README.md                  # This File
```

## 📄 License

This project is created for assessment purposes.

---
