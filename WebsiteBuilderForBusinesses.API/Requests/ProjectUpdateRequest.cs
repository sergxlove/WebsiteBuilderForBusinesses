namespace WebsiteBuilderForBusinesses.API.Requests
{
    public class ProjectUpdateRequest
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string TextHtml { get; set; } = string.Empty;
    }
}
