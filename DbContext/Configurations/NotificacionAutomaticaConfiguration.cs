using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaGestionAgricola.Models.Entities;

namespace SistemaGestionAgricola.Models.Configurations
{
    public class NotificacionAutomaticaConfiguration : IEntityTypeConfiguration<NotificacionAutomatica>
    {
        public void Configure(EntityTypeBuilder<NotificacionAutomatica> builder)
        {
            builder.HasKey(na => na.Id);

            builder.Property(na => na.TipoEvento)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(na => na.DiasDespuesSiembra)
                .IsRequired();

            builder.Property(na => na.Mensaje)
                .IsRequired()
                .HasColumnType("text");

            builder.Property(na => na.CreatedAt)
                .IsRequired()
                .HasColumnType("datetime");

            // Relación muchos-a-uno con TipoCultivo
            builder.HasOne(na => na.TipoCultivo)
                .WithMany(tc => tc.NotificacionesAutomaticas)
                .HasForeignKey(na => na.TipoCultivoId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relación muchos-a-uno con Usuario
            builder.HasOne(na => na.Usuario)
                .WithMany(u => u.NotificacionesAutomaticas)
                .HasForeignKey(na => na.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            // Índices para búsquedas
            builder.HasIndex(na => na.TipoCultivoId);
            builder.HasIndex(na => na.UsuarioId);
            builder.HasIndex(na => na.TipoEvento);
        }
    }
}