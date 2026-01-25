using backend.src.Application.DTOs.PublicationDTO;
using backend.src.Application.DTOs.PublicationDTO.ValidationDTOs;
using backend.src.Domain.Models;
using Mapster;

namespace backend.src.Application.Mappers
{
    public class PublicationMapper
    {
        public void ConfigureAllMappings()
        {
            ConfigurePublicationMapping();
        }

        public void ConfigurePublicationMapping()
        {
            TypeAdapterConfig<Publication, PublicationsDTO>
                .NewConfig()
                .Map(dest => dest.IdPublication, src => src.Id)
                .Map(dest => dest.Title, src => src.Title)
                .Map(dest => dest.Types, src => src.Type)
                .Map(dest => dest.Description, src => src.Description)
                .Map(dest => dest.PublicationDate, src => src.CreatedAt)
                .Map(dest => dest.Images, src => src.Images)
                .Map(dest => dest.IsActive, src => src.IsOpen)
                .Map(dest => dest.StatusValidation, src => src.StatusValidation);

            TypeAdapterConfig<Publication, PublicationForValidationDTO>
                .NewConfig()
                .Map(dest => dest.PublicationId, src => src.Id)
                .Map(dest => dest.Title, src => src.Title)
                .Map(dest => dest.Type, src => src.Type)
                .Map(dest => dest.CreatedAt, src => src.CreatedAt)
                .Map(dest => dest.CreatedBy, src => src.User.UserName)
                .Map(dest => dest.Status, src => src.StatusValidation)
                .Map(
                    dest => dest.OfferType,
                    src => src is Offer ? ((Offer)src).OfferType.ToString() : null
                )
                .Map(
                    dest => dest.Price,
                    src => src is BuySell ? ((BuySell)src).Price.ToString() : null
                );
        }
    }
    /*
    /// <summary>
    /// Mapper para convertir entidades de Publication a DTOs.
    /// Usado en el ReviewService para obtener información de publicaciones asociadas a reseñas.
    /// </summary>
    public static class PublicationMapper
    {
        /// <summary>
        /// Convierte una entidad Publication a PublicationsDTO.
        /// </summary>
        /// <param name="publication">La entidad Publication a convertir.</param>
        /// <returns>El DTO de la publicación.</returns>
        public static PublicationsDTO ToDTO(Publication publication)
        {
            return new PublicationsDTO
            {
                IdPublication = publication.Id,
                Title = publication.Title,
                Types = publication.Type,
                Description = publication.Description,
                PublicationDate = publication.CreatedAt,
                Images = publication.Images,
                IsActive = publication.IsOpen,
                StatusValidation = publication.StatusValidation,
            };
        }
    }
    */
}
