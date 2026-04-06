namespace WebsiteBuilderForBusinesses.DataAccess.Postgres.Dto
{
    public class ShortUserDto
    {
        public Guid Id { get; set; }
        public string Login { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
