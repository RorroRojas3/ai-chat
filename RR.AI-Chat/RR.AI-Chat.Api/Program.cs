using Microsoft.Extensions.AI;
using RR.AI_Chat.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS
var corsOrigins = builder.Configuration.GetSection("CorsOrigins").Get<string[]>();
corsOrigins = corsOrigins ?? [];
builder.Services.AddCors(builder => builder.AddPolicy("AllowSpecificOrigins", policy =>
{
    policy.WithOrigins(corsOrigins).AllowAnyHeader().AllowAnyMethod();
}));

/// AI Chat clients
var ollamaUrl = builder.Configuration.GetValue<string>("OllamaUrl");
builder.Services.AddChatClient(new OllamaChatClient(new Uri(ollamaUrl ?? "http://localhost:11434/"), "llama3.2"));

// AI Embedding Generators
IEmbeddingGenerator<string, Embedding<float>> ollamaGenerator =
    new OllamaEmbeddingGenerator(new Uri("http://localhost:11434/"), "llama3.2");
builder.Services.AddEmbeddingGenerator(ollamaGenerator);

builder.Services.AddTransient<IChatService, ChatService>();
builder.Services.AddSingleton<ChatStore>();

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
