using WebsiteBuilderForBusinesses.Core.Models;

namespace WebsiteBuilderForBusinesses.Applications.Abstractions
{
    public interface IUsersService
    {
        Task<bool> CheckAsync(string login, CancellationToken token);
        Task<Guid> CreateAsync(Users user, CancellationToken token);
        Task<string> GetRoleAsync(string login, CancellationToken token);
        Task<bool> VerifyAsync(string login, string password);
    }
}