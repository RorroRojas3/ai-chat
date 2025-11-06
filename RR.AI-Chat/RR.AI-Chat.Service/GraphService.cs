using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using RR.AI_Chat.Dto.Actions.Graph;

namespace RR.AI_Chat.Service
{
    public interface IGraphService
    {
        /// <summary>
        /// Creates a new user in Microsoft Entra ID (Azure AD) via Microsoft Graph.
        /// </summary>
        /// <param name="request">
        /// The user details to create, including first name, last name, and email address. Must not be null.
        /// </param>
        /// <param name="cancellationToken">A token to cancel the asynchronous operation.</param>
        /// <returns>The created Microsoft Graph user resource.</returns>
        /// <exception cref="System.ArgumentNullException">Thrown if <paramref name="request"/> is null.</exception>
        /// <exception cref="System.OperationCanceledException">Thrown if the operation is canceled.</exception>
        /// <exception cref="System.Exception">Thrown if the Microsoft Graph API call fails.</exception>
        Task<User> CreateUserAsync(CreateGraphUserActionDto request, CancellationToken cancellationToken);
    }   

    public class GraphService(ILogger<GraphService> logger, GraphServiceClient graphServiceClient) : IGraphService
    {
        private readonly ILogger<GraphService> _logger = logger;
        private readonly GraphServiceClient _graphClient = graphServiceClient;

        /// <inheritdoc />
        public async Task<User> CreateUserAsync(CreateGraphUserActionDto request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            var sanitizedEmail = request.Email.Replace("\r", "").Replace("\n", "");
            _logger.LogInformation("Creating user in Microsoft Graph: {Email}", sanitizedEmail);

            var user = new User
            {
                AccountEnabled = true,
                GivenName = request.FirstName,
                Surname = request.LastName,
                DisplayName = $"{request.FirstName} {request.LastName}",
                MailNickname = request.Email.Split('@')[0],
                UserPrincipalName = $"{request.Email}",
                PasswordProfile = new PasswordProfile
                {
                    ForceChangePasswordNextSignIn = false,
                    Password = "TempPassword123!"
                }
            };

            user = await _graphClient.Users.PostAsync(user, null, cancellationToken);

            _logger.LogInformation("User created in Microsoft Graph with ID: {UserId}", user!.Id);

            return user;
        }
    }
}
