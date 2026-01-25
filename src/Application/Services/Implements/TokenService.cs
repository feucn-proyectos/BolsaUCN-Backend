using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using backend.src.Application.Services.Interfaces;
using backend.src.Domain.Models;
using backend.src.Infrastructure.Repositories.Interfaces;
using Microsoft.IdentityModel.Tokens;
using Serilog;

namespace backend.src.Application.Services.Implements
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly ITokenRepository _tokenRepository;
        private readonly string _jwtSecret;

        public TokenService(IConfiguration configuration, ITokenRepository tokenRepository)
        {
            _configuration = configuration;
            _tokenRepository = tokenRepository;
            _jwtSecret = _configuration.GetValue<string>("Jwt:Key")!;
        }

        /// <summary>
        /// Crea un token JWT para el usuario dado.
        /// </summary>
        /// <param name="user">Usuario</param>
        /// <param name="roleName">Nombre del rol</param>
        /// <param name="rememberMe">Indica si se debe recordar al usuario</param>
        /// <returns>Token JWT</returns>
        public string CreateToken(User user, IList<string> roleNames, bool rememberMe)
        {
            try
            {
                Log.Information(
                    "Creando token JWT para usuario ID: {UserId}, Email: {Email}, Role: {Role}, RememberMe: {RememberMe}",
                    user.Id,
                    user.Email,
                    roleNames,
                    user.UserType.ToString(),
                    rememberMe
                );

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim("userName", user.UserName!.ToString()),
                    new Claim("userType", user.UserType.ToString()),
                    new Claim(ClaimTypes.Email, user.Email!),
                };

                foreach (var roleName in roleNames)
                {
                    claims.Add(new Claim(ClaimTypes.Role, roleName));
                }

                var key = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(_jwtSecret));

                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var expirationHours = rememberMe ? 24 : 1;
                var token = new JwtSecurityToken(
                    claims: claims,
                    expires: DateTime.Now.AddHours(expirationHours),
                    signingCredentials: creds
                );

                Log.Information(
                    "Token JWT creado exitosamente para usuario ID: {UserId}, expira en {Hours} horas",
                    user.Id,
                    expirationHours
                );
                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error al crear token JWT para usuario ID: {UserId}", user.Id);
                throw new InvalidOperationException("Error creando el token JWT.", ex);
            }
        }

        /// <summary>
        /// Agrega un token a la whitelist para el usuario dado.
        /// </summary>
        /// <param name="user">Usuario al que se le agrega el token</param>
        /// <param name="token">Token a agregar</param>
        /// <returns>Indica si la operación fue exitosa</returns>
        public async Task<bool> AddToWhitelistAsync(Whitelist token)
        {
            var result = await _tokenRepository.AddToWhitelistAsync(token);
            if (result == null)
            {
                Log.Error(
                    $"Error al agregar token a la whitelist para el usuario ID: {token.UserId}"
                );
                return false;
            }
            Log.Information($"Token agregado a la whitelist para el usuario ID: {token.UserId}");
            return true;
        }

        /// <summary>
        /// Revoca todos los tokens activos para el usuario dado.
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <returns>Indica si la operación fue exitosa</returns>
        public async Task<bool> RevokeAllActiveTokensAsync(int userId)
        {
            var activeTokens = await _tokenRepository.ExistsByUserIdAsync(userId);
            if (!activeTokens)
            {
                Log.Information($"No hay tokens activos para revocar para el usuario ID: {userId}");
                return false; // No hay tokens que revocar
            }

            Log.Information($"Revocando tokens para el usuario ID: {userId}");
            await _tokenRepository.RemoveAllFromWhitelistByUserIdAsync(userId);
            Log.Information($"Tokens activos revocados para el usuario ID: {userId}");
            return true;
        }
    }
}
