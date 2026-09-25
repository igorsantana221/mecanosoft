using Microsoft.EntityFrameworkCore;
using MonkOrc.Api.Models;
using MonkOrc.Api.Services;

namespace MonkOrc.Api.Data
{
    public class AppDbContext : DbContext
    {
        private readonly ITenantService _tenantService;

        public AppDbContext(DbContextOptions<AppDbContext> options, ITenantService tenantService) : base(options)
        {
            _tenantService = tenantService;
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Tenant> Tenants { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Quote> Quotes { get; set; }
        public DbSet<QuoteItem> QuoteItems { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // IMPORTANTE: O Global Query Filter deve referenciar _tenantService diretamente
            // na lambda, NÃO chamar GetTenantId() aqui. OnModelCreating é executado apenas
            // uma vez pelo EF Core (cache do modelo) — chamar GetTenantId() aqui capturaria
            // null (sem HTTP context). A lambda abaixo chama GetTenantId() em runtime,
            // a cada query, garantindo o isolamento correto por tenant.
            modelBuilder.Entity<User>().HasQueryFilter(u => u.TenantId == _tenantService.GetTenantId());
            modelBuilder.Entity<Customer>().HasQueryFilter(c => c.TenantId == _tenantService.GetTenantId());
            modelBuilder.Entity<Quote>().HasQueryFilter(q => q.TenantId == _tenantService.GetTenantId());
            modelBuilder.Entity<Product>().HasQueryFilter(p => p.TenantId == _tenantService.GetTenantId());
            modelBuilder.Entity<Vehicle>().HasQueryFilter(v => v.TenantId == _tenantService.GetTenantId());


            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<Vehicle>()
                .HasIndex(v => new { v.TenantId, v.LicensePlate })
                .IsUnique();

            // Configure relationships
            modelBuilder.Entity<User>()
                .HasOne(u => u.Tenant)
                .WithMany(t => t.Users)
                .HasForeignKey(u => u.TenantId);

            modelBuilder.Entity<Customer>()
                .HasOne(c => c.Tenant)
                .WithMany()
                .HasForeignKey(c => c.TenantId);

            modelBuilder.Entity<Vehicle>()
                .HasOne(v => v.Customer)
                .WithMany(c => c.Vehicles)
                .HasForeignKey(v => v.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Vehicle>()
                .HasOne(v => v.Tenant)
                .WithMany()
                .HasForeignKey(v => v.TenantId);

            modelBuilder.Entity<Quote>()
                .HasOne(q => q.Customer)
                .WithMany()
                .HasForeignKey(q => q.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Quote>()
                .HasOne(q => q.Tenant)
                .WithMany()
                .HasForeignKey(q => q.TenantId);

            modelBuilder.Entity<Quote>()
                .HasMany(q => q.Items)
                .WithOne(i => i.Quote)
                .HasForeignKey(i => i.QuoteId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Quote>()
                .Property(q => q.Total)
                .HasColumnType("decimal(18,2)");

            modelBuilder.Entity<Quote>()
                .Property(q => q.Discount)
                .HasColumnType("decimal(5,2)");

            modelBuilder.Entity<Quote>()
                .Property(q => q.Tax)
                .HasColumnType("decimal(5,2)");

            modelBuilder.Entity<QuoteItem>()
                .Property(i => i.UnitPrice)
                .HasColumnType("decimal(18,2)");

            // Product decimal columns
            modelBuilder.Entity<Product>()
                .HasOne(p => p.Tenant)
                .WithMany()
                .HasForeignKey(p => p.TenantId);

            foreach (var prop in new[] { "SalePrice", "CostPrice" })
                modelBuilder.Entity<Product>().Property(prop).HasColumnType("decimal(18,4)");

            foreach (var prop in new[] { "IcmsRate", "IpiRate", "PisRate", "CofinsRate", "IssqnRate" })
                modelBuilder.Entity<Product>().Property(prop).HasColumnType("decimal(5,2)");
        }

        public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var tenantId = _tenantService.GetTenantId();

            foreach (var entry in ChangeTracker.Entries<IMustHaveTenant>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        if (tenantId.HasValue && entry.Entity.TenantId == Guid.Empty)
                        {
                            entry.Entity.TenantId = tenantId.Value;
                        }
                        break;
                }
            }

            return base.SaveChangesAsync(cancellationToken);
        }
    }
}
