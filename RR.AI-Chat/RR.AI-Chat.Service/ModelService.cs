using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RR.AI_Chat.Dto;
using RR.AI_Chat.Repository;

namespace RR.AI_Chat.Service
{
    public interface IModelService
    {
        Task<List<ModelDto>> GetModelsAsync();
    }

    public class ModelService(ILogger<ModelService> logger,
        AIChatDbContext ctx) : IModelService
    {
        private readonly ILogger<ModelService> _logger = logger;
        private readonly AIChatDbContext _ctx = ctx;

        /// <summary>
        /// Retrieves all available AI models from the database.
        /// </summary>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains a list of <see cref="ModelDto"/> 
        /// objects representing all available models with their ID and name.
        /// </returns>
        /// <exception cref="InvalidOperationException">
        /// Thrown when the database context is not properly configured or the Models DbSet is null.
        /// </exception>
        public async Task<List<ModelDto>> GetModelsAsync()
        {
            var models = await _ctx.Models
                            .AsNoTracking()
                            .Select(x => new ModelDto
                            {
                                Id = x.Id,
                                Name = x.Name
                            })
                            .ToListAsync();

            return models;
        }
    }
}
