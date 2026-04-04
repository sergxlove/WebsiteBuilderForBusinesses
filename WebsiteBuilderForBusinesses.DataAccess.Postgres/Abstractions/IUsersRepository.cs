using WebsiteBuilderForBusinesses.Core.Models;

namespace WebsiteBuilderForBusinesses.DataAccess.Postgres.Abstractions
{
    public interface IUsersRepository
    {
        Task<bool> CheckAsync(string login, CancellationToken token);
        Task<Guid> CreateAsync(Users user, CancellationToken token);
        Task<string> GetRoleAsync(string login, CancellationToken token);
        Task<bool> VerifyAsync(string login, string password);
    }
}