namespace backend.src.Application.Events.Implements
{
    public class PublicationClosedByAdminEvent : DomainEvent
    {
        public int PublicationId { get; set; }
        public required string OfferorEmail { get; set; }
        public required string PublicationTitle { get; set; }
        public required string ClosedByAdminReason { get; set; }
    }
}
