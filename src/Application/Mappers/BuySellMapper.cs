using backend.src.Application.DTOs.PublicationDTO;
using backend.src.Domain.Models;
using Mapster;

/// <summary>
/// Configuración de mapeos entre entidades Offer y sus DTOs usando Mapster
/// </summary>
namespace backend.src.Application.Mappers
{
    public class BuySellMapper
    {
        /// <summary>
        /// Configura todos los mapeos relacionados con BuySell
        /// </summary>
        public void ConfigureAllMappings()
        {
            ConfigureBuySellMappings();
        }

        public void ConfigureBuySellMappings()
        {
            TypeAdapterConfig<CreateBuySellDTO, BuySell>
                .NewConfig()
                .Map(dest => dest.Title, src => src.Title)
                .Map(dest => dest.Description, src => src.Description)
                .Map(dest => dest.Location, src => src.Location)
                .Map(dest => dest.Price, src => src.Price)
                .Map(dest => dest.Category, src => src.Category)
                .Map(dest => dest.Quantity, src => src.Quantity)
                .Map(dest => dest.Availability, src => src.Availability)
                .Map(dest => dest.Condition, src => src.Condition)
                .Map(dest => dest.Location, src => src.Location)
                .Map(dest => dest.AdditionalContactEmail, src => src.AdditionalContactEmail);

            TypeAdapterConfig<BuySell, BuySellSummaryDto>
                .NewConfig()
                .Map(dest => dest.Title, src => src.Title)
                .Map(dest => dest.Category, src => src.Category)
                .Map(dest => dest.Price, src => src.Price)
                .Map(dest => dest.Location, src => src.Location)
                .Map(dest => dest.PublicationDate, src => src.CreatedAt)
                .Map(dest => dest.UserName, src => src.User.UserName);

            TypeAdapterConfig<BuySell, BuySellDetailDto>
                .NewConfig()
                .Map(dest => dest.Title, src => src.Title)
                .Map(dest => dest.Description, src => src.Description)
                .Map(dest => dest.Category, src => src.Category)
                .Map(dest => dest.Price, src => src.Price)
                .Map(dest => dest.Location, src => src.Location)
                .Map(dest => dest.ContactInfo, src => src.AdditionalContactEmail)
                .Map(dest => dest.PublicationDate, src => src.CreatedAt)
                .Map(dest => dest.UserName, src => src.User.UserName)
                .Map(dest => dest.IsActive, src => src.IsOpen);
        }
    }
}
