namespace backend.src.Application.Events.Interfaces
{
    /// <summary>
    /// Interface que representa un evento de dominio.
    /// </summary>
    public interface IDomainEvent
    {
        /// <summary>
        /// Fecha y hora en que ocurrió el evento.
        /// </summary>
        DateTime OccurredAt { get; }
    }
}
