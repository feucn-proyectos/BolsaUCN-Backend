namespace backend.src.Application.Events.Implements
{
    public class OfferSlotsFilledEvent : DomainEvent
    {
        public required int OfferId { get; set; }
        public required string OfferTitle { get; set; }
        public required int OfferorId { get; set; }
        public required string CloseApplicationsJobId { get; set; }
    }
}
