#!/bin/bash

# AI Chat Application Setup Script
# This script helps set up the development environment

echo "🚀 AI Chat Application Setup"
echo "=============================="

# Check prerequisites
echo "📋 Checking prerequisites..."

# Check .NET
if command -v dotnet &> /dev/null; then
    DOTNET_VERSION=$(dotnet --version)
    echo "✅ .NET SDK found: $DOTNET_VERSION"
    
    # Check if .NET 9 is available
    if [[ "$DOTNET_VERSION" == 9.* ]]; then
        echo "✅ .NET 9.0 SDK detected"
    else
        echo "⚠️  Warning: .NET 9.0 SDK recommended (current: $DOTNET_VERSION)"
        echo "   Download from: https://dotnet.microsoft.com/download/dotnet/9.0"
    fi
else
    echo "❌ .NET SDK not found. Please install .NET 9.0 SDK"
    echo "   Download from: https://dotnet.microsoft.com/download/dotnet/9.0"
    exit 1
fi

# Check Node.js
if command -v node &> /dev/null; then
    NODE_VERSION=$(node --version)
    echo "✅ Node.js found: $NODE_VERSION"
    
    # Check Node.js version (should be 18+)
    NODE_MAJOR_VERSION=$(echo $NODE_VERSION | cut -d'.' -f1 | sed 's/v//')
    if [ "$NODE_MAJOR_VERSION" -ge 18 ]; then
        echo "✅ Node.js version is compatible"
    else
        echo "⚠️  Warning: Node.js 18+ recommended (current: $NODE_VERSION)"
    fi
else
    echo "❌ Node.js not found. Please install Node.js 18+"
    echo "   Download from: https://nodejs.org/"
    exit 1
fi

# Check Angular CLI
if command -v ng &> /dev/null; then
    echo "✅ Angular CLI found"
else
    echo "⚠️  Angular CLI not found. Installing..."
    npm install -g @angular/cli
fi

echo ""
echo "🔧 Setting up the project..."

# Setup API
echo "📦 Setting up .NET API..."
cd RR.AI-Chat
if dotnet restore; then
    echo "✅ API packages restored"
else
    echo "❌ Failed to restore API packages"
    exit 1
fi

# Initialize user secrets
cd RR.AI-Chat.Api
if dotnet user-secrets init; then
    echo "✅ User secrets initialized"
else
    echo "⚠️  User secrets may already be initialized"
fi

cd ../..

# Setup UI
echo "📦 Setting up Angular UI..."
cd ai-chat-ui
if npm install; then
    echo "✅ UI packages installed"
else
    echo "❌ Failed to install UI packages"
    exit 1
fi

cd ..

echo ""
echo "🎉 Setup completed!"
echo ""
echo "📝 Next steps:"
echo "1. Configure your AI service API keys:"
echo "   cd RR.AI-Chat/RR.AI-Chat.Api"
echo "   dotnet user-secrets set \"OpenAI:ApiKey\" \"your-api-key\""
echo ""
echo "2. Start the API:"
echo "   cd RR.AI-Chat"
echo "   dotnet run --project RR.AI-Chat.Api"
echo ""
echo "3. Start the UI (in a new terminal):"
echo "   cd ai-chat-ui"
echo "   npm start"
echo ""
echo "4. Access the application:"
echo "   Frontend: http://localhost:4200"
echo "   API Docs: https://localhost:7045/swagger"
echo ""
echo "📚 For detailed setup instructions, see README.md"