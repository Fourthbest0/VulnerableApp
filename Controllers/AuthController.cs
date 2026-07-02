using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VulnerableApp.Data;

namespace VulnerableApp.Controllers
{
    public class AuthController : Controller
    {
        private readonly AppDbContext _db;
        private readonly ILogger<AuthController> _logger;

        public AuthController(AppDbContext db, ILogger<AuthController> logger)
        {
            _db = db;
            _logger = logger;
        }

        private string? CurrentIp => HttpContext.Connection.RemoteIpAddress?.ToString();

        public IActionResult Login()
        {
            _logger.LogInformation("Inicio Auth.Login (GET). IP:{IP}", CurrentIp);
            return View();
        }

        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var ip = CurrentIp;

            // IMPORTANTE: 'password' nunca se incluye en ningun log, ni siquiera en el catch.
            _logger.LogInformation("Inicio Auth.Login (POST). Usuario:{User} IP:{IP}", username, ip);

            try
            {
                var user = _db.Users.FirstOrDefault(u => u.Username == username);
                if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                {
                    _logger.LogWarning("Evento de autenticacion: Login fallido. Usuario:{User} IP:{IP}", username, ip);
                    ViewBag.Error = "Credenciales inválidas";
                    sw.Stop();
                    return View();
                }

                HttpContext.Session.SetString("User", user.Username);
                HttpContext.Session.SetInt32("UserId", user.Id);

                sw.Stop();
                _logger.LogInformation("Evento de autenticacion: Login exitoso. Usuario:{User} IP:{IP} DuracionMs:{Duracion}",
                    user.Username, ip, sw.ElapsedMilliseconds);

                return RedirectToAction("Dashboard");
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "Error en Auth.Login. Usuario:{User} IP:{IP} DuracionMs:{Duracion}",
                    username, ip, sw.ElapsedMilliseconds);
                throw;
            }
        }

        public IActionResult Dashboard()
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var ip = CurrentIp;
            var userId = HttpContext.Session.GetInt32("UserId");
            var userName = HttpContext.Session.GetString("User") ?? "Anonimo";

            _logger.LogInformation("Inicio Auth.Dashboard. Usuario:{User} IP:{IP}", userName, ip);

            if (!userId.HasValue)
            {
                _logger.LogWarning("Auth.Dashboard acceso sin sesion activa. IP:{IP}", ip);
                return RedirectToAction("Login");
            }

            try
            {
                var user = _db.Users.Find(userId.Value);

                sw.Stop();
                _logger.LogInformation("Fin Auth.Dashboard. Usuario:{User} IP:{IP} DuracionMs:{Duracion}",
                    userName, ip, sw.ElapsedMilliseconds);

                return View(user);
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "Error en Auth.Dashboard. Usuario:{User} IP:{IP} DuracionMs:{Duracion}",
                    userName, ip, sw.ElapsedMilliseconds);
                throw;
            }
        }

        public IActionResult Logout()
        {
            var ip = CurrentIp;
            var userName = HttpContext.Session.GetString("User") ?? "Anonimo";

            _logger.LogInformation("Evento de autenticacion: Logout. Usuario:{User} IP:{IP}", userName, ip);

            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Home");
        }
    }
}