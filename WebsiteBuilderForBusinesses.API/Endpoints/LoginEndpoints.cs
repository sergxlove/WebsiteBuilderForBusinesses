using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebsiteBuilderForBusinesses.API.Requests;
using WebsiteBuilderForBusinesses.Applications.Abstractions;
using WebsiteBuilderForBusinesses.Applications.Requests;
using WebsiteBuilderForBusinesses.Core.Abstractions;
using WebsiteBuilderForBusinesses.Core.Models;

namespace WebsiteBuilderForBusinesses.API.Endpoints
{
    public static class LoginEndpoints
    {
        public static IEndpointRouteBuilder MapLoginEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/login", async (HttpContext context,
                [FromBody] LoginRequest request,
                [FromServices] IUsersService userService,
                [FromServices] IJwtProviderService jwtService,
                CancellationToken token) =>
            {
                try
                {
                    if (request.Login == string.Empty || request.Password == string.Empty)
                        return Results.BadRequest("Пустые значения логин или пароль");
                    if (!await userService.VerifyAsync(request.Login, request.Password))
                        return Results.BadRequest("Неверный логин или пароль");
                    string userRole = await userService.GetRoleAsync(request.Login, token);
                    var claims = new List<Claim>()
                    {
                        new Claim(ClaimTypes.Role, userRole),
                        new Claim(ClaimTypes.Email, request.Login),
                    };
                    var jwttoken = jwtService.GenerateToken(new JwtRequest()
                    {
                        Claims = claims
                    });
                    context.Response.Cookies.Append("jwt", jwttoken!);
                    return Results.Ok();
                }
                catch
                {
                    return Results.InternalServerError();
                }
            }).RequireRateLimiting("LoginPolicy");

            app.MapPost("/reg", async (HttpContext context,
                [FromBody] RegistrRequest request,
                [FromServices] IUsersService userService,
                [FromServices] IJwtProviderService jwtService,
                [FromServices] IPasswordHasherService passwordHasher,
                CancellationToken token) =>
            {
                try
                {
                    if (request.Login == string.Empty || request.Password == string.Empty ||
                        request.AgainPassword == string.Empty)
                        return Results.BadRequest("Пустые значения логин или пароль");
                    if (request.Password != request.AgainPassword)
                        return Results.BadRequest("Пароли не совпадают");
                    var user = Users.Create(Guid.NewGuid(), request.Login, request.Password,
                        request.Role, passwordHasher);
                    if (!user.IsSuccess) return Results.BadRequest(user.Error);
                    if (await userService.CheckAsync(user.Value.Login, token))
                    {
                        return Results.BadRequest("Данный пользователь уже есть");
                    }
                    var result = await userService.CreateAsync(user.Value, token);
                    var claims = new List<Claim>()
                    {
                        new Claim(ClaimTypes.Role, request.Role),
                        new Claim(ClaimTypes.Email, request.Login),
                    };
                    var jwttoken = jwtService.GenerateToken(new JwtRequest()
                    {
                        Claims = claims
                    });
                    context.Response.Cookies.Append("jwt", jwttoken!);
                    return Results.Ok();
                }
                catch
                {
                    return Results.InternalServerError();
                }
            }).RequireRateLimiting("GeneralPolicy");

            app.MapGet("/logout", (HttpContext context) =>
            {
                context.Response.Cookies.Delete("jwt");
                return Results.Ok();
            });

            return app;
        }
    }
}
