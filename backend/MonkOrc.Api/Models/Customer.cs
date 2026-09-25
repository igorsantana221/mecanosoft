using System.ComponentModel.DataAnnotations;

namespace MonkOrc.Api.Models
{
    public class Customer : IMustHaveTenant
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(150)]
        public string? Email { get; set; }

        [MaxLength(20)]
        public string? Phone { get; set; }

        [MaxLength(50)]
        public string? Document { get; set; }

        [MaxLength(300)]
        public string? Address { get; set; }

        public DateTime CreatedAt { get; set; } = Helpers.AppTime.Now();

        [Required]
        public Guid TenantId { get; set; }
        public Tenant? Tenant { get; set; }

        public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    }
}
