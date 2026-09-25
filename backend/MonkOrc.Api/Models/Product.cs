using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MonkOrc.Api.Models
{
    /// <summary>
    /// Tipo do produto: Produto Físico (gera NF-e) ou Serviço (gera NFS-e).
    /// </summary>
    public enum ProductType
    {
        Product = 0,
        Service = 1
    }

    public class Product : IMustHaveTenant
    {
        public Guid Id { get; set; }

        // ─── Informações Gerais ────────────────────────────────────────────────
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Sku { get; set; }

        [MaxLength(2000)]
        public string? Description { get; set; }

        [MaxLength(100)]
        public string? Category { get; set; }

        /// <summary>Unidade de medida: UN, M², M³, KG, L, CX, etc.</summary>
        [MaxLength(20)]
        public string Unit { get; set; } = "UN";

        public ProductType Type { get; set; } = ProductType.Product;

        public bool IsActive { get; set; } = true;

        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        // ─── Preços ───────────────────────────────────────────────────────────
        [Column(TypeName = "decimal(18,4)")]
        public decimal CostPrice { get; set; } = 0;

        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal SalePrice { get; set; } = 0;

        // ─── Campos Fiscais — Produto (NF-e) ─────────────────────────────────
        /// <summary>Nomenclatura Comum do Mercosul (8 dígitos).</summary>
        [MaxLength(10)]
        public string? Ncm { get; set; }

        /// <summary>Código Fiscal de Operações e Prestações (ex: 5102).</summary>
        [MaxLength(10)]
        public string? Cfop { get; set; }

        /// <summary>Código de Situação Tributária — ICMS (Simples: CSOSN / Regime Normal: CST).</summary>
        [MaxLength(10)]
        public string? Cst { get; set; }

        /// <summary>Origem da mercadoria: 0=Nacional, 1=Estrangeira importação direta, etc.</summary>
        [MaxLength(2)]
        public string? Origin { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? IcmsRate { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? IpiRate { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? PisRate { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? CofinsRate { get; set; }

        // ─── Campos Fiscais — Serviço (NFS-e) ───────────────────────────────
        /// <summary>Código de Serviço da LC 116/2003 (ex: 7.01, 7.02).</summary>
        [MaxLength(10)]
        public string? ServiceCode { get; set; }

        [Column(TypeName = "decimal(5,2)")]
        public decimal? IssqnRate { get; set; }

        /// <summary>Reter ISS na fonte? (para NFS-e).</summary>
        public bool RetainIss { get; set; } = false;

        // ─── Metadados ────────────────────────────────────────────────────────
        public DateTime CreatedAt { get; set; } = Helpers.AppTime.Now();
        public DateTime? UpdatedAt { get; set; }

        [Required]
        public Guid TenantId { get; set; }
        public Tenant? Tenant { get; set; }
    }
}
