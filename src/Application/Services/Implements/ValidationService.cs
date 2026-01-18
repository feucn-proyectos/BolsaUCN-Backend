using backend.src.Application.Services.Interfaces;
using backend.src.Domain.Options;

namespace backend.src.Application.Services.Implements
{
    public class ValidationService : IValidationService
    {
        private readonly IPublicationService _publicationService;

        public ValidationService(IPublicationService publicationService)
        {
            _publicationService = publicationService;
        }

        public async Task<string> ValidatePublication(
            int adminUserId,
            int publicationId,
            string type,
            string action
        )
        {
            // Implementación de la lógica para validar o rechazar una publicación
            var publication = await _publicationService.GetPublicationByIdAsync(
                publicationId,
                new PublicationQueryOptions { }
            );
            return "Validación realizada";
        }
    }
}
