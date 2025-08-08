# Development Tips - AI Chat Application

This document contains useful tips for developers working on the AI Chat application.

## Development Workflow

### API Development
```bash
cd RR.AI-Chat
dotnet watch run --project RR.AI-Chat.Api
```
This enables hot reload for C# code changes.

### UI Development
```bash
cd ai-chat-ui
ng serve --open
```
This starts the development server with automatic browser opening and hot reload.

## Useful Commands

### .NET API
```bash
# Restore packages
dotnet restore

# Build solution
dotnet build

# Run specific project
dotnet run --project RR.AI-Chat.Api

# Watch for changes
dotnet watch run --project RR.AI-Chat.Api

# Entity Framework migrations (when available)
dotnet ef migrations add InitialCreate --project RR.AI-Chat.Repository --startup-project RR.AI-Chat.Api
dotnet ef database update --project RR.AI-Chat.Api

# User secrets management
dotnet user-secrets list --project RR.AI-Chat.Api
dotnet user-secrets set "OpenAI:ApiKey" "your-key" --project RR.AI-Chat.Api
dotnet user-secrets remove "OpenAI:ApiKey" --project RR.AI-Chat.Api
```

### Angular UI
```bash
# Install dependencies
npm install

# Start development server
npm start
# or
ng serve

# Build for production
npm run build
# or
ng build --configuration production

# Run tests
npm test
# or
ng test

# Generate components
ng generate component components/new-component
ng generate service services/new-service

# Lint code
ng lint

# Update Angular
ng update @angular/core @angular/cli
```

## IDE Configuration

### Visual Studio Code
Recommended extensions:
- C# Dev Kit (for .NET development)
- Angular Language Service
- ESLint
- Prettier

### Visual Studio
- Works out of the box for .NET development
- Use VS Code for Angular development

## Environment Configuration

### Development Environment Variables
Create `RR.AI-Chat/RR.AI-Chat.Api/appsettings.Development.json`:
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "DetailedErrors": true,
  "CorsOrigins": ["http://localhost:4200", "https://localhost:4200"]
}
```

### User Secrets for Development
```bash
cd RR.AI-Chat/RR.AI-Chat.Api

# Set OpenAI
dotnet user-secrets set "OpenAI:ApiKey" "sk-your-openai-key"

# Set Azure AI Foundry
dotnet user-secrets set "AzureAIFoundry:Url" "https://your-resource.openai.azure.com/"
dotnet user-secrets set "AzureAIFoundry:ApiKey" "your-azure-key"
dotnet user-secrets set "AzureAIFoundry:EmbeddingModel" "text-embedding-ada-002"

# Set Anthropic
dotnet user-secrets set "Anthropic:ApiKey" "sk-ant-your-anthropic-key"
```

## Database Development

### SQL Server Setup for Development
```bash
# Using Docker
docker run -e "ACCEPT_EULA=Y" -e "SA_PASSWORD=YourStrong@Passw0rd123" \
  -p 1433:1433 --name sql_server_dev \
  -d mcr.microsoft.com/mssql/server:2022-latest

# Connection string for Docker SQL Server
"Server=localhost,1433;Database=aichat;User Id=sa;Password=YourStrong@Passw0rd123;Encrypt=true;TrustServerCertificate=true;"
```

## Debugging

### API Debugging
1. Set breakpoints in Visual Studio or VS Code
2. Start debugging with F5
3. Make requests from the UI or Swagger

### UI Debugging
1. Open browser developer tools (F12)
2. Use Angular DevTools extension
3. Check console for errors
4. Use Network tab to monitor API calls

## Performance Tips

### API Performance
- Use async/await for all I/O operations
- Implement proper logging
- Consider caching for frequently accessed data

### UI Performance
- Use OnPush change detection strategy
- Implement lazy loading for routes
- Optimize bundle size with proper imports

## Testing

### API Testing
```bash
# Run unit tests
dotnet test

# Run with coverage
dotnet test --collect:"XPlat Code Coverage"
```

### UI Testing
```bash
# Unit tests
npm test

# E2E tests (if configured)
npm run e2e
```

## Code Style

### .NET Code Style
- Follow C# naming conventions
- Use async/await patterns
- Implement proper error handling
- Add XML documentation comments

### Angular Code Style
- Follow Angular style guide
- Use TypeScript strict mode
- Implement proper type definitions
- Use reactive patterns with RxJS

## Common Issues

### Build Issues
- Clear bin/obj folders: `dotnet clean`
- Restore packages: `dotnet restore`
- Clear npm cache: `npm cache clean --force`
- Delete node_modules: `rm -rf node_modules && npm install`

### CORS Issues
- Ensure CorsOrigins includes your frontend URL
- Check both HTTP and HTTPS URLs
- Verify API is running on expected port

### Database Issues
- Check SQL Server is running
- Verify connection string
- Run database migrations

## Contributing Guidelines

1. Create feature branch from main
2. Make small, focused commits
3. Write descriptive commit messages
4. Include tests for new features
5. Update documentation as needed
6. Submit PR with clear description

## Resources

- [.NET Documentation](https://docs.microsoft.com/dotnet/)
- [Angular Documentation](https://angular.io/docs)
- [Entity Framework Core](https://docs.microsoft.com/ef/core/)
- [ASP.NET Core](https://docs.microsoft.com/aspnet/core/)
- [TypeScript Handbook](https://www.typescriptlang.org/docs/)