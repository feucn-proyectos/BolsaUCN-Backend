using backend.src.Application.Events.Interfaces;
using Serilog;

namespace backend.src.Application.Events.Implements
{
    public class EventDispatcher : IEventDispatcher
    {
        private readonly IServiceProvider _serviceProvider;

        public EventDispatcher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task DispatchAsync<TEvent>(
            TEvent domainEvent,
            CancellationToken cancellationToken = default
        )
            where TEvent : IDomainEvent
        {
            var eventType = typeof(TEvent);
            Log.Information(
                "Despachando evento de tipo {EventType} ocurrido en {OccurredAt}",
                eventType.Name,
                domainEvent.OccurredAt
            );
            var handlers = _serviceProvider.GetServices<IEventHandler<TEvent>>();
            if (!handlers.Any())
            {
                Log.Warning(
                    "No se encontraron manejadores para el evento de tipo {EventType}",
                    eventType.Name
                );
                return;
            }

            foreach (var handler in handlers)
            {
                try
                {
                    Log.Information(
                        "Ejecutando manejador {HandlerType} para evento de tipo {EventType}",
                        handler.GetType().Name,
                        eventType.Name
                    );
                    await handler.HandleAsync(domainEvent, cancellationToken);
                    Log.Information(
                        "Manejador {HandlerType} ejecutado exitosamente",
                        handler.GetType().Name
                    );
                }
                catch (Exception ex)
                {
                    Log.Error(
                        ex,
                        "Error al ejecutar el manejador {HandlerType} para evento de tipo {EventType}",
                        handler.GetType().Name,
                        eventType.Name
                    );
                    // Continuar con el siguiente manejador incluso si uno falla
                }
            }
        }
    }
}
