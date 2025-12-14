using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaGestionAgricola.Models.Entities;

namespace SistemaGestionAgricola.Models.Configurations
{
    public class NotificacionConfiguration : IEntityTypeConfiguration<Notificacion>
    {
        public void Configure(EntityTypeBuilder<Notificacion> builder)
        {
            builder.HasKey(n => n.Id);
            
            builder.Property(n => n.Tipo)
                .IsRequired()
                .HasMaxLength(50)
                .HasConversion(
                    v => v.ToLower(),
                    v => v
                );
            
            builder.Property(n => n.Mensaje)
                .IsRequired()
                .HasMaxLength(500)
                .HasColumnType("varchar(500)");
            
            builder.Property(n => n.FechaProgramada)
                .IsRequired()
                .HasColumnType("date");
            
            builder.Property(n => n.Estado)
                .IsRequired()
                .HasMaxLength(20)
                .HasDefaultValue("pendiente")
                .HasConversion(
                    v => v.ToLower(),
                    v => v
                );
            
            // CORREGIDO: Usar datetime en lugar de datetime2 para MySQL
            builder.Property(n => n.FechaEnvio)
                .HasColumnType("datetime");
            
            // CORREGIDO: Usar datetime y función NOW() de MySQL
            builder.Property(n => n.CreatedAt)
                .IsRequired()
                .HasColumnType("datetime")
                .HasDefaultValueSql("NOW()");

            // Relación muchos-a-uno con Usuario
            builder.HasOne(n => n.Usuario)
                .WithMany(u => u.Notificaciones)
                .HasForeignKey(n => n.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relación muchos-a-uno con Cultivo
            builder.HasOne(n => n.Cultivo)
                .WithMany(c => c.Notificaciones)
                .HasForeignKey(n => n.CultivoId)
                .OnDelete(DeleteBehavior.Cascade);

            // Índices para búsquedas
            builder.HasIndex(n => n.UsuarioId);
            builder.HasIndex(n => n.CultivoId);
            builder.HasIndex(n => n.Tipo);
            builder.HasIndex(n => n.Estado);
            builder.HasIndex(n => n.FechaProgramada);
            
            // Índice compuesto para consultas frecuentes
            builder.HasIndex(n => new { n.Estado, n.FechaProgramada });
            
            // Índice para notificaciones pendientes por usuario
            builder.HasIndex(n => new { n.UsuarioId, n.Estado, n.FechaProgramada });
        }
    }
}