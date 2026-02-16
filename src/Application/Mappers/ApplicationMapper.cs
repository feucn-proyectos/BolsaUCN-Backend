using backend.src.Application.DTOs.JobAplicationDTO.ApplicantsDTOs;
using backend.src.Application.DTOs.PublicationDTO.ApplicationsForOfferorDTOs;
using backend.src.Application.DTOs.PublicationDTO.ForAdminDTOs.ApplicantsForAdminDTOs;
using backend.src.Domain.Models;
using Mapster;

namespace backend.src.Application.Mappers
{
    public class ApplicationMapper
    {
        public void ConfigureAllMappings()
        {
            ConfigureApplicationMapping();
        }

        public void ConfigureApplicationMapping()
        {
            TypeAdapterConfig<JobApplication, ApplicationForApplicantDTO>
                .NewConfig()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.OfferTitle, src => src.JobOffer!.Title)
                .Map(dest => dest.Status, src => src.Status.ToString())
                .Map(dest => dest.CreatedAt, src => src.CreatedAt);

            TypeAdapterConfig<JobApplication, GetApplicationDetailsDTO>
                .NewConfig()
                .Map(dest => dest.OfferTitle, src => src.JobOffer!.Title)
                .Map(dest => dest.Description, src => src.JobOffer!.Description)
                .Map(dest => dest.ApplicationDeadline, src => src.JobOffer!.ApplicationDeadline)
                .Map(dest => dest.CreatedAt, src => src.JobOffer!.CreatedAt)
                .Map(dest => dest.EndDate, src => src.JobOffer!.EndDate)
                .Map(dest => dest.Remuneration, src => src.JobOffer!.Remuneration)
                // Oferente
                .Map(dest => dest.OfferorUserType, src => src.JobOffer!.User!.UserType.ToString())
                .Map(
                    dest => dest.ProfilePhotoUrl,
                    src =>
                        src.JobOffer!.User!.ProfilePhoto != null
                            ? src.JobOffer!.User!.ProfilePhoto!.Url
                            : null
                )
                // Contacto
                .Map(dest => dest.ContactEmail, src => src.JobOffer!.User!.Email)
                .Map(dest => dest.ContactPhoneNumber, src => src.JobOffer!.User!.PhoneNumber)
                .Map(
                    dest => dest.AdditionalContactEmail,
                    src => src.JobOffer!.AdditionalContactEmail
                )
                .Map(
                    dest => dest.AdditionalContactPhoneNumber,
                    src => src.JobOffer!.AdditionalContactPhoneNumber
                )
                // Datos de la postulación
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.CoverLetter, src => src.CoverLetter)
                .Map(dest => dest.Status, src => src.Status.ToString());

            TypeAdapterConfig<JobApplication, ApplicationForOfferorDTO>
                .NewConfig()
                .Map(dest => dest.ApplicationId, src => src.Id)
                .Map(dest => dest.ApplicantId, src => src.StudentId)
                .Map(
                    dest => dest.ApplicantPhotoUrl,
                    src => src.Student!.ProfilePhoto != null ? src.Student!.ProfilePhoto!.Url : null
                )
                .Map(dest => dest.ApplicantFirstName, src => src.Student!.FirstName)
                .Map(dest => dest.ApplicantLastName, src => src.Student!.LastName)
                .Map(dest => dest.ApplicantEmail, src => src.Student!.Email)
                .Map(dest => dest.ApplicationDate, src => src.CreatedAt)
                .Map(dest => dest.CVUrl, src => src.Student!.CV != null ? "true" : null) // temp fix
                .Map(dest => dest.CoverLetter, src => src.CoverLetter ?? null)
                .Map(dest => dest.Status, src => src.Status.ToString());

            TypeAdapterConfig<JobApplication, ApplicationForAdminDTO>
                .NewConfig()
                .Map(dest => dest.ApplicationId, src => src.Id)
                .Map(dest => dest.ApplicantId, src => src.StudentId)
                .Map(
                    dest => dest.ApplicantPhotoUrl,
                    src => src.Student!.ProfilePhoto != null ? src.Student!.ProfilePhoto!.Url : null
                )
                .Map(dest => dest.ApplicantFirstName, src => src.Student!.FirstName)
                .Map(dest => dest.ApplicantLastName, src => src.Student!.LastName)
                .Map(dest => dest.ApplicantEmail, src => src.Student!.Email)
                .Map(dest => dest.ApplicationDate, src => src.CreatedAt)
                .Map(dest => dest.CVUrl, src => src.Student!.CV != null ? "true" : null) // temp fix
                .Map(dest => dest.CoverLetter, src => src.CoverLetter ?? null)
                .Map(dest => dest.Status, src => src.Status.ToString());
        }
    }
}
