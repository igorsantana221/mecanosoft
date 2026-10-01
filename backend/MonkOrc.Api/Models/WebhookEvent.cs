using System.ComponentModel.DataAnnotations;

namespace MonkOrc.Api.Models
{
    public class WebhookEvent
    {
        public Guid Id { get; set; }
        
        [Required]
        [MaxLength(50)]
        public string Provider { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(150)]
        public string EventId { get; set; } = string.Empty;
        
        [Required]
        [MaxLength(100)]
        public string EventType { get; set; } = string.Empty;
        
        public string? Payload { get; set; }
        
        public bool Processed { get; set; } = false;
        
        public DateTime? ProcessedAt { get; set; }
        
        public DateTime CreatedAt { get; set; } = Helpers.AppTime.Now();
    }
}
