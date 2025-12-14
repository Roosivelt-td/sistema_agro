using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaGestionAgricola.Models.Entities;

namespace SistemaGestionAgricola.Models.Configurations
{
    public class TipoCultivoConfiguration : IEntityTypeConfiguration<TipoCultivo>
    {
        public void Configure(EntityTypeBuilder<TipoCultivo> builder)
        {
            builder.HasKey(tc => tc.Id);
            
            builder.Property(tc => tc.Nombre)
                .IsRequired()
                .HasMaxLength(100);
            
            builder.Property(tc => tc.TiempoSiembraCosecha)
                .IsRequired();
            
            builder.Property(tc => tc.InstruccionesRiegos)
                .HasColumnType("text");
            
            builder.Property(tc => tc.InstruccionesFumigaciones)
                .HasColumnType("text");
            
            builder.Property(tc => tc.CreatedAt)
                .IsRequired()
                .HasColumnType("datetime");

            // Índice único para nombre
            builder.HasIndex(tc => tc.Nombre)
                .IsUnique();
        }
    }
}