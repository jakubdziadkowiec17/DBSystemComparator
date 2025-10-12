namespace DBSystemComparator_API.Models.DTOs
{
    public class ResponseDTO
    {
        public string Message { get; set; }

        public ResponseDTO(string message)
        {
            Message = message;
        }
    }
}