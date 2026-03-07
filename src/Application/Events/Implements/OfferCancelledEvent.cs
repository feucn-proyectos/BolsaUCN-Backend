namespace backend.src.Application.Events.Implements
{
    public class OfferCancelledEvent : DomainEvent
    {
        public required string ApplicantEmail { get; set; }
        public required string OfferTitle { get; set; }
        public required bool CancelledByAdmin { get; set; }
        public required string CancelReason { get; set; }
    }
}
