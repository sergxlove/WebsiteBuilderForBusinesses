using WebsiteBuilderForBusinesses.Applications.Requests;

namespace WebsiteBuilderForBusinesses.Applications.Abstractions
{
    public interface IJwtProviderService
    {
        string GenerateToken(JwtRequest request);
    }
}