using System.ComponentModel.DataAnnotations;

namespace MonkOrc.Api.Models
{
    public enum QuoteStatus
    {
        Draft = 0,
        Pending = 1,
        Paid = 2,
        Canceled = 3
    }

    public class Quote : IMustHaveTenant
    {
        public Guid Id { get; set; }

        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [MaxLength(20)]
        public string Number { get; set; } = string.Empty;

        [Required]
        public Guid CustomerId { get; set; }
        public Customer? Customer { get; set; }

        public DateTime IssueDate { get; set; } = Helpers.AppTime.Now();

        public int ValidityDays { get; set; } = 15;

        public QuoteStatus Status { get; set; } = QuoteStatus.Draft;

        [MaxLength(2000)]
        public string? Notes { get; set; }

        public decimal Discount { get; set; }

        public decimal Tax { get; set; }

        public decimal Total { get; set; }

        public DateTime CreatedAt { get; set; } = Helpers.AppTime.Now();
        public DateTime UpdatedAt { get; set; } = Helpers.AppTime.Now();

        [Required]
        public Guid TenantId { get; set; }
        public Tenant? Tenant { get; set; }

        public ICollection<QuoteItem> Items { get; set; } = new List<QuoteItem>();
    }
}
