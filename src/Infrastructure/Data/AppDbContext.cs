using backend.src.Domain.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Serilog;

namespace backend.src.Infrastructure.Data
{
    /// <summary>
    /// Contexto de base de datos principal de la aplicación
    /// Hereda de IdentityDbContext para incluir las funcionalidades de autenticación
    /// </summary>
    public class AppDbContext : IdentityDbContext<User, Role, int>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options) { }

        // DbSets - Representan las tablas en la base de datos
        public DbSet<Image> Images { get; set; }
        public DbSet<UserImage> UserImages { get; set; }
        public DbSet<Curriculum> CVs { get; set; }
        public DbSet<AdminLog> AdminLogs { get; set; }
        public DbSet<Whitelist> Whitelists { get; set; }
        public DbSet<VerificationCode> VerificationCodes { get; set; }
        public DbSet<Publication> Publications { get; set; }
        public DbSet<Offer> Offers { get; set; }
        public DbSet<BuySell> BuySells { get; set; }
        public DbSet<NotificationDTO> Notifications { get; set; }
        public DbSet<JobApplication> JobApplications { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<AdminNotification> AdminNotifications { get; set; }

        /// <summary>
        /// Configura las relaciones entre entidades y otras configuraciones de EF Core
        /// </summary>
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Relaciones de JobApplication (Postulación a oferta)
            // Un estudiante puede hacer muchas postulaciones
            builder
                .Entity<JobApplication>()
                .HasOne(ja => ja.Student)
                .WithMany()
                .HasForeignKey(ja => ja.StudentId);

            // Una oferta puede tener muchas postulaciones
            builder
                .Entity<JobApplication>()
                .HasOne(ja => ja.JobOffer)
                .WithMany(o => o.Applications)
                .HasForeignKey(ja => ja.JobOfferId);

            // Relaciones de Publication (clase base para ofertas y compra/venta)
            // Un usuario puede crear muchas publicaciones
            builder
                .Entity<Publication>()
                .HasOne(p => p.User)
                .WithMany(u => u.Publications)
                .HasForeignKey(p => p.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Relaciones de UserImage (Imágenes de usuario)
            // Relación uno a uno entre GeneralUser y su imágen de perfil
            builder
                .Entity<User>()
                .HasOne(gu => gu.ProfilePhoto)
                .WithOne()
                .HasForeignKey<User>(gu => gu.ProfilePhotoId)
                .OnDelete(DeleteBehavior.SetNull);
            // Relaciones de Documentos
            builder
                .Entity<User>()
                .HasOne(gu => gu.CV)
                .WithOne()
                .HasForeignKey<User>(gu => gu.CVId)
                .OnDelete(DeleteBehavior.SetNull);

            // --- Fix: almacenar ApplicationStatus como string en la base de datos ---
            builder.Entity<JobApplication>().Property(j => j.Status).HasConversion<string>();
        }

        #region Override de SaveChangesAsync
        /// <summary>
        /// Override de SaveChangesAsync para:
        /// 1. Actualizar automáticamente UpdatedAt en entidades modificadas
        /// 2. Detectar publicaciones cerradas y crear reviews iniciales
        /// </summary>
        public override async Task<int> SaveChangesAsync(
            CancellationToken cancellationToken = default
        )
        {
            Log.Information("Iniciando SaveChangesAsync con lógica extendida");
            // Actualizar UpdatedAt automáticamente
            UpdateTimestamps();

            // Guardar cambios
            var result = await base.SaveChangesAsync(cancellationToken);
            return result;
        }

        #endregion
        #region Metodos auxiliares SaveChangesAsync
        /// <summary>
        /// Actualiza automáticamente el campo UpdatedAt de todas las entidades
        /// modificadas o agregadas que tengan esta propiedad.
        /// </summary>
        private void UpdateTimestamps()
        {
            var now = DateTime.UtcNow;
            // Obtener todas las entidades modificadas o agregadas que tengan la propiedad UpdatedAt
            var modifiedEntries = ChangeTracker
                .Entries()
                .Where(e => e.State == EntityState.Modified || e.State == EntityState.Added);
            foreach (var entry in modifiedEntries)
            {
                var updatedAtProperty = entry.Entity.GetType().GetProperty("UpdatedAt");
                if (updatedAtProperty != null && updatedAtProperty.CanWrite)
                {
                    updatedAtProperty.SetValue(entry.Entity, now);
                }
            }
        }
        #endregion
    }
}
