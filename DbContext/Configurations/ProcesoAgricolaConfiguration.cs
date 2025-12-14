using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaGestionAgricola.Models.Entities;

namespace SistemaGestionAgricola.Models.Configurations
{
    public class ProcesoAgricolaConfiguration : IEntityTypeConfiguration<ProcesoAgricola>
    {
        public void Configure(EntityTypeBuilder<ProcesoAgricola> builder)
        {
            builder.HasKey(pa => pa.Id);
            
            builder.Property(pa => pa.Fecha)
                .IsRequired()
                .HasColumnType("date");
            
            builder.Property(pa => pa.CostoManoObra)
                .IsRequired()
                .HasColumnType("decimal(10,2)");
            
            builder.Property(pa => pa.Observaciones)
                .HasColumnType("text");
            
            builder.Property(pa => pa.CreatedAt)
                .IsRequired()
                .HasColumnType("datetime");

            // Relación muchos-a-uno con Cultivo
            builder.HasOne(pa => pa.Cultivo)
                .WithMany(c => c.ProcesosAgricolas)
                .HasForeignKey(pa => pa.CultivoId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relación muchos-a-uno con TipoProceso
            builder.HasOne(pa => pa.TipoProceso)
                .WithMany(tp => tp.ProcesosAgricolas)
                .HasForeignKey(pa => pa.TipoProcesoId)
                .OnDelete(DeleteBehavior.Restrict);

            // Índices para búsquedas
            builder.HasIndex(pa => pa.CultivoId);
            builder.HasIndex(pa => pa.TipoProcesoId);
            builder.HasIndex(pa => pa.Fecha);
        }
    }
}