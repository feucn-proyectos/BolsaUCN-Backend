using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.src.Application.DTOs.PublicationDTO.ValidationDTOs;
using backend.src.Domain.Models;
using backend.src.Domain.Options;
using Mapster;
using Resend;

namespace backend.src.Infrastructure.Repositories.Interfaces
{
    /// <summary>
    /// Obteniene una publicacion cual sea, por la id de usuario y el estado de validación de la misma.
    /// </summary>
    public interface IPublicationRepository
    {
        Task<IEnumerable<Publication>> GetPublishedPublicationsByUserIdAsync(string userId);
        Task<IEnumerable<Publication>> GetRejectedPublicationsByUserIdAsync(string userId);
        Task<IEnumerable<Publication>> GetPendingPublicationsByUserIdAsync(string userId);

        Task<Publication?> GetByIdAsync(int id);

        /// <summary>
        /// Actualiza una publicación en la base de datos.
        /// </summary>
        /// <param name="publication">La publicación a actualizar</param>
        Task UpdateAsync(Publication publication);

        /// <summary>
        /// Obtiene una publicación por su ID con opciones de consulta específicas.
        /// </summary>
        /// <param name="publicationId">Id de la publicación</param>
        /// <param name="options">Opciones de consulta</param>
        /// <returns>Retorna la publicación si se encuentra, de lo contrario null</returns>
        Task<Publication?> GetPublicationByIdAsync(
            int publicationId,
            PublicationQueryOptions options
        );

        /// <summary>
        /// Obtiene todas las publicaciones pendientes para validación con paginación y filtros
        /// </summary>
        /// <param name="searchParamsDTO">Parámetros de búsqueda y paginación</param>
        /// <returns></returns>
        Task<(IEnumerable<Publication>, int)> GetAllPendingForApprovalAsync(
            SearchParamsDTO searchParamsDTO
        );
    }
}
