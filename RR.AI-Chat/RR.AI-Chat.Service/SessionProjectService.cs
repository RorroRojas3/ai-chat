using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RR.AI_Chat.Dto;
using RR.AI_Chat.Dto.Actions.Session;
using RR.AI_Chat.Entity;
using RR.AI_Chat.Repository;

namespace RR.AI_Chat.Service
{
    public interface ISessionProjectService
    {
        /// <summary>
        /// Searches the current user's active session projects using an optional name filter and returns a paginated result.
        /// </summary>
        /// <param name="filter">
        /// An optional substring to match against project <c>Name</c> using a SQL LIKE pattern. Pass <see langword="null"/> or whitespace to return all active projects.
        /// </param>
        /// <param name="skip">The number of items to skip before returning results (for pagination).</param>
        /// <param name="take">The maximum number of items to return (page size).</param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>
        /// A <see cref="PaginatedResponseDto{T}"/> of <see cref="SessionProjectDto"/> containing the page of items and paging metadata.
        /// </returns>
        /// <remarks>
        /// Results are filtered to the current user and to projects that are not deactivated, ordered by <c>DateCreated</c> descending.
        /// The <c>CurrentPage</c> is computed as <c>(skip / take) + 1</c>.
        /// </remarks>
        /// <exception cref="InvalidOperationException">Thrown when the current user's OID is not available.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="skip"/> or <paramref name="take"/> is negative.</exception>
        /// <exception cref="OperationCanceledException">Thrown if the operation is canceled.</exception>
        Task<PaginatedResponseDto<SessionProjectDto>> SearchSessionProjectsAsync(string? filter, int skip = 0, int take = 10, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new session project for the currently authenticated user.
        /// </summary>
        /// <param name="request">
        /// The request containing the target session identifier along with the project's <c>Name</c> and <c>Instructions</c>.
        /// </param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>
        /// A <see cref="SessionProjectDto"/> representing the newly created project.
        /// </returns>
        /// <remarks>
        /// The request is validated using FluentValidation. The current user's Object ID (OID) is resolved from the token service.
        /// The project is persisted using the application's DbContext with UTC timestamps for creation and modification.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is <see langword="null"/>.</exception>
        /// <exception cref="ValidationException">Thrown when the <paramref name="request"/> fails validation.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the current user's OID is not available.</exception>
        /// <exception cref="DbUpdateException">Thrown when persisting the project fails.</exception>
        Task<SessionProjectDto> CreateProjectAsync(UpsertSessionProjectActionDto request, CancellationToken cancellationToken);

        /// <summary>
        /// Updates an existing session project for the currently authenticated user.
        /// </summary>
        /// <param name="request">
        /// The request containing the project identifier to update along with the new values for <c>Name</c>, <c>Description</c>, and <c>Instructions</c>.
        /// </param>
        /// <param name="cancellationToken">A token to observe while waiting for the task to complete.</param>
        /// <returns>
        /// A <see cref="SessionProjectDto"/> representing the updated project.
        /// </returns>
        /// <remarks>
        /// The request is validated using FluentValidation. The current user's Object ID (OID) is resolved from the token service.
        /// Only active (non-deactivated) projects belonging to the current user can be updated.
        /// The <c>DateModified</c> timestamp is updated to the current UTC time upon successful update.
        /// </remarks>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="request"/> is <see langword="null"/>.</exception>
        /// <exception cref="ValidationException">Thrown when the <paramref name="request"/> fails validation.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the current user's OID is not available or when the session project is not found or already deactivated.</exception>
        /// <exception cref="DbUpdateException">Thrown when persisting the updated project fails.</exception>
        Task<SessionProjectDto> UpdateSessionProjectAsync(UpsertSessionProjectActionDto request, CancellationToken cancellationToken);
    }

    public class SessionProjectService(ILogger<SessionProjectService> logger,
        ITokenService tokenService,
        IValidator<UpsertSessionProjectActionDto> upsertSessionProjectValidator,
        AIChatDbContext ctx) : ISessionProjectService
    {
        private readonly ILogger<SessionProjectService> _logger = logger;
        private readonly ITokenService _tokenService = tokenService;
        private readonly IValidator<UpsertSessionProjectActionDto> _upsertSessionProjectValidator = upsertSessionProjectValidator;
        private readonly AIChatDbContext _ctx = ctx;

        /// <inheritdoc />
        public async Task<PaginatedResponseDto<SessionProjectDto>> SearchSessionProjectsAsync(string? filter, int skip = 0, int take = 10, CancellationToken cancellationToken = default)
        {
            var userId = _tokenService.GetOid()!.Value;

            var query = _ctx.SessionProjects
                .AsNoTracking()
                .Where(x => x.UserId == userId && !x.DateDeactivated.HasValue);

            if (!string.IsNullOrWhiteSpace(filter))
            {
                query = query.Where(x => !string.IsNullOrWhiteSpace(x.Name) && EF.Functions.Like(x.Name, $"%{filter}%"));
            }

            var totalCount = await query.CountAsync(cancellationToken);

            var items = await query
                .OrderByDescending(x => x.DateCreated)
                .Skip(skip)
                .Take(take)
                .Select(s => s.MapToSessionProjectDto())
                .ToListAsync(cancellationToken);

            return new PaginatedResponseDto<SessionProjectDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageSize = take,
                CurrentPage = (skip / take) + 1
            };
        }

        /// <inheritdoc />
        public async Task<SessionProjectDto> CreateProjectAsync(UpsertSessionProjectActionDto request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            _upsertSessionProjectValidator.ValidateAndThrow(request);

            var userId = _tokenService.GetOid()!.Value;

            var sessionExists = await _ctx.Sessions
                .Where(x => x.Id == request.SessionId && x.UserId == userId && !x.DateDeactivated.HasValue)
                .AnyAsync(cancellationToken);
            if (!sessionExists)
            {
                _logger.LogWarning("Session with id {id} not found or already deactivated.", request.SessionId);
                throw new InvalidOperationException($"Session not found or already deactivated.");
            }

            var date = DateTimeOffset.UtcNow;
            var newProject = new SessionProject()
            {
                SessionId = request.SessionId,
                UserId = userId,
                Name = request.Name,
                Description = request.Description,
                Instructions = request.Instructions,
                DateCreated = date,
                DateModified = date
            };
            await _ctx.AddAsync(newProject, cancellationToken);
            await _ctx.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Session project {Id} successfully created.", newProject.Id);

            return newProject.MapToSessionProjectDto();
        }

        /// <inheritdoc />
        public async Task<SessionProjectDto> UpdateSessionProjectAsync(UpsertSessionProjectActionDto request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            _upsertSessionProjectValidator.ValidateAndThrow(request);

            var userId = _tokenService.GetOid()!.Value;

            var existingProject = await _ctx.SessionProjects
                .Where(x => x.Id == request.Id && 
                        x.SessionId == request.SessionId && 
                        x.UserId == userId && 
                        !x.DateDeactivated.HasValue)
                .FirstOrDefaultAsync(cancellationToken);
            if (existingProject == null)
            {
                _logger.LogWarning("Session project with id {id} not found or already deactivated.", request.SessionId);
                throw new InvalidOperationException($"Session project not found or already deactivated.");
            }

            existingProject.Name = request.Name;
            existingProject.Description = request.Description;
            existingProject.Instructions = request.Instructions;
            existingProject.DateModified = DateTimeOffset.UtcNow;
            await _ctx.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Session project {Id} successfully updated.", existingProject.Id);

            return existingProject.MapToSessionProjectDto();
        }
    }
}
