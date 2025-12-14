using Microsoft.EntityFrameworkCore;
using SistemaGestionAgricola.Models.Configurations;
using SistemaGestionAgricola.Models.Entities;

namespace SistemaGestionAgricola.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Agricultor> Agricultores { get; set; }
        public DbSet<Terreno> Terrenos { get; set; }
        public DbSet<TipoCultivo> TipoCultivos { get; set; }
        public DbSet<Cultivo> Cultivos { get; set; }
        public DbSet<TipoProceso> TipoProcesos { get; set; }
        public DbSet<ProcesoAgricola> ProcesosAgricolas { get; set; }
        public DbSet<DetallePreparacionTerreno> DetallesPreparacionTerreno { get; set; }
        public DbSet<TipoInsumo> TipoInsumos { get; set; }
        public DbSet<InsumoUtilizado> InsumosUtilizados { get; set; } 
        public DbSet<ManoObra> ManosObra { get; set; } 
        public DbSet<Proveedor> Proveedores { get; set; }
        public DbSet<Cosecha> Cosechas { get; set; }
        public DbSet<Comprador> Compradores { get; set; } 
        public DbSet<Venta> Ventas { get; set; }
        public DbSet<NotificacionAutomatica> NotificacionesAutomaticas { get; set; }
        public DbSet<Notificacion> Notificaciones { get; set; }
        public DbSet<EmailVerification> EmailVerifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Aplicar todas las configuraciones
            modelBuilder.ApplyConfiguration(new UsuarioConfiguration());
            modelBuilder.ApplyConfiguration(new AgricultorConfiguration());
            modelBuilder.ApplyConfiguration(new TerrenoConfiguration());
            modelBuilder.ApplyConfiguration(new TipoCultivoConfiguration());
            modelBuilder.ApplyConfiguration(new CultivoConfiguration());
            modelBuilder.ApplyConfiguration(new TipoProcesoConfiguration());
            modelBuilder.ApplyConfiguration(new ProcesoAgricolaConfiguration()); 
            modelBuilder.ApplyConfiguration(new DetallePreparacionTerrenoConfiguration());
            modelBuilder.ApplyConfiguration(new TipoInsumoConfiguration());
            modelBuilder.ApplyConfiguration(new InsumoUtilizadoConfiguration());
            modelBuilder.ApplyConfiguration(new ManoObraConfiguration());
            modelBuilder.ApplyConfiguration(new ProveedorConfiguration()); 
            modelBuilder.ApplyConfiguration(new CosechaConfiguration());
            modelBuilder.ApplyConfiguration(new CompradorConfiguration());
            modelBuilder.ApplyConfiguration(new VentaConfiguration());
            modelBuilder.ApplyConfiguration(new NotificacionAutomaticaConfiguration());
            modelBuilder.ApplyConfiguration(new NotificacionConfiguration());
            modelBuilder.ApplyConfiguration(new EmailVerificationConfiguration());
        }
    }
}