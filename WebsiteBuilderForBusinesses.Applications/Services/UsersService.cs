using WebsiteBuilderForBusinesses.Applications.Abstractions;
using WebsiteBuilderForBusinesses.Core.Models;
using WebsiteBuilderForBusinesses.DataAccess.Postgres.Abstractions;

namespace WebsiteBuilderForBusinesses.Applications.Services
{
    public class UsersService : IUsersService
    {
        private readonly IUsersRepository _repository;
        public UsersService(IUsersRepository repository)
        {
            _repository = repository;
        }
        public async Task<bool> CheckAsync(string login, CancellationToken token)
        {
            return await _repository.CheckAsync(login, token);
        }
        public async Task<Guid> CreateAsync(Users user, CancellationToken token)
        {
            return await _repository.CreateAsync(user, token);
        }
        public async Task<string> GetRoleAsync(string login, CancellationToken token)
        {
            return await _repository.GetRoleAsync(login, token);
        }
        public async Task<bool> VerifyAsync(string login, string password)
        {
            return await _repository.VerifyAsync(login, password);
        }
    }
}
