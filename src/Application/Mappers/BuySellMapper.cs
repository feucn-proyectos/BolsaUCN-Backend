using backend.src.Application.DTOs.PublicationDTO;
using backend.src.Application.DTOs.PublicationDTO.CreatePublicationDTOs;
using backend.src.Application.DTOs.PublicationDTO.ExplorePublicationsDTOs.BuySells;
using backend.src.Application.DTOs.PublicationDTO.MyPublicationsDTOs;
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
            ConfigureExploreBuySellsMappings();
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
                .Map(dest => dest.IsEmailAvailable, src => src.ShowEmail)
                .Map(dest => dest.AdditionalContactEmail, src => src.AdditionalContactEmail)
                .Map(dest => dest.IsPhoneAvailable, src => src.ShowPhoneNumber)
                .Map(
                    dest => dest.AdditionalContactPhoneNumber,
                    src => src.AdditionalContactPhoneNumber
                );
        }

        public void ConfigureExploreBuySellsMappings()
        {
            TypeAdapterConfig<BuySell, BuySellForApplicantDTO>
                .NewConfig()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.Title, src => src.Title)
                .Map(dest => dest.Description, src => src.Description)
                .Map(dest => dest.AuthorName, src => src.User.FullName)
                .Map(dest => dest.CreatedAt, src => src.CreatedAt)
                .Map(dest => dest.ImageUrls, src => src.Images.Select(img => img.Url).ToList())
                .Map(dest => dest.Price, src => src.Price)
                .Map(dest => dest.Category, src => src.Category)
                .Map(dest => dest.Condition, src => src.Condition.ToString())
                .Map(dest => dest.Location, src => src.Location);

            TypeAdapterConfig<BuySell, BuySellDetailsForPublicDTO>
                .NewConfig()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.Title, src => src.Title)
                .Map(dest => dest.Description, src => src.Description)
                .Map(dest => dest.AuthorName, src => src.User.FullName)
                .Map(dest => dest.CreatedAt, src => src.CreatedAt)
                .Map(dest => dest.Price, src => src.Price)
                .Map(dest => dest.Category, src => src.Category)
                .Map(dest => dest.ImageUrls, src => src.Images.Select(img => img.Url).ToList());

            TypeAdapterConfig<BuySell, BuySellDetailsForApplicantDTO>
                .NewConfig()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.Title, src => src.Title)
                .Map(dest => dest.Description, src => src.Description)
                .Map(dest => dest.AuthorName, src => src.User.FullName)
                .Map(dest => dest.CreatedAt, src => src.CreatedAt)
                .Map(dest => dest.Price, src => src.Price)
                .Map(dest => dest.Category, src => src.Category)
                .Map(dest => dest.Quantity, src => src.Quantity)
                .Map(dest => dest.IsAvailable, src => src.IsAvailable)
                .Map(dest => dest.Condition, src => src.Condition.ToString())
                .Map(dest => dest.Location, src => src.Location)
                .Map(
                    dest => dest.ChosenContactEmail,
                    src =>
                        src.IsEmailAvailable ? src.AdditionalContactEmail ?? src.User.Email : null
                )
                .Map(
                    dest => dest.ChosenContactPhoneNumber,
                    src =>
                        src.IsPhoneAvailable
                            ? src.AdditionalContactPhoneNumber ?? src.User.PhoneNumber
                            : null
                )
                .Map(dest => dest.ImageUrls, src => src.Images.Select(img => img.Url).ToList());
        }

        public void ConfigureEditBuySellMappings()
        {
            TypeAdapterConfig<EditMyBuySellDetailsDTO, BuySell>
                .NewConfig()
                .Map(dest => dest.Title, src => src.Title)
                .Map(dest => dest.Description, src => src.Description)
                .Map(dest => dest.Location, src => src.Location)
                .Map(dest => dest.Price, src => src.Price)
                .Map(dest => dest.Category, src => src.Category)
                .Map(dest => dest.Quantity, src => src.Quantity)
                .Map(dest => dest.Condition, src => src.Condition)
                .Map(dest => dest.Location, src => src.Location)
                .Map(dest => dest.IsEmailAvailable, src => src.ShowEmail)
                .Map(dest => dest.IsPhoneAvailable, src => src.ShowPhoneNumber);
        }
    }
}
