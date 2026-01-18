using backend.src.Domain.Models;

namespace backend.src.Application.DTOs.PublicationDTO
{
    /// <summary>
    /// DTO básico de una publicación de compra/venta para administradores
    /// </summary>
    public class BuySellBasicAdminDto
    {
        /// <summary>
        /// Identificador único de la publicación
        /// </summary>
        public required int Id { get; set; }

        /// <summary>
        /// Título de la publicación
        /// </summary>
        public required string Title { get; set; }

        /// <summary>
        /// Nombre del propietario de la publicación
        /// </summary>
        public required string NameOwner { get; set; }

        /// <summary>
        /// Fecha de publicación
        /// </summary>
        public required DateTime PublicationDate { get; set; }

        /// <summary>
        /// Tipo de publicación (Compra o Venta)
        /// </summary>
        public required Types Type { get; set; }

        /// <summary>
        /// Indica si la publicación está activa o no
        /// </summary>
        public required bool IsActive { get; set; }
    }
}
