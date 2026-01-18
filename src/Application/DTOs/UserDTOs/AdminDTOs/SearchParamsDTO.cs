using System.ComponentModel.DataAnnotations;

namespace backend.src.Application.DTOs.UserDTOs.AdminDTOs
{
    public class SearchParamsDTO
    {
        /// <summary>
        /// Término de búsqueda para filtrar usuarios por nombre, correo electrónico, RUT o número de teléfono.
        /// </summary>
        [MaxLength(
            100,
            ErrorMessage = "El término de búsqueda no puede exceder los 100 caracteres."
        )]
        public string? SearchTerm { get; set; }

        /// <summary>
        /// Tipo de usuario para filtrar (Student, Individual, Company, Admin).
        /// </summary>
        [RegularExpression(
            "Estudiante|Particular|Empresa|Administrador",
            ErrorMessage = "Tipo de usuario inválido."
        )]
        public string? UserType { get; set; }

        /// <summary>
        /// Estado bloqueado para filtrar (Blocked, Unblocked).
        /// </summary>
        [RegularExpression("Blocked|Unblocked", ErrorMessage = "Estado bloqueado inválido.")]
        public string? BlockedStatus { get; set; }

        /// <summary>
        /// Campo por el cual ordenar los resultados.
        /// </summary>
        [RegularExpression(
            "UserName|Email|Rut|UserType|Rating|Banned",
            ErrorMessage = "Campo de ordenamiento inválido."
        )]
        public string? SortBy { get; set; }

        /// <summary>
        /// Orden de los resultados: 'asc' para ascendente, 'desc' para descendente.
        /// </summary>
        [RegularExpression("asc|desc", ErrorMessage = "Orden de ordenamiento inválido.")]
        public string? SortOrder { get; set; }

        /// <summary>
        /// Número de página para la paginación.
        /// </summary>
        [Range(1, int.MaxValue, ErrorMessage = "Numero de pagina debe ser al menos 1.")]
        public int PageNumber { get; set; }

        /// <summary>
        /// Tamaño de página para la paginación.
        /// </summary>
        [Range(1, 100, ErrorMessage = "Tamaño de página debe estar entre 1 y 100.")]
        public int? PageSize { get; set; }
    }
}
