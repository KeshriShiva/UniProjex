using Microsoft.EntityFrameworkCore;
using UniProjex.Api.Models;
namespace UniProjex.Api.Data;
public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options) {
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Project> Projects => Set<Project>();
    protected override void OnModelCreating(ModelBuilder b) {
        b.Entity<AppUser>().HasIndex(x => x.Email).IsUnique();
        b.Entity<Project>().HasIndex(x => x.UniversityName);
        b.Entity<Project>().HasIndex(x => x.Category);
        b.Entity<Project>().HasIndex(x => x.Status);
    }
}
