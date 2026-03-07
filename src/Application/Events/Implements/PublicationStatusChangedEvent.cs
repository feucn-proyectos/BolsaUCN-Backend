namespace backend.src.Application.Events.Implements
{
    public class PublicationStatusChangedEvent : DomainEvent
    {
        public int PublicationId { get; set; }
        public required string OfferorEmail { get; set; }
        public required string PublicationTitle { get; set; }
        public required string NewStatus { get; set; }
    }
}