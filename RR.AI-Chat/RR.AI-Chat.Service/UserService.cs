using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RR.AI_Chat.Dto.Actions.Graph;
using RR.AI_Chat.Dto.Actions.User;
using RR.AI_Chat.Entity;
using RR.AI_Chat.Repository;

namespace RR.AI_Chat.Service
{
    public interface IUserService
    {
        /// <summary>
        /// Creates a new user via the graph service and persists it to the local database.
        /// </summary>
        /// <param name="request">The user details to create.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is null.</exception>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task CreateUserAsync(CreateUserActionDto request, CancellationToken cancellationToken);

        /// <summary>
        /// Determines whether a user with the specified identifier exists in the database.
        /// </summary>
        /// <param name="userId">The unique identifier of the user.</param>
        /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
        /// <returns><c>true</c> if the user exists; otherwise, <c>false</c>.</returns>
        Task<bool> IsUserInDatabaseAsync(Guid userId, CancellationToken cancellationToken);
    }   

    public class UserService(ILogger<UserService> logger,
        IGraphService graphService,
        AIChatDbContext ctx) : IUserService
    {
        private readonly ILogger<UserService> _logger = logger;
        private readonly IGraphService _graphService = graphService;
        private readonly AIChatDbContext _ctx = ctx;

        /// <inheritdoc />
        public async Task CreateUserAsync(CreateUserActionDto request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request); 

            var graphRequest = new CreateGraphUserActionDto
            {
                FirstName = request.FirstName,
                LastName = request.LastName,
                Email = request.Email
            };

            var user = await _graphService.CreateUserAsync(graphRequest, cancellationToken);

            var date = DateTime.UtcNow;
            var newUser = new User
            {
                Id = Guid.Parse(user.Id!),
                FirstName = user.GivenName!,
                LastName = user.Surname!,
                Email = request.Email,
                DateCreated = date,
                DateModified = date,
            };
            await _ctx.Users.AddAsync(newUser, cancellationToken);
            await _ctx.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created new user with ID {UserId} and email {Email}", newUser.Id, newUser.Email);
        }

        /// <inheritdoc />
        public async Task<bool> IsUserInDatabaseAsync(Guid userId, CancellationToken cancellationToken)
        {
            return await _ctx.Users.Where(x => x.Id == userId).AnyAsync(cancellationToken);
        }
    }
}
