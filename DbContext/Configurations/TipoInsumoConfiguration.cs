using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaGestionAgricola.Models.Entities;

namespace SistemaGestionAgricola.Models.Configurations
{
    public class TipoInsumoConfiguration : IEntityTypeConfiguration<TipoInsumo>
    {
        public void Configure(EntityTypeBuilder<TipoInsumo> builder)
        {
            builder.HasKey(ti => ti.Id);
            
            builder.Property(ti => ti.Nombre)
                .IsRequired()
                .HasMaxLength(100);
            
            builder.Property(ti => ti.Categoria)
                .IsRequired()
                .HasMaxLength(50);
            
            builder.Property(ti => ti.Descripcion)
                .HasColumnType("text");
            
            builder.Property(ti => ti.CreatedAt)
                .IsRequired()
                .HasColumnType("datetime");

            // Índice único para nombre
            builder.HasIndex(ti => ti.Nombre)
                .IsUnique();

            // Índice para búsquedas por categoría
            builder.HasIndex(ti => ti.Categoria);
        }
    }
}