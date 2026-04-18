using Microsoft.EntityFrameworkCore;
using WebsiteBuilderForBusinesses.Core.Models;
using WebsiteBuilderForBusinesses.DataAccess.Postgres.Abstractions;
using WebsiteBuilderForBusinesses.DataAccess.Postgres.Dto;
using WebsiteBuilderForBusinesses.DataAccess.Postgres.Models;

namespace WebsiteBuilderForBusinesses.DataAccess.Postgres.Repositories
{
    public class UsersRepository : IUsersRepository
    {
        private readonly WebBuilderDbContext _context;
        public UsersRepository(WebBuilderDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> CreateAsync(Users user, CancellationToken token)
        {
            try
            {
                UsersEntity usersEntity = new UsersEntity()
                {
                    Id = user.Id,
                    Login = user.Login,
                    HashPassword = user.HashPassword,
                    Role = user.Role
                };
                await _context.Users.AddAsync(usersEntity, token);
                await _context.SaveChangesAsync(token);
                return usersEntity.Id;
            }
            catch
            {
                return Guid.Empty;
            }
        }

        public async Task<bool> VerifyAsync(string login, string password)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(a => a.Login == login);
            if (user == null) return false;
            return Users.VerifyPassword(password, user.HashPassword);
        }

        public async Task<bool> CheckAsync(string login, CancellationToken token)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(a => a.Login == login, token);
            if (user is null) return false;
            return true;
        }

        public async Task<string> GetRoleAsync(string login, CancellationToken token)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(a => a.Login == login, token);
            if (user is null) return "user";
            return user.Role;
        }

        public async Task<Guid> GetIdAsync(string login, CancellationToken token)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(a => a.Login == login, token);
            if(user is null) return Guid.Empty;
            return user.Id;
        }

        public async Task<int> UpdatePasswordAsync(Users user, CancellationToken token)
        {
            return await _context.Users
                .AsNoTracking()
                .Where(a => a.Login == user.Login)
                .ExecuteUpdateAsync(a => a
                .SetProperty(a => a.HashPassword, user.HashPassword), token);
        }

        public async Task<int> UpdateRoleAsync(Users user, CancellationToken token)
        {
            return await _context.Users
                .AsNoTracking()
                .Where(a => a.Login == user.Login)
                .ExecuteUpdateAsync(a => a
                .SetProperty(a => a.Role, user.Role), token);
        }

        public async Task<List<ShortUserDto>> GetAllAsync(CancellationToken token)
        {
            return await _context.Users
                .AsNoTracking()
                .Select(pr => new ShortUserDto
                {
                    Id = pr.Id,
                    Login = pr.Login,
                    Role = pr.Role,
                })
                .ToListAsync(token);
        }

        public async Task<int> DeleteAsync(Guid id, CancellationToken token)
        {
            return await _context.Users
                .AsNoTracking()
                .Where(a => a.Id == id)
                .ExecuteDeleteAsync(token);
        }
    }
}
