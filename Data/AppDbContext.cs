using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;
using WorkScheduleApp.Models;

namespace WorkScheduleApp.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<AppUser> AppUsers { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> opts) : base(opts) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var entity = modelBuilder.Entity<AppUser>();
            modelBuilder.Entity<AppUser>().ToTable("APP_USERS");
            entity.HasKey(e => e.Username);
            entity.Property(e => e.Username).HasMaxLength(100).HasColumnName("USERNAME");
            entity.Property(e => e.PasswordHash).HasMaxLength(2000).HasColumnName("PASSWORD_HASH");
            entity.Property(e => e.Salt).HasMaxLength(2000).HasColumnName("SALT");
            entity.Property(e => e.FailedAttempts).HasColumnName("FAILED_ATTEMPTS");
            entity.Property(e => e.LockedUntil).HasColumnName("LOCKED_UNTIL");
        }
    }
}
