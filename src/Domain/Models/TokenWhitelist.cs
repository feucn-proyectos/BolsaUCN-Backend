namespace backend.src.Domain.Models
{
    public class Whitelist : ModelBase
    {
        public required int UserId { get; set; }
        public required string Email { get; set; }
        public DateTime Expiration { get; set; } = DateTime.UtcNow.AddHours(1);
        public required string Token { get; set; }
    }
}
