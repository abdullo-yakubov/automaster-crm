namespace AutoMaster.API.Middleware;

/// <summary>Базовые security-заголовки для JSON API.</summary>
public class SecurityHeadersMiddleware
{
    private readonly RequestDelegate _next;

    public SecurityHeadersMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public Task InvokeAsync(HttpContext context)
    {
        var headers = context.Response.Headers;
        headers["X-Content-Type-Options"] = "nosniff";
        headers["X-Frame-Options"] = "DENY";
        headers["Referrer-Policy"] = "no-referrer";
        headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=()";
        headers["Cross-Origin-Opener-Policy"] = "same-origin";

        // Swagger UI нужны скрипты и стили, остальному API — ничего.
        if (!context.Request.Path.StartsWithSegments("/swagger"))
            headers["Content-Security-Policy"] = "default-src 'none'; frame-ancestors 'none'";

        if (context.Request.Path.StartsWithSegments("/api"))
            headers.CacheControl = "no-store";

        return _next(context);
    }
}
