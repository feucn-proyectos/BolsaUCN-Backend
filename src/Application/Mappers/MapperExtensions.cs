using Mapster;

namespace backend.src.Application.Mappers
{
    public class MapperExtensions
    {
        public static void ConfigureMapster(IServiceProvider serviceProvider)
        {
            var userMapper = serviceProvider.GetService<UserMapper>();
            userMapper?.ConfigureAllMappings();
            var publicationMapper = serviceProvider.GetService<PublicationMapper>();
            publicationMapper?.ConfigureAllMappings();
            var offerMapper = serviceProvider.GetService<OfferMapper>();
            offerMapper?.ConfigureAllMappings();
            var buySellMapper = serviceProvider.GetService<BuySellMapper>();
            buySellMapper?.ConfigureAllMappings();
            var applicationMapper = serviceProvider.GetService<ApplicationMapper>();
            applicationMapper?.ConfigureAllMappings();
            var profileMapper = serviceProvider.GetService<ProfileMapper>();
            profileMapper?.ConfigureAllMappings();

            TypeAdapterConfig.GlobalSettings.Default.IgnoreNullValues(true);
        }
    }
}
