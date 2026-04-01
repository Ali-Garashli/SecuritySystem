using System;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SecurityDashboard.Models;

namespace SecurityDashboard.Data {
    public class DataContext : IdentityDbContext<AppUser> {

        public DbSet<Sensor> Sensors { get; set; } = null!;
        public DbSet<History> SensorHistories { get; set; } = null!;
        public DbSet<AlertSystem> AlertSystems { get; set; } = null!;


        public DataContext(DbContextOptions<DataContext> options)
            : base(options) { }


        protected override void OnModelCreating(ModelBuilder modelBuilder) {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Sensor>(entity => {
                entity.ToTable("SensorReadings");

                entity.HasIndex(s => s.SensorType)
                      .IsUnique(); // one row per sensor

                entity.Property(s => s.SensorType)
                      .HasConversion<int>();

                entity.Property(s => s.State)
                      .HasConversion<int>();

                entity.Property(s => s.Unit)
                      .HasMaxLength(20)
                      .HasDefaultValue("");
            });

            modelBuilder.Entity<History>(entity => {
                entity.ToTable("AlertHistories");

                entity.Property(h => h.SensorType)
                      .HasConversion<int>();

                entity.Property(h => h.State)
                      .HasConversion<int>();

                entity.Property(h => h.Unit)
                      .HasMaxLength(20)
                      .HasDefaultValue("");

                // indexing for faster queries
                entity.HasIndex(h => h.ReadingTime);
                entity.HasIndex(h => h.SensorType);
            });

            modelBuilder.Entity<AlertSystem>(entity => {
                entity.ToTable("SystemStatus");

                entity.HasData(new AlertSystem {
                    Id = 1,
                    IsArmed = true,
                    SwitchedTime = DateTime.Now
                });
            });
        }
    }
}

