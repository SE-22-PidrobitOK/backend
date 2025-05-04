using System.Data;
using JobService.Models;
using Microsoft.EntityFrameworkCore;

namespace JobService.DatabaseContext
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Skill> Skills { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }
    }
}
