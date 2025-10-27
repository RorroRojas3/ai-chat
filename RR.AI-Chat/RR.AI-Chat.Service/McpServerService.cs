using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Identity.Web;
using ModelContextProtocol.Client;
using RR.AI_Chat.Service.Settings;
using System.Net.Http.Headers;

namespace RR.AI_Chat.Service
{
    /// <summary>
    /// Defines the contract for MCP (Model Context Protocol) server operations.
    /// </summary>
    public interface IMcpServerService
    {
        Task<McpClient> CreateClientAsync(string name, CancellationToken cancellationToken);

        Task<IList<McpClientTool>> GetToolsFromServerAsync(McpClient mcpClient, CancellationToken cancellationToken);
    }

    /// <summary>
    /// Service for managing MCP (Model Context Protocol) server operations and tool retrieval.
    /// </summary>
    /// <param name="logger">Logger instance for recording service operations and errors.</param>
    /// <param name="ctx">Database context for accessing MCP server configuration data.</param>
    public class McpServerService(ILogger<McpServerService> logger,
        ITokenAcquisition tokenAcquisition,
        IOptions<List<McpServerSettings>> mcpServerSettings,
        IHttpClientFactory httpClientFactory) : IMcpServerService
    {
        private readonly ILogger<McpServerService> _logger = logger;
        private readonly ITokenAcquisition _tokenAcquisition = tokenAcquisition;
        private readonly IHttpClientFactory _httpClientFactory = httpClientFactory;
        private readonly List<McpServerSettings> _mcpServerSettings = mcpServerSettings.Value;

        public async Task<McpClient> CreateClientAsync(string name, CancellationToken cancellationToken)
        {
            ArgumentException.ThrowIfNullOrEmpty(name);

            var mcpServer = _mcpServerSettings.FirstOrDefault(s => s.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
            if (mcpServer == null)
            {
                _logger.LogError("MCP server with name {name} not found in configuration.", name);
                throw new InvalidOperationException($"MCP server with name {name} not found in configuration.");
            }

            var scopes = new[] { mcpServer.Scope };
            var accessToken = await _tokenAcquisition.GetAccessTokenForUserAsync(scopes);

            var httpClient = _httpClientFactory.CreateClient();
            httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", accessToken);
            httpClient.BaseAddress = mcpServer.Uri;

            var transport = new HttpClientTransport(new()
            {
                Endpoint = mcpServer.Uri,
                Name = mcpServer.Name,
            }, httpClient);

            var mcpClient = await McpClient.CreateAsync(transport, null, null, cancellationToken).ConfigureAwait(false);

            return mcpClient;
        }

        public async Task<IList<McpClientTool>> GetToolsFromServerAsync(McpClient mcpClient, CancellationToken cancellationToken)
        {
            var tools = await mcpClient.ListToolsAsync(null, cancellationToken);
            return tools;
        }
    }
}
