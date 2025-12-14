using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaGestionAgricola.Models.Entities;

namespace SistemaGestionAgricola.Models.Configurations
{
    public class CosechaConfiguration : IEntityTypeConfiguration<Cosecha>
    {
        public void Configure(EntityTypeBuilder<Cosecha> builder)
        {
            builder.HasKey(c => c.Id);
            
            builder.Property(c => c.Fecha)
                .IsRequired()
                .HasColumnType("date");
            
            builder.Property(c => c.CantidadKilos)
                .IsRequired()
                .HasColumnType("decimal(10,2)");
            
            builder.Property(c => c.CostoCosecha)
                .IsRequired()
                .HasColumnType("decimal(10,2)");
            
            builder.Property(c => c.Observaciones)
                .HasColumnType("text");
            
            builder.Property(c => c.CreatedAt)
                .IsRequired()
                .HasColumnType("datetime");

            // Relación muchos-a-uno con Cultivo
            builder.HasOne(c => c.Cultivo)
                .WithMany(c => c.Cosechas)
                .HasForeignKey(c => c.CultivoId)
                .OnDelete(DeleteBehavior.Cascade);

            // Índices para búsquedas
            builder.HasIndex(c => c.CultivoId);
            builder.HasIndex(c => c.Fecha);
        }
    }
}