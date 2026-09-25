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
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        public ProductsController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _context.Products
                .OrderByDescending(p => p.CreatedAt)
                .Select(p => MapToResponse(p))
                .ToListAsync();

            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(Guid id)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound("Produto não encontrado.");

            return Ok(MapToResponse(product));
        }

        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Sku = request.Sku,
                Description = request.Description,
                Category = request.Category,
                Unit = request.Unit,
                Type = request.Type,
                IsActive = request.IsActive,
                ImageUrl = request.ImageUrl,
                CostPrice = request.CostPrice,
                SalePrice = request.SalePrice,
                Ncm = request.Ncm,
                Cfop = request.Cfop,
                Cst = request.Cst,
                Origin = request.Origin,
                IcmsRate = request.IcmsRate,
                IpiRate = request.IpiRate,
                PisRate = request.PisRate,
                CofinsRate = request.CofinsRate,
                ServiceCode = request.ServiceCode,
                IssqnRate = request.IssqnRate,
                RetainIss = request.RetainIss,
                CreatedAt = Helpers.AppTime.Now()
                // TenantId injected by AppDbContext.SaveChangesAsync()
            };

            await _context.Products.AddAsync(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, MapToResponse(product));
        }

        [HttpPost("upload-image")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Nenhum arquivo enviado.");

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
                return BadRequest("Formato de arquivo inválido. Use JPG, PNG ou WEBP.");

            if (file.Length > 5 * 1024 * 1024)
                return BadRequest("O arquivo não pode ter mais de 5MB.");

            var uploadsFolder = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads", "products");

            try
            {
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);
            }
            catch (Exception ex) { 
                Console.WriteLine(ex.ToString());
            }
            

            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var url = $"/uploads/products/{uniqueFileName}";
            return Ok(new { url });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(Guid id, [FromBody] UpdateProductRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound("Produto não encontrado.");

            product.Name = request.Name;
            product.Sku = request.Sku;
            product.Description = request.Description;
            product.Category = request.Category;
            product.Unit = request.Unit;
            product.Type = request.Type;
            product.IsActive = request.IsActive;
            product.ImageUrl = request.ImageUrl;
            product.CostPrice = request.CostPrice;
            product.SalePrice = request.SalePrice;
            product.Ncm = request.Ncm;
            product.Cfop = request.Cfop;
            product.Cst = request.Cst;
            product.Origin = request.Origin;
            product.IcmsRate = request.IcmsRate;
            product.IpiRate = request.IpiRate;
            product.PisRate = request.PisRate;
            product.CofinsRate = request.CofinsRate;
            product.ServiceCode = request.ServiceCode;
            product.IssqnRate = request.IssqnRate;
            product.RetainIss = request.RetainIss;
            product.UpdatedAt = Helpers.AppTime.Now();

            await _context.SaveChangesAsync();

            return Ok(MapToResponse(product));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound("Produto não encontrado.");

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private static ProductResponse MapToResponse(Product p) => new()
        {
            Id = p.Id,
            Name = p.Name,
            Sku = p.Sku,
            Description = p.Description,
            Category = p.Category,
            Unit = p.Unit,
            Type = p.Type,
            IsActive = p.IsActive,
            ImageUrl = p.ImageUrl,
            CostPrice = p.CostPrice,
            SalePrice = p.SalePrice,
            Ncm = p.Ncm,
            Cfop = p.Cfop,
            Cst = p.Cst,
            Origin = p.Origin,
            IcmsRate = p.IcmsRate,
            IpiRate = p.IpiRate,
            PisRate = p.PisRate,
            CofinsRate = p.CofinsRate,
            ServiceCode = p.ServiceCode,
            IssqnRate = p.IssqnRate,
            RetainIss = p.RetainIss,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt
        };
    }
}
