using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Diagnostics;
using System.Text;
using WebsiteBuilderForBusinesses.API.Requests;
using WebsiteBuilderForBusinesses.Applications.Abstractions;
using WebsiteBuilderForBusinesses.Core.Abstractions;
using WebsiteBuilderForBusinesses.Core.Infrastructures;
using WebsiteBuilderForBusinesses.Core.Models;

namespace WebsiteBuilderForBusinesses.API.Endpoints
{
    public static class AdminEndpoints
    {
        public static IEndpointRouteBuilder MapAdminEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/users/all", async (HttpContext context,
                [FromServices] IUsersService userService,
                CancellationToken token) =>
            {
                try
                {
                    return Results.Ok(await userService.GetAllAsync(token));
                }
                catch(Exception ex)
                {
                    Log.Error(ex.Message);
                    return Results.InternalServerError();
                }
            }).RequireAuthorization("OnlyForAdmin")
            .RequireRateLimiting("GeneralPolicy");

            app.MapPost("/users/password/update", async (HttpContext context,
                [FromBody] PasswordUpdateRequest request,
                [FromServices] IUsersService userService,
                [FromServices] IPasswordHasherService passwordHasher,
                CancellationToken token) =>
            {
                try
                {
                    if (request is null) return Results.BadRequest("Пустые данные");
                    ResultModel<Users> user = Users.Create(request.Id, request.Login,
                        request.NewPassword, "user", passwordHasher);
                    if (user.Error != string.Empty) return Results.BadRequest(user.Error);
                    int resultUpdate = await userService.UpdatePasswordAsync(user.Value, token);
                    if (resultUpdate == 0) return Results.BadRequest("Не удалось обновить данные пользователя");
                    return Results.Ok();
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message);
                    return Results.InternalServerError();
                }
            }).RequireAuthorization("OnlyForAdmin")
            .RequireRateLimiting("GeneralPolicy");

            app.MapPost("/users/role/update", async (HttpContext context,
                [FromBody] RoleUpdateRequest request,
                [FromServices] IUsersService userService,
                [FromServices] IPasswordHasherService passwordHasher,
                CancellationToken token) =>
            {
                try
                {
                    if (request is null) return Results.BadRequest("Пустые данные");
                    ResultModel<Users> user = Users.Create(request.Id, request.Login,
                            "password", request.NewRole, passwordHasher);
                    if (user.Error != string.Empty) return Results.BadRequest(user.Error);
                    int resultUpdate = await userService.UpdateRoleAsync(user.Value, token);
                    if (resultUpdate == 0) return Results.BadRequest("Не удалось обновить данные пользователя");
                    return Results.Ok();
                }
                catch(Exception ex)
                {
                    Log.Error(ex.Message);
                    return Results.InternalServerError();
                }
            }).RequireAuthorization("OnlyForAdmin")
            .RequireRateLimiting("GeneralPolicy");

            app.MapDelete("/users", async (HttpContext context,
                [FromBody] IdRequest request,
                [FromServices] IUsersService userService,
                CancellationToken token) =>
            {
                try
                {
                    if (request is null) return Results.BadRequest("Пустые данные");
                    int resultDelete = await userService.DeleteAsync(request.Id, token);
                    if (resultDelete == 0) return Results.BadRequest("Произошла ошибка при удалении");
                    return Results.Ok();
                }
                catch(Exception ex)
                {
                    Log.Error(ex.Message);
                    return Results.InternalServerError();
                }
            }).RequireAuthorization("OnlyForAdmin")
            .RequireRateLimiting("GeneralPolicy");

            app.MapGet("/api/backup/create", async () =>
            {
                string fileName = $"backup_{DateTime.Now:yyyyMMdd_HHmmss}.sql";
                string containerName = "webbuilder-db";
                try
                {
                    var process = new Process
                    {
                        StartInfo = new ProcessStartInfo
                        {
                            FileName = "docker",
                            Arguments = $"exec {containerName} pg_dump -U postgres -d db",
                            RedirectStandardOutput = true,
                            RedirectStandardError = true,
                            UseShellExecute = false,
                            CreateNoWindow = true,
                            StandardOutputEncoding = Encoding.UTF8
                        }
                    };
                    process.Start();
                    string output = await process.StandardOutput.ReadToEndAsync();
                    string error = await process.StandardError.ReadToEndAsync();
                    await process.WaitForExitAsync();
                    if (process.ExitCode != 0)
                    {
                        return Results.BadRequest($"Ошибка pg_dump: {error}");
                    }
                    var fileBytes = Encoding.UTF8.GetBytes(output);
                    return Results.File(fileBytes, "application/octet-stream", fileName);
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message);
                    return Results.InternalServerError();
                }
            }).RequireAuthorization("OnlyForAdmin")
            .RequireRateLimiting("GeneralPolicy");

            return app;
        }
    }
}
