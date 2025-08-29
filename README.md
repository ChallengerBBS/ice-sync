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
┌─────────────────┐    HTTP/REST    ┌─────────────────┐    HTTP/JWT    ┌─────────────────┐
│                 │ ◄──────────────► │                 │ ◄─────────────► │                 │
│  React Frontend │                 │  .NET 9 Web API │                │ Universal Loader│
│   (TypeScript)  │                 │                 │                │      API        │
│                 │                 │                 │                │                 │
└─────────────────┘                 └─────────────────┘                └─────────────────┘
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

## 🛠️ Quick Start

### Option 1: Use the Startup Scripts (Recommended)

We've provided convenient startup scripts to run both projects simultaneously:

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

### Option 2: Manual Setup

If you prefer to set up manually or need more control:

## 📦 Detailed Setup Instructions

### 1. Clone the Repository

```bash
git clone <repository-url>
cd "The Ice Cream Company"
```

### 2. Database Setup

The application uses SQL Server LocalDB by default. The database will be created automatically when you first run the application.

**Custom SQL Server Instance (Optional):**

If you want to use a different SQL Server instance, update the connection string in `IceSync.Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=your-server;Database=IceSyncDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  }
}
```

### 3. Backend Setup (.NET API)

1. **Navigate to the API project:**
   ```bash
   cd IceSync.Api
   ```

2. **Restore NuGet packages:**
   ```bash
   dotnet restore
   ```

3. **Build the project:**
   ```bash
   dotnet build
   ```

4. **Run the API:**
   ```bash
   dotnet run
   ```

The API will be available at:
- **HTTPS**: `https://localhost:7041`
- **HTTP**: `http://localhost:5041`

### 4. Frontend Setup (React App)

1. **Open a new terminal and navigate to the web project:**
   ```bash
   cd IceSync.Web
   ```

2. **Install dependencies:**
   ```bash
   npm install
   ```

3. **Start the development server:**
   ```bash
   npm start
   ```

The web application will be available at `http://localhost:3000` and will automatically open in your browser.

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

### Environment Variables (Optional)

You can also configure the frontend API URL using environment variables:

Create a `.env` file in `IceSync.Web/`:
```
REACT_APP_API_URL=https://localhost:7041/api
```

## 🔌 API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/workflows` | Get all workflows from database |
| `POST` | `/api/workflows/{workflowId}/run` | Run a specific workflow |
| `POST` | `/api/workflows/sync` | Manually trigger synchronization |

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

## 🗄️ Database Schema

The application uses Entity Framework Core with a single table `Workflows`:

```sql
CREATE TABLE Workflows (
    WorkflowId NVARCHAR(100) PRIMARY KEY,
    WorkflowName NVARCHAR(255) NOT NULL,
    IsActive BIT NOT NULL,
    MultiExecBehavior NVARCHAR(100) NULL
)
```

## 🔒 Security

- **JWT Authentication**: Tokens are cached for 55 minutes to optimize API calls
- **CORS Configuration**: Properly configured to allow frontend-backend communication
- **Input Validation**: Comprehensive validation on all API endpoints
- **Error Handling**: Graceful error handling without exposing sensitive information

## 🚧 Development

### Building the Entire Solution

```bash
# Build backend
dotnet build IceSync.sln

# Build frontend
cd IceSync.Web
npm run build
```

### Database Migrations (If needed)

```bash
cd IceSync.Api
dotnet ef migrations add MigrationName
dotnet ef database update
```

### Code Quality

The project follows these standards:
- **C#**: Clean Architecture principles, SOLID principles
- **TypeScript/React**: Functional components, hooks, proper typing
- **Testing**: High test coverage with meaningful test cases

## 🐛 Troubleshooting

### Common Issues

| Issue | Solution |
|-------|----------|
| **Database Connection Error** | Ensure SQL Server LocalDB is installed and running |
| **API Connection Failed** | Verify Universal Loader API credentials and network connectivity |
| **CORS Issues** | Check that the API is running on port 7041 and frontend on 3000 |
| **Port Already in Use** | Kill processes using the ports or change ports in configuration |
| **npm install fails** | Try `npm cache clean --force` and reinstall |

### Logs and Debugging

- **Backend Logs**: Check console output when running `dotnet run`
- **Frontend Logs**: Open browser developer tools (F12) → Console tab
- **Network Issues**: Check Network tab in browser developer tools

### Port Configuration

Default ports:
- **API**: `https://localhost:7041` (HTTPS), `http://localhost:5041` (HTTP)
- **Frontend**: `http://localhost:3000`

To change ports:
- **API**: Modify `IceSync.Api/Properties/launchSettings.json`
- **Frontend**: Set `PORT=3001` in environment variables

## 🚀 Production Deployment

For production deployment, consider:

1. **Environment Configuration**:
   - Use production SQL Server instance
   - Implement proper secrets management (Azure Key Vault, etc.)
   - Configure production API URLs

2. **Security**:
   - Set up HTTPS certificates
   - Implement proper authentication/authorization
   - Configure security headers

3. **Performance**:
   - Enable response compression
   - Configure caching strategies
   - Optimize database queries

4. **Monitoring**:
   - Set up application logging (Serilog, NLog)
   - Configure health checks
   - Implement monitoring and alerting

5. **Frontend Build**:
   ```bash
   cd IceSync.Web
   npm run build
   ```

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

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This project is created for The Ice Cream Company as a custom workflow management solution.

---

**Happy Coding! 🍦🚀**

For questions or support, please contact the development team.