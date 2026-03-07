using backend.src.Domain.Models;
using backend.src.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace backend.src.Infrastructure.Repositories.Interfaces
{
    public class VerificationCodeRepository : IVerificationCodeRepository
    {
        private readonly AppDbContext _context;

        public VerificationCodeRepository(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Crea un nuevo código de verificación en la base de datos.
        /// </summary>
        /// <param name="code">El objeto VerificationCode a crear</param>
        /// <returns>El código de verificación creado</returns>
        public async Task<VerificationCode> CreateCodeAsync(VerificationCode code)
        {
            Log.Information(
                $"Creando código de verificación para usuario ID: {code.UserId}, Tipo: {code.CodeType}"
            );
            await _context.VerificationCodes.AddAsync(code);
            await _context.SaveChangesAsync();
            Log.Information(
                $"Código de verificación creado exitosamente para usuario ID: {code.UserId}, Código ID: {code.Id}"
            );
            return code;
        }

        /// <summary>
        /// Actualiza un código de verificación existente en la base de datos.
        /// </summary>
        /// <param name="code">El objeto VerificationCode con los datos actualizados</param>
        /// <returns>El código de verificación actualizado</returns>
        public async Task<VerificationCode> UpdateCodeAsync(VerificationCode code)
        {
            Log.Information(
                $"Actualizando código de verificación ID: {code.Id} para usuario ID: {code.UserId}"
            );
            await _context
                .VerificationCodes.Where(vc => vc.Id == code.Id)
                .ExecuteUpdateAsync(vc =>
                    vc.SetProperty(v => v.Code, code.Code)
                        .SetProperty(v => v.Attempts, code.Attempts)
                        .SetProperty(v => v.Expiration, code.Expiration)
                );
            await _context.SaveChangesAsync();
            Log.Information($"Código de verificación ID: {code.Id} actualizado exitosamente");
            var newVerificationCode = await _context
                .VerificationCodes.AsNoTracking()
                .FirstOrDefaultAsync(vc => vc.Id == code.Id);
            return newVerificationCode!;
        }

        /// <summary>
        /// Obtiene un código de verificación por su valor y tipo.
        /// </summary>
        /// <param name="code">El valor del código de verificación</param>
        /// <param name="type">El tipo de código de verificación</param>
        /// <returns>El código de verificación encontrado</returns>
        public async Task<VerificationCode> GetByCodeAsync(string code, CodeType type)
        {
            Log.Information($"Obteniendo el codigo con el numero {code} de tipo {type}");
            var verificationCode = await _context
                .VerificationCodes.Where(vc => vc.Code == code && vc.CodeType == type)
                .OrderByDescending(vc => vc.CreatedAt)
                .FirstOrDefaultAsync();
            if (verificationCode != null)
            {
                Log.Information($"Codigo de verificacion con el numero {code} de tipo {type}");
            }
            return verificationCode!;
        }

        /// <summary>
        /// Obtiene el último código de verificación para un usuario dado y tipo de código.
        /// </summary>
        /// <param name="userId">El ID del usuario</param>
        /// <param name="codeType">El tipo de código de verificación</param>
        /// <returns>El último código de verificación encontrado</returns>
        public async Task<VerificationCode> GetByLatestUserIdAsync(int userId, CodeType codeType)
        {
            Log.Information(
                $"Obteniendo último código de verificación para usuario ID: {userId}, Tipo: {codeType}"
            );
            var verificationCode = await _context
                .VerificationCodes.Where(vc => vc.UserId == userId && vc.CodeType == codeType)
                .OrderByDescending(vc => vc.CreatedAt)
                .FirstOrDefaultAsync();

            if (verificationCode != null)
            {
                Log.Information(
                    $"Código de verificación encontrado para usuario ID: {userId}, Código ID: {verificationCode.Id}"
                );
            }
            else
            {
                Log.Warning(
                    $"No se encontró código de verificación para usuario ID: {userId}, Tipo: {codeType}"
                );
            }
            return verificationCode!;
        }

        /// <summary>
        /// Incrementa el número de intentos de verificación para un usuario y tipo de código dado.
        /// </summary>
        /// <param name="userId">El ID del usuario</param>
        /// <param name="codeType">El tipo de código de verificación</param>
        /// <returns>El número actualizado de intentos</returns>
        public async Task<int> IncreaseAttemptsAsync(int userId, CodeType codeType)
        {
            Log.Information(
                $"Incrementando intentos de verificación para usuario ID: {userId}, Tipo: {codeType}"
            );
            var verificationCode = await _context
                .VerificationCodes.Where(vc => vc.UserId == userId && vc.CodeType == codeType)
                .OrderByDescending(vc => vc.CreatedAt)
                .ExecuteUpdateAsync(vc => vc.SetProperty(v => v.Attempts, v => v.Attempts + 1));
            var attempts = await _context
                .VerificationCodes.Where(vc => vc.UserId == userId && vc.CodeType == codeType)
                .OrderByDescending(vc => vc.CreatedAt)
                .Select(vc => vc.Attempts)
                .FirstAsync();
            Log.Information(
                $"Intentos incrementados para usuario ID: {userId}, Total intentos: {attempts}"
            );
            return attempts;
        }

        /// <summary>
        /// Elimina los códigos de verificación para un usuario y tipo de código dado.
        /// </summary>
        /// <param name="userId">El ID del usuario</param>
        /// <param name="codeType">El tipo de código de verificación</param>
        /// <returns>True si la eliminación fue exitosa, de lo contrario False</returns>
        public async Task<bool> DeleteByUserIdAsync(int userId, CodeType codeType)
        {
            Log.Information(
                $"Eliminando códigos de verificación para usuario ID: {userId}, Tipo: {codeType}"
            );
            await _context
                .VerificationCodes.Where(vc => vc.UserId == userId && vc.CodeType == codeType)
                .ExecuteDeleteAsync();
            var exists = await _context.VerificationCodes.AnyAsync(vc =>
                vc.UserId == userId && vc.CodeType == codeType
            );

            if (!exists)
            {
                Log.Information(
                    $"Códigos de verificación eliminados exitosamente para usuario ID: {userId}"
                );
            }
            else
            {
                Log.Warning($"Error al eliminar códigos de verificación para usuario ID: {userId}");
            }
            return !exists;
        }
    }
}
