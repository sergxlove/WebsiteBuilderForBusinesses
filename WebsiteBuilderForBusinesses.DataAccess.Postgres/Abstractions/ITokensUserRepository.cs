using WebsiteBuilderForBusinesses.Core.Models;

namespace WebsiteBuilderForBusinesses.DataAccess.Postgres.Abstractions
{
    public interface ITokensUserRepository
    {
        Task<Guid> AddAsync(TokensUser tokensUser, CancellationToken token);
        Task<int> DeleteAsync(Guid id, CancellationToken token);
        Task<TokensUser?> GetAsync(Guid id, CancellationToken token);
        Task<int> UpdateAsync(TokensUser tokenUser, CancellationToken token);
    }
}