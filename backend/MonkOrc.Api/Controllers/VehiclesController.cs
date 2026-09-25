using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MonkOrc.Api.Data;
using MonkOrc.Api.DTOs;
using MonkOrc.Api.Models;

namespace MonkOrc.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class VehiclesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly Services.IVehicleLookupService _vehicleLookupService;

        public VehiclesController(AppDbContext context, Services.IVehicleLookupService vehicleLookupService)
        {
            _context = context;
            _vehicleLookupService = vehicleLookupService;
        }

        [HttpPost("consult-plate")]
        [HttpPost("consultar-placa")]
        public async Task<IActionResult> ConsultPlate([FromBody] VehicleLookupRequest request)
        {
            if (request == null || string.IsNullOrWhiteSpace(request.LicensePlate))
            {
                return BadRequest(new VehicleLookupResponse
                {
                    Success = false,
                    Message = "Informe a placa do veículo para consulta."
                });
            }

            var result = await _vehicleLookupService.ConsultPlateAsync(request.LicensePlate);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetVehicles(
            [FromQuery] string? search,
            [FromQuery] string? licensePlate,
            [FromQuery] string? brand,
            [FromQuery] string? model,
            [FromQuery] Guid? customerId)
        {
            var query = _context.Vehicles
                .Include(v => v.Customer)
                .AsQueryable();

            if (customerId.HasValue && customerId.Value != Guid.Empty)
            {
                query = query.Where(v => v.CustomerId == customerId.Value);
            }

            if (!string.IsNullOrWhiteSpace(licensePlate))
            {
                var cleanPlate = licensePlate.Trim().ToUpper().Replace("-", "");
                query = query.Where(v => v.LicensePlate.Replace("-", "").ToUpper().Contains(cleanPlate));
            }

            if (!string.IsNullOrWhiteSpace(brand))
            {
                query = query.Where(v => EF.Functions.ILike(v.Brand, $"%{brand.Trim()}%"));
            }

            if (!string.IsNullOrWhiteSpace(model))
            {
                query = query.Where(v => EF.Functions.ILike(v.Model, $"%{model.Trim()}%"));
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                var cleanTermPlate = term.ToUpper().Replace("-", "");

                query = query.Where(v =>
                    v.LicensePlate.Replace("-", "").ToUpper().Contains(cleanTermPlate) ||
                    EF.Functions.ILike(v.Brand, $"%{term}%") ||
                    EF.Functions.ILike(v.Model, $"%{term}%") ||
                    (v.Customer != null && (
                        EF.Functions.ILike(v.Customer.Name, $"%{term}%") ||
                        (v.Customer.Document != null && v.Customer.Document.Contains(term)) ||
                        (v.Customer.Phone != null && v.Customer.Phone.Contains(term))
                    ))
                );
            }

            var vehicles = await query
                .OrderByDescending(v => v.CreatedAt)
                .Select(v => new VehicleResponse
                {
                    Id = v.Id,
                    CustomerId = v.CustomerId,
                    CustomerName = v.Customer != null ? v.Customer.Name : "",
                    CustomerPhone = v.Customer != null ? v.Customer.Phone : null,
                    CustomerDocument = v.Customer != null ? v.Customer.Document : null,
                    LicensePlate = v.LicensePlate,
                    Brand = v.Brand,
                    Model = v.Model,
                    Year = v.Year,
                    Color = v.Color,
                    Mileage = v.Mileage,
                    Chassis = v.Chassis,
                    Renavam = v.Renavam,
                    Notes = v.Notes,
                    CreatedAt = v.CreatedAt,
                    UpdatedAt = v.UpdatedAt
                })
                .ToListAsync();

            return Ok(vehicles);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetVehicle(Guid id)
        {
            var vehicle = await _context.Vehicles
                .Include(v => v.Customer)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vehicle == null)
                return NotFound("Veículo não encontrado.");

            return Ok(new VehicleResponse
            {
                Id = vehicle.Id,
                CustomerId = vehicle.CustomerId,
                CustomerName = vehicle.Customer?.Name ?? "",
                CustomerPhone = vehicle.Customer?.Phone,
                CustomerDocument = vehicle.Customer?.Document,
                LicensePlate = vehicle.LicensePlate,
                Brand = vehicle.Brand,
                Model = vehicle.Model,
                Year = vehicle.Year,
                Color = vehicle.Color,
                Mileage = vehicle.Mileage,
                Chassis = vehicle.Chassis,
                Renavam = vehicle.Renavam,
                Notes = vehicle.Notes,
                CreatedAt = vehicle.CreatedAt,
                UpdatedAt = vehicle.UpdatedAt
            });
        }

        [HttpGet("/api/customers/{customerId}/vehicles")]
        public async Task<IActionResult> GetCustomerVehicles(Guid customerId)
        {
            var customerExists = await _context.Customers.AnyAsync(c => c.Id == customerId);
            if (!customerExists)
                return NotFound("Cliente não encontrado.");

            var vehicles = await _context.Vehicles
                .Where(v => v.CustomerId == customerId)
                .OrderByDescending(v => v.CreatedAt)
                .Select(v => new VehicleResponse
                {
                    Id = v.Id,
                    CustomerId = v.CustomerId,
                    CustomerName = v.Customer != null ? v.Customer.Name : "",
                    CustomerPhone = v.Customer != null ? v.Customer.Phone : null,
                    CustomerDocument = v.Customer != null ? v.Customer.Document : null,
                    LicensePlate = v.LicensePlate,
                    Brand = v.Brand,
                    Model = v.Model,
                    Year = v.Year,
                    Color = v.Color,
                    Mileage = v.Mileage,
                    Chassis = v.Chassis,
                    Renavam = v.Renavam,
                    Notes = v.Notes,
                    CreatedAt = v.CreatedAt,
                    UpdatedAt = v.UpdatedAt
                })
                .ToListAsync();

            return Ok(vehicles);
        }

        [HttpPost]
        public async Task<IActionResult> CreateVehicle([FromBody] CreateVehicleRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == request.CustomerId);
            if (customer == null)
                return BadRequest("Cliente não encontrado.");

            var cleanPlate = request.LicensePlate.Trim().ToUpper();

            var plateExists = await _context.Vehicles
                .AnyAsync(v => v.LicensePlate.ToUpper() == cleanPlate);
            if (plateExists)
                return BadRequest($"Já existe um veículo cadastrado com a placa {cleanPlate}.");

            var vehicle = new Vehicle
            {
                Id = Guid.NewGuid(),
                CustomerId = request.CustomerId,
                LicensePlate = cleanPlate,
                Brand = request.Brand.Trim(),
                Model = request.Model.Trim(),
                Year = request.Year,
                Color = request.Color?.Trim(),
                Mileage = request.Mileage,
                Chassis = request.Chassis?.Trim().ToUpper(),
                Renavam = request.Renavam?.Trim(),
                Notes = request.Notes?.Trim(),
                CreatedAt = Helpers.AppTime.Now(),
                UpdatedAt = Helpers.AppTime.Now()
            };

            await _context.Vehicles.AddAsync(vehicle);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetVehicle), new { id = vehicle.Id }, new VehicleResponse
            {
                Id = vehicle.Id,
                CustomerId = vehicle.CustomerId,
                CustomerName = customer.Name,
                CustomerPhone = customer.Phone,
                CustomerDocument = customer.Document,
                LicensePlate = vehicle.LicensePlate,
                Brand = vehicle.Brand,
                Model = vehicle.Model,
                Year = vehicle.Year,
                Color = vehicle.Color,
                Mileage = vehicle.Mileage,
                Chassis = vehicle.Chassis,
                Renavam = vehicle.Renavam,
                Notes = vehicle.Notes,
                CreatedAt = vehicle.CreatedAt,
                UpdatedAt = vehicle.UpdatedAt
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateVehicle(Guid id, [FromBody] UpdateVehicleRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var vehicle = await _context.Vehicles
                .Include(v => v.Customer)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (vehicle == null)
                return NotFound("Veículo não encontrado.");

            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == request.CustomerId);
            if (customer == null)
                return BadRequest("Cliente não encontrado.");

            var cleanPlate = request.LicensePlate.Trim().ToUpper();
            if (vehicle.LicensePlate.ToUpper() != cleanPlate)
            {
                var plateExists = await _context.Vehicles
                    .AnyAsync(v => v.Id != id && v.LicensePlate.ToUpper() == cleanPlate);
                if (plateExists)
                    return BadRequest($"Já existe outro veículo cadastrado com a placa {cleanPlate}.");
            }

            vehicle.CustomerId = request.CustomerId;
            vehicle.LicensePlate = cleanPlate;
            vehicle.Brand = request.Brand.Trim();
            vehicle.Model = request.Model.Trim();
            vehicle.Year = request.Year;
            vehicle.Color = request.Color?.Trim();
            vehicle.Mileage = request.Mileage;
            vehicle.Chassis = request.Chassis?.Trim().ToUpper();
            vehicle.Renavam = request.Renavam?.Trim();
            vehicle.Notes = request.Notes?.Trim();
            vehicle.UpdatedAt = Helpers.AppTime.Now();

            await _context.SaveChangesAsync();

            return Ok(new VehicleResponse
            {
                Id = vehicle.Id,
                CustomerId = vehicle.CustomerId,
                CustomerName = customer.Name,
                CustomerPhone = customer.Phone,
                CustomerDocument = customer.Document,
                LicensePlate = vehicle.LicensePlate,
                Brand = vehicle.Brand,
                Model = vehicle.Model,
                Year = vehicle.Year,
                Color = vehicle.Color,
                Mileage = vehicle.Mileage,
                Chassis = vehicle.Chassis,
                Renavam = vehicle.Renavam,
                Notes = vehicle.Notes,
                CreatedAt = vehicle.CreatedAt,
                UpdatedAt = vehicle.UpdatedAt
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteVehicle(Guid id)
        {
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == id);
            if (vehicle == null)
                return NotFound("Veículo não encontrado.");

            _context.Vehicles.Remove(vehicle);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
