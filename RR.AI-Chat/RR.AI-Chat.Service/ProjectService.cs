using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Graph.IdentityGovernance.LifecycleWorkflows.DeletedItems.Workflows.Item.MicrosoftGraphIdentityGovernanceActivateWithScope;
using RR.AI_Chat.Dto;
using RR.AI_Chat.Dto.Actions.Project;
using RR.AI_Chat.Entity;
using RR.AI_Chat.Repository;

namespace RR.AI_Chat.Service
{
    public interface IProjectService
    {
        /// <summary>
        /// Searches for active projects belonging to the current user with optional filtering and pagination.
        /// </summary>
        /// <param name="filter">Optional filter string to search project names using a LIKE pattern.</param>
        /// <param name="skip">Number of records to skip for pagination. Default is 0.</param>
        /// <param name="take">Number of records to take for pagination. Default is 10.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>A paginated response containing matching projects ordered by creation date descending.</returns>
        Task<PaginatedResponseDto<ProjectDto>> SearchProjectsAsync(string? filter, int skip = 0, int take = 10, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates a new project for the current authenticated user.
        /// </summary>
        /// <param name="request">The project creation request containing name, description, and instructions.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>The created project as a <see cref="ProjectDto"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the request is null.</exception>
        /// <exception cref="ValidationException">Thrown when the request fails validation.</exception>
        Task<ProjectDto> CreateProjectAsync(UpsertProjectActionDto request, CancellationToken cancellationToken);

        /// <summary>
        /// Updates an existing active project belonging to the current user.
        /// </summary>
        /// <param name="request">The project update request containing the project ID and updated values.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>The updated project as a <see cref="ProjectDto"/>.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the request is null.</exception>
        /// <exception cref="ValidationException">Thrown when the request fails validation.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the project is not found or already deactivated.</exception>
        Task<ProjectDto> UpdateProjectAsync(UpsertProjectActionDto request, CancellationToken cancellationToken);

        /// <summary>
        /// Deactivates a project and removes its association from all related sessions.
        /// </summary>
        /// <param name="id">The unique identifier of the project to deactivate.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the project is not found or already deactivated.</exception>
        /// <remarks>
        /// This operation is transactional. The project's DateDeactivated is set, and all associated sessions 
        /// have their ProjectId set to null within a single database transaction.
        /// </remarks>
        Task DeactivateProjectAsync(Guid id, CancellationToken cancellationToken);

        /// <summary>
        /// Retrieves detailed information about a specific active project belonging to the current user.
        /// </summary>
        /// <param name="id">The unique identifier of the project to retrieve.</param>
        /// <param name="cancellationToken">Cancellation token to cancel the operation.</param>
        /// <returns>The project details as a <see cref="ProjectDetailDto"/>.</returns>
        /// <exception cref="InvalidOperationException">Thrown when the project is not found or already deactivated.</exception>
        Task<ProjectDetailDto> GetProjectByIdAsync(Guid id, CancellationToken cancellationToken);
    }

    public class ProjectService(ILogger<ProjectService> logger,
        ITokenService tokenService,
        IValidator<UpsertProjectActionDto> upsertProjectValidator,
        AIChatDbContext ctx) : IProjectService
    {
        private readonly ILogger<ProjectService> _logger = logger;
        private readonly ITokenService _tokenService = tokenService;
        private readonly IValidator<UpsertProjectActionDto> _upsertProjectValidator = upsertProjectValidator;
        private readonly AIChatDbContext _ctx = ctx;

        /// <inheritdoc />
        public async Task<PaginatedResponseDto<ProjectDto>> SearchProjectsAsync(string? filter, int skip = 0, int take = 10, CancellationToken cancellationToken = default)
        {
            var userId = _tokenService.GetOid()!.Value;

            var query = _ctx.Projects
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
                .Select(s => s.MapToProjectDto())
                .ToListAsync(cancellationToken);

            return new PaginatedResponseDto<ProjectDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageSize = take,
                CurrentPage = (skip / take) + 1
            };
        }

        /// <inheritdoc />
        public async Task<ProjectDto> CreateProjectAsync(UpsertProjectActionDto request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            _upsertProjectValidator.ValidateAndThrow(request);

            var userId = _tokenService.GetOid()!.Value;

            var date = DateTimeOffset.UtcNow;
            var newProject = new Project()
            {
                UserId = userId,
                Name = request.Name,
                Description = request.Description,
                Instructions = request.Instructions,
                DateCreated = date,
                DateModified = date
            };
            await _ctx.AddAsync(newProject, cancellationToken);
            await _ctx.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Project {Id} successfully created.", newProject.Id);

            return newProject.MapToProjectDto();
        }

        /// <inheritdoc />
        public async Task<ProjectDto> UpdateProjectAsync(UpsertProjectActionDto request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            _upsertProjectValidator.ValidateAndThrow(request);

            var userId = _tokenService.GetOid()!.Value;

            var existingProject = await _ctx.Projects
                .Where(x => x.Id == request.Id && 
                        x.UserId == userId && 
                        !x.DateDeactivated.HasValue)
                .FirstOrDefaultAsync(cancellationToken);
            if (existingProject == null)
            {
                _logger.LogWarning("Project not found or already deactivated.");
                throw new InvalidOperationException($"Project not found or already deactivated.");
            }

            existingProject.Name = request.Name;
            existingProject.Description = request.Description;
            existingProject.Instructions = request.Instructions;
            existingProject.DateModified = DateTimeOffset.UtcNow;
            await _ctx.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Project {Id} successfully updated.", existingProject.Id);

            return existingProject.MapToProjectDto();
        }

        /// <inheritdoc />
        public async Task<ProjectDetailDto> GetProjectByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            var userId = _tokenService.GetOid()!.Value;

            var project = await _ctx.Projects
                .AsNoTracking()
                .Where(x => x.Id == id &&
                        x.UserId == userId &&
                        !x.DateDeactivated.HasValue)
                .Select(p => p.MapToProjectDetailDto())
                .FirstOrDefaultAsync(cancellationToken);
            if (project == null)
            {
                _logger.LogWarning("Project with id {Id} not found or already deactivated.", id);
                throw new InvalidOperationException($"Project not found or already deactivated.");
            }

            return project;
        }

        /// <inheritdoc />
        public async Task DeactivateProjectAsync(Guid id, CancellationToken cancellationToken)
        {
            var userId = _tokenService.GetOid()!.Value;

            var projectExists = await _ctx.Projects
                .Where(x => x.Id == id &&
                        x.UserId == userId &&
                        !x.DateDeactivated.HasValue)
                .AnyAsync(cancellationToken);

            if (!projectExists)
            {
                _logger.LogWarning("Project with id {Id} not found or already deactivated.", id);
                throw new InvalidOperationException($"Project not found or already deactivated.");
            }

            var date = DateTimeOffset.UtcNow;
            using var transaction = await _ctx.Database.BeginTransactionAsync(cancellationToken);

            await _ctx.Projects
                .Where(x => x.Id == id)
                .ExecuteUpdateAsync(p => p
                    .SetProperty(x => x.DateDeactivated, date)
                    .SetProperty(x => x.DateModified, date),
                    cancellationToken);

            await _ctx.ProjectDocuments
                .Where(pd => pd.ProjectId == id)
                .ExecuteUpdateAsync(pd => pd
                    .SetProperty(x => x.DateDeactivated, date)
                    .SetProperty(x => x.DateModified, date),
                    cancellationToken);

            await _ctx.ProjectDocumentPages
                .Where(pdp => pdp.ProjectDocument.ProjectId == id)
                .ExecuteUpdateAsync(pdp => pdp
                    .SetProperty(x => x.DateDeactivated, date)
                    .SetProperty(x => x.DateModified, date),
                    cancellationToken);

            await _ctx.Sessions
                .Where(s => s.ProjectId == id &&
                            s.UserId == userId &&
                            !s.DateDeactivated.HasValue)
                .ExecuteUpdateAsync(s => s
                    .SetProperty(x => x.ProjectId, (Guid?)null)
                    .SetProperty(x => x.DateModified, date),
                    cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            _logger.LogInformation("Project {Id} successfully deactivated.", id);
        }
    }
}
