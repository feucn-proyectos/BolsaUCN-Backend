using backend.src.Application.Events.Interfaces;
using Hangfire;
using Serilog;

namespace backend.src.Application.Events.Implements.Handlers
{
    public class CloseOfferOnSlotsFilledHandler : IEventHandler<OfferSlotsFilledEvent>
    {
        public Task HandleAsync(
            OfferSlotsFilledEvent evt,
            CancellationToken cancellationToken = default
        )
        {
            BackgroundJob.Reschedule(evt.CloseApplicationsJobId.ToString(), DateTime.UtcNow);
            Log.Information(
                "Oferta ID: {OfferId} - {OfferTitle} ha llenado todos sus cupos. Se ha programado el cierre de postulaciones.",
                evt.OfferId,
                evt.OfferTitle
            );
            return Task.CompletedTask;
        }
    }
}
