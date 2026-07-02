using Microsoft.AspNetCore.Mvc;
using VulnerableApp.Data;

namespace VulnerableApp.Controllers
{
    [ApiController]
    [Route("api")]
    public class ApiController : ControllerBase
    {
        private readonly AppDbContext _db;
        private readonly ILogger<ApiController> _logger;

        public ApiController(AppDbContext db, ILogger<ApiController> logger)
        {
            _db = db;
            _logger = logger;
        }

        private string CurrentUser => HttpContext.Session.GetString("User") ?? "Anonimo";
        private string? CurrentIp => HttpContext.Connection.RemoteIpAddress?.ToString();

        [HttpGet("user/{id}")]
        public IActionResult GetUser(int id)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var user = CurrentUser;
            var ip = CurrentIp;

            _logger.LogInformation("Inicio Api.GetUser. Usuario:{User} IP:{IP} Id:{Id}", user, ip, id);

            try
            {
                var currentUserId = HttpContext.Session.GetInt32("UserId");
                if (!currentUserId.HasValue)
                {
                    _logger.LogWarning("Api.GetUser acceso no autorizado (sin sesion). Usuario:{User} IP:{IP}", user, ip);
                    sw.Stop();
                    return Unauthorized();
                }

                if (id != currentUserId.Value)
                {
                    _logger.LogWarning("Api.GetUser intento de acceso a Id ajeno. Usuario:{User} IP:{IP} IdSolicitado:{Id} IdSesion:{SessionId}",
                        user, ip, id, currentUserId.Value);
                    sw.Stop();
                    return Forbid();
                }

                var dbUser = _db.Users.Find(id);
                if (dbUser == null)
                {
                    _logger.LogWarning("Api.GetUser usuario no encontrado. Usuario:{User} IP:{IP} Id:{Id}", user, ip, id);
                    sw.Stop();
                    return NotFound();
                }

                sw.Stop();
                _logger.LogInformation("Fin Api.GetUser. Usuario:{User} IP:{IP} Id:{Id} DuracionMs:{Duracion}",
                    user, ip, id, sw.ElapsedMilliseconds);

                return Ok(new { dbUser.Id, dbUser.Username, dbUser.Email });
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "Error en Api.GetUser. Usuario:{User} IP:{IP} Id:{Id} DuracionMs:{Duracion}",
                    user, ip, id, sw.ElapsedMilliseconds);
                throw;
            }
        }

        [HttpGet("users")]
        public IActionResult GetAllUsers()
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var user = CurrentUser;
            var ip = CurrentIp;

            _logger.LogInformation("Inicio Api.GetAllUsers. Usuario:{User} IP:{IP}", user, ip);

            try
            {
                // Hallazgo de seguridad: este endpoint no valida sesion/rol antes de listar todos los usuarios.
                var users = _db.Users.ToList();

                sw.Stop();
                _logger.LogInformation("Fin Api.GetAllUsers. Usuario:{User} IP:{IP} TotalRegistros:{Total} DuracionMs:{Duracion}",
                    user, ip, users.Count, sw.ElapsedMilliseconds);

                return Ok(users);
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "Error en Api.GetAllUsers. Usuario:{User} IP:{IP} DuracionMs:{Duracion}",
                    user, ip, sw.ElapsedMilliseconds);
                throw;
            }
        }
    }
}