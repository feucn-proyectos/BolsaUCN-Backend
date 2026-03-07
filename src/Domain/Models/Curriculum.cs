namespace backend.src.Domain.Models
{
    public class Curriculum : ModelBase
    {
        // === METADATA DEL ARCHIVO ===
        public long FileSizeBytes { get; set; }

        // === STORAGE ===
        public required string PublicId { get; set; }

        // === AUDITORÍA ===
        public DateTime LastRequestedAt { get; set; } = DateTime.UtcNow;
    }
}
