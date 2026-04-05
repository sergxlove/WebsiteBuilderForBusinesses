using Microsoft.EntityFrameworkCore;
using WebsiteBuilderForBusinesses.Core.Models;
using WebsiteBuilderForBusinesses.DataAccess.Postgres.Abstractions;
using WebsiteBuilderForBusinesses.DataAccess.Postgres.Dto;
using WebsiteBuilderForBusinesses.DataAccess.Postgres.Models;

namespace WebsiteBuilderForBusinesses.DataAccess.Postgres.Repositories
{
    public class ProjectsRepository : IProjectsRepository
    {
        private readonly WebBuilderDbContext _context;
        public ProjectsRepository(WebBuilderDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> CreateAsync(Projects project, CancellationToken token)
        {
            ProjectEntity projectEntity = new()
            {
                Id = project.Id,
                Name = project.Name,
                DateOpen = project.DateOpen,
                TextHtml = project.TextHtml,
            };
            await _context.Projects.AddAsync(projectEntity, token);
            await _context.SaveChangesAsync(token);
            return projectEntity.Id;
        }

        public async Task<string> GetHtmlByIdAsync(Guid id, CancellationToken token)
        {
            ProjectEntity? result = await _context.Projects
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id, token);
            if (result is null) return string.Empty;
            return result.TextHtml;
        }

        public async Task<List<ShortProject>> GetAllAsync(CancellationToken token)
        {
            return await _context.Projects
                .AsNoTracking()
                .Select(pr => new ShortProject
                {
                    Id = pr.Id,
                    Name = pr.Name,
                    DateOpen = pr.DateOpen,
                })
                .ToListAsync(token);
        }

        public async Task<int> UpdateHtmlAsync(Projects project, CancellationToken token)
        {
            return await _context.Projects
                .AsNoTracking()
                .Where(a => a.Id == project.Id)
                .ExecuteUpdateAsync(a => a
                .SetProperty(a => a.DateOpen, project.DateOpen)
                .SetProperty(a => a.TextHtml, project.TextHtml), token);
        }

        public async Task<int> UpdateNameAsync(string oldName, string newName, CancellationToken token)
        {
            return await _context.Projects
                .AsNoTracking()
                .Where(a => a.Name == oldName)
                .ExecuteUpdateAsync(a => a
                .SetProperty(a => a.Name, newName), token);
        }

        public async Task<int> DeleteAsync(Guid id, CancellationToken token)
        {
            return await _context.Projects
                .AsNoTracking()
                .Where(a => a.Id == id)
                .ExecuteDeleteAsync(token);
        }

        public async Task<bool> CheckNameAsync(string name, CancellationToken token)
        {
            ProjectEntity? result = await _context.Projects
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Name == name, token);
            if (result is null) return false;
            return true;
        }
    }
}
