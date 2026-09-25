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
    public class CustomersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CustomersController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetCustomers()
        {
            var customers = await _context.Customers
                .Include(c => c.Vehicles)
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new CustomerResponse
                {
                    Id = c.Id,
                    Name = c.Name,
                    Email = c.Email,
                    Phone = c.Phone,
                    Document = c.Document,
                    Address = c.Address,
                    CreatedAt = c.CreatedAt,
                    VehicleCount = c.Vehicles.Count,
                    Vehicles = c.Vehicles.Select(v => new VehicleResponse
                    {
                        Id = v.Id,
                        CustomerId = v.CustomerId,
                        CustomerName = c.Name,
                        CustomerPhone = c.Phone,
                        CustomerDocument = c.Document,
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
                    }).ToList()
                })
                .ToListAsync();

            return Ok(customers);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCustomer(Guid id)
        {
            var customer = await _context.Customers
                .Include(c => c.Vehicles)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (customer == null)
                return NotFound("Cliente não encontrado.");

            return Ok(new CustomerResponse
            {
                Id = customer.Id,
                Name = customer.Name,
                Email = customer.Email,
                Phone = customer.Phone,
                Document = customer.Document,
                Address = customer.Address,
                CreatedAt = customer.CreatedAt,
                VehicleCount = customer.Vehicles.Count,
                Vehicles = customer.Vehicles.Select(v => new VehicleResponse
                {
                    Id = v.Id,
                    CustomerId = v.CustomerId,
                    CustomerName = customer.Name,
                    CustomerPhone = customer.Phone,
                    CustomerDocument = customer.Document,
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
                }).ToList()
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateCustomer([FromBody] CreateCustomerRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var customer = new Customer
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Email = request.Email,
                Phone = request.Phone,
                Document = request.Document,
                Address = request.Address,
                CreatedAt = Helpers.AppTime.Now()
            };

            await _context.Customers.AddAsync(customer);

            // Optional Vehicle payload during customer creation
            if (request.Vehicle != null && !string.IsNullOrWhiteSpace(request.Vehicle.LicensePlate))
            {
                var cleanPlate = request.Vehicle.LicensePlate.Trim().ToUpper();

                var plateExists = await _context.Vehicles.AnyAsync(v => v.LicensePlate.ToUpper() == cleanPlate);
                if (plateExists)
                    return BadRequest($"Já existe um veículo cadastrado com a placa {cleanPlate}.");

                var vehicle = new Vehicle
                {
                    Id = Guid.NewGuid(),
                    CustomerId = customer.Id,
                    LicensePlate = cleanPlate,
                    Brand = request.Vehicle.Brand.Trim(),
                    Model = request.Vehicle.Model.Trim(),
                    Year = request.Vehicle.Year,
                    Color = request.Vehicle.Color?.Trim(),
                    Mileage = request.Vehicle.Mileage,
                    Chassis = request.Vehicle.Chassis?.Trim().ToUpper(),
                    Renavam = request.Vehicle.Renavam?.Trim(),
                    Notes = request.Vehicle.Notes?.Trim(),
                    CreatedAt = Helpers.AppTime.Now(),
                    UpdatedAt = Helpers.AppTime.Now()
                };

                await _context.Vehicles.AddAsync(vehicle);
            }

            await _context.SaveChangesAsync();

            // Re-fetch created customer with vehicles
            var createdCustomer = await _context.Customers
                .Include(c => c.Vehicles)
                .FirstOrDefaultAsync(c => c.Id == customer.Id);

            return CreatedAtAction(nameof(GetCustomer), new { id = customer.Id }, new CustomerResponse
            {
                Id = customer.Id,
                Name = customer.Name,
                Email = customer.Email,
                Phone = customer.Phone,
                Document = customer.Document,
                Address = customer.Address,
                CreatedAt = customer.CreatedAt,
                VehicleCount = createdCustomer?.Vehicles.Count ?? 0,
                Vehicles = createdCustomer?.Vehicles.Select(v => new VehicleResponse
                {
                    Id = v.Id,
                    CustomerId = v.CustomerId,
                    CustomerName = customer.Name,
                    CustomerPhone = customer.Phone,
                    CustomerDocument = customer.Document,
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
                }).ToList() ?? new List<VehicleResponse>()
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomer(Guid id, [FromBody] UpdateCustomerRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var customer = await _context.Customers
                .Include(c => c.Vehicles)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (customer == null)
                return NotFound("Cliente não encontrado.");

            customer.Name = request.Name;
            customer.Email = request.Email;
            customer.Phone = request.Phone;
            customer.Document = request.Document;
            customer.Address = request.Address;

            await _context.SaveChangesAsync();

            return Ok(new CustomerResponse
            {
                Id = customer.Id,
                Name = customer.Name,
                Email = customer.Email,
                Phone = customer.Phone,
                Document = customer.Document,
                Address = customer.Address,
                CreatedAt = customer.CreatedAt,
                VehicleCount = customer.Vehicles.Count,
                Vehicles = customer.Vehicles.Select(v => new VehicleResponse
                {
                    Id = v.Id,
                    CustomerId = v.CustomerId,
                    CustomerName = customer.Name,
                    CustomerPhone = customer.Phone,
                    CustomerDocument = customer.Document,
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
                }).ToList()
            });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(Guid id)
        {
            var customer = await _context.Customers
                .Include(c => c.Vehicles)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (customer == null)
                return NotFound("Cliente não encontrado.");

            if (customer.Vehicles != null && customer.Vehicles.Count > 0)
            {
                return BadRequest($"Não é possível excluir o cliente '{customer.Name}' pois ele possui {customer.Vehicles.Count} veículo(s) vinculado(s). Exclua ou transfira os veículos antes de remover o cliente.");
            }

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
