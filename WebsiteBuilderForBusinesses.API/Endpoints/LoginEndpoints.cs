using Microsoft.AspNetCore.Mvc;
using Serilog;
using System.Security.Claims;
using WebsiteBuilderForBusinesses.API.Requests;
using WebsiteBuilderForBusinesses.Applications.Abstractions;
using WebsiteBuilderForBusinesses.Applications.Requests;
using WebsiteBuilderForBusinesses.Core.Abstractions;
using WebsiteBuilderForBusinesses.Core.Infrastructures;
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
                [FromServices] ITokensUserService tokenService,
                [FromServices] IConfiguration configuration,
                CancellationToken token) =>
            {
                try
                {
                    if (request.Login == string.Empty || request.Password == string.Empty)
                        return Results.BadRequest("Пустые значения логин или пароль");
                    if (!await userService.VerifyAsync(request.Login, request.Password))
                        return Results.BadRequest("Неверный логин или пароль");
                    string userRole = await userService.GetRoleAsync(request.Login, token);
                    Guid userId = await userService.GetIdAsync(request.Login, token);
                    IConfigurationSection? jwtSettings = configuration.GetSection("JwtSettings");
                    int lifetimeAccess = Convert.ToInt32(configuration["JwtSettings:LifetimeAccessMinutes"]);
                    int lifetimeRefresh = Convert.ToInt32(configuration["JwtSettings:LifetimeRefreshDays"]);
                    var claims = new List<Claim>()
                    {
                        new Claim(ClaimTypes.Sid, userId.ToString()),
                        new Claim(ClaimTypes.Role, userRole),
                        new Claim(ClaimTypes.Email, request.Login),
                    };
                    string accessToken = jwtService.GenerateToken(new JwtRequest()
                    {
                        Audience = jwtSettings["Audience"]!,
                        Issuer = jwtSettings["Issuer"]!,
                        Claims = claims,
                        SecretKey = jwtSettings["SecretKey"]!,
                        Expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["LifetimeAccessMinutes"]!))
                    });
                    ResultModel<TokensUser> newTokenUser = TokensUser.Create(Guid.NewGuid(), userId,
                        DateTime.UtcNow, DateTime.UtcNow + TimeSpan.FromDays(lifetimeRefresh),
                        request.Login, userRole);
                    if (!string.IsNullOrEmpty(newTokenUser.Error))
                        return Results.BadRequest(newTokenUser.Error);
                    int result = await tokenService.UpdateAsync(newTokenUser.Value, token);
                    if (result == 0) return
                        Results.Unauthorized();
                    CookieOptions cookieOptions = new()
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        IsEssential = true
                    };
                    cookieOptions.MaxAge = TimeSpan.FromMinutes(lifetimeAccess);
                    context.Response.Cookies.Append("access_token", accessToken, cookieOptions);
                    cookieOptions.MaxAge = TimeSpan.FromDays(lifetimeRefresh);
                    context.Response.Cookies.Append("refresh_token", newTokenUser.Value.Id.ToString(),
                        cookieOptions);
                    return Results.Ok();
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message);
                    return Results.InternalServerError();
                }
            }).RequireRateLimiting("LoginPolicy");

            app.MapPost("/reg", async (HttpContext context,
                [FromBody] RegistrRequest request,
                [FromServices] IUsersService userService,
                [FromServices] IJwtProviderService jwtService,
                [FromServices] IPasswordHasherService passwordHasher,
                [FromServices] IConfiguration configuration,
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
                    IConfigurationSection? jwtSettings = configuration.GetSection("JwtSettings");
                    var claims = new List<Claim>()
                    {
                        new Claim(ClaimTypes.Sid, user.Value.Id.ToString()),
                        new Claim(ClaimTypes.Role, request.Role),
                        new Claim(ClaimTypes.Email, request.Login),
                    };
                    var jwttoken = jwtService.GenerateToken(new JwtRequest()
                    {
                        Audience = jwtSettings["Audience"]!,
                        Issuer = jwtSettings["Issuer"]!,
                        Claims = claims,
                        SecretKey = jwtSettings["SecretKey"]!,
                        Expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["LifetimeAccessMinutes"]!))
                    });
                    context.Response.Cookies.Append("jwt", jwttoken!);
                    return Results.Ok();
                }
                catch (Exception ex)
                {
                    Log.Error(ex.Message);
                    return Results.InternalServerError();
                }
            }).RequireAuthorization("OnlyForAdmin")
            .RequireRateLimiting("GeneralPolicy");

            app.MapPost("/refresh", async (HttpContext context, 
                [FromServices] ITokensUserService tokenService, 
                [FromServices] IJwtProviderService jwtService, 
                [FromServices] IConfiguration configuration, 
                CancellationToken token) => 
            {
                string? refreshToken = context.Request.Cookies["refresh_token"];
                if (string.IsNullOrEmpty(refreshToken))
                    return Results.Unauthorized();
                Guid refreshTokenGuid = Guid.Parse(refreshToken!);
                TokensUser? tokenDb = await tokenService.GetAsync(refreshTokenGuid, token);
                if (tokenDb is null) 
                    return Results.Unauthorized();
                var claims = new List<Claim>()
                    {
                        new Claim(ClaimTypes.Sid, tokenDb.UserId.ToString()),
                        new Claim(ClaimTypes.Role, tokenDb.Role),
                        new Claim(ClaimTypes.Email, tokenDb.Email),
                    };
                int lifetimeAccess = Convert.ToInt32(configuration["JwtSettings:LifetimeAccessMinutes"]);
                int lifetimeRefresh = Convert.ToInt32(configuration["JwtSettings:LifetimeRefreshDays"]);
                IConfigurationSection? jwtSettings = configuration.GetSection("JwtSettings");
                string accessToken = jwtService.GenerateToken(new JwtRequest()
                {
                    Audience = jwtSettings["Audience"]!,
                    Issuer = jwtSettings["Issuer"]!,
                    Claims = claims,
                    SecretKey = jwtSettings["SecretKey"]!,
                    Expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(jwtSettings["LifetimeAccessMinutes"]!))
                });
                ResultModel<TokensUser> newTokenUser = TokensUser.Create(Guid.NewGuid(), tokenDb.UserId,
                    DateTime.UtcNow, DateTime.UtcNow + TimeSpan.FromDays(lifetimeRefresh),
                    tokenDb.Email, tokenDb.Role);
                if (!string.IsNullOrEmpty(newTokenUser.Error))
                    return Results.BadRequest(newTokenUser.Error);
                int result = await tokenService.UpdateAsync(newTokenUser.Value, token);
                if (result == 0) return 
                    Results.Unauthorized();
                CookieOptions cookieOptions = new()
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    IsEssential = true
                };
                cookieOptions.MaxAge = TimeSpan.FromMinutes(lifetimeAccess);
                context.Response.Cookies.Append("access_token", accessToken, cookieOptions);
                cookieOptions.MaxAge = TimeSpan.FromDays(lifetimeRefresh);
                context.Response.Cookies.Append("refresh_token", tokenDb.Id.ToString(),
                    cookieOptions);
                return Results.Ok();
            }).RequireRateLimiting("GeneralPolicy");

            app.MapGet("/logout", (HttpContext context) =>
            {
                try
                {
                    context.Response.Cookies.Delete("access_token");
                    context.Response.Cookies.Delete("refresh_token");
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
