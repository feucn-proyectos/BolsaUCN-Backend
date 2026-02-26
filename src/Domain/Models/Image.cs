namespace backend.src.Domain.Models
{
    public class Image : ModelBase
    {
        public required string Url { get; set; }
        public required string PublicId { get; set; }
        public int BuySellId { get; set; }
    }
}
