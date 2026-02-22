namespace backend.src.Domain.Models
{
    public enum ReviewStatus
    {
        Pendiente,
        OferenteEvaluoEstudiante,
        EstudianteEvaluoOferente,
        Completada,
        Cerrada,
    }

    public class NewReview : ModelBase
    {
        // === Atributos de navegación principales ===
        //  Sirven para relacionar la revisión con la solicitud de trabajo que se está revisando.
        // La solicitud es donde se encuentra el "contrato" con toda la informacion necesaria.
        public JobApplication? Application { get; set; }
        public required int ApplicationId { get; set; }

        // === Atributos de navegacion extras ===
        // Sirven para facilitar la consulta de datos relacionados a la revisión.
        public User? Offeror { get; set; }
        public required int OfferorId { get; set; }
        public User? Applicant { get; set; }
        public required int ApplicantId { get; set; }

        // === Atributos de la revisión ===
        public float? OfferorRatingOfApplicant { get; set; }
        public string? OfferorCommentForApplicant { get; set; }
        public float? ApplicantRatingOfOfferor { get; set; }
        public string? ApplicantCommentForOfferor { get; set; }
        public bool IsOnTime { get; set; } = false;
        public bool IsPresentable { get; set; } = false;
        public bool IsRespectful { get; set; } = false;

        // === Atributos de auditoria ===
        // Estos atributos son para llevar un registro de cuando se crean o actualizan las reseñas.
        public DateTime? OfferorReviewCompletedAt { get; set; }
        public DateTime? ApplicantReviewCompletedAt { get; set; }
        public DateTime? ReviewClosedAt { get; set; }

        // Estos atributos son para que el frontend sepa si los comentarios has sido accionados por los administradores y actue acorde
        public bool IsOfferorCommentForApplicantHidden { get; set; } = false;
        public bool IsOfferorRatingOfApplicantHidden { get; set; } = false;
        public DateTime? OfferorReviewHiddenAt { get; set; }
        public string? OfferorReviewHiddenReason { get; set; }
        public bool IsApplicantCommentForOfferorHidden { get; set; } = false;
        public bool IsApplicantRatingOfOfferorHidden { get; set; } = false;
        public DateTime? ApplicantReviewHiddenAt { get; set; }
        public string? ApplicantReviewHiddenReason { get; set; }
        public bool IsReviewActionedByAdmin =>
            OfferorReviewHiddenAt.HasValue
            || IsOfferorCommentForApplicantHidden
            || IsOfferorRatingOfApplicantHidden
            || IsApplicantCommentForOfferorHidden
            || IsApplicantRatingOfOfferorHidden
            || ApplicantReviewHiddenAt.HasValue;

        // === Atributos auxiliares ===
        public bool IsPending => !HasOfferorEvaluatedApplicant || !HasApplicantEvaluatedOfferor;
        public bool HasOfferorEvaluatedApplicant => OfferorRatingOfApplicant.HasValue;
        public bool HasApplicantEvaluatedOfferor => ApplicantRatingOfOfferor.HasValue;
        public bool IsCompleted => HasOfferorEvaluatedApplicant && HasApplicantEvaluatedOfferor;
        public bool IsClosed => IsCompleted || ReviewClosedAt.HasValue;

        public ReviewStatus CurrentStatus
        {
            get
            {
                if (IsClosed)
                    return ReviewStatus.Cerrada;
                if (IsCompleted)
                    return ReviewStatus.Completada;
                if (HasOfferorEvaluatedApplicant && !HasApplicantEvaluatedOfferor)
                    return ReviewStatus.OferenteEvaluoEstudiante;
                if (!HasOfferorEvaluatedApplicant && HasApplicantEvaluatedOfferor)
                    return ReviewStatus.EstudianteEvaluoOferente;
                return ReviewStatus.Pendiente;
            }
        }
    }
}
