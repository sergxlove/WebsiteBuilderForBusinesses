namespace WebsiteBuilderForBusinesses.DataAccess.Postgres.Dto
{
    public class ShortProjectDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public DateTime DateOpen { get; set; }
    }
}
