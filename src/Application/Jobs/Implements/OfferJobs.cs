using backend.src.Application.Jobs.Interfaces;
using backend.src.Application.Services.Interfaces;
using Serilog;

namespace backend.src.Application.Jobs.Implements
{
    public class OfferJobs : IOfferJobs
    {
        private readonly IPublicationService _publicationService;

        public OfferJobs(IPublicationService publicationService)
        {
            _publicationService = publicationService;
        }

        public async Task SetAsCloseForApplicationsAsync(int offerId)
        {
            Log.Information(
                "Iniciando proceso para cerrar la oferta con ID {OfferId} para nuevas postulaciones.",
                offerId
            );
            await _publicationService.CloseOfferForApplicationsAsync(offerId);
        }

        public async Task SetAsCompleteAndInitializeReviewsAsync(int offerId)
        {
            Log.Information(
                "Iniciando proceso para marcar la oferta con ID {OfferId} como Trabajo Finalizado para que se generen las calificaciones.",
                offerId
            );
            await _publicationService.CompleteAndInitializeReviewsAsync(offerId);
        }

        public async Task SetAsFinalizedAndCloseReviewsAsync(int offerId)
        {
            Log.Information(
                "Iniciando proceso para marcar la oferta con ID {OfferId} como Finalizada y cerrar las reseñas asociadas.",
                offerId
            );
            await _publicationService.FinalizeAndCloseReviewsAsync(offerId);
        }
    }
}
