using backend.src.Application.DTOs.PublicationDTO;
using backend.src.Application.DTOs.PublicationDTO.MyPublicationsDTOs;
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
                .Map(dest => dest.Types, src => src.PublicationType)
                .Map(dest => dest.Description, src => src.Description)
                .Map(dest => dest.PublicationDate, src => src.CreatedAt)
                .Map(dest => dest.Images, src => src.Images)
                .Map(dest => dest.IsActive, src => src.IsOpen)
                .Map(dest => dest.StatusValidation, src => src.ApprovalStatus);

            TypeAdapterConfig<Publication, PublicationAwaitingApprovalDTO>
                .NewConfig()
                .Map(dest => dest.PublicationId, src => src.Id)
                .Map(dest => dest.Title, src => src.Title)
                .Map(dest => dest.Type, src => src.PublicationType)
                .Map(dest => dest.CreatedAt, src => src.CreatedAt)
                .Map(dest => dest.CreatedBy, src => src.User.UserName)
                .Map(dest => dest.Status, src => src.ApprovalStatus)
                .Map(
                    dest => dest.OfferType,
                    src => src is Offer ? ((Offer)src).OfferType.ToString() : null
                )
                .Map(
                    dest => dest.Price,
                    src => src is BuySell ? ((BuySell)src).Price.ToString() : null
                );
            TypeAdapterConfig<Publication, PublicationDetailsForApprovalDTO>
                .NewConfig()
                // Atributos comunes
                .Map(dest => dest.PublicationId, src => src.Id)
                .Map(dest => dest.UserId, src => src.User.Id)
                .Map(dest => dest.UserEmail, src => src.User.Email)
                .Map(dest => dest.UserName, src => src.User.UserName)
                .Map(dest => dest.Title, src => src.Title)
                .Map(dest => dest.Description, src => src.Description)
                .Map(dest => dest.Images, src => src.Images)
                .Map(dest => dest.PublicationDate, src => src.CreatedAt)
                .Map(dest => dest.PublicationType, src => src.PublicationType.ToString())
                .Map(dest => dest.IsOpen, src => src.IsOpen)
                .Map(dest => dest.ApprovalStatus, src => src.ApprovalStatus.ToString())
                .Map(dest => dest.Location, src => src.Location)
                .Map(
                    dest => dest.AdditionalContactEmail,
                    src => src.AdditionalContactEmail ?? string.Empty
                )
                .Map(
                    dest => dest.AdditionalContactPhoneNumber,
                    src => src.AdditionalContactPhoneNumber ?? string.Empty
                )
                .Map(dest => dest.AboutMe, src => src.User.AboutMe ?? string.Empty)
                .Map(dest => dest.Rating, src => src.User.Rating)
                // Atributos de Oferta
                .Map(
                    dest => dest.EndDate,
                    src => src is Offer ? ((Offer)src).EndDate.ToString() : string.Empty
                )
                .Map(
                    dest => dest.DeadlineDate,
                    src => src is Offer ? ((Offer)src).ApplicationDeadline.ToString() : string.Empty
                )
                .Map(
                    dest => dest.Remuneration,
                    src => src is Offer ? ((Offer)src).Remuneration : (int?)null
                )
                .Map(
                    dest => dest.OfferType,
                    src => src is Offer ? ((Offer)src).OfferType.ToString() : string.Empty
                )
                // Atributos de Compra/Venta
                .Map(
                    dest => dest.Category,
                    src => src is BuySell ? ((BuySell)src).Category : string.Empty
                )
                .Map(dest => dest.Price, src => src is BuySell ? ((BuySell)src).Price : (int?)null)
                // Legacy
                .Map(dest => dest.Active, src => src.IsOpen)
                .Map(dest => dest.IsActive, src => src.IsOpen)
                .Map(dest => dest.ImageUrls, src => src.Images)
                .Map(dest => dest.CompanyName, src => src.User.FirstName);
        }

        public void ConfigurePublicationsForOfferor()
        {
            TypeAdapterConfig<Publication, MyPublicationDetailsDTO>
                .NewConfig()
                // === PROPIEDADES COMUNES A TODAS LAS PUBLICACIONES ===
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.Title, src => src.Title)
                .Map(dest => dest.Description, src => src.Description)
                .Map(dest => dest.Location, src => src.Location)
                .Map(dest => dest.AdditionalContactEmail, src => src.AdditionalContactEmail)
                .Map(dest => dest.AdditionalContactPhone, src => src.AdditionalContactPhoneNumber)
                .Map(dest => dest.PublicationType, src => src.PublicationType.ToString())
                .Map(dest => dest.ApprovalStatus, src => src.ApprovalStatus.ToString())
                .Map(dest => dest.CreatedAt, src => src.CreatedAt)
                // === OFERTAS DE TRABAJO ===
                .Map(
                    dest => dest.OfferType,
                    src => src is Offer ? ((Offer)src).OfferType.ToString() : null
                )
                .Map(
                    dest => dest.EndDate,
                    src => src is Offer ? ((Offer)src).EndDate : (DateTime?)null
                )
                .Map(
                    dest => dest.ApplicationDeadline,
                    src => src is Offer ? ((Offer)src).ApplicationDeadline : (DateTime?)null
                )
                .Map(
                    dest => dest.Remuneration,
                    src => src is Offer ? ((Offer)src).Remuneration : (int?)null
                )
                .Map(
                    dest => dest.IsCvRequired,
                    src => src is Offer ? ((Offer)src).IsCvRequired : (bool?)null
                )
                .Map(
                    dest => dest.ApplicationsCount,
                    src => src is Offer ? ((Offer)src).Applications.Count : (int?)null
                )
                // === COMPRA / VENTAS ===
                .Map(dest => dest.ImageUrls, src => src is BuySell ? ((BuySell)src).Images : null)
                .Map(dest => dest.Price, src => src is BuySell ? ((BuySell)src).Price : (int?)null)
                .Map(dest => dest.Category, src => src is BuySell ? ((BuySell)src).Category : null)
                .Map(
                    dest => dest.Quantity,
                    src => src is BuySell ? ((BuySell)src).Quantity : (int?)null
                )
                .Map(
                    dest => dest.Availability,
                    src => src is BuySell ? ((BuySell)src).Availability.ToString() : null
                )
                .Map(
                    dest => dest.Condition,
                    src => src is BuySell ? ((BuySell)src).Condition.ToString() : null
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
