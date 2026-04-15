using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System.Text;
using System.Threading.RateLimiting;
using WebsiteBuilderForBusinesses.API.Extensions;
using WebsiteBuilderForBusinesses.Applications.Abstractions;
using WebsiteBuilderForBusinesses.Applications.Services;
using WebsiteBuilderForBusinesses.Core.Abstractions;
using WebsiteBuilderForBusinesses.Core.Services;
using WebsiteBuilderForBusinesses.DataAccess.Postgres;
using WebsiteBuilderForBusinesses.DataAccess.Postgres.Abstractions;
using WebsiteBuilderForBusinesses.DataAccess.Postgres.Repositories;

namespace WebsiteBuilderForBusinesses.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            IConfigurationSection? seqSetting = builder.Configuration.GetSection("SeqSetting");
            IConfigurationSection? aspnetSetting = builder.Configuration.GetSection("AspnetSetting");
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithThreadId()
                .WriteTo.Seq(
                    serverUrl: seqSetting["ServerUrl"]!,
                    apiKey: seqSetting["ApiKey"]) 
                .WriteTo.Console()
                .CreateLogger();
            builder.Host.UseSerilog();
            IConfigurationSection? postgresSetting = builder.Configuration.GetSection("PostgresSetting");
            builder.Services.AddDbContext<WebBuilderDbContext>(options =>
                options.UseNpgsql(postgresSetting["ConnectionString"]));
            builder.Services.AddScoped<IPasswordHasherService, PasswordHasherService>();
            builder.Services.AddScoped<IJwtProviderService, JwtProviderService>();
            builder.Services.AddScoped<IUsersRepository, UsersRepository>();
            builder.Services.AddScoped<IUsersService, UsersService>();
            builder.Services.AddScoped<IProjectsRepository, ProjectsRepository>();
            builder.Services.AddScoped<IProjectsService, ProjectsService>();

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    IConfigurationSection? jwtSettings = builder.Configuration
                        .GetSection("JwtSettings");
                    options.TokenValidationParameters = new()
                    {
                        ValidateIssuer = true,
                        ValidIssuer = jwtSettings["Issuer"],
                        ValidateAudience = true,
                        ValidAudience = jwtSettings["Audience"],
                        ValidateLifetime = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8
                            .GetBytes(jwtSettings["SecretKey"]!)),
                        ValidateIssuerSigningKey = true
                    };
                    options.Events = new JwtBearerEvents()
                    {
                        OnMessageReceived = context =>
                        {
                            context.Token = context.Request.Cookies["jwt"];
                            return Task.CompletedTask;
                        }
                    };
                });

            builder.Services.AddAuthorization(options =>
            {
                options.AddPolicy("OnlyForAdmin", policy =>
                {
                    policy.RequireRole("admin");
                });
                options.AddPolicy("OnlyForAuthUser", policy =>
                {
                    policy.RequireRole("user", "admin");
                });
            });

            builder.Services.AddRateLimiter(options =>
            {
                options.AddFixedWindowLimiter("GeneralPolicy", opt =>
                {
                    opt.PermitLimit = 100;
                    opt.Window = TimeSpan.FromMinutes(1);
                    opt.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
                    opt.QueueLimit = 10;
                });
                options.AddFixedWindowLimiter("LoginPolicy", opt =>
                {
                    opt.PermitLimit = 5;
                    opt.Window = TimeSpan.FromMinutes(1);
                });
                options.AddTokenBucketLimiter("UploadPolicy", opt =>
                {
                    opt.TokenLimit = 10;
                    opt.ReplenishmentPeriod = TimeSpan.FromMinutes(1);
                    opt.TokensPerPeriod = 2;
                    opt.AutoReplenishment = true;
                });
            });

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.SetIsOriginAllowed(origin => true)
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                });
            });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.WebHost.UseUrls($"http://localhost:{aspnetSetting["Port"]}");
            var app = builder.Build();
            app.UseStaticFiles();
            app.UseStatusCodePagesWithReExecute("/error/{0}");
            app.UseCors("AllowAll");
            app.UseSwagger();
            app.UseSwaggerUI();
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseRateLimiter();
            app.MapAllEndpoints();
            app.Run();
        }
    }
}
