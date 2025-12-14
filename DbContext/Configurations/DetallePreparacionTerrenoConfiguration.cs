using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaGestionAgricola.Models.Entities;

namespace SistemaGestionAgricola.Models.Configurations
{
    public class DetallePreparacionTerrenoConfiguration : IEntityTypeConfiguration<DetallePreparacionTerreno>
    {
        public void Configure(EntityTypeBuilder<DetallePreparacionTerreno> builder)
        {
            builder.HasKey(d => d.Id);
            
            builder.Property(d => d.TipoPreparacion)
                .IsRequired()
                .HasMaxLength(100);
            
            builder.Property(d => d.HorasMaquinaria)
                .IsRequired()
                .HasColumnType("decimal(5,2)");
            
            builder.Property(d => d.Costo)
                .IsRequired()
                .HasColumnType("decimal(10,2)");
            
            builder.Property(d => d.Observaciones)
                .HasColumnType("text");
            
            builder.Property(d => d.CreatedAt)
                .IsRequired()
                .HasColumnType("datetime");

            // Relación muchos-a-uno con ProcesoAgricola
            builder.HasOne(d => d.ProcesoAgricola)
                .WithMany(pa => pa.DetallesPreparacionTerreno)
                .HasForeignKey(d => d.ProcesoId)
                .OnDelete(DeleteBehavior.Cascade);

            // Índice para búsquedas por proceso
            builder.HasIndex(d => d.ProcesoId);
        }
    }
}