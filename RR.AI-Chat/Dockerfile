# Multi-stage Dockerfile for .NET API

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy project files
COPY ["RR.AI-Chat.Api/RR.AI-Chat.Api.csproj", "RR.AI-Chat.Api/"]
COPY ["RR.AI-Chat.Service/RR.AI-Chat.Service.csproj", "RR.AI-Chat.Service/"]
COPY ["RR.AI-Chat.Repository/RR.AI-Chat.Repository.csproj", "RR.AI-Chat.Repository/"]
COPY ["RR.AI-Chat.Entity/RR.AI-Chat.Entity.csproj", "RR.AI-Chat.Entity/"]
COPY ["RR.AI-Chat.Dto/RR.AI-Chat.Dto.csproj", "RR.AI-Chat.Dto/"]

# Restore dependencies
RUN dotnet restore "RR.AI-Chat.Api/RR.AI-Chat.Api.csproj"

# Copy source code
COPY . .

# Build application
WORKDIR "/src/RR.AI-Chat.Api"
RUN dotnet build "RR.AI-Chat.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "RR.AI-Chat.Api.csproj" -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

# Install SQL Server tools for health checks (optional)
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

COPY --from=publish /app/publish .

# Expose ports
EXPOSE 80
EXPOSE 443

ENTRYPOINT ["dotnet", "RR.AI-Chat.Api.dll"]