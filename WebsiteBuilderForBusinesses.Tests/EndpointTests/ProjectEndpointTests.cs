using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using WebsiteBuilderForBusinesses.API;
using WebsiteBuilderForBusinesses.API.Requests;
using WebsiteBuilderForBusinesses.Core.Abstractions;
using WebsiteBuilderForBusinesses.Core.Models;
using WebsiteBuilderForBusinesses.DataAccess.Postgres;
using WebsiteBuilderForBusinesses.DataAccess.Postgres.Models;

namespace WebsiteBuilderForBusinesses.Tests.EndpointTests
{
    public class ProjectEndpointTests
    {
        private CustomWebApplicationFactory _factory = null!;
        private HttpClient _client = null!;

        [SetUp]
        public async Task SetUp()
        {
            _factory = new CustomWebApplicationFactory();
            _client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WebBuilderDbContext>();
            var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasherService>();
            dbContext.Users.RemoveRange(dbContext.Users);
            dbContext.Projects.RemoveRange(dbContext.Projects);
            await dbContext.SaveChangesAsync();
            var userResult = Users.Create(
                id: Guid.NewGuid(),
                login: "user@test.com",
                password: "user123",
                role: "user",
                passwordHasherService: passwordHasher
            );
            Assert.That(userResult.IsSuccess, Is.True);
            var user = userResult.Value;
            await dbContext.Users.AddAsync(new UsersEntity
            {
                Id = user.Id,
                Login = user.Login,
                HashPassword = user.HashPassword,
                Role = user.Role
            });
            await dbContext.SaveChangesAsync();
            var loginResponse = await _client.PostAsJsonAsync("/login", new
            {
                Login = "user@test.com",
                Password = "user123"
            });
            var userCookie = loginResponse.Headers.GetValues("Set-Cookie").First();
            _client.DefaultRequestHeaders.Add("Cookie", userCookie);
        }

        [TearDown]
        public async Task TearDown()
        {
            _client?.Dispose();
            if (_factory != null)
            {
                await _factory.DisposeAsync();
            }
        }

        [Test]
        public async Task GetAllProjects_ReturnsEmptyList_WhenNoProjects()
        {
            var response = await _client.GetAsync("/project/all");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var projects = await response.Content.ReadFromJsonAsync<List<Projects>>();
            Assert.That(projects, Is.Empty);
        }

        [Test]
        public async Task GetAllProjects_ReturnsAllProjectsFromDatabase()
        {
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WebBuilderDbContext>();

            var project1Result = Projects.Create(
                id: Guid.NewGuid(),
                name: "Project 1",
                dateOpen: DateTime.UtcNow,
                textHtml: "<html>1</html>"
            );

            var project2Result = Projects.Create(
                id: Guid.NewGuid(),
                name: "Project 2",
                dateOpen: DateTime.UtcNow,
                textHtml: "<html>2</html>"
            );

            Assert.That(project1Result.IsSuccess, Is.True);
            Assert.That(project2Result.IsSuccess, Is.True);

            var project1 = project1Result.Value;
            var project2 = project2Result.Value;

            await dbContext.Projects.AddRangeAsync(
                new ProjectEntity { Id = project1.Id, Name = project1.Name, TextHtml = project1.TextHtml, DateOpen = project1.DateOpen },
                new ProjectEntity { Id = project2.Id, Name = project2.Name, TextHtml = project2.TextHtml, DateOpen = project2.DateOpen }
            );
            await dbContext.SaveChangesAsync();

            var response = await _client.GetAsync("/project/all");

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var result = await response.Content.ReadFromJsonAsync<List<Projects>>();
            Assert.That(result, Has.Count.EqualTo(2));
            Assert.That(result!.Select(p => p.Name), Contains.Item("Project 1"));
        }

        [Test]
        public async Task GetProjectHtmlById_ReturnsHtml_WhenProjectExists()
        {
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WebBuilderDbContext>();

            var projectId = Guid.NewGuid();
            var projectResult = Projects.Create(
                id: projectId,
                name: "Test Project",
                dateOpen: DateTime.UtcNow,
                textHtml: "<html><body><h1>Hello World</h1></body></html>"
            );

            Assert.That(projectResult.IsSuccess, Is.True);
            var project = projectResult.Value;

            await dbContext.Projects.AddAsync(new ProjectEntity
            {
                Id = project.Id,
                Name = project.Name,
                TextHtml = project.TextHtml,
                DateOpen = project.DateOpen
            });
            await dbContext.SaveChangesAsync();

            var request = new IdRequest { Id = projectId };
            var response = await _client.PostAsJsonAsync("/project/html", request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var html = await response.Content.ReadAsStringAsync();
            Assert.That(html, Is.EqualTo("<html><body><h1>Hello World</h1></body></html>"));
        }

        [Test]
        public async Task DeleteProject_ReturnsOk_WhenProjectExists()
        {
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WebBuilderDbContext>();

            var projectId = Guid.NewGuid();
            var projectResult = Projects.Create(
                id: projectId,
                name: "To Delete",
                dateOpen: DateTime.UtcNow,
                textHtml: "<html></html>"
            );

            Assert.That(projectResult.IsSuccess, Is.True);
            var project = projectResult.Value;

            await dbContext.Projects.AddAsync(new ProjectEntity
            {
                Id = project.Id,
                Name = project.Name,
                TextHtml = project.TextHtml,
                DateOpen = project.DateOpen
            });
            await dbContext.SaveChangesAsync();

            var request = new IdRequest { Id = projectId };
            var httpRequest = new HttpRequestMessage(HttpMethod.Delete, "/project/html")
            {
                Content = JsonContent.Create(request)
            };

            var response = await _client.SendAsync(httpRequest);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            using var verifyScope = _factory.Services.CreateScope();
            var verifyDbContext = verifyScope.ServiceProvider.GetRequiredService<WebBuilderDbContext>();
            var deletedProject = await verifyDbContext.Projects.FindAsync(projectId);
            Assert.That(deletedProject, Is.Null);
        }

        [Test]
        public async Task UpdateProjectHtml_ReturnsOk_AndUpdatesDatabase()
        {
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WebBuilderDbContext>();

            var projectId = Guid.NewGuid();
            var projectResult = Projects.Create(
                id: projectId,
                name: "Test Project",
                dateOpen: DateTime.UtcNow,
                textHtml: "<html>Old content</html>"
            );

            Assert.That(projectResult.IsSuccess, Is.True);
            var project = projectResult.Value;

            await dbContext.Projects.AddAsync(new ProjectEntity
            {
                Id = project.Id,
                Name = project.Name,
                TextHtml = project.TextHtml,
                DateOpen = project.DateOpen
            });
            await dbContext.SaveChangesAsync();

            var request = new ProjectUpdateRequest
            {
                Id = projectId,
                Name = "Test Project",
                TextHtml = "<html>New updated content!</html>"
            };

            var response = await _client.PostAsJsonAsync("/project/html/update", request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            using var verifyScope = _factory.Services.CreateScope();
            var verifyDbContext = verifyScope.ServiceProvider.GetRequiredService<WebBuilderDbContext>();
            var updatedProjectEntity = await verifyDbContext.Projects.FindAsync(projectId);
            Assert.That(updatedProjectEntity!.TextHtml, Is.EqualTo("<html>New updated content!</html>"));
        }

        [Test]
        public async Task UpdateProjectName_ReturnsOk_AndUpdatesDatabase()
        {
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WebBuilderDbContext>();

            var projectResult = Projects.Create(
                id: Guid.NewGuid(),
                name: "Old Name",
                dateOpen: DateTime.UtcNow,
                textHtml: "<html></html>"
            );

            Assert.That(projectResult.IsSuccess, Is.True);
            var project = projectResult.Value;

            await dbContext.Projects.AddAsync(new ProjectEntity
            {
                Id = project.Id,
                Name = project.Name,
                TextHtml = project.TextHtml,
                DateOpen = project.DateOpen
            });
            await dbContext.SaveChangesAsync();

            var request = new ProjectNameUpdateRequest
            {
                OldName = "Old Name",
                NewName = "Brand New Name"
            };

            var response = await _client.PostAsJsonAsync("/project/name/update", request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task CreateNewProject_ReturnsOk_AndSavesToDatabase()
        {
            var request = new ProjectCreateRequest { Name = "My Awesome Project" };

            var response = await _client.PostAsJsonAsync("/project/new", request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task CreateNewProject_ReturnsBadRequest_WhenProjectNameAlreadyExists()
        {
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WebBuilderDbContext>();

            var existingProjectResult = Projects.Create(
                id: Guid.NewGuid(),
                name: "Duplicate Name",
                dateOpen: DateTime.UtcNow,
                textHtml: "<html></html>"
            );

            Assert.That(existingProjectResult.IsSuccess, Is.True);
            var existingProject = existingProjectResult.Value;

            await dbContext.Projects.AddAsync(new ProjectEntity
            {
                Id = existingProject.Id,
                Name = existingProject.Name,
                TextHtml = existingProject.TextHtml,
                DateOpen = existingProject.DateOpen
            });
            await dbContext.SaveChangesAsync();

            var request = new ProjectCreateRequest { Name = "Duplicate Name" };

            var response = await _client.PostAsJsonAsync("/project/new", request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            var error = await response.Content.ReadAsStringAsync();
            Assert.That(error, Is.EqualTo("Проект с таким названием уже существует"));
        }
    }
}
