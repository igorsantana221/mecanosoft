using System.ComponentModel.DataAnnotations;

namespace MonkOrc.Api.DTOs
{
    public class CreateVehicleRequest
    {
        [Required(ErrorMessage = "O cliente é obrigatório.")]
        public Guid CustomerId { get; set; }

        [Required(ErrorMessage = "A placa é obrigatória.")]
        [MaxLength(10, ErrorMessage = "A placa deve ter no máximo 10 caracteres.")]
        public string LicensePlate { get; set; } = string.Empty;

        [Required(ErrorMessage = "A marca é obrigatória.")]
        [MaxLength(100, ErrorMessage = "A marca deve ter no máximo 100 caracteres.")]
        public string Brand { get; set; } = string.Empty;

        [Required(ErrorMessage = "O modelo é obrigatório.")]
        [MaxLength(100, ErrorMessage = "O modelo deve ter no máximo 100 caracteres.")]
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
    }

    public class UpdateVehicleRequest
    {
        [Required(ErrorMessage = "O cliente é obrigatório.")]
        public Guid CustomerId { get; set; }

        [Required(ErrorMessage = "A placa é obrigatória.")]
        [MaxLength(10, ErrorMessage = "A placa deve ter no máximo 10 caracteres.")]
        public string LicensePlate { get; set; } = string.Empty;

        [Required(ErrorMessage = "A marca é obrigatória.")]
        [MaxLength(100)]
        public string Brand { get; set; } = string.Empty;

        [Required(ErrorMessage = "O modelo é obrigatório.")]
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
    }

    public class VehicleResponse
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string? CustomerPhone { get; set; }
        public string? CustomerDocument { get; set; }
        public string LicensePlate { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public int Year { get; set; }
        public string? Color { get; set; }
        public int Mileage { get; set; }
        public string? Chassis { get; set; }
        public string? Renavam { get; set; }
        public string? Notes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class OptionalVehicleRequest
    {
        [Required(ErrorMessage = "A placa é obrigatória.")]
        [MaxLength(10)]
        public string LicensePlate { get; set; } = string.Empty;

        [Required(ErrorMessage = "A marca é obrigatória.")]
        [MaxLength(100)]
        public string Brand { get; set; } = string.Empty;

        [Required(ErrorMessage = "O modelo é obrigatório.")]
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
    }
}
