using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaGestionAgricola.Models.Entities;

namespace SistemaGestionAgricola.Models.Configurations
{
    public class TerrenoConfiguration : IEntityTypeConfiguration<Terreno>
    {
        public void Configure(EntityTypeBuilder<Terreno> builder)
        {
            builder.HasKey(t => t.Id);
            
            builder.Property(t => t.Nombre)
                .IsRequired()
                .HasMaxLength(255);
            
            builder.Property(t => t.Ubicacion)
                .HasColumnType("text");
            
            builder.Property(t => t.AreaHectareas)
                .IsRequired()
                .HasColumnType("decimal(10,2)");
            
            builder.Property(t => t.TipoTenencia)
                .IsRequired()
                .HasMaxLength(20);
            
            builder.Property(t => t.CostoAlquiler)
                .IsRequired()
                .HasColumnType("decimal(10,2)");
            
            builder.Property(t => t.CreatedAt)
                .IsRequired()
                .HasColumnType("datetime");
            
            builder.Property(t => t.UpdatedAt)
                .IsRequired()
                .HasColumnType("datetime");

            // Relación muchos-a-uno con Agricultor
            builder.HasOne(t => t.Agricultor)
                .WithMany(a => a.Terrenos)
                .HasForeignKey(t => t.AgricultorId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(t => t.AgricultorId);
        }
    }
}