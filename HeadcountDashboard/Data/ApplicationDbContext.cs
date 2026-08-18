using HeadcountDashboard.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HeadcountDashboard.Data
{
    public class ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<Department> Departments => Set<Department>();

        public DbSet<DailyHeadcount> DailyHeadcounts => Set<DailyHeadcount>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Department>()
                .HasIndex(d => d.Code)
                .IsUnique();

            builder.Entity<DailyHeadcount>()
                .HasIndex(h => new { h.DepartmentId, h.BusinessDate })
                .IsUnique();

            builder.Entity<DailyHeadcount>()
                .Property(h => h.AShiftCount)
                .HasDefaultValue(0);

            builder.Entity<DailyHeadcount>()
                .Property(h => h.BShiftCount)
                .HasDefaultValue(0);

            builder.Entity<DailyHeadcount>()
                .Property(h => h.CShiftCount)
                .HasDefaultValue(0);
        }
    }
}