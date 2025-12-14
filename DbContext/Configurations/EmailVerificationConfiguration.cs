using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SistemaGestionAgricola.Models.Entities;

namespace SistemaGestionAgricola.Models.Configurations
{
    public class EmailVerificationConfiguration : IEntityTypeConfiguration<EmailVerification>
    {
        public void Configure(EntityTypeBuilder<EmailVerification> builder)
        {
            builder.ToTable("EmailVerifications");
            
            builder.HasKey(e => e.Id);
            
            builder.Property(e => e.Email)
                .IsRequired()
                .HasMaxLength(255);
            
            builder.Property(e => e.Code)
                .IsRequired()
                .HasMaxLength(6);
            
            builder.Property(e => e.Attempts)
                .HasDefaultValue(0);
            
            builder.Property(e => e.CreatedAt)
                .IsRequired();
            
            builder.Property(e => e.ExpiresAt)
                .IsRequired();
            
            builder.Property(e => e.IsUsed)
                .HasDefaultValue(false);
            
            builder.Property(e => e.VerificationType)
                .HasMaxLength(50);
            
            // Índice para búsquedas por email
            builder.HasIndex(e => e.Email);
            
            // Índice para limpieza de códigos expirados
            builder.HasIndex(e => e.ExpiresAt);
        }
    }
}