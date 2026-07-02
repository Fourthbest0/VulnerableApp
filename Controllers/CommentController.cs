using Microsoft.AspNetCore.Mvc;

namespace VulnerableApp.Controllers
{
    public class CommentController : Controller
    {
        private static List<string> _comments = new();
        private readonly ILogger<CommentController> _logger;

        public CommentController(ILogger<CommentController> logger)
        {
            _logger = logger;
        }

        private string CurrentUser => HttpContext.Session.GetString("User") ?? "Anonimo";
        private string? CurrentIp => HttpContext.Connection.RemoteIpAddress?.ToString();

        public IActionResult Index()
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var user = CurrentUser;
            var ip = CurrentIp;

            _logger.LogInformation("Inicio Comment.Index. Usuario:{User} IP:{IP}", user, ip);

            try
            {
                var result = View(_comments);

                sw.Stop();
                _logger.LogInformation("Fin Comment.Index. Usuario:{User} IP:{IP} TotalComentarios:{Total} DuracionMs:{Duracion}",
                    user, ip, _comments.Count, sw.ElapsedMilliseconds);

                return result;
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "Error en Comment.Index. Usuario:{User} IP:{IP} DuracionMs:{Duracion}",
                    user, ip, sw.ElapsedMilliseconds);
                throw;
            }
        }

        [HttpPost]
        public IActionResult AddComment(string comment)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var user = CurrentUser;
            var ip = CurrentIp;
            var longitud = comment?.Length ?? 0;

            // No se loguea el contenido completo del comentario para evitar volcar
            // texto arbitrario de usuario (posible payload) directamente en los logs.
            _logger.LogInformation("Inicio Comment.AddComment. Usuario:{User} IP:{IP} LongitudComentario:{Longitud}",
                user, ip, longitud);

            try
            {
                if (string.IsNullOrEmpty(comment))
                {
                    _logger.LogWarning("Comment.AddComment recibido comentario vacio. Usuario:{User} IP:{IP}", user, ip);
                }
                else
                {
                    _comments.Add(comment);
                }

                sw.Stop();
                _logger.LogInformation("Fin Comment.AddComment. Usuario:{User} IP:{IP} DuracionMs:{Duracion}",
                    user, ip, sw.ElapsedMilliseconds);

                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "Error en Comment.AddComment. Usuario:{User} IP:{IP} DuracionMs:{Duracion}",
                    user, ip, sw.ElapsedMilliseconds);
                throw;
            }
        }
    }
}