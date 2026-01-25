using backend.src.Application.DTOs.PublicationDTO;
using backend.src.Application.DTOs.ReviewDTO;
using backend.src.Domain.Models;
using Mapster;
using Serilog;

namespace backend.src.Application.Mappers
{
    /// <summary>
    /// Mapper para transformar entre entidades de dominio Review y sus DTOs correspondientes.
    /// Proporciona métodos para mapear datos en ambas direcciones y actualizar entidades existentes.
    /// </summary>
    public static class ReviewMapper
    {
        /// <summary>
        /// Actualiza una reseña existente con la evaluación del oferente hacia el estudiante.
        /// Marca la evaluación del oferente como completada.
        /// </summary>
        /// <param name="dto">DTO con la calificación y comentarios del oferente para el estudiante.</param>
        /// <param name="review">La entidad de reseña a actualizar.</param>
        /// <returns>La reseña actualizada con los datos del oferente.</returns>
        public static Review StudentUpdateReview(ReviewForStudentDTO dto, Review review)
        {
            review.RatingForStudent = dto.RatingForStudent;
            review.CommentForStudent = dto.CommentForStudent;
            review.ReviewChecklistValues.AtTime = dto.AtTime;
            review.ReviewChecklistValues.GoodPresentation = dto.GoodPresentation;
            review.ReviewChecklistValues.StudentHasRespectOfferor = dto.StudentHasRespectOfferor;
            review.IsReviewForStudentCompleted = true;
            review.IsCompleted =
                review.IsReviewForStudentCompleted && review.IsReviewForOfferorCompleted;
            return review;
        }

        /// <summary>
        /// Actualiza una reseña existente con la evaluación del estudiante hacia el oferente.
        /// Marca la evaluación del estudiante como completada.
        /// </summary>
        /// <param name="dto">DTO con la calificación y comentarios del estudiante para el oferente.</param>
        /// <param name="review">La entidad de reseña a actualizar.</param>
        /// <returns>La reseña actualizada con los datos del estudiante.</returns>
        public static Review OfferorUpdateReview(ReviewForOfferorDTO dto, Review review)
        {
            review.RatingForOfferor = dto.RatingForOfferor;
            review.CommentForOfferor = dto.CommentForOfferor;
            review.IsReviewForOfferorCompleted = true;
            review.IsCompleted =
                review.IsReviewForStudentCompleted && review.IsReviewForOfferorCompleted;
            return review;
        }

        /// <summary>
        /// Crea una reseña inicial en estado pendiente a partir de un DTO inicial.
        /// Establece automáticamente la ventana de revisión en 14 días desde la creación.
        /// Las evaluaciones deben completarse posteriormente por ambas partes.
        /// </summary>
        /// <param name="dto">DTO con los identificadores del estudiante, oferente y publicación.</param>
        /// <returns>Una nueva entidad Review con estado inicial y ventana de revisión de 14 días.</returns>
        public static Review CreateInitialReviewAsync(
            InitialReviewDTO dto,
            User student,
            User offeror
        )
        {
            return new Review
            {
                StudentId = dto.StudentId,
                Student = student,
                OfferorId = dto.OfferorId,
                Offeror = offeror,
                PublicationId = dto.PublicationId,
            };
        }

        // TODO: Revisar si es necesario este método
        // public static Review ToEntity(ReviewDTO dto)
        // {
        //     return new Review
        //     {
        //         Rating = dto.Rating,
        //         Comment = dto.Comment,
        //         StudentId = dto.StudentId,
        //         ProviderId = dto.ProviderId
        //     };
        // }
        /// <summary>
        /// Convierte un DTO de reseña a una entidad Review del dominio.
        /// </summary>
        /// <param name="dto">El DTO de reseña a convertir.</param>
        /// <returns>Una entidad Review con los datos del DTO.</returns>
        public static ReviewDTO ToDTO(Review entity)
        {
            return new ReviewDTO
            {
                IdReview = entity.Id,
                RatingForStudent = entity.RatingForStudent,
                CommentForStudent = entity.CommentForStudent,
                RatingForOfferor = entity.RatingForOfferor,
                CommentForOfferor = entity.CommentForOfferor,
                AtTime = entity.ReviewChecklistValues.AtTime,
                GoodPresentation = entity.ReviewChecklistValues.GoodPresentation,
                StudentHasRespectOfferor = entity.ReviewChecklistValues.StudentHasRespectOfferor,
                IdStudent = entity.StudentId,
                IdOfferor = entity.OfferorId,
                IdPublication = entity.PublicationId,
                HasReviewForOfferorBeenDeleted = entity.HasReviewForOfferorBeenDeleted,
                HasReviewForStudentBeenDeleted = entity.HasReviewForStudentBeenDeleted,
                IsComplete = entity.IsCompleted,
            };
        }

        public static ShowReviewDTO ShowReviewDTO(Review entity)
        {
            // TODO: Manejar en caso donde las relaciones sean nulas.
            return new ShowReviewDTO
            {
                IdReview = entity.Id,
                StudentName = entity.Student?.UserName ?? "Estudiante desconocido",
                OfferorName = entity.Offeror?.UserName ?? "Oferente desconocido",
                RatingForStudent = entity.RatingForStudent ?? 0,
                CommentForStudent = entity.CommentForStudent ?? string.Empty,
                RatingForOfferor = entity.RatingForOfferor ?? 0,
                CommentForOfferor = entity.CommentForOfferor ?? string.Empty,
                AtTime = entity.ReviewChecklistValues.AtTime,
                GoodPresentation = entity.ReviewChecklistValues.GoodPresentation,
                StudentHasRespectOfferor = entity.ReviewChecklistValues.StudentHasRespectOfferor,
                IsCompleted = entity.IsCompleted,
                IsReviewForStudentCompleted = entity.IsReviewForStudentCompleted,
                IsReviewForOfferorCompleted = entity.IsReviewForOfferorCompleted,
                HasReviewForOfferorBeenDeleted = entity.HasReviewForOfferorBeenDeleted,
                HasReviewForStudentBeenDeleted = entity.HasReviewForStudentBeenDeleted,
                IsClosed = entity.IsClosed,
            };
        }

        public static PublicationAndReviewInfoDTO MapToPublicationAndReviewInfoDTO(
            Review review,
            Publication publication,
            UserType userType
        )
        {
            var reviewDto = ShowReviewDTO(review);
            var publicationDto = publication.Adapt<PublicationsDTO>();
            // Ocultar datos según el tipo de usuario y el estado de la review
            var bool1 = !reviewDto.IsClosed && userType != UserType.Administrador;
            var bool2 =
                !review.HasReviewForOfferorBeenDeleted && !review.HasReviewForStudentBeenDeleted;
            if (bool1 && bool2)
            {
                // Si es oferente, no ha completado su review pero el estudiante si
                if (
                    (userType == UserType.Empresa || userType == UserType.Particular)
                    && reviewDto.IsReviewForOfferorCompleted
                )
                {
                    reviewDto.RatingForOfferor = 0;
                    reviewDto.CommentForOfferor =
                        "No podrás ver la reseña de la contraparte si aún no realizas la tuya.";
                }
                // Si es estudiante y no ha completado su review pero el oferente si
                else if (userType == UserType.Estudiante && reviewDto.IsReviewForStudentCompleted)
                {
                    // Si la Review no esta completada,
                    reviewDto.RatingForStudent = 0;
                    reviewDto.CommentForStudent =
                        "No podrás ver la reseña de la contraparte si aún no realizas la tuya.";
                    reviewDto.AtTime = false;
                    reviewDto.GoodPresentation = false;
                    reviewDto.StudentHasRespectOfferor = false;
                }
            }
            return new PublicationAndReviewInfoDTO
            {
                Review = reviewDto,
                Publication = publicationDto,
            };
        }

        public static Review DeleteReviewForOfferor(Review review)
        {
            review.RatingForOfferor = 0;
            review.CommentForOfferor = "Reseña eliminada.";
            review.IsReviewForOfferorCompleted = false;
            review.IsCompleted = false;
            review.HasReviewForOfferorBeenDeleted = true;
            Log.Information("Deleted offeror part of review ID {ReviewId}", review.Id);
            return review;
        }

        public static Review DeleteReviewForStudent(Review review)
        {
            review.RatingForStudent = 0;
            review.CommentForStudent = "Reseña eliminada.";
            review.ReviewChecklistValues.AtTime = false;
            review.ReviewChecklistValues.GoodPresentation = false;
            review.ReviewChecklistValues.StudentHasRespectOfferor = false;
            review.IsReviewForStudentCompleted = false;
            review.IsCompleted = false;
            review.HasReviewForStudentBeenDeleted = true;
            Log.Information("Deleted student part of review ID {ReviewId}", review.Id);
            return review;
        }
    }
}
