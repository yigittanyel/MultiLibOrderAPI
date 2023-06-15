using Microsoft.EntityFrameworkCore;
using MultiLLibray.API.Models;

namespace MultiLLibray.API.Context
{
    public class ApplicationDbContext:DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options):base(options)
        {
            
        }
        public DbSet<Order> Orders { get; set; }
    }
}
