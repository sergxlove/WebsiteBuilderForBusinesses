using WebsiteBuilderForBusinesses.Core.Models;

namespace WebsiteBuilderForBusinesses.Tests.UnitTests
{
    public class TokensUserTests
    {
        private Guid _validId;
        private Guid _validUserId;
        private DateTime _validCreated;
        private DateTime _validEnded;
        private const string ValidEmail = "test@example.com";
        private const string ValidRole = "user";

        [SetUp]
        public void Setup()
        {
            _validId = Guid.NewGuid();
            _validUserId = Guid.NewGuid();
            _validCreated = DateTime.UtcNow;
            _validEnded = DateTime.UtcNow.AddDays(7);
        }

        [Test]
        public void Create_WithAllValidParameters_ShouldReturnSuccess()
        {
            var result = TokensUser.Create(
                _validId,
                _validUserId,
                _validCreated,
                _validEnded,
                ValidEmail,
                ValidRole
            );
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Error, Is.Null.Or.Empty);
                Assert.That(result.Value, Is.Not.Null);
            });
        }

        [Test]
        public void Create_WithAllValidParameters_ShouldSetPropertiesCorrectly()
        {
            var result = TokensUser.Create(
                _validId,
                _validUserId,
                _validCreated,
                _validEnded,
                ValidEmail,
                ValidRole
            );
            Assert.That(result.IsSuccess, Is.True);
            var tokenUser = result.Value;
            Assert.Multiple(() =>
            {
                Assert.That(tokenUser.Id, Is.EqualTo(_validId));
                Assert.That(tokenUser.UserId, Is.EqualTo(_validUserId));
                Assert.That(tokenUser.Created, Is.EqualTo(_validCreated));
                Assert.That(tokenUser.Ended, Is.EqualTo(_validEnded));
                Assert.That(tokenUser.Email, Is.EqualTo(ValidEmail));
                Assert.That(tokenUser.Role, Is.EqualTo(ValidRole));
            });
        }

        [Test]
        public void Create_WhenIdIsEmpty_ShouldReturnFailure()
        {
            var emptyId = Guid.Empty;
            var result = TokensUser.Create(
                emptyId,
                _validUserId,
                _validCreated,
                _validEnded,
                ValidEmail,
                ValidRole
            );
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Error, Is.EqualTo("id is null"));
                Assert.That(result.Value, Is.Null);
            });
        }

        [Test]
        public void Create_WhenIdIsValid_ShouldNotReturnIdError()
        {
            var result = TokensUser.Create(
                _validId,
                _validUserId,
                _validCreated,
                _validEnded,
                ValidEmail,
                ValidRole
            );
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Error, Is.Not.EqualTo("id is null"));
            });
        }

        [Test]
        public void Create_WhenUserIdIsEmpty_ShouldReturnFailure()
        {
            var emptyUserId = Guid.Empty;
            var result = TokensUser.Create(
                _validId,
                emptyUserId,
                _validCreated,
                _validEnded,
                ValidEmail,
                ValidRole
            );
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Error, Is.EqualTo("id user is null"));
                Assert.That(result.Value, Is.Null);
            });
        }

        [Test]
        public void Create_WhenUserIdIsValid_ShouldNotReturnUserIdError()
        {
            var result = TokensUser.Create(
                _validId,
                _validUserId,
                _validCreated,
                _validEnded,
                ValidEmail,
                ValidRole
            );
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Error, Is.Not.EqualTo("id user is null"));
            });
        }

        [Test]
        public void Create_WhenEmailIsEmpty_ShouldReturnFailure()
        {
            var emptyEmail = string.Empty;
            var result = TokensUser.Create(
                _validId,
                _validUserId,
                _validCreated,
                _validEnded,
                emptyEmail,
                ValidRole
            );
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Error, Is.EqualTo("email is null"));
                Assert.That(result.Value, Is.Null);
            });
        }

        [Test]
        public void Create_WhenEmailHasValidFormat_ShouldSucceed()
        {
            var validEmails = new[]
            {
                "user@example.com",
                "user.name@example.co.uk",
                "user+tag@example.com",
                "user@subdomain.example.com"
            };

            foreach (var email in validEmails)
            {
                var result = TokensUser.Create(
                    _validId,
                    _validUserId,
                    _validCreated,
                    _validEnded,
                    email,
                    ValidRole
                );
                Assert.Multiple(() =>
                {
                    Assert.That(result.IsSuccess, Is.True, $"Email '{email}' should be valid");
                    Assert.That(result.Value.Email, Is.EqualTo(email));
                });
            }
        }

        [Test]
        public void Create_WhenRoleIsEmpty_ShouldReturnFailure()
        {
            var emptyRole = string.Empty;
            var result = TokensUser.Create(
                _validId,
                _validUserId,
                _validCreated,
                _validEnded,
                ValidEmail,
                emptyRole
            );
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.False);
                Assert.That(result.Error, Is.EqualTo("role is  null"));
                Assert.That(result.Value, Is.Null);
            });
        }

        [Test]
        public void Create_WhenCreatedIsInPast_ShouldSucceed()
        {
            var pastDate = DateTime.UtcNow.AddDays(-30);
            var result = TokensUser.Create(
                _validId,
                _validUserId,
                pastDate,
                _validEnded,
                ValidEmail,
                ValidRole
            );
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Value.Created, Is.EqualTo(pastDate));
            });
        }

        [Test]
        public void Create_WhenCreatedIsNow_ShouldSucceed()
        {
            var now = DateTime.UtcNow;
            var result = TokensUser.Create(
                _validId,
                _validUserId,
                now,
                _validEnded,
                ValidEmail,
                ValidRole
            );
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Value.Created, Is.EqualTo(now));
            });
        }

        [Test]
        public void Create_WhenCreatedIsInFuture_ShouldSucceed()
        {
            var futureDate = DateTime.UtcNow.AddDays(30);
            var result = TokensUser.Create(
                _validId,
                _validUserId,
                futureDate,
                _validEnded,
                ValidEmail,
                ValidRole
            );
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Value.Created, Is.EqualTo(futureDate));
            });
        }

        [Test]
        public void Create_WhenEndedIsInFuture_ShouldSucceed()
        {
            var futureEnded = DateTime.UtcNow.AddDays(30);
            var result = TokensUser.Create(
                _validId,
                _validUserId,
                _validCreated,
                futureEnded,
                ValidEmail,
                ValidRole
            );
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Value.Ended, Is.EqualTo(futureEnded));
            });
        }

        [Test]
        public void Create_WhenEndedIsInPast_ShouldSucceed()
        {
            var pastEnded = DateTime.UtcNow.AddDays(-30);
            var result = TokensUser.Create(
                _validId,
                _validUserId,
                _validCreated,
                pastEnded,
                ValidEmail,
                ValidRole
            );
            Assert.Multiple(() =>
            {
                Assert.That(result.IsSuccess, Is.True);
                Assert.That(result.Value.Ended, Is.EqualTo(pastEnded));
            });
        }

        [Test]
        public void Create_WhenEndedIsBeforeCreated_ShouldSucceed()
        {
            var ended = _validCreated.AddDays(-1);
            var result = TokensUser.Create(
                _validId,
                _validUserId,
                _validCreated,
                ended,
                ValidEmail,
                ValidRole
            );
            Assert.That(result.IsSuccess, Is.True);
            Assert.That(result.Value.Ended, Is.LessThan(result.Value.Created));
        }

        [Test]
        public void Create_WithValidDataButDifferentTypes_ShouldSucceed()
        {
            var testCases = new[]
            {
                new { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), Email = "admin@test.com", Role = "admin" },
                new { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), Email = "user@test.com", Role = "user" },
                new { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), Email = "guest@test.com", Role = "guest" }
            };
            foreach (var testCase in testCases)
            {
                var result = TokensUser.Create(
                    testCase.Id,
                    testCase.UserId,
                    _validCreated,
                    _validEnded,
                    testCase.Email,
                    testCase.Role
                );
                Assert.Multiple(() =>
                {
                    Assert.That(result.IsSuccess, Is.True);
                    Assert.That(result.Value.Id, Is.EqualTo(testCase.Id));
                    Assert.That(result.Value.UserId, Is.EqualTo(testCase.UserId));
                    Assert.That(result.Value.Email, Is.EqualTo(testCase.Email));
                    Assert.That(result.Value.Role, Is.EqualTo(testCase.Role));
                });
            }
        }

        [Test]
        public void Create_ShouldCreateNewInstanceEachTime()
        {
            var result1 = TokensUser.Create(
                _validId,
                _validUserId,
                _validCreated,
                _validEnded,
                ValidEmail,
                ValidRole
            );
            var result2 = TokensUser.Create(
                _validId,
                _validUserId,
                _validCreated,
                _validEnded,
                ValidEmail,
                ValidRole
            );
            Assert.Multiple(() =>
            {
                Assert.That(result1.IsSuccess, Is.True);
                Assert.That(result2.IsSuccess, Is.True);
                Assert.That(result1.Value, Is.Not.SameAs(result2.Value));
            });
        }
    }
}
