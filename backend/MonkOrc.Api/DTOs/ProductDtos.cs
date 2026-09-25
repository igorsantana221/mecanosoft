using System.ComponentModel.DataAnnotations;
using MonkOrc.Api.Models;

namespace MonkOrc.Api.DTOs
{
    public class CreateProductRequest
    {
        // ─── Informações Gerais ───────────────────────────────────────────────
        [Required(ErrorMessage = "O nome do produto é obrigatório.")]
        [MaxLength(200, ErrorMessage = "O nome deve ter no máximo 200 caracteres.")]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Sku { get; set; }

        [MaxLength(2000)]
        public string? Description { get; set; }

        [MaxLength(100)]
        public string? Category { get; set; }

        [MaxLength(20)]
        public string Unit { get; set; } = "UN";

        public ProductType Type { get; set; } = ProductType.Product;

        public bool IsActive { get; set; } = true;

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        // ─── Preços ───────────────────────────────────────────────────────────
        [Range(0, double.MaxValue, ErrorMessage = "O preço de custo não pode ser negativo.")]
        public decimal CostPrice { get; set; } = 0;

        [Required(ErrorMessage = "O preço de venda é obrigatório.")]
        [Range(0, double.MaxValue, ErrorMessage = "O preço de venda não pode ser negativo.")]
        public decimal SalePrice { get; set; } = 0;

        // ─── Campos Fiscais — Produto (NF-e) ─────────────────────────────────
        [MaxLength(10)]
        public string? Ncm { get; set; }

        [MaxLength(10)]
        public string? Cfop { get; set; }

        [MaxLength(10)]
        public string? Cst { get; set; }

        [MaxLength(2)]
        public string? Origin { get; set; }

        [Range(0, 100)]
        public decimal? IcmsRate { get; set; }

        [Range(0, 100)]
        public decimal? IpiRate { get; set; }

        [Range(0, 100)]
        public decimal? PisRate { get; set; }

        [Range(0, 100)]
        public decimal? CofinsRate { get; set; }

        // ─── Campos Fiscais — Serviço (NFS-e) ────────────────────────────────
        [MaxLength(10)]
        public string? ServiceCode { get; set; }

        [Range(0, 100)]
        public decimal? IssqnRate { get; set; }

        public bool RetainIss { get; set; } = false;
    }

    public class UpdateProductRequest
    {
        [Required(ErrorMessage = "O nome do produto é obrigatório.")]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Sku { get; set; }

        [MaxLength(2000)]
        public string? Description { get; set; }

        [MaxLength(100)]
        public string? Category { get; set; }

        [MaxLength(20)]
        public string Unit { get; set; } = "UN";

        public ProductType Type { get; set; } = ProductType.Product;

        public bool IsActive { get; set; } = true;

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        [Range(0, double.MaxValue)]
        public decimal CostPrice { get; set; } = 0;

        [Required]
        [Range(0, double.MaxValue)]
        public decimal SalePrice { get; set; } = 0;

        [MaxLength(10)]
        public string? Ncm { get; set; }

        [MaxLength(10)]
        public string? Cfop { get; set; }

        [MaxLength(10)]
        public string? Cst { get; set; }

        [MaxLength(2)]
        public string? Origin { get; set; }

        [Range(0, 100)]
        public decimal? IcmsRate { get; set; }

        [Range(0, 100)]
        public decimal? IpiRate { get; set; }

        [Range(0, 100)]
        public decimal? PisRate { get; set; }

        [Range(0, 100)]
        public decimal? CofinsRate { get; set; }

        [MaxLength(10)]
        public string? ServiceCode { get; set; }

        [Range(0, 100)]
        public decimal? IssqnRate { get; set; }

        public bool RetainIss { get; set; } = false;
    }

    public class ProductResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Sku { get; set; }
        public string? Description { get; set; }
        public string? Category { get; set; }
        public string Unit { get; set; } = "UN";
        public ProductType Type { get; set; }
        public bool IsActive { get; set; }
        public string? ImageUrl { get; set; }
        public decimal CostPrice { get; set; }
        public decimal SalePrice { get; set; }
        public string? Ncm { get; set; }
        public string? Cfop { get; set; }
        public string? Cst { get; set; }
        public string? Origin { get; set; }
        public decimal? IcmsRate { get; set; }
        public decimal? IpiRate { get; set; }
        public decimal? PisRate { get; set; }
        public decimal? CofinsRate { get; set; }
        public string? ServiceCode { get; set; }
        public decimal? IssqnRate { get; set; }
        public bool RetainIss { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
