namespace backend.src.Application.Events.Interfaces
{
    /// <summary>
    /// Interfaz para el manejador de eventos, que define un método para manejar eventos de dominio específicos.
    /// </summary>
    /// <typeparam name="TEvent">El tipo de evento de dominio a manejar.</typeparam>
    public interface IEventHandler<in TEvent>
        where TEvent : IDomainEvent
    {
        /// <summary>
        /// Maneja un evento de dominio.
        /// </summary>
        /// <param name="domainEvent">El evento de dominio a manejar.</param>
        /// <param name="cancellationToken">Token de cancelación para operaciones asíncronas.</param>
        Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken = default);
    }
}
