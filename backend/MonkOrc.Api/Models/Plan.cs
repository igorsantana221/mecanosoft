using System.ComponentModel.DataAnnotations;

namespace MonkOrc.Api.Models
{
    public class Plan
    {
        public Guid Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;
        
        [MaxLength(500)]
        public string? Description { get; set; }
        
        public decimal Price { get; set; }
        
        [Required]
        [MaxLength(20)]
        public string BillingCycle { get; set; } = "MONTHLY"; // MONTHLY, YEARLY
        
        public int TrialDays { get; set; } = 7;
        
        public bool IsActive { get; set; } = true;
        
        public List<string> Features { get; set; } = new();

        public DateTime CreatedAt { get; set; } = Helpers.AppTime.Now();
        public DateTime? UpdatedAt { get; set; }
    }
}
