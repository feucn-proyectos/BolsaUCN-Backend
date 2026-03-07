namespace backend.src.Application.Events.Interfaces
{
    /// <summary>
    /// Interfaz para el despachador de eventos, que permite enviar eventos a los manejadores registrados.
    /// </summary>
    public interface IEventDispatcher
    {
        /// <summary>
        /// Despacha un evento a los manejadores registrados.
        /// </summary>
        /// <param name="event">El evento a despachar.</param>
        Task DispatchAsync<TEvent>(
            TEvent domainEvent,
            CancellationToken cancellationToken = default
        )
            where TEvent : IDomainEvent;
    }
}
