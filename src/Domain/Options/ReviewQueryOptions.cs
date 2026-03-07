namespace backend.src.Domain.Options
{
    public class ReviewQueryOptions
    {
        public bool TrackChanges { get; set; } = false;
        public bool IncludeApplication { get; set; } = false;
        public bool IncludeOfferor { get; set; } = false;
        public bool IncludeApplicant { get; set; } = false;
    }
}
