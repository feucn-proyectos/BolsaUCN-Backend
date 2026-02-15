using backend.src.Application.DTOs.UserDTOs;
using backend.src.Domain.Models;
using Mapster;

namespace backend.src.Application.Mappers
{
    public class DocumentMapper
    {
        public void ConfigureAllMappings()
        {
            ConfigureCVMappings();
        }

        public void ConfigureCVMappings()
        {
            TypeAdapterConfig<UploadCVDTO, Curriculum>
                .NewConfig()
                .Map(dest => dest.OriginalFileName, src => src.CVFile.FileName)
                .Map(dest => dest.FileSizeBytes, src => src.CVFile.Length);
        }
    }
}
