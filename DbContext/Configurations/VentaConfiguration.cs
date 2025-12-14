using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaGestionAgricola.Models.Entities;

namespace SistemaGestionAgricola.Models.Configurations
{
    public class VentaConfiguration : IEntityTypeConfiguration<Venta>
    {
        public void Configure(EntityTypeBuilder<Venta> builder)
        {
            builder.HasKey(v => v.Id);
            
            builder.Property(v => v.Fecha)
                .IsRequired()
                .HasColumnType("date");
            
            builder.Property(v => v.Cantidad)
                .IsRequired()
                .HasColumnType("decimal(10,2)");
            
            builder.Property(v => v.PrecioKg)
                .IsRequired()
                .HasColumnType("decimal(10,2)");
            
            builder.Property(v => v.CostoFlete)
                .IsRequired()
                .HasColumnType("decimal(10,2)");
            
            builder.Property(v => v.Observaciones)
                .HasColumnType("text");
            
            builder.Property(v => v.CreatedAt)
                .IsRequired()
                .HasColumnType("datetime");

            // Relación muchos-a-uno con Cosecha
            builder.HasOne(v => v.Cosecha)
                .WithMany(c => c.Ventas)
                .HasForeignKey(v => v.CosechaId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relación muchos-a-uno con Comprador
            builder.HasOne(v => v.Comprador)
                .WithMany(c => c.Ventas)
                .HasForeignKey(v => v.CompradorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Índices para búsquedas
            builder.HasIndex(v => v.CosechaId);
            builder.HasIndex(v => v.CompradorId);
            builder.HasIndex(v => v.Fecha);
        }
    }
}