using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaGestionAgricola.Models.Entities;

namespace SistemaGestionAgricola.Models.Configurations
{
    public class AgricultorConfiguration : IEntityTypeConfiguration<Agricultor>
    {
        public void Configure(EntityTypeBuilder<Agricultor> builder)
        {
            builder.HasKey(a => a.Id);
            
            builder.Property(a => a.Dni)
                .IsRequired()
                .HasMaxLength(20);
            
            builder.Property(a => a.Direccion)
                .HasColumnType("text");
            
            builder.Property(a => a.Experiencia)
                .HasColumnType("text");
            
            builder.Property(a => a.CreatedAt)
                .IsRequired()
                .HasColumnType("datetime");
            
            builder.Property(a => a.UpdatedAt)
                .IsRequired()
                .HasColumnType("datetime");

            // Relación uno-a-uno con Usuario
            builder.HasOne(a => a.Usuario)
                .WithOne(u => u.Agricultor)
                .HasForeignKey<Agricultor>(a => a.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(a => a.Dni)
                .IsUnique();
            
            builder.HasIndex(a => a.UsuarioId)
                .IsUnique();
        }
    }
}