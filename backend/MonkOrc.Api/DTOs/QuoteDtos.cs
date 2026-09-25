using System.ComponentModel.DataAnnotations;

namespace MonkOrc.Api.DTOs
{
    // --- Request DTOs ---

    public class CreateQuoteRequest
    {
        [Required(ErrorMessage = "O título é obrigatório.")]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "O cliente é obrigatório.")]
        public Guid CustomerId { get; set; }

        public DateTime IssueDate { get; set; } = Helpers.AppTime.Now();

        [Range(1, 365, ErrorMessage = "A validade deve ser entre 1 e 365 dias.")]
        public int ValidityDays { get; set; } = 15;

        [MaxLength(2000)]
        public string? Notes { get; set; }

        [Range(0, 100, ErrorMessage = "O desconto deve ser entre 0 e 100%.")]
        public decimal Discount { get; set; }

        [Range(0, 100, ErrorMessage = "O imposto deve ser entre 0 e 100%.")]
        public decimal Tax { get; set; }

        [Required(ErrorMessage = "O orçamento deve ter pelo menos um item.")]
        [MinLength(1, ErrorMessage = "O orçamento deve ter pelo menos um item.")]
        public List<CreateQuoteItemRequest> Items { get; set; } = new();
    }

    public class CreateQuoteItemRequest
    {
        [Required(ErrorMessage = "A descrição do item é obrigatória.")]
        [MaxLength(300)]
        public string Description { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Details { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "A quantidade mínima é 1.")]
        public int Quantity { get; set; } = 1;

        [Range(0, double.MaxValue, ErrorMessage = "O preço unitário deve ser positivo.")]
        public decimal UnitPrice { get; set; }
    }

    public class UpdateQuoteRequest
    {
        [Required(ErrorMessage = "O título é obrigatório.")]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "O cliente é obrigatório.")]
        public Guid CustomerId { get; set; }

        public DateTime IssueDate { get; set; }

        [Range(1, 365)]
        public int ValidityDays { get; set; } = 15;

        public int Status { get; set; }

        [MaxLength(2000)]
        public string? Notes { get; set; }

        [Range(0, 100)]
        public decimal Discount { get; set; }

        [Range(0, 100)]
        public decimal Tax { get; set; }

        [Required(ErrorMessage = "O orçamento deve ter pelo menos um item.")]
        [MinLength(1)]
        public List<CreateQuoteItemRequest> Items { get; set; } = new();
    }

    public class SendQuoteEmailRequest
    {
        public string? Email { get; set; }
    }

    // --- Response DTOs ---

    public class QuoteResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Number { get; set; } = string.Empty;
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public DateTime IssueDate { get; set; }
        public int ValidityDays { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public decimal Discount { get; set; }
        public decimal Tax { get; set; }
        public decimal Total { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<QuoteItemResponse> Items { get; set; } = new();
    }

    public class QuoteItemResponse
    {
        public Guid Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? Details { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal Subtotal => Quantity * UnitPrice;
    }
}
