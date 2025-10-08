using Azure.AI.OpenAI;
using Hangfire;
using Hangfire.SqlServer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using RR.AI_Chat.Repository;
using RR.AI_Chat.Service;
using System.ClientModel;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddHttpContextAccessor();

builder.Services.AddDbContext<AIChatDbContext>(options =>
  options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"), o => o.UseVectorSearch()));

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS
var corsOrigins = builder.Configuration.GetSection("CorsOrigins").Get<string[]>();
corsOrigins ??= [];
builder.Services.AddCors(builder => builder.AddPolicy("AllowSpecificOrigins", policy =>
{
    policy.WithOrigins(corsOrigins).AllowAnyHeader().AllowAnyMethod();
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


// Register the singleton lock service
builder.Services.AddSingleton<ISessionLockService, SessionLockService>();

// Keep other services as Scoped
builder.Services.AddScoped<IChatService, ChatService>();
builder.Services.AddScoped<IDocumentService, DocumentService>();
builder.Services.AddScoped<IDocumentToolService, DocumentToolService>();
builder.Services.AddScoped<ISessionService, SessionService>();
builder.Services.AddScoped<IModelService, ModelService>();
builder.Services.AddScoped<IMcpServerService, McpServerService>();

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

app.UseAuthorization();

app.MapControllers();

app.Run();
