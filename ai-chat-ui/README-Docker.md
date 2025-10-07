# AI Chat UI - Docker Setup

This project includes Docker configurations for both development and production environments.

## Files Overview

- `Dockerfile` - Production build (multi-stage with Nginx)
- `Dockerfile.dev` - Development build (with hot reload)
- `docker-compose.yml` - Multi-profile compose file
- `docker-compose.dev.yml` - Development-only compose file
- `nginx.conf` - Custom Nginx configuration for production

## Development Setup

### Option 1: Direct Docker Build

```bash
# Build development image
docker build -f Dockerfile.dev -t ai-chat-ui-dev .

# Run development container
docker run -p 4200:4200 -v ${PWD}:/app -v /app/node_modules ai-chat-ui-dev
```

### Option 2: Docker Compose (Recommended)

```bash
# Using development-specific compose file
docker-compose -f docker-compose.dev.yml up

# Or using profiles in main compose file
docker-compose --profile dev up
```

**Development Features:**

- Hot reload enabled
- Source code mounted as volume
- Runs on port 4200
- Development server with live updates

## Production Setup

### Option 1: Direct Docker Build

```bash
# Build production image
docker build -t ai-chat-ui .

# Run production container
docker run -p 8080:80 ai-chat-ui
```

### Option 2: Docker Compose

```bash
# Using profiles in main compose file
docker-compose --profile prod up
```

**Production Features:**

- Optimized build with Angular CLI
- Served by Nginx
- Smaller image size (multi-stage build)
- Production optimizations enabled
- Runs on port 80 (mapped to 8080)

## Key Differences

| Feature         | Development                 | Production           |
| --------------- | --------------------------- | -------------------- |
| Build Type      | Development server          | Production build     |
| Hot Reload      | ✅ Yes                      | ❌ No                |
| Volume Mounting | ✅ Yes (live editing)       | ❌ No                |
| Image Size      | Larger (includes dev tools) | Smaller (optimized)  |
| Performance     | Development optimized       | Production optimized |
| Port            | 4200                        | 80 (mapped to 8080)  |
| Server          | Angular Dev Server          | Nginx                |

## Environment Variables

You can customize the build using environment variables:

```bash
# Development
NODE_ENV=development

# Production
NODE_ENV=production
```

## Nginx Configuration

The production setup includes a custom Nginx configuration that:

- Handles Angular routing (SPA support)
- Sets up caching for static assets
- Includes security headers
- Enables gzip compression

## Troubleshooting

### Build Issues

- If you encounter budget errors, the production build includes `--no-budgets` flag
- For memory issues during build, you might need to increase Docker memory limits

### Port Conflicts

- Development uses port 4200
- Production uses port 8080 (mapped from 80)
- Change ports in docker-compose files if needed

### Volume Issues (Development)

- The `/app/node_modules` volume prevents local node_modules from overriding container modules
- If you face permission issues, adjust volume mounting based on your OS

## Quick Commands

```bash
# Development - Quick start
docker-compose -f docker-compose.dev.yml up

# Production - Quick start
docker-compose --profile prod up

# Build only (no run)
docker build -t ai-chat-ui .

# Clean rebuild
docker build --no-cache -t ai-chat-ui .
```
