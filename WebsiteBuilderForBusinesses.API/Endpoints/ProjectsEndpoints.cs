using Microsoft.AspNetCore.Mvc;
using Serilog;
using WebsiteBuilderForBusinesses.API.Requests;
using WebsiteBuilderForBusinesses.Applications.Abstractions;
using WebsiteBuilderForBusinesses.Core.Infrastructures;
using WebsiteBuilderForBusinesses.Core.Models;

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
                catch (Exception ex) 
                {
                    Log.Error(ex.Message);
                    return Results.InternalServerError();
                }
            }).RequireAuthorization("OnlyForAuthUser")
            .RequireRateLimiting("GeneralPolicy");

            app.MapPost("/project/html", async (HttpContext context,
                [FromBody] IdRequest request,
                [FromServices] IProjectsService projectService,
                CancellationToken token) =>
            {
                try
                {
                    return Results.Ok(await projectService.GetHtmlByIdAsync(request.Id, token));
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message);
                    return Results.InternalServerError();
                }
            }).RequireAuthorization("OnlyForAuthUser")
            .RequireRateLimiting("GeneralPolicy");

            app.MapDelete("/project/html", async (HttpContext context,
                [FromBody] IdRequest request,
                [FromServices] IProjectsService projectService,
                CancellationToken token) =>
            {
                try
                {
                    if (request is null) return Results.BadRequest("Пустые данные");
                    int result = await projectService.DeleteAsync(request.Id, token);
                    if (result == 0) return Results.BadRequest("Проект не был удален из-за ошибки");
                    return Results.Ok();
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message);
                    return Results.InternalServerError();
                }
            }).RequireAuthorization("OnlyForAuthUser")
            .RequireRateLimiting("GeneralPolicy");

            app.MapPost("/project/html/update", async (HttpContext context,
                [FromBody] ProjectUpdateRequest request,
                [FromServices] IProjectsService projectService,
                CancellationToken token) =>
            {
                try
                {
                    if (request is null) return Results.BadRequest("Пустые данные");
                    ResultModel<Projects> project = Projects.Create(request.Id, request.Name,
                        DateTime.UtcNow, request.TextHtml);
                    if (project.Error != string.Empty) return Results.BadRequest(project.Error);
                    int resultUpdate = await projectService.UpdateHtmlAsync(project.Value, token);
                    if (resultUpdate == 0) return Results.BadRequest("Не удалось обновить проект");
                    return Results.Ok();
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message);
                    return Results.InternalServerError();
                }
            }).RequireAuthorization("OnlyForAuthUser")
            .RequireRateLimiting("GeneralPolicy");

            app.MapPost("/project/name/update", async (HttpContext context,
                [FromBody] ProjectNameUpdateRequest request,
                [FromServices] IProjectsService projectService,
                CancellationToken token) =>
            {
                try
                {
                    if (request is null) return Results.BadRequest("Пустые данные");
                    ResultModel<Projects> project = Projects.Create(Guid.NewGuid(), request.OldName,
                        DateTime.UtcNow, string.Empty);
                    if (project.Error != string.Empty) return Results.BadRequest(project.Error);
                    int resultUpdate = await projectService.UpdateNameAsync(request.OldName,
                        request.NewName, token);
                    if (resultUpdate == 0) return Results.BadRequest("Не удалось обновить проект");
                    return Results.Ok();
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message);
                    return Results.InternalServerError();
                }
            }).RequireAuthorization("OnlyForAuthUser")
            .RequireRateLimiting("GeneralPolicy");

            app.MapPost("/project/new", async (HttpContext context,
                [FromBody] ProjectCreateRequest request,
                [FromServices] IProjectsService projectService,
                CancellationToken token) =>
            {
                try
                {
                    if (request is null) return Results.BadRequest("Пустые данные");
                    ResultModel<Projects> project = Projects.Create(Guid.NewGuid(), request.Name,
                            DateTime.UtcNow, string.Empty);
                    if (project.Error != string.Empty) return Results.BadRequest(project.Error);
                    bool checkResult = await projectService.CheckNameAsync(project.Value.Name, token);
                    if (checkResult) return Results.BadRequest("Проект с таким названием уже существует");
                    Guid updateResult = await projectService.CreateAsync(project.Value, token);
                    if (updateResult != project.Value.Id) return Results.BadRequest("Произошла ошибка");
                    return Results.Ok();
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message);
                    return Results.InternalServerError();
                }
            }).RequireAuthorization("OnlyForAuthUser")
            .RequireRateLimiting("GeneralPolicy");

            return app;
        }
    }
}
