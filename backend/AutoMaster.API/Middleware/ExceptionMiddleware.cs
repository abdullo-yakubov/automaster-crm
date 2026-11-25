using System.Net;
using System.Text.Json;

namespace AutoMaster.API.Middleware;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            // Бизнес-ошибки (бросает наш код) показываем пользователю. Те же типы исключений от
            // EF Core / фреймворка — это внутренние ошибки: текст содержит SQL/LINQ и наружу не уходит.
            var status = !IsThrownByApplication(ex)
                ? HttpStatusCode.InternalServerError
                : ex switch
                {
                    KeyNotFoundException => HttpStatusCode.NotFound,
                    UnauthorizedAccessException => HttpStatusCode.Unauthorized,
                    InvalidOperationException => HttpStatusCode.BadRequest,
                    ArgumentException => HttpStatusCode.BadRequest,
                    _ => HttpStatusCode.InternalServerError
                };

            if (status == HttpStatusCode.InternalServerError)
                _logger.LogError(ex, "Unhandled exception, traceId {TraceId}", context.TraceIdentifier);
            else
                _logger.LogWarning("{Status}: {Message}", (int)status, ex.Message);

            if (context.Response.HasStarted) throw;

            context.Response.Clear();
            context.Response.ContentType = "application/json";
            context.Response.StatusCode = (int)status;

            // Детали внутренних ошибок (SQL, стек) наружу не отдаём.
            var message = status == HttpStatusCode.InternalServerError
                ? "Внутренняя ошибка сервера"
                : ex.Message;

            await context.Response.WriteAsync(JsonSerializer.Serialize(new { error = message, traceId = context.TraceIdentifier }));
        }
    }

    private static bool IsThrownByApplication(Exception ex) =>
        ex.TargetSite?.DeclaringType?.Namespace?.StartsWith("AutoMaster", StringComparison.Ordinal) == true;
}
