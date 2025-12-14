using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaGestionAgricola.Models.Entities;

namespace SistemaGestionAgricola.Models.Configurations
{
    public class InsumoUtilizadoConfiguration : IEntityTypeConfiguration<InsumoUtilizado>
    {
        public void Configure(EntityTypeBuilder<InsumoUtilizado> builder)
        {
            builder.HasKey(i => i.Id);
            
            builder.Property(i => i.Cantidad)
                .IsRequired()
                .HasColumnType("decimal(10,2)");
            
            builder.Property(i => i.CostoUnitario)
                .IsRequired()
                .HasColumnType("decimal(10,2)");
            
            builder.Property(i => i.CostoFlete)
                .IsRequired()
                .HasColumnType("decimal(10,2)");
            
            builder.Property(i => i.Observaciones)
                .HasColumnType("text");
            
            builder.Property(i => i.CreatedAt)
                .IsRequired()
                .HasColumnType("datetime");

            // Relación muchos-a-uno con ProcesoAgricola
            builder.HasOne(i => i.ProcesoAgricola)
                .WithMany(pa => pa.InsumosUtilizados)
                .HasForeignKey(i => i.ProcesoId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relación muchos-a-uno con TipoInsumo
            builder.HasOne(i => i.TipoInsumo)
                .WithMany(ti => ti.InsumosUtilizados)
                .HasForeignKey(i => i.TipoInsumoId)
                .OnDelete(DeleteBehavior.Restrict);

            // Índices para búsquedas
            builder.HasIndex(i => i.ProcesoId);
            builder.HasIndex(i => i.TipoInsumoId);
            
            // Relación opcional con Proveedor
            builder.HasOne(i => i.Proveedor)
                .WithMany(p => p.InsumosUtilizados)
                .HasForeignKey(i => i.ProveedorId)
                .OnDelete(DeleteBehavior.SetNull); // Si se elimina el proveedor, setear a NULL
        }
    }
}