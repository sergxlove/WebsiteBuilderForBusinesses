namespace WebsiteBuilderForBusinesses.DataAccess.Postgres.Models
{
    public class UsersEntity
    {
        public Guid Id { get; set; }
        public string Login { get; set; } = string.Empty;
        public string HashPassword { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
