namespace WebsiteBuilderForBusinesses.API.Requests
{
    public class PasswordUpdateRequest
    {
        public Guid Id { get; set; }
        public string Login { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
