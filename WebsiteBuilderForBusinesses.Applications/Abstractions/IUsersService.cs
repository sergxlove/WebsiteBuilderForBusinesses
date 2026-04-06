using WebsiteBuilderForBusinesses.Core.Models;
using WebsiteBuilderForBusinesses.DataAccess.Postgres.Dto;

namespace WebsiteBuilderForBusinesses.Applications.Abstractions
{
    public interface IUsersService
    {
        Task<bool> CheckAsync(string login, CancellationToken token);
        Task<Guid> CreateAsync(Users user, CancellationToken token);
        Task<string> GetRoleAsync(string login, CancellationToken token);
        Task<bool> VerifyAsync(string login, string password);
        Task<int> UpdatePasswordAsync(Users user, CancellationToken token);
        Task<int> UpdateRoleAsync(Users user, CancellationToken token);
        Task<List<ShortUserDto>> GetAllAsync(CancellationToken token);
        Task<int> DeleteAsync(Guid id, CancellationToken token);
    }
}