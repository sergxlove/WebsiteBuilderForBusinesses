using WebsiteBuilderForBusinesses.API.Endpoints;

namespace WebsiteBuilderForBusinesses.API.Extensions
{
    public static class RegistrEndpoints
    {
        public static IEndpointRouteBuilder MapAllEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapLoginEndpoints();
            app.MapGetPageEndpoints();
            app.MapProjectEndpoints();
            return app;
        }
    }
}
