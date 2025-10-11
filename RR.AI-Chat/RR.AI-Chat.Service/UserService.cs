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
        Task CreateUserAsync(CreateUserActionDto request, CancellationToken cancellationToken);

        Task<bool> IsUserInDatabaseAsync(Guid userId, CancellationToken cancellationToken);
    }   

    public class UserService(ILogger<UserService> logger,
        IGraphService graphService,
        AIChatDbContext ctx) : IUserService
    {
        private readonly ILogger<UserService> _logger = logger;
        private readonly IGraphService _graphService = graphService;
        private readonly AIChatDbContext _ctx = ctx;

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
        }

        public async Task<bool> IsUserInDatabaseAsync(Guid userId, CancellationToken cancellationToken)
        {
            return await _ctx.Users.Where(x => x.Id == userId).AnyAsync(cancellationToken);
        }
    }
}
