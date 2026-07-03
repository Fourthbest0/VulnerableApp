namespace VulnerableApp.Middleware
{
    public class CorrelationIdMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<CorrelationIdMiddleware> _logger;

        public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var cid = Guid.NewGuid().ToString();
            context.Response.Headers["X-Correlation-ID"] = cid;

            // Push al contexto de logging: así TODOS los logs de esta petición
            // (incluso los que ya escribes en tus controladores) incluyen el CorrelationId automáticamente.
            using (Serilog.Context.LogContext.PushProperty("CorrelationId", cid))
            {
                await _next(context);
            }
        }
    }
}