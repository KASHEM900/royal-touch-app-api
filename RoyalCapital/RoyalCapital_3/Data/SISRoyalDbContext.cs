using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace RoyalCapital_3.Data
{
    public class SISRoyalDbContext : DbContext
    {
        public SISRoyalDbContext(DbContextOptions<SISRoyalDbContext> options)
               : base(options)
        {
        }
       
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        }
    }
}
