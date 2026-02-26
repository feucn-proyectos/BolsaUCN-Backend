namespace backend.src.Application.DTOs.ReviewDTO.MyReviewsDTO
{
    /// <summary>
    /// DTO que representa las reseñas asociadas a un usuario específico, ya sea como oferente o estudiante.
    /// Contiene información sobre las reseñas realizadas por el usuario y las recibidas por el usuario.
    /// </summary>
    public class MyReviewsDTO
    {
        public required List<MyReviewDTO> Reviews { get; set; }
        public required int TotalCount { get; set; }
        public required int TotalPages { get; set; }
        public required int CurrentPage { get; set; }
        public required int PageSize { get; set; }
    }
}
