using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaGestionAgricola.Models.Entities;

namespace SistemaGestionAgricola.Models.Configurations
{
    public class TipoProcesoConfiguration : IEntityTypeConfiguration<TipoProceso>
    {
        public void Configure(EntityTypeBuilder<TipoProceso> builder)
        {
            builder.HasKey(tp => tp.Id);
            
            builder.Property(tp => tp.Nombre)
                .IsRequired()
                .HasMaxLength(100);
            
            builder.Property(tp => tp.Descripcion)
                .HasColumnType("text");
            
            builder.Property(tp => tp.CreatedAt)
                .IsRequired()
                .HasColumnType("datetime");

            // Índice único para nombre
            builder.HasIndex(tp => tp.Nombre)
                .IsUnique();
        }
    }
}