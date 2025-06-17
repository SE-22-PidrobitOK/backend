using Microsoft.EntityFrameworkCore;
using PidrobitOK.ModeratorService.Models;

namespace PidrobitOK.ModeratorService
{
    public class ModerationDbContext : DbContext
    {
        public DbSet<JobModeration> JobModerations { get; set; }
        public DbSet<Complaint> Complaints { get; set; }
        public DbSet<ModerationLog> ModerationLogs { get; set; }

        public ModerationDbContext(DbContextOptions<ModerationDbContext> options)
            : base(options) { }
    }
}
