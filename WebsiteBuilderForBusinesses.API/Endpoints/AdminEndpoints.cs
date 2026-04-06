using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebsiteBuilderForBusinesses.API.Requests;
using WebsiteBuilderForBusinesses.Applications.Abstractions;
using WebsiteBuilderForBusinesses.Applications.Services;
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
                catch
                {
                    return Results.InternalServerError();
                }
            });

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
                catch
                {
                    return Results.InternalServerError();
                }
            });

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
                catch
                {
                    return Results.InternalServerError();
                }
            });

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
                catch
                {
                    return Results.InternalServerError();
                }
            });

            return app;
        }
    }
}
