using backend.src.Domain.Models;

namespace backend.src.Application.Services.Interfaces
{
    public interface IDocumentStorageProvider
    {
        /// <summary>
        /// Guarda el CV de un usuario.
        /// </summary>
        /// <param name="cvFile">El archivo del CV a subir.</param>
        /// <param name="generalUser">El usuario al que pertenece el CV.</param>
        /// <returns>La dirección del CV subido.</returns>
        Task<bool> UploadCVAsync(IFormFile cvFile, User user);

        /// <summary>
        /// Elimina el CV de un usuario.
        /// </summary>
        /// <param name="generalUser">El usuario al que pertenece el CV.</param>
        /// <returns>Mensaje indicando el resultado de la eliminación.</returns>
        Task<bool> DeleteCVAsync(User user);

        /// <summary>
        /// Descarga el CV de un usuario.
        /// </summary>
        /// <param name="generalUser">El usuario al que pertenece el CV.</param>
        /// <returns>El archivo del CV en formato byte array, o null si no existe.</returns>
        Task<Curriculum?> DownloadCVAsync(User user);

        /// <summary>
        /// Verifica si un usuario tiene un CV subido.
        /// </summary>
        /// <param name="generalUser">El usuario a verificar.</param>
        /// <returns>True si el usuario tiene un CV subido, de lo contrario False.</returns>
        Task<bool> CVExistsAsync(User user);
    }
}
