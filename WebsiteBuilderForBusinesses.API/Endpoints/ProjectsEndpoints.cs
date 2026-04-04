using Microsoft.AspNetCore.Mvc;
using WebsiteBuilderForBusinesses.Applications.Abstractions;

namespace WebsiteBuilderForBusinesses.API.Endpoints
{
    public static class ProjectsEndpoints
    {
        public static IEndpointRouteBuilder MapProjectEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/project/all", async (HttpContext context,
                [FromServices] IProjectsService projectService,
                CancellationToken token) =>
            {
                try
                {
                    return Results.Ok(await projectService.GetAllAsync(token));
                }
                catch
                {
                    return Results.InternalServerError();
                }
            });

            return app;
        }
    }
}
