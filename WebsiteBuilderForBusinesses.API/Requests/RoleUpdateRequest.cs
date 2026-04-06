namespace WebsiteBuilderForBusinesses.API.Requests
{
    public class RoleUpdateRequest
    {
        public Guid Id { get; set; }
        public string Login { get; set; } = string.Empty;
        public string NewRole { get; set; } = string.Empty;
    }
}
