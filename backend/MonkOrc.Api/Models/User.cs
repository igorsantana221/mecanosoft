using System.ComponentModel.DataAnnotations;

namespace MonkOrc.Api.Models
{
    public class User : IMustHaveTenant
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(150)]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public string? Role { get; set; } = "User";

        [MaxLength(20)]
        public string? Phone { get; set; }

        [MaxLength(500)]
        public string? AvatarUrl { get; set; }

        public bool IsEmailConfirmed { get; set; } = false;
        public string? EmailConfirmationToken { get; set; }
        public string? PasswordResetToken { get; set; }
        public DateTime? ResetTokenExpires { get; set; }

        public DateTime CreatedAt { get; set; } = Helpers.AppTime.Now();

        [Required]
        public Guid TenantId { get; set; }
        public Tenant? Tenant { get; set; }
    }
}
