using System.ComponentModel.DataAnnotations;

namespace MonkOrc.Api.Models
{
    public class QuoteItem
    {
        public Guid Id { get; set; }

        [Required]
        public Guid QuoteId { get; set; }
        public Quote? Quote { get; set; }

        [Required]
        [MaxLength(300)]
        public string Description { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Details { get; set; }

        public int Quantity { get; set; } = 1;

        public decimal UnitPrice { get; set; }
    }
}
