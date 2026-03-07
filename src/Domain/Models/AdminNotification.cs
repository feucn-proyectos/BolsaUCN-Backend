namespace backend.src.Domain.Models
{
    public enum AdminNotificationType
    {
        NuevaPublicacion, // Administrador recibe una nueva publicación para revisar
        PublicacionApelada, // Admin recibe una apelación de una publicación
        OfertaTerminada, // Admin recibe una notificación de que una oferta ha terminado
        CalificacionCompletada, // Admin recibe una notificación de que una calificación ha sido completada
        UsuarioRegistrado, // Admin recibe una notificación de que un nuevo usuario se ha registrado
        AvisoMalPuntaje, // Admin recibe una notificación de que un usuario tiene un puntaje bajo
    }

    public class AdminNotification : ModelBase
    {
        public bool IsSent { get; set; }
        public DateTime SentAt { get; set; }
        public string Payload { get; set; } = string.Empty;
        public AdminNotificationType Type { get; set; }
    }
}
