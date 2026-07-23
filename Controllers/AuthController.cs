using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VulnerableApp.Data;

namespace VulnerableApp.Controllers
{
    public class AuthController : Controller
    {
        private readonly AppDbContext _db;
        private readonly ILogger<AuthController> _logger;

        // FIX (CWE-307): tracking simple de intentos fallidos por usuario+IP
        private static readonly Dictionary<string, (int Intentos, DateTime Bloqueo)> _intentosFallidos = new();
        private const int MaxIntentos = 5;
        private static readonly TimeSpan TiempoBloqueo = TimeSpan.FromMinutes(5);

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
            var clave = $"{username}:{ip}";

            _logger.LogInformation("Inicio Auth.Login (POST). Usuario:{User} IP:{IP}", username, ip);

            // FIX: checar bloqueo por intentos previos antes de validar credenciales
            if (_intentosFallidos.TryGetValue(clave, out var estado) &&
                estado.Intentos >= MaxIntentos &&
                DateTime.UtcNow < estado.Bloqueo)
            {
                _logger.LogWarning("Auth.Login bloqueado por rate limit. Usuario:{User} IP:{IP}", username, ip);
                ViewBag.Error = "Demasiados intentos. Intenta de nuevo en unos minutos.";
                sw.Stop();
                return View();
            }

            try
            {
                var user = _db.Users.FirstOrDefault(u => u.Username == username);
                if (user == null || !BCrypt.Net.BCrypt.Verify(password, user.PasswordHash))
                {
                    // FIX: incrementar contador de intentos fallidos
                    var intentosActuales = _intentosFallidos.TryGetValue(clave, out var e) ? e.Intentos + 1 : 1;
                    _intentosFallidos[clave] = (intentosActuales, DateTime.UtcNow.Add(TiempoBloqueo));

                    _logger.LogWarning("Evento de autenticacion: Login fallido. Usuario:{User} IP:{IP} Intentos:{Intentos}",
                        username, ip, intentosActuales);
                    ViewBag.Error = "Credenciales inválidas";
                    sw.Stop();
                    return View();
                }

                // FIX: login exitoso limpia el contador de esa clave
                _intentosFallidos.Remove(clave);

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