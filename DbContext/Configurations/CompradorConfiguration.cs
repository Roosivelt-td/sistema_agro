using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaGestionAgricola.Models.Entities;

namespace SistemaGestionAgricola.Models.Configurations
{
    public class CompradorConfiguration : IEntityTypeConfiguration<Comprador>
    {
        public void Configure(EntityTypeBuilder<Comprador> builder)
        {
            builder.HasKey(c => c.Id);
            
            builder.Property(c => c.Nombre)
                .IsRequired()
                .HasMaxLength(255);
            
            builder.Property(c => c.Ruc)
                .HasMaxLength(20);
            
            builder.Property(c => c.Telefono)
                .HasMaxLength(20);
            
            builder.Property(c => c.Direccion)
                .HasColumnType("text");
            
            builder.Property(c => c.Contacto)
                .HasMaxLength(100);
            
            builder.Property(c => c.Email)
                .HasMaxLength(255);
            
            builder.Property(c => c.TipoComprador)
                .HasMaxLength(50);
            
            builder.Property(c => c.CreatedAt)
                .IsRequired()
                .HasColumnType("datetime");
            
            builder.Property(c => c.UpdatedAt)
                .IsRequired()
                .HasColumnType("datetime");

            // Índice único para RUC (si se proporciona)
            builder.HasIndex(c => c.Ruc)
                .IsUnique()
                .HasFilter("[Ruc] IS NOT NULL");

            // Índice para búsquedas por nombre
            builder.HasIndex(c => c.Nombre);

            // Índice para búsquedas por tipo de comprador
            builder.HasIndex(c => c.TipoComprador);
        }
    }
}