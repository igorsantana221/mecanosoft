using System.ComponentModel.DataAnnotations;

namespace MonkOrc.Api.Models
{
    public class Vehicle : IMustHaveTenant
    {
        public Guid Id { get; set; }

        [Required]
        public Guid CustomerId { get; set; }
        public Customer? Customer { get; set; }

        [Required]
        [MaxLength(10)]
        public string LicensePlate { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Brand { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string Model { get; set; } = string.Empty;

        public int Year { get; set; }

        [MaxLength(50)]
        public string? Color { get; set; }

        public int Mileage { get; set; }

        [MaxLength(50)]
        public string? Chassis { get; set; }

        [MaxLength(50)]
        public string? Renavam { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; } = Helpers.AppTime.Now();
        public DateTime UpdatedAt { get; set; } = Helpers.AppTime.Now();

        [Required]
        public Guid TenantId { get; set; }
        public Tenant? Tenant { get; set; }
    }
}
