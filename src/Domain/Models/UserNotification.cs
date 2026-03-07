namespace backend.src.Domain.Models
{
    public enum UserNotificationType
    {
        NuevaPostulacion, // Oferente recibe una nueva postulación a una oferta
    }

    public class UserNotification : ModelBase
    {
        public User? Recipient { get; set; }
        public required int RecipientId { get; set; }
        public required UserNotificationType Type { get; set; }
        public required bool IsSent { get; set; }
        public DateTime SentAt { get; set; }
    }
}
