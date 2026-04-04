namespace WebsiteBuilderForBusinesses.API.Requests
{
    public class RegistrRequest
    {
        public string Login { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string AgainPassword { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }
}
