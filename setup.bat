@echo off
setlocal enabledelayedexpansion

:: AI Chat Application Setup Script for Windows
:: This script helps set up the development environment

echo 🚀 AI Chat Application Setup
echo ==============================

:: Check prerequisites
echo 📋 Checking prerequisites...

:: Check .NET
where dotnet >nul 2>nul
if %errorlevel% equ 0 (
    for /f "tokens=*" %%i in ('dotnet --version') do set DOTNET_VERSION=%%i
    echo ✅ .NET SDK found: !DOTNET_VERSION!
    
    :: Check if .NET 9 is available
    echo !DOTNET_VERSION! | findstr /R "^9\." >nul
    if !errorlevel! equ 0 (
        echo ✅ .NET 9.0 SDK detected
    ) else (
        echo ⚠️  Warning: .NET 9.0 SDK recommended ^(current: !DOTNET_VERSION!^)
        echo    Download from: https://dotnet.microsoft.com/download/dotnet/9.0
    )
) else (
    echo ❌ .NET SDK not found. Please install .NET 9.0 SDK
    echo    Download from: https://dotnet.microsoft.com/download/dotnet/9.0
    pause
    exit /b 1
)

:: Check Node.js
where node >nul 2>nul
if %errorlevel% equ 0 (
    for /f "tokens=*" %%i in ('node --version') do set NODE_VERSION=%%i
    echo ✅ Node.js found: !NODE_VERSION!
    
    :: Extract major version number
    set NODE_VERSION_NUM=!NODE_VERSION:~1,2!
    if !NODE_VERSION_NUM! geq 18 (
        echo ✅ Node.js version is compatible
    ) else (
        echo ⚠️  Warning: Node.js 18+ recommended ^(current: !NODE_VERSION!^)
    )
) else (
    echo ❌ Node.js not found. Please install Node.js 18+
    echo    Download from: https://nodejs.org/
    pause
    exit /b 1
)

:: Check Angular CLI
where ng >nul 2>nul
if %errorlevel% equ 0 (
    echo ✅ Angular CLI found
) else (
    echo ⚠️  Angular CLI not found. Installing...
    npm install -g @angular/cli
)

echo.
echo 🔧 Setting up the project...

:: Setup API
echo 📦 Setting up .NET API...
cd RR.AI-Chat
dotnet restore
if %errorlevel% equ 0 (
    echo ✅ API packages restored
) else (
    echo ❌ Failed to restore API packages
    pause
    exit /b 1
)

:: Initialize user secrets
cd RR.AI-Chat.Api
dotnet user-secrets init
if %errorlevel% equ 0 (
    echo ✅ User secrets initialized
) else (
    echo ⚠️  User secrets may already be initialized
)

cd ..\..

:: Setup UI
echo 📦 Setting up Angular UI...
cd ai-chat-ui
npm install
if %errorlevel% equ 0 (
    echo ✅ UI packages installed
) else (
    echo ❌ Failed to install UI packages
    pause
    exit /b 1
)

cd ..

echo.
echo 🎉 Setup completed!
echo.
echo 📝 Next steps:
echo 1. Configure your AI service API keys:
echo    cd RR.AI-Chat\RR.AI-Chat.Api
echo    dotnet user-secrets set "OpenAI:ApiKey" "your-api-key"
echo.
echo 2. Start the API:
echo    cd RR.AI-Chat
echo    dotnet run --project RR.AI-Chat.Api
echo.
echo 3. Start the UI ^(in a new command prompt^):
echo    cd ai-chat-ui
echo    npm start
echo.
echo 4. Access the application:
echo    Frontend: http://localhost:4200
echo    API Docs: https://localhost:7045/swagger
echo.
echo 📚 For detailed setup instructions, see README.md

pause