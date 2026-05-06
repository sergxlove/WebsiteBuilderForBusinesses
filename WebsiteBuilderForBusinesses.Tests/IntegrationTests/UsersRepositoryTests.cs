using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WebsiteBuilderForBusinesses.Core.Models;
using WebsiteBuilderForBusinesses.Core.Services;
using WebsiteBuilderForBusinesses.DataAccess.Postgres;
using WebsiteBuilderForBusinesses.DataAccess.Postgres.Repositories;

namespace WebsiteBuilderForBusinesses.Tests.IntegrationTests
{
    public class UsersRepositoryTests
    {
        private WebBuilderDbContext _context;
        private UsersRepository _repository;
        private CancellationToken _cancellationToken;
        private PasswordHasherService _passwordHasher;
        private SqliteConnection _connection;

        [SetUp]
        public void SetUp()
        {
            _connection = new SqliteConnection("Filename=:memory:");
            _connection.Open();

            var options = new DbContextOptionsBuilder<WebBuilderDbContext>()
                .UseSqlite(_connection)
                .Options;

            _context = new WebBuilderDbContext(options);
            _context.Database.EnsureCreated();

            _repository = new UsersRepository(_context);
            _cancellationToken = CancellationToken.None;
            _passwordHasher = new PasswordHasherService();
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
            _connection.Dispose();
        }

        private Users CreateValidUser(string login = "testuser", string password = "TestPassword123!")
        {
            var result = Users.Create(
                Guid.NewGuid(),
                login,
                password,
                "user",
                _passwordHasher
            );
            return result.Value;
        }

        [Test]
        public async Task CreateAsync_ValidUser_ReturnsUserId()
        {
            var user = CreateValidUser();
            var result = await _repository.CreateAsync(user, _cancellationToken);
            Assert.That(result, Is.EqualTo(user.Id));
            var savedUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
            Assert.That(savedUser, Is.Not.Null);
            Assert.Multiple(() =>
            {
                Assert.That(savedUser.Login, Is.EqualTo(user.Login));
                Assert.That(savedUser.Role, Is.EqualTo(user.Role));
            });
        }

        [Test]
        public async Task VerifyAsync_ValidCredentials_ReturnsTrue()
        {
            var password = "TestPassword123!";
            var user = CreateValidUser("testuser", password);
            await _repository.CreateAsync(user, _cancellationToken);
            var result = await _repository.VerifyAsync("testuser", password);
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task VerifyAsync_InvalidPassword_ReturnsFalse()
        {
            var password = "TestPassword123!";
            var user = CreateValidUser("testuser", password);
            await _repository.CreateAsync(user, _cancellationToken);
            var result = await _repository.VerifyAsync("testuser", "WrongPassword");
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task VerifyAsync_NonExistingUser_ReturnsFalse()
        {
            var result = await _repository.VerifyAsync("nonexisting", "password");
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task CheckAsync_ExistingLogin_ReturnsTrue()
        {
            var user = CreateValidUser("existinguser");
            await _repository.CreateAsync(user, _cancellationToken);
            var result = await _repository.CheckAsync("existinguser", _cancellationToken);
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task CheckAsync_NonExistingLogin_ReturnsFalse()
        {
            var result = await _repository.CheckAsync("nonexisting", _cancellationToken);
            Assert.That(result, Is.False);
        }

        [Test]
        public async Task GetRoleAsync_ExistingUser_ReturnsRole()
        {
            var user = CreateValidUser("adminuser");
            var userWithRole = Users.Create(
                user.Id,
                "adminuser",
                "password",
                "admin",
                _passwordHasher
            ).Value;
            await _repository.CreateAsync(userWithRole, _cancellationToken);
            var result = await _repository.GetRoleAsync("adminuser", _cancellationToken);
            Assert.That(result, Is.EqualTo("admin"));
        }

        [Test]
        public async Task GetRoleAsync_NonExistingUser_ReturnsDefaultRole()
        {
            var result = await _repository.GetRoleAsync("nonexisting", _cancellationToken);
            Assert.That(result, Is.EqualTo(string.Empty));
        }

        [Test]
        public async Task UpdatePasswordAsync_ExistingUser_UpdatesPassword()
        {
            var oldPassword = "oldpassword123";
            var newPassword = "newpassword456";
            var user = CreateValidUser("testuser", oldPassword);
            await _repository.CreateAsync(user, _cancellationToken);
            var oldPasswordValid = await _repository.VerifyAsync("testuser", oldPassword);
            Assert.That(oldPasswordValid, Is.True);
            var updatedUser = Users.Create(
                user.Id,
                "testuser",
                newPassword,
                "user",
                _passwordHasher
            ).Value;
            var result = await _repository.UpdatePasswordAsync(updatedUser, _cancellationToken);
            Assert.That(result, Is.EqualTo(1));
        }

        [Test]
        public async Task UpdatePasswordAsync_NonExistingUser_ReturnsZero()
        {
            var user = CreateValidUser("nonexisting");
            var result = await _repository.UpdatePasswordAsync(user, _cancellationToken);
            Assert.That(result, Is.EqualTo(0));
        }

        [Test]
        public async Task UpdateRoleAsync_ExistingUser_UpdatesRole()
        {
            var user = CreateValidUser("testuser");
            await _repository.CreateAsync(user, _cancellationToken);
            var updatedUser = Users.Create(
                user.Id,
                "testuser",
                "password",
                "admin",
                _passwordHasher
            ).Value;
            var result = await _repository.UpdateRoleAsync(updatedUser, _cancellationToken);
            Assert.That(result, Is.EqualTo(1));
        }

        [Test]
        public async Task UpdateRoleAsync_NonExistingUser_ReturnsZero()
        {
            var user = CreateValidUser("nonexisting");
            var result = await _repository.UpdateRoleAsync(user, _cancellationToken);
            Assert.That(result, Is.EqualTo(0));
        }

        [Test]
        public async Task GetAllAsync_MultipleUsers_ReturnsShortUserDtoList()
        {
            var user1 = CreateValidUser("user1");
            var user2 = CreateValidUser("user2");
            var user3 = CreateValidUser("user3");
            await _repository.CreateAsync(user1, _cancellationToken);
            await _repository.CreateAsync(user2, _cancellationToken);
            await _repository.CreateAsync(user3, _cancellationToken);
            var result = await _repository.GetAllAsync(_cancellationToken);
            Assert.Multiple(() =>
            {
                Assert.That(result, Is.Not.Null);
                Assert.That(result.All(r => r.Id != Guid.Empty), Is.True);
                Assert.That(result.All(r => !string.IsNullOrEmpty(r.Login)), Is.True);
                Assert.That(result.All(r => !string.IsNullOrEmpty(r.Role)), Is.True);
                Assert.That(result.Select(r => r.Login), Contains.Item("user1"));
                Assert.That(result.Select(r => r.Login), Contains.Item("user2"));
                Assert.That(result.Select(r => r.Login), Contains.Item("user3"));
            });
        }

        [Test]
        public async Task DeleteAsync_ExistingUser_DeletesUser()
        {
            var user = CreateValidUser("testuser");
            await _repository.CreateAsync(user, _cancellationToken);
            var result = await _repository.DeleteAsync(user.Id, _cancellationToken);
            Assert.That(result, Is.EqualTo(1));
            var exists = await _repository.CheckAsync("testuser", _cancellationToken);
            Assert.That(exists, Is.False);
        }

        [Test]
        public async Task DeleteAsync_NonExistingUser_ReturnsZero()
        {
            var result = await _repository.DeleteAsync(Guid.NewGuid(), _cancellationToken);
            Assert.That(result, Is.EqualTo(0));
        }

        [Test]
        public async Task DeleteAsync_DeleteUser_ShouldNotAffectOtherUsers()
        {
            var user1 = CreateValidUser("user1");
            var user2 = CreateValidUser("user2");
            await _repository.CreateAsync(user1, _cancellationToken);
            await _repository.CreateAsync(user2, _cancellationToken);
            var result = await _repository.DeleteAsync(user1.Id, _cancellationToken);
            Assert.That(result, Is.EqualTo(1));
        }

        [Test]
        public void CreateUser_WithInvalidParameters_ShouldReturnFailure()
        {
            var resultWithEmptyId = Users.Create(
                Guid.Empty,
                "login",
                "password",
                "user",
                _passwordHasher
            );
            var resultWithEmptyLogin = Users.Create(
                Guid.NewGuid(),
                string.Empty,
                "password",
                "user",
                _passwordHasher
            );
            var resultWithEmptyPassword = Users.Create(
                Guid.NewGuid(),
                "login",
                string.Empty,
                "user",
                _passwordHasher
            );
            var resultWithEmptyRole = Users.Create(
                Guid.NewGuid(),
                "login",
                "password",
                string.Empty,
                _passwordHasher
            );
            Assert.Multiple(() =>
            {
                Assert.That(resultWithEmptyId.IsSuccess, Is.False);
                Assert.That(resultWithEmptyId.Error, Is.EqualTo("Поле Id не должно быть пустым"));
                Assert.That(resultWithEmptyLogin.IsSuccess, Is.False);
                Assert.That(resultWithEmptyLogin.Error, Is.EqualTo("Поле Имя не должно быть пустым"));
                Assert.That(resultWithEmptyPassword.IsSuccess, Is.False);
                Assert.That(resultWithEmptyPassword.Error, Is.EqualTo("Поле Пароль не должно быть пустым"));
                Assert.That(resultWithEmptyRole.IsSuccess, Is.False);
                Assert.That(resultWithEmptyRole.Error, Is.EqualTo("Поле Роль не должно быть пустым"));
            });
        }

        [Test]
        public async Task VerifyAsync_WithSpecialCharactersInPassword_WorksCorrectly()
        {
            var specialPassword = "P@ssw0rd!@#$%^&*()_+";
            var user = CreateValidUser("specialuser", specialPassword);
            await _repository.CreateAsync(user, _cancellationToken);
            var result = await _repository.VerifyAsync("specialuser", specialPassword);
            Assert.That(result, Is.True);
        }

        [Test]
        public async Task CreateAsync_UserWithLongLogin_WorksCorrectly()
        {
            var longLogin = new string('a', 100);
            var user = CreateValidUser(longLogin);
            var result = await _repository.CreateAsync(user, _cancellationToken);
            Assert.That(result, Is.EqualTo(user.Id));
            var savedUser = await _context.Users.FirstOrDefaultAsync(u => u.Login == longLogin);
            Assert.That(savedUser, Is.Not.Null);
        }
    }
}
