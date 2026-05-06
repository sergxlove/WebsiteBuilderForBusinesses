using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using WebsiteBuilderForBusinesses.API;
using WebsiteBuilderForBusinesses.Core.Abstractions;
using WebsiteBuilderForBusinesses.Core.Models;
using WebsiteBuilderForBusinesses.DataAccess.Postgres;
using WebsiteBuilderForBusinesses.DataAccess.Postgres.Models;

namespace WebsiteBuilderForBusinesses.Tests.EndpointTests
{
    public class LoginEndpointTests
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
            await dbContext.SaveChangesAsync();
            var userResult = Users.Create(
                id: Guid.NewGuid(),
                login: "testuser@test.com",
                password: "validPass123",
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
        public async Task Login_ReturnsOkAndSetsCookie_WhenCredentialsValid()
        {
            var request = new { Login = "testuser@test.com", Password = "validPass123" };
            var response = await _client.PostAsJsonAsync("/login", request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var cookies = response.Headers.GetValues("Set-Cookie").ToList();
            Assert.That(cookies.Any(c => c.Contains("jwt=")), Is.True);
        }

        [Test]
        public async Task Login_ReturnsBadRequest_WhenLoginEmpty()
        {
            var request = new { Login = "", Password = "pass123" };
            var response = await _client.PostAsJsonAsync("/login", request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            var error = await response.Content.ReadAsStringAsync();
            Assert.That(error, Is.EqualTo("Пустые значения логин или пароль"));
        }

        [Test]
        public async Task Login_ReturnsBadRequest_WhenPasswordEmpty()
        {
            var request = new { Login = "testuser@test.com", Password = "" };
            var response = await _client.PostAsJsonAsync("/login", request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Test]
        public async Task Login_ReturnsBadRequest_WhenCredentialsInvalid()
        {
            var request = new { Login = "testuser@test.com", Password = "wrongPassword" };
            var response = await _client.PostAsJsonAsync("/login", request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            var error = await response.Content.ReadAsStringAsync();
            Assert.That(error, Is.EqualTo("Неверный логин или пароль"));
        }

        [Test]
        public async Task Register_ReturnsOk_WhenValidAndAdminAuthorized()
        {
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WebBuilderDbContext>();
            var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasherService>();
            var adminResult = Users.Create(
                id: Guid.NewGuid(),
                login: "admin@test.com",
                password: "admin123",
                role: "admin",
                passwordHasherService: passwordHasher
            );
            Assert.That(adminResult.IsSuccess, Is.True);
            var admin = adminResult.Value;
            await dbContext.Users.AddAsync(new UsersEntity
            {
                Id = admin.Id,
                Login = admin.Login,
                HashPassword = admin.HashPassword,
                Role = admin.Role
            });
            await dbContext.SaveChangesAsync();
            var loginResponse = await _client.PostAsJsonAsync("/login", new
            {
                Login = "admin@test.com",
                Password = "admin123"
            });
            var adminCookie = loginResponse.Headers.GetValues("Set-Cookie").First();
            _client.DefaultRequestHeaders.Add("Cookie", adminCookie);
            var newUser = new
            {
                Login = "newuser@test.com",
                Password = "newPass123",
                AgainPassword = "newPass123",
                Role = "user"
            };
            var response = await _client.PostAsJsonAsync("/reg", newUser);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
        }

        [Test]
        public async Task Register_ReturnsBadRequest_WhenPasswordsDoNotMatch()
        {
            using var scope = _factory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<WebBuilderDbContext>();
            var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasherService>();
            var adminResult = Users.Create(
                id: Guid.NewGuid(),
                login: "admin2@test.com",
                password: "admin123",
                role: "admin",
                passwordHasherService: passwordHasher
            );
            Assert.That(adminResult.IsSuccess, Is.True);
            var admin = adminResult.Value;
            await dbContext.Users.AddAsync(new UsersEntity
            {
                Id = admin.Id,
                Login = admin.Login,
                HashPassword = admin.HashPassword,
                Role = admin.Role
            });
            await dbContext.SaveChangesAsync();
            var loginResponse = await _client.PostAsJsonAsync("/login", new
            {
                Login = "admin2@test.com",
                Password = "admin123"
            });
            var adminCookie = loginResponse.Headers.GetValues("Set-Cookie").First();
            _client.DefaultRequestHeaders.Add("Cookie", adminCookie);
            var newUser = new
            {
                Login = "newuser2@test.com",
                Password = "pass123",
                AgainPassword = "different",
                Role = "user"
            };
            var response = await _client.PostAsJsonAsync("/reg", newUser);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            var error = await response.Content.ReadAsStringAsync();
            Assert.That(error, Is.EqualTo("Пароли не совпадают"));
        }

        [Test]
        public async Task Logout_ReturnsOkAndDeletesCookie()
        {
            var loginResponse = await _client.PostAsJsonAsync("/login", new
            {
                Login = "testuser@test.com",
                Password = "validPass123"
            });
            var cookie = loginResponse.Headers.GetValues("Set-Cookie").First();
            _client.DefaultRequestHeaders.Add("Cookie", cookie);
            var response = await _client.GetAsync("/logout");
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var cookies = response.Headers.GetValues("Set-Cookie").ToList();
            Assert.That(cookies.Any(c => c.Contains("jwt=") && c.Contains("expires=")), Is.True);
        }
    }
}
