using Microsoft.EntityFrameworkCore;
using WebsiteBuilderForBusinesses.DataAccess.Postgres;
using WebsiteBuilderForBusinesses.DataAccess.Postgres.Models;
using WebsiteBuilderForBusinesses.DataAccess.Postgres.Repositories;

namespace WebsiteBuilderForBusinesses.Tests
{
    public abstract class RepositoryTestBase
    {
        protected WebBuilderDbContext _context;
        protected ProjectsRepository _repository;
        protected CancellationToken _cancellationToken;

        [SetUp]
        public virtual async Task Setup()
        {
            var options = new DbContextOptionsBuilder<WebBuilderDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new WebBuilderDbContext(options);
            _repository = new ProjectsRepository(_context);
            _cancellationToken = CancellationToken.None;

            await SeedDatabase();
        }

        [TearDown]
        public virtual async Task TearDown()
        {
            await _context.DisposeAsync();
        }

        protected virtual async Task SeedDatabase()
        {
            await Task.CompletedTask;
        }

        protected async Task<int> GetProjectsCount()
        {
            return await _context.Projects.CountAsync(_cancellationToken);
        }

        protected async Task<ProjectEntity?> GetProjectById(Guid id)
        {
            return await _context.Projects.FindAsync(id, _cancellationToken);
        }
    }
}
