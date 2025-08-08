# AI Chat Application

A full-stack AI chat application built with .NET 9 Web API backend and Angular 19 frontend. The application supports multiple AI service providers including Ollama, OpenAI, Azure AI Foundry, and Anthropic, with document management and vector search capabilities.

## 🚀 Features

- **Multi-AI Provider Support**: Integrate with Ollama, OpenAI, Azure AI Foundry, and Anthropic
- **Real-time Chat**: Server-sent events for streaming responses
- **Document Management**: Upload and search documents with vector embeddings
- **Session Management**: Persistent chat sessions with history
- **Modern UI**: Responsive Angular frontend with Bootstrap 5
- **Vector Search**: AI-powered document search using SQL Server Vector Search

## 🏗️ Architecture

```
ai-chat/
├── RR.AI-Chat/                 # .NET 9 Web API Backend
│   ├── RR.AI-Chat.Api/         # API Controllers & Program.cs
│   ├── RR.AI-Chat.Service/     # Business Logic Services
│   ├── RR.AI-Chat.Repository/  # Data Access Layer
│   ├── RR.AI-Chat.Entity/      # Entity Framework Models
│   └── RR.AI-Chat.Dto/         # Data Transfer Objects
└── ai-chat-ui/                 # Angular 19 Frontend
    ├── src/app/services/       # HTTP Services
    ├── src/app/dtos/           # TypeScript DTOs
    └── src/environments/       # Environment Configuration
```

## 🛠️ Tech Stack

### Backend (.NET API)
- **.NET 9.0** - Web API Framework
- **Entity Framework Core 9.0** - ORM with SQL Server
- **SQL Server Vector Search** - Vector embeddings storage
- **Microsoft.Extensions.AI** - AI service abstractions
- **Swagger/OpenAPI** - API Documentation

### Frontend (Angular UI)
- **Angular 19** - Frontend Framework
- **TypeScript 5.6** - Programming Language
- **Bootstrap 5.3** - CSS Framework
- **RxJS** - Reactive Programming
- **Highlight.js** - Code Syntax Highlighting
- **Markdown-it** - Markdown Rendering

### AI Service Integrations
- **Ollama** - Local AI models
- **OpenAI** - GPT models
- **Azure AI Foundry** - Azure OpenAI Service
- **Anthropic** - Claude models

## 📋 Prerequisites

### Required Software
- **.NET 9.0 SDK** - [Download here](https://dotnet.microsoft.com/download/dotnet/9.0)
- **Node.js 18+** - [Download here](https://nodejs.org/)
- **SQL Server** - Express, Developer, or Full edition
- **Angular CLI** - Install via `npm install -g @angular/cli`

### Optional (for local AI)
- **Ollama** - [Install here](https://ollama.ai/) for local AI models

### AI Service API Keys (at least one required)
- **OpenAI API Key** - For GPT models
- **Azure AI Foundry** - Endpoint URL and API Key
- **Anthropic API Key** - For Claude models

## 🚀 Quick Start

### 1. Clone the Repository
```bash
git clone https://github.com/RorroRojas3/ai-chat.git
cd ai-chat
```

### 2. Setup the Database
```bash
# Create database (replace connection string as needed)
# Default: Server=localhost;Database=aichat;Integrated Security=true;Encrypt=true;TrustServerCertificate=true;
```

### 3. Configure API Environment Variables

Create user secrets for the API project:
```bash
cd RR.AI-Chat/RR.AI-Chat.Api
dotnet user-secrets init
```

Add your AI service configurations:
```bash
# For OpenAI
dotnet user-secrets set "OpenAI:ApiKey" "your-openai-api-key"

# For Azure AI Foundry
dotnet user-secrets set "AzureAIFoundry:Url" "https://your-endpoint.openai.azure.com/"
dotnet user-secrets set "AzureAIFoundry:ApiKey" "your-azure-api-key"
dotnet user-secrets set "AzureAIFoundry:EmbeddingModel" "text-embedding-ada-002"

# For Anthropic
dotnet user-secrets set "Anthropic:ApiKey" "your-anthropic-api-key"

# For custom Ollama URL (optional, defaults to http://localhost:11434/)
dotnet user-secrets set "OllamaUrl" "http://localhost:11434/"
```

### 4. Run the API
```bash
cd RR.AI-Chat
dotnet restore
dotnet build
dotnet run --project RR.AI-Chat.Api
```

The API will start at `https://localhost:7045` (HTTPS) and `http://localhost:5045` (HTTP).

### 5. Run the Frontend
```bash
cd ai-chat-ui
npm install
npm start
```

The frontend will start at `http://localhost:4200`.

### 6. Access the Application
- **Frontend**: http://localhost:4200
- **API Documentation**: https://localhost:7045/swagger

## 🔧 Configuration

### API Configuration (`appsettings.json`)

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "CorsOrigins": [ "http://localhost:4200" ],
  "OllamaUrl": "http://localhost:11434/",
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=aichat;Integrated Security=true;Encrypt=true;TrustServerCertificate=true;"
  }
}
```

### Frontend Configuration (`src/environments/environment.ts`)

```typescript
export const environment = {
  production: false,
  apiUrl: 'https://localhost:7045/api/',
};
```

### Environment Variables Reference

| Variable | Description | Required | Default |
|----------|-------------|----------|---------|
| `OpenAI:ApiKey` | OpenAI API key for GPT models | No* | - |
| `AzureAIFoundry:Url` | Azure OpenAI endpoint URL | No* | - |
| `AzureAIFoundry:ApiKey` | Azure OpenAI API key | No* | - |
| `AzureAIFoundry:EmbeddingModel` | Embedding model name | No | text-embedding-ada-002 |
| `Anthropic:ApiKey` | Anthropic API key for Claude models | No* | - |
| `OllamaUrl` | Ollama server URL | No | http://localhost:11434/ |
| `ConnectionStrings:DefaultConnection` | SQL Server connection string | Yes | See above |

*At least one AI service must be configured.

## 🧪 Database Setup

The application uses Entity Framework migrations. To set up the database:

1. **Update Connection String**: Modify the connection string in `appsettings.json` or set via user secrets:
   ```bash
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" "your-connection-string"
   ```

2. **Run Migrations** (when available):
   ```bash
   cd RR.AI-Chat
   dotnet ef database update --project RR.AI-Chat.Api
   ```

## 📚 API Endpoints

### Chat Endpoints
- `POST /api/chats/sessions/{sessionId}/stream` - Stream chat responses
- `POST /api/chats/sessions/{sessionId}/completion` - Get chat completion

### Session Management
- `GET /api/sessions` - Get all sessions
- `POST /api/sessions` - Create new session
- `GET /api/sessions/{id}` - Get session by ID
- `DELETE /api/sessions/{id}` - Delete session

### Document Management
- `POST /api/documents` - Upload document
- `GET /api/documents` - Get all documents
- `POST /api/documents/search` - Search documents

### Models
- `GET /api/models` - Get available AI models

## 🔧 Development

### Running Tests

**Backend Tests**:
```bash
cd RR.AI-Chat
dotnet test
```

**Frontend Tests**:
```bash
cd ai-chat-ui
npm test
```

### Building for Production

**Backend**:
```bash
cd RR.AI-Chat
dotnet publish -c Release -o ./publish
```

**Frontend**:
```bash
cd ai-chat-ui
npm run build
```

## 🐛 Troubleshooting

### Common Issues

#### 1. .NET SDK Version Error
**Error**: `The current .NET SDK does not support targeting .NET 9.0`

**Solution**: Install .NET 9.0 SDK from [Microsoft's download page](https://dotnet.microsoft.com/download/dotnet/9.0).

#### 2. Database Connection Issues
**Error**: Cannot connect to SQL Server

**Solutions**:
- Ensure SQL Server is running
- Verify connection string in `appsettings.json`
- Check if Windows Authentication is enabled (for Integrated Security)
- For Docker SQL Server, ensure proper port mapping

#### 3. CORS Errors
**Error**: CORS policy blocking requests from frontend

**Solutions**:
- Verify `CorsOrigins` in `appsettings.json` includes your frontend URL
- Ensure the API is running on the expected port
- Check if HTTPS redirects are causing issues

#### 4. AI Service Errors
**Error**: API key authentication failed

**Solutions**:
- Verify API keys are correctly set in user secrets
- Check if the AI service endpoint URLs are correct
- Ensure at least one AI service is properly configured

#### 5. Vector Search Issues
**Error**: Vector search operations failing

**Solutions**:
- Ensure SQL Server supports Vector Search (SQL Server 2022+)
- Verify EFCore.SqlServer.VectorSearch package is installed
- Check if embedding model is properly configured

#### 6. Node.js/Angular Issues
**Error**: Node.js version compatibility

**Solutions**:
- Use Node.js 18+ (recommended: LTS version)
- Clear npm cache: `npm cache clean --force`
- Delete `node_modules` and run `npm install` again

#### 7. Port Conflicts
**Error**: Port already in use

**Solutions**:
- API: Modify `launchSettings.json` to use different ports
- Frontend: Use `ng serve --port 4201` to specify different port

### Logs and Debugging

- **API Logs**: Check console output when running `dotnet run`
- **Frontend Logs**: Open browser developer tools (F12)
- **Database**: Use SQL Server Management Studio or Azure Data Studio

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🆘 Support

If you encounter any issues or have questions:

1. Check the [Troubleshooting](#-troubleshooting) section
2. Review the [API documentation](https://localhost:7045/swagger) when the API is running
3. Create an issue in the GitHub repository

## 🔗 Useful Links

- [.NET 9 Documentation](https://docs.microsoft.com/dotnet/)
- [Angular Documentation](https://angular.dev/)
- [Entity Framework Core](https://docs.microsoft.com/ef/core/)
- [OpenAI API Reference](https://platform.openai.com/docs/)
- [Azure OpenAI Service](https://azure.microsoft.com/services/cognitive-services/openai-service/)
- [Anthropic API](https://docs.anthropic.com/)
- [Ollama Documentation](https://ollama.ai/docs)