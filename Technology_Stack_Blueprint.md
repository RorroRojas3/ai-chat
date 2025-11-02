# Technology Stack Blueprint - AI Chat Application

**Project Type**: Full-stack Application (.NET Backend + Angular Frontend)  
**Depth Level**: Implementation-Ready  
**Generated**: 2025-11-02  
**Format**: Markdown  
**Categorization**: Technology Type & Layer

---

## Executive Summary

The AI Chat Application is a modern, full-stack solution built on .NET 9 and Angular 19, designed to provide multi-provider AI chat capabilities with document management and vector search. The architecture follows clean separation of concerns with distinct layers for API, Service, Repository, Entity, and DTO components on the backend, complemented by a component-based Angular frontend.

### Technology Decision Context

- **.NET 9.0**: Latest LTS version providing modern C# features, improved performance, and enhanced AI capabilities
- **Angular 19**: Latest version of Angular with standalone components, signals, and modern TypeScript features
- **SQL Server with Vector Search**: Chosen for enterprise-grade data persistence with AI-powered document search capabilities
- **Microsoft.Extensions.AI**: Microsoft's unified AI abstraction layer supporting multiple providers (OpenAI, Azure AI, Anthropic, Ollama)
- **Azure AD/Microsoft Identity**: Enterprise authentication and authorization
- **Hangfire**: Background job processing for document embeddings and long-running operations

---

## 1. .NET Backend Stack Analysis

### Target Framework & Language
- **Target Framework**: `net9.0`
- **C# Language Version**: C# 13 (default for .NET 9)
- **Nullable Reference Types**: Enabled (`<Nullable>enable</Nullable>`)
- **Implicit Usings**: Enabled (`<ImplicitUsings>enable</ImplicitUsings>`)

### Project Structure

```
RR.AI-Chat/
├── RR.AI-Chat.Api/         # Web API Controllers, Program.cs, Middleware
├── RR.AI-Chat.Service/     # Business Logic Services
├── RR.AI-Chat.Repository/  # Data Access Layer, EF Core DbContext
├── RR.AI-Chat.Entity/      # Entity Models
└── RR.AI-Chat.Dto/         # Data Transfer Objects, Enums
```

### NuGet Package Dependencies

#### Core Framework & AI Services
| Package | Version | Purpose |
|---------|---------|---------|
| **Microsoft.Extensions.AI** | 9.10.1 | Unified AI abstraction layer for chat clients |
| **Microsoft.Extensions.AI.Abstractions** | 9.10.1 | AI service interfaces and abstractions |
| **Microsoft.Extensions.AI.OpenAI** | 9.7.1-preview.1.25365.4 | OpenAI integration for AI services |
| **Azure.AI.OpenAI** | 2.5.0-beta.1 | Azure OpenAI Service client |
| **Anthropic.SDK** | 5.8.0 | Claude AI models integration |
| **OllamaSharp** | 5.4.8 | Local Ollama AI models |
| **OpenAI** | 2.5.0 | OpenAI GPT models |
| **Azure.AI.DocumentIntelligence** | 1.0.0 | Azure document parsing and extraction |
| **ModelContextProtocol** | 0.4.0-preview.3 | MCP server integration |

#### Data Access & Storage
| Package | Version | Purpose |
|---------|---------|---------|
| **Microsoft.EntityFrameworkCore** | 9.0.10 | ORM for database operations |
| **Microsoft.EntityFrameworkCore.SqlServer** | 9.0.10 | SQL Server provider |
| **EFCore.SqlServer.VectorSearch** | 9.0.0 | Vector embedding search capabilities |
| **Microsoft.EntityFrameworkCore.Design** | 9.0.10 | Design-time support for migrations |
| **Microsoft.EntityFrameworkCore.Tools** | 9.0.10 | EF Core tooling for migrations |
| **Azure.Storage.Blobs** | 12.26.0 | Azure Blob Storage for document storage |

#### Authentication & Identity
| Package | Version | Purpose |
|---------|---------|---------|
| **Microsoft.Identity.Web** | 4.0.1 | Azure AD authentication for APIs |
| **Microsoft.Graph** | 5.95.0 | Microsoft Graph API client for user management |

#### Background Processing
| Package | Version | Purpose |
|---------|---------|---------|
| **Hangfire** | 1.8.21 | Background job processing server |
| **Hangfire.Core** | 1.8.21 | Core Hangfire functionality |

#### Document Processing
| Package | Version | Purpose |
|---------|---------|---------|
| **Aspose.PDF** | 25.10.0 | Advanced PDF document processing |
| **Aspose.Words** | 25.10.0 | Word document processing |
| **PdfPig** | 0.1.11 | PDF parsing and text extraction |
| **Markdig** | 0.43.0 | Markdown parsing and processing |
| **ReverseMarkdown** | 4.7.1 | HTML to Markdown conversion |
| **Microsoft.ML.Tokenizers** | 2.0.0-preview.25503.2 | Text tokenization for AI models |

#### API & Development Tools
| Package | Version | Purpose |
|---------|---------|---------|
| **Swashbuckle.AspNetCore** | 9.0.6 | Swagger/OpenAPI documentation |

---

## 2. Angular Frontend Stack Analysis

### Core Framework
- **Angular Version**: 19.0.0
- **TypeScript Version**: 5.6.2
- **ECMAScript Target**: ES2022
- **Module System**: ES2022 (ESM)
- **Build System**: Angular CLI with application builder

### NPM Dependencies

#### Core Angular Packages
| Package | Version | Purpose |
|---------|---------|---------|
| **@angular/core** | ^19.0.0 | Core Angular framework |
| **@angular/common** | ^19.0.0 | Common Angular utilities |
| **@angular/router** | ^19.0.0 | Client-side routing |
| **@angular/forms** | ^19.0.0 | Reactive and template-driven forms |
| **@angular/platform-browser** | ^19.0.0 | Browser platform support |
| **@angular/animations** | ^19.0.0 | Animation framework |
| **zone.js** | ~0.15.0 | Change detection mechanism |
| **rxjs** | ~7.8.0 | Reactive programming library |

#### Authentication
| Package | Version | Purpose |
|---------|---------|---------|
| **@azure/msal-angular** | ^4.0.20 | Azure AD authentication for Angular |
| **@azure/msal-browser** | ^4.25.0 | MSAL browser support |

#### UI & Styling
| Package | Version | Purpose |
|---------|---------|---------|
| **bootstrap** | ^5.3.8 | CSS framework for responsive design |
| **bootstrap-icons** | ^1.13.1 | Icon library |
| **@popperjs/core** | ^2.11.8 | Tooltip and popover positioning |

#### Content Rendering
| Package | Version | Purpose |
|---------|---------|---------|
| **markdown-it** | ^14.1.0 | Markdown parsing and rendering |
| **markdown-it-highlightjs** | ^4.2.0 | Code syntax highlighting in markdown |
| **highlight.js** | ^11.11.1 | Code syntax highlighting |

#### Development Dependencies
| Package | Version | Purpose |
|---------|---------|---------|
| **@angular/cli** | ^19.0.6 | Angular command-line interface |
| **@angular-devkit/build-angular** | ^19.0.6 | Angular build tools |
| **@angular/compiler-cli** | ^19.0.0 | Angular compiler |
| **jasmine-core** | ~5.4.0 | Testing framework |
| **karma** | ~6.4.0 | Test runner |

### TypeScript Configuration
```json
{
  "target": "ES2022",
  "module": "ES2022",
  "strict": true,
  "noImplicitOverride": true,
  "noPropertyAccessFromIndexSignature": true,
  "noImplicitReturns": true,
  "noFallthroughCasesInSwitch": true,
  "experimentalDecorators": true,
  "strictInjectionParameters": true,
  "strictTemplates": true
}
```

### Angular Application Structure
```
ai-chat-ui/src/app/
├── components/          # Reusable UI components
├── pages/              # Route-level page components
├── services/           # HTTP and business logic services
├── dtos/              # TypeScript data transfer objects
├── shared/            # Shared utilities and components
├── store/             # State management service
├── app.component.ts   # Root component
├── app.config.ts      # Application configuration
└── app.routes.ts      # Routing configuration
```

---

## 3. Implementation Patterns & Conventions

### .NET Backend Conventions

#### Naming Conventions

**Namespaces & Root Names**
- Root namespace pattern: `RR.AI_Chat.{Layer}` (e.g., `RR.AI_Chat.Api`, `RR.AI_Chat.Service`)
- File-scoped namespaces used throughout

**Class Naming**
- Controllers: `{Entity}Controller` (e.g., `ChatsController`, `SessionsController`)
- Services: `I{Entity}Service` (interface), `{Entity}Service` (implementation)
- DTOs: `{Purpose}Dto` (e.g., `ChatStreamRequestDto`, `SessionDto`)
- Entities: `{Name}` (e.g., `Session`, `Document`, `User`)

**Method Naming**
- Async methods: Always end with `Async` suffix
- Query methods: `Get{Entity}Async`, `List{Entities}Async`
- Command methods: `Create{Entity}Async`, `Update{Entity}Async`, `Delete{Entity}Async`

**File Naming**
- One class per file
- File name matches class name exactly

#### Code Organization

**Project Layer Responsibilities**
```
Api Layer:
  - Controllers (API endpoints)
  - Program.cs (DI registration, middleware pipeline)
  - Middleware components
  - No business logic

Service Layer:
  - Business logic implementation
  - Service interfaces and implementations
  - Cross-cutting concerns (logging, validation)
  - Document processing services
  - AI service orchestration

Repository Layer:
  - DbContext configuration
  - Entity configurations
  - Database migrations
  - No business logic

Entity Layer:
  - Domain models/entities
  - Entity base classes
  - No business logic

Dto Layer:
  - Data transfer objects
  - Enums
  - Request/Response models
  - Actions (MCP protocol actions)
```

#### Dependency Injection Patterns

**Service Registration Approach** (from `Program.cs`):
```csharp
// Singleton - Stateless services, caching, locks
builder.Services.AddSingleton<ISessionLockService, SessionLockService>();
builder.Services.AddSingleton<ITokenService, TokenService>();
builder.Services.AddSingleton<IGraphService, GraphService>();
builder.Services.AddSingleton<IBlobStorageService, BlobStorageService>();
builder.Services.AddSingleton<IDocumentIntelligenceService, DocumentIntelligenceService>();
builder.Services.AddSingleton<IHtmlService, HtmlService>();
builder.Services.AddSingleton<IPdfService, PdfService>();
builder.Services.AddSingleton<IWordService, WordService>();
builder.Services.AddSingleton<IMarkdownService, MarkdownService>();

// Scoped - Per-request services with DbContext dependency
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IDocumentToolService, DocumentToolService>();
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<IModelService, ModelService>();
builder.Services.AddScoped<IMcpServerService, McpServerService>();
builder.Services.AddScoped<IUserService, UserService>();

// Keyed Services - Multiple implementations of same interface
builder.Services.AddKeyedChatClient("azureaifoundry", sp => ...);
```

**Constructor Injection Pattern**:
```csharp
public class ChatService(
    ILogger<ChatService> logger,
    IDocumentToolService documentToolService,
    ISessionService sessionService,
    IModelService modelService,
    [FromKeyedServices("azureaifoundry")] IChatClient azureAIFoundry,
    IMcpServerService mcpServerService,
    ISessionLockService sessionLockService,
    ITokenService tokenService,
    AIChatDbContext ctx) : IChatService
{
    private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly IChatClient _azureAIFoundry = azureAIFoundry ?? throw new ArgumentNullException(nameof(azureAIFoundry));
    // ... null checks for all dependencies
}
```

#### Controller Patterns

**Standard Controller Structure**:
```csharp
[Authorize]  // All controllers require authentication
[Route("api/[controller]")]
[ApiController]
public class ChatsController(IChatService chatService) : ControllerBase
{
    private readonly IChatService _chatService = chatService;

    [HttpPost("sessions/{sessionId}/stream")]
    public async Task GetChatStreamingAsync(Guid sessionId, ChatStreamRequestDto request, CancellationToken cancellationToken)
    {
        // Server-sent events configuration
        Response.Headers.Append("Content-Type", "text/event-stream");
        Response.Headers.Append("Cache-Control", "no-cache");
        Response.Headers.Append("Connection", "keep-alive");

        // Streaming response pattern
        await foreach (var message in _chatService.GetChatStreamingAsync(sessionId, request, cancellationToken))
        {
            await Response.WriteAsync($"{message}", cancellationToken);
            await Response.Body.FlushAsync(cancellationToken);
        }
    }
}
```

**Route Attribute Conventions**:
- Base route: `api/[controller]`
- Action routes: `{action}/{id}` or custom semantic routes
- RESTful resource routes: `sessions/{sessionId}/conversations`

#### Data Access Patterns

**DbContext Configuration**:
```csharp
public class AIChatDbContext(DbContextOptions<AIChatDbContext> options) : DbContext(options)
{
    public DbSet<AIService> AIServices { get; set; }
    public DbSet<Document> Documents { get; set; }
    public DbSet<Session> Sessions { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // JSON serialization for complex types
        modelBuilder.Entity<Session>()
            .Property(e => e.Conversations)
            .HasColumnType("nvarchar(max)")
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<List<Conversation>>(v, (JsonSerializerOptions?)null) ?? new());

        // Vector embeddings configuration
        modelBuilder.Entity<DocumentPage>()
            .Property(p => p.Embedding)
            .HasColumnType("vector(1536)");

        // Configuration classes
        modelBuilder.ApplyConfiguration(new AIServiceConfiguration());
        
        // Global delete behavior
        foreach (var relationship in modelBuilder.Model.GetEntityTypes()
            .SelectMany(e => e.GetForeignKeys()))
        {
            relationship.DeleteBehavior = DeleteBehavior.NoAction;
        }
    }
}
```

**Query Patterns**:
```csharp
// Single entity retrieval with filtering
var session = await _ctx.Sessions
    .SingleOrDefaultAsync(x => 
        x.Id == sessionId && 
        x.UserId == userId && 
        !x.DateDeactivated.HasValue, 
        cancellationToken);

// No explicit repository pattern - DbContext used directly in services
```

**Migration Strategy**:
- Automatic migration on application startup:
```csharp
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AIChatDbContext>();
    context.Database.Migrate();
}
```

#### Error Handling Patterns

**Null Checks**:
```csharp
// Constructor parameters
private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));

// Method parameters - ArgumentNullException.ThrowIfNull preferred for .NET 9
ArgumentNullException.ThrowIfNull(parameter);
```

**Exception Types**:
```csharp
// Invalid operation
throw new InvalidOperationException($"Session {sessionId} not found.");

// Logging before throwing
_logger.LogError("Session with id {id} not found.", sessionId);
throw new InvalidOperationException($"Session with id {sessionId} not found.");
```

#### Asynchronous Programming Patterns

**Async/Await Usage**:
- All I/O operations are async
- CancellationToken passed through entire call chain
- ConfigureAwait(false) used in service layer for performance

**Async Streaming**:
```csharp
public async IAsyncEnumerable<string?> GetChatStreamingAsync(
    Guid sessionId, 
    ChatStreamRequestDto request, 
    [EnumeratorCancellation] CancellationToken cancellationToken)
{
    await foreach (var message in chatClient.GetStreamingResponseAsync(..., cancellationToken))
    {
        yield return message;
    }
}
```

#### Configuration Access Patterns

**Configuration Binding**:
```csharp
// Section binding
var corsOrigins = builder.Configuration.GetSection("CorsOrigins").Get<string[]>();

// Direct value access with default
var ollamaUrl = builder.Configuration.GetValue<string>("OllamaUrl") ?? "http://localhost:11434/";

// Typed configuration
builder.Services.Configure<List<McpServerSettings>>(
    builder.Configuration.GetSection("McpServers"));
```

**User Secrets for Sensitive Data**:
- API keys stored in user secrets, not appsettings.json
- Connection strings can be in appsettings.json for local development

### Angular Frontend Conventions

#### Naming Conventions

**Files & Components**
- Components: `{name}.component.ts` (e.g., `chat.component.ts`)
- Services: `{name}.service.ts` (e.g., `session.service.ts`)
- DTOs: `{Name}Dto.ts` (e.g., `SessionDto.ts`)
- PascalCase for class names, kebab-case for file names

**Component Selectors**
- Prefix: `app-` (e.g., `app-chat`, `app-session-list`)

**Method Naming**
- Observable methods: No special suffix, return type indicates Observable
- Async operations: Use RxJS operators, not async/await in services

#### Component Patterns

**Standalone Components** (Angular 19 pattern):
```typescript
@Component({
  selector: 'app-chat',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './chat.component.html',
  styleUrl: './chat.component.scss'
})
export class ChatComponent {
  // Component logic
}
```

**Dependency Injection**:
```typescript
export class ChatService {
  constructor(
    private http: HttpClient,
    private storeService: StoreService,
    private zone: NgZone,
    private msalService: MsalService
  ) {}
}
```

#### HTTP Service Patterns

**Standard HTTP Service**:
```typescript
@Injectable({ providedIn: 'root' })
export class SessionService {
  constructor(private http: HttpClient) {}

  getSessions(): Observable<SessionDto[]> {
    return this.http.get<SessionDto[]>(`${environment.apiUrl}sessions`);
  }

  createSession(): Observable<SessionDto> {
    return this.http.post<SessionDto>(`${environment.apiUrl}sessions`, {});
  }
}
```

**Server-Sent Events Pattern**:
```typescript
getServerSentEvent(prompt: string): Observable<string> {
  return new Observable((observer) => {
    const decoder = new TextDecoder();
    
    acquireToken().then(accessToken => {
      return fetch(url, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          'Accept': 'text/event-stream',
          'Authorization': `Bearer ${accessToken}`
        },
        body: JSON.stringify(request)
      });
    })
    .then(response => {
      const reader = response.body.getReader();
      // Stream processing logic
    });
  });
}
```

#### State Management Pattern

**Store Service** (Signals-based):
```typescript
@Injectable({ providedIn: 'root' })
export class StoreService {
  sessionId = signal<string>('');
  selectedModel = signal<ModelDto | null>(null);
  selectedMcps = signal<McpDto[]>([]);
}
```

#### Authentication Pattern

**MSAL Configuration**:
```typescript
export function MSALInstanceFactory(): IPublicClientApplication {
  return new PublicClientApplication({
    auth: {
      clientId: environment.msalConfig.auth.clientId,
      authority: environment.msalConfig.auth.authority,
      redirectUri: '/',
    },
    cache: {
      cacheLocation: BrowserCacheLocation.SessionStorage,
    }
  });
}
```

**Token Acquisition**:
```typescript
const account = this.msalService.instance.getActiveAccount();
const tokenResponse = await this.msalService.instance.acquireTokenSilent({
  scopes: environment.apiConfig.scopes,
  account: account,
});
```

#### Styling Approach

**SCSS with Bootstrap**:
- Bootstrap 5.3 for base styles
- Component-specific SCSS files
- Global styles in `styles.scss`
- Bootstrap icons for UI icons

---

## 4. Technology Stack Map

### Backend Technology Layers

```
┌─────────────────────────────────────────────────────────┐
│                    API Layer (Controllers)               │
│  • Authentication (Azure AD/JWT)                        │
│  • Authorization Attributes                             │
│  • Request/Response DTOs                                │
│  • Swagger Documentation                                │
└────────────────────┬────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────┐
│                 Service Layer (Business Logic)           │
│  • ChatService (AI orchestration)                       │
│  • DocumentService (document management)                │
│  • SessionService (session management)                  │
│  • GraphService (Microsoft Graph integration)           │
│  • McpServerService (MCP protocol)                      │
└────────────────────┬────────────────────────────────────┘
                     │
         ┌───────────┴──────────┐
         │                      │
┌────────▼─────────┐  ┌────────▼──────────────────────────┐
│ Repository Layer │  │   External Services               │
│  • DbContext     │  │  • Azure AI Foundry               │
│  • Configurations│  │  • OpenAI                         │
│  • Migrations    │  │  • Anthropic                      │
└────────┬─────────┘  │  • Ollama                         │
         │            │  • Azure Blob Storage             │
┌────────▼─────────┐  │  • Azure Document Intelligence    │
│   Entity Layer   │  │  • Microsoft Graph                │
│  • Domain Models │  └───────────────────────────────────┘
└──────────────────┘
         │
┌────────▼──────────────────────────────────────────────┐
│                SQL Server Database                     │
│  • Tables (Sessions, Documents, Users, Models)        │
│  • Vector Search (document embeddings)                │
│  • Hangfire tables (background jobs)                  │
└────────────────────────────────────────────────────────┘
```

### Frontend Technology Layers

```
┌─────────────────────────────────────────────────────────┐
│                  Pages (Route Components)                │
│  • Chat Page                                            │
│  • Session Management                                   │
└────────────────────┬────────────────────────────────────┘
                     │
┌────────────────────▼────────────────────────────────────┐
│              Components (UI Components)                  │
│  • Message Display                                      │
│  • Chat Input                                           │
│  • Session List                                         │
│  • Document Upload                                      │
└────────────────────┬────────────────────────────────────┘
                     │
         ┌───────────┴──────────┐
         │                      │
┌────────▼─────────┐  ┌────────▼─────────────────────────┐
│  Services Layer  │  │   Store (State Management)       │
│  • HTTP Services │  │  • Signals-based                 │
│  • ChatService   │  │  • Session state                 │
│  • SessionSvc    │  │  • Model selection               │
│  • DocumentSvc   │  │  • MCP selection                 │
└────────┬─────────┘  └──────────────────────────────────┘
         │
┌────────▼──────────────────────────────────────────────┐
│              Authentication Layer                      │
│  • MSAL Angular                                       │
│  • Token Interceptor                                  │
│  • Azure AD Integration                               │
└────────┬──────────────────────────────────────────────┘
         │
┌────────▼──────────────────────────────────────────────┐
│                  Backend API                           │
│  • HTTPS requests with JWT tokens                     │
│  • Server-sent events for streaming                   │
└────────────────────────────────────────────────────────┘
```

### Integration Points

#### Authentication Flow
```
1. User Login → MSAL Angular
2. MSAL Angular → Azure AD (OAuth 2.0/OIDC)
3. Azure AD → Access Token (JWT)
4. Angular → API Request + Bearer Token
5. API → Microsoft.Identity.Web validates token
6. API → UserExistenceMiddleware ensures user in database
7. API → Controller action authorized
```

#### Chat Streaming Flow
```
1. User Input → Chat Component
2. Chat Component → ChatService.getServerSentEvent()
3. ChatService → Acquire MSAL token
4. ChatService → POST /api/chats/sessions/{id}/stream (SSE)
5. API → ChatsController.GetChatStreamingAsync()
6. ChatsController → ChatService.GetChatStreamingAsync()
7. ChatService → Session validation & lock acquisition
8. ChatService → IChatClient (keyed service: azureaifoundry)
9. IChatClient → Azure AI Foundry/OpenAI
10. AI Response → Streaming chunks back through chain
11. Frontend → Real-time rendering in UI
```

#### Document Processing Flow
```
1. User Upload → DocumentComponent
2. Component → DocumentService.uploadDocument()
3. API → DocumentsController
4. Controller → DocumentService
5. DocumentService → BlobStorageService (upload file)
6. DocumentService → Document entity saved to DB
7. DocumentService → Hangfire background job queued
8. Hangfire Job → DocumentIntelligenceService
9. DocumentIntelligence → Azure Document Intelligence API
10. Document parsed → Text extraction
11. Text → IEmbeddingGenerator (Azure OpenAI)
12. Embeddings → DocumentPage entities with vector data
13. DocumentPages → SQL Server with vector search
```

---

## 5. Development Tooling & Infrastructure

### Backend Development Tools

#### .NET CLI Commands
```bash
# Build solution
dotnet build

# Run API project
dotnet run --project RR.AI-Chat.Api

# Run tests
dotnet test

# Entity Framework migrations
dotnet ef migrations add {MigrationName} --project RR.AI-Chat.Api
dotnet ef database update --project RR.AI-Chat.Api

# User secrets management
dotnet user-secrets init --project RR.AI-Chat.Api
dotnet user-secrets set "OpenAI:ApiKey" "your-key" --project RR.AI-Chat.Api
```

#### Code Analysis
- **Nullable Reference Types**: Enabled project-wide
- **Implicit Usings**: Reduces boilerplate
- **EditorConfig**: (If present) for consistent formatting

### Frontend Development Tools

#### Angular CLI Commands
```bash
# Install dependencies
npm install

# Development server
npm start  # or ng serve

# Build for production
npm run build  # or ng build

# Run tests
npm test  # or ng test

# Generate component/service
ng generate component components/my-component
ng generate service services/my-service
```

#### Build Configuration
- **Builder**: @angular-devkit/build-angular:application (modern builder)
- **Style Preprocessor**: SCSS
- **Bundle Budgets**: 
  - Initial: 2MB warning, 5MB error
  - Component styles: 10kB warning, 20kB error

#### Code Quality Tools
- **TypeScript strict mode**: Enabled
- **Linters**: (Standard Angular linting if configured)
- **Jasmine + Karma**: Unit testing

### Development Environment

#### Required Software
- **.NET 9.0 SDK**: Latest LTS
- **Node.js 18+**: For Angular development
- **SQL Server**: Express, Developer, or Full edition
- **Visual Studio 2022** or **VS Code**: Primary IDEs

#### Optional Tools
- **Ollama**: For local AI model testing
- **Azure Data Studio**: Database management
- **Postman**: API testing
- **Docker**: (Previously used, now removed from project)

### Deployment Pipeline

#### Backend Deployment
- **Build**: `dotnet publish -c Release`
- **Configuration**: User secrets → Environment variables
- **Database**: Migrations applied automatically on startup
- **Background Jobs**: Hangfire server runs in-process

#### Frontend Deployment
- **Build**: `ng build --configuration production`
- **Output**: `dist/ai-chat-ui/`
- **Static hosting**: Can be served from any static web server
- **Environment config**: `environment.prod.ts` for API URL

---

## 6. Security & Authentication Architecture

### Backend Security

#### Authentication Mechanism
- **Provider**: Azure Active Directory (Azure AD)
- **Protocol**: OAuth 2.0 / OpenID Connect
- **Token Type**: JWT (JSON Web Tokens)
- **Library**: Microsoft.Identity.Web 4.0.1

**Configuration**:
```csharp
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(options => {
        builder.Configuration.Bind("AzureAd", options);
        options.TokenValidationParameters.ValidateAudience = true;
        options.TokenValidationParameters.ValidAudiences = [
            builder.Configuration["AzureAd:ClientId"],
            $"api://{builder.Configuration["AzureAd:ClientId"]}"
        ];
    }, options => builder.Configuration.Bind("AzureAd", options))
    .EnableTokenAcquisitionToCallDownstreamApi(...)
    .AddInMemoryTokenCaches();
```

#### Authorization Pattern
- **Controller-level**: `[Authorize]` attribute on all controllers
- **User Context**: Retrieved via `ITokenService.GetOid()` from JWT claims
- **User Validation**: `UserExistenceMiddleware` ensures authenticated user exists in database

#### API Security
- **CORS**: Restricted to configured origins
- **HTTPS**: HTTPS redirection enabled
- **Secrets Management**: User secrets for development, environment variables for production

### Frontend Security

#### Authentication Library
- **MSAL Angular**: @azure/msal-angular 4.0.20
- **MSAL Browser**: @azure/msal-browser 4.25.0

**Configuration**:
```typescript
{
  auth: {
    clientId: environment.msalConfig.auth.clientId,
    authority: environment.msalConfig.auth.authority,
    redirectUri: '/'
  },
  cache: {
    cacheLocation: BrowserCacheLocation.SessionStorage
  }
}
```

#### Token Management
- **Acquisition**: Silent token refresh with fallback to interactive
- **Storage**: SessionStorage for security
- **Interceptor**: Automatic token attachment to API requests
- **Scopes**: Configured per environment

---

## 7. Database Schema & Data Models

### Entity Framework Core Configuration

#### Entities
1. **User** - Application users (synced with Azure AD)
2. **Session** - Chat sessions with conversation history
3. **AIService** - AI service provider configurations
4. **Model** - Available AI models per service
5. **Document** - Uploaded documents metadata
6. **DocumentPage** - Document pages with vector embeddings

#### Special Configurations

**JSON Serialization** (Conversations):
```csharp
entity.Property(e => e.Conversations)
    .HasColumnType("nvarchar(max)")
    .HasConversion(
        v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
        v => JsonSerializer.Deserialize<List<Conversation>>(v, (JsonSerializerOptions?)null) ?? new()
    );
```

**Vector Embeddings**:
```csharp
modelBuilder.Entity<DocumentPage>()
    .Property(p => p.Embedding)
    .HasColumnType("vector(1536)");
```

#### Delete Behavior
- **Global Setting**: `DeleteBehavior.NoAction` to prevent cascading deletes
- Manual cleanup required for referential integrity

---

## 8. AI Service Integration Architecture

### Microsoft.Extensions.AI Abstraction

The application uses Microsoft's unified AI abstraction layer, allowing seamless switching between AI providers.

#### Registered Chat Clients

**Azure AI Foundry** (Primary):
```csharp
builder.Services.AddKeyedChatClient("azureaifoundry", sp =>
    new AzureOpenAIClient(new Uri(azureAIFoundryUrl), new ApiKeyCredential(azureAIFoundryKey))
        .GetChatClient(azureAIFoundryDefaultModel)
        .AsIChatClient()
)
.UseOpenTelemetry()
.UseFunctionInvocation(null, x => {
    x.AllowConcurrentInvocation = false;
    x.IncludeDetailedErrors = true;
    x.MaximumIterationsPerRequest = 5;
});
```

#### Embedding Generator
```csharp
IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator =
    new AzureOpenAIClient(new Uri(azureAIFoundryUrl), new ApiKeyCredential(azureAIFoundryKey))
        .GetEmbeddingClient(embeddingModel)
        .AsIEmbeddingGenerator();
```

#### Supported AI Providers
1. **Azure AI Foundry** - Azure OpenAI Service
2. **OpenAI** - Direct OpenAI API
3. **Anthropic** - Claude models
4. **Ollama** - Local AI models

#### Model Context Protocol (MCP)
- **Package**: ModelContextProtocol 0.4.0-preview.3
- **Purpose**: Standardized protocol for AI agent interactions
- **Configuration**: McpServers section in appsettings.json

---

## 9. Background Processing Architecture

### Hangfire Configuration

**Purpose**: Document processing, embedding generation, long-running tasks

**Storage**: SQL Server (shared with main database)

**Configuration**:
```csharp
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(connectionString, new SqlServerStorageOptions {
        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
        QueuePollInterval = TimeSpan.Zero,
        UseRecommendedIsolationLevel = true,
        DisableGlobalLocks = true,
    }));

builder.Services.AddHangfireServer(options => {
    options.WorkerCount = Environment.ProcessorCount * 2;
});
```

**Dashboard**: Available at `/hangfire` endpoint

---

## 10. Implementation Blueprint for New Features

### Adding a New API Endpoint

#### 1. Create DTO (RR.AI-Chat.Dto)
```csharp
public record MyRequestDto(string Property1, int Property2);
public record MyResponseDto(Guid Id, string Result);
```

#### 2. Create Service Interface & Implementation (RR.AI-Chat.Service)
```csharp
public interface IMyService
{
    Task<MyResponseDto> ProcessAsync(MyRequestDto request, CancellationToken cancellationToken);
}

public class MyService(
    ILogger<MyService> logger,
    AIChatDbContext ctx) : IMyService
{
    private readonly ILogger _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    private readonly AIChatDbContext _ctx = ctx ?? throw new ArgumentNullException(nameof(ctx));

    public async Task<MyResponseDto> ProcessAsync(MyRequestDto request, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        
        // Implementation
        _logger.LogInformation("Processing request");
        
        // Database operations
        var entity = await _ctx.MyEntities
            .FirstOrDefaultAsync(x => x.Property == request.Property1, cancellationToken);
        
        return new MyResponseDto(Guid.NewGuid(), "Success");
    }
}
```

#### 3. Register Service (Program.cs)
```csharp
builder.Services.AddScoped<IMyService, MyService>();
```

#### 4. Create Controller (RR.AI-Chat.Api/Controllers)
```csharp
[Authorize]
[Route("api/[controller]")]
[ApiController]
public class MyController(IMyService myService) : ControllerBase
{
    private readonly IMyService _myService = myService;

    [HttpPost]
    public async Task<IActionResult> ProcessAsync(MyRequestDto request, CancellationToken cancellationToken)
    {
        var result = await _myService.ProcessAsync(request, cancellationToken);
        return Ok(result);
    }
}
```

### Adding a New Angular Component

#### 1. Generate Component
```bash
ng generate component components/my-component
```

#### 2. Create Service (if needed)
```bash
ng generate service services/my-service
```

#### 3. Implement Service
```typescript
@Injectable({ providedIn: 'root' })
export class MyService {
  constructor(private http: HttpClient) {}

  getData(): Observable<MyResponseDto> {
    return this.http.get<MyResponseDto>(`${environment.apiUrl}my-endpoint`);
  }

  postData(request: MyRequestDto): Observable<MyResponseDto> {
    return this.http.post<MyResponseDto>(`${environment.apiUrl}my-endpoint`, request);
  }
}
```

#### 4. Implement Component
```typescript
@Component({
  selector: 'app-my-component',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './my-component.component.html',
  styleUrl: './my-component.component.scss'
})
export class MyComponent implements OnInit {
  data$ = signal<MyResponseDto | null>(null);

  constructor(private myService: MyService) {}

  ngOnInit(): void {
    this.loadData();
  }

  loadData(): void {
    this.myService.getData().subscribe({
      next: (data) => this.data$.set(data),
      error: (error) => console.error('Error loading data', error)
    });
  }
}
```

### Adding Database Entity & Migration

#### 1. Create Entity (RR.AI-Chat.Entity)
```csharp
public class MyEntity : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public User? User { get; set; }
}
```

#### 2. Add DbSet (AIChatDbContext)
```csharp
public DbSet<MyEntity> MyEntities { get; set; }
```

#### 3. Create Configuration (RR.AI-Chat.Repository/Configurations)
```csharp
public class MyEntityConfiguration : IEntityTypeConfiguration<MyEntity>
{
    public void Configure(EntityTypeBuilder<MyEntity> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).HasMaxLength(200).IsRequired();
        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
```

#### 4. Apply Configuration (OnModelCreating)
```csharp
modelBuilder.ApplyConfiguration(new MyEntityConfiguration());
```

#### 5. Create & Apply Migration
```bash
cd RR.AI-Chat
dotnet ef migrations add AddMyEntity --project RR.AI-Chat.Api
dotnet ef database update --project RR.AI-Chat.Api
```

---

## 11. Testing Approach

### Backend Testing
- **Framework**: xUnit (inferred from .NET best practices)
- **Test Location**: Separate test projects (if present)
- **Coverage**: Unit tests for services, integration tests for controllers

### Frontend Testing
- **Framework**: Jasmine
- **Runner**: Karma
- **Test Location**: `*.spec.ts` files alongside components
- **Command**: `npm test`

---

## 12. Environment Configuration

### Backend Configuration Files

**appsettings.json** (public settings):
```json
{
  "Logging": { "LogLevel": { "Default": "Information" } },
  "AllowedHosts": "*",
  "CorsOrigins": ["http://localhost:4200"],
  "OllamaUrl": "http://localhost:11434/",
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=aichat;Integrated Security=true;TrustServerCertificate=true;"
  }
}
```

**User Secrets** (sensitive settings):
- OpenAI:ApiKey
- AzureAIFoundry:Url
- AzureAIFoundry:ApiKey
- AzureAIFoundry:DefaultModel
- AzureAIFoundry:EmbeddingModel
- Anthropic:ApiKey
- AzureAd:ClientId
- AzureAd:ClientSecret
- AzureAd:TenantId
- AzureStorage:ConnectionString
- DocumentIntelligence:Endpoint
- DocumentIntelligence:ApiKey

### Frontend Configuration Files

**environment.ts** (development):
```typescript
export const environment = {
  production: false,
  apiUrl: 'https://localhost:7045/api/',
  msalConfig: { ... },
  apiConfig: { ... }
};
```

**environment.prod.ts** (production):
```typescript
export const environment = {
  production: true,
  apiUrl: 'https://api.yourdomain.com/api/',
  msalConfig: { ... },
  apiConfig: { ... }
};
```

---

## 13. Code Quality & Best Practices Summary

### .NET Backend

#### Modern C# Features Used
- **Primary Constructors**: Simplified constructor syntax
- **Record Types**: For immutable DTOs
- **Nullable Reference Types**: Enabled project-wide
- **File-scoped Namespaces**: Reduced indentation
- **Pattern Matching**: In switch expressions and conditions
- **Async Streams**: For server-sent events (`IAsyncEnumerable<T>`)
- **Collection Expressions**: `[]` syntax for arrays

#### SOLID Principles
- **Single Responsibility**: Each service has one clear purpose
- **Open/Closed**: Extension methods, middleware pipeline
- **Liskov Substitution**: Interface-based services
- **Interface Segregation**: Focused service interfaces
- **Dependency Inversion**: Constructor injection throughout

#### Error Handling
- Specific exception types (`ArgumentNullException`, `InvalidOperationException`)
- Logging before throwing exceptions
- No silent catch blocks
- Proper null handling

#### Async Best Practices
- All I/O operations async
- CancellationToken throughout
- Proper async streaming
- No sync-over-async

### Angular Frontend

#### TypeScript Best Practices
- **Strict Mode**: Enabled for type safety
- **Interfaces**: For all DTOs
- **Type Safety**: Explicit return types
- **Signals**: Modern reactive state management

#### Component Design
- **Standalone Components**: Latest Angular pattern
- **Single Responsibility**: Focused components
- **OnPush Change Detection**: (If optimized)
- **RxJS**: Proper subscription management

---

## 14. Dependency Upgrade Paths

### .NET Backend

**Current**: .NET 9.0 (Latest)  
**Upgrade Path**: None required (on latest LTS)

**NuGet Package Considerations**:
- Monitor Microsoft.Extensions.AI for stable releases (currently using preview packages)
- Azure.AI.OpenAI beta version - watch for stable release
- Keep Entity Framework Core aligned with .NET version

### Angular Frontend

**Current**: Angular 19.0.0 (Latest)  
**Upgrade Path**: None required (on latest version)

**NPM Package Considerations**:
- Bootstrap 5.3.x is current LTS
- RxJS 7.x is stable
- MSAL packages updated regularly for security

---

## 15. Technology Decision Rationale

### Why .NET 9?
- **Latest LTS**: Long-term support and latest features
- **Performance**: Improved runtime performance
- **AI Integration**: Native Microsoft.Extensions.AI support
- **Modern C#**: C# 13 features for cleaner code
- **Azure Integration**: First-class Azure service support

### Why Angular 19?
- **Modern Framework**: Latest features and performance
- **Standalone Components**: Simplified architecture
- **Signals**: Better reactivity and performance
- **TypeScript Support**: Strong typing and IDE support
- **Ecosystem**: Rich component libraries

### Why SQL Server with Vector Search?
- **Enterprise-Grade**: Reliable, scalable database
- **Vector Support**: Native AI embedding search
- **Entity Framework**: Seamless ORM integration
- **Azure Compatible**: Easy cloud migration path

### Why Microsoft.Extensions.AI?
- **Provider Agnostic**: Switch between AI providers easily
- **Consistent API**: Same interface for all providers
- **Microsoft Supported**: Official Microsoft library
- **Future-Proof**: Aligned with .NET AI strategy

### Why Hangfire?
- **SQL Server Storage**: No additional infrastructure
- **Dashboard**: Built-in monitoring
- **Reliable**: Proven background job processing
- **.NET Native**: First-class .NET integration

---

## 16. Known Constraints & Limitations

### Technology Constraints
- **SQL Server Vector Search**: Requires SQL Server 2022+ or Azure SQL
- **Azure AI Services**: Require active Azure subscriptions
- **MSAL Authentication**: Requires Azure AD configuration
- **Aspose Libraries**: Commercial licenses may be required

### Architectural Constraints
- **No Repository Pattern**: DbContext used directly (intentional for simplicity)
- **In-Process Hangfire**: Background jobs run in API process
- **Session Storage**: MSAL tokens in SessionStorage (cleared on browser close)
- **Automatic Migrations**: Database migrated on app startup

---

## 17. Getting Started Checklist for New Developers

### Prerequisites Installation
- [ ] Install .NET 9.0 SDK
- [ ] Install Node.js 18+ (LTS recommended)
- [ ] Install SQL Server (Express/Developer/Full)
- [ ] Install Visual Studio 2022 or VS Code
- [ ] Install Angular CLI (`npm install -g @angular/cli`)

### Repository Setup
- [ ] Clone repository
- [ ] Restore .NET packages (`dotnet restore`)
- [ ] Restore npm packages (`npm install` in ai-chat-ui/)
- [ ] Create database or update connection string

### Configuration
- [ ] Initialize user secrets (`dotnet user-secrets init --project RR.AI-Chat.Api`)
- [ ] Set Azure AD configuration
- [ ] Set at least one AI service API key (OpenAI/Azure/Anthropic/Ollama)
- [ ] Configure frontend environment files

### First Run
- [ ] Run database migrations (`dotnet ef database update` or let app auto-migrate)
- [ ] Start backend API (`dotnet run --project RR.AI-Chat.Api`)
- [ ] Start frontend (`npm start` in ai-chat-ui/)
- [ ] Access Swagger at https://localhost:7045/swagger
- [ ] Access frontend at http://localhost:4200

### Verify Setup
- [ ] Backend API responds to health check
- [ ] Frontend loads and shows login
- [ ] Can authenticate via Azure AD
- [ ] Can create a new chat session
- [ ] Can send a message to AI
- [ ] Swagger documentation accessible

---

## 18. Additional Resources

### Documentation
- [.NET 9 Documentation](https://docs.microsoft.com/dotnet/core/whats-new/dotnet-9)
- [Angular 19 Documentation](https://angular.dev/)
- [Entity Framework Core](https://docs.microsoft.com/ef/core/)
- [Microsoft.Extensions.AI](https://learn.microsoft.com/dotnet/ai/)
- [Azure AD Authentication](https://docs.microsoft.com/azure/active-directory/)
- [MSAL Angular](https://github.com/AzureAD/microsoft-authentication-library-for-js)

### Project Documentation
- `README.md` - Quick start and overview
- `QUICK_START.md` - Fastest setup path
- `DEVELOPMENT.md` - Development guidelines
- Swagger UI - API documentation when running

---

## Appendix A: Complete Dependency Matrix

### Backend Dependencies (.NET 9.0)

| Package | Version | License | Purpose | Project |
|---------|---------|---------|---------|---------|
| Microsoft.Extensions.AI | 9.10.1 | MIT | AI abstraction layer | Api, Entity |
| Microsoft.Extensions.AI.Abstractions | 9.10.1 | MIT | AI interfaces | Api, Service |
| Microsoft.Extensions.AI.OpenAI | 9.7.1-preview | MIT | OpenAI integration | Api |
| Azure.AI.OpenAI | 2.5.0-beta.1 | MIT | Azure OpenAI client | Api |
| OpenAI | 2.5.0 | MIT | OpenAI SDK | Api |
| Anthropic.SDK | 5.8.0 | MIT | Claude AI integration | Api |
| OllamaSharp | 5.4.8 | MIT | Ollama integration | Api |
| ModelContextProtocol | 0.4.0-preview.3 | MIT | MCP protocol | Api, Service |
| Microsoft.EntityFrameworkCore | 9.0.10 | MIT | ORM framework | Api, Repository |
| Microsoft.EntityFrameworkCore.SqlServer | 9.0.10 | MIT | SQL Server provider | Repository |
| EFCore.SqlServer.VectorSearch | 9.0.0 | MIT | Vector search extension | Api, Repository, Service |
| Microsoft.Identity.Web | 4.0.1 | MIT | Azure AD auth | Api, Service |
| Microsoft.Graph | 5.95.0 | MIT | Microsoft Graph API | Api, Service |
| Azure.Storage.Blobs | 12.26.0 | MIT | Blob storage | Service |
| Azure.AI.DocumentIntelligence | 1.0.0 | MIT | Document parsing | Api, Service |
| Hangfire | 1.8.21 | LGPL-3.0 | Background jobs | Api |
| Hangfire.Core | 1.8.21 | LGPL-3.0 | Background jobs core | Service |
| Swashbuckle.AspNetCore | 9.0.6 | MIT | Swagger/OpenAPI | Api, Service |
| Aspose.PDF | 25.10.0 | Commercial | PDF processing | Service |
| Aspose.Words | 25.10.0 | Commercial | Word processing | Service |
| PdfPig | 0.1.11 | Apache-2.0 | PDF parsing | Service |
| Markdig | 0.43.0 | BSD-2-Clause | Markdown processing | Service |
| ReverseMarkdown | 4.7.1 | MIT | HTML to Markdown | Service |
| Microsoft.ML.Tokenizers | 2.0.0-preview | MIT | Text tokenization | Service |

### Frontend Dependencies (Angular 19)

| Package | Version | License | Purpose | Type |
|---------|---------|---------|---------|------|
| @angular/core | 19.0.0 | MIT | Core framework | Runtime |
| @angular/common | 19.0.0 | MIT | Common utilities | Runtime |
| @angular/router | 19.0.0 | MIT | Routing | Runtime |
| @angular/forms | 19.0.0 | MIT | Forms | Runtime |
| @angular/animations | 19.0.0 | MIT | Animations | Runtime |
| @azure/msal-angular | 4.0.20 | MIT | Azure AD auth | Runtime |
| @azure/msal-browser | 4.25.0 | MIT | MSAL browser | Runtime |
| bootstrap | 5.3.8 | MIT | CSS framework | Runtime |
| bootstrap-icons | 1.13.1 | MIT | Icons | Runtime |
| highlight.js | 11.11.1 | BSD-3-Clause | Code highlighting | Runtime |
| markdown-it | 14.1.0 | MIT | Markdown parsing | Runtime |
| markdown-it-highlightjs | 4.2.0 | ISC | Markdown code highlight | Runtime |
| rxjs | 7.8.0 | Apache-2.0 | Reactive programming | Runtime |
| typescript | 5.6.2 | Apache-2.0 | TypeScript compiler | Dev |
| @angular/cli | 19.0.6 | MIT | Angular CLI | Dev |
| jasmine-core | 5.4.0 | MIT | Testing framework | Dev |
| karma | 6.4.0 | MIT | Test runner | Dev |

---

## Appendix B: File Structure Reference

### Backend File Structure
```
RR.AI-Chat/
├── RR.AI-Chat.sln
├── RR.AI-Chat.Api/
│   ├── Controllers/
│   │   ├── ChatsController.cs
│   │   ├── SessionsController.cs
│   │   ├── DocumentsController.cs
│   │   ├── ModelsController.cs
│   │   ├── McpsController.cs
│   │   └── UsersController.cs
│   ├── Program.cs
│   ├── appsettings.json
│   ├── RR.AI-Chat.Api.csproj
│   └── launchSettings.json
├── RR.AI-Chat.Service/
│   ├── ChatService.cs
│   ├── SessionService.cs
│   ├── DocumentService.cs
│   ├── ModelService.cs
│   ├── GraphService.cs
│   ├── BlobStorageService.cs
│   ├── DocumentIntelligenceService.cs
│   ├── McpServerService.cs
│   ├── PdfService.cs
│   ├── WordService.cs
│   ├── HtmlService.cs
│   ├── MarkdownService.cs
│   ├── TokenService.cs
│   ├── SessionLockService.cs
│   ├── DocumentToolService.cs
│   ├── Middleware/
│   │   └── UserExistenceMiddleware.cs
│   ├── Settings/
│   │   └── McpServerSettings.cs
│   └── RR.AI-Chat.Service.csproj
├── RR.AI-Chat.Repository/
│   ├── AIChatDbContext.cs
│   ├── AIChatDbContextFactory.cs
│   ├── Configurations/
│   │   ├── AIServiceConfiguration.cs
│   │   └── ModelConfiguration.cs
│   ├── Migrations/
│   └── RR.AI-Chat.Repository.csproj
├── RR.AI-Chat.Entity/
│   ├── BaseEntity.cs
│   ├── User.cs
│   ├── Session.cs
│   ├── AIService.cs
│   ├── Model.cs
│   ├── Document.cs
│   ├── DocumentPage.cs
│   └── RR.AI-Chat.Entity.csproj
└── RR.AI-Chat.Dto/
    ├── SessionDto.cs
    ├── ModelDto.cs
    ├── DocumentDto.cs
    ├── ChatStreamRequestDto.cs
    ├── ChatCompletionRequestDto.cs
    ├── ChatCompletionDto.cs
    ├── SessionMessageDto.cs
    ├── ApiResponseDto.cs
    ├── PaginatedResponseDto.cs
    ├── McpDto.cs
    ├── FileDto.cs
    ├── JobDto.cs
    ├── JobStatusDto.cs
    ├── PageEmbeddingDto.cs
    ├── SearchDocumentRequestDto.cs
    ├── DocumentExtractorDto.cs
    ├── Enums/
    │   ├── ChatRoleType.cs
    │   ├── AIServiceType.cs
    │   ├── DocumentFormats.cs
    │   ├── JobStatus.cs
    │   └── JobName.cs
    ├── Actions/
    │   ├── Session/SessionActions.cs
    │   ├── Graph/GraphActions.cs
    │   └── User/UserActions.cs
    └── RR.AI-Chat.Dto.csproj
```

### Frontend File Structure
```
ai-chat-ui/
├── src/
│   ├── app/
│   │   ├── components/
│   │   │   ├── chat/
│   │   │   ├── message/
│   │   │   ├── session-list/
│   │   │   └── document-upload/
│   │   ├── pages/
│   │   │   ├── chat-page/
│   │   │   └── session-page/
│   │   ├── services/
│   │   │   ├── chat.service.ts
│   │   │   ├── session.service.ts
│   │   │   ├── document.service.ts
│   │   │   ├── model.service.ts
│   │   │   └── mcp.service.ts
│   │   ├── dtos/
│   │   │   ├── SessionDto.ts
│   │   │   ├── ModelDto.ts
│   │   │   ├── ChatStreamRequestDto.ts
│   │   │   └── DocumentDto.ts
│   │   ├── shared/
│   │   │   ├── guards/
│   │   │   └── interceptors/
│   │   ├── store/
│   │   │   └── store.service.ts
│   │   ├── app.component.ts
│   │   ├── app.component.html
│   │   ├── app.component.scss
│   │   ├── app.config.ts
│   │   └── app.routes.ts
│   ├── environments/
│   │   ├── environment.ts
│   │   └── environment.prod.ts
│   ├── assets/
│   ├── index.html
│   ├── main.ts
│   └── styles.scss
├── package.json
├── tsconfig.json
├── angular.json
└── README.md
```

---

**End of Technology Stack Blueprint**

*This blueprint serves as the comprehensive reference for understanding, maintaining, and extending the AI Chat Application. It should be updated as the technology stack evolves.*
