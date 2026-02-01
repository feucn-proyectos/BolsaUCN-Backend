using backend.src.Application.DTOs.OfferDTOs;
using backend.src.Application.DTOs.PublicationDTO;
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
            .Map(dest => dest.Requirements, src => src.Requirements)
            .Map(dest => dest.AdditionalContactEmail, src => src.AdditionalContactEmail)
            .Map(dest => dest.AdditionalContactPhoneNumber, src => src.AdditionalContactPhoneNumber)
            .Map(dest => dest.IsCvRequired, src => src.IsCvRequired);

        // Map Offer to OfferSummaryDto (used for lists)
        TypeAdapterConfig<Offer, OfferSummaryDto>
            .NewConfig()
            .Map(dest => dest.Title, src => src.Title)
            .Map(dest => dest.CompanyName, src => src.User.FirstName);

        // Map Offer to OfferDetailDto (full details)
        TypeAdapterConfig<Offer, OfferDetailDto>
            .NewConfig()
            .Map(dest => dest.Title, src => src.Title)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.PostDate, src => src.CreatedAt)
            .Map(dest => dest.EndDate, src => src.EndDate)
            .Map(dest => dest.Remuneration, src => src.Remuneration)
            .Map(dest => dest.OfferType, src => src.OfferType.ToString())
            .Map(dest => dest.CompanyName, src => src.User.FirstName);
    }
}
