using System.ComponentModel.DataAnnotations;

namespace MonkOrc.Api.Models
{
    public class Subscription : IMustHaveTenant
    {
        public Guid Id { get; set; }

        public Guid PlanId { get; set; }
        public Plan? Plan { get; set; }

        [Required]
        [MaxLength(30)]
        public string Status { get; set; } = "TRIALING"; // TRIALING, ACTIVE, PAST_DUE, CANCELED, EXPIRED, SUSPENDED

        [MaxLength(50)]
        public string? Provider { get; set; } // Asaas, MercadoPago, etc

        [MaxLength(100)]
        public string? ProviderCustomerId { get; set; }

        [MaxLength(100)]
        public string? ProviderSubscriptionId { get; set; }

        public DateTime? TrialStartDate { get; set; }
        public DateTime? TrialEndDate { get; set; }
        public DateTime? CurrentPeriodStart { get; set; }
        public DateTime? CurrentPeriodEnd { get; set; }
        public DateTime? CanceledAt { get; set; }

        public DateTime CreatedAt { get; set; } = Helpers.AppTime.Now();
        public DateTime? UpdatedAt { get; set; }

        [Required]
        public Guid TenantId { get; set; }
        public Tenant? Tenant { get; set; }
    }
}
