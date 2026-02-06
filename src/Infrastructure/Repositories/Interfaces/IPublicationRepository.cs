using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using backend.src.Application.DTOs.PublicationDTO.MyPublicationsDTOs;
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
        /// <summary>
        /// Crea una nueva publicación en la base de datos.
        /// </summary>
        Task<bool> CreatePublicationAsync<T>(T publication)
            where T : Publication;

        /// <summary>
        /// Revisa si una publicación pertenece a un usuario específico.
        /// </summary>
        Task<bool> CheckOwnershipAsync(int offerorId, int publicationId);

        /// <summary>
        /// Revisa el tipo de publicación (Oferta o CompraVenta) según su Id.
        /// </summary>
        Task<bool?> CheckType(int publicationId, PublicationType type);

        /// <summary>
        /// Actualiza una publicación en la base de datos.
        /// </summary>
        /// <param name="publication">La publicación a actualizar</param>
        Task<bool> UpdateAsync<T>(T publication)
            where T : Publication;

        /// <summary>
        /// Obtiene una publicación por su ID con opciones de consulta específicas.
        /// </summary>
        /// <param name="publicationId">Id de la publicación</param>
        /// <param name="options">Opciones de consulta</param>
        /// <returns>Retorna la publicación si se encuentra, de lo contrario null</returns>
        Task<T?> GetPublicationByIdAsync<T>(
            int publicationId,
            PublicationQueryOptions? options = null
        )
            where T : Publication;

        /// <summary>
        /// Obtiene todas las publicaciones pendientes para validación con paginación y filtros
        /// </summary>
        /// <param name="searchParamsDTO">Parámetros de búsqueda y paginación</param>
        /// <returns></returns>
        Task<(IEnumerable<Publication>, int)> GetAllPendingForApprovalAsync(
            PendingPublicationSearchParamsDTO searchParamsDTO
        );

        /// <summary>
        /// Obtiene las publicaciones del usuario autenticado con filtros y paginación
        /// </summary>
        /// <param name="offerorId">Id del usuario autenticado</param>
        /// <param name="searchParams">Parámetros de búsqueda y paginación</param>
        /// <returns></returns>
        Task<(IEnumerable<Publication>, int)> GetMyPublicationsFilteredByUserIdAsync(
            int offerorId,
            MyPublicationsSeachParamsDTO searchParams
        );
    }
}
