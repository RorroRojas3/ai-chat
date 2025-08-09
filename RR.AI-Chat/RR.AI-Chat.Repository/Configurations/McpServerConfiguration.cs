using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RR.AI_Chat.Entity;

namespace RR.AI_Chat.Repository.Configurations
{
    public class McpServerConfiguration : IEntityTypeConfiguration<McpServer>
    {
        public void Configure(EntityTypeBuilder<McpServer> builder)
        {
            var dateTime = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            builder.HasData(
                
                new McpServer
                {
                    Id = new("0a515abd-7d7d-48f5-9037-531745843548"),
                    Name = "Test MCP Server",
                    Command = "dotnet",
                    Arguments = ["run", "--project", "C:\\Users\\Rorro\\source\\repos\\RR.MCPServer\\RR.MCPServer\\RR.MCPServer.csproj", "--configuration", "Release"],
                    WorkingDirectory = "C:\\Users\\Rorro\\source\\repos\\RR.MCPServer\\RR.MCPServer",
                    DateCreated = dateTime
                }
            );
        }
    }
}
