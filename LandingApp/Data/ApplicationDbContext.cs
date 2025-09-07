using LandingApp.Models;
using Microsoft.EntityFrameworkCore;

namespace LandingApp.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<LeadModel> Leads { get; set; }
        public DbSet<TariffModel> Tariffs { get; set; }

      
    }
}
