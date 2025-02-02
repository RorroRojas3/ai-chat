using Microsoft.Extensions.AI;
using RR.AI_Chat.Service;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

/// AI Chat clients
builder.Services.AddChatClient(new OllamaChatClient(new Uri("http://localhost:11434/"), "llama3.2"));

// AI Embedding Generators
IEmbeddingGenerator<string, Embedding<float>> ollamaGenerator =
    new OllamaEmbeddingGenerator(new Uri("http://localhost:11434/"), "llama3.2");
builder.Services.AddEmbeddingGenerator(ollamaGenerator);

builder.Services.AddTransient<IChatService, ChatService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
