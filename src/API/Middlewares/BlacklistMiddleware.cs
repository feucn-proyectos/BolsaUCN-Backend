using System.Security.Claims;
using System.Text.Json;
using backend.src.Application.DTOs.BaseResponse;
using backend.src.Domain.Models;
using backend.src.Infrastructure.Repositories.Interfaces;
using Serilog;

namespace backend.src.API.Middlewares
{
    public class BlacklistMiddleware(RequestDelegate next)
    {
        private readonly RequestDelegate _next = next;

        public async Task InvokeAsync(HttpContext context, ITokenRepository tokenRepository)
        {
            var authHeader = context.Request.Headers["Authorization"].FirstOrDefault();
            if (!string.IsNullOrEmpty(authHeader) && authHeader.StartsWith("Bearer "))
            {
                string token = authHeader.Substring("Bearer ".Length).Trim();
                string? userId = context
                    .User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)
                    ?.Value;
                if (int.TryParse(userId, out int userIdInt))
                { // Verificar si el token está en la whitelist
                    bool isWhitelisted = await tokenRepository.IsTokenWhitelistedAsync(
                        userIdInt,
                        token
                    );

                    if (!isWhitelisted)
                    {
                        Log.Warning(
                            $"Acceso denegado. Token no está en la whitelist. Usuario ID: {userId}"
                        );
                        throw new UnauthorizedAccessException("El usuario no está en la whitelist");
                    }
                }
            }
            await _next(context);
        }
    }
}
