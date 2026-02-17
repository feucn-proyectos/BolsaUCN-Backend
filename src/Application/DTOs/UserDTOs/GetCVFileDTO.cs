namespace backend.src.Application.DTOs.UserDTOs
{
    public class GetCVFileDTO
    {
        public required Stream FileStream { get; set; }
        public required string FileName { get; set; }
        public required string ContentType { get; set; }
    }
}
