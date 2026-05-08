using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using gasosa_backend.Models;

public class DataContext : IdentityDbContext<Usuario>
{
    public DataContext(DbContextOptions<DataContext> options) : base(options) { }

    public DbSet<Posto> Postos => Set<Posto>();

    public DbSet<Avaliacao> Avaliacoes => Set<Avaliacao>();

    public DbSet<PostoFoto> PostoFotos { get; set; }

    public DbSet<PasswordResetCode> PasswordResetCodes { get; set; }

    public DbSet<AvaliacaoVoto> AvaliacaoVotos => Set<AvaliacaoVoto>();

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

        builder.Entity<Avaliacao>(entity =>
        {
            entity.ToTable("avaliacoes");

            entity.HasOne(a => a.Posto)
                .WithMany()
                .HasForeignKey(a => a.PostoId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(a => a.Usuario)
                .WithMany()
                .HasForeignKey(a => a.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<PasswordResetCode>(entity =>
        {
            entity.ToTable("password_reset_codes");
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Email).HasMaxLength(256).IsRequired();
            entity.Property(p => p.Code).HasMaxLength(6).IsRequired();
        });

        builder.Entity<AvaliacaoVoto>(entity =>
        {
            entity.ToTable("avaliacao_votos");
            entity.HasKey(v => v.Id);

            // Um usuário só pode ter um voto por avaliação
            entity.HasIndex(v => new { v.UsuarioId, v.AvaliacaoId }).IsUnique();

            entity.HasOne(v => v.Avaliacao)
                .WithMany()
                .HasForeignKey(v => v.AvaliacaoId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(v => v.Usuario)
                .WithMany()
                .HasForeignKey(v => v.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}