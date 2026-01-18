using backend.src.Application.DTOs.AuthDTOs;
using backend.src.Domain.Models;
using Mapster;

namespace backend.src.Application.Mappers
{
    public class UserMapper
    {
        public void ConfigureAllMappings()
        {
            ConfigureAuthMapping();
        }

        public void ConfigureAuthMapping()
        {
            TypeAdapterConfig<RegisterStudentDTO, User>
                .NewConfig()
                .Map(
                    dest => dest.UserName,
                    src =>
                        src.Email != null && src.Email.Contains('@')
                            ? src.Email.Substring(0, src.Email.IndexOf("@"))
                            : src.Email
                )
                .Map(dest => dest.FirstName, src => src.FirstName)
                .Map(dest => dest.LastName, src => src.LastName)
                .Map(dest => dest.Email, src => src.Email)
                .Map(dest => dest.PhoneNumber, src => src.PhoneNumber)
                .Map(dest => dest.Rut, src => src.Rut)
                .Map(dest => dest.UserType, src => UserType.Estudiante)
                .Map(dest => dest.Disability, src => src.Disability)
                .Map(dest => dest.IsBlocked, src => false)
                .Map(dest => dest.EmailConfirmed, src => false);

            TypeAdapterConfig<RegisterIndividualDTO, User>
                .NewConfig()
                .Map(
                    dest => dest.UserName,
                    src =>
                        src.Email != null && src.Email.Contains('@')
                            ? src.Email.Substring(0, src.Email.IndexOf("@"))
                            : src.Email
                )
                .Map(dest => dest.FirstName, src => src.FirstName)
                .Map(dest => dest.LastName, src => src.LastName)
                .Map(dest => dest.Email, src => src.Email)
                .Map(dest => dest.PhoneNumber, src => src.PhoneNumber)
                .Map(dest => dest.Rut, src => src.Rut)
                .Map(dest => dest.UserType, src => UserType.Particular)
                .Map(dest => dest.IsBlocked, src => false)
                .Map(dest => dest.EmailConfirmed, src => false);

            TypeAdapterConfig<RegisterCompanyDTO, User>
                .NewConfig()
                .Map(
                    dest => dest.UserName,
                    src =>
                        src.Email != null && src.Email.Contains('@')
                            ? src.Email.Substring(0, src.Email.IndexOf("@"))
                            : src.Email
                )
                .Map(dest => dest.FirstName, src => src.CompanyName)
                .Map(dest => dest.LastName, src => src.LegalName)
                .Map(dest => dest.Email, src => src.Email)
                .Map(dest => dest.PhoneNumber, src => src.PhoneNumber)
                .Map(dest => dest.Rut, src => src.Rut)
                .Map(dest => dest.UserType, src => UserType.Empresa)
                .Map(dest => dest.IsBlocked, src => false)
                .Map(dest => dest.EmailConfirmed, src => false);

            TypeAdapterConfig<RegisterAdminDTO, User>
                .NewConfig()
                .Map(
                    dest => dest.UserName,
                    src =>
                        src.Email != null && src.Email.Contains('@')
                            ? src.Email.Substring(0, src.Email.IndexOf("@"))
                            : src.Email
                )
                .Map(dest => dest.FirstName, src => src.FirstName)
                .Map(dest => dest.LastName, src => src.LastName)
                .Map(dest => dest.Email, src => src.Email)
                .Map(dest => dest.PhoneNumber, src => src.PhoneNumber)
                .Map(dest => dest.Rut, src => src.Rut)
                .Map(dest => dest.UserType, src => UserType.Administrador)
                .Map(dest => dest.IsBlocked, src => false)
                .Map(dest => dest.EmailConfirmed, src => false);
        }
    }
}
