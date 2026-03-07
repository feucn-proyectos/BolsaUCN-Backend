using System.Security;
using System.Text.Json;
using backend.src.Application.DTOs.BaseResponse;
using backend.src.Infrastructure.Exceptions;
using Serilog;

namespace backend.src.API.Middlewares.ErrorHandlingMiddleware;

/// <summary>
/// Middleware para el manejo de excepciones en la aplicación.
/// </summary>
public class ErrorHandlingMiddleware(RequestDelegate next)
{
    private readonly RequestDelegate _next = next;

    /// <summary>
    /// Método que se invoca en cada solicitud HTTP.
    /// </summary>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
            if (context.Response.StatusCode == StatusCodes.Status401Unauthorized)
            {
                Log.Warning(
                    "Acceso no autorizado a: {Path} - IP: {IP}",
                    context.Request.Path,
                    context.Connection.RemoteIpAddress
                );
                context.Response.ContentType = "application/json";
                var result = System.Text.Json.JsonSerializer.Serialize(
                    new
                    {
                        message = "No tienes autorización para realizar esta acción. Por favor inicia sesión.",
                    }
                );
                await context.Response.WriteAsync(result);
            }
        }
        catch (Exception ex)
        {
            //Capturamos excepciones no controladas y generación de un ID de seguimiento único
            var traceId = Guid.NewGuid().ToString();
            context.Response.Headers["trace-id"] = traceId;

            var (statusCode, title) = MapExceptionToStatus(ex);

            // Creamos un objeto ProblemDetails para la respuesta
            ErrorDetail error = new ErrorDetail(title, ex.Message);

            Log.Error(
                ex,
                "Excepción no controlada. Trace ID: {TraceId}, Path: {Path}, Method: {Method}, IP: {IP}",
                traceId,
                context.Request.Path,
                context.Request.Method,
                context.Connection.RemoteIpAddress
            );

            // Configuramos la respuesta HTTP como JSON
            context.Response.ContentType = "application/json";
            // Establecemos el código de estado HTTP adecuado
            context.Response.StatusCode = statusCode;

            // Serializamos el objeto ProblemDetails a JSON y lo escribimos en la respuesta
            var json = JsonSerializer.Serialize(
                error,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }
            );

            // Acá se escribe la respuesta al cliente
            await context.Response.WriteAsync(json);
        }
    }

    private static (int, string) MapExceptionToStatus(Exception ex)
    {
        return ex switch
        {
            UnauthorizedAccessException _ => (StatusCodes.Status401Unauthorized, "No autorizado"),
            ArgumentNullException _ => (StatusCodes.Status400BadRequest, "Solicitud inválida"),
            KeyNotFoundException _ => (StatusCodes.Status404NotFound, "Recurso no encontrado"),
            InvalidOperationException _ => (
                StatusCodes.Status409Conflict,
                "Conflicto de operación"
            ),
            EmailNotVerifiedException _ => (StatusCodes.Status403Forbidden, "Email no verificado"),
            FormatException _ => (StatusCodes.Status400BadRequest, "Formato inválido"),
            SecurityException _ => (StatusCodes.Status403Forbidden, "Acceso prohibido"),
            ArgumentOutOfRangeException _ => (
                StatusCodes.Status400BadRequest,
                "Argumento fuera de rango"
            ),
            ArgumentException _ => (StatusCodes.Status400BadRequest, "Argumento inválido"),
            TimeoutException _ => (StatusCodes.Status429TooManyRequests, "Demasiadas solicitudes"),
            JsonException _ => (StatusCodes.Status400BadRequest, "JSON inválido"),
            _ => (StatusCodes.Status500InternalServerError, "Error interno del servidor"),
        };
    }
}
