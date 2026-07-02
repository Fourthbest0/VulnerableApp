using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VulnerableApp.Data;
using VulnerableApp.Models;

namespace VulnerableApp.Controllers
{
    public class SearchController : Controller
    {
        private readonly AppDbContext _db;
        private readonly ILogger<SearchController> _logger;

        public SearchController(AppDbContext db, ILogger<SearchController> logger)
        {
            _db = db;
            _logger = logger;
        }

        private string CurrentUser => HttpContext.Session.GetString("User") ?? "Anonimo";
        private string? CurrentIp => HttpContext.Connection.RemoteIpAddress?.ToString();

        public IActionResult Index(string search)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var user = CurrentUser;
            var ip = CurrentIp;

            _logger.LogInformation("Inicio Search.Index. Usuario:{User} IP:{IP} Query:{Query}", user, ip, search);

            try
            {
                if (string.IsNullOrEmpty(search))
                {
                    _logger.LogWarning("Search.Index llamado sin parametro de busqueda. Usuario:{User} IP:{IP}", user, ip);
                    sw.Stop();
                    return View(new List<User>());
                }

                var users = _db.Users
                    .Where(u => u.Username.Contains(search))
                    .ToList();

                if (users.Count == 0)
                {
                    _logger.LogWarning("Search.Index sin resultados. Usuario:{User} IP:{IP} Query:{Query}", user, ip, search);
                }

                sw.Stop();
                _logger.LogInformation("Fin Search.Index. Usuario:{User} IP:{IP} Query:{Query} Resultados:{Total} DuracionMs:{Duracion}",
                    user, ip, search, users.Count, sw.ElapsedMilliseconds);

                return View(users);
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "Error en Search.Index. Usuario:{User} IP:{IP} Query:{Query} DuracionMs:{Duracion}",
                    user, ip, search, sw.ElapsedMilliseconds);
                throw;
            }
        }
    }
}