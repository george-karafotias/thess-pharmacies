using Microsoft.EntityFrameworkCore;
using ThessPharmacies.Api.Data.Entities;
using ThessPharmacies.Parser;

namespace ThessPharmacies.Api.Data;

public sealed class ThessPharmaciesDbContext : DbContext
{
    public ThessPharmaciesDbContext(
        DbContextOptions<ThessPharmaciesDbContext> options)
        : base(options)
    {
    }

    public DbSet<Pharmacy> Pharmacies => Set<Pharmacy>();

    public DbSet<PharmacyDuty> PharmacyDuties => Set<PharmacyDuty>();

    public DbSet<DutyImport> DutyImports => Set<DutyImport>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Pharmacy>(
            entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.Name)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(x => x.Address)
                    .IsRequired()
                    .HasMaxLength(300);

                entity.Property(x => x.Phone)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(x => x.Area)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.HasIndex(x => new
                {
                    x.Name,
                    x.Address,
                    x.Phone
                })
                .IsUnique();
            });

        modelBuilder.Entity<PharmacyDuty>(
            entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.DutyType)
                    .HasConversion<string>()
                    .HasMaxLength(50);

                entity.HasOne(x => x.Pharmacy)
                    .WithMany(x => x.Duties)
                    .HasForeignKey(x => x.PharmacyId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

        modelBuilder.Entity<DutyImport>(
            entity =>
            {
                entity.HasKey(x => x.Id);

                entity.Property(x => x.SourceFileName)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.HasIndex(x => new
                {
                    x.Area,
                    x.DutyDate
                })
                .IsUnique();
            });
    }
}