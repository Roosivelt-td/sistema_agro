using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaGestionAgricola.Models.Entities;

namespace SistemaGestionAgricola.Models.Configurations
{
    public class ProveedorConfiguration : IEntityTypeConfiguration<Proveedor>
    {
        public void Configure(EntityTypeBuilder<Proveedor> builder)
        {
            builder.HasKey(p => p.Id);
            
            builder.Property(p => p.Nombre)
                .IsRequired()
                .HasMaxLength(255);
            
            builder.Property(p => p.Ruc)
                .HasMaxLength(20);
            
            builder.Property(p => p.Telefono)
                .HasMaxLength(20);
            
            builder.Property(p => p.Direccion)
                .HasColumnType("text");
            
            builder.Property(p => p.TipoServicio)
                .IsRequired()
                .HasMaxLength(50);
            
            builder.Property(p => p.Contacto)
                .HasMaxLength(100);
            
            builder.Property(p => p.Email)
                .HasMaxLength(255);
            
            builder.Property(p => p.CreatedAt)
                .IsRequired()
                .HasColumnType("datetime");
            
            builder.Property(p => p.UpdatedAt)
                .IsRequired()
                .HasColumnType("datetime");

            // Índice único para RUC (si se proporciona)
            builder.HasIndex(p => p.Ruc)
                .IsUnique()
                .HasFilter("[Ruc] IS NOT NULL");

            // Índice para búsquedas por nombre
            builder.HasIndex(p => p.Nombre);

            // Índice para búsquedas por tipo de servicio
            builder.HasIndex(p => p.TipoServicio);
        }
    }
}