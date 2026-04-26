namespace PuntoVenta.Application.Options
{
    public class SmtpSettings
    {
        public const string SectionKey = nameof(SmtpSettings);
        
        public string SmtpHost { get; set; } = string.Empty;
        public int SmtpPort { get; set; }
        public string SmtpUsername { get; set; } = string.Empty;
        public string SmtpPassword { get; set; } = string.Empty;
        public string SenderEmail { get; set; } = string.Empty;
        public string AdminEmail { get; set; } = string.Empty;
    }
}
