using WebsiteBuilderForBusinesses.Core.Abstractions;

namespace WebsiteBuilderForBusinesses.Core.Services
{
    public class PasswordHasherService : IPasswordHasherService
    {
        public string HashBCrypt(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }

        public bool VerifyBCrypt(string password, string hashedPassword)
        {
            return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
        }
    }
}
