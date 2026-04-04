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

            return app;
        }
    }
}
