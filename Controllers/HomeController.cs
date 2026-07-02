using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using VulnerableApp.Models;

namespace VulnerableApp.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger)
    {
        _logger = logger;
    }

    private string CurrentUser => HttpContext.Session.GetString("User") ?? "Anonimo";
    private string? CurrentIp => HttpContext.Connection.RemoteIpAddress?.ToString();

    public IActionResult Index()
    {
        var sw = Stopwatch.StartNew();
        var user = CurrentUser;
        var ip = CurrentIp;

        _logger.LogInformation("Inicio Home.Index. Usuario:{User} IP:{IP}", user, ip);

        var result = View();

        sw.Stop();
        _logger.LogInformation("Fin Home.Index. Usuario:{User} IP:{IP} DuracionMs:{Duracion}",
            user, ip, sw.ElapsedMilliseconds);

        return result;
    }

    public IActionResult Privacy()
    {
        var sw = Stopwatch.StartNew();
        var user = CurrentUser;
        var ip = CurrentIp;

        _logger.LogInformation("Inicio Home.Privacy. Usuario:{User} IP:{IP}", user, ip);

        var result = View();

        sw.Stop();
        _logger.LogInformation("Fin Home.Privacy. Usuario:{User} IP:{IP} DuracionMs:{Duracion}",
            user, ip, sw.ElapsedMilliseconds);

        return result;
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        var user = CurrentUser;
        var ip = CurrentIp;
        var requestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;

        _logger.LogWarning("Home.Error alcanzado. Usuario:{User} IP:{IP} RequestId:{RequestId}",
            user, ip, requestId);

        return View(new ErrorViewModel { RequestId = requestId });
    }
}