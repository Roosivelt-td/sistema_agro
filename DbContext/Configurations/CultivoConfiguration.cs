using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaGestionAgricola.Models.Entities;

namespace SistemaGestionAgricola.Models.Configurations
{
    public class CultivoConfiguration : IEntityTypeConfiguration<Cultivo>
    {
        public void Configure(EntityTypeBuilder<Cultivo> builder)
        {
            builder.HasKey(c => c.Id);
            
            builder.Property(c => c.FechaSiembra)
                .IsRequired()
                .HasColumnType("date");
            
            builder.Property(c => c.FechaCosechaEstimada)
                .IsRequired()
                .HasColumnType("date");
            
            builder.Property(c => c.Estado)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("planificado");
            
            builder.Property(c => c.CreatedAt)
                .IsRequired()
                .HasColumnType("datetime");
            
            builder.Property(c => c.UpdatedAt)
                .IsRequired()
                .HasColumnType("datetime");

            // Relación muchos-a-uno con Terreno
            builder.HasOne(c => c.Terreno)
                .WithMany(t => t.Cultivos)
                .HasForeignKey(c => c.TerrenoId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relación muchos-a-uno con TipoCultivo
            builder.HasOne(c => c.TipoCultivo)
                .WithMany(tc => tc.Cultivos)
                .HasForeignKey(c => c.TipoCultivoId)
                .OnDelete(DeleteBehavior.Restrict);

            // Índices para búsquedas
            builder.HasIndex(c => c.TerrenoId);
            builder.HasIndex(c => c.TipoCultivoId);
            builder.HasIndex(c => c.Estado);
            builder.HasIndex(c => c.FechaSiembra);
            builder.HasIndex(c => c.FechaCosechaEstimada);
        }
    }
}