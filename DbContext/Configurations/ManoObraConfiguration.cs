using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaGestionAgricola.Models.Entities;

namespace SistemaGestionAgricola.Models.Configurations
{
    public class ManoObraConfiguration : IEntityTypeConfiguration<ManoObra>
    {
        public void Configure(EntityTypeBuilder<ManoObra> builder)
        {
            builder.HasKey(m => m.Id);
            
            builder.Property(m => m.NumeroPeones)
                .IsRequired();
            
            builder.Property(m => m.DiasTrabajo)
                .IsRequired();
            
            builder.Property(m => m.CostoPorDia)
                .IsRequired()
                .HasColumnType("decimal(10,2)");
            
            builder.Property(m => m.CostoTotal)
                .IsRequired()
                .HasColumnType("decimal(10,2)");
            
            builder.Property(m => m.Observaciones)
                .HasColumnType("text");
            
            builder.Property(m => m.CreatedAt)
                .IsRequired()
                .HasColumnType("datetime");

            // Relación muchos-a-uno con ProcesoAgricola
            builder.HasOne(m => m.ProcesoAgricola)
                .WithMany(pa => pa.ManosObra)
                .HasForeignKey(m => m.ProcesoId)
                .OnDelete(DeleteBehavior.Cascade);

            // Índice para búsquedas por proceso
            builder.HasIndex(m => m.ProcesoId);
        }
    }
}