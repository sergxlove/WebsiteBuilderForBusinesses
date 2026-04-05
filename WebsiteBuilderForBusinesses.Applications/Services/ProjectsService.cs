using WebsiteBuilderForBusinesses.Applications.Abstractions;
using WebsiteBuilderForBusinesses.Core.Models;
using WebsiteBuilderForBusinesses.DataAccess.Postgres.Abstractions;
using WebsiteBuilderForBusinesses.DataAccess.Postgres.Dto;

namespace WebsiteBuilderForBusinesses.Applications.Services
{
    public class ProjectsService : IProjectsService
    {
        private readonly IProjectsRepository _repository;
        public ProjectsService(IProjectsRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> CheckNameAsync(string name, CancellationToken token)
        {
            return await _repository.CheckNameAsync(name, token);
        }
        public async Task<Guid> CreateAsync(Projects project, CancellationToken token)
        {
            return await _repository.CreateAsync(project, token);
        }
        public async Task<int> DeleteAsync(Guid id, CancellationToken token)
        {
            return await _repository.DeleteAsync(id, token);
        }
        public async Task<List<ShortProject>> GetAllAsync(CancellationToken token)
        {
            return await _repository.GetAllAsync(token);
        }
        public async Task<string> GetHtmlByIdAsync(Guid id, CancellationToken token)
        {
            return await _repository.GetHtmlByIdAsync(id, token);
        }
        public async Task<int> UpdateHtmlAsync(Projects project, CancellationToken token)
        {
            return await _repository.UpdateHtmlAsync(project, token);
        }

        public async Task<int> UpdateNameAsync(string oldName, string newName, CancellationToken token)
        {
            return await _repository.UpdateNameAsync(oldName, newName, token);
        }
    }
}
