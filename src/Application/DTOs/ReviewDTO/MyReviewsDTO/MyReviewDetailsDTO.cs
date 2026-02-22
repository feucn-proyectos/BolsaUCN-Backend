namespace backend.src.Application.DTOs.ReviewDTO.MyReviewsDTO
{
    public class MyReviewDetailsDTO
    {
        // === Atributos generales ===
        public required int ReviewId { get; set; }
        public float? OfferorRatingOfApplicant { get; set; }
        public string? OfferorCommentForApplicant { get; set; }
        public float? ApplicantRatingOfOfferor { get; set; }
        public string? ApplicantCommentForOfferor { get; set; }
        public bool? IsOnTime { get; set; }
        public bool? IsPresentable { get; set; }
        public bool? IsRespectful { get; set; }
        public DateTime? OfferorReviewCompletedAt { get; set; }
        public DateTime? ApplicantReviewCompletedAt { get; set; }
        public DateTime? ReviewClosedAt { get; set; }

        // === Atributos de visibilidad ===
        public bool? IsOfferorCommentForApplicantHidden { get; set; }
        public DateTime? OfferorReviewHiddenAt { get; set; }
        public bool? IsApplicantCommentForOfferorHidden { get; set; }
        public DateTime? ApplicantReviewHiddenAt { get; set; }

        // === Atributos relacionados a la oferta y postulacion ===
        public required int JobOfferId { get; set; }
        public required string JobOfferTitle { get; set; }
        public required int ApplicationId { get; set; }
        public required sbyte ApplicantId { get; set; }
        public required string ApplicantFullName { get; set; }
        public required sbyte OfferorId { get; set; }
        public required string OfferorFullName { get; set; }

        // === Atributos auxiliares ===
        public required DateTime OpenUntil { get; set; }
        public required string ReviewStatus { get; set; }
    }
}
