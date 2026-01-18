namespace backend.src.Domain.Options
{
    public class PublicationQueryOptions
    {
        public bool TrackChanges { get; set; } = false;
        public bool IncludeImages { get; set; } = false;
        public bool IncludeUser { get; set; } = false;
    }
}
