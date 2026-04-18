namespace WebsiteBuilderForBusinesses.DataAccess.Postgres.Models
{
    public class TokensUserEntity
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public DateTime Created { get; set; }
        public DateTime Ended { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
