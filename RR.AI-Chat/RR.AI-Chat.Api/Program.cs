using Azure.AI.OpenAI;
using Azure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.AI;
using OllamaSharp;
using OpenAI;
using RR.AI_Chat.Repository;
using RR.AI_Chat.Service;

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

/// AI Chat clients
// 1) Ollama under key "ollama"
var ollamaUrl = builder.Configuration.GetValue<string>("OllamaUrl") ?? "http://localhost:11434/";
builder.Services.AddKeyedChatClient(
        "ollama",
        sp => new OllamaApiClient(new Uri(ollamaUrl), "llama3.2:latest")
);

// 2) OpenAI under key "openai"
var openAiKey = builder.Configuration.GetValue<string>("OpenAI:ApiKey") ?? string.Empty;
builder.Services.AddKeyedChatClient(
        "openai",
        sp => new OpenAIClient(openAiKey)
                  .GetChatClient("gpt-4.1-nano")
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

// 3) Azure OpenAI under key "azureopenai"
var azureOpenAiUrl = builder.Configuration.GetValue<string>("AzureOpenAI:Url") ?? string.Empty;
var azureOpenAiKey = builder.Configuration.GetValue<string>("AzureOpenAI:ApiKey") ?? string.Empty;
builder.Services.AddKeyedChatClient(
        "azureopenai",
        sp => new AzureOpenAIClient(new Uri(azureOpenAiUrl), new System.ClientModel.ApiKeyCredential(azureOpenAiKey))
                  .GetChatClient("gpt-4.1-nano")
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
IEmbeddingGenerator<string, Embedding<float>> ollamaGenerator =
    new OllamaApiClient(new Uri("http://localhost:11434/"), "nomic-embed-text");
builder.Services.AddEmbeddingGenerator(ollamaGenerator);

builder.Services.AddTransient<IChatService, ChatService>();
builder.Services.AddSingleton<DocumentStore>();
builder.Services.AddTransient<IDocumentService, DocumentService>();
builder.Services.AddTransient<IDocumentToolService, DocumentToolService>();
builder.Services.AddTransient<ISessionService, SessionService>();
builder.Services.AddTransient<IModelService, ModelService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("AllowSpecificOrigins");

app.UseAuthorization();

app.MapControllers();

app.Run();
