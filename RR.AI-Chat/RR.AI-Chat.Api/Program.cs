using Azure.AI.OpenAI;
using Azure.Identity;
using Azure.Storage.Blobs;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using RR.AI_Chat.Repository;
using RR.AI_Chat.Service;
using RR.AI_Chat.Service.Middleware;
using System.ClientModel;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(options =>
    {
        builder.Configuration.Bind("AzureAd", options);

        // Explicitly validate audience to ensure token is for this API
        options.TokenValidationParameters.ValidateAudience = true;
        options.TokenValidationParameters.ValidAudiences =
        [
            builder.Configuration["AzureAd:ClientId"],
            $"api://{builder.Configuration["AzureAd:ClientId"]}"
        ];
    },
    options => builder.Configuration.Bind("AzureAd", options));
builder.Services.AddControllers();

builder.Services.AddHttpContextAccessor();

builder.Services.AddDbContext<AIChatDbContext>(options =>
  options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), o => o.UseVectorSearch()));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();

// Add CORS
var corsOrigins = builder.Configuration.GetSection("CorsOrigins").Get<string[]>();
corsOrigins ??= [];
builder.Services.AddCors(builder => builder.AddPolicy("AllowSpecificOrigins", policy =>
{
    policy.WithOrigins(corsOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
}));


// 1) Azure AI Foundry under key "azureaifoundry"
var azureAIFoundryUrl = builder.Configuration.GetValue<string>("AzureAIFoundry:Url") ?? string.Empty;
var azureAIFoundryKey = builder.Configuration.GetValue<string>("AzureAIFoundry:ApiKey") ?? string.Empty;
var azureAIFoundryDefaultModel = builder.Configuration.GetValue<string>("AzureAIFoundry:DefaultModel") ?? string.Empty;
builder.Services.AddKeyedChatClient(
        "azureaifoundry",
        sp => new AzureOpenAIClient(new Uri(azureAIFoundryUrl), new ApiKeyCredential(azureAIFoundryKey))
                  .GetChatClient(azureAIFoundryDefaultModel)
                  .AsIChatClient()
    )
    .UseOpenTelemetry()
    .UseFunctionInvocation(null, x =>
    {
        x.AllowConcurrentInvocation = false;
        x.IncludeDetailedErrors = true;
        x.MaximumIterationsPerRequest = 5;
        x.MaximumConsecutiveErrorsPerRequest = 5;
    });

// AI Embedding Generators
var embeddingModel = builder.Configuration.GetValue<string>("AzureAIFoundry:EmbeddingModel") ?? string.Empty;
IEmbeddingGenerator<string, Embedding<float>> ollamaGenerator =
    new AzureOpenAIClient(new Uri(azureAIFoundryUrl), new ApiKeyCredential(azureAIFoundryKey))
        .GetEmbeddingClient(embeddingModel)
        .AsIEmbeddingGenerator();
builder.Services.AddEmbeddingGenerator(ollamaGenerator);

// Add Hangfire services
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection"), new SqlServerStorageOptions
    {
        CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
        SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
        QueuePollInterval = TimeSpan.Zero,
        UseRecommendedIsolationLevel = true,
        DisableGlobalLocks = true,
    }));

// Add the processing server as IHostedService
builder.Services.AddHangfireServer(options =>
{
    options.WorkerCount = Environment.ProcessorCount * 2;
});

// Register IStorageConnection for dependency injection
builder.Services.AddScoped(provider => JobStorage.Current.GetConnection());

// Add Microsoft Graph Service
builder.Services.AddSingleton(sp =>
{
    var tenantId = builder.Configuration["AzureAd:TenantId"];
    var clientId = builder.Configuration["AzureAd:ClientId"];
    var clientSecret = builder.Configuration["AzureAd:ClientSecret"];

    var clientSecretCredential = new ClientSecretCredential(
        tenantId,
        clientId,
        clientSecret
    );

    return new GraphServiceClient(clientSecretCredential);
});

// Azure Storage
builder.Services.AddSingleton(x =>
{
    var connectionString = builder.Configuration["AzureStorage:ConnectionString"];
    return new BlobServiceClient(connectionString);
});


// Register the singleton lock service
builder.Services.AddSingleton<ISessionLockService, SessionLockService>();
builder.Services.AddSingleton<ITokenService, TokenService>();
builder.Services.AddSingleton<IGraphService, GraphService>();
builder.Services.AddSingleton<IBlobStorageService, BlobStorageService>();

// Keep other services as Scoped
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IDocumentToolService, DocumentToolService>();
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<IModelService, ModelService>();
builder.Services.AddScoped<IMcpServerService, McpServerService>();
builder.Services.AddScoped<IUserService, UserService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Add Hangfire Dashboard
app.UseHangfireDashboard("/hangfire", new DashboardOptions
{
    //Authorization = new[] { new HangfireAuthorizationFilter() }
});

app.UseHttpsRedirection();

app.UseCors("AllowSpecificOrigins");

app.UseAuthentication();

app.UseMiddleware<UserExistenceMiddleware>();

app.UseAuthorization();

app.MapControllers();

app.Run();
