using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using gasosa_backend.Models;

public class DataContext : IdentityDbContext<Usuario>
{
    public DataContext(DbContextOptions<DataContext> options) : base(options) { }

    public DbSet<Posto> Postos => Set<Posto>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Posto>(entity =>
        {
            entity.ToTable("postos");
            entity.HasKey(p => p.Id);

            entity.Property(p => p.Nome).HasMaxLength(100).IsRequired();
            entity.Property(p => p.Bandeira).HasMaxLength(100);
            entity.Property(p => p.Latitude).HasPrecision(10, 8).IsRequired();
            entity.Property(p => p.Longitude).HasPrecision(11, 8).IsRequired();

            entity.HasOne(p => p.Usuario)
                .WithMany()
                .HasForeignKey(p => p.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}