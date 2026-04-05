using WebsiteBuilderForBusinesses.Core.Models;
using WebsiteBuilderForBusinesses.Core.Services;

namespace WebsiteBuilderForBusinesses.Tests.UnitTests
{
    public class UsersTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Create_WithValidData_ReturnsSuccess()
        {
            var id = Guid.NewGuid();
            var login = "testuser";
            var password = "SecurePass123!";
            var role = "Admin";
            var hasherService = new PasswordHasherService();

            var result = Users.Create(id, login, password, role, hasherService);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Value, Is.Not.Null);
            });
        }

        [Test]
        public void Create_WithValidData_PropertiesAreSetCorrectly()
        {
            var id = Guid.NewGuid();
            var login = "john_doe";
            var password = "MyPassword123";
            var role = "User";
            var hasherService = new PasswordHasherService();

            var result = Users.Create(id, login, password, role, hasherService);
            var user = result.Value;

            Assert.Multiple(() =>
            {
                Assert.That(user.Id, Is.EqualTo(id));
                Assert.That(user.Login, Is.EqualTo(login));
                Assert.That(user.Role, Is.EqualTo(role));
                Assert.That(user.HashPassword, Is.Not.EqualTo(password));
                Assert.That(user.HashPassword, Is.Not.Null.Or.Empty);
            });
        }

        [Test]
        public void Create_PasswordIsHashed_NotStoredInPlainText()
        {
            var id = Guid.NewGuid();
            var login = "user";
            var password = "PlainPassword";
            var role = "User";
            var hasherService = new PasswordHasherService();

            var result = Users.Create(id, login, password, role, hasherService);

            Assert.Multiple(() =>
            {
                Assert.That(result.Value.HashPassword, Is.Not.EqualTo(password));
                Assert.That(result.Value.HashPassword.Length, Is.GreaterThan(password.Length));
            });
        }

        public void Create_WithEmptyGuid_ReturnsFailure()
        {
            var id = Guid.Empty;
            var login = "user";
            var password = "Password123";
            var role = "User";
            var hasherService = new PasswordHasherService();

            var result = Users.Create(id, login, password, role, hasherService);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Error, Is.EqualTo("Поле Id не должно быть пустым"));
                Assert.That(result.Value, Is.Null);
            });
        }

        [Test]
        public void Create_WithEmptyLogin_ReturnsFailure()
        {
            var id = Guid.NewGuid();
            var login = string.Empty;
            var password = "Password123";
            var role = "User";
            var hasherService = new PasswordHasherService();

            var result = Users.Create(id, login, password, role, hasherService);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Error, Is.EqualTo("Поле Имя не должно быть пустым"));
            });
        }

        [Test]
        public void Create_WithValidLoginWithSpecialChars_ReturnsSuccess()
        {
            var id = Guid.NewGuid();
            var login = "user@test.com";
            var password = "Password123";
            var role = "User";
            var hasherService = new PasswordHasherService();

            var result = Users.Create(id, login, password, role, hasherService);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Value.Login, Is.EqualTo("user@test.com"));
            });
        }

        [Test]
        public void Create_WithEmptyPassword_ReturnsFailure()
        {
            var id = Guid.NewGuid();
            var login = "user";
            var password = string.Empty;
            var role = "User";
            var hasherService = new PasswordHasherService();

            var result = Users.Create(id, login, password, role, hasherService);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Error, Is.EqualTo("Поле Пароль не должно быть пустым"));
            });
        }

        [Test]
        public void Create_WithEmptyRole_ReturnsFailure()
        {
            var id = Guid.NewGuid();
            var login = "user";
            var password = "Password123";
            var role = string.Empty;
            var hasherService = new PasswordHasherService();

            var result = Users.Create(id, login, password, role, hasherService);

            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Error, Is.EqualTo("Поле Роль не должно быть пустым"));
            });
        }

        [Test]
        public void VerifyPassword_CorrectPassword_ReturnsTrue()
        {
            var password = "TestPassword123";
            var hasherService = new PasswordHasherService();
            var hashPassword = hasherService.HashBCrypt(password);

            var result = Users.VerifyPassword(password, hashPassword);

            Assert.That(result, Is.True);
        }

        [Test]
        public void VerifyPassword_WrongPassword_ReturnsFalse()
        {
            var password = "TestPassword123";
            var wrongPassword = "WrongPassword";
            var hasherService = new PasswordHasherService();
            var hashPassword = hasherService.HashBCrypt(password);

            var result = Users.VerifyPassword(wrongPassword, hashPassword);

            Assert.That(result, Is.False);
        }

        [Test]
        public void VerifyPassword_EmptyPassword_ReturnsFalse()
        {
            var hasherService = new PasswordHasherService();
            var hashPassword = hasherService.HashBCrypt("SomePassword");

            var result = Users.VerifyPassword(string.Empty, hashPassword);

            Assert.That(result, Is.False);
        }
    }
}
