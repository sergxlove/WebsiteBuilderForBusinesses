namespace WebsiteBuilderForBusinesses.API.Endpoints
{
    public static class GetPagesEndpoints
    {
        public static IEndpointRouteBuilder MapGetPageEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/", async (HttpContext context) =>
            {
                context.Response.ContentType = "text/html; charset=utf-8";
                await context.Response.SendFileAsync("wwwroot/Pages/LoginPage.html");
            });

            app.MapGet("/page/login", async (HttpContext context) => 
            {
                context.Response.ContentType = "text/html; charset=utf-8";
                await context.Response.SendFileAsync("wwwroot/Pages/LoginPage.html");
            });

            app.MapGet("/index", async (HttpContext context) =>
            {
                context.Response.ContentType = "text/html; charset=utf-8";
                await context.Response.SendFileAsync("wwwroot/Pages/MainPage.html");
            });

            app.MapGet("/page/reg", async (HttpContext context) =>
            {
                context.Response.ContentType = "text/html; charset=utf-8";
                await context.Response.SendFileAsync("wwwroot/Pages/RegPage.html");
            });

            app.MapGet("/page/projects", async (HttpContext context) =>
            {
                context.Response.ContentType = "text/html; charset=utf-8";
                await context.Response.SendFileAsync("wwwroot/Pages/ProjectsPage.html");
            });

            app.MapGet("/page/admin", async (HttpContext context) =>
            {
                context.Response.ContentType = "text/html; charset=utf-8";
                await context.Response.SendFileAsync("wwwroot/Pages/AdminPage.html");
            });

            app.MapGet("/error/{statusCode:int}", async (int statusCode, HttpContext context) =>
            {
                context.Response.ContentType = "text/html; charset=utf-8";
                switch (statusCode)
                {
                    case 403:
                        await context.Response.SendFileAsync("wwwroot/Pages/Errors/Error403Page.html");
                        break;
                    case 404:
                        await context.Response.SendFileAsync("wwwroot/Pages/Errors/Error404Page.html");
                        break;
                }
            });

            return app;
        }
    }
}
