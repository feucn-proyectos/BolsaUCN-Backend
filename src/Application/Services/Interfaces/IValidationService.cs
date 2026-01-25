using backend.src.Application.DTOs.PublicationDTO.ValidationDTOs;

namespace backend.src.Application.Services.Interfaces
{
    public interface IValidationService
    {
        /// <summary>
        /// Valida o rechaza una publicación (oferta o compra/venta) según la acción indicada
        /// </summary>
        /// <param name="adminUserId">ID del administrador que realiza la validación</param>
        /// <param name="publicationId">ID de la publicación a validar</param>
        /// <param name="type">Tipo de publicación (oferta o compra/venta)</param>
        /// <param name="action">Acción a realizar (publicar o rechazar)</param>
        /// <returns>Respuesta indicando si la validación fue exitosa con la ID de la publicación</returns>
        Task<ValidationResponseDTO> ValidatePublication(
            int userId,
            int publicationId,
            string action
        );

        /// <summary>
        /// Obtiene las publicaciones pendientes para validación con paginación y filtros
        /// </summary>
        /// <param name="adminUserId">ID del administrador que realiza la validación</param>
        /// <param name="searchParamsDTO">Parámetros de búsqueda y paginación</param>
        /// <returns>Publicaciones pendientes para validación</returns>
        Task<PublicationsForValidationDTO> GetPublicationsForValidationAsync(
            int userId,
            SearchParamsDTO searchParamsDTO
        );
    }
}
