using System.ComponentModel.DataAnnotations;

namespace MonkOrc.Api.Models
{
    public class Tenant
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;

        [MaxLength(20)]
        public string? Cnpj { get; set; }

        [MaxLength(20)]
        public string? Phone { get; set; }

        [MaxLength(10)]
        public string? ZipCode { get; set; }

        [MaxLength(200)]
        public string? Street { get; set; }

        [MaxLength(20)]
        public string? Number { get; set; }

        [MaxLength(100)]
        public string? Complement { get; set; }

        [MaxLength(100)]
        public string? Neighborhood { get; set; }

        [MaxLength(100)]
        public string? City { get; set; }

        [MaxLength(2)]
        public string? State { get; set; }
        public DateTime CreatedAt { get; set; } = Helpers.AppTime.Now();

        // Navigation property
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
