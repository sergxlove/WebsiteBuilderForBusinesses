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
    public class AdminEndpointTests
    {
        private CustomWebApplicationFactory _factory = null!;
        private HttpClient _client = null!;
        private string _adminCookie = null!;

        [SetUp]
        public async Task SetUp()
        {
            _factory = new CustomWebApplicationFactory();
            _client = _factory.CreateClient();
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WebBuilderDbContext>();
            var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasherService>();
            dbContext.Users.RemoveRange(dbContext.Users);
            await dbContext.SaveChangesAsync();
            var adminResult = Users.Create(
                id: Guid.NewGuid(),
                login: "admin@test.com",
                password: "admin123",
                role: "admin",
                passwordHasherService: passwordHasher
            );
            Assert.That(adminResult.IsSuccess, Is.True);
            var admin = adminResult.Value;
            var adminEntity = new UsersEntity
            {
                Id = admin.Id,
                Login = admin.Login,
                HashPassword = admin.HashPassword,
                Role = admin.Role
            };
            await dbContext.Users.AddAsync(adminEntity);
            await dbContext.SaveChangesAsync();
            var loginResponse = await _client.PostAsJsonAsync("/login", new
            {
                Login = "admin@test.com",
                Password = "admin123"
            });
            _adminCookie = loginResponse.Headers.GetValues("Set-Cookie").First();
            _client.DefaultRequestHeaders.Add("Cookie", _adminCookie);
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
        public async Task GetUsersAll_ReturnsAllUsers_WhenAdminAuthorized()
        {
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WebBuilderDbContext>();
            var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasherService>();
            var user1Result = Users.Create(
                id: Guid.NewGuid(),
                login: "user1@test.com",
                password: "pass1",
                role: "user",
                passwordHasherService: passwordHasher
            );
            var user2Result = Users.Create(
                id: Guid.NewGuid(),
                login: "user2@test.com",
                password: "pass2",
                role: "user",
                passwordHasherService: passwordHasher
            );
            Assert.That(user1Result.IsSuccess, Is.True);
            Assert.That(user2Result.IsSuccess, Is.True);
            var user1 = user1Result.Value;
            var user2 = user2Result.Value;
            await dbContext.Users.AddRangeAsync(
                new UsersEntity { Id = user1.Id, Login = user1.Login, HashPassword = user1.HashPassword, Role = user1.Role },
                new UsersEntity { Id = user2.Id, Login = user2.Login, HashPassword = user2.HashPassword, Role = user2.Role }
            );
            await dbContext.SaveChangesAsync();
            var response = await _client.GetAsync("/users/all");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var users = await response.Content.ReadFromJsonAsync<List<Users>>();
            Assert.That(users, Has.Count.EqualTo(3)); // admin + 2 users
            Assert.That(users!.Select(u => u.Login), Contains.Item("admin@test.com"));
            Assert.That(users.Select(u => u.Login), Contains.Item("user1@test.com"));
        }

        [Test]
        public async Task UpdateUserPassword_ReturnsOk_WhenValidRequest()
        {
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WebBuilderDbContext>();
            var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasherService>();
            var userId = Guid.NewGuid();
            var userResult = Users.Create(
                id: userId,
                login: "target@test.com",
                password: "oldPass123",
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
            var request = new PasswordUpdateRequest
            {
                Id = userId,
                Login = "target@test.com",
                NewPassword = "newPass456"
            };
            var response = await _client.PostAsJsonAsync("/users/password/update", request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            using var verifyScope = _factory.Services.CreateScope();
            var verifyDbContext = verifyScope.ServiceProvider.GetRequiredService<WebBuilderDbContext>();
            var updatedUserEntity = await verifyDbContext.Users.FindAsync(userId);
            Assert.That(Users.VerifyPassword("newPass456", updatedUserEntity!.HashPassword), Is.True);
        }

        [Test]
        public async Task UpdateUserPassword_ReturnsBadRequest_WhenUserNotFound()
        {
            var request = new PasswordUpdateRequest
            {
                Id = Guid.NewGuid(),
                Login = "nonexistent@test.com",
                NewPassword = "newPass"
            };
            var response = await _client.PostAsJsonAsync("/users/password/update", request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            var error = await response.Content.ReadAsStringAsync();
            Assert.That(error, Is.EqualTo("Не удалось обновить данные пользователя"));
        }

        [Test]
        public async Task UpdateUserRole_ReturnsOk_WhenValidRequest()
        {
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WebBuilderDbContext>();
            var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasherService>();
            var userId = Guid.NewGuid();
            var userResult = Users.Create(
                id: userId,
                login: "ordinary@test.com",
                password: "pass123",
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
            var request = new RoleUpdateRequest
            {
                Id = userId,
                Login = "ordinary@test.com",
                NewRole = "admin"
            };
            var response = await _client.PostAsJsonAsync("/users/role/update", request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            using var verifyScope = _factory.Services.CreateScope();
            var verifyDbContext = verifyScope.ServiceProvider.GetRequiredService<WebBuilderDbContext>();
            var updatedUserEntity = await verifyDbContext.Users.FindAsync(userId);
            Assert.That(updatedUserEntity!.Role, Is.EqualTo("admin"));
        }

        [Test]
        public async Task DeleteUser_ReturnsOk_WhenUserExists()
        {
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WebBuilderDbContext>();
            var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasherService>();
            var userId = Guid.NewGuid();
            var userResult = Users.Create(
                id: userId,
                login: "todelete@test.com",
                password: "pass123",
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
            var request = new IdRequest { Id = userId };
            var httpRequest = new HttpRequestMessage(HttpMethod.Delete, "/users")
            {
                Content = JsonContent.Create(request)
            };
            var response = await _client.SendAsync(httpRequest);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            using var verifyScope = _factory.Services.CreateScope();
            var verifyDbContext = verifyScope.ServiceProvider.GetRequiredService<WebBuilderDbContext>();
            var deletedUser = await verifyDbContext.Users.FindAsync(userId);
            Assert.That(deletedUser, Is.Null);
        }
        [Test]
        public async Task DeleteUser_ReturnsBadRequest_WhenUserDoesNotExist()
        {
            var request = new IdRequest { Id = Guid.NewGuid() };
            var httpRequest = new HttpRequestMessage(HttpMethod.Delete, "/users")
            {
                Content = JsonContent.Create(request)
            };
            var response = await _client.SendAsync(httpRequest);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            var error = await response.Content.ReadAsStringAsync();
            Assert.That(error, Is.EqualTo("Произошла ошибка при удалении"));
        }
    }
}
