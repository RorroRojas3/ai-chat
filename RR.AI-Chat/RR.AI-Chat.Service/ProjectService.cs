using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RR.AI_Chat.Dto;
using RR.AI_Chat.Dto.Actions.Session;
using RR.AI_Chat.Entity;
using RR.AI_Chat.Repository;

namespace RR.AI_Chat.Service
{
    public interface IProjectService
    {
        Task<PaginatedResponseDto<ProjectDto>> SearchProjectsAsync(string? filter, int skip = 0, int take = 10, CancellationToken cancellationToken = default);

        Task<ProjectDto> CreateProjectAsync(UpsertProjectActionDto request, CancellationToken cancellationToken);

        Task<ProjectDto> UpdateProjectAsync(UpsertProjectActionDto request, CancellationToken cancellationToken);
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
                _logger.LogWarning("Project with id {Id} not found or already deactivated.", request.Id);
                throw new InvalidOperationException($"Session project not found or already deactivated.");
            }

            existingProject.Name = request.Name;
            existingProject.Description = request.Description;
            existingProject.Instructions = request.Instructions;
            existingProject.DateModified = DateTimeOffset.UtcNow;
            await _ctx.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Project {Id} successfully updated.", existingProject.Id);

            return existingProject.MapToProjectDto();
        }
    }
}
