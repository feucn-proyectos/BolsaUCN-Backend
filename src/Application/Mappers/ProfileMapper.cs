using backend.src.Application.DTOs.UserDTOs;
using backend.src.Application.DTOs.UserDTOs.AdminDTOs;
using backend.src.Application.DTOs.UserDTOs.UserProfileDTOs;
using backend.src.Domain.Models;
using Mapster;

namespace backend.src.Application.Mappers
{
    /// <summary>
    /// Clase encargada de mapear entidades relacionadas con el perfil de usuario.
    /// </summary>
    public class ProfileMapper
    {
        /// <summary>
        /// Configura todos los mapeos relacionados con el perfil de usuario.
        /// </summary>
        public void ConfigureAllMappings()
        {
            ConfigureGetProfileMapping();
            ConfigureUpdateProfileMappings();
            ConfigureCVMappings();
            ConfigureAdminMappings();
        }

        public void ConfigureGetProfileMapping()
        {
            TypeAdapterConfig<User, GetUserProfileDTO>
                .NewConfig()
                .Map(dest => dest.UserName, src => src.UserName)
                .Map(dest => dest.FirstName, src => src.FirstName)
                .Map(dest => dest.LastName, src => src.LastName)
                .Map(dest => dest.Rut, src => src.Rut)
                .Map(dest => dest.Email, src => src.Email)
                .Map(dest => dest.PhoneNumber, src => src.PhoneNumber)
                .Map(dest => dest.AboutMe, src => src.AboutMe)
                .Map(dest => dest.Rating, src => src.Rating)
                .Map(dest => dest.CurriculumVitae, src => src.CV != null ? src.CV.Url : null)
                .Map(
                    dest => dest.Disability,
                    src => src.Disability != null ? src.Disability.ToString() : null
                )
                .Map(dest => dest.ProfilePhoto, src => src.ProfilePhoto!.Url);
            TypeAdapterConfig<User, GetPhotoDTO>
                .NewConfig()
                .Map(
                    dest => dest.PhotoUrl,
                    src => src.ProfilePhoto != null ? src.ProfilePhoto.Url : string.Empty
                );
            TypeAdapterConfig<User, GetCVDTO>
                .NewConfig()
                .Map(dest => dest.Url, src => src.CV!.Url)
                .Map(dest => dest.OriginalFileName, src => src.CV!.OriginalFileName)
                .Map(dest => dest.FileSizeBytes, src => src.CV!.FileSizeBytes)
                .Map(dest => dest.UploadDate, src => src.CV!.CreatedAt);
        }

        public void ConfigureUpdateProfileMappings()
        {
            TypeAdapterConfig<UpdateUserProfileDTO, User>
                .NewConfig()
                .IgnoreNullValues(true)
                .Map(dest => dest.UserName, src => src.UserName)
                .Map(dest => dest.FirstName, src => src.FirstName)
                .Map(dest => dest.LastName, src => src.LastName)
                .Map(dest => dest.Email, src => src.Email)
                .Map(dest => dest.AboutMe, src => src.AboutMe)
                .Map(dest => dest.PhoneNumber, src => src.PhoneNumber);
        }

        public void ConfigureCVMappings()
        {
            TypeAdapterConfig<User, GetCVDTO>
                .NewConfig()
                .Map(dest => dest.OriginalFileName, src => src.CV!.OriginalFileName)
                .Map(dest => dest.Url, src => src.CV!.Url)
                .Map(dest => dest.FileSizeBytes, src => src.CV!.FileSizeBytes)
                .Map(dest => dest.UploadDate, src => src.CV!.UpdatedAt);
        }

        public void ConfigureAdminMappings()
        {
            TypeAdapterConfig<User, UserForAdminDTO>
                .NewConfig()
                .Map(dest => dest.UserName, src => src.UserName)
                .Map(dest => dest.Email, src => src.Email)
                .Map(dest => dest.Rut, src => src.Rut)
                .Map(dest => dest.Rating, src => src.Rating)
                .Map(dest => dest.UserType, src => src.UserType.ToString())
                .Map(dest => dest.Banned, src => src.IsBlocked);

            TypeAdapterConfig<User, UserProfileForAdminDTO>
                .NewConfig()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.Username, src => src.UserName)
                .Map(dest => dest.Email, src => src.Email)
                .Map(dest => dest.FirstName, src => src.FirstName)
                .Map(dest => dest.LastName, src => src.LastName)
                .Map(dest => dest.PhoneNumber, src => src.PhoneNumber)
                .Map(dest => dest.Rut, src => src.Rut)
                .Map(dest => dest.Rating, src => src.Rating)
                .Map(
                    dest => dest.ProfilePictureUrl,
                    src => src.ProfilePhoto != null ? src.ProfilePhoto.Url : null
                )
                .Map(dest => dest.AboutMe, src => src.AboutMe)
                .Map(dest => dest.UserType, src => src.UserType.ToString())
                .Map(dest => dest.Banned, src => src.IsBlocked)
                .Map(dest => dest.CreatedAt, src => src.CreatedAt)
                .Map(dest => dest.UpdatedAt, src => src.UpdatedAt)
                .Map(dest => dest.LastLoginAt, src => src.LastLoginAt)
                .Map(dest => dest.CVUrl, src => src.CV != null ? src.CV.Url : null)
                .Map(
                    dest => dest.Disability,
                    src => src.Disability != null ? src.Disability.ToString() : null
                );
        }
    }
}
