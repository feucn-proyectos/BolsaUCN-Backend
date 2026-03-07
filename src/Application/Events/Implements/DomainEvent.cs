using backend.src.Application.Events.Interfaces;

namespace backend.src.Application.Events.Implements
{
    /// <summary>
    /// Clase base para eventos de dominio, implementa la interfaz IDomainEvent.
    /// Proporciona una propiedad OccurredAt para registrar la fecha y hora del evento.
    /// </summary>
    public abstract class DomainEvent : IDomainEvent
    {
        public DateTime OccurredAt { get; protected set; } = DateTime.UtcNow;
    }
}
