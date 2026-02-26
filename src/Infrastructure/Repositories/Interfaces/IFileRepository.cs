using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.src.Domain.Models;

namespace backend.src.Infrastructure.Repositories.Interfaces
{
    public interface IFileRepository
    {
        /// <summary>
        /// Crea un archivo de imagen en la base de datos.
        /// </summary>
        /// <param name="file">El archivo de imagen a crear.</param>
        /// <returns>True si el archivo se creó correctamente, de lo contrario false y null en caso de que la imagen ya existe.</returns>
        Task<bool?> CreateAsync(Image file);

        /// <summary>
        /// Crea un archivo de imagen en la base de datos.
        /// </summary>
        /// <param name="files">La lista de archivos de imagen a crear.</param>
        /// <returns>True si los archivos se crearon correctamente, de lo contrario false.</returns>
        Task<bool> CreateBatchAsync(List<Image> files);

        /// <summary>
        /// Crea un archivo de imagen de usuario en la base de datos.
        /// </summary>
        /// <param name="file">El archivo de imagen a crear.</param>
        /// <returns>True si el archivo se creó correctamente, de lo contrario false y null en caso de que la imagen ya existe.</returns>
        Task<bool?> CreateUserImageAsync(UserImage file);

        /// <summary>
        /// Crea un archivo de CV en la base de datos.
        /// </summary>
        /// <param name="file">El archivo de CV a crear.</param>
        /// <returns>True si el archivo se creó correctamente, de lo contrario false y null en caso de que el CV ya existe.</returns>
        Task<bool> CreateCVAsync(Curriculum file);

        /// <summary>
        /// Actualiza un archivo de CV en la base de datos.
        /// </summary>
        /// <param name="publicId">El identificador público del archivo a actualizar.</param>
        /// <param name="updatedFile">El archivo de CV actualizado.</param>
        /// <returns>True si el archivo se actualizó correctamente, de lo contrario false y null si el archivo no existe.</returns>
        Task<bool?> UpdateCVAsync(string publicId, Curriculum updatedFile);

        /// <summary>
        /// Elimina un archivo de imagen de la base de datos.
        /// </summary>
        /// <param name="publicId">El identificador público del archivo a eliminar.</param>
        /// <returns>True si el archivo se eliminó correctamente, de lo contrario false y null si la imagen no existe.</returns>
        Task<bool?> DeleteAsync(string publicId);

        /// <summary>
        /// Elimina un archivo de imagen de usuario de la base de datos.
        /// </summary>
        /// <param name="publicId">El identificador público del archivo a eliminar.</param>
        /// <returns>True si el archivo se eliminó correctamente, de lo contrario false y null si la imagen no existe.</returns>
        Task<bool?> DeleteUserImageAsync(string publicId);

        /// <summary>
        /// Elimina un archivo de CV de la base de datos.
        /// </summary>
        /// <param name="publicId">El identificador público del archivo a eliminar.</param>
        /// <returns>True si el archivo se eliminó correctamente, de lo contrario false y null si el CV no existe.</returns>
        Task<bool> DeleteCVAsync(string publicId);
    }
}
