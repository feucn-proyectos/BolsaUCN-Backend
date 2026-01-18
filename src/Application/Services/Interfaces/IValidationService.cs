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
        Task<string> ValidatePublication(
            int adminUserId,
            int publicationId,
            string type,
            string action
        );
    }
}
