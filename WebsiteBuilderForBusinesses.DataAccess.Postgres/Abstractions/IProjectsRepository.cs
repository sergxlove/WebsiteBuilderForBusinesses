using WebsiteBuilderForBusinesses.Core.Models;
using WebsiteBuilderForBusinesses.DataAccess.Postgres.Dto;

namespace WebsiteBuilderForBusinesses.DataAccess.Postgres.Abstractions
{
    public interface IProjectsRepository
    {
        Task<bool> CheckNameAsync(string name, CancellationToken token);
        Task<Guid> CreateAsync(Projects project, CancellationToken token);
        Task<int> DeleteAsync(Guid id, CancellationToken token);
        Task<List<ShortProject>> GetAllAsync(CancellationToken token);
        Task<string> GetHtmlByIdAsync(Guid id, CancellationToken token);
        Task<int> UpdateHtmlAsync(Projects project, CancellationToken token);
    }
}