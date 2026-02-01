using backend.src.Application.DTOs.JobAplicationDTO.ApplicantsDTOs;
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
                .Map(dest => dest.OfferTitle, src => src.JobOffer.Title)
                .Map(dest => dest.Status, src => src.Status.ToString())
                .Map(dest => dest.CreatedAt, src => src.CreatedAt);

            TypeAdapterConfig<JobApplication, GetApplicationDetailsDTO>
                .NewConfig()
                .Map(dest => dest.Id, src => src.Id)
                .Map(dest => dest.OfferTitle, src => src.JobOffer.Title)
                .Map(dest => dest.ApplicationDeadline, src => src.JobOffer.ApplicationDeadline)
                .Map(dest => dest.CreatedAt, src => src.JobOffer.CreatedAt)
                .Map(dest => dest.EndDate, src => src.JobOffer.EndDate)
                .Map(dest => dest.Remuneration, src => src.JobOffer.Remuneration)
                .Map(dest => dest.Description, src => src.JobOffer.Description)
                .Map(dest => dest.Requirements, src => src.JobOffer.Requirements)
                .Map(dest => dest.ContactInfo, src => src.JobOffer.AdditionalContactEmail)
                .Map(dest => dest.CoverLetter, src => src.CoverLetter)
                .Map(dest => dest.Status, src => src.Status.ToString());
        }
    }
}
