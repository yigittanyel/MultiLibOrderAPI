using Microsoft.EntityFrameworkCore;
using MultiLLibray.API.Models;

namespace MultiLLibray.API.Context
{
    public sealed class ApplicationDbContext:DbContext
    {
        public ApplicationDbContext()
        {
        }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options):base(options)
        {
            
        }
        public DbSet<Order> Orders { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=.;Database=MultiLibray;TrustServerCertificate=True;Encrypt=False;Trusted_Connection=True;Trusted_Connection=True");
            }
        }
    }
}
