# Quick Start Guide - AI Chat Application

This guide helps you get the AI Chat application running quickly for development or demonstration purposes.

## Option 1: Frontend Only (Demo Mode)
If you just want to see the UI without setting up the full backend:

```bash
cd ai-chat-ui
npm install
npm start
```

Visit `http://localhost:4200` to see the frontend. Note: API calls will fail without the backend.

## Option 2: Full Stack with Docker
If you have Docker installed:

```bash
# Build and run the entire stack
docker-compose up -d

# Wait for services to start, then visit:
# Frontend: http://localhost:4200
# API: http://localhost:5000
```

## Option 3: Full Local Development

### Prerequisites
- .NET 9.0 SDK
- Node.js 18+
- SQL Server

### Quick Setup
```bash
# Auto-setup (Linux/macOS)
./setup.sh

# Or manually:
cd RR.AI-Chat
dotnet restore
dotnet user-secrets set "OpenAI:ApiKey" "your-api-key"

cd ../ai-chat-ui  
npm install
```

### Run Applications
```bash
# Terminal 1 - API
cd RR.AI-Chat
dotnet run --project RR.AI-Chat.Api

# Terminal 2 - UI
cd ai-chat-ui
npm start
```

## Next Steps
- Configure AI service API keys (see main README.md)
- Set up database connection
- Review troubleshooting section for common issues

For complete setup instructions, see the main [README.md](README.md) file.