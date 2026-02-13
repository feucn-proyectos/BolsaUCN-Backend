using backend.src.Application.DTOs.PublicationDTO;
using backend.src.Application.DTOs.PublicationDTO.ForAdminDTOs;
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
            ConfigurePublicationsForOfferor();
            ConfigurePublicationsForAdmin();
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
                .Map(dest => dest.PublicationDate, src => src.CreatedAt)
                .Map(dest => dest.PublicationType, src => src.PublicationType.ToString())
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
                .Map(
                    dest => dest.ImageUrls,
                    src =>
                        src is BuySell
                            ? ((BuySell)src).Images.Select(i => i.Url).ToList()
                            : new List<string>()
                )
                .Map(dest => dest.CompanyName, src => src.User.FirstName);
        }

        public void ConfigurePublicationsForOfferor()
        {
            // Mapeo para listar publicaciones del offerente
            TypeAdapterConfig<Publication, PublicationForOfferorDTO>
                .NewConfig()
                .Map(dest => dest.PublicationId, src => src.Id)
                .Map(dest => dest.Title, src => src.Title)
                .Map(dest => dest.PublicationType, src => src.PublicationType)
                .Map(dest => dest.PublicationDate, src => src.CreatedAt)
                .Map(dest => dest.ApprovalStatus, src => src.ApprovalStatus)
                .Map(dest => dest.HasAppealed, src => src.AppealCount > 0);

            // Mapeo para detalles de publicación del offerente
            TypeAdapterConfig<Publication, MyPublicationDetailsDTO>
                .NewConfig()
                // === PROPIEDADES COMUNES A TODAS LAS PUBLICACIONES ===
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.Title, src => src.Title)
                .Map(dest => dest.Description, src => src.Description)
                .Map(dest => dest.Location, src => src.Location)
                .Map(dest => dest.ContactEmail, src => src.User.Email)
                .Map(dest => dest.ContactPhone, src => src.User.PhoneNumber)
                .Map(dest => dest.AdditionalContactEmail, src => src.AdditionalContactEmail)
                .Map(
                    dest => dest.AdditionalContactPhoneNumber,
                    src => src.AdditionalContactPhoneNumber
                )
                .Map(dest => dest.PublicationType, src => src.PublicationType.ToString())
                .Map(dest => dest.ApprovalStatus, src => src.ApprovalStatus.ToString())
                .Map(dest => dest.CreatedAt, src => src.CreatedAt)
                .Map(dest => dest.ReasonForClosure, src => src.ClosedByAdminReason)
                .Map(dest => dest.ReasonForRejection, src => src.RejectedByAdminReason)
                .Map(dest => dest.AppealCount, src => src.AppealCount)
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
                .Map(
                    dest => dest.ImageUrls,
                    src =>
                        src is BuySell
                            ? ((BuySell)src).Images.Select(img => img.Url).ToList()
                            : null
                )
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

            // Mapeos para actualizar publicaciones a partir de una apelación de rechazo
            TypeAdapterConfig<AppealRejectionDTO, Offer>
                .NewConfig()
                .Map(dest => dest.Title, src => src.Tittle)
                .Map(dest => dest.Description, src => src.Description)
                .Map(dest => dest.Location, src => src.Location)
                .Map(dest => dest.AdditionalContactEmail, src => src.AdditionalContactEmail)
                .Map(
                    dest => dest.AdditionalContactPhoneNumber,
                    src => src.AdditionalContactPhoneNumber
                )
                .Map(dest => dest.ApplicationDeadline, src => src.ApplicationDeadline)
                .Map(dest => dest.EndDate, src => src.EndDate)
                .Map(dest => dest.Remuneration, src => src.Remuneration)
                .Map(dest => dest.AvailableSlots, src => src.RequiredApplicants)
                .Map(dest => dest.OfferType, src => src.OfferType)
                .Map(dest => dest.IsCvRequired, src => src.IsCvRequired);
            TypeAdapterConfig<AppealRejectionDTO, BuySell>
                .NewConfig()
                .Map(dest => dest.Title, src => src.Tittle)
                .Map(dest => dest.Description, src => src.Description)
                .Map(dest => dest.Location, src => src.Location)
                .Map(dest => dest.AdditionalContactEmail, src => src.AdditionalContactEmail)
                .Map(
                    dest => dest.AdditionalContactPhoneNumber,
                    src => src.AdditionalContactPhoneNumber
                )
                .Map(dest => dest.Category, src => src.Category)
                .Map(dest => dest.Price, src => src.Price)
                .Map(dest => dest.Quantity, src => src.Quantity)
                .Map(dest => dest.Availability, src => src.Availability)
                .Map(dest => dest.Condition, src => src.Condition);
        }

        public void ConfigurePublicationsForAdmin()
        {
            TypeAdapterConfig<Publication, PublicationForAdminDTO>
                .NewConfig()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.PublicationType, src => src.PublicationType.ToString())
                .Map(dest => dest.Title, src => src.Title)
                .Map(dest => dest.Description, src => src.Description)
                .Map(dest => dest.AuthorName, src => src.User.UserName)
                .Map(dest => dest.Location, src => src.Location)
                .Map(dest => dest.CreatedAt, src => src.CreatedAt)
                .Map(dest => dest.ApprovalStatus, src => src.ApprovalStatus)
                .Map(dest => dest.AppealsCount, src => src.AppealCount)
                // Informacion del autor
                .Map(dest => dest.AuthorId, src => src.User.Id)
                .Map(
                    dest => dest.AuthorName,
                    src =>
                        src.User.UserType == UserType.Empresa
                            ? src.User.FirstName
                            : src.User.FirstName + " " + src.User.LastName
                )
                .Map(dest => dest.UserType, src => src.User.UserType.ToString())
                .Map(dest => dest.ProfilePhotoUrl, src => src.User.ProfilePhoto!.Url)
                .Map(dest => dest.AuthorEmail, src => src.User.Email);

            TypeAdapterConfig<Publication, PublicationDetailsForAdminDTO>
                .NewConfig()
                // === PROPIEDADES COMUNES A TODAS LAS PUBLICACIONES ===
                .Map(dest => dest.PublicationId, src => src.Id)
                .Map(dest => dest.Title, src => src.Title)
                .Map(dest => dest.Description, src => src.Description)
                .Map(dest => dest.PublicationDate, src => src.CreatedAt)
                .Map(dest => dest.PublicationType, src => src.PublicationType.ToString())
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
                // === INFORMACION DEL AUTOR ===
                .Map(dest => dest.UserId, src => src.User.Id)
                .Map(dest => dest.UserEmail, src => src.User.Email)
                .Map(dest => dest.UserPhoneNumber, src => src.User.PhoneNumber)
                .Map(dest => dest.ProfilePhotoUrl, src => src.User.ProfilePhoto!.Url)
                .Map(
                    dest => dest.UserName,
                    src =>
                        src.User.UserType == UserType.Empresa
                            ? src.User.FirstName
                            : src.User.FirstName + " " + src.User.LastName
                )
                .Map(dest => dest.UserType, src => src.User.UserType.ToString())
                .Map(dest => dest.AboutMe, src => src.User.AboutMe ?? string.Empty)
                .Map(dest => dest.Rating, src => src.User.Rating)
                // === ATRIBUTOS DE OFERTA ===
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
                .Map(
                    dest => dest.ApplicantsCount,
                    src => src is Offer ? ((Offer)src).Applications.Count : (int?)null
                )
                .Map(
                    dest => dest.IsCvRequired,
                    src => src is Offer ? ((Offer)src).IsCvRequired : (bool?)null
                )
                // === ATRIBUTOS DE COMPRA/VENTA ===
                .Map(
                    dest => dest.Images,
                    src =>
                        src is BuySell
                            ? ((BuySell)src).Images.Select(img => img.Url).ToList()
                            : new List<string>()
                )
                .Map(
                    dest => dest.Category,
                    src => src is BuySell ? ((BuySell)src).Category : string.Empty
                )
                .Map(dest => dest.Price, src => src is BuySell ? ((BuySell)src).Price : (int?)null);
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
