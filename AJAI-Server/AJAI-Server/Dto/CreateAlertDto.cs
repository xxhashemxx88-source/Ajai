namespace AJAI_Server.Dto
{
    public class CreateAlertDto
    {
        public string CameraEmail { get; set; } = string.Empty;
        public string AlertType { get; set; } = string.Empty; 
        public string Base64Image { get; set; } = string.Empty;
    }
}