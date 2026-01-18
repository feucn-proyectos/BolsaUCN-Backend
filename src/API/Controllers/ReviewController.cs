using System.Security.Claims;
using backend.src.Application.DTOs.ReviewDTO;
using backend.src.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace backend.src.API.Controllers
{
    [ApiController]
    [Route("api/review")]
    /// <summary>
    /// Controlador para gestionar las reseñas entre oferentes y estudiantes.
    /// </summary>
    public class ReviewController(
        IReviewService reviewService,
        IPdfGeneratorService pdfGeneratorService
    ) : BaseController
    {
        private readonly IReviewService _reviewService = reviewService;
        private readonly IPdfGeneratorService _pdfGeneratorService = pdfGeneratorService;

        #region Endpoints de Usuario (Applicant/Offerent)

        /// <summary>
        /// Obtiene todas las reseñas del usuario autenticado.
        /// Funciona tanto para estudiantes (Applicant) como para oferentes (Offerent).
        /// El usuario solo puede ver sus propias reseñas.
        /// </summary>
        /// <returns>Lista de reseñas del usuario autenticado</returns>
        [HttpGet("my-reviews")]
        [Authorize(Roles = "Applicant,Offerent")]
        public async Task<IActionResult> GetMyReviews()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (
                string.IsNullOrEmpty(userIdClaim)
                || !int.TryParse(userIdClaim, out int currentUserId)
            )
            {
                return Unauthorized("No se pudo identificar al usuario autenticado.");
            }

            var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

            if (userRole == "Applicant")
            {
                var reviews = await _reviewService.GetReviewsByStudentAsync(currentUserId);
                return Ok(reviews);
            }
            else if (userRole == "Offerent")
            {
                var reviews = await _reviewService.GetReviewsByOfferorAsync(currentUserId);
                return Ok(reviews);
            }

            return Unauthorized("El usuario no tiene un rol válido para ver reseñas.");
        }

        /// <summary>
        /// Genera un PDF con todas las reviews del usuario autenticado.
        /// El PDF incluye un resumen con el promedio de calificación y el detalle de cada review.
        /// </summary>
        /// <returns>Archivo PDF con el reporte de calificaciones</returns>
        [HttpGet("my-reviews/pdf")]
        [Authorize(Roles = "Applicant,Offerent")]
        public async Task<IActionResult> GetMyReviewsPdf()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (
                string.IsNullOrEmpty(userIdClaim)
                || !int.TryParse(userIdClaim, out int currentUserId)
            )
            {
                return Unauthorized("No se pudo identificar al usuario autenticado.");
            }

            try
            {
                var pdfBytes = await _pdfGeneratorService.GenerateUserReviewsPdfAsync(
                    currentUserId
                );
                return File(
                    pdfBytes,
                    "application/pdf",
                    $"reviews_{currentUserId}_{DateTime.Now:yyyyMMdd}.pdf"
                );
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error generando PDF: {ex.Message}");
            }
        }

        /// <summary>
        /// Obtiene la información de las publicaciones asociadas a las reseñas del usuario autenticado.
        /// Identifica automáticamente si el usuario es estudiante u oferente.
        /// </summary>
        /// <returns>Una lista de publicaciones asociadas a las reseñas del usuario.</returns>
        [HttpGet("publications")]
        [Authorize(Roles = "Applicant,Offerent")]
        public async Task<IActionResult> GetMyPublicationInformation()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdString) || !int.TryParse(userIdString, out int userId))
            {
                return Unauthorized("No se pudo identificar al usuario.");
            }

            var publicationInfo = await _reviewService.GetPublicationInformationAsync(userId);
            return Ok(publicationInfo);
        }

        /// <summary>
        /// Agrega la reseña del oferente hacia el estudiante.
        /// </summary>
        /// <param name="dto">DTO con la calificación y comentarios para el estudiante</param>
        /// <returns></returns>
        [HttpPost("AddStudentReview")]
        [Authorize(Roles = "Offerent")]
        public async Task<IActionResult> AddStudentReview([FromBody] ReviewForStudentDTO dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (
                string.IsNullOrEmpty(userIdClaim)
                || !int.TryParse(userIdClaim, out int currentUserId)
            )
            {
                return Unauthorized("No se pudo identificar al usuario autenticado.");
            }

            await _reviewService.AddStudentReviewAsync(dto, currentUserId);
            return Ok("Student review added successfully");
        }

        /// <summary>
        /// Agrega la reseña del estudiante hacia el oferente.
        /// </summary>
        /// <param name="dto">DTO con la calificación y comentarios para el oferente</param>
        /// <returns></returns>
        [HttpPost("AddOfferorReview")]
        [Authorize(Roles = "Applicant")]
        public async Task<IActionResult> AddOfferorReview([FromBody] ReviewForOfferorDTO dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (
                string.IsNullOrEmpty(userIdClaim)
                || !int.TryParse(userIdClaim, out int currentUserId)
            )
            {
                return Unauthorized("No se pudo identificar al usuario autenticado.");
            }

            await _reviewService.AddOfferorReviewAsync(dto, currentUserId);
            return Ok("Offeror review added successfully");
        }

        #endregion

        #region Endpoints Públicos

        /// <summary>
        /// Obtiene todas las reseñas para un oferente específico.
        /// </summary>
        /// <param name="offerorId">ID del oferente</param>
        /// <returns>Lista de reseñas del oferente</returns>
        [HttpGet("offeror/{offerorId}")]
        public async Task<IActionResult> GetReviewsByOfferorId(int offerorId)
        {
            var reviews = await _reviewService.GetReviewsByOfferorAsync(offerorId);
            return Ok(reviews);
        }

        /// <summary>
        /// Obtiene todas las reseñas para un estudiante específico.
        /// </summary>
        /// <param name="studentId">ID del estudiante</param>
        /// <returns>Lista de reseñas del estudiante</returns>
        [HttpGet("student/{studentId}")]
        public async Task<IActionResult> GetReviewsByStudentId(int studentId)
        {
            var reviews = await _reviewService.GetReviewsByStudentAsync(studentId);
            return Ok(reviews);
        }

        /// <summary>
        /// Obtiene una reseña específica por su ID.
        /// </summary>
        /// <param name="id">ID de la reseña</param>
        /// <returns>Detalle de la reseña</returns>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetReview(int id)
        {
            var review = await _reviewService.GetReviewAsync(id);
            return Ok(review);
        }

        /// <summary>
        /// Obtiene la calificación promedio de un usuario específico.
        /// </summary>
        /// <param name="userId">ID del usuario</param>
        /// <returns>Promedio de calificación del usuario</returns>
        [HttpGet("rating/{userId}")]
        public async Task<IActionResult> GetRating(int userId)
        {
            var avg = await _reviewService.GetUserAverageRatingAsync(userId);
            return Ok(avg);
        }

        /// <summary>
        /// Agrega una nueva reseña inicial.
        /// </summary>
        /// <param name="dto">DTO con la información inicial de la reseña</param>
        /// <returns>El ID de la reseña creada</returns>
        [HttpPost("AddInitialReview")]
        public async Task<IActionResult> AddInitialReview([FromBody] InitialReviewDTO dto)
        {
            var review = await _reviewService.CreateInitialReviewAsync(dto);
            return Ok(new { reviewId = review.Id, message = "Initial review added successfully" });
        }

        /// <summary>
        /// Agrega una nueva reseña (método legacy - no implementado).
        /// </summary>
        /// <param name="dto">DTO con la información de la reseña</param>
        /// <returns></returns>
        [HttpPost("AddReview")]
        public async Task<IActionResult> AddReview([FromBody] ReviewDTO dto)
        {
            await _reviewService.AddReviewAsync(dto);
            return Ok("Review added successfully");
        }

        #endregion

        #region Endpoints de Administrador

        /// <summary>
        /// Obtiene todas las reseñas del sistema.
        /// Solo accesible para administradores.
        /// </summary>
        /// <returns>Lista completa de reseñas</returns>
        [HttpGet("Admin/system-reviews")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllReviews()
        {
            var reviews = await _reviewService.GetAllReviewsAsync();
            return Ok(reviews);
        }

        /// <summary>
        /// Genera un PDF con todas las reviews de un usuario específico.
        /// Solo accesible para administradores.
        /// El PDF incluye un resumen con el promedio de calificación y el detalle de cada review.
        /// </summary>
        /// <param name="userId">ID del usuario para generar el reporte</param>
        /// <returns>Archivo PDF con el reporte del usuario</returns>
        [HttpGet("admin/user-reviews/pdf")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetUserReviewsPdf(int userId)
        {
            try
            {
                var pdfBytes = await _pdfGeneratorService.GenerateUserReviewsPdfAsync(userId);
                return File(
                    pdfBytes,
                    "application/pdf",
                    $"reviews_{userId}_{DateTime.Now:yyyyMMdd}.pdf"
                );
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error generando PDF: {ex.Message}");
            }
        }

        /// <summary>
        /// Genera un PDF con todas las reviews del sistema.
        /// Solo accesible para administradores.
        /// El PDF incluye estadísticas generales y todas las reviews ordenadas por fecha (más recientes primero).
        /// </summary>
        /// <returns>Archivo PDF con el reporte general del sistema</returns>
        [HttpGet("Admin/system-reviews/pdf")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetSystemReviewsPdf()
        {
            try
            {
                var pdfBytes = await _pdfGeneratorService.GenerateSystemReviewsPdfAsync();
                return File(
                    pdfBytes,
                    "application/pdf",
                    $"system_reviews_{DateTime.Now:yyyyMMdd}.pdf"
                );
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Error generando PDF del sistema: {ex.Message}");
            }
        }

        /// <summary>
        /// Elimina una parte de la reseña (estudiante y/o oferente).
        /// Solo accesible para administradores.
        /// </summary>
        /// <param name="dto">DTO especificando qué partes eliminar</param>
        /// <returns></returns>
        [HttpPost("Admin/DeleteReviewPart")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteReviewPart([FromBody] DeleteReviewPartDTO dto)
        {
            await _reviewService.DeleteReviewPartAsync(dto);
            return Ok("Review part(s) deleted successfully");
        }

        #endregion
    }
}
