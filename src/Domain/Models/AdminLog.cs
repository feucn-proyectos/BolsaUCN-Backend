namespace backend.src.Domain.Models
{
    public enum AuditAction
    {
        Blocked,
        Unblocked,
        Deleted,
        UpdatedProfile,
        Other,
    }

    public class AdminLog : ModelBase
    {
        public User? User { get; set; }
        public int UserId { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? Details { get; set; }
    }
}
