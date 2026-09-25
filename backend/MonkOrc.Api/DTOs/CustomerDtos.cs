using System.ComponentModel.DataAnnotations;

namespace MonkOrc.Api.DTOs
{
    public class CreateCustomerRequest
    {
        [Required(ErrorMessage = "O nome é obrigatório.")]
        [MaxLength(150, ErrorMessage = "O nome deve ter no máximo 150 caracteres.")]
        public string Name { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "E-mail em formato inválido.")]
        [MaxLength(150)]
        public string? Email { get; set; }

        [MaxLength(20)]
        public string? Phone { get; set; }

        [MaxLength(50)]
        public string? Document { get; set; }

        [MaxLength(300)]
        public string? Address { get; set; }

        public OptionalVehicleRequest? Vehicle { get; set; }
    }

    public class UpdateCustomerRequest
    {
        [Required(ErrorMessage = "O nome é obrigatório.")]
        [MaxLength(150, ErrorMessage = "O nome deve ter no máximo 150 caracteres.")]
        public string Name { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "E-mail em formato inválido.")]
        [MaxLength(150)]
        public string? Email { get; set; }

        [MaxLength(20)]
        public string? Phone { get; set; }

        [MaxLength(50)]
        public string? Document { get; set; }

        [MaxLength(300)]
        public string? Address { get; set; }
    }

    public class CustomerResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? Document { get; set; }
        public string? Address { get; set; }
        public DateTime CreatedAt { get; set; }
        public int VehicleCount { get; set; }
        public List<VehicleResponse> Vehicles { get; set; } = new List<VehicleResponse>();
    }
}
