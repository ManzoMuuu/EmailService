using System.Diagnostics;
using System.Text;

namespace EmailService.API.Middleware
{
    /// <summary>
    /// Middleware che traccia le richieste HTTP e misura le prestazioni
    /// Ottimizzato per scenari dove l'API e i client sono sulla stessa macchina
    /// </summary>
    public class RequestTrackingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestTrackingMiddleware> _logger;

        /// <summary>
        /// Costruttore che inizializza il middleware con il logger e il prossimo middleware nella pipeline
        /// </summary>
        /// <param name="next">Delegato al prossimo middleware nella pipeline</param>
        /// <param name="logger">Logger per registrare le richieste e risposte</param>
        public RequestTrackingMiddleware(RequestDelegate next, ILogger<RequestTrackingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        /// <summary>
        /// Metodo che elabora la richiesta HTTP
        /// </summary>
        /// <param name="context">Contesto HTTP della richiesta corrente</param>
        public async Task InvokeAsync(HttpContext context)
        {
            // Crea un ID di correlazione univoco per questa richiesta
            var correlationId = Guid.NewGuid().ToString();
            context.Items["CorrelationId"] = correlationId;
            
            // Registra solo le informazioni essenziali sulla chiamata API
            // Esclusi IP e altri dettagli di rete dato che tutto è locale
            _logger.LogInformation(
                "API call {CorrelationId} started: {Method} {Path}",
                correlationId,
                context.Request.Method,
                context.Request.Path);

            // Misura il tempo di elaborazione della richiesta
            var stopwatch = Stopwatch.StartNew();
            
            try
            {
                // Estrae informazioni utili dagli header se presenti
                // Poiché IIS potrebbe aggiungere informazioni utili
                var appName = context.Request.Headers["X-Application-Name"].FirstOrDefault() ?? "unknown";
                context.Items["ApplicationName"] = appName;
                
                // Elabora la richiesta attraverso il resto della pipeline
                await _next(context);
                
                // Registra informazioni sulla risposta 
                stopwatch.Stop();
                
                _logger.LogInformation(
                    "API call {CorrelationId} from {AppName} completed: {StatusCode} in {ElapsedMilliseconds}ms",
                    correlationId,
                    appName,
                    context.Response.StatusCode,
                    stopwatch.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(
                    ex,
                    "API call {CorrelationId} failed after {ElapsedMilliseconds}ms: {ExceptionMessage}",
                    correlationId,
                    stopwatch.ElapsedMilliseconds,
                    ex.Message);
                
                throw; // Rilancia l'eccezione per essere gestita da altri middleware
            }
        }
    }

    /// <summary>
    /// Estensioni per registrare il middleware nella pipeline di richiesta
    /// </summary>
    public static class RequestTrackingMiddlewareExtensions
    {
        /// <summary>
        /// Aggiunge il middleware di tracciamento delle richieste alla pipeline
        /// </summary>
        /// <param name="builder">Application builder per la configurazione della pipeline</param>
        /// <returns>Application builder per concatenare ulteriori configurazioni</returns>
        public static IApplicationBuilder UseRequestTracking(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<RequestTrackingMiddleware>();
        }
    }
}