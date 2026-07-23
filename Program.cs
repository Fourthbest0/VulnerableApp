using Microsoft.EntityFrameworkCore;
using VulnerableApp.Data;
using VulnerableApp.Middleware;
using Serilog;

var configuration = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .Build();

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .WriteTo.Seq("http://localhost:5341")
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .CreateLogger();

try
{
    Log.Information("Iniciando VulnerableApp...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    builder.Services.AddControllersWithViews();
    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

    builder.Services.AddSession();

    var app = builder.Build();

    app.UseMiddleware<ExceptionMiddleware>();
    app.UseMiddleware<CorrelationIdMiddleware>();
    app.UseMiddleware<RequestLoggingMiddleware>();

    if (!app.Environment.IsDevelopment())
    {
        app.UseExceptionHandler("/Home/Error");
        app.UseHsts();
    }

    app.UseHttpsRedirection();
    app.UseRouting();
    app.UseHttpsRedirection();
    app.UseRouting();

    app.Use(async (context, next) =>
    {
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Append("X-Frame-Options", "DENY");
        context.Response.Headers.Append("Content-Security-Policy",
            "default-src 'self'; frame-ancestors 'self'; object-src 'none'; base-uri 'self'; form-action 'self'");
        await next();
    });

    app.UseAuthorization();
    app.UseSession();
    app.MapStaticAssets();
    app.UseAuthorization();
    app.UseSession();
    app.MapStaticAssets();

    app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");

    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "La aplicación falló al iniciar");
}
finally
{
    Log.CloseAndFlush();
}