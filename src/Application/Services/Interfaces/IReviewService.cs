using backend.src.Application.DTOs.ReviewDTO;
using backend.src.Application.DTOs.ReviewDTO.AdminDTOs;
using backend.src.Application.DTOs.ReviewDTO.CreateReviewDTOs;
using backend.src.Application.DTOs.ReviewDTO.MyReviewsDTO;
using backend.src.Domain.Models;

namespace backend.src.Application.Services.Interfaces
{
    /// <summary>
    /// Interfaz que define la lógica de negocio para la gestión de reseñas.
    /// Proporciona operaciones para crear, actualizar y consultar reseñas entre oferentes y estudiantes.
    /// </summary>
    public interface IReviewService
    {
        #region Create Reviews

        /// <summary>
        /// Crea las reseñas iniciales para una oferta completada, asignando una reseña pendiente tanto para el oferente como para el postulante.
        /// Estas reseñas se completarán posteriormente cuando ambas partes hayan dejado su evaluación, o cuando se llegue a la fecha de cierre guardada en la oferta.
        /// </summary>
        /// <param name="publicationId">El identificador de la publicación (oferta) para la cual se crearán las reseñas iniciales.</param>
        /// <returns>El número de reseñas creadas.</returns>
        Task<int> CreateInitialReviewsForCompletedOfferAsync(int publicationId);

        /// <summary>
        /// Crea una reseña de un postulante hacia un oferente para una oferta específica.
        /// </summary>
        /// <param name="reviewId">El identificador de la reseña donde se esta guardando la informacion.</param>
        /// <param name="applicantId">El identificador del postulante que crea la reseña.</param>
        /// <param name="reviewDTO">Los datos de la reseña a crear.</param>
        /// <returns>Mensaje de resultado de la operación.</returns>
        Task<string> CreateApplicantReviewForOfferorAsync(
            int reviewId,
            int applicantId,
            ApplicantReviewForOfferorDTO reviewDTO
        );

        /// <summary>
        /// Crea una reseña de un oferente hacia un postulante para una oferta específica.
        /// </summary>
        /// <param name="reviewId">El identificador de la reseña donde se esta guardando la informacion.</param>
        /// <param name="offerorId">El identificador del oferente que crea la reseña.</param>
        /// <param name="reviewDTO">Los datos de la reseña a crear.</param>
        /// <returns>Mensaje de resultado de la operación.</returns>
        Task<string> CreateOfferorReviewForApplicantAsync(
            int reviewId,
            int offerorId,
            OfferorReviewForApplicantDTO reviewDTO
        );
        #endregion

        #region Get Reviews

        /// <summary>
        /// Obtiene las reseñas asociadas a un usuario específico, ya sea como oferente o como postulante, con soporte para paginación y filtrado.
        /// </summary>
        /// <param name="searchParams">Los parámetros de búsqueda para filtrar las reseñas.</param>
        /// <param name="userId">El identificador del usuario que está consultando sus reseñas.</param>
        /// <returns>Un objeto DTO con las reseñas encontradas y la información de paginación.</returns>
        Task<MyReviewsDTO> GetMyReviewsAsync(MyReviewsSearchParamsDTO searchParams, int userId);

        /// <summary>
        /// Obtiene los detalles de una reseña específica para un usuario, asegurando que el usuario tenga permiso para ver esa reseña (es decir, que sea parte de la reseña como oferente o postulante).
        /// </summary>
        /// <param name="reviewId">El identificador de la reseña.</param>
        /// <param name="userId">El identificador del usuario que está consultando los detalles de la reseña.</param>
        /// <returns>Un objeto DTO con los detalles de la reseña encontrada.</returns>
        Task<MyReviewDetailsDTO> GetMyReviewDetailsByIdAsync(int reviewId, int userId);

        /// <summary>
        /// Obtiene todas las reseñas asociadas a un administrador específico, con soporte para paginación y filtrado, para que el administrador pueda gestionar las reseñas (por ejemplo, ocultar información inapropiada).
        /// </summary>
        /// <param name="adminId">El identificador del administrador que está consultando las reseñas.</param>
        /// <param name="searchParams">Los parámetros de búsqueda para filtrar las reseñas.</param>
        /// <returns>Un objeto DTO con las reseñas encontradas y la información de paginación.</returns>
        Task<GetReviewsDTO> GetAllReviewsForAdminAsync(
            int adminId,
            GetReviewsSearchParamsDTO searchParams
        );

        /// <summary>
        /// Obtiene los detalles de una reseña específica para un administrador, asegurando que el administrador tenga permiso para ver esa reseña.
        /// </summary>
        /// <param name="reviewId">El identificador de la reseña.</param>
        /// <param name="adminId">El identificador del administrador que está consultando los detalles de la reseña.</param>
        /// <returns>Un objeto DTO con los detalles de la reseña encontrada.</returns>
        Task<GetReviewDetailsDTO> GetReviewDetailsForAdminByIdAsync(int reviewId, int adminId);
        #endregion

        #region Auxiliary Methods for Review Management

        /// <summary>
        /// Obtiene el conteo de reseñas pendientes para un usuario específico, con la opción de filtrar por rol (oferente o postulante).
        /// Este rol se refiere especificamente a la función del usuario en la reseña pendiente:
        /// Si el rol es "offeror", se contarán las reseñas pendientes donde el usuario es el oferente; si el rol es "applicant", se contarán las reseñas pendientes donde el usuario es el postulante.
        /// Si no se especifica un rol, se contarán todas las reseñas pendientes para el usuario, independientemente de su rol.
        /// </summary>
        /// <param name="user">El usuario para el cual se está contando las reseñas pendientes.</param>
        /// <param name="role">El rol del usuario (opcional, puede ser Oferente o Postulante: Use RoleNames constant).</param>
        /// <returns>El número de reseñas pendientes para el usuario.</returns>
        Task<int> GetPendingReviewsCountAsync(User user, string? role = null);

        /// <summary>
        /// Oculta información específica de una reseña, como el comentario o la calificación, para que no sea visible para los usuarios que consultan la reseña. Esta operación solo puede ser realizada por un administrador.
        /// </summary>
        /// <param name="reviewId">El identificador de la reseña cuya información se ocultará.</param>
        /// <param name="adminId">El identificador del administrador que está ocultando la información de la reseña.</param>
        /// <param name="infoDTO">Los datos de la reseña que se ocultara (Oferente o Postulante) y el comentario con la razon.</param>
        /// <returns>Un mensaje indicando el resultado de la operación.</returns>
        Task<string> HideReviewInfoAsync(int reviewId, int adminId, HideReviewInfoDTO infoDTO);
        #endregion
    }
}
