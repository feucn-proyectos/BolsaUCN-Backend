namespace backend.src.Application.DTOs.OfferDTOs
{
    public class OfferDetailsDto
    {
        /// <summary>
        /// Unique identifier of the offer detail DTO.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Title or position name for the offer.
        /// </summary>
        public string Titulo { get; set; } = string.Empty; // Cargo

        /// <summary>
        /// Full description of the offer.
        /// </summary>
        public string Descripcion { get; set; } = string.Empty;

        /// <summary>
        /// Requirements or qualifications required by the offer.
        /// </summary>
        public string Requisitos { get; set; } = string.Empty;

        /// <summary>
        /// Duration of the position (e.g., 6 months, indefinite).
        /// </summary>
        public string Duracion { get; set; } = string.Empty;

        /// <summary>
        /// Contact email for the offer.
        /// </summary>
        public string CorreoContacto { get; set; } = string.Empty;

        /// <summary>
        /// Contact phone number for the offer.
        /// </summary>
        public string TelefonoContacto { get; set; } = string.Empty;

        /// <summary>
        /// Indicates whether the offer is active and accepting applications.
        /// </summary>
        public bool Activa { get; set; } // Indica si está disponible para postular
    }
}
