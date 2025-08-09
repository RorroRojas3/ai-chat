using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;
using RR.AI_Chat.Repository;

namespace RR.AI_Chat.Service
{
    /// <summary>
    /// Defines the contract for MCP (Model Context Protocol) server operations.
    /// </summary>
    public interface IMcpServerService
    {
        /// <summary>
        /// Retrieves all available tools from active MCP servers.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>A list of MCP client tools from all active servers.</returns>
        Task<IList<McpClientTool>> GetToolsAsync(CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Service for managing MCP (Model Context Protocol) server operations and tool retrieval.
    /// </summary>
    /// <param name="logger">Logger instance for recording service operations and errors.</param>
    /// <param name="ctx">Database context for accessing MCP server configuration data.</param>
    public class McpServerService(ILogger<McpServerService> logger, AIChatDbContext ctx) : IMcpServerService
    {
        private readonly ILogger<McpServerService> _logger = logger;
        private readonly AIChatDbContext _ctx = ctx;

        /// <summary>
        /// Retrieves all available tools from active MCP servers by connecting to each server
        /// and querying their available tools.
        /// </summary>
        /// <param name="cancellationToken">A cancellation token to cancel the operation.</param>
        /// <returns>A list of MCP client tools aggregated from all active servers.</returns>
        /// <exception cref="OperationCanceledException">Thrown when the operation is cancelled via the cancellation token.</exception>
        public async Task<IList<McpClientTool>> GetToolsAsync(CancellationToken cancellationToken = default)
        {
            var mcpServers = await _ctx.McpServers.AsNoTracking()
                                .Where(x => !x.DateDeactivated.HasValue)
                                .ToListAsync();
            if (mcpServers == null || mcpServers.Count < 0)
            {
                _logger.LogWarning("No MCP servers found in the database.");
                return [];
            }

            List<McpClientTool> tools = [];
            foreach (var server in mcpServers)
            {
                var clientTransport = new StdioClientTransport(new StdioClientTransportOptions
                {
                    Name = server.Name,
                    Command = server.Command,
                    Arguments = server.Arguments,
                    WorkingDirectory = server.WorkingDirectory,
                });

                var mcpClient = await McpClientFactory.CreateAsync(clientTransport, null, null, cancellationToken)
                                .ConfigureAwait(false);
                var mcpTools = await mcpClient.ListToolsAsync(null, cancellationToken);
                tools.AddRange(mcpTools);
            }

            return tools;
        }
    }
}
