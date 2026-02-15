namespace backend.src.Application.DTOs.UserDTOs
{
    public class GetCVDTO
    {
        public required string OriginalFileName { get; set; }
        public required string Url { get; set; }
        public required long FileSizeBytes { get; set; }
        public required DateTime UploadDate { get; set; }
    }
}
