using Microsoft.EntityFrameworkCore;
using WebsiteBuilderForBusinesses.Core.Infrastructures;
using WebsiteBuilderForBusinesses.Core.Models;
using WebsiteBuilderForBusinesses.DataAccess.Postgres.Models;

namespace WebsiteBuilderForBusinesses.Tests.IntegrationTests
{
    public class ProjectRepositoryTests : RepositoryTestBase
    {
        private Projects _testProject = default!;
        private ProjectEntity _testProjectEntity = default!;

        protected override async Task SeedDatabase()
        {
            ResultModel<Projects> testProject = Projects.Create(Guid.NewGuid(), "Test Project",
                new DateTime(2024, 1, 1, 10, 0, 0, DateTimeKind.Utc), "<html><body>Test Content</body></html>");
            _testProject = testProject.Value;
            _testProjectEntity = new ProjectEntity
            {
                Id = _testProject.Id,
                Name = _testProject.Name,
                DateOpen = _testProject.DateOpen,
                TextHtml = _testProject.TextHtml
            };

            await _context.Projects.AddAsync(_testProjectEntity);
            await _context.SaveChangesAsync();
        }

        #region CreateAsync Tests

        [Test]
        public async Task CreateAsync_ValidProject_ShouldCreateSuccessfully()
        {
            // Arrange
            var newProjectResult = Projects.Create(
                Guid.NewGuid(),
                "New Project",
                DateTime.UtcNow,
                "<html><body>New Content</body></html>"
            );

            Assert.That(newProjectResult.IsSuccess, Is.True);
            var newProject = newProjectResult.Value;

            // Act
            var result = await _repository.CreateAsync(newProject, _cancellationToken);

            // Assert
            Assert.That(result, Is.EqualTo(newProject.Id));

            var savedProject = await _context.Projects.FindAsync(newProject.Id);
            Assert.That(savedProject, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(savedProject.Name, Is.EqualTo(newProject.Name));
                Assert.That(savedProject.TextHtml, Is.EqualTo(newProject.TextHtml));
                Assert.That(savedProject.DateOpen, Is.EqualTo(newProject.DateOpen));
            });
        }

        [Test]
        public async Task CreateAsync_ProjectWithEmptyHtml_ShouldCreateSuccessfully()
        {
            // Arrange
            var emptyHtmlProjectResult = Projects.Create(
                Guid.NewGuid(),
                "Empty HTML Project",
                DateTime.UtcNow,
                ""  // Пустой HTML
            );

            Assert.That(emptyHtmlProjectResult.IsSuccess, Is.True);
            var emptyHtmlProject = emptyHtmlProjectResult.Value;

            // Act
            var result = await _repository.CreateAsync(emptyHtmlProject, _cancellationToken);

            // Assert
            Assert.That(result, Is.EqualTo(emptyHtmlProject.Id));

            var savedProject = await _context.Projects.FindAsync(emptyHtmlProject.Id);
            Assert.That(savedProject, Is.Not.Null);
            Assert.That(savedProject.TextHtml, Is.EqualTo(""));
        }

        [Test]
        public void CreateAsync_DuplicateId_ShouldThrowException()
        {
            // Arrange
            var duplicateProjectResult = Projects.Create(
                _testProject.Id, // Существующий ID
                "Duplicate Project",
                DateTime.UtcNow,
                "<html></html>"
            );

            Assert.That(duplicateProjectResult.IsSuccess, Is.True);
            var duplicateProject = duplicateProjectResult.Value;

            // Act & Assert
            Assert.ThrowsAsync<DbUpdateException>(
                async () => await _repository.CreateAsync(duplicateProject, _cancellationToken)
            );
        }

        [Test]
        public async Task CreateAsync_ProjectWithVeryLongName_ShouldSucceed()
        {
            // Arrange
            var longName = new string('a', 500);
            var longNameProjectResult = Projects.Create(
                Guid.NewGuid(),
                longName,
                DateTime.UtcNow,
                "<html>Long name test</html>"
            );

            Assert.That(longNameProjectResult.IsSuccess, Is.True);
            var longNameProject = longNameProjectResult.Value;

            // Act
            var result = await _repository.CreateAsync(longNameProject, _cancellationToken);

            // Assert
            Assert.That(result, Is.EqualTo(longNameProject.Id));

            var savedProject = await _context.Projects.FindAsync(longNameProject.Id);
            Assert.That(savedProject, Is.Not.Null);
            Assert.That(savedProject.Name, Is.EqualTo(longName));
        }

        [Test]
        public async Task CreateAsync_ProjectWithMinimalValidData_ShouldSucceed()
        {
            // Arrange
            var minimalProjectResult = Projects.Create(
                Guid.NewGuid(),
                "Min",
                DateTime.UtcNow,
                "<html></html>"
            );

            Assert.That(minimalProjectResult.IsSuccess, Is.True);
            var minimalProject = minimalProjectResult.Value;

            // Act
            var result = await _repository.CreateAsync(minimalProject, _cancellationToken);

            // Assert
            Assert.That(result, Is.EqualTo(minimalProject.Id));

            var savedProject = await _context.Projects.FindAsync(minimalProject.Id);
            Assert.That(savedProject, Is.Not.Null);
            Assert.That(savedProject.Name, Is.EqualTo("Min"));
        }

        #endregion

        #region GetHtmlByIdAsync Tests

        [Test]
        public async Task GetHtmlByIdAsync_ExistingId_ShouldReturnHtml()
        {
            // Act
            var result = await _repository.GetHtmlByIdAsync(_testProject.Id, _cancellationToken);

            // Assert
            Assert.That(result, Is.EqualTo(_testProject.TextHtml));
        }

        [Test]
        public async Task GetHtmlByIdAsync_NonExistentId_ShouldReturnEmptyString()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var result = await _repository.GetHtmlByIdAsync(nonExistentId, _cancellationToken);

            // Assert
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetHtmlByIdAsync_EmptyGuid_ShouldReturnEmptyString()
        {
            // Act
            var result = await _repository.GetHtmlByIdAsync(Guid.Empty, _cancellationToken);

            // Assert
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetHtmlByIdAsync_AfterDeletion_ShouldReturnEmptyString()
        {
            // Arrange
            await _repository.DeleteAsync(_testProject.Id, _cancellationToken);

            // Act
            var result = await _repository.GetHtmlByIdAsync(_testProject.Id, _cancellationToken);

            // Assert
            Assert.That(result, Is.Empty);
        }

        #endregion

        #region GetAllAsync Tests

        [Test]
        public async Task GetAllAsync_WithMultipleProjects_ShouldReturnAllProjects()
        {
            // Arrange
            var project2Result = Projects.Create(
                Guid.NewGuid(),
                "Project 2",
                DateTime.UtcNow,
                "<html>2</html>"
            );
            var project3Result = Projects.Create(
                Guid.NewGuid(),
                "Project 3",
                DateTime.UtcNow,
                "<html>3</html>"
            );

            Assert.That(project2Result.IsSuccess, Is.True);
            Assert.That(project3Result.IsSuccess, Is.True);

            var project2 = project2Result.Value;
            var project3 = project3Result.Value;

            var projectEntity2 = new ProjectEntity
            {
                Id = project2.Id,
                Name = project2.Name,
                DateOpen = project2.DateOpen,
                TextHtml = project2.TextHtml
            };
            var projectEntity3 = new ProjectEntity
            {
                Id = project3.Id,
                Name = project3.Name,
                DateOpen = project3.DateOpen,
                TextHtml = project3.TextHtml
            };

            await _context.Projects.AddRangeAsync(projectEntity2, projectEntity3);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllAsync(_cancellationToken);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(3));
            Assert.That(result.Any(p => p.Name == "Test Project"), Is.True);
            Assert.That(result.Any(p => p.Name == "Project 2"), Is.True);
            Assert.That(result.Any(p => p.Name == "Project 3"), Is.True);
        }

        [Test]
        public async Task GetAllAsync_EmptyDatabase_ShouldReturnEmptyList()
        {
            // Arrange
            _context.Projects.RemoveRange(_context.Projects);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetAllAsync(_cancellationToken);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetAllAsync_ShouldReturnDtoWithCorrectFields()
        {
            // Act
            var result = await _repository.GetAllAsync(_cancellationToken);
            var project = result.First();

            // Assert
            Assert.Multiple(() =>
            {
                Assert.That(project.Id, Is.Not.EqualTo(Guid.Empty));
                Assert.That(project.Name, Is.Not.Null);
                Assert.That(project.DateOpen, Is.Not.EqualTo(default(DateTime)));
            });

            // Проверяем, что TextHtml не включен в DTO
            var propertyInfo = project.GetType().GetProperty("TextHtml");
            Assert.That(propertyInfo, Is.Null);
        }

        #endregion

        #region UpdateHtmlAsync Tests

        [Test]
        public async Task UpdateHtmlAsync_ExistingProject_ShouldUpdateSuccessfully()
        {
            // Arrange
            var updatedProjectResult = Projects.Create(
                _testProject.Id,
                _testProject.Name,
                DateTime.UtcNow.AddDays(1),
                "<html><body>Updated Content</body></html>"
            );

            Assert.That(updatedProjectResult.IsSuccess, Is.True);
            var updatedProject = updatedProjectResult.Value;

            // Act
            var result = await _repository.UpdateHtmlAsync(updatedProject, _cancellationToken);

            // Assert
            Assert.That(result, Is.EqualTo(1));

            var updatedEntity = await _context.Projects.FindAsync(_testProject.Id);
            Assert.Multiple(() =>
            {
                Assert.That(updatedEntity?.TextHtml, Is.EqualTo(updatedProject.TextHtml));
                Assert.That(updatedEntity?.DateOpen, Is.EqualTo(updatedProject.DateOpen));
                Assert.That(updatedEntity?.Name, Is.EqualTo(_testProject.Name)); // Имя не должно измениться
            });
        }

        [Test]
        public async Task UpdateHtmlAsync_NonExistentProject_ShouldReturnZero()
        {
            // Arrange
            var nonExistentProjectResult = Projects.Create(
                Guid.NewGuid(),
                "Non Existent",
                DateTime.UtcNow,
                "<html></html>"
            );

            Assert.That(nonExistentProjectResult.IsSuccess, Is.True);
            var nonExistentProject = nonExistentProjectResult.Value;

            // Act
            var result = await _repository.UpdateHtmlAsync(nonExistentProject, _cancellationToken);

            // Assert
            Assert.That(result, Is.EqualTo(0));
        }

        [Test]
        public async Task UpdateHtmlAsync_UpdateOnlyHtml_ShouldNotChangeOtherFields()
        {
            // Arrange
            var originalName = _testProject.Name;
            var originalDate = _testProject.DateOpen;

            var updatedProjectResult = Projects.Create(
                _testProject.Id,
                "This Should Not Change", // Это имя не должно обновиться
                originalDate,
                "<html>Brand New HTML</html>"
            );

            Assert.That(updatedProjectResult.IsSuccess, Is.True);
            var updatedProject = updatedProjectResult.Value;

            // Act
            await _repository.UpdateHtmlAsync(updatedProject, _cancellationToken);

            // Assert
            var updatedEntity = await _context.Projects.FindAsync(_testProject.Id);
            Assert.Multiple(() =>
            {
                Assert.That(updatedEntity?.Name, Is.EqualTo(originalName));
                Assert.That(updatedEntity?.TextHtml, Is.EqualTo("<html>Brand New HTML</html>"));
                Assert.That(updatedEntity?.DateOpen, Is.EqualTo(originalDate));
            });
        }

        [Test]
        public async Task UpdateHtmlAsync_WithEmptyHtml_ShouldUpdateToEmpty()
        {
            // Arrange
            var updatedProjectResult = Projects.Create(
                _testProject.Id,
                _testProject.Name,
                _testProject.DateOpen,
                ""
            );

            Assert.That(updatedProjectResult.IsSuccess, Is.True);
            var updatedProject = updatedProjectResult.Value;

            // Act
            var result = await _repository.UpdateHtmlAsync(updatedProject, _cancellationToken);

            // Assert
            Assert.That(result, Is.EqualTo(1));

            var updatedEntity = await _context.Projects.FindAsync(_testProject.Id);
            Assert.That(updatedEntity?.TextHtml, Is.EqualTo(""));
        }

        #endregion

        #region UpdateNameAsync Tests

        [Test]
        public async Task UpdateNameAsync_ExistingName_ShouldUpdateSuccessfully()
        {
            // Arrange
            var oldName = "Test Project";
            var newName = "Renamed Project";

            // Act
            var result = await _repository.UpdateNameAsync(oldName, newName, _cancellationToken);

            // Assert
            Assert.That(result, Is.EqualTo(1));

            var updatedProject = await _context.Projects
                .FirstOrDefaultAsync(p => p.Id == _testProject.Id);
            Assert.That(updatedProject?.Name, Is.EqualTo(newName));
        }

        [Test]
        public async Task UpdateNameAsync_NonExistentName_ShouldReturnZero()
        {
            // Arrange
            var oldName = "Non Existent Project";
            var newName = "New Name";

            // Act
            var result = await _repository.UpdateNameAsync(oldName, newName, _cancellationToken);

            // Assert
            Assert.That(result, Is.EqualTo(0));
        }

        [Test]
        public async Task UpdateNameAsync_UpdateToSameName_ShouldWork()
        {
            // Arrange
            var oldName = "Test Project";
            var newName = "Test Project";

            // Act
            var result = await _repository.UpdateNameAsync(oldName, newName, _cancellationToken);

            // Assert
            Assert.That(result, Is.EqualTo(1));
        }

        [Test]
        public async Task UpdateNameAsync_EmptyNewName_ShouldUpdateToEmpty()
        {
            // Arrange
            var oldName = "Test Project";
            var newName = "";

            // Act
            var result = await _repository.UpdateNameAsync(oldName, newName, _cancellationToken);

            // Assert
            Assert.That(result, Is.EqualTo(1));

            var updatedProject = await _context.Projects.FindAsync(_testProject.Id);
            Assert.That(updatedProject?.Name, Is.EqualTo(""));
        }

        [Test]
        public async Task UpdateNameAsync_WithMultipleSameNames_ShouldUpdateAll()
        {
            // Arrange
            var secondProjectResult = Projects.Create(
                Guid.NewGuid(),
                "Test Project", // То же имя
                DateTime.UtcNow,
                "<html>Duplicate</html>"
            );

            Assert.That(secondProjectResult.IsSuccess, Is.True);
            var secondProject = secondProjectResult.Value;

            var secondProjectEntity = new ProjectEntity
            {
                Id = secondProject.Id,
                Name = secondProject.Name,
                DateOpen = secondProject.DateOpen,
                TextHtml = secondProject.TextHtml
            };

            await _context.Projects.AddAsync(secondProjectEntity);
            await _context.SaveChangesAsync();

            var oldName = "Test Project";
            var newName = "Updated All";

            // Act
            var result = await _repository.UpdateNameAsync(oldName, newName, _cancellationToken);

            // Assert
            Assert.That(result, Is.EqualTo(2)); // Должно обновить 2 записи

            var allProjects = await _context.Projects.Where(p => p.Name == newName).ToListAsync();
            Assert.That(allProjects.Count, Is.EqualTo(2));
        }

        #endregion

        #region DeleteAsync Tests

        [Test]
        public async Task DeleteAsync_ExistingId_ShouldDeleteSuccessfully()
        {
            // Act
            var result = await _repository.DeleteAsync(_testProject.Id, _cancellationToken);

            // Assert
            Assert.That(result, Is.EqualTo(1));

            var deletedProject = await _context.Projects.FindAsync(_testProject.Id);
            Assert.That(deletedProject, Is.Null);
        }

        [Test]
        public async Task DeleteAsync_NonExistentId_ShouldReturnZero()
        {
            // Arrange
            var nonExistentId = Guid.NewGuid();

            // Act
            var result = await _repository.DeleteAsync(nonExistentId, _cancellationToken);

            // Assert
            Assert.That(result, Is.EqualTo(0));
        }

        [Test]
        public async Task DeleteAsync_EmptyGuid_ShouldReturnZero()
        {
            // Act
            var result = await _repository.DeleteAsync(Guid.Empty, _cancellationToken);

            // Assert
            Assert.That(result, Is.EqualTo(0));
        }

        [Test]
        public async Task DeleteAsync_AfterDeletion_ShouldNotAffectOtherRecords()
        {
            // Arrange
            var otherProjectResult = Projects.Create(
                Guid.NewGuid(),
                "Other Project",
                DateTime.UtcNow,
                "<html>Other</html>"
            );

            Assert.That(otherProjectResult.IsSuccess, Is.True);
            var otherProject = otherProjectResult.Value;

            var otherProjectEntity = new ProjectEntity
            {
                Id = otherProject.Id,
                Name = otherProject.Name,
                DateOpen = otherProject.DateOpen,
                TextHtml = otherProject.TextHtml
            };

            await _context.Projects.AddAsync(otherProjectEntity);
            await _context.SaveChangesAsync();

            var initialCount = await _context.Projects.CountAsync();

            // Act
            await _repository.DeleteAsync(_testProject.Id, _cancellationToken);

            // Assert
            var finalCount = await _context.Projects.CountAsync();
            Assert.That(finalCount, Is.EqualTo(initialCount - 1));

            var remainingProject = await _context.Projects.FindAsync(otherProject.Id);
            Assert.That(remainingProject, Is.Not.Null);
        }

        #endregion

        #region CheckNameAsync Tests

        [Test]
        public async Task CheckNameAsync_ExistingName_ShouldReturnTrue()
        {
            // Act
            var result = await _repository.CheckNameAsync("Test Project", _cancellationToken);

            // Assert
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task CheckNameAsync_NonExistentName_ShouldReturnFalse()
        {
            // Act
            var result = await _repository.CheckNameAsync("Non Existent Project", _cancellationToken);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task CheckNameAsync_EmptyName_ShouldReturnFalse()
        {
            // Act
            var result = await _repository.CheckNameAsync("", _cancellationToken);

            // Assert
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task CheckNameAsync_CaseSensitive_ShouldBeCaseSensitive()
        {
            // Act
            var resultLowerCase = await _repository.CheckNameAsync("test project", _cancellationToken);
            var resultUpperCase = await _repository.CheckNameAsync("TEST PROJECT", _cancellationToken);

            // Assert
            Assert.That(resultLowerCase, Is.False);
            Assert.That(resultUpperCase, Is.False);
        }

        [Test]
        public async Task CheckNameAsync_AfterNameUpdate_ShouldReturnTrueForNewName()
        {
            // Arrange
            var newName = "Updated Name";
            await _repository.UpdateNameAsync("Test Project", newName, _cancellationToken);

            // Act
            var resultOld = await _repository.CheckNameAsync("Test Project", _cancellationToken);
            var resultNew = await _repository.CheckNameAsync(newName, _cancellationToken);

            // Assert
            Assert.That(resultOld, Is.False);
            Assert.That(resultNew, Is.True);
        }

        #endregion
    }
}
