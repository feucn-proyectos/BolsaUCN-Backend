using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.src.Domain.Models;

namespace backend.src.Application.Services.Interfaces
{
    public interface IFileService
    {
        /// <summary>
        /// Sube un archivo a Cloudinary.
        /// </summary>
        /// <param name="file">El archivo a subir.</param>
        /// <param name="productId">El ID del producto al que pertenece la imagen.</param>
        /// <returns>True si la carga fue exitosa, de lo contrario False.</returns>
        Task<bool> UploadAsync(IFormFile file, int productId);

        /// <summary>
        /// Sube una imagen de usuario a Cloudinary.
        /// </summary>
        /// <param name="file">El archivo de imagen a subir.</param>
        /// <param name="userId">El ID del usuario al que pertenece la imagen.</param>
        /// <param name="imageType">El tipo de imagen de usuario.</param>
        /// <returns>True si la carga fue exitosa, de lo contrario False.</returns>
        Task<bool> UploadUserImageAsync(IFormFile file, User generalUser);

        /// <summary>
        /// Elimina un archivo de Cloudinary.
        /// </summary>
        /// <param name="publicId">El ID público del archivo a eliminar.</param>
        /// <returns>True si la eliminación fue exitosa, de lo contrario false.</returns>
        Task<bool> DeleteAsync(string publicId);
    }
}
