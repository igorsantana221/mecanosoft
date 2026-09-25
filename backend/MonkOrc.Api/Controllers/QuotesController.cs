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
    public class QuotesController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly MonkOrc.Api.Services.IEmailService _emailService;

        public QuotesController(AppDbContext context, MonkOrc.Api.Services.IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        [HttpGet]
        public async Task<IActionResult> GetQuotes()
        {
            var quotes = await _context.Quotes
                .Include(q => q.Customer)
                .Include(q => q.Items)
                .OrderByDescending(q => q.CreatedAt)
                .Select(q => new QuoteResponse
                {
                    Id = q.Id,
                    Title = q.Title,
                    Number = q.Number,
                    CustomerId = q.CustomerId,
                    CustomerName = q.Customer != null ? q.Customer.Name : "",
                    IssueDate = q.IssueDate,
                    ValidityDays = q.ValidityDays,
                    Status = q.Status.ToString(),
                    Notes = q.Notes,
                    Discount = q.Discount,
                    Tax = q.Tax,
                    Total = q.Total,
                    CreatedAt = q.CreatedAt,
                    UpdatedAt = q.UpdatedAt,
                    Items = q.Items.Select(i => new QuoteItemResponse
                    {
                        Id = i.Id,
                        Description = i.Description,
                        Details = i.Details,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice
                    }).ToList()
                })
                .ToListAsync();

            return Ok(quotes);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetQuote(Guid id)
        {
            var quote = await _context.Quotes
                .Include(q => q.Customer)
                .Include(q => q.Items)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quote == null)
                return NotFound("Orçamento não encontrado.");

            return Ok(new QuoteResponse
            {
                Id = quote.Id,
                Title = quote.Title,
                Number = quote.Number,
                CustomerId = quote.CustomerId,
                CustomerName = quote.Customer?.Name ?? "",
                IssueDate = quote.IssueDate,
                ValidityDays = quote.ValidityDays,
                Status = quote.Status.ToString(),
                Notes = quote.Notes,
                Discount = quote.Discount,
                Tax = quote.Tax,
                Total = quote.Total,
                CreatedAt = quote.CreatedAt,
                UpdatedAt = quote.UpdatedAt,
                Items = quote.Items.Select(i => new QuoteItemResponse
                {
                    Id = i.Id,
                    Description = i.Description,
                    Details = i.Details,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList()
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateQuote([FromBody] CreateQuoteRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // FirstOrDefaultAsync aplica o Global Query Filter de tenant (FindAsync bypassa os filtros)
            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == request.CustomerId);
            if (customer == null)
                return BadRequest("Cliente não encontrado.");

            var quoteNumber = await GenerateQuoteNumber();

            var subtotal = request.Items.Sum(i => i.Quantity * i.UnitPrice);
            var withDiscount = subtotal * (1 - request.Discount / 100);
            var total = withDiscount * (1 + request.Tax / 100);

            var quote = new Quote
            {
                Id = Guid.NewGuid(),
                Title = request.Title,
                Number = quoteNumber,
                CustomerId = request.CustomerId,
                IssueDate = request.IssueDate,
                ValidityDays = request.ValidityDays,
                Status = QuoteStatus.Pending,
                Notes = request.Notes,
                Discount = request.Discount,
                Tax = request.Tax,
                Total = Math.Round(total, 2),
                CreatedAt = Helpers.AppTime.Now(),
                UpdatedAt = Helpers.AppTime.Now(),
                Items = request.Items.Select(i => new QuoteItem
                {
                    Id = Guid.NewGuid(),
                    Description = i.Description,
                    Details = i.Details,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList()
                // TenantId is automatically injected by AppDbContext.SaveChangesAsync()
            };

            try { 
                await _context.Quotes.AddAsync(quote);
                await _context.SaveChangesAsync();
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex);
            }

            

            return CreatedAtAction(nameof(GetQuote), new { id = quote.Id }, new QuoteResponse
            {
                Id = quote.Id,
                Title = quote.Title,
                Number = quote.Number,
                CustomerId = quote.CustomerId,
                CustomerName = customer.Name,
                IssueDate = quote.IssueDate,
                ValidityDays = quote.ValidityDays,
                Status = quote.Status.ToString(),
                Notes = quote.Notes,
                Discount = quote.Discount,
                Tax = quote.Tax,
                Total = quote.Total,
                CreatedAt = quote.CreatedAt,
                UpdatedAt = quote.UpdatedAt,
                Items = quote.Items.Select(i => new QuoteItemResponse
                {
                    Id = i.Id,
                    Description = i.Description,
                    Details = i.Details,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList()
            });
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateQuote(Guid id, [FromBody] UpdateQuoteRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var quote = await _context.Quotes
                .Include(q => q.Items)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quote == null)
                return NotFound("Orçamento não encontrado.");

            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == request.CustomerId);
            if (customer == null)
                return BadRequest("Cliente não encontrado.");

            // Update quote fields
            quote.Title = request.Title;
            quote.CustomerId = request.CustomerId;
            quote.IssueDate = request.IssueDate;
            quote.ValidityDays = request.ValidityDays;
            quote.Status = (QuoteStatus)request.Status;
            quote.Notes = request.Notes;
            quote.Discount = request.Discount;
            quote.Tax = request.Tax;
            quote.UpdatedAt = Helpers.AppTime.Now();

            // 1. Remove os itens antigos do ChangeTracker / Banco
            _context.QuoteItems.RemoveRange(quote.Items);

            // 2. Instancia os novos itens
            var newItems = request.Items.Select(i => new QuoteItem
            {
                Id = Guid.NewGuid(),
                QuoteId = quote.Id,
                Description = i.Description,
                Details = i.Details,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList();

            // 3. Adiciona os novos itens via DbSet (sem reatribuir quote.Items)
            _context.QuoteItems.AddRange(newItems);

            // Recalculate total
            var subtotal = request.Items.Sum(i => i.Quantity * i.UnitPrice);
            var withDiscount = subtotal * (1 - request.Discount / 100);
            quote.Total = Math.Round(withDiscount * (1 + request.Tax / 100), 2);

            await _context.SaveChangesAsync();

            return Ok(new QuoteResponse
            {
                Id = quote.Id,
                Title = quote.Title,
                Number = quote.Number,
                CustomerId = quote.CustomerId,
                CustomerName = customer.Name,
                IssueDate = quote.IssueDate,
                ValidityDays = quote.ValidityDays,
                Status = quote.Status.ToString(),
                Notes = quote.Notes,
                Discount = quote.Discount,
                Tax = quote.Tax,
                Total = quote.Total,
                CreatedAt = quote.CreatedAt,
                UpdatedAt = quote.UpdatedAt,
                Items = newItems.Select(i => new QuoteItemResponse
                {
                    Id = i.Id,
                    Description = i.Description,
                    Details = i.Details,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList()
            });
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteQuote(Guid id)
        {
            var quote = await _context.Quotes
                .Include(q => q.Items)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quote == null)
                return NotFound("Orçamento não encontrado.");

            _context.Quotes.Remove(quote);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpPost("{id}/send-email")]
        public async Task<IActionResult> SendQuoteEmail(Guid id, [FromBody] SendQuoteEmailRequest request)
        {
            var quote = await _context.Quotes
                .Include(q => q.Customer)
                .Include(q => q.Items)
                .FirstOrDefaultAsync(q => q.Id == id);

            if (quote == null)
                return NotFound("Orçamento não encontrado.");

            if (quote.Customer == null || string.IsNullOrEmpty(quote.Customer.Email))
                return BadRequest("O cliente deste orçamento não possui um e-mail cadastrado.");

            // Use requested email if provided, otherwise fallback to customer email
            var targetEmail = !string.IsNullOrEmpty(request.Email) ? request.Email : quote.Customer.Email;

            var subject = $"Proposta Comercial {quote.Number} - {quote.Title}";
            var body = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; color: #333;'>
                    <h2 style='color: #4F46E5;'>Olá, {quote.Customer.Name}</h2>
                    <p>Segue a proposta comercial solicitada:</p>
                    
                    <div style='background: #f9fafb; border: 1px solid #e5e7eb; border-radius: 8px; padding: 20px; margin-top: 20px;'>
                        <h3 style='margin-top: 0;'>Resumo do Orçamento: {quote.Number}</h3>
                        <p><strong>Título:</strong> {quote.Title}</p>
                        <p><strong>Data de Emissão:</strong> {quote.IssueDate:dd/MM/yyyy}</p>
                        <p><strong>Validade:</strong> {quote.ValidityDays} dias</p>
                        
                        <table style='width: 100%; border-collapse: collapse; margin-top: 20px;'>
                            <thead>
                                <tr style='border-bottom: 2px solid #e5e7eb; text-align: left;'>
                                    <th style='padding: 8px 0;'>Item</th>
                                    <th style='padding: 8px 0; text-align: center;'>Qtd</th>
                                    <th style='padding: 8px 0; text-align: right;'>Preço Unit.</th>
                                    <th style='padding: 8px 0; text-align: right;'>Total</th>
                                </tr>
                            </thead>
                            <tbody>
                                {string.Join("", quote.Items.Select(i => $@"
                                    <tr style='border-bottom: 1px solid #e5e7eb;'>
                                        <td style='padding: 8px 0;'>{i.Description}</td>
                                        <td style='padding: 8px 0; text-align: center;'>{i.Quantity}</td>
                                        <td style='padding: 8px 0; text-align: right;'>R$ {i.UnitPrice:N2}</td>
                                        <td style='padding: 8px 0; text-align: right;'>R$ {(i.Quantity * i.UnitPrice):N2}</td>
                                    </tr>
                                "))}
                            </tbody>
                        </table>
                        
                        <div style='margin-top: 20px; text-align: right;'>
                            <p><strong>Subtotal:</strong> R$ {quote.Items.Sum(i => i.Quantity * i.UnitPrice):N2}</p>
                            {(quote.Discount > 0 ? $"<p><strong>Desconto:</strong> {quote.Discount}%</p>" : "")}
                            <p style='font-size: 1.25em; font-weight: bold; color: #4F46E5;'>Total da Proposta: R$ {quote.Total:N2}</p>
                        </div>
                    </div>
                    
                    <p style='margin-top: 30px; font-size: 0.9em; color: #6b7280;'>
                        Esta é uma mensagem automática. Para dúvidas, por favor, entre em contato conosco.
                    </p>
                </div>
            ";

            try
            {
                await _emailService.SendEmailAsync(targetEmail, subject, body);
                return Ok(new { Message = "E-mail enviado com sucesso." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro ao enviar e-mail: {ex.Message}");
            }
        }

        private async Task<string> GenerateQuoteNumber()
        {
            var year = Helpers.AppTime.Now().Year;
            var count = await _context.Quotes.CountAsync() + 1;
            return $"#ORC-{year}-{count:D3}";
        }
    }
}
