using backend.src.Application.DTOs.OfferDTOs;
using backend.src.Application.DTOs.PublicationDTO.CreatePublicationDTOs;
using backend.src.Application.DTOs.PublicationDTO.ExplorePublicationsDTOs.Offers;
using backend.src.Domain.Models;
using Mapster;

namespace backend.src.Application.Mappers;

/// <summary>
/// Configures mappings between the domain <see cref="Domain.Models.Offer"/> entity and its DTO types using Mapster.
/// This class centralizes mapping rules used across the application to keep mapping logic consistent.
/// </summary>
public class OfferMapper
{
    /// <summary>
    /// Registers all Mapster mapping configurations for <see cref="Domain.Models.Offer"/>.
    /// Call this during application startup to ensure DTO mappings are available.
    /// </summary>
    public void ConfigureAllMappings()
    {
        ConfigureOfferMapping();
        ConfigureExploreOffersMappings();
    }

    public void ConfigureOfferMapping()
    {
        TypeAdapterConfig<CreateOfferDTO, Offer>
            .NewConfig()
            .Map(dest => dest.Title, src => src.Title)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.EndDate, src => src.EndDate.ToUniversalTime())
            .Map(dest => dest.ApplicationDeadline, src => src.ApplicationDeadline.ToUniversalTime())
            .Map(dest => dest.Remuneration, src => src.Remuneration)
            .Map(dest => dest.OfferType, src => src.OfferType)
            .Map(dest => dest.Location, src => src.Location)
            .Map(dest => dest.AdditionalContactEmail, src => src.AdditionalContactEmail)
            .Map(dest => dest.AdditionalContactPhoneNumber, src => src.AdditionalContactPhoneNumber)
            .Map(dest => dest.AvailableSlots, src => src.RequiredApplicants)
            .Map(dest => dest.IsCvRequired, src => src.IsCvRequired);
    }

    public void ConfigureExploreOffersMappings()
    {
        TypeAdapterConfig<Offer, OfferForApplicantDTO>
            .NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.OfferType, src => src.OfferType.ToString())
            .Map(dest => dest.Title, src => src.Title)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.AuthorName, src => src.User.FullName)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt)
            .Map(dest => dest.ApplicationDeadline, src => src.ApplicationDeadline)
            .Map(dest => dest.AvailableSlots, src => src.AvailableSlots)
            .Map(dest => dest.Remuneration, src => src.Remuneration);

        TypeAdapterConfig<Offer, OfferDetailsForPublicDTO>
            .NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.OfferType, src => src.OfferType.ToString())
            .Map(dest => dest.Title, src => src.Title)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.AuthorName, src => src.User.FullName)
            .Map(dest => dest.Location, src => src.Location)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt)
            .Map(dest => dest.Remuneration, src => src.Remuneration)
            .Map(dest => dest.IsCVRequired, src => src.IsCvRequired);

        TypeAdapterConfig<Offer, OfferDetailsForApplicantDTO>
            .NewConfig()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.OfferType, src => src.OfferType.ToString())
            .Map(dest => dest.Title, src => src.Title)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.AuthorName, src => src.User.FullName)
            .Map(dest => dest.Location, src => src.Location)
            .Map(dest => dest.CreatedAt, src => src.CreatedAt)
            .Map(dest => dest.Remuneration, src => src.Remuneration)
            .Map(dest => dest.IsCVRequired, src => src.IsCvRequired)
            .Ignore(dest => dest.HasApplied) // Este campo se establecerá manualmente en el servicio, no se mapea directamente desde la entidad Offer
            .Ignore(dest => dest.ContactEmail) // Estos campos de contacto se establecerán manualmente en el servicio, no se mapean directamente desde la entidad Offer
            .Ignore(dest => dest.ContactPhoneNumber)
            .Ignore(dest => dest.AdditionalContactEmail!)
            .Ignore(dest => dest.AdditionalContactPhoneNumber!);
    }
}
