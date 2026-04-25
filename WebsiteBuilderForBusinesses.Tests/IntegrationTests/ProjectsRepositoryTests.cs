using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WebsiteBuilderForBusinesses.Core.Models;
using WebsiteBuilderForBusinesses.DataAccess.Postgres;
using WebsiteBuilderForBusinesses.DataAccess.Postgres.Models;
using WebsiteBuilderForBusinesses.DataAccess.Postgres.Repositories;

namespace WebsiteBuilderForBusinesses.Tests.IntegrationTests
{
    public class ProjectsRepositoryTests
    {
        private WebBuilderDbContext _context;
        private ProjectsRepository _repository;
        private CancellationToken _cancellationToken;
        private SqliteConnection _connection;

        [SetUp]
        public void SetUp()
        {
            // Создаем соединение SQLite в памяти
            _connection = new SqliteConnection("Filename=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<WebBuilderDbContext>()
                .UseSqlite(_connection)
                .Options;

            _context = new WebBuilderDbContext(options);
            _context.Database.EnsureCreated();

            _repository = new ProjectsRepository(_context);
            _cancellationToken = CancellationToken.None;
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
            _connection.Dispose();
        }

        private Projects CreateValidProject()
        {
            var result = Projects.Create(
                Guid.NewGuid(),
                "Test Project",
                DateTime.UtcNow,
                "<html><body>Test</body></html>"
            );
            return result.Value;
        }

        [Test]
        public async Task CreateAsync_ValidProject_ReturnsProjectId()
        {
            // Arrange
            var project = CreateValidProject();

            // Act
            var result = await _repository.CreateAsync(project, _cancellationToken);

            // Assert
            Assert.That(result, Is.EqualTo(project.Id));
            var savedProject = await _context.Projects.FirstOrDefaultAsync(p => p.Id == project.Id);
            Assert.That(savedProject, Is.Not.Null);
            Assert.That(savedProject.Name, Is.EqualTo(project.Name));
            Assert.That(savedProject.TextHtml, Is.EqualTo(project.TextHtml));
        }

        [Test]
        public async Task GetHtmlByIdAsync_ExistingProject_ReturnsHtml()
        {
            // Arrange
            var project = CreateValidProject();
            await _repository.CreateAsync(project, _cancellationToken);

            // Act
            var result = await _repository.GetHtmlByIdAsync(project.Id, _cancellationToken);

            // Assert
            Assert.That(result, Is.EqualTo(project.TextHtml));
        }

        [Test]
        public async Task GetHtmlByIdAsync_NonExistingProject_ReturnsEmptyString()
        {
            // Act
            var result = await _repository.GetHtmlByIdAsync(Guid.NewGuid(), _cancellationToken);

            // Assert
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetAllAsync_MultipleProjects_ReturnsShortProjectDtoList()
        {
            // Arrange
            var projects = new List<ProjectEntity>
            {
                new() { Id = Guid.NewGuid(), Name = "Project 1", DateOpen = DateTime.UtcNow, TextHtml = "Html1" },
                new() { Id = Guid.NewGuid(), Name = "Project 2", DateOpen = DateTime.UtcNow, TextHtml = "Html2" },
                new() { Id = Guid.NewGuid(), Name = "Project 3", DateOpen = DateTime.UtcNow, TextHtml = "Html3" }
            };
            await _context.Projects.AddRangeAsync(projects);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllAsync(_cancellationToken);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(3));
            Assert.That(result.All(r => r.Id != Guid.Empty), Is.True);
            Assert.That(result.All(r => !string.IsNullOrEmpty(r.Name)), Is.True);
        }

        [Test]
        public async Task GetAllAsync_NoProjects_ReturnsEmptyList()
        {
            // Act
            var result = await _repository.GetAllAsync(_cancellationToken);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task UpdateHtmlAsync_ExistingProject_UpdatesHtmlAndDate()
        {
            // Arrange
            var project = CreateValidProject();
            await _repository.CreateAsync(project, _cancellationToken);

            var updatedProject = Projects.Create(
                project.Id,
                project.Name,
                DateTime.UtcNow,
                "New Html"
            ).Value;

            // Act
            var result = await _repository.UpdateHtmlAsync(updatedProject, _cancellationToken);

            // Assert
            Assert.That(result, Is.EqualTo(1));
            var updatedHtml = await _repository.GetHtmlByIdAsync(project.Id, _cancellationToken);
            Assert.That(updatedHtml, Is.EqualTo("New Html"));
        }

        [Test]
        public async Task UpdateHtmlAsync_NonExistingProject_ReturnsZero()
        {
            // Arrange
            var project = Projects.Create(
                Guid.NewGuid(),
                "Non Existing",
                DateTime.UtcNow,
                "New Html"
            ).Value;

            // Act
            var result = await _repository.UpdateHtmlAsync(project, _cancellationToken);

            // Assert
            Assert.That(result, Is.EqualTo(0));
        }

        [Test]
        public async Task UpdateNameAsync_ExistingName_UpdatesProjectName()
        {
            // Arrange
            var project = CreateValidProject();
            await _repository.CreateAsync(project, _cancellationToken);

            // Act
            var result = await _repository.UpdateNameAsync("Test Project", "New Name", _cancellationToken);

            // Assert
            Assert.That(result, Is.EqualTo(1));
            var allProjects = await _repository.GetAllAsync(_cancellationToken);
            var updatedProject = allProjects.First(p => p.Id == project.Id);
            Assert.That(updatedProject.Name, Is.EqualTo("New Name"));
        }

        [Test]
        public async Task UpdateNameAsync_NonExistingName_ReturnsZero()
        {
            // Act
            var result = await _repository.UpdateNameAsync("Non Existing", "New Name", _cancellationToken);

            // Assert
            Assert.That(result, Is.EqualTo(0));
        }

        [Test]
        public async Task UpdateNameAsync_MultipleProjectsWithSameName_UpdatesAll()
        {
            // Arrange
            var project1 = Projects.Create(Guid.NewGuid(), "Same Name", DateTime.UtcNow, "Html1").Value;
            var project2 = Projects.Create(Guid.NewGuid(), "Same Name", DateTime.UtcNow, "Html2").Value;

            await _repository.CreateAsync(project1, _cancellationToken);
            await _repository.CreateAsync(project2, _cancellationToken);

            // Act
            var result = await _repository.UpdateNameAsync("Same Name", "New Name", _cancellationToken);

            // Assert
            Assert.That(result, Is.EqualTo(2));
            var allProjects = await _repository.GetAllAsync(_cancellationToken);
            Assert.That(allProjects.Count(p => p.Name == "New Name"), Is.EqualTo(2));
        }

        [Test]
        public async Task DeleteAsync_ExistingProject_DeletesProject()
        {
            // Arrange
            var project = CreateValidProject();
            await _repository.CreateAsync(project, _cancellationToken);

            // Act
            var result = await _repository.DeleteAsync(project.Id, _cancellationToken);

            // Assert
            Assert.That(result, Is.EqualTo(1));
            var html = await _repository.GetHtmlByIdAsync(project.Id, _cancellationToken);
            Assert.That(html, Is.Empty);
        }

        [Test]
        public async Task DeleteAsync_NonExistingProject_ReturnsZero()
        {
            // Act
            var result = await _repository.DeleteAsync(Guid.NewGuid(), _cancellationToken);

            // Assert
            Assert.That(result, Is.EqualTo(0));
        }

        [Test]
        public async Task CheckNameAsync_ExistingName_ReturnsTrue()
        {
            // Arrange
            var project = CreateValidProject();
            await _repository.CreateAsync(project, _cancellationToken);

            // Act
            var result = await _repository.CheckNameAsync("Test Project", _cancellationToken);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task CheckNameAsync_NonExistingName_ReturnsFalse()
        {
            // Act
            var result = await _repository.CheckNameAsync("Non Existing", _cancellationToken);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public void CreateProject_WithInvalidParameters_ShouldReturnFailure()
        {
            // Act
            var resultWithEmptyId = Projects.Create(
                Guid.Empty,
                "Test",
                DateTime.UtcNow,
                "Html"
            );

            var resultWithEmptyName = Projects.Create(
                Guid.NewGuid(),
                string.Empty,
                DateTime.UtcNow,
                "Html"
            );

            // Assert
            Assert.That(resultWithEmptyId.IsSuccess, Is.False);
            Assert.That(resultWithEmptyId.Error, Is.EqualTo("Поле Id не должно быть пустым"));

            Assert.That(resultWithEmptyName.IsSuccess, Is.False);
            Assert.That(resultWithEmptyName.Error, Is.EqualTo("Название не должно быть пустым"));
        }
    }
}
