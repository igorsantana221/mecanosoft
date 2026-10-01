using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MonkOrc.Api.Data;
using MonkOrc.Api.DTOs;
using MonkOrc.Api.Models;
using MonkOrc.Api.Services;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace MonkOrc.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public AuthController(AppDbContext context, IConfiguration configuration, IEmailService emailService)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequestDto request)
        {
            var normalizedEmail = request.Email?.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(normalizedEmail))
            {
                return BadRequest("O e-mail é obrigatório.");
            }

            if (await _context.Users.IgnoreQueryFilters().AnyAsync(u => u.Email.ToLower() == normalizedEmail))
            {
                return BadRequest("E-mail já cadastrado.");
            }

            var confirmationToken = Guid.NewGuid().ToString();

            // Create Tenant
            var tenant = new Tenant
            {
                Id = Guid.NewGuid(),
                Name = !string.IsNullOrWhiteSpace(request.CompanyName) ? request.CompanyName.Trim() : $"Empresa de {request.Name?.Trim()}",
                Cnpj = request.Cnpj?.Trim(),
                Phone = request.Phone?.Trim(),
                ZipCode = request.ZipCode?.Trim(),
                Street = request.Street?.Trim(),
                Number = request.Number?.Trim(),
                Complement = request.Complement?.Trim(),
                Neighborhood = request.Neighborhood?.Trim(),
                City = request.City?.Trim(),
                State = request.State?.Trim(),
                CreatedAt = Helpers.AppTime.Now()
            };

            var user = new User
            {
                Id = Guid.NewGuid(),
                Name = request.Name?.Trim() ?? string.Empty,
                Email = normalizedEmail,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                EmailConfirmationToken = confirmationToken,
                IsEmailConfirmed = false,
                CreatedAt = Helpers.AppTime.Now(),
                TenantId = tenant.Id
            };

            try
            {
                await _context.Tenants.AddAsync(tenant);
                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateException ex)
            {
                if (ex.InnerException is Npgsql.PostgresException pgEx && pgEx.SqlState == "23505")
                {
                    return BadRequest("E-mail já cadastrado.");
                }
                throw;
            }

            // Send Confirmation Email
            try
            {
                var confirmationLink = $"{Request.Scheme}://{Request.Host}/api/auth/confirm-email?email={user.Email}&token={confirmationToken}";
                
                var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "RegistrationEmail.html");
                string emailBody = await System.IO.File.ReadAllTextAsync(templatePath);
                emailBody = emailBody.Replace("{{user.Name}}", user.Name)
                                     .Replace("{{confirmationLink}}", confirmationLink);

                await _emailService.SendEmailAsync(user.Email, "Confirmação de Cadastro - Mecanosoft", emailBody);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Aviso] Falha ao enviar e-mail de confirmação: {ex.Message}");
            }

            return Ok(new { message = "Usuário cadastrado com sucesso. Por favor, verifique seu e-mail para confirmar sua conta." });
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string email, string token)
        {
            var normalizedEmail = email?.Trim().ToLowerInvariant();
            var user = await _context.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail && u.EmailConfirmationToken == token);

            if (user == null)
            {
                return BadRequest("Link de confirmação inválido.");
            }

            user.IsEmailConfirmed = true;
            user.EmailConfirmationToken = null;
            await _context.SaveChangesAsync();

            return Ok(new { message = "E-mail confirmado com sucesso. Você já pode fazer login." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestDto request)
        {
            var loginEmail = request.Email?.Trim().ToLowerInvariant();

            var user = await _context.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Email.ToLower() == loginEmail);

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return Unauthorized("E-mail ou senha inválidos.");
            }

            if (!user.IsEmailConfirmed)
            {
                return BadRequest("Por favor, confirme seu e-mail antes de fazer login.");
            }

            var token = GenerateJwtToken(user);

            return Ok(new AuthResponse
            {
                Token = token,
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
                AvatarUrl = user.AvatarUrl
            });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            var email = request.Email?.Trim().ToLowerInvariant();

            var user = await _context.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => u.Email.ToLower() == email);

            if (user == null) return Ok(new { message = "Se o e-mail existir, um link de redefinição foi enviado." });

            var resetToken = Guid.NewGuid().ToString();
            user.PasswordResetToken = resetToken;
            user.ResetTokenExpires = Helpers.AppTime.Now().AddHours(2);
            await _context.SaveChangesAsync();

            // Send Confirmation Email
            var frontendUrl = _configuration["FrontendUrl"]?.TrimEnd('/') ?? "http://localhost:4200";
            var resetLink = $"{frontendUrl}/reset-password?token={resetToken}&email={user.Email}";

            var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "Templates", "ForgotPasswordEmail.html");
            string emailBody = await System.IO.File.ReadAllTextAsync(templatePath);
            emailBody = emailBody.Replace("{{user.Name}}", user.Name)
                                 .Replace("{{resetLink}}", resetLink);

            await _emailService.SendEmailAsync(user.Email, "Redefinição de acesso - Mecanosoft", emailBody);


            return Ok(new { message = "Se o e-mail existir, um link de redefinição foi enviado." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPassword request)
        {
            var user = await _context.Users
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(u => 
                u.Email == request.Email && 
                u.PasswordResetToken == request.Token && 
                u.ResetTokenExpires > Helpers.AppTime.Now());

            if (user == null) return BadRequest("Token de redefinição inválido ou expirado.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.PasswordResetToken = null;
            user.ResetTokenExpires = null;
            //Confirmar email do usuario caso ele tente trocar de senha.
            user.IsEmailConfirmed = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Senha redefinida com sucesso." });
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("Usuário não identificado.");
            }

            var user = await _context.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return NotFound("Usuário não encontrado.");
            }

            return Ok(new UserProfileResponse
            {
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
                AvatarUrl = user.AvatarUrl
            });
        }

        [Authorize]
        [HttpPut("profile")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("Usuário não identificado.");
            }

            var user = await _context.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return NotFound("Usuário não encontrado.");
            }

            user.Name = request.Name;
            user.Phone = request.Phone;
            user.AvatarUrl = request.AvatarUrl;

            await _context.SaveChangesAsync();

            var token = GenerateJwtToken(user);

            return Ok(new AuthResponse
            {
                Token = token,
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
                AvatarUrl = user.AvatarUrl
            });
        }

        [Authorize]
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst("sub")?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized("Usuário não identificado.");
            }

            var user = await _context.Users.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.Id == userId);
            if (user == null)
            {
                return NotFound("Usuário não encontrado.");
            }

            if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            {
                return BadRequest("Senha atual incorreta.");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Senha alterada com sucesso." });
        }

        [Authorize]
        [HttpPost("upload-avatar")]
        public async Task<IActionResult> UploadAvatar(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Nenhum arquivo enviado.");

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
                return BadRequest("Formato de arquivo inválido. Use JPG, PNG ou WEBP.");

            if (file.Length > 5 * 1024 * 1024)
                return BadRequest("O arquivo não pode ter mais de 5MB.");

            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "avatars");

            try
            {
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);
            }
            catch (Exception ex) 
            { 
                Console.WriteLine(ex.ToString());
            }

            var uniqueFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var url = $"/uploads/avatars/{uniqueFileName}";
            return Ok(new { url });
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Secret Key is not configured.");

            var key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Role, user.Role ?? "User"),
                new Claim("TenantId", user.TenantId.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var expirationInMinutes = int.Parse(jwtSettings["ExpirationInMinutes"] ?? "120");

            var token = new JwtSecurityToken(
                issuer: jwtSettings["Issuer"],
                audience: jwtSettings["Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(expirationInMinutes),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
