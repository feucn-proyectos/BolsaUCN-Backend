namespace backend.src.Application.Events.Implements
{
    public class InitialReviewsCreatedEvent : DomainEvent
    {
        public int OfferId { get; set; }
        public required string OfferName { get; set; }
        public required string OfferorEmail { get; set; }
        public List<string> ApplicantEmails { get; set; } = new List<string>();
        public required int ReviewsCreatedCount { get; set; }
        public required int DaysUntilReviewAutoClose { get; set; }
    }
}
